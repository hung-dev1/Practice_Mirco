using QuizService.DTOs;
using QuizService.Entity;
using QuizService.Repository.Interface;
using QuizService.Service.Interface;
using QuizService.ServiceClients;

namespace QuizService.Service.Implement
{
    public class QuizServiceImpl : IQuizService
    {
        private readonly IQuizRepository _repository;
        private readonly IQuestionClient _questionClient;

        public QuizServiceImpl(IQuizRepository repository, IQuestionClient questionClient)
        {
            _repository = repository;
            _questionClient = questionClient;
        }

        public async Task<IEnumerable<QuizResponse>> GetAllAsync()
            => (await _repository.GetAllAsync()).Select(MapToResponse);

        public async Task<QuizResponse?> GetByIdAsync(long id)
        {
            var quiz = await _repository.GetByIdAsync(id);
            return quiz is null ? null : MapToResponse(quiz);
        }

        public async Task<QuizResponse> CreateAsync(CreateQuizRequest request, long createdBy)
        {
            var quiz = new Quiz
            {
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                Duration = request.Duration,
                IsActive = request.IsActive,
                CreatedBy = createdBy
            };
            return MapToResponse(await _repository.CreateAsync(quiz));
        }

        public async Task<QuizResponse> UpdateAsync(long id, UpdateQuizRequest request)
        {
            var quiz = await FindQuizAsync(id);
            quiz.Title = request.Title.Trim();
            quiz.Description = request.Description?.Trim();
            quiz.Duration = request.Duration;
            quiz.IsActive = request.IsActive;
            return MapToResponse(await _repository.UpdateAsync(quiz));
        }

        public async Task DeleteAsync(long id)
            => await _repository.DeleteAsync(await FindQuizAsync(id));

        public async Task<QuestionResponse> AddQuestionAsync(
            long quizId, long questionId, string authorization)
        {
            if (questionId <= 0)
                throw new ArgumentException("QuestionId must be greater than zero.");

            var quiz = await FindQuizAsync(quizId);
            if (quiz.QuizQuestions.Any(q => q.QuestionId == questionId))
                throw new InvalidOperationException("Question is already in this quiz.");

            var question = await _questionClient.GetByIdAsync(questionId, authorization);
            await SaveQuestionsAsync(quiz, new[] { question });
            return question;
        }

        public async Task<List<QuestionResponse>> AddRandomQuestionsAsync(
            long quizId, string authorization, int count = 10)
        {
            if (count <= 0 || count > 100)
                throw new ArgumentException("count must be between 1 and 100.");

            var quiz = await FindQuizAsync(quizId);
            var questions = await _questionClient.GetRandomAsync(authorization, count);
            var existingIds = quiz.QuizQuestions.Select(q => q.QuestionId).ToHashSet();
            var added = questions.DistinctBy(q => q.Id)
                .Where(q => !existingIds.Contains(q.Id)).ToList();

            if (added.Count > 0)
                await SaveQuestionsAsync(quiz, added);

            return added;
        }

        public async Task<SubmitQuizResponse> SubmitQuizAsync(
            long quizId, SubmitQuizRequest request, string authorization)
        {
            if (quizId <= 0)
                throw new ArgumentException("QuizId must be greater than zero.");
            if (request?.Answers is null)
                throw new ArgumentException("Answers are required.");

            var quiz = await FindQuizAsync(quizId);
            if (!quiz.IsActive)
                throw new InvalidOperationException("Quiz is not active.");

            var questionIds = quiz.QuizQuestions.Select(q => q.QuestionId).ToHashSet();
            if (questionIds.Count == 0)
                throw new InvalidOperationException("Quiz has no questions.");

            var answers = new Dictionary<long, string>();
            foreach (var answer in request.Answers)
            {
                if (answer is null || answer.QuestionId <= 0 || string.IsNullOrEmpty(answer.Answer)
                    || answer.Answer.Length != 1 || !"ABCD".Contains(char.ToUpperInvariant(answer.Answer[0])))
                    throw new ArgumentException("Each answer must have a positive QuestionId and an answer A, B, C or D.");
                if (!questionIds.Contains(answer.QuestionId))
                    throw new ArgumentException($"Question {answer.QuestionId} does not belong to this quiz.");
                if (!answers.TryAdd(answer.QuestionId, answer.Answer))
                    throw new ArgumentException($"Question {answer.QuestionId} was answered more than once.");
            }

            var correctAnswers = 0;
            foreach (var answer in answers)
            {
                var question = await _questionClient.GetByIdAsync(answer.Key, authorization);
                if (string.Equals(answer.Value, question.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
                    correctAnswers++;
            }

            return new SubmitQuizResponse
            {
                QuizId = quiz.Id,
                TotalQuestions = questionIds.Count,
                AnsweredQuestions = answers.Count,
                CorrectAnswers = correctAnswers,
                Score = Math.Round(correctAnswers * 10m / questionIds.Count, 2)
            };
        }

        private async Task<Quiz> FindQuizAsync(long id)
            => await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Quiz with id {id} not found.");

        private Task SaveQuestionsAsync(Quiz quiz, IEnumerable<QuestionResponse> questions)
        {
            var order = quiz.QuizQuestions.Select(q => q.QuestionOrder).DefaultIfEmpty(0).Max();
            var links = questions.Select(q => new QuizQuestion
            {
                QuizId = quiz.Id,
                QuestionId = q.Id,
                QuestionOrder = ++order
            }).ToList();
            return _repository.AddQuestionsAsync(links);
        }

        private static QuizResponse MapToResponse(Quiz quiz) => new()
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            Duration = quiz.Duration,
            CreatedBy = quiz.CreatedBy,
            CreatedAt = quiz.CreatedAt,
            IsActive = quiz.IsActive,
            Questions = quiz.QuizQuestions.OrderBy(q => q.QuestionOrder)
                .Select(q => new QuizQuestionResponse
                {
                    QuestionId = q.QuestionId,
                    QuestionOrder = q.QuestionOrder
                }).ToList()
        };
    }
}
