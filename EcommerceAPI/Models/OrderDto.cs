namespace EcommerceAPI.Models
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = "Pending";
        //public decimal TotalPrice => Quantity * Price;
    }
}
