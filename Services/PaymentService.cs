// Services/PaymentService.cs
using Microsoft.AspNetCore.SignalR;
using CarTrackingApi.Hubs;
using CarTrackingApi.Dtos;

namespace CarTrackingApi.Services
{
    /// <summary>
    /// Implementação do serviço de pagamento.
    /// Simula o processamento assíncrono de pagamentos e notifica o usuário via SignalR.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly IHubContext<PaymentHub> _hubContext;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IHubContext<PaymentHub> hubContext,
            ILogger<PaymentService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Processa um pagamento de forma assíncrona.
        /// Envia atualizações de status APENAS para o usuário específico que iniciou o pagamento.
        /// </summary>
        public async Task ProcessPaymentAsync(PaymentRequestDto request, string userId)
        {
            if (request == null)
            {
                _logger.LogError("PaymentRequestDto é null");
                return;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("UserId é null ou vazio para o pedido {OrderId}", request.OrderId);
                return;
            }

            // Cria o nome do grupo SEGURO - idêntico ao usado no PaymentHub
            // Apenas o usuário que criou este pagamento receberá as atualizações
            var groupName = $"payment-status-{userId}-{request.OrderId}";

            _logger.LogInformation(
                "Iniciando processamento do pagamento. OrderId: {OrderId}, UserId: {UserId}, Amount: {Amount}, Group: {GroupName}",
                request.OrderId, userId, request.Amount, groupName);

            try
            {
                // --- FASE 1: Status "Processing" ---
                var processingStatus = new PaymentStatusDto
                {
                    OrderId = request.OrderId,
                    Status = "Processing",
                    Message = "Seu pagamento está sendo processado pelo gateway...",
                    Timestamp = DateTime.UtcNow,
                    Amount = request.Amount
                };

                await _hubContext.Clients.Group(groupName).SendAsync("PaymentStatusUpdate", processingStatus);
                
                _logger.LogInformation("Status 'Processing' enviado para o grupo {GroupName}", groupName);

                // --- SIMULAÇÃO: Aguarda o processamento do gateway (5 segundos) ---
                await Task.Delay(5000);

                // --- FASE 2: Determina o resultado (simulado) ---
                // Para esta demo, vamos aprovar 80% dos pagamentos
                var isApproved = Random.Shared.Next(100) < 80;

                PaymentStatusDto finalStatus;

                if (isApproved)
                {
                    finalStatus = new PaymentStatusDto
                    {
                        OrderId = request.OrderId,
                        Status = "Approved",
                        Message = $"Pagamento aprovado com sucesso! Valor: R$ {request.Amount:N2}",
                        Timestamp = DateTime.UtcNow,
                        Amount = request.Amount
                    };

                    _logger.LogInformation(
                        "Pagamento APROVADO. OrderId: {OrderId}, UserId: {UserId}, Amount: {Amount}",
                        request.OrderId, userId, request.Amount);
                }
                else
                {
                    finalStatus = new PaymentStatusDto
                    {
                        OrderId = request.OrderId,
                        Status = "Rejected",
                        Message = "Pagamento recusado. Entre em contato com sua operadora de cartão.",
                        Timestamp = DateTime.UtcNow,
                        Amount = request.Amount
                    };

                    _logger.LogWarning(
                        "Pagamento RECUSADO. OrderId: {OrderId}, UserId: {UserId}, Amount: {Amount}",
                        request.OrderId, userId, request.Amount);
                }

                // --- FASE 3: Envia o status final ---
                await _hubContext.Clients.Group(groupName).SendAsync("PaymentStatusUpdate", finalStatus);

                _logger.LogInformation(
                    "Status final '{Status}' enviado para o grupo {GroupName}",
                    finalStatus.Status, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Erro ao processar pagamento. OrderId: {OrderId}, UserId: {UserId}",
                    request.OrderId, userId);

                // Envia notificação de erro para o usuário
                var errorStatus = new PaymentStatusDto
                {
                    OrderId = request.OrderId,
                    Status = "Error",
                    Message = "Ocorreu um erro ao processar seu pagamento. Por favor, tente novamente.",
                    Timestamp = DateTime.UtcNow,
                    Amount = request.Amount
                };

                try
                {
                    await _hubContext.Clients.Group(groupName).SendAsync("PaymentStatusUpdate", errorStatus);
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "Erro ao enviar status de erro via SignalR");
                }
            }
        }
    }
}
