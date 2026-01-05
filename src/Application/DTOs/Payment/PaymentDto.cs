using System;

namespace OnlineShop.Application.DTOs.Payment
{
    public class InitiatePaymentDto
    {
        public Guid OrderId { get; set; }
        public string CallbackUrl { get; set; } = string.Empty;
    }

    public class PaymentInitiationResultDto
    {
        public string PaymentUrl { get; set; } = string.Empty;
        public string Authority { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class VerifyPaymentDto
    {
        public string Authority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class PaymentVerificationResultDto
    {
        public bool IsSuccess { get; set; }
        public string RefId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
    }
}
