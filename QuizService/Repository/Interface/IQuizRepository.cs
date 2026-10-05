using QuizService.Entity;

namespace QuizService.Repository.Interface
{
    public interface IQuizRepository
    {
        Task<IEnumerable<Quiz>> GetAllAsync();
        Task<Quiz?> GetByIdAsync(long id);
        Task<Quiz> CreateAsync(Quiz quiz);
        Task<Quiz> UpdateAsync(Quiz quiz);
        Task DeleteAsync(Quiz quiz);
        Task AddQuestionsAsync(IEnumerable<QuizQuestion> questions);
    }
}
