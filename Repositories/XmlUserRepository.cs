using SalesOrderApp.Data;
using SalesOrderApp.Interfaces;
using SalesOrderApp.Models;

namespace SalesOrderApp.Repositories
{
    public class XmlUserRepository : IUserRepository
    {
        private readonly XmlDbContext _context;

        public XmlUserRepository(XmlDbContext context)
        {
            _context = context;
        }

        public async Task<User> AddUserAsync(User user)
        {
            _context.Users.Add(user);
            return user;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return _context.Users.GetAll().FirstOrDefault(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return _context.Users.GetAll().Any(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }
}
