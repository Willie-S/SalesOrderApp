namespace SalesOrderApp.ViewModels
{
    public class OrdersAddViewModel
    {
        public OrderHeaderViewModel OrderHeader { get; set; }
        public List<OrderLineViewModel> OrderLines { get; set; }
    }
}
