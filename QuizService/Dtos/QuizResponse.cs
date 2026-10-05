namespace QuizService.DTOs
{
    public class QuizResponse
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Duration { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<QuizQuestionResponse> Questions { get; set; } = new();
    }

    public class QuizQuestionResponse
    {
        public long QuestionId { get; set; }
        public int QuestionOrder { get; set; }
    }
}
