// Dtos/LoginResponseDto.cs
namespace CarTrackingApi.Dtos
{
    /// <summary>
    /// DTO para resposta de login bem-sucedido
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// Token JWT para autentica��o
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Nome do usu�rio autenticado
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Data de expira��o do token (UTC)
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Tipo do token (sempre "Bearer")
        /// </summary>
        public string TokenType { get; set; } = "Bearer";
    }
}
