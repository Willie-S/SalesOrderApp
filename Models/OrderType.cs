namespace SalesOrderApp.Models
{
    public class OrderType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public enum OrderTypeEnum
    {
        Normal = 1,
        Staff,
        Mechanical,
        Perishable
    }
}
