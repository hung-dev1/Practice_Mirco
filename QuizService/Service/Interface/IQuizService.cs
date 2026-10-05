using QuizService.DTOs;

namespace QuizService.Service.Interface
{
    public interface IQuizService
    {
        Task<IEnumerable<QuizResponse>> GetAllAsync();
        Task<QuizResponse?> GetByIdAsync(long id);
        Task<QuizResponse> CreateAsync(CreateQuizRequest request, long createdBy);
        Task<QuizResponse> UpdateAsync(long id, UpdateQuizRequest request);
        Task DeleteAsync(long id);
        Task<QuestionResponse> AddQuestionAsync(long quizId, long questionId, string authorization);
        Task<List<QuestionResponse>> AddRandomQuestionsAsync(long quizId, string authorization, int count = 10);
        Task<SubmitQuizResponse> SubmitQuizAsync(long quizId, SubmitQuizRequest request, string authorization);
    }
}
