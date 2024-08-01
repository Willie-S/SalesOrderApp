using System.Xml.Serialization;

namespace SalesOrderApp.Models
{
    public class SalesOrder : BaseEntity
    {
        public int Id { get; set; }
        public int CreatedByUserId { get; set; }
        public int UpdatedByUserId { get; set; }

        // Navigation properties
        [XmlIgnore]
        public User CreatedByUser { get; set; }
        [XmlIgnore]
        public User UpdatedByUser { get; set; }
        [XmlIgnore]
        public OrderHeader OrderHeader { get; set; }
        [XmlIgnore]
        public ICollection<OrderLine> OrderLines { get; set; }
    }
}
