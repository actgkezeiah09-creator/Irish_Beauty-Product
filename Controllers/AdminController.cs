using AspNetCoreGeneratedDocument;
using Irish_Beauty_Product.Filters;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Irish_Beauty_Product.Controllers
{
    [AuthorizeRole("Admin", "Cashier", "Staff")]
    public class AdminController : Controller
    {
        private readonly string _connectionString;

        public AdminController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Home");
            }
            return View();
        }

        public JsonResult GetMonthlySales()
        {
            var results = new List<object>();

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                var query = @"
            WITH Months AS (
                SELECT 1 AS MonthNum UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4
                UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8
                UNION ALL SELECT 9 UNION ALL SELECT 10 UNION ALL SELECT 11 UNION ALL SELECT 12
            )
            SELECT 
                DATENAME(MONTH, DATEFROMPARTS(YEAR(GETDATE()), m.MonthNum, 1)) AS [Month],
                ISNULL(SUM((sd.UnitPrice - p.Cost) * sd.Quantity), 0) AS Total
            FROM Months m
            LEFT JOIN Sales s 
                ON MONTH(s.SaleDate) = m.MonthNum 
               AND YEAR(s.SaleDate) = YEAR(GETDATE())
            LEFT JOIN SaleDetails sd ON s.Id = sd.SalesId
            LEFT JOIN Products p ON sd.ProductId = p.Id
            GROUP BY m.MonthNum
            ORDER BY m.MonthNum";

                using (var cmd = new SqlCommand(query, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new
                        {
                            month = reader["Month"].ToString(),
                            total = Convert.ToDecimal(reader["Total"])
                        });
                    }
                }
            }

            return Json(results);
        }


        [HttpGet]
        public JsonResult GetDashboardStats()
        {
            decimal monthlyEarnings = 0;
            decimal annualEarnings = 0;
            int totalSales = 0;

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

               
                var cmdMonthly = new SqlCommand(@"
            SELECT 
                ISNULL(SUM((sd.UnitPrice - p.Cost) * sd.Quantity), 0)
            FROM SaleDetails sd
            INNER JOIN Sales s ON sd.SalesId = s.Id
            INNER JOIN Products p ON sd.ProductId = p.Id
            WHERE MONTH(s.SaleDate) = MONTH(GETDATE())
              AND YEAR(s.SaleDate) = YEAR(GETDATE())", con);
                monthlyEarnings = Convert.ToDecimal(cmdMonthly.ExecuteScalar() ?? 0);

               
                var cmdAnnual = new SqlCommand(@"
            SELECT 
                ISNULL(SUM((sd.UnitPrice - p.Cost) * sd.Quantity), 0)
            FROM SaleDetails sd
            INNER JOIN Sales s ON sd.SalesId = s.Id
            INNER JOIN Products p ON sd.ProductId = p.Id
            WHERE YEAR(s.SaleDate) = YEAR(GETDATE())", con);
                annualEarnings = Convert.ToDecimal(cmdAnnual.ExecuteScalar() ?? 0);

                var cmdCount = new SqlCommand("SELECT COUNT(*) FROM Sales", con);
                totalSales = Convert.ToInt32(cmdCount.ExecuteScalar() ?? 0);
            }

            return Json(new
            {
                monthly = monthlyEarnings,
                annual = annualEarnings,
                totalSales = totalSales
            });
        }



        [HttpGet]
        public JsonResult GetDailySales()
        {
            decimal dailyTotal = 0;

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                var cmd = new SqlCommand(@"
                SELECT ISNULL(SUM(TotalAmount), 0)
                FROM Sales
                WHERE CAST(SaleDate AS DATE) = CAST(GETDATE() AS DATE)", con);
                dailyTotal = Convert.ToDecimal(cmd.ExecuteScalar() ?? 0);
            }

            return Json(new { Daily = dailyTotal });
        }

        [HttpGet]
        public JsonResult GetWeeklySales()
        {
            decimal weeklyTotal = 0;

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                var cmd = new SqlCommand(@"
                SELECT ISNULL(SUM(TotalAmount), 0)
                FROM Sales
                WHERE SaleDate >= DATEADD(DAY, 1 - DATEPART(WEEKDAY, GETDATE()), CAST(GETDATE() AS DATE))
                  AND SaleDate < DATEADD(DAY, 8 - DATEPART(WEEKDAY, GETDATE()), CAST(GETDATE() AS DATE))", con);
                weeklyTotal = Convert.ToDecimal(cmd.ExecuteScalar() ?? 0);
            }

            return Json(new { Weekly = weeklyTotal });
        }

        [HttpGet]
        public JsonResult GetRevenueSources()
        {
            var results = new List<object>();

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                var query = @"
                    SELECT p.Category, SUM(sd.Quantity * sd.UnitPrice) AS Total
                    FROM SaleDetails sd
                    INNER JOIN Products p ON sd.ProductId = p.Id
                    INNER JOIN Sales s ON sd.SalesId = s.Id
                    WHERE YEAR(s.SaleDate) = YEAR(GETDATE())
                    GROUP BY p.Category";

                using (var cmd = new SqlCommand(query, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new
                        {
                            Category = reader["Category"].ToString(),
                            Total = Convert.ToDecimal(reader["Total"])
                        });
                    }
                }
            }

            return Json(results);
        }

        [HttpGet]
        public JsonResult GetAnnualEarnings()
        {
            var results = new List<object>();

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                var query = @"
                    SELECT YEAR(SaleDate) AS [Year], 
                           SUM(TotalAmount) AS Total
                    FROM Sales
                    GROUP BY YEAR(SaleDate)
                    ORDER BY YEAR(SaleDate)";

                using (var cmd = new SqlCommand(query, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new
                        {
                            Year = Convert.ToInt32(reader["Year"]),
                            Total = Convert.ToDecimal(reader["Total"])
                        });
                    }
                }
            }

            return Json(results);
        }

        public IActionResult InventoryReport()
        {
            var items = new List<object>();

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                var query = @"
            SELECT 
                p.Id,
                p.Name,
                p.Category,
                p.Cost,
                p.Price,
                p.QuantityOnHand,
                p.ExpiryDate,
                ISNULL(SUM(r.QuantityReturned), 0) AS TotalReturned,
                ISNULL(SUM(sd.Quantity), 0) AS TotalSold
            FROM Products p
            LEFT JOIN Returns r ON r.ProductId = p.Id
            LEFT JOIN SaleDetails sd ON sd.ProductId = p.Id
            GROUP BY 
                p.Id, p.Name, p.Category, p.Cost, p.Price, 
                p.QuantityOnHand, p.ExpiryDate";

                using (var cmd = new SqlCommand(query, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var cost = Convert.ToDecimal(reader["Cost"]);
                        var price = Convert.ToDecimal(reader["Price"]);
                        var quantityOnHand = Convert.ToInt32(reader["QuantityOnHand"]);
                        var totalReturned = Convert.ToInt32(reader["TotalReturned"]);
                        var totalSold = Convert.ToInt32(reader["TotalSold"]);

                        var adjustedQuantity = quantityOnHand + totalReturned;
                        var totalValue = cost * adjustedQuantity;

                        items.Add(new
                        {
                            Id = reader["Id"],
                            Name = reader["Name"].ToString(),
                            Category = reader["Category"].ToString(),
                            Cost = cost,
                            Price = price,
                            QuantityOnHand = adjustedQuantity,
                            ExpiryDate = reader["ExpiryDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ExpiryDate"]),
                            TotalReturned = totalReturned,
                            TotalSold = totalSold,
                            TotalValue = totalValue
                        });
                    }
                }
            }

            return View(items);
        }
    }
}
