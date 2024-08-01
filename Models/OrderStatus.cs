namespace SalesOrderApp.Models
{
    public class OrderStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public enum OrderStatusEnum
    {
        New = 1,
        Processing,
        Complete
    }
}
