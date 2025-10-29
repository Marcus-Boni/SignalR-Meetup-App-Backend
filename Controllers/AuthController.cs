// Controllers/AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarTrackingApi.Dtos;
using CarTrackingApi.Services;

namespace CarTrackingApi.Controllers
{
    /// <summary>
    /// Controller respons�vel pela autentica��o de usu�rios
    /// </summary>
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
        /// Endpoint de login - Retorna um token JWT
        /// </summary>
        /// <param name="request">Credenciais do usu�rio</param>
        /// <returns>Token JWT se as credenciais forem v�lidas</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Requisi��o de login inv�lida");
                return BadRequest(ModelState);
            }

            var result = await _authService.AuthenticateAsync(request);

            if (result == null)
            {
                _logger.LogWarning("Login falhou para usu�rio: {Username}", request.Username);
                return Unauthorized(new { message = "Usu�rio ou senha inv�lidos" });
            }

            _logger.LogInformation("Login bem-sucedido para usu�rio: {Username}", request.Username);
            return Ok(result);
        }

        /// <summary>
        /// Endpoint de valida��o de token - Verifica se o token ainda � v�lido
        /// </summary>
        /// <returns>Informa��es do usu�rio autenticado</returns>
        [HttpGet("validate")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult ValidateToken()
        {
            var username = User.Identity?.Name;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("Token validado para usu�rio: {Username}", username);

            return Ok(new
            {
                isValid = true,
                username = username,
                userId = userId,
                claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }

        /// <summary>
        /// Endpoint de health check para autentica��o
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                service = "AuthController",
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                demoUsers = new[]
                {
                    "admin (admin123)",
                    "usuario1 (senha123)",
                    "usuario2 (senha456)",
                    "teste (teste123)"
                }
            });
        }
    }
}
