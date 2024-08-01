using System.Threading.Tasks;
using SalesOrderApp.Interfaces;
using SalesOrderApp.Models;
using SalesOrderApp.Repositories;

namespace SalesOrderApp.Services
{
    public class OrderService : IOrderService
    {
        private readonly ISalesOrderRepository _salesOrderRepository;
        private readonly IXmlSalesOrderRepository _xmlSalesOrderRepository;

        public OrderService(ISalesOrderRepository salesOrderRepository, IXmlSalesOrderRepository xmlSalesOrderRepository)
        {
            _salesOrderRepository = salesOrderRepository;
            _xmlSalesOrderRepository = xmlSalesOrderRepository;
        }

        public async Task<SalesOrder> CreateOrder(SalesOrder newOrder)
        {
            // Commit the changes to the SQL DB
            var savedOrder = await _salesOrderRepository.AddAsync(newOrder);
            await _salesOrderRepository.ReassignLineNumbersAsync(savedOrder.Id);

            // Commit the changes to the XML DB
            var xmlSavedOrder = await _xmlSalesOrderRepository.AddAsync(newOrder);
            await _xmlSalesOrderRepository.ReassignLineNumbersAsync(xmlSavedOrder.Id);

            return savedOrder;
        }
    }
}
