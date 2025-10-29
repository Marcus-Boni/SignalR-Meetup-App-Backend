// Services/IAuthService.cs
using CarTrackingApi.Dtos;

namespace CarTrackingApi.Services
{
    /// <summary>
    /// Interface para o serviço de autenticação
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Autentica um usuário e retorna um token JWT
        /// </summary>
        /// <param name="request">Credenciais do usuário</param>
        /// <returns>Token JWT ou null se as credenciais forem inválidas</returns>
        Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request);

        /// <summary>
        /// Valida se um usuário existe (para demo)
        /// </summary>
        /// <param name="username">Nome do usuário</param>
        /// <returns>True se o usuário existe</returns>
        bool ValidateUser(string username);
    }
}
