// Dtos/PaymentRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace CarTrackingApi.Dtos
{
    /// <summary>
    /// DTO para requisi��o de pagamento
    /// </summary>
    public class PaymentRequestDto
    {
        /// <summary>
        /// Identificador �nico do pedido/ordem
        /// </summary>
        [Required(ErrorMessage = "OrderId � obrigat�rio")]
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// Valor do pagamento a ser processado
        /// </summary>
        [Required(ErrorMessage = "Amount � obrigat�rio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount deve ser maior que zero")]
        public decimal Amount { get; set; }
    }
}
