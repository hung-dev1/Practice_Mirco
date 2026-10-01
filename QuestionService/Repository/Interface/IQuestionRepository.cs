using QuestionService.Entity;

namespace QuestionService.Repository.Interface
{
    public interface IQuestionRepository
    {
        Task<IEnumerable<Question>> GetAllAsync();
        Task<Question?> GetByIdAsync(long id);
        Task<Question> CreateAsync(Question question);
        Task<Question> UpdateAsync(Question question);
        Task DeleteAsync(Question question);
        Task<bool> ExistsByIdAsync(long id);
    }
}
