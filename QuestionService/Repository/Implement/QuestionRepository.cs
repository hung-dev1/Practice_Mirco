using Microsoft.EntityFrameworkCore;
using QuestionService.Data;
using QuestionService.Entity;
using QuestionService.Repository.Interface;

namespace QuestionService.Repository.Implement
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly AppDbContext _context;

        public QuestionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Question>> GetAllAsync()
            => await _context.Questions
                             .OrderBy(q => q.Id)
                             .ToListAsync();

        public async Task<Question?> GetByIdAsync(long id)
            => await _context.Questions
                             .FirstOrDefaultAsync(q => q.Id == id);

        public async Task<Question> CreateAsync(Question question)
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task<Question> UpdateAsync(Question question)
        {
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task DeleteAsync(Question question)
        {
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByIdAsync(long id)
            => await _context.Questions.AnyAsync(q => q.Id == id);

        public async Task<IEnumerable<Question>> GetRandomAsync(int count)
            => await _context.Questions
                             .OrderBy(q => EF.Functions.Random())
                             .Take(count)
                             .ToListAsync();
    }
}
