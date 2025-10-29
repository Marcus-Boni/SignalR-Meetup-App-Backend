// Dtos/PaymentRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace CarTrackingApi.Dtos
{
    /// <summary>
    /// DTO para requisição de pagamento
    /// </summary>
    public class PaymentRequestDto
    {
        /// <summary>
        /// Identificador único do pedido/ordem
        /// </summary>
        [Required(ErrorMessage = "OrderId é obrigatório")]
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// Valor do pagamento a ser processado
        /// </summary>
        [Required(ErrorMessage = "Amount é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount deve ser maior que zero")]
        public decimal Amount { get; set; }
    }
}
