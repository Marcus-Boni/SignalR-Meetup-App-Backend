// Services/IAuthService.cs
using CarTrackingApi.Dtos;

namespace CarTrackingApi.Services
{
    /// <summary>
    /// Interface para o servi�o de autentica��o
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Autentica um usu�rio e retorna um token JWT
        /// </summary>
        /// <param name="request">Credenciais do usu�rio</param>
        /// <returns>Token JWT ou null se as credenciais forem inv�lidas</returns>
        Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request);

        /// <summary>
        /// Valida se um usu�rio existe (para demo)
        /// </summary>
        /// <param name="username">Nome do usu�rio</param>
        /// <returns>True se o usu�rio existe</returns>
        bool ValidateUser(string username);
    }
}
