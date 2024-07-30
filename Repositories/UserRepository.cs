using SalesOrderApp.Data;
using SalesOrderApp.Interfaces;

namespace SalesOrderApp.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SalesOrderAppDbContext _context;

        public UserRepository(SalesOrderAppDbContext context)
        {
            _context = context;
        }
    }
}
