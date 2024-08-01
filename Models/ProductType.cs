namespace SalesOrderApp.Models
{
    public class ProductType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public enum ProductTypeEnum
    {
        Apparel = 1,
        Parts,
        Equipment,
        Motor
    }
}
