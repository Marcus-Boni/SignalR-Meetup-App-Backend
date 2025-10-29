// Controllers/PaymentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarTrackingApi.Dtos;
using CarTrackingApi.Services;
using System.Security.Claims;

namespace CarTrackingApi.Controllers
{
    /// <summary>
    /// Controller para gerenciar pagamentos.
    /// Recebe requisições HTTP e delega o processamento para o IPaymentService.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Inicia o processamento de um pagamento.
        /// O status será enviado via SignalR (PaymentHub) de forma assíncrona.
        /// </summary>
        /// <param name="request">Dados do pagamento</param>
        /// <returns>Resposta HTTP 202 Accepted com confirmação de início do processamento</returns>
        [HttpPost("pay")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult InitiatePayment([FromBody] PaymentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Requisição de pagamento inválida. Erros: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            // Obtém o identificador do usuário do token JWT
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                        ?? User.FindFirst(ClaimTypes.Name)?.Value 
                        ?? User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("Não foi possível obter o UserId do token de autenticação");
                return Unauthorized(new { message = "Não foi possível identificar o usuário" });
            }

            _logger.LogInformation(
                "Pagamento iniciado via HTTP. OrderId: {OrderId}, UserId: {UserId}, Amount: {Amount}",
                request.OrderId, userId, request.Amount);

            // IMPORTANTE: Fire-and-forget pattern
            // Não damos await aqui! O processamento acontece em background.
            // O cliente receberá as atualizações via SignalR, não via esta resposta HTTP.
            _ = _paymentService.ProcessPaymentAsync(request, userId);

            // Retorna imediatamente com status 202 Accepted
            return Accepted(new
            {
                orderId = request.OrderId,
                status = "Processing initiated",
                message = "Seu pagamento foi recebido e está sendo processado. Você receberá atualizações em tempo real.",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Endpoint de health check para verificar se o serviço de pagamentos está online.
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                service = "PaymentsController",
                status = "Healthy",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
