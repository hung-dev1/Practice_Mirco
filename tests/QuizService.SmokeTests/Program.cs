using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using QuizService.DTOs;
using QuizService.Entity;
using QuizService.Repository.Interface;
using QuizService.Service.Implement;
using QuizService.ServiceClients;
using Refit;

// Run the actual Refit client against a fake HTTP transport; no external services required.
var transport = new QuestionTransport();
var services = new ServiceCollection();
services.AddRefitGeneratedClient<IQuestionClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://questionservice:8080"))
    .ConfigurePrimaryHttpMessageHandler(() => transport);
using var provider = services.BuildServiceProvider();
var repository = new MemoryQuizRepository();
var service = new QuizServiceImpl(repository, provider.GetRequiredService<IQuestionClient>());

var created = await service.CreateAsync(new CreateQuizRequest
{
    Title = " Example quiz ", Description = " Description ", Duration = 30
}, 7);
Check(created.Title == "Example quiz" && created.Description == "Description" && created.CreatedBy == 7);
Check(created.Questions.Count == 0 && created.IsActive && created.CreatedAt.Kind == DateTimeKind.Utc);
Check((await service.GetAllAsync()).Single().Id == created.Id);
Check(await service.GetByIdAsync(999) is null);

var updated = await service.UpdateAsync(created.Id, new UpdateQuizRequest
{
    Title = " Updated ", Duration = 45, IsActive = false
});
Check(updated.Title == "Updated" && updated.Duration == 45 && !updated.IsActive);
Check(updated.CreatedBy == 7 && updated.CreatedAt == created.CreatedAt);

var question = await service.AddQuestionAsync(created.Id, 42, "Bearer first");
Check(question.Id == 42 && question.CorrectAnswer == "A");
Check(transport.Path == "/api/questions/42" && transport.Authorization == "Bearer first");
var calls = transport.Calls;
await Expect<InvalidOperationException>(() => service.AddQuestionAsync(created.Id, 42, "Bearer first"));
Check(transport.Calls == calls);

var added = await service.AddRandomQuestionsAsync(created.Id, "Bearer second", 5);
Check(transport.Path == "/api/questions/random?count=5" && transport.Authorization == "Bearer second");
Check(added.Select(q => q.Id).SequenceEqual(new long[] { 43, 44 }));
var quiz = (await service.GetByIdAsync(created.Id))!;
Check(quiz.Questions.Select(q => q.QuestionId).SequenceEqual(new long[] { 42, 43, 44 }));
Check(quiz.Questions.Select(q => q.QuestionOrder).SequenceEqual(new[] { 1, 2, 3 }));
var saves = repository.QuestionSaves;
Check((await service.AddRandomQuestionsAsync(created.Id, "Bearer third")).Count == 0);
Check(transport.Path == "/api/questions/random?count=10" && transport.Authorization == "Bearer third");
Check(repository.QuestionSaves == saves);

await Expect<ArgumentException>(() => service.AddRandomQuestionsAsync(created.Id, "Bearer first", 0));
await Expect<ArgumentException>(() => service.AddRandomQuestionsAsync(created.Id, "Bearer first", 101));
await Expect<ArgumentException>(() => service.AddQuestionAsync(created.Id, 0, "Bearer first"));
await Expect<KeyNotFoundException>(() => service.AddQuestionAsync(999, 42, "Bearer first"));
await Expect<KeyNotFoundException>(() => service.UpdateAsync(999, new UpdateQuizRequest()));
await Expect<KeyNotFoundException>(() => service.DeleteAsync(999));
try
{
    await service.AddQuestionAsync(created.Id, 999, "Bearer first");
    throw new Exception("Expected a downstream 404.");
}
catch (ApiException ex)
{
    Check(ex.StatusCode == HttpStatusCode.NotFound);
    Check(repository.QuestionSaves == saves);
}
transport.FailRandom = true;
await Expect<ApiException>(() => service.AddRandomQuestionsAsync(created.Id, "Bearer first", 5));
Check(repository.QuestionSaves == saves);
var invalidRequest = new CreateQuizRequest { Title = " ", Duration = 0 };
Check(!Validator.TryValidateObject(invalidRequest,
    new ValidationContext(invalidRequest), new List<ValidationResult>(), true));

