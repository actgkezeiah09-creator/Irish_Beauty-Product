using Irish_Beauty_Product.Filters;
using Irish_Beauty_Product.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Irish_Beauty_Product.Controllers
{
    [AuthorizeRole("Admin", "Staff" , "Cashier")]
    public class SalesController : Controller
    {
        private readonly string _connectionString;

        public SalesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult SaleHistory()
        {
            var salesHistory = GetSalesHistory();
            return View(salesHistory);
        }

        private List<SalesHistory> GetSalesHistory()
        {
            var sales = new List<SalesHistory>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Use direct join instead of view to avoid ambiguity
                var command = new SqlCommand(@"
                    SELECT 
                        s.Id AS SalesId,          
                        s.SaleDate,
                        s.TotalAmount,
                        p.Name AS ProductName,
                        sd.Quantity,
                        sd.UnitPrice,
                        (sd.Quantity * sd.UnitPrice) AS LineTotal
                    FROM Sales s
                    INNER JOIN SaleDetails sd ON s.Id = sd.SalesId
                    INNER JOIN Products p ON sd.ProductId = p.Id
                    ORDER BY s.SaleDate DESC", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sales.Add(new SalesHistory
                        {
                            SaleId = reader.GetInt32(reader.GetOrdinal("SalesId")),
                            SaleDate = reader.GetDateTime(reader.GetOrdinal("SaleDate")),
                            TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                            ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                            UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                            LineTotal = reader.GetDecimal(reader.GetOrdinal("LineTotal"))
                        });
                    }
                }
            }
            return sales;
        }

        [HttpGet]
        public JsonResult GetSalesSummary()
        {
            try
            {
                var summary = GetSalesSummaryData();
                return Json(new { success = true, summary = summary });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private dynamic GetSalesSummaryData()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(@"
                    SELECT 
                        COUNT(DISTINCT s.Id) as TotalSales,
                        SUM(s.TotalAmount) as TotalRevenue,
                        AVG(s.TotalAmount) as AverageSale,
                        SUM(sd.Quantity) as TotalItemsSold
                    FROM Sales s
                    INNER JOIN SaleDetails sd ON s.Id = sd.SalesId", connection);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new
                        {
                            TotalSales = reader.IsDBNull(reader.GetOrdinal("TotalSales")) ? 0 : reader.GetInt32(reader.GetOrdinal("TotalSales")),
                            TotalRevenue = reader.IsDBNull(reader.GetOrdinal("TotalRevenue")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalRevenue")),
                            AverageSale = reader.IsDBNull(reader.GetOrdinal("AverageSale")) ? 0 : reader.GetDecimal(reader.GetOrdinal("AverageSale")),
                            TotalItemsSold = reader.IsDBNull(reader.GetOrdinal("TotalItemsSold")) ? 0 : reader.GetInt32(reader.GetOrdinal("TotalItemsSold"))
                        };
                    }
                }
            }
            return new { TotalSales = 0, TotalRevenue = 0m, AverageSale = 0m, TotalItemsSold = 0 };
        }


      
        [HttpGet]
        public JsonResult GetSaleDetails(int saleId)
        {
            try
            {
                var saleItems = new List<object>();

                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(@"
            SELECT 
                p.Name AS ProductName, 
                sd.Quantity, 
                sd.UnitPrice, 
                (sd.Quantity * sd.UnitPrice) AS LineTotal
            FROM SaleDetails sd
            INNER JOIN Products p ON sd.ProductId = p.Id
            WHERE sd.SalesId = @SalesId", connection))
                {
                    command.Parameters.AddWithValue("@SalesId", saleId);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            saleItems.Add(new
                            {
                                ProductName = reader["ProductName"] != DBNull.Value ? reader["ProductName"].ToString() : "",
                                Quantity = reader["Quantity"] != DBNull.Value ? Convert.ToInt32(reader["Quantity"]) : 0,
                                UnitPrice = reader["UnitPrice"] != DBNull.Value ? Convert.ToDecimal(reader["UnitPrice"]) : 0m,
                                LineTotal = reader["LineTotal"] != DBNull.Value ? Convert.ToDecimal(reader["LineTotal"]) : 0m
                            });
                        }
                    }
                }

                return Json(new { success = true, saleDetails = saleItems });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


    }
}



   
