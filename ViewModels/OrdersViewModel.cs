using SalesOrderApp.Models;

namespace SalesOrderApp.ViewModels
{
    public class OrdersViewModel
    {
        public List<SalesOrder> SalesOrders { get; set; }
        public SalesOrder? SelectedSalesOrder { get; set; }
    }
}
