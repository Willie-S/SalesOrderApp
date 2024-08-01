using SalesOrderApp.Models;

namespace SalesOrderApp.Interfaces
{
    public interface ISalesOrderRepository
    {
        Task<IEnumerable<SalesOrder>> GetAllAsync();
        Task<IEnumerable<SalesOrder>> GetAllByUserIdAsync(int userId);
        Task<SalesOrder> GetByIdAsync(int salesOrderId);
        Task<SalesOrder> AddAsync(SalesOrder salesOrder);
        Task DeleteAsync(int id);
        Task<OrderHeader> GetOrderHeaderByIdAsync(int orderHeaderId);
        Task UpdateAsync(int salesOrderId, OrderHeader orderHeader, int userId);
        Task<OrderLine> GetOrderLineByIdAsync(int orderLineId);
        Task AddOrderLineAsync(int salesOrderId, OrderLine orderLine, int userId);
        Task AddOrderLinesBulkAsync(int salesOrderId, IEnumerable<OrderLine> orderLines, int userId);
        Task UpdateOrderLineAsync(OrderLine orderLine, int userId);
        Task<OrderLine> DeleteOrderLineAsync(int orderLineId, int userId);
        Task ReassignLineNumbersAsync(int salesOrderId);
    }
}
