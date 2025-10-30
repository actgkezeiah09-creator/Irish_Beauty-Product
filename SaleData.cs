using Irish_Beauty_Product.Models;
using System.ComponentModel.DataAnnotations;

namespace Irish_Beauty_Product
{
    public class SaleData
    {
        public List<CartItem> Cart { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public decimal DiscountRate { get; set; }
    }

    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        

    }

}
    

