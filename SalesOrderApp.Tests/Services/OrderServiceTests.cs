using System.Threading.Tasks;
using Moq;
using Xunit;
using SalesOrderApp.Models;
using SalesOrderApp.Repositories;
using SalesOrderApp.Services;
using SalesOrderApp.Interfaces;

namespace SalesOrderApp.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<ISalesOrderRepository> _mockSalesOrderRepository;
        private readonly Mock<IXmlSalesOrderRepository> _mockXmlSalesOrderRepository;
        private readonly IOrderService _orderService;

        public OrderServiceTests()
        {
            _mockSalesOrderRepository = new Mock<ISalesOrderRepository>();
            _mockXmlSalesOrderRepository = new Mock<IXmlSalesOrderRepository>();
            _orderService = new OrderService(_mockSalesOrderRepository.Object, _mockXmlSalesOrderRepository.Object);
        }

        [Fact]
        public async Task CreateOrder_ShouldCreateNewSalesOrderAndOrderLines()
        {
            // Arrange
            var newOrder = new SalesOrder
            {
                OrderHeader = new OrderHeader
                {
                    OrderNumber = "SO123456",
                    OrderTypeId = 1,
                    OrderStatusId = 1,
                    CustomerName = "Test Customer",
                    CreateDate = DateTime.Now
                },
                OrderLines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        ProductCode = "P001",
                        ProductTypeId = 1,
                        CostPrice = 10,
                        SalesPrice = 15,
                        Quantity = 1
                    },
                    new OrderLine
                    {
                        ProductCode = "P002",
                        ProductTypeId = 1,
                        CostPrice = 20,
                        SalesPrice = 25,
                        Quantity = 2
                    }
                }
            };

            _mockSalesOrderRepository.Setup(r => r.AddAsync(It.IsAny<SalesOrder>()))
                .ReturnsAsync((SalesOrder order) => order);
            _mockXmlSalesOrderRepository.Setup(r => r.AddAsync(It.IsAny<SalesOrder>()))
                .ReturnsAsync((SalesOrder order) => order);

            // Act
            var result = await _orderService.CreateOrder(newOrder);

            // Assert
            _mockSalesOrderRepository.Verify(r => r.AddAsync(It.IsAny<SalesOrder>()), Times.Once);
            _mockXmlSalesOrderRepository.Verify(r => r.AddAsync(It.IsAny<SalesOrder>()), Times.Once);
            Assert.NotNull(result);
            Assert.NotNull(result.OrderHeader);
            Assert.Equal("SO123456", result.OrderHeader.OrderNumber);
            Assert.Equal(2, result.OrderLines.Count);
        }

        [Fact]
        public async Task CreateOrder_ShouldAssignSequentialLineNumbers()
        {
            // Arrange
            var newOrder = new SalesOrder
            {
                OrderHeader = new OrderHeader
                {
                    OrderNumber = "SO123456",
                    OrderTypeId = 1,
                    OrderStatusId = 1,
                    CustomerName = "Test Customer",
                    CreateDate = DateTime.Now
                },
                OrderLines = new List<OrderLine>
                {
                    new OrderLine
                    {
                        ProductCode = "P001",
                        ProductTypeId = 1,
                        CostPrice = 10,
                        SalesPrice = 15,
                        Quantity = 1
                    },
                    new OrderLine
                    {
                        ProductCode = "P002",
                        ProductTypeId = 1,
                        CostPrice = 20,
                        SalesPrice = 25,
                        Quantity = 2
                    }
                }
            };

            _mockSalesOrderRepository.Setup(r => r.AddAsync(It.IsAny<SalesOrder>()))
                .ReturnsAsync((SalesOrder order) =>
                {
                    int lineNumber = 1;
                    foreach (var line in order.OrderLines)
                    {
                        line.LineNumber = lineNumber++;
                    }
                    return order;
                });
            _mockXmlSalesOrderRepository.Setup(r => r.AddAsync(It.IsAny<SalesOrder>()))
                .ReturnsAsync((SalesOrder order) =>
                {
                    int lineNumber = 1;
                    foreach (var line in order.OrderLines)
                    {
                        line.LineNumber = lineNumber++;
                    }
                    return order;
                });

            // Act
            var result = await _orderService.CreateOrder(newOrder);

            // Assert
            _mockSalesOrderRepository.Verify(r => r.AddAsync(It.IsAny<SalesOrder>()), Times.Once);
            _mockXmlSalesOrderRepository.Verify(r => r.AddAsync(It.IsAny<SalesOrder>()), Times.Once);
            Assert.NotNull(result);
            Assert.Equal(1, result.OrderLines.First().LineNumber);
            Assert.Equal(2, result.OrderLines.Last().LineNumber);
        }
    }
}
