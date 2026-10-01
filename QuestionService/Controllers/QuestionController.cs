using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionService.DTOs;
using QuestionService.Service.Interface;

namespace QuestionService.Controllers
{
    [ApiController]
    [Route("api/questions")]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _service;

        public QuestionController(IQuestionService service)
        {
            _service = service;
        }

        // ── GET RANDOM ────────────────────────────────────────────────────────
        // Mặc định 10 câu, có thể truyền ?count=5 để lấy số khác
        [HttpGet("random")]
        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<ActionResult<IEnumerable<QuestionResponse>>> GetRandom(
            [FromQuery] int count = 10)
        {
            if (count <= 0 || count > 100)
                return BadRequest(new { message = "count must be between 1 and 100." });

            var questions = await _service.GetRandomAsync(count);
            return Ok(questions);
        }

        // ── GET ALL ───────────────────────────────────────────────────────────
        // Tất cả user đã đăng nhập đều xem được
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<QuestionResponse>>> GetAll()
        {
            var questions = await _service.GetAllAsync();
            return Ok(questions);
        }

        // ── GET BY ID ─────────────────────────────────────────────────────────
        [HttpGet("{id:long}")]
        [Authorize]
        public async Task<ActionResult<QuestionResponse>> GetById(long id)
        {
            var question = await _service.GetByIdAsync(id);
            if (question is null)
                return NotFound(new { message = $"Question with id {id} not found." });

            return Ok(question);
        }

        // ── CREATE ────────────────────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<ActionResult<QuestionResponse>> Create(
            [FromBody] CreateQuestionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ── UPDATE ────────────────────────────────────────────────────────────
        [HttpPut("{id:long}")]
        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<ActionResult<QuestionResponse>> Update(
            long id,
            [FromBody] UpdateQuestionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _service.UpdateAsync(id, request);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ── DELETE ────────────────────────────────────────────────────────────
        [HttpDelete("{id:long}")]
        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
