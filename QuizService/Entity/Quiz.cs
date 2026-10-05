using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizService.Entity
{
    [Table("quiz")]
    public class Quiz
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("title")]
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("duration")]
        [Required]
        public int Duration { get; set; }

        [Column("created_by")]
        [Required]
        public long CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
    }
}
