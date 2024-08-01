using Microsoft.EntityFrameworkCore;
using SalesOrderApp.Data;
using SalesOrderApp.Interfaces;
using SalesOrderApp.Models;

namespace SalesOrderApp.Repositories
{
    public class SalesOrderRepository : ISalesOrderRepository
    {
        private readonly SalesOrderAppDbContext _context;

        public SalesOrderRepository(SalesOrderAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SalesOrder>> GetAllByUserIdAsync(int userId)
        {
            return await _context.SalesOrders
                .Include(s => s.OrderHeader).ThenInclude(h => h.OrderType)
                .Include(s => s.OrderHeader).ThenInclude(h => h.OrderStatus)
                .Include(s => s.OrderLines).ThenInclude(l => l.ProductType)
                .Where(s => s.CreatedByUserId == userId)
                .ToListAsync();
        }

        public async Task<SalesOrder> GetByIdAsync(int salesOrderId)
        {
            return await _context.SalesOrders
                .Include(so => so.OrderHeader)
                .Include(so => so.OrderLines)
                .FirstOrDefaultAsync(so => so.Id == salesOrderId);
        }

        public async Task<SalesOrder> AddAsync(SalesOrder salesOrder)
        {
            _context.SalesOrders.Add(salesOrder);
            await _context.SaveChangesAsync();
            return salesOrder;
        }

        public async Task DeleteAsync(int id)
        {
            var salesOrder = await _context.SalesOrders
                .FirstOrDefaultAsync(s => s.Id == id);

            if (salesOrder != null)
            {
                _context.SalesOrders.Remove(salesOrder);
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task<OrderHeader> GetOrderHeaderByIdAsync(int orderHeaderId)
        {
            return await _context.OrderHeaders.FindAsync(orderHeaderId);
        }

        public async Task UpdateAsync(int salesOrderId, OrderHeader orderHeader, int userId)
        {
            var salesOrder = await _context.SalesOrders.FindAsync(salesOrderId);

            if (salesOrder != null)
            {
                salesOrder.UpdatedByUserId = userId;

                var existingHeader = await _context.OrderHeaders.FindAsync(orderHeader.Id);
                if (existingHeader != null) _context.Entry(existingHeader).CurrentValues.SetValues(orderHeader);

                await _context.SaveChangesAsync();
            }
        }

        public async Task<OrderLine> GetOrderLineByIdAsync(int orderId)
        {
            return await _context.OrderLines.FindAsync(orderId);
        }

        public async Task AddOrderLineAsync(int salesOrderId, OrderLine orderLine, int userId)
        {
            var salesOrder = await _context.SalesOrders.FindAsync(salesOrderId);
            if (salesOrder != null)
            {
                salesOrder.UpdatedByUserId = userId;
                orderLine.SalesOrderId = salesOrderId;

                await _context.OrderLines.AddAsync(orderLine);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddOrderLinesBulkAsync(int salesOrderId, IEnumerable<OrderLine> orderLines, int userId)
        {
            var salesOrder = await _context.SalesOrders.FindAsync(salesOrderId);
            if (salesOrder != null)
            {
                salesOrder.UpdatedByUserId = userId;

                foreach (var orderLine in orderLines)
                {
                    orderLine.SalesOrderId = salesOrderId;
                }

                await _context.OrderLines.AddRangeAsync(orderLines);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateOrderLineAsync(OrderLine orderLine, int userId)
        {
            var salesOrder = await _context.SalesOrders.FindAsync(orderLine.SalesOrderId);
            if (salesOrder != null)
            {
                salesOrder.UpdatedByUserId = userId;

                var existingOrderLine = await _context.OrderLines.FindAsync(orderLine.Id);
                if (existingOrderLine != null) _context.Entry(existingOrderLine).CurrentValues.SetValues(orderLine);

                await _context.SaveChangesAsync();
            }
        }

        public async Task<OrderLine> DeleteOrderLineAsync(int orderLineId, int userId)
        {
            var orderLine = await _context.OrderLines.FindAsync(orderLineId);
            if (orderLine != null)
            {
                var salesOrder = await _context.SalesOrders.FindAsync(orderLine.SalesOrderId);
                if (salesOrder != null)
                {
                    salesOrder.UpdatedByUserId = userId;
                }

                _context.OrderLines.Remove(orderLine);
                await _context.SaveChangesAsync();
            }

            return orderLine;
        }

        public async Task ReassignLineNumbersAsync(int salesOrderId)
        {
            var orderLines = await _context.OrderLines
                .Where(ol => ol.SalesOrderId == salesOrderId)
                .ToListAsync();

            if (orderLines.Count == 0) return;

            // Reassign LineNumber based on the index in the list
            for (int i = 0; i < orderLines.Count; i++)
            {
                orderLines[i].LineNumber = i + 1;
            }

            _context.OrderLines.UpdateRange(orderLines);
            await _context.SaveChangesAsync();
        }
    }
}
