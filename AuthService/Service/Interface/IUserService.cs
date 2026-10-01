using AuthService.DTOs;

namespace AuthService.Service.Interface
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponse>> GetAllAsync(string callerRole);
        Task<UserResponse> GetByIdAsync(int id, string callerRole);
        Task<UserResponse> CreateAsync(CreateUserRequest request, string callerRole);
        Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request, string callerRole);
        Task DeleteAsync(int id, string callerRole);
    }
}
