using QuizService.DTOs;
using Refit;

namespace QuizService.ServiceClients
{
    public interface IQuestionClient
    {
        [Get("/api/questions/random")]
        Task<List<QuestionResponse>> GetRandomAsync(
            [Header("Authorization")] string authorization,
            [Query] int count = 10);

        [Get("/api/questions/{id}")]
        Task<QuestionResponse> GetByIdAsync(
            long id,
            [Header("Authorization")] string authorization);
    }
}
