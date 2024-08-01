using SalesOrderApp.Data;
using SalesOrderApp.Interfaces;
using SalesOrderApp.Models;
using System.Xml.Linq;

namespace SalesOrderApp.Repositories
{
    public class XmlSalesOrderRepository : IXmlSalesOrderRepository
    {
        private readonly XmlDbContext _context;

        public XmlSalesOrderRepository(XmlDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SalesOrder>> GetAllAsync()
        {
            return await Task.Run(() =>
            {
                // Load all sales orders
                var salesOrders = _context.SalesOrders.GetAll().ToList();

                // Load related OrderHeaders and OrderLines manually
                foreach (var salesOrder in salesOrders)
                {
                    // Load the related OrderHeader
                    salesOrder.OrderHeader = _context.OrderHeaders.GetAll().FirstOrDefault(oh => oh.SalesOrderId == salesOrder.Id);

                    // Load the enum props
                    salesOrder.OrderHeader.OrderType = _context.OrderType.GetById(salesOrder.OrderHeader.OrderTypeId);
                    salesOrder.OrderHeader.OrderStatus = _context.OrderStatus.GetById(salesOrder.OrderHeader.OrderStatusId);

                    // Load related OrderLines
                    salesOrder.OrderLines = _context.OrderLines.GetAll().Where(l => l.SalesOrderId == salesOrder.Id).ToList();

                    // Load each line's enum props
                    foreach (var orderLine in salesOrder.OrderLines)
                    {
                        orderLine.ProductType = _context.ProductType.GetById(orderLine.ProductTypeId);
                    }
                }

                return salesOrders;
            });
        }

        public async Task<IEnumerable<SalesOrder>> GetAllByUserIdAsync(int userId)
        {
            return await Task.Run(() =>
            {
                // Load all sales orders
                var salesOrders = _context.SalesOrders.GetAll().Where(s => s.CreatedByUserId == userId).ToList();

                // Load related OrderHeaders and OrderLines manually
                foreach (var salesOrder in salesOrders)
                {
                    // Load the related OrderHeader
                    salesOrder.OrderHeader = _context.OrderHeaders.GetAll().FirstOrDefault(oh => oh.SalesOrderId == salesOrder.Id);

                    // Load the enum props
                    salesOrder.OrderHeader.OrderType = _context.OrderType.GetById(salesOrder.OrderHeader.OrderTypeId);
                    salesOrder.OrderHeader.OrderStatus = _context.OrderStatus.GetById(salesOrder.OrderHeader.OrderStatusId);

                    // Load related OrderLines
                    salesOrder.OrderLines = _context.OrderLines.GetAll().Where(l => l.SalesOrderId == salesOrder.Id).ToList();

                    // Load each line's enum props
                    foreach (var orderLine in salesOrder.OrderLines)
                    {
                        orderLine.ProductType = _context.ProductType.GetById(orderLine.ProductTypeId);
                    }
                }

                return salesOrders;
            });
        }

        public async Task<SalesOrder> GetByIdAsync(int salesOrderId)
        {
            return await Task.Run(() =>
            {
                // Load the specific SalesOrder
                var salesOrder = _context.SalesOrders.GetById(salesOrderId);

                if (salesOrder != null)
                {
                    // Load the related OrderHeader
                    salesOrder.OrderHeader = _context.OrderHeaders.GetAll().FirstOrDefault(oh => oh.SalesOrderId == salesOrder.Id);

                    // Load the enum props
                    salesOrder.OrderHeader.OrderType = _context.OrderType.GetById(salesOrder.OrderHeader.OrderTypeId);
                    salesOrder.OrderHeader.OrderStatus = _context.OrderStatus.GetById(salesOrder.OrderHeader.OrderStatusId);

                    // Load related OrderLines
                    salesOrder.OrderLines = _context.OrderLines.GetAll().Where(l => l.SalesOrderId == salesOrder.Id).ToList();

                    // Load each line's enum props
                    foreach (var orderLine in salesOrder.OrderLines)
                    {
                        orderLine.ProductType = _context.ProductType.GetById(orderLine.ProductTypeId);
                    }
                }

                return salesOrder;
            });
        }

        public async Task<SalesOrder> AddAsync(SalesOrder salesOrder)
        {
            return await Task.Run(() =>
            {
                // Add the SalesOrder to the context
                _context.SalesOrders.Add(salesOrder);

                // Check if the SalesOrder has a new OrderHeader to add
                if (salesOrder.OrderHeader != null)
                {
                    // Add the OrderHeader to the context
                    salesOrder.OrderHeader.SalesOrderId = salesOrder.Id;
                    _context.OrderHeaders.Add(salesOrder.OrderHeader);
                }

                // Check if the SalesOrder has OrderLines to add
                if (salesOrder.OrderLines != null && salesOrder.OrderLines.Any())
                {
                    foreach (var orderLine in salesOrder.OrderLines)
                    {
                        // Add each OrderLine to the context
                        orderLine.SalesOrderId = salesOrder.Id;
                        _context.OrderLines.Add(orderLine);
                    }
                }

                return salesOrder;
            });
        }

        public async Task DeleteAsync(int id)
        {
            await Task.Run(() =>
            {
                var salesOrder = _context.SalesOrders.GetById(id);
                if (salesOrder != null)
                {
                    // Remove the related OrderHeader
                    var orderHeaders = _context.OrderHeaders.GetAll().Where(oh => oh.SalesOrderId == salesOrder.Id);
                    foreach (var header in orderHeaders)
                    {
                        _context.OrderHeaders.Delete(header);
                    }

                    // Remove the related OrderLines
                    var orderLines = _context.OrderLines.GetAll().Where(ol => ol.SalesOrderId == salesOrder.Id);
                    foreach (var line in orderLines)
                    {
                        _context.OrderLines.Delete(line);
                    }

                    // Remove the SalesOrder
                    _context.SalesOrders.Delete(salesOrder);
                }
            });
        }

        public async Task<OrderHeader> GetOrderHeaderByIdAsync(int orderHeaderId)
        {
            return await Task.Run(() =>
            {
                var orderHeader = _context.OrderHeaders.GetById(orderHeaderId);

                // Load the enum props
                orderHeader.OrderType = _context.OrderType.GetById(orderHeader.OrderTypeId);
                orderHeader.OrderStatus = _context.OrderStatus.GetById(orderHeader.OrderStatusId);

                return orderHeader;
            });
        }

        public async Task UpdateAsync(int salesOrderId, OrderHeader orderHeader, int userId)
        {
            var salesOrder = _context.SalesOrders.GetById(salesOrderId);
            if (salesOrder != null)
            {
                var existingHeader = _context.OrderHeaders.GetById(orderHeader.Id);
                if (existingHeader != null)
                {
                    _context.OrderHeaders.Update(orderHeader);
                }

                salesOrder.UpdatedByUserId = userId;
                _context.SalesOrders.Update(salesOrder);
            }
            await Task.CompletedTask;
        }

        public async Task<OrderLine> GetOrderLineByIdAsync(int orderLineId)
        {
            return await Task.Run(() =>
            {
                var orderLine = _context.OrderLines.GetById(orderLineId);

                // Load the enum props
                orderLine.ProductType = _context.ProductType.GetById(orderLine.ProductTypeId);

                return orderLine;
            });
        }

        public async Task AddOrderLineAsync(int salesOrderId, OrderLine orderLine, int userId)
        {
            var salesOrder = _context.SalesOrders.GetById(salesOrderId);
            if (salesOrder != null)
            {
                orderLine.SalesOrderId = salesOrderId;
                _context.OrderLines.Add(orderLine);

                salesOrder.UpdatedByUserId = userId;
                _context.SalesOrders.Update(salesOrder);
            }
            await Task.CompletedTask;
        }

        public async Task AddOrderLinesBulkAsync(int salesOrderId, IEnumerable<OrderLine> orderLines, int userId)
        {
            var salesOrder = _context.SalesOrders.GetById(salesOrderId);
            if (salesOrder != null)
            {
                foreach (var orderLine in orderLines)
                {
                    orderLine.SalesOrderId = salesOrderId;
                    _context.OrderLines.Add(orderLine);
                }

                salesOrder.UpdatedByUserId = userId;
                _context.SalesOrders.Update(salesOrder);
            }
            await Task.CompletedTask;
        }

        public async Task UpdateOrderLineAsync(OrderLine orderLine, int userId)
        {
            var salesOrder = _context.SalesOrders.GetById(orderLine.SalesOrderId);
            if (salesOrder != null)
            {
                var existingOrderLine = _context.OrderLines.GetById(orderLine.Id);
                if (existingOrderLine != null)
                {
                    _context.OrderLines.Update(orderLine);
                }

                salesOrder.UpdatedByUserId = userId;
                _context.SalesOrders.Update(salesOrder);
            }
            await Task.CompletedTask;
        }

        public async Task<OrderLine> DeleteOrderLineAsync(int orderLineId, int userId)
        {
            var orderLine = _context.OrderLines.GetById(orderLineId);
            if (orderLine != null)
            {
                var salesOrder = _context.SalesOrders.GetById(orderLine.SalesOrderId);
                if (salesOrder != null)
                {
                    salesOrder.UpdatedByUserId = userId;
                    _context.SalesOrders.Update(salesOrder);
                }

                _context.OrderLines.Delete(orderLine);
            }
            return await Task.FromResult(orderLine);
        }

        public async Task ReassignLineNumbersAsync(int salesOrderId)
        {
            var orderLines = _context.OrderLines.GetAll().Where(ol => ol.SalesOrderId == salesOrderId).ToList();

            if (orderLines.Count == 0) return;

            for (int i = 0; i < orderLines.Count; i++)
            {
                orderLines[i].LineNumber = i + 1;
                _context.OrderLines.Update(orderLines[i]);
            }

            await Task.CompletedTask;
        }
    }
}