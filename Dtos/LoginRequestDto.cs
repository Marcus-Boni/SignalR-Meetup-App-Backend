// Dtos/LoginRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace CarTrackingApi.Dtos
{
    /// <summary>
    /// DTO para requisi��o de login
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// Nome de usu�rio ou email
        /// </summary>
        [Required(ErrorMessage = "Username � obrigat�rio")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Senha do usu�rio
        /// </summary>
        [Required(ErrorMessage = "Password � obrigat�rio")]
        public string Password { get; set; } = string.Empty;
    }
}
