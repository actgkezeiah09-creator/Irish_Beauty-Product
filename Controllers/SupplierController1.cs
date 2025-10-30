using Microsoft.AspNetCore.Mvc;
using Irish_Beauty_Product.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;

namespace Irish_Beauty_Product.Controllers
{
    public class SupplierController : Controller
    {
        private readonly string _connectionString;

        public SupplierController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // 🧾 Display supplier list
        public IActionResult Index()
        {
            var suppliers = new List<Supplier>();

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var cmd = new SqlCommand("SELECT * FROM Supplier", con);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    suppliers.Add(new Supplier
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        ContactNumber = reader["ContactNumber"].ToString(),
                        Address = reader["Address"].ToString()
                    });
                }
            }

            return View(suppliers);
        }

        // 📝 Add new supplier
        [HttpPost]
        public IActionResult AddSupplier(Supplier supplier)
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    var cmd = new SqlCommand(
                        "INSERT INTO Supplier (Name, Email, ContactNumber, Address) VALUES (@Name, @Email, @ContactNumber, @Address)",
                        con);

                    cmd.Parameters.AddWithValue("@Name", supplier.Name);
                    cmd.Parameters.AddWithValue("@Email", supplier.Email);
                    cmd.Parameters.AddWithValue("@ContactNumber", supplier.ContactNumber ?? "");
                    cmd.Parameters.AddWithValue("@Address", supplier.Address ?? "");

                    cmd.ExecuteNonQuery();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ✏️ Show Edit Form
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Supplier supplier = null;

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                var cmd = new SqlCommand("SELECT * FROM Supplier WHERE Id=@Id", con);
                cmd.Parameters.AddWithValue("@Id", id);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    supplier = new Supplier
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        ContactNumber = reader["ContactNumber"].ToString(),
                        Address = reader["Address"].ToString()
                    };
                }
            }

            if (supplier == null)
            {
                TempData["Error"] = "Supplier not found.";
                return RedirectToAction("Index");
            }

            return View(supplier);
        }

        // ✏️ Handle Edit
        [HttpPost]
        public IActionResult Edit(Supplier supplier)
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    var cmd = new SqlCommand(
                        "UPDATE Supplier SET Name=@Name, Email=@Email, ContactNumber=@ContactNumber, Address=@Address WHERE Id=@Id",
                        con);

                    cmd.Parameters.AddWithValue("@Id", supplier.Id);
                    cmd.Parameters.AddWithValue("@Name", supplier.Name);
                    cmd.Parameters.AddWithValue("@Email", supplier.Email);
                    cmd.Parameters.AddWithValue("@ContactNumber", supplier.ContactNumber ?? "");
                    cmd.Parameters.AddWithValue("@Address", supplier.Address ?? "");

                    cmd.ExecuteNonQuery();
                }

                TempData["Success"] = "Supplier updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating supplier: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // 📩 Order Product and Send Email
        [HttpPost]
        public IActionResult OrderProduct(int supplierId, string productDescription, int quantity, string supplierEmail)
        {
            try
            {
                var fromAddress = new MailAddress("act.gbentulan4@gmail.com", "Irish Beauty Product Wellness");
                var toAddress = new MailAddress(supplierEmail);
                const string fromPassword = "stnr tqhx pqyl xmad";

                string subject = "New Product Order";
                string body = $@"
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f9f9f9;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            background: #ffffff;
            margin: auto;
            padding: 20px;
            border-radius: 10px;
            box-shadow: 0 2px 6px rgba(0,0,0,0.1);
        }}
        h2 {{
            color: #2e86de;
            text-align: center;
        }}
        .details {{
            margin-top: 20px;
        }}
        .details p {{
            font-size: 14px;
            margin: 5px 0;
        }}
        .footer {{
            margin-top: 30px;
            text-align: center;
            font-size: 12px;
            color: #888;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>New Order Request</h2>
        <p>Hello,</p>
        <p>You have received a new order from <strong>Irish Beauty Product Wellness</strong>. Please see the details below:</p>

        <div class='details'>
            <p><strong>Product Description:</strong> {productDescription}</p>
            <p><strong>Quantity:</strong> {quantity}</p>
            <p><strong>Date Ordered:</strong> {DateTime.Now.ToString("MMMM dd, yyyy")}</p>
        </div>

        <p>Kindly prepare the items and reply to this email for confirmation.</p>

        <div class='footer'>
            <p>Thank you for your service!</p>
            <p>© {DateTime.Now.Year} Irish Beauty Product Wellness</p>
        </div>
    </div>
</body>
</html>
";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true // ✅ important for HTML template
                })
                {
                    smtp.Send(message);
                }

                return Json(new { success = true, message = "Order placed and email sent successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to send email: " + ex.Message });
            }
        }

    }
}
