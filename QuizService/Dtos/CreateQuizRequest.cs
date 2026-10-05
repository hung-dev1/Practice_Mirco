using System.ComponentModel.DataAnnotations;

namespace QuizService.DTOs
{
    public class CreateQuizRequest
    {
        [Required, MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int Duration { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
