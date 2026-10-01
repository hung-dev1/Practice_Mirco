using AuthService.DTOs;
using AuthService.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
            
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Đăng ký tài khoản mới
        /// </summary>
        /// <param name="request">Thông tin đăng ký</param>
        /// <returns>Thông tin user vừa tạo</returns>
        [HttpPost("signup")]
        [ProducesResponseType(typeof(SignUpResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                SignUpResponse response = await _authService.SignUpAsync(request);
                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("SignUp conflict: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SignUp unexpected error");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Đăng nhập và nhận AccessToken + RefreshToken
        /// </summary>
        /// <param name="request">Thông tin đăng nhập</param>
        /// <returns>AccessToken và RefreshToken</returns>
        [HttpPost("signin")]
        [ProducesResponseType(typeof(SignInResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                SignInResponse response = await _authService.SignInAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("SignIn unauthorized: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SignIn unexpected error");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An unexpected error occurred." });
            }
        }
    }
}
