namespace SalesOrderApp.ViewModels
{
    public class OrdersEditLineViewModel
    {
        public int SalesOrderId { get; set; }
        public int OrderLineId { get; set; }
        public OrderLineViewModel OrderLine { get; set; }
    }
}
