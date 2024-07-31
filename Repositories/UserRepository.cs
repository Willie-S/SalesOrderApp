using Microsoft.EntityFrameworkCore;
using SalesOrderApp.Data;
using SalesOrderApp.Interfaces;
using SalesOrderApp.Models;
using SalesOrderApp.Utilities;

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
            return await _context.Users.FirstOrDefaultAsync(u => GeneralHelper.CompareStrings(u.Email, email));
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.Trim().ToLower() == email.Trim().ToLower());
        }
    }
}
