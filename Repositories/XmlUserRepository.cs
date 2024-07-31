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
            await _context.AddUserAsync(user);
            return user;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.GetUserByEmailAsync(email);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.EmailExistsAsync(email);
        }
    }
}
