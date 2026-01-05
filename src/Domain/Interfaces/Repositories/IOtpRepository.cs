using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces.Repositories
{
    public interface IOtpRepository
    {
        Task<Otp?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Otp>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Otp otp, CancellationToken cancellationToken = default);
        Task UpdateAsync(Otp otp, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the most recent valid OTP for a phone number
        /// </summary>
        Task<Otp?> GetValidOtpByPhoneAsync(string phoneNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all OTPs for a phone number (for cleanup)
        /// </summary>
        Task<List<Otp>> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete expired OTPs (cleanup job)
        /// </summary>
        Task DeleteExpiredOtpsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Invalidate all previous OTPs for a phone number
        /// </summary>
        Task InvalidatePreviousOtpsAsync(string phoneNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the latest OTP for a phone number (for rate limiting)
        /// </summary>
        Task<Otp?> GetLatestOtpAsync(string phoneNumber, CancellationToken cancellationToken = default);
    }
}
