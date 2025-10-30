using Irish_Beauty_Product.Data;
using Irish_Beauty_Product.Filters;
using Irish_Beauty_Product.Helpers;
using Irish_Beauty_Product.Models;
using Microsoft.AspNetCore.Mvc;

namespace Irish_Beauty_Product.Controllers
{
    [AuthorizeRole("Admin")]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Show list of users (Admin only)
        [HttpGet]
        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        //  GET: Add user
        [HttpGet]
        public IActionResult Create() => View();

        //  POST: Add user with hashed password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(User model)
        {
            if (ModelState.IsValid)
            {
                model.Password = PasswordHelper.HashPassword(model.Password); // Hash password
                _context.Users.Add(model);
                _context.SaveChanges();

                TempData["Success"] = "User has been added successfully!";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        //  Edit user
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();
            return View(user);
        }

        //  Edit user
        [HttpPost]
        [ValidateAntiForgeryToken]
        //  Edit User (keep or re-hash password)
        [HttpPost]
        public IActionResult Edit(User model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _context.Users.Find(model.UserId);
                if (existingUser == null) return NotFound();

                existingUser.Username = model.Username;
                existingUser.Role = model.Role;
                existingUser.Status = model.Status;

                // If password field is not empty, re-hash it
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    existingUser.Password = PasswordHelper.HashPassword(model.Password);
                }

                _context.Users.Update(existingUser);
                _context.SaveChanges();

                TempData["Warning"] = "User details have been updated!";
                return RedirectToAction("Index");
            }
            return View(model);
        }
    

     

        // ✅ Delete user
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();
            TempData["Error"] = "User has been deleted!";
            return RedirectToAction("Index");
        }
    }
}
