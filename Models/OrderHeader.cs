namespace SalesOrderApp.Models
{
    public class OrderHeader : BaseEntity
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public int OrderTypeId { get; set; }
        public int OrderStatusId { get; set; }
        public string CustomerName { get; set; }
        public DateTime CreateDate { get; set; }
        public int SalesOrderId { get; set; }

        // Navigation properties
        public OrderType OrderType { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public SalesOrder SalesOrder { get; set; }
    }
}
