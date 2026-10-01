using AuthService.DTOs;
using AuthService.Entities;
using AuthService.Repository.Interface;
using AuthService.Service.Interface;

namespace AuthService.Service.Implement
{
    public class UserServiceImpl : IUserService
    {
        private readonly IUserRepository _userRepo;

        // Role constants
        private const int StudentRoleId = 2;

        public UserServiceImpl(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        // ── GET ALL ───────────────────────────────────────────────────────────
        public async Task<IEnumerable<UserResponse>> GetAllAsync(string callerRole)
        {
            // TEACHER chỉ thấy danh sách STUDENT (roleId = 2)
            // ADMIN thấy tất cả
            var users = callerRole == "ADMIN"
                ? await _userRepo.GetAllAsync()
                : await _userRepo.GetAllByRoleIdAsync(StudentRoleId);

            return users.Select(MapToResponse);
        }

        // ── GET BY ID ─────────────────────────────────────────────────────────
        public async Task<UserResponse> GetByIdAsync(int id, string callerRole)
        {
            var user = await _userRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"User with id {id} not found.");

            // TEACHER chỉ được xem user có role STUDENT
            if (callerRole != "ADMIN" && user.RoleId != StudentRoleId)
                throw new UnauthorizedAccessException("You do not have permission to view this user.");

            return MapToResponse(user);
        }

        // ── CREATE ────────────────────────────────────────────────────────────
        public async Task<UserResponse> CreateAsync(CreateUserRequest request, string callerRole)
        {
            // Kiểm tra trùng username / email
            if (await _userRepo.ExistsByUsernameAsync(request.Username))
                throw new InvalidOperationException("Username already exists.");

            if (await _userRepo.ExistsByEmailAsync(request.Email))
                throw new InvalidOperationException("Email already exists.");

            // Xác định role cho user mới
            int roleId;
            if (callerRole == "ADMIN")
            {
                // ADMIN có thể gán bất kỳ role nào, mặc định STUDENT nếu không truyền
                roleId = request.RoleId ?? StudentRoleId;
            }
            else
            {
                // TEACHER luôn tạo tài khoản STUDENT, bỏ qua RoleId nếu có truyền
                roleId = StudentRoleId;
            }

            var user = new User
            {
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Email    = request.Email,
                RoleId   = roleId
            };

            var created = await _userRepo.CreateAsync(user);
            return MapToResponse(created);
        }

        // ── UPDATE ────────────────────────────────────────────────────────────
        public async Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request, string callerRole)
        {
            var user = await _userRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"User with id {id} not found.");

            // TEACHER chỉ được sửa user có role STUDENT
            if (callerRole != "ADMIN" && user.RoleId != StudentRoleId)
                throw new UnauthorizedAccessException("You do not have permission to update this user.");

            // Kiểm tra trùng username nếu đổi
            if (request.Username is not null && request.Username != user.Username)
            {
                if (await _userRepo.ExistsByUsernameAsync(request.Username))
                    throw new InvalidOperationException("Username already exists.");
                user.Username = request.Username;
            }

            // Kiểm tra trùng email nếu đổi
            if (request.Email is not null && request.Email != user.Email)
            {
                if (await _userRepo.ExistsByEmailAsync(request.Email))
                    throw new InvalidOperationException("Email already exists.");
                user.Email = request.Email;
            }

            if (request.Password is not null)
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Chỉ ADMIN mới được đổi role
            if (request.RoleId is not null)
            {
                if (callerRole != "ADMIN")
                    throw new UnauthorizedAccessException("Only ADMIN can change user role.");
                user.RoleId = request.RoleId.Value;
            }

            var updated = await _userRepo.UpdateAsync(user);
            return MapToResponse(updated);
        }

        // ── DELETE ────────────────────────────────────────────────────────────
        public async Task DeleteAsync(int id, string callerRole)
        {
            var user = await _userRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"User with id {id} not found.");

            // TEACHER chỉ được xóa user có role STUDENT
            if (callerRole != "ADMIN" && user.RoleId != StudentRoleId)
                throw new UnauthorizedAccessException("You do not have permission to delete this user.");

            await _userRepo.SoftDeleteAsync(user);
        }

        // ── HELPER ────────────────────────────────────────────────────────────
        private static UserResponse MapToResponse(User user) => new()
        {
            Id       = user.Id,
            Username = user.Username,
            Email    = user.Email,
            RoleName = user.Role?.Name ?? string.Empty
        };
    }
}
