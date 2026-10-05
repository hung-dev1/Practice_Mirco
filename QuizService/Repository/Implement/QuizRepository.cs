using Microsoft.EntityFrameworkCore;
using QuizService.Data;
using QuizService.Entity;
using QuizService.Repository.Interface;

namespace QuizService.Repository.Implement
{
    public class QuizRepository : IQuizRepository
    {
        private readonly AppDbContext _context;

        public QuizRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Quiz>> GetAllAsync()
            => await _context.Quizzes.AsNoTracking()
                .Include(q => q.QuizQuestions).OrderBy(q => q.Id).ToListAsync();

        public Task<Quiz?> GetByIdAsync(long id)
            => _context.Quizzes.Include(q => q.QuizQuestions)
                .FirstOrDefaultAsync(q => q.Id == id);

        public async Task<Quiz> CreateAsync(Quiz quiz)
        {
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();
            return quiz;
        }

        public async Task<Quiz> UpdateAsync(Quiz quiz)
        {
            _context.Quizzes.Update(quiz);
            await _context.SaveChangesAsync();
            return quiz;
        }

        public async Task DeleteAsync(Quiz quiz)
        {
            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
        }

        public async Task AddQuestionsAsync(IEnumerable<QuizQuestion> questions)
        {
            _context.QuizQuestions.AddRange(questions);
            await _context.SaveChangesAsync();
        }
    }
}