var submission = new SubmitQuizRequest
{
    Answers = new()
    {
        new() { QuestionId = 42, Answer = "a" },
        new() { QuestionId = 43, Answer = "B" }
    }
};
await Expect<InvalidOperationException>(() => service.SubmitQuizAsync(created.Id, submission, "Bearer student"));
await service.UpdateAsync(created.Id, new UpdateQuizRequest { Title = "Active", Duration = 30, IsActive = true });
var result = await service.SubmitQuizAsync(created.Id, submission, "Bearer student");
Check(result.QuizId == created.Id && result.TotalQuestions == 3 && result.AnsweredQuestions == 2);
Check(result.CorrectAnswers == 1 && result.Score == 3.33m);
Check(transport.Authorization == "Bearer student");
Check(repository.QuestionSaves == saves);
var emptyResult = await service.SubmitQuizAsync(created.Id, new SubmitQuizRequest(), "Bearer student");
Check(emptyResult.Score == 0 && emptyResult.AnsweredQuestions == 0);
var fullResult = await service.SubmitQuizAsync(created.Id, new SubmitQuizRequest
{
    Answers = new() { new() { QuestionId = 42, Answer = "A" }, new() { QuestionId = 43, Answer = "A" }, new() { QuestionId = 44, Answer = "A" } }
}, "Bearer student");
Check(fullResult.Score == 10 && fullResult.CorrectAnswers == 3);
calls = transport.Calls;
await Expect<ArgumentException>(() => service.SubmitQuizAsync(created.Id, new SubmitQuizRequest
{
    Answers = new() { new() { QuestionId = 42, Answer = "A" }, new() { QuestionId = 42, Answer = "B" } }
}, "Bearer student"));
await Expect<ArgumentException>(() => service.SubmitQuizAsync(created.Id, new SubmitQuizRequest
{
    Answers = new() { new() { QuestionId = 999, Answer = "A" } }
}, "Bearer student"));
await Expect<ArgumentException>(() => service.SubmitQuizAsync(created.Id, new SubmitQuizRequest
{
    Answers = new() { new() { QuestionId = 42, Answer = "E" } }
}, "Bearer student"));
await Expect<ArgumentException>(() => service.SubmitQuizAsync(created.Id, new SubmitQuizRequest { Answers = null! }, "Bearer student"));
await Expect<ArgumentException>(() => service.SubmitQuizAsync(0, submission, "Bearer student"));
await Expect<KeyNotFoundException>(() => service.SubmitQuizAsync(999, submission, "Bearer student"));
var emptyQuiz = await service.CreateAsync(new CreateQuizRequest { Title = "Empty", Duration = 30 }, 7);
await Expect<InvalidOperationException>(() => service.SubmitQuizAsync(emptyQuiz.Id, new SubmitQuizRequest(), "Bearer student"));
Check(transport.Calls == calls);
transport.FailById = true;
await Expect<ApiException>(() => service.SubmitQuizAsync(created.Id, submission, "Bearer student"));
transport.FailById = false;

await service.DeleteAsync(created.Id);
Check(await service.GetByIdAsync(created.Id) is null && repository.Links.Count == 0);
Console.WriteLine("PASS: Quiz CRUD, Refit routes/headers, question ordering, validation, downstream failures, submit scoring (partial/empty/full), duplicate/foreign/invalid answers, inactive/empty quizzes, delete links.");

static void Check(bool value)
{
    if (!value) throw new Exception("Assertion failed.");
}

static async Task Expect<T>(Func<Task> action) where T : Exception
{
    try { await action(); }
    catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}.");
}

sealed class MemoryQuizRepository : IQuizRepository
{
    private readonly List<Quiz> _quizzes = new();
    public List<QuizQuestion> Links { get; } = new();
    public int QuestionSaves { get; private set; }
    public Task<IEnumerable<Quiz>> GetAllAsync() => Task.FromResult<IEnumerable<Quiz>>(_quizzes.ToList());
    public Task<Quiz?> GetByIdAsync(long id) => Task.FromResult(_quizzes.SingleOrDefault(q => q.Id == id));
    public Task<Quiz> CreateAsync(Quiz quiz)
    {
        quiz.Id = _quizzes.Count + 1;
        _quizzes.Add(quiz);
        return Task.FromResult(quiz);
    }
    public Task<Quiz> UpdateAsync(Quiz quiz) => Task.FromResult(quiz);
    public Task DeleteAsync(Quiz quiz)
    {
        _quizzes.Remove(quiz);
        Links.RemoveAll(q => q.QuizId == quiz.Id);
        return Task.CompletedTask;
    }
    public Task AddQuestionsAsync(IEnumerable<QuizQuestion> questions)
    {
        QuestionSaves++;
        foreach (var question in questions)
        {
            Links.Add(question);
            _quizzes.Single(q => q.Id == question.QuizId).QuizQuestions.Add(question);
        }
        return Task.CompletedTask;
    }
}

sealed class QuestionTransport : HttpMessageHandler
{
    public string? Path { get; private set; }
    public string? Authorization { get; private set; }
    public int Calls { get; private set; }
    public bool FailRandom { get; set; }
    public bool FailById { get; set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        Path = request.RequestUri!.PathAndQuery;
        Authorization = request.Headers.Authorization?.ToString();
        var json = """{"id":42,"title":"Example","optionA":"A","optionB":"B","optionC":"C","optionD":"D","correctAnswer":"A"}""";
        var random = Path.StartsWith("/api/questions/random");
        if (random)
            json = "[" + json + "," + json.Replace("42", "43") + "," + json.Replace("42", "43") + "," + json.Replace("42", "44") + "]";
        var status = Path.EndsWith("/999") ? HttpStatusCode.NotFound
            : (random && FailRandom) || (!random && FailById) ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK;
        return Task.FromResult(new HttpResponseMessage(status)
        {
            RequestMessage = request,
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
    }
}
