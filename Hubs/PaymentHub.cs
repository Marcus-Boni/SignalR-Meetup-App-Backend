// Hubs/PaymentHub.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CarTrackingApi.Hubs
{
    /// <summary>
    /// Hub especializado para notificações de status de pagamento.
    /// Este é um "Thin Hub" - apenas gerencia grupos e subscrições.
    /// A lógica de pagamento está no IPaymentService.
    /// </summary>
    [Authorize]
    public class PaymentHub : Hub
    {
        private readonly ILogger<PaymentHub> _logger;

        public PaymentHub(ILogger<PaymentHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Subscreve o cliente para receber atualizações de status de um pagamento específico.
        /// Client-Initiated: O cliente se inscreve após iniciar um pagamento via HTTP.
        /// </summary>
        /// <param name="orderId">Identificador do pedido para monitorar</param>
        public async Task SubscribeToPaymentStatus(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                _logger.LogWarning("Tentativa de subscrever a pagamento com OrderId inválido. ConnectionId: {ConnectionId}", 
                    Context.ConnectionId);
                return;
            }

            // Obtém o identificador único do usuário autenticado
            var userId = Context.UserIdentifier ?? Context.User?.Identity?.Name;

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Tentativa de subscrever a pagamento sem usuário autenticado. ConnectionId: {ConnectionId}", 
                    Context.ConnectionId);
                return;
            }

            // Cria um nome de grupo SEGURO e PRIVADO para este usuário e pedido
            // Apenas este usuário receberá as atualizações deste pagamento
            var groupName = $"payment-status-{userId}-{orderId}";

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation(
                "Usuário {UserId} (ConnectionId: {ConnectionId}) subscrito para atualizações do pagamento {OrderId}. Grupo: {GroupName}", 
                userId, Context.ConnectionId, orderId, groupName);

            // Confirma a subscrição para o cliente
            await Clients.Caller.SendAsync("SubscriptionConfirmed", new
            {
                OrderId = orderId,
                Message = "Você está inscrito para receber atualizações deste pagamento",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Cancela a subscrição de atualizações de pagamento
        /// </summary>
        /// <param name="orderId">Identificador do pedido para cancelar a subscrição</param>
        public async Task UnsubscribeFromPaymentStatus(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return;
            }

            var userId = Context.UserIdentifier ?? Context.User?.Identity?.Name;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            var groupName = $"payment-status-{userId}-{orderId}";

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation(
                "Usuário {UserId} (ConnectionId: {ConnectionId}) removido das atualizações do pagamento {OrderId}", 
                userId, Context.ConnectionId, orderId);
        }

        /// <summary>
        /// Chamado quando um cliente se conecta ao Hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier ?? Context.User?.Identity?.Name ?? "Anonymous";
            _logger.LogInformation("Cliente conectado ao PaymentHub. User: {UserId}, ConnectionId: {ConnectionId}", 
                userId, Context.ConnectionId);
            
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Chamado quando um cliente se desconecta do Hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier ?? Context.User?.Identity?.Name ?? "Anonymous";
            
            if (exception != null)
            {
                _logger.LogError(exception, "Cliente desconectado do PaymentHub com erro. User: {UserId}, ConnectionId: {ConnectionId}", 
                    userId, Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation("Cliente desconectado do PaymentHub. User: {UserId}, ConnectionId: {ConnectionId}", 
                    userId, Context.ConnectionId);
            }
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}
