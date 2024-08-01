using SalesOrderApp.Models;
using System.ComponentModel.DataAnnotations;

namespace SalesOrderApp.ViewModels
{
    public class OrderLineViewModel
    {
        [Required]
        public string ProductCode { get; set; }

        [Required]
        public ProductTypeEnum ProductType { get; set; }

        [Required]
        public decimal CostPrice { get; set; }

        [Required]
        public decimal SalesPrice { get; set; }

        [Required]
        public int Quantity { get; set; }
    }
}
