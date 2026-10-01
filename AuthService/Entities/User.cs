using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Entities
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("username")]
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Column("password")]
        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Column("email")]
        [Required]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Column("role_id")]
        public int RoleId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation property
        [ForeignKey("RoleId")]
        public Role Role { get; set; } = null!;
    }
}
