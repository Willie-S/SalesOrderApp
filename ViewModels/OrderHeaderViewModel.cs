using SalesOrderApp.Models;
using System.ComponentModel.DataAnnotations;

namespace SalesOrderApp.ViewModels
{
    public class OrderHeaderViewModel
    {
        [Required]
        public string OrderNumber { get; set; }

        [Required]
        public OrderTypeEnum OrderType { get; set; }

        [Required]
        public OrderStatusEnum OrderStatus { get; set; }

        [Required]
        public string CustomerName { get; set; }
        
        [Required]
        public DateTime CreateDate { get; set; }
    }
}
