using QuestionService.DTOs;

namespace QuestionService.Service.Interface
{
    public interface IQuestionService
    {
        Task<IEnumerable<QuestionResponse>> GetAllAsync();
        Task<QuestionResponse?> GetByIdAsync(long id);
        Task<QuestionResponse> CreateAsync(CreateQuestionRequest request);
        Task<QuestionResponse> UpdateAsync(long id, UpdateQuestionRequest request);
        Task DeleteAsync(long id);
        Task<IEnumerable<QuestionResponse>> GetRandomAsync(int count = 10);
    }
}
