using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizService.DTOs;
using QuizService.Service.Interface;

namespace QuizService.Controllers
{
    [ApiController]
    [Route("api/quizzes")]
    [Authorize]
    [TypeFilter(typeof(QuizExceptionFilter))]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _service;

        public QuizController(IQuizService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizResponse>>> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpGet("{id:long}")]
        public async Task<ActionResult<QuizResponse>> GetById(long id)
        {
            var quiz = await _service.GetByIdAsync(id);
            return quiz is null
                ? NotFound(new { message = $"Quiz with id {id} not found." })
                : Ok(quiz);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<ActionResult<QuizResponse>> Create(CreateQuizRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!long.TryParse(userId, out var createdBy) || createdBy <= 0)
                return Unauthorized(new { message = "Token does not contain a valid user id." });

            var quiz = await _service.CreateAsync(request, createdBy);
            return CreatedAtAction(nameof(GetById), new { id = quiz.Id }, quiz);
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<ActionResult<QuizResponse>> Update(long id, UpdateQuizRequest request)
            => Ok(await _service.UpdateAsync(id, request));

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<IActionResult> Delete(long id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{id:long}/questions")]
        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<ActionResult<QuestionResponse>> AddQuestion(long id, AddQuestionRequest request)
            => Ok(await _service.AddQuestionAsync(
                id, request.QuestionId, Request.Headers.Authorization.ToString()));

        [HttpPost("{id:long}/questions/random")]
        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<ActionResult<List<QuestionResponse>>> AddRandomQuestions(
            long id, [FromQuery, Range(1, 100)] int count = 10)
            => Ok(await _service.AddRandomQuestionsAsync(
                id, Request.Headers.Authorization.ToString(), count));
    }
}
