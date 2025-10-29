// Services/IPaymentService.cs
using CarTrackingApi.Dtos;

namespace CarTrackingApi.Services
{
    /// <summary>
    /// Interface para o serviço de processamento de pagamentos.
    /// Abstrai a lógica de negócios de pagamento do Hub (Clean Architecture).
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Processa um pagamento de forma assíncrona e envia atualizações de status via SignalR.
        /// Server-Initiated: Este método é chamado pelo Controller e o serviço empurra as atualizações.
        /// </summary>
        /// <param name="request">Dados do pagamento a ser processado</param>
        /// <param name="userId">Identificador único do usuário que iniciou o pagamento</param>
        /// <returns>Task representando a operação assíncrona</returns>
        Task ProcessPaymentAsync(PaymentRequestDto request, string userId);
    }
}
