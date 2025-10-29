// Dtos/ChatMessageDto.cs
namespace CarTrackingApi.Dtos
{
    /// <summary>
    /// DTO para mensagens de chat em tempo real
    /// </summary>
    public class ChatMessageDto
    {
        /// <summary>
        /// Nome ou identificador do usuário que enviou a mensagem
        /// </summary>
        public string User { get; set; } = string.Empty;

        /// <summary>
        /// Conteúdo da mensagem
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Nome da sala/grupo onde a mensagem foi enviada
        /// </summary>
        public string RoomName { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp da mensagem (UTC)
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
