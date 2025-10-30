using Irish_Beauty_Product.Filters;
using Irish_Beauty_Product.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;


namespace Irish_Beauty_Product.Controllers
{
    [AuthorizeRole("Admin", "Staff")]
    public class InventoryController : Controller
    {
        private readonly string _connectionString;

        public InventoryController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Inventory()
        {
            var products = GetAllProducts();
            return View(products);
        }

        [HttpPost]
        public JsonResult AddProduct([FromBody] Product product)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    int newProductId = AddProductToDatabase(product);
                    product.Id = newProductId;
                    return Json(new
                    {
                        success = true,
                        message = "Product added successfully!",
                        product = new
                        {
                            Id = newProductId,
                            Name = product.Name,
                            Category = product.Category,
                            Price = product.Price,
                            QuantityOnHand = product.QuantityOnHand,
                            SKU = product.SKU,
                            ExpiryDate = product.ExpiryDate?.ToString("yyyy-MM-dd"),
                            BatchNumber = product.BatchNumber,
                            LowStockAlert = product.LowStockAlert
                        }
                    });
                }

                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = "Validation failed: " + string.Join(", ", errors) });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error adding product: {ex.Message}" });
            }
        }

        // Return to the inventory page

        [HttpGet]
        public JsonResult GetProductDetails(int id)
        {
            try
            {
                var product = GetProductById(id);
                if (product != null)
                {
                    return Json(new
                    {
                        success = true,
                        product = new
                        {
                            Id = product.Id,
                            Name = product.Name,
                            SKU = product.SKU,
                            ExpiryDate = product.ExpiryDate?.ToString("yyyy-MM-dd"),
                            BatchNumber = product.BatchNumber
                        }
                    });  
                }
                else
                {
                    return Json(new { success = false, message = "Product not found" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        private bool ProductExists(string sku)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT COUNT(1) FROM Products WHERE SKU = @SKU", connection);
                command.Parameters.AddWithValue("@SKU", sku);

                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }


        [HttpPost]
        public JsonResult UpdateExpiryDate(int productId, DateTime? expiryDate, string batchNumber)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        "UPDATE Products SET ExpiryDate = @ExpiryDate, BatchNumber = @BatchNumber WHERE Id = @Id",
                        connection);

                        if (expiryDate.HasValue)
                          command.Parameters.AddWithValue("@ExpiryDate",expiryDate.Value);
                        else
                        command.Parameters.AddWithValue("@ExpiryDate", DBNull.Value);
 
                          command.Parameters.AddWithValue("@BatchNumber", string.IsNullOrEmpty(batchNumber) ? DBNull.Value : batchNumber);
                          command.Parameters.AddWithValue("@Id", productId);

                         int rowsAffected = command.ExecuteNonQuery();

                         if (rowsAffected > 0)
                        return Json(new { success = true, message = "Expiry date updated successfully!" });
                        else
                        return Json(new { success = false, message = "Product not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public JsonResult DeleteProduct(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand("UPDATE Products SET IsActive = 0 WHERE Id = @Id", connection);
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }

                return Json(new { success = true, message = "Product archived (soft deleted) successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        private List<Product> GetAllProducts()
        {
            var products = new List<Product>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(@"
                    SELECT Id, Name, Category, Price, Cost, QuantityOnHand, SKU, ExpiryDate, BatchNumber, LowStockAlert, SupplierId
                    FROM Products", connection);

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
                            Cost = reader.GetDecimal("Cost"),
                            QuantityOnHand = reader.GetInt32("QuantityOnHand"),
                            SKU = reader.GetString("SKU"),
                            ExpiryDate = reader.IsDBNull("ExpiryDate") ? null : reader.GetDateTime("ExpiryDate"),
                            BatchNumber = reader.IsDBNull("BatchNumber") ? null : reader.GetString("BatchNumber"),
                            LowStockAlert = reader.GetInt32("LowStockAlert"),
                            SupplierId = reader.IsDBNull("SupplierId") ? null : reader.GetInt32("SupplierId")
                        });
                    }
                }
            }
            return products;
        }

        private int AddProductToDatabase(Product product)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
            INSERT INTO Products 
            (Name, Category, Price, Cost, QuantityOnHand, SKU, ExpiryDate, BatchNumber, LowStockAlert, SupplierId, CreatedDate, IsActive)
            OUTPUT INSERTED.Id
            VALUES 
            (@Name, @Category, @Price, @Cost, @Quantity, @SKU, @ExpiryDate, @BatchNumber, @LowStockAlert, @SupplierId, GETDATE(), 1)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Category", product.Category);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@Cost", product.Cost);
                    command.Parameters.AddWithValue("@Quantity", product.QuantityOnHand);
                    command.Parameters.AddWithValue("@SKU", product.SKU);
                    command.Parameters.AddWithValue("@LowStockAlert", product.LowStockAlert);

                    command.Parameters.AddWithValue("@SupplierId",
                        product.SupplierId.HasValue ? (object)product.SupplierId.Value : DBNull.Value);

                    command.Parameters.AddWithValue("@ExpiryDate",
                        product.ExpiryDate.HasValue ? (object)product.ExpiryDate.Value : DBNull.Value);

                    command.Parameters.AddWithValue("@BatchNumber",
                        string.IsNullOrEmpty(product.BatchNumber) ? DBNull.Value : product.BatchNumber);

                    
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        private Product GetProductById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(@"
                SELECT 
                Id, 
                Name, 
                Category, 
                Price, 
                Cost, 
                QuantityOnHand, 
                SKU, 
                ExpiryDate, 
                BatchNumber, 
                LowStockAlert,
                SupplierId
               FROM Products 
               WHERE Id = @Id", connection);

                command.Parameters.AddWithValue("@Id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Product
                        {
                            Id = reader.GetInt32("Id"),
                            Name = reader.GetString("Name"),
                            Category = reader.GetString("Category"),
                            Price = reader.GetDecimal("Price"),
                            Cost = reader.GetDecimal("Cost"),
                            QuantityOnHand = reader.GetInt32("QuantityOnHand"),
                            SKU = reader.GetString("SKU"),
                            ExpiryDate = reader.IsDBNull("ExpiryDate") ? null : reader.GetDateTime("ExpiryDate"),
                            BatchNumber = reader.IsDBNull("BatchNumber") ? null : reader.GetString("BatchNumber"),
                            LowStockAlert = reader.GetInt32("LowStockAlert"),
                            SupplierId = reader.IsDBNull("SupplierId") ? null : reader.GetInt32("SupplierId")
                        };
                    }
                }
            }
            return null;

        }

       
      
        [HttpPost]
        public IActionResult ProcessReturn([FromBody] ReturnModel model)
{
    if (model == null || model.ProductId <= 0 || model.QuantityReturned <= 0)
        return Json(new { success = false, message = "Invalid return data." });

    using (var con = new SqlConnection(_connectionString))
    {
        con.Open();

        //  Validate product by ID
        var cmdFind = new SqlCommand("SELECT COUNT(*) FROM Products WHERE Id = @Id", con);
        cmdFind.Parameters.AddWithValue("@Id", model.ProductId);
        int exists = (int)cmdFind.ExecuteScalar();

        if (exists == 0)
            return Json(new { success = false, message = "Product not found." });

        //  Insert into Returns table
        var cmdInsert = new SqlCommand(@"
            INSERT INTO Returns (ProductId, QuantityReturned, Reason, ReturnDate, ProcessedBy)
            VALUES (@ProductId, @QuantityReturned, @Reason, GETDATE(), @ProcessedBy)", con);
        cmdInsert.Parameters.AddWithValue("@ProductId", model.ProductId);
        cmdInsert.Parameters.AddWithValue("@QuantityReturned", model.QuantityReturned);
        cmdInsert.Parameters.AddWithValue("@Reason", model.Reason);
        cmdInsert.Parameters.AddWithValue("@ProcessedBy", HttpContext.Session.GetString("Username") ?? "Admin");
        cmdInsert.ExecuteNonQuery();

        //  Update inventory
        var cmdUpdate = new SqlCommand(@"
            UPDATE Products 
            SET QuantityOnHand = QuantityOnHand + @QuantityReturned 
            WHERE Id = @ProductId", con);
        cmdUpdate.Parameters.AddWithValue("@QuantityReturned", model.QuantityReturned);
        cmdUpdate.Parameters.AddWithValue("@ProductId", model.ProductId);
        cmdUpdate.ExecuteNonQuery();
    }

    return Json(new
    {
        success = true,
        redirectUrl = Url.Action("GetReturnHistory", "Inventory"),
        message = "Return processed successfully!"
    });
}




        [HttpPost]
        public JsonResult ReturnProduct(int id, int quantity = 1, string reason = "Customer Return")
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                var query = @"
                    UPDATE Products 
                    SET QuantityOnHand = QuantityOnHand + @Quantity
                    WHERE Id = @Id;

                    INSERT INTO Returns (ProductId, Quantity, DateReturned, Reason)
                    VALUES (@Id, @Quantity, GETDATE(), @Reason);";

                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@Reason", reason);
                    cmd.ExecuteNonQuery();
                }
            }

            return Json(new { success = true, message = "Product successfully returned and added back to inventory." });
        }

        //  return history for a specific product
       
        
        [HttpGet]
        public IActionResult GetReturnHistory()
        {
            var returns = new List<object>();

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                var query = @"
            SELECT 
                r.Id AS ReturnId,
                p.Name AS ProductName,
                p.Category,
                r.QuantityReturned,
                r.Reason,
                r.ReturnDate,
                r.ProcessedBy
            FROM Returns r
            INNER JOIN Products p ON r.ProductId = p.Id
            ORDER BY r.ReturnDate DESC";

                using (var cmd = new SqlCommand(query, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        returns.Add(new
                        {
                            ReturnId = reader["ReturnId"],
                            ProductName = reader["ProductName"].ToString(),
                            Category = reader["Category"].ToString(),
                            QuantityReturned = Convert.ToInt32(reader["QuantityReturned"]),
                            Reason = reader["Reason"].ToString(),
                            ReturnDate = Convert.ToDateTime(reader["ReturnDate"]),
                            ProcessedBy = reader["ProcessedBy"].ToString()
                        });
                    }
                }
            }

            return View(returns);
        }

 
    }
}


    
