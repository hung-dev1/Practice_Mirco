using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Entities
{
    [Table("roles")]
    public class Role
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
