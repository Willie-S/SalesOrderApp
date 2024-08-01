namespace SalesOrderApp.ViewModels
{
    public class OrdersEditHeaderViewModel
    {
        public int SalesOrderId { get; set; }
        public int OrderHeaderId { get; set; }
        public OrderHeaderViewModel OrderHeader { get; set; }
    }
}
