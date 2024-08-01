using SalesOrderApp.Models;
using SalesOrderApp.Utilities;
using System.Xml.Linq;

namespace SalesOrderApp.Data
{
    public class XmlDbContext
    {
        public XmlDbSet<SalesOrder> SalesOrders { get; private set; }
        public XmlDbSet<OrderHeader> OrderHeaders { get; private set; }
        public XmlDbSet<OrderLine> OrderLines { get; private set; }
        public XmlDbSet<OrderStatus> OrderStatus { get; private set; }
        public XmlDbSet<OrderType> OrderType { get; private set; }
        public XmlDbSet<ProductType> ProductType { get; private set; }
        public XmlDbSet<User> Users { get; private set; }
        public XmlDbSet<UserRole> UserRoles { get; private set; }

        public XmlDbContext(string orderHeadersFilePath, string orderLinesFilePath, string orderStatusFilePath, string orderTypeFilePath, string productTypeFilePath, string salesOrdersFilePath, string userFilePath, string userRolesFilePath)
        {
            SalesOrders = new XmlDbSet<SalesOrder>(salesOrdersFilePath);
            OrderHeaders = new XmlDbSet<OrderHeader>(orderHeadersFilePath);
            OrderLines = new XmlDbSet<OrderLine>(orderLinesFilePath);
            OrderStatus = new XmlDbSet<OrderStatus>(orderStatusFilePath);
            OrderType = new XmlDbSet<OrderType>(orderTypeFilePath);
            ProductType = new XmlDbSet<ProductType>(productTypeFilePath);
            Users = new XmlDbSet<User>(userFilePath);
            UserRoles = new XmlDbSet<UserRole>(userRolesFilePath);

            SeedOrderStatusData();
            SeedOrderTypeData();
            SeedProductTypeData();
            SeedUserRolesData();
        }

        private void SeedOrderStatusData()
        {
            var existingIds = OrderStatus.GetAll().Select(x => x.Id).ToHashSet();

            foreach (OrderStatusEnum status in Enum.GetValues(typeof(OrderStatusEnum)))
            {
                if (!existingIds.Contains((int)status))
                {
                    OrderStatus.Add(new OrderStatus { Id = (int)status, Name = status.ToString() }, false);
                }
            }
        }

        private void SeedOrderTypeData()
        {
            var existingIds = OrderType.GetAll().Select(x => x.Id).ToHashSet();

            foreach (OrderTypeEnum type in Enum.GetValues(typeof(OrderTypeEnum)))
            {
                if (!existingIds.Contains((int)type))
                {
                    OrderType.Add(new OrderType { Id = (int)type, Name = type.ToString() }, false);
                }
            }
        }

        private void SeedProductTypeData()
        {
            var existingIds = ProductType.GetAll().Select(x => x.Id).ToHashSet();

            foreach (ProductTypeEnum type in Enum.GetValues(typeof(ProductTypeEnum)))
            {
                if (!existingIds.Contains((int)type))
                {
                    ProductType.Add(new ProductType { Id = (int)type, Name = type.ToString() }, false);
                }
            }
        }

        private void SeedUserRolesData()
        {
            var existingIds = UserRoles.GetAll().Select(x => x.Id).ToHashSet();

            foreach (UserRoleEnum role in Enum.GetValues(typeof(UserRoleEnum)))
            {
                if (!existingIds.Contains((int)role))
                {
                    UserRoles.Add(new UserRole { Id = (int)role, Name = role.ToString() }, false);
                }
            }
        }
    }
}
