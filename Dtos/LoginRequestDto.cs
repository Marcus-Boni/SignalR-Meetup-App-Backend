// Dtos/LoginRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace CarTrackingApi.Dtos
{
    /// <summary>
    /// DTO para requisição de login
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// Nome de usuário ou email
        /// </summary>
        [Required(ErrorMessage = "Username é obrigatório")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Senha do usuário
        /// </summary>
        [Required(ErrorMessage = "Password é obrigatório")]
        public string Password { get; set; } = string.Empty;
    }
}
