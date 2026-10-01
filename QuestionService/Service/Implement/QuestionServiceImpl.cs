using QuestionService.DTOs;
using QuestionService.Entity;
using QuestionService.Repository.Interface;
using QuestionService.Service.Interface;

namespace QuestionService.Service.Implement
{
    public class QuestionServiceImpl : IQuestionService
    {
        private readonly IQuestionRepository _repository;

        public QuestionServiceImpl(IQuestionRepository repository)
        {
            _repository = repository;
        }

        // ── GET ALL ───────────────────────────────────────────────────────────
        public async Task<IEnumerable<QuestionResponse>> GetAllAsync()
        {
            var questions = await _repository.GetAllAsync();
            return questions.Select(MapToResponse);
        }

        // ── GET BY ID ─────────────────────────────────────────────────────────
        public async Task<QuestionResponse?> GetByIdAsync(long id)
        {
            var question = await _repository.GetByIdAsync(id);
            return question is null ? null : MapToResponse(question);
        }

        // ── CREATE ────────────────────────────────────────────────────────────
        public async Task<QuestionResponse> CreateAsync(CreateQuestionRequest request)
        {
            var question = new Question
            {
                Title         = request.Title.Trim(),
                OptionA       = request.OptionA.Trim(),
                OptionB       = request.OptionB.Trim(),
                OptionC       = request.OptionC.Trim(),
                OptionD       = request.OptionD.Trim(),
                CorrectAnswer = request.CorrectAnswer.ToUpper()
            };

            var created = await _repository.CreateAsync(question);
            return MapToResponse(created);
        }

        // ── UPDATE ────────────────────────────────────────────────────────────
        public async Task<QuestionResponse> UpdateAsync(long id, UpdateQuestionRequest request)
        {
            var question = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Question with id {id} not found.");

            question.Title         = request.Title.Trim();
            question.OptionA       = request.OptionA.Trim();
            question.OptionB       = request.OptionB.Trim();
            question.OptionC       = request.OptionC.Trim();
            question.OptionD       = request.OptionD.Trim();
            question.CorrectAnswer = request.CorrectAnswer.ToUpper();

            var updated = await _repository.UpdateAsync(question);
            return MapToResponse(updated);
        }

        // ── DELETE ────────────────────────────────────────────────────────────
        public async Task DeleteAsync(long id)
        {
            var question = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Question with id {id} not found.");

            await _repository.DeleteAsync(question);
        }

        // ── RANDOM ────────────────────────────────────────────────────────────
        public async Task<IEnumerable<QuestionResponse>> GetRandomAsync(int count = 10)
        {
            var questions = await _repository.GetRandomAsync(count);
            return questions.Select(MapToResponse);
        }

        // ── MAPPER ────────────────────────────────────────────────────────────
        private static QuestionResponse MapToResponse(Question q) => new()
        {
            Id            = q.Id,
            Title         = q.Title,
            OptionA       = q.OptionA,
            OptionB       = q.OptionB,
            OptionC       = q.OptionC,
            OptionD       = q.OptionD,
            CorrectAnswer = q.CorrectAnswer
        };
    }
}
