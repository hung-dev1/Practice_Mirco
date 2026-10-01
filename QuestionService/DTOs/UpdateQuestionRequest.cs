using System.ComponentModel.DataAnnotations;

namespace QuestionService.DTOs
{
    public class UpdateQuestionRequest
    {
        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string OptionA { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string OptionB { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string OptionC { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string OptionD { get; set; } = string.Empty;

        /// <summary>Đáp án đúng: "A", "B", "C" hoặc "D"</summary>
        [Required]
        [RegularExpression("^[AaBbCcDd]$", ErrorMessage = "CorrectAnswer must be A, B, C or D.")]
        public string CorrectAnswer { get; set; } = string.Empty;
    }
}
