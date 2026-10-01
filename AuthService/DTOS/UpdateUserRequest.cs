using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class UpdateUserRequest
    {
        [MaxLength(100)]
        public string? Username { get; set; }

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(150)]
        public string? Email { get; set; }

        /// <summary>
        /// Chỉ ADMIN mới được đổi RoleId
        /// </summary>
        public int? RoleId { get; set; }
    }
}
