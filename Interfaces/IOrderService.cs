using SalesOrderApp.Models;

namespace SalesOrderApp.Interfaces
{
    public interface IOrderService
    {
        Task<SalesOrder> CreateOrder(SalesOrder newOrder);
    }
}
