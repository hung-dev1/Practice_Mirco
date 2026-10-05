using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizService.Entity
{
    [Table("quiz_question")]
    public class QuizQuestion
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("quiz_id")]
        [Required]
        public long QuizId { get; set; }

        [Column("question_id")]
        [Required]
        public long QuestionId { get; set; }

        [Column("question_order")]
        [Required]
        public int QuestionOrder { get; set; }

        // Navigation property
        [ForeignKey("QuizId")]
        public Quiz Quiz { get; set; } = null!;
    }
}
