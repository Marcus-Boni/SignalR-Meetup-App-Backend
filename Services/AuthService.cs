// Services/AuthService.cs
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CarTrackingApi.Dtos;

namespace CarTrackingApi.Services
{
    /// <summary>
    /// Serviço de autenticação que gera tokens JWT
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        // DEMO: Usuários em memória (em produção, use um banco de dados)
        private readonly Dictionary<string, string> _users = new()
        {
            { "admin", "admin123" },
            { "usuario1", "senha123" },
            { "usuario2", "senha456" },
            { "teste", "teste123" }
        };

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Autentica o usuário e retorna um token JWT
        /// </summary>
        public async Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request)
        {
            // DEMO: Validação simples (em produção, use hash de senha + banco de dados)
            if (!_users.TryGetValue(request.Username, out var expectedPassword) || 
                expectedPassword != request.Password)
            {
                _logger.LogWarning("Tentativa de login falhou para usuário: {Username}", request.Username);
                return null;
            }

            _logger.LogInformation("Usuário autenticado com sucesso: {Username}", request.Username);

            // Gera o token JWT
            var token = await Task.Run(() => GenerateJwtToken(request.Username));
            
            return new LoginResponseDto
            {
                Token = token.Token,
                Username = request.Username,
                ExpiresAt = token.ExpiresAt,
                TokenType = "Bearer"
            };
        }

        /// <summary>
        /// Valida se um usuário existe (para uso interno)
        /// </summary>
        public bool ValidateUser(string username)
        {
            return _users.ContainsKey(username);
        }

        /// <summary>
        /// Gera um token JWT para o usuário
        /// </summary>
        private (string Token, DateTime ExpiresAt) GenerateJwtToken(string username)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? 
                "SuperSecretKeyForDevelopmentOnly_ChangeInProduction_MinimumLength32Characters!";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "CarTrackingApi";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "CarTrackingApiClients";
            var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

            // Claims do token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, username),
                // Adicione mais claims conforme necessário (roles, permissions, etc.)
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("Token JWT gerado para {Username}. Expira em: {ExpiresAt}", 
                username, expiresAt);

            return (tokenString, expiresAt);
        }
    }
}
