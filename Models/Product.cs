using System.ComponentModel.DataAnnotations;

namespace Irish_Beauty_Product.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Cost is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than 0")]
        public decimal Cost { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int QuantityOnHand { get; set; }

        [Required(ErrorMessage = "SKU is required")]
        public string SKU { get; set; }

        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Batch Number")]
        public string BatchNumber { get; set; }

        [Display(Name = "Low Stock Alert")]
        public int LowStockAlert { get; set; } = 5;

        public int? SupplierId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    public class SaleData
    {
        public List<CartItem> Cart { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }
    }

    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class SalesHistory
    {
        public int SaleId { get; set; }
        public DateTime SaleDate { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public decimal TotalAmount { get; set; }
    }
}

