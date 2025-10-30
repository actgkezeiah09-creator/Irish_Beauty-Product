using System.Security.Cryptography;
using System.Text;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Irish_Beauty_Product.Helpers
{
    public static class PasswordHelper
    {
        // Hash a password using SHA256
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Verify a password against the stored hash
        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            string hashOfInput = HashPassword(enteredPassword);
            return hashOfInput.Equals(storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
