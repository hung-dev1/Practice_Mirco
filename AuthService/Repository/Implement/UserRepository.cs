using AuthService.Data;
using AuthService.Entities;
using AuthService.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repository.Implement
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
            => await _context.Users
                             .Include(u => u.Role)
                             .Where(u => u.IsActive)
                             .OrderBy(u => u.Id)
                             .ToListAsync();

        public async Task<IEnumerable<User>> GetAllByRoleIdAsync(int roleId)
            => await _context.Users
                             .Include(u => u.Role)
                             .Where(u => u.RoleId == roleId && u.IsActive)
                             .OrderBy(u => u.Id)
                             .ToListAsync();

        public async Task<User?> GetByIdAsync(int id)
            => await _context.Users
                             .Include(u => u.Role)
                             .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);

        public async Task<User?> GetByUsernameAsync(string username)
            => await _context.Users
                             .Include(u => u.Role)
                             .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        public async Task<User?> GetByEmailAsync(string email)
            => await _context.Users
                             .Include(u => u.Role)
                             .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            // Reload để có navigation property Role
            await _context.Entry(user).Reference(u => u.Role).LoadAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            await _context.Entry(user).Reference(u => u.Role).LoadAsync();
            return user;
        }

        public async Task SoftDeleteAsync(User user)
        {
            user.IsActive = false;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
            => await _context.Users.AnyAsync(u => u.Username == username && u.IsActive);

        public async Task<bool> ExistsByEmailAsync(string email)
            => await _context.Users.AnyAsync(u => u.Email == email && u.IsActive);
    }
}
