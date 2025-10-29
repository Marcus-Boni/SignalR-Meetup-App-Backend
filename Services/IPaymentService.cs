// Services/IPaymentService.cs
using CarTrackingApi.Dtos;

namespace CarTrackingApi.Services
{
    /// <summary>
    /// Interface para o servi�o de processamento de pagamentos.
    /// Abstrai a l�gica de neg�cios de pagamento do Hub (Clean Architecture).
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Processa um pagamento de forma ass�ncrona e envia atualiza��es de status via SignalR.
        /// Server-Initiated: Este m�todo � chamado pelo Controller e o servi�o empurra as atualiza��es.
        /// </summary>
        /// <param name="request">Dados do pagamento a ser processado</param>
        /// <param name="userId">Identificador �nico do usu�rio que iniciou o pagamento</param>
        /// <returns>Task representando a opera��o ass�ncrona</returns>
        Task ProcessPaymentAsync(PaymentRequestDto request, string userId);
    }
}
