using System.ComponentModel.DataAnnotations;

namespace QuizService.DTOs
{
    public class SubmitQuizRequest
    {
        [Required]
        public List<SubmitQuizAnswer> Answers { get; set; } = new();
    }

    public class SubmitQuizAnswer
    {
        [Range(1, long.MaxValue)]
        public long QuestionId { get; set; }

        [Required]
        [RegularExpression("^[AaBbCcDd]$", ErrorMessage = "Answer must be A, B, C or D.")]
        public string Answer { get; set; } = string.Empty;
    }
}
