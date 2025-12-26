namespace OnlineShop.Infrastructure.PaymentGateways.Sadad.Exceptions
{
    /// <summary>
    /// Exception thrown when Sadad gateway operation fails
    /// </summary>
    public class SadadGatewayException : Exception
    {
        public int? ErrorCode { get; }

        public SadadGatewayException(string message, int? errorCode = null) : base(message)
        {
            ErrorCode = errorCode;
        }

        public SadadGatewayException(string message, Exception innerException, int? errorCode = null) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}

