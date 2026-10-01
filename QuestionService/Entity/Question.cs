using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionService.Entity
{
    [Table("question")]
    public class Question
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("title")]
        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [Column("option_a")]
        [Required]
        [MaxLength(255)]
        public string OptionA { get; set; } = string.Empty;

        [Column("option_b")]
        [Required]
        [MaxLength(255)]
        public string OptionB { get; set; } = string.Empty;

        [Column("option_c")]
        [Required]
        [MaxLength(255)]
        public string OptionC { get; set; } = string.Empty;

        [Column("option_d")]
        [Required]
        [MaxLength(255)]
        public string OptionD { get; set; } = string.Empty;

        [Column("correct_answer")]
        [Required]
        [MaxLength(1)]
        public string CorrectAnswer { get; set; } = string.Empty;
    }
}
