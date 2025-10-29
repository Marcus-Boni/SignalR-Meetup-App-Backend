// Hubs/ChatHub.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using CarTrackingApi.Dtos;

namespace CarTrackingApi.Hubs
{
    /// <summary>
    /// Hub especializado para chat multi-sala em tempo real.
    /// Este é um "Thin Hub" - contém apenas lógica de comunicação e gerenciamento de grupos.
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Adiciona o cliente à sala especificada.
        /// Client-Initiated: O cliente chama este método para entrar em uma sala.
        /// </summary>
        /// <param name="roomName">Nome da sala para entrar</param>
        public async Task JoinRoom(string roomName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roomName))
                {
                    _logger.LogWarning("Tentativa de entrar em sala com nome inválido. ConnectionId: {ConnectionId}", Context.ConnectionId);
                    throw new HubException("Nome da sala não pode ser vazio");
                }

                var userName = Context.UserIdentifier ?? Context.User?.Identity?.Name ?? "Anonymous";
                
                await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                
                _logger.LogInformation("Usuário {UserName} (ConnectionId: {ConnectionId}) entrou na sala {RoomName}", 
                    userName, Context.ConnectionId, roomName);

                // Notifica os outros membros da sala que um novo usuário entrou
                await Clients.Group(roomName).SendAsync("UserJoined", new
                {
                    User = userName,
                    RoomName = roomName,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao entrar na sala {RoomName}. ConnectionId: {ConnectionId}", roomName, Context.ConnectionId);
                throw new HubException($"Erro ao entrar na sala: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove o cliente da sala especificada.
        /// Client-Initiated: O cliente chama este método para sair de uma sala.
        /// </summary>
        /// <param name="roomName">Nome da sala para sair</param>
        public async Task LeaveRoom(string roomName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roomName))
                {
                    _logger.LogWarning("Tentativa de sair de sala com nome inválido. ConnectionId: {ConnectionId}", Context.ConnectionId);
                    throw new HubException("Nome da sala não pode ser vazio");
                }

                var userName = Context.UserIdentifier ?? Context.User?.Identity?.Name ?? "Anonymous";
                
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
                
                _logger.LogInformation("Usuário {UserName} (ConnectionId: {ConnectionId}) saiu da sala {RoomName}", 
                    userName, Context.ConnectionId, roomName);

                // Notifica os membros restantes que um usuário saiu
                await Clients.Group(roomName).SendAsync("UserLeft", new
                {
                    User = userName,
                    RoomName = roomName,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao sair da sala {RoomName}. ConnectionId: {ConnectionId}", roomName, Context.ConnectionId);
                throw new HubException($"Erro ao sair da sala: {ex.Message}");
            }
        }

        /// <summary>
        /// Envia uma mensagem para todos os membros de uma sala específica.
        /// Client-Initiated: O cliente invoca este método para enviar mensagens.
        /// </summary>
        /// <param name="roomName">Nome da sala onde enviar a mensagem</param>
        /// <param name="message">Conteúdo da mensagem</param>
        public async Task SendMessage(string roomName, string message)
        {
            try
            {
                _logger.LogInformation("?? SendMessage chamado. RoomName: '{RoomName}', Message: '{Message}', ConnectionId: {ConnectionId}", 
                    roomName, message, Context.ConnectionId);

                if (string.IsNullOrWhiteSpace(roomName))
                {
                    _logger.LogWarning("? Tentativa de enviar mensagem para sala inválida. ConnectionId: {ConnectionId}", Context.ConnectionId);
                    throw new HubException("Nome da sala não pode ser vazio");
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    _logger.LogWarning("? Tentativa de enviar mensagem vazia. ConnectionId: {ConnectionId}", Context.ConnectionId);
                    throw new HubException("Mensagem não pode ser vazia");
                }

                // Obtém o identificador do usuário do contexto de autenticação
                var userName = Context.UserIdentifier ?? Context.User?.Identity?.Name ?? "Anonymous";

                var chatMessage = new ChatMessageDto
                {
                    User = userName,
                    Message = message.Trim(),
                    RoomName = roomName,
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("? Mensagem processada e será enviada por {UserName} na sala {RoomName}", userName, roomName);

                // Envia a mensagem apenas para os membros do grupo (sala)
                await Clients.Group(roomName).SendAsync("ReceiveMessage", chatMessage);

                _logger.LogInformation("? Mensagem enviada com sucesso para o grupo {RoomName}", roomName);
            }
            catch (HubException)
            {
                // Re-lança HubExceptions (erros que já foram tratados)
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Erro inesperado ao enviar mensagem. RoomName: {RoomName}, ConnectionId: {ConnectionId}", 
                    roomName, Context.ConnectionId);
                throw new HubException($"Erro ao enviar mensagem: {ex.Message}");
            }
        }

        /// <summary>
        /// Chamado quando um cliente se conecta ao Hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            try
            {
                var userName = Context.UserIdentifier ?? Context.User?.Identity?.Name ?? "Anonymous";
                _logger.LogInformation("? Cliente conectado ao ChatHub. User: {UserName}, ConnectionId: {ConnectionId}", 
                    userName, Context.ConnectionId);
                
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Erro ao conectar cliente. ConnectionId: {ConnectionId}", Context.ConnectionId);
                throw;
            }
        }

        /// <summary>
        /// Chamado quando um cliente se desconecta do Hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var userName = Context.UserIdentifier ?? Context.User?.Identity?.Name ?? "Anonymous";
                
                if (exception != null)
                {
                    _logger.LogError(exception, "? Cliente desconectado do ChatHub com erro. User: {UserName}, ConnectionId: {ConnectionId}", 
                        userName, Context.ConnectionId);
                }
                else
                {
                    _logger.LogInformation("?? Cliente desconectado do ChatHub. User: {UserName}, ConnectionId: {ConnectionId}", 
                        userName, Context.ConnectionId);
                }
                
                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Erro ao desconectar cliente. ConnectionId: {ConnectionId}", Context.ConnectionId);
                throw;
            }
        }
    }
}
