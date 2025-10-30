using Irish_Beauty_Product.Models;
using Irish_Beauty_Product.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Irish_Beauty_Product.Helpers;

namespace Irish_Beauty_Product.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Login(User model)
        {
            // Find user by username first
            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username && u.Status == "Active");

            if (user != null)
            {
                // Hash the entered password before comparing
                string hashedInput = PasswordHelper.HashPassword(model.Password);

                if (user.Password == hashedInput)
                {
                    // ✅ Store session info
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Role", user.Role);

                    // ✅ Redirect based on role
                    switch (user.Role)
                    {
                        case "Admin":
                            return RedirectToAction("Dashboard", "Admin");

                        case "Cashier":
                            return RedirectToAction("POS", "POS"); 

                        case "Staff":
                            return RedirectToAction("Inventory", "Inventory");

                        default:
                            return RedirectToAction("Index", "Home");
                    }
                }
            }

            //  Login failed
            ViewBag.Error = "Invalid username or password!";
            return View(model);
        }




        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");

        }

    
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string username)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                ViewBag.Error = "No account found with that username.";
                return View();
            }

           
            TempData["Username"] = username;
            return RedirectToAction("ResetPassword");
        }

       

        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (TempData["Username"] == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Username = TempData["Username"].ToString();
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string username, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(username))
            {
                ViewBag.Error = "Invalid session. Please try again.";
                return RedirectToAction("ForgotPassword");
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match!";
                ViewBag.Username = username;
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                ViewBag.Error = "User not found.";
                return View();
            }

            user.Password = PasswordHelper.HashPassword(newPassword);
            _context.SaveChanges();

            TempData["ResetSuccess"] = "Your password has been reset successfully!";
            return RedirectToAction("ResetSuccess");
        }

        
        [HttpGet]
        public IActionResult ResetSuccess()
        {
            return View();
        }

    }
}

    



