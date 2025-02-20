﻿using System.Xml.Serialization;

namespace SalesOrderApp.Models
{
    public class OrderLine : BaseEntity
    {
        public int Id { get; set; }
        public int LineNumber { get; set; }
        public string ProductCode { get; set; }
        public int ProductTypeId { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SalesPrice { get; set; }
        public int Quantity { get; set; }
        public int SalesOrderId { get; set; }

        // Navigation properties
        [XmlIgnore]
        public ProductType ProductType { get; set; }
        [XmlIgnore]
        public SalesOrder SalesOrder { get; set; }
    }
}
