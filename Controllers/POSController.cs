using Irish_Beauty_Product;
using Irish_Beauty_Product.Filters;
using Irish_Beauty_Product.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;


namespace Irish_Beauty_Product.Controllers
{
    [AuthorizeRole("Admin", "Cashier")]
    public class POSController : Controller
    {
        private readonly string _connectionString;

        public POSController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult POS()
        {
            var products = GetProductsFromDatabase();
            return View(products);
        }


   
        [HttpPost]
        public JsonResult ProcessSale([FromBody] SaleData saleData)
        {
            if (saleData == null || saleData.Cart == null || !saleData.Cart.Any())
            {
                return Json(new { success = false, message = "Cart is empty." });
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 1. Validate stock levels
                            foreach (var item in saleData.Cart)
                            {
                                if (!HasSufficientStock(item.Id, item.Quantity, connection, transaction))
                                {
                                    transaction.Rollback();
                                    return Json(new
                                    {
                                        success = false,
                                        message = $"Insufficient stock for {item.Name}"
                                    });
                                }
                            }

                            // 2. Insert sale record
                            var saleId = InsertSaleRecord(
                        saleData.TotalAmount,
                        saleData.PaymentMethod,
                        connection,
                        transaction,
                        saleData.DiscountRate
                        );

                            // 3. Insert sale details & update inventory
                            foreach (var item in saleData.Cart)
                            {
                                InsertSaleDetail(saleId, item, connection, transaction);
                                UpdateInventory(item.Id, item.Quantity, connection, transaction);
                            }

                            // Commit everything
                            transaction.Commit();

                            // 4. Return invoice data
                            return Json(new
                            {
                                success = true,
                                saleId = saleId,
                                date = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                                total = saleData.TotalAmount,
                                items = saleData.Cart
                            });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return Json(new { success = false, message = ex.Message });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private List<Product> GetProductsFromDatabase()
        {
            var products = new List<Product>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var command = new SqlCommand(@"
            SELECT Id, Name, Category, Price, QuantityOnHand, SKU, ExpiryDate, BatchNumber
            FROM Products
            WHERE QuantityOnHand > 0 
              AND (ExpiryDate IS NULL OR ExpiryDate >= CAST(GETDATE() AS date))", connection);
                // ^ Keeps only non-expired or no-expiry products

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name"),
                            Category = reader.GetString("Category"),
                            Price = reader.GetDecimal("Price"),
                            QuantityOnHand = reader.GetInt32("QuantityOnHand"),
                            SKU = reader.GetString("SKU"),
                            ExpiryDate = reader.IsDBNull("ExpiryDate") ? null : reader.GetDateTime("ExpiryDate"),
                            BatchNumber = reader.IsDBNull("BatchNumber") ? null : reader.GetString("BatchNumber")
                        });
                    }
                }
            }
            return products;
        }

        private bool HasSufficientStock(int productId, int quantity, SqlConnection connection, SqlTransaction transaction)
        {
            var command = new SqlCommand(
                "SELECT QuantityOnHand FROM Products WHERE Id = @ProductId",
                connection, transaction);
            command.Parameters.AddWithValue("@ProductId", productId);

            var currentStock = (int)command.ExecuteScalar();
            return currentStock >= quantity;
        }


        private void InsertSaleDetail(int saleId, CartItem item, SqlConnection connection, SqlTransaction transaction)
        {
            var command = new SqlCommand(
                "INSERT INTO SaleDetails (SalesId, ProductId, Quantity, UnitPrice) VALUES (@SalesId, @ProductId, @Quantity, @UnitPrice)",
                connection, transaction);
            command.Parameters.AddWithValue("@SalesId", saleId);
            command.Parameters.AddWithValue("@ProductId", item.Id);
            command.Parameters.AddWithValue("@Quantity", item.Quantity);
            command.Parameters.AddWithValue("@UnitPrice", item.Price);
            command.ExecuteNonQuery();
        }

        private void UpdateInventory(int productId, int quantity, SqlConnection connection, SqlTransaction transaction)
        {
            var command = new SqlCommand(
                "UPDATE Products SET QuantityOnHand = QuantityOnHand - @Quantity WHERE Id = @ProductId",
                connection, transaction);
            command.Parameters.AddWithValue("@Quantity", quantity);
            command.Parameters.AddWithValue("@ProductId", productId);
            command.ExecuteNonQuery();
        }

        public JsonResult GetProducts() {
            var products = GetProductsFromDatabase();
            return Json(products);
        }
        private int InsertSaleRecord(decimal totalAmount, string paymentMethod, SqlConnection connection, SqlTransaction transaction, decimal discountRate)
        {
           
            decimal discountedAmount = totalAmount - (totalAmount * discountRate);

            var query = @"
        INSERT INTO Sales (SaleDate, TotalAmount, PaymentMethod, DiscountRate)
        OUTPUT INSERTED.Id
        VALUES (@SaleDate, @TotalAmount, @PaymentMethod, @DiscountRate)";

            using (var cmd = new SqlCommand(query, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@SaleDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@TotalAmount", discountedAmount);
                cmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                cmd.Parameters.AddWithValue("@DiscountRate", discountRate);
                return (int)cmd.ExecuteScalar();
            }
        }


    }
}