using SalesOrderApp.Models;

namespace SalesOrderApp.Interfaces
{
    public interface ISalesOrderRepository
    {
        Task<IEnumerable<SalesOrder>> GetAllByUserIdAsync(int userId);
        Task<SalesOrder> GetByIdAsync(int salesOrderId);
        Task<SalesOrder> AddAsync(SalesOrder salesOrder);
        Task DeleteAsync(int id);
        Task UpdateAsync(int salesOrderId, OrderHeader orderHeader, int userId);
        Task AddOrderLineAsync(int salesOrderId, OrderLine orderLine, int userId);
        Task AddOrderLinesBulkAsync(int salesOrderId, IEnumerable<OrderLine> orderLines, int userId);
        Task UpdateOrderLineAsync(OrderLine orderLine, int userId);
        Task DeleteOrderLineAsync(int orderLineId, int userId);
        Task ReassignLineNumbersAsync(int salesOrderId);
    }
}
