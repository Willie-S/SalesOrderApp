using Microsoft.EntityFrameworkCore;
using SalesOrderApp.Data;
using SalesOrderApp.Interfaces;
using SalesOrderApp.Models;

namespace SalesOrderApp.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SalesOrderAppDbContext _context;

        public UserRepository(SalesOrderAppDbContext context)
        {
            _context = context;
        }

        public async Task<User> AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email.Trim().ToLower() == email.Trim().ToLower());
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.Trim().ToLower() == email.Trim().ToLower());
        }
    }
}
