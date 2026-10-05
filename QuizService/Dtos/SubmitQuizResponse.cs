namespace QuizService.DTOs
{
    public class SubmitQuizResponse
    {
        public long QuizId { get; set; }
        public int TotalQuestions { get; set; }
        public int AnsweredQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public decimal Score { get; set; }
    }
}
