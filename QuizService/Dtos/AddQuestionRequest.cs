using System.ComponentModel.DataAnnotations;

namespace QuizService.DTOs
{
    public class AddQuestionRequest
    {
        [Range(1, long.MaxValue)]
        public long QuestionId { get; set; }
    }
}
