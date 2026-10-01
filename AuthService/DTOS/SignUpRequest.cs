using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class SignUpRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

   
        public string RoleName { get; set; } = "user";
    }
}
