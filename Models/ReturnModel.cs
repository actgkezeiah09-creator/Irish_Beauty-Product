namespace Irish_Beauty_Product.Models
{
    public class ReturnModel
    {
        public int ProductId { get; set; }
        
        public int QuantityReturned { get; set; }    
        public string Reason { get; set; }       
        public string ProcessedBy { get; set; }    
        public DateTime DateReturned { get; set; } = DateTime.Now;
    }
}
