using SalesOrderApp.Data;
using SalesOrderApp.Interfaces;

namespace SalesOrderApp.Repositories
{
    public class XmlUserRepository : IUserRepository
    {
        private readonly XmlDbContext _context;

        public XmlUserRepository(XmlDbContext context)
        {
            _context = context;
        }
    }
}
