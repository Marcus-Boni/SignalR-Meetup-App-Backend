// Hubs/PaymentHub.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CarTrackingApi.Hubs
{
    /// <summary>
    /// Hub especializado para notifica��es de status de pagamento.
    /// Este � um "Thin Hub" - apenas gerencia grupos e subscri��es.
    /// A l�gica de pagamento est� no IPaymentService.
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
        /// Subscreve o cliente para receber atualiza��es de status de um pagamento espec�fico.
        /// Client-Initiated: O cliente se inscreve ap�s iniciar um pagamento via HTTP.
        /// </summary>
        /// <param name="orderId">Identificador do pedido para monitorar</param>
        public async Task SubscribeToPaymentStatus(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                _logger.LogWarning("Tentativa de subscrever a pagamento com OrderId inv�lido. ConnectionId: {ConnectionId}", 
                    Context.ConnectionId);
                return;
            }

            // Obt�m o identificador �nico do usu�rio autenticado
            var userId = Context.UserIdentifier ?? Context.User?.Identity?.Name;

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Tentativa de subscrever a pagamento sem usu�rio autenticado. ConnectionId: {ConnectionId}", 
                    Context.ConnectionId);
                return;
            }

            // Cria um nome de grupo SEGURO e PRIVADO para este usu�rio e pedido
            // Apenas este usu�rio receber� as atualiza��es deste pagamento
            var groupName = $"payment-status-{userId}-{orderId}";

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation(
                "Usu�rio {UserId} (ConnectionId: {ConnectionId}) subscrito para atualiza��es do pagamento {OrderId}. Grupo: {GroupName}", 
                userId, Context.ConnectionId, orderId, groupName);

            // Confirma a subscri��o para o cliente
            await Clients.Caller.SendAsync("SubscriptionConfirmed", new
            {
                OrderId = orderId,
                Message = "Voc� est� inscrito para receber atualiza��es deste pagamento",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Cancela a subscri��o de atualiza��es de pagamento
        /// </summary>
        /// <param name="orderId">Identificador do pedido para cancelar a subscri��o</param>
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
                "Usu�rio {UserId} (ConnectionId: {ConnectionId}) removido das atualiza��es do pagamento {OrderId}", 
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
