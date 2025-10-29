// Dtos/PaymentStatusDto.cs
namespace CarTrackingApi.Dtos
{
    /// <summary>
    /// DTO para atualização de status de pagamento
    /// </summary>
    public class PaymentStatusDto
    {
        /// <summary>
        /// Identificador do pedido/ordem
        /// </summary>
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// Status atual do pagamento (Processing, Approved, Rejected)
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Mensagem descritiva sobre o status
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp da atualização (UTC)
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Valor do pagamento (opcional)
        /// </summary>
        public decimal? Amount { get; set; }
    }
}
