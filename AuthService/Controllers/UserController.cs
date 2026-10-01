using AuthService.DTOs;
using AuthService.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/user")]
    [Authorize(Roles = "ADMIN,TEACHER")]   // chỉ ADMIN và TEACHER mới vào được
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // ── Lấy role của người đang gọi từ JWT claim ─────────────────────────
        private string CallerRole =>
            User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        // ── GET ALL ───────────────────────────────────────────────────────────
        /// <summary>Lấy danh sách tất cả user</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var users = await _userService.GetAllAsync(CallerRole);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll unexpected error");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // ── GET BY ID ─────────────────────────────────────────────────────────
        /// <summary>Lấy user theo id</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id, CallerRole);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetById unexpected error");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // CREATE 
        /// Tạo user mới.
        /// ADMIN: có thể gán RoleId tùy ý (mặc định STUDENT nếu không truyền).
        /// TEACHER: luôn tạo tài khoản STUDENT, RoleId bị bỏ qua.
        [HttpPost]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _userService.CreateAsync(request, CallerRole);
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Create conflict: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create unexpected error");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // ── UPDATE ────────────────────────────────────────────────────────────
        /// <summary>
        /// Cập nhật thông tin user.
        /// Chỉ ADMIN mới được đổi RoleId.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _userService.UpdateAsync(id, request, CallerRole);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Update conflict: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update unexpected error");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        // ── DELETE ────────────────────────────────────────────────────────────
        /// <summary>Xóa user theo id</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _userService.DeleteAsync(id, CallerRole);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete unexpected error");
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
