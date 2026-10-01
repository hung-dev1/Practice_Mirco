using AuthService.Entities;

namespace AuthService.Repository.Interface
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<IEnumerable<User>> GetAllByRoleIdAsync(int roleId);
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task SoftDeleteAsync(User user);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<bool> ExistsByEmailAsync(string email);
    }
}
