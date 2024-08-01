namespace SalesOrderApp.Models
{
    public class SalesOrder : BaseEntity
    {
        public int Id { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }

        // Navigation properties
        public User CreatedByUser { get; set; }
        public User UpdatedByUser { get; set; }
        public OrderHeader OrderHeader { get; set; }
        public ICollection<OrderLine> OrderLines { get; set; }
    }
}
