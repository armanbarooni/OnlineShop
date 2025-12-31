using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlineShop.Application.Common.Models;

using OnlineShop.Application.Contracts.Services;
using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Domain.Entities;

using OnlineShop.Domain.Interfaces.Repositories;
using System.Text.Json;
namespace OnlineShop.Application.Features.Auth.Commands.SendOtp
{
    public class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, Result<OtpResponseDto>>
    {
        private readonly IOtpRepository _otpRepository;
        private readonly ISmsService _smsService;
        private readonly SmsSettings _smsSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Microsoft.Extensions.Logging.ILogger<SendOtpCommandHandler> _logger;

        public SendOtpCommandHandler(
            IOtpRepository otpRepository,
            ISmsService smsService,
            IOptions<SmsSettings> smsSettings,
            UserManager<ApplicationUser> userManager,
            Microsoft.Extensions.Logging.ILogger<SendOtpCommandHandler> logger)
        {
            _otpRepository = otpRepository;
            _smsService = smsService;
            _smsSettings = smsSettings.Value;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Result<OtpResponseDto>> Handle(SendOtpCommand request, CancellationToken cancellationToken)
        {
            // #region agent log
            try {
                var logEntry = new {
                    id = $"log_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = "SendOtpCommandHandler.cs:37",
                    message = "SendOtp Handle entry",
                    data = new { phoneNumber = request.Request.PhoneNumber, purpose = request.Request.Purpose },
                    sessionId = "debug-session",
                    runId = "run1",
                    hypothesisId = "A"
                };
                await System.IO.File.AppendAllTextAsync(@"c:\Users\Tommy2sec\Desktop\New folder\.cursor\debug.log", JsonSerializer.Serialize(logEntry) + "\n", cancellationToken);
            } catch { }
            // #endregion

            // Invalidate all previous OTPs for this phone number
            await _otpRepository.InvalidatePreviousOtpsAsync(request.Request.PhoneNumber, cancellationToken);

            // #region agent log
            try {
                var logEntry = new {
                    id = $"log_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = "SendOtpCommandHandler.cs:43",
                    message = "Before Login purpose check",
                    data = new { 
                        purpose = request.Request.Purpose, 
                        purposeEqualsLogin = string.Equals(request.Request.Purpose, "Login", StringComparison.OrdinalIgnoreCase),
                        phoneNumber = request.Request.PhoneNumber
                    },
                    sessionId = "debug-session",
                    runId = "run1",
                    hypothesisId = "A"
                };
                await System.IO.File.AppendAllTextAsync(@"c:\Users\Tommy2sec\Desktop\New folder\.cursor\debug.log", JsonSerializer.Serialize(logEntry) + "\n", cancellationToken);
            } catch { }
            // #endregion

            // Check if user exists for Login purpose
            if (string.Equals(request.Request.Purpose, "Login", StringComparison.OrdinalIgnoreCase))
            {
                // #region agent log
                try {
                    var logEntry = new {
                        id = $"log_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        location = "SendOtpCommandHandler.cs:45",
                        message = "Login check branch entered - querying user",
                        data = new { phoneNumber = request.Request.PhoneNumber },
                        sessionId = "debug-session",
                        runId = "run1",
                        hypothesisId = "B"
                    };
                    await System.IO.File.AppendAllTextAsync(@"c:\Users\Tommy2sec\Desktop\New folder\.cursor\debug.log", JsonSerializer.Serialize(logEntry) + "\n", cancellationToken);
                } catch { }
                // #endregion

                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.Request.PhoneNumber, cancellationToken);

                // #region agent log
                try {
                    var logEntry = new {
                        id = $"log_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        location = "SendOtpCommandHandler.cs:46",
                        message = "User lookup result",
                        data = new { 
                            phoneNumber = request.Request.PhoneNumber,
                            userFound = user != null,
                            userId = user?.Id
                        },
                        sessionId = "debug-session",
                        runId = "run1",
                        hypothesisId = "B"
                    };
                    await System.IO.File.AppendAllTextAsync(@"c:\Users\Tommy2sec\Desktop\New folder\.cursor\debug.log", JsonSerializer.Serialize(logEntry) + "\n", cancellationToken);
                } catch { }
                // #endregion

                if (user == null)
                {
                    // #region agent log
                    try {
                        var logEntry = new {
                            id = $"log_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}",
                            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            location = "SendOtpCommandHandler.cs:47",
                            message = "User not found - returning failure",
                            data = new { phoneNumber = request.Request.PhoneNumber },
                            sessionId = "debug-session",
                            runId = "run1",
                            hypothesisId = "B"
                        };
                        await System.IO.File.AppendAllTextAsync(@"c:\Users\Tommy2sec\Desktop\New folder\.cursor\debug.log", JsonSerializer.Serialize(logEntry) + "\n", cancellationToken);
                    } catch { }
                    // #endregion

                    _logger.LogWarning("SendOtp failed - user not found for login: {PhoneNumber}", request.Request.PhoneNumber);
                    return Result<OtpResponseDto>.Failure("نام کاربری یا کلمه عبور اشتباه است");
                }

                // #region agent log
                try {
                    var logEntry = new {
                        id = $"log_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        location = "SendOtpCommandHandler.cs:51",
                        message = "User found - proceeding to generate OTP",
                        data = new { phoneNumber = request.Request.PhoneNumber, userId = user.Id },
                        sessionId = "debug-session",
                        runId = "run1",
                        hypothesisId = "B"
                    };
                    await System.IO.File.AppendAllTextAsync(@"c:\Users\Tommy2sec\Desktop\New folder\.cursor\debug.log", JsonSerializer.Serialize(logEntry) + "\n", cancellationToken);
                } catch { }
                // #endregion
            }
            else
            {
                // #region agent log
                try {
                    var logEntry = new {
                        id = $"log_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        location = "SendOtpCommandHandler.cs:52",
                        message = "Not Login purpose - skipping user check",
                        data = new { purpose = request.Request.Purpose, phoneNumber = request.Request.PhoneNumber },
                        sessionId = "debug-session",
                        runId = "run1",
                        hypothesisId = "A"
                    };
                    await System.IO.File.AppendAllTextAsync(@"c:\Users\Tommy2sec\Desktop\New folder\.cursor\debug.log", JsonSerializer.Serialize(logEntry) + "\n", cancellationToken);
                } catch { }
                // #endregion
            }

            // Generate OTP code
            var code = GenerateOtpCode(_smsSettings.OtpLength);
            _logger.LogInformation("Generated OTP: {Code} with length {Length} for {PhoneNumber}", code, code.Length, request.Request.PhoneNumber);

            // #region agent log
            try {
                var logEntry = new {
                    id = $"log_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = "SendOtpCommandHandler.cs:54",
                    message = "OTP code generated",
                    data = new { phoneNumber = request.Request.PhoneNumber, codeLength = code.Length },
                    sessionId = "debug-session",
                    runId = "run1",
                    hypothesisId = "C"
                };
                await System.IO.File.AppendAllTextAsync(@"c:\Users\Tommy2sec\Desktop\New folder\.cursor\debug.log", JsonSerializer.Serialize(logEntry) + "\n", cancellationToken);
            } catch { }
            // #endregion

            // Create OTP entity
            var otp = Otp.Create(
                request.Request.PhoneNumber,
                code,
                _smsSettings.OtpExpirationMinutes,
                request.Request.Purpose
            );

            // Save to database
            await _otpRepository.AddAsync(otp, cancellationToken);

            // #region agent log
            try {
                var logEntry = new {
                    id = $"log_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = "SendOtpCommandHandler.cs:69",
                    message = "Before SMS send",
                    data = new { phoneNumber = request.Request.PhoneNumber },
                    sessionId = "debug-session",
                    runId = "run1",
                    hypothesisId = "C"
                };
                await System.IO.File.AppendAllTextAsync(@"c:\Users\Tommy2sec\Desktop\New folder\.cursor\debug.log", JsonSerializer.Serialize(logEntry) + "\n", cancellationToken);
            } catch { }
            // #endregion

            // Send SMS
            var smsSent = await _smsService.SendOtpAsync(request.Request.PhoneNumber, code, cancellationToken);

            // #region agent log
            try {
                var logEntry = new {
                    id = $"log_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = "SendOtpCommandHandler.cs:71",
                    message = "After SMS send",
                    data = new { phoneNumber = request.Request.PhoneNumber, smsSent = smsSent },
                    sessionId = "debug-session",
                    runId = "run1",
                    hypothesisId = "C"
                };
                await System.IO.File.AppendAllTextAsync(@"c:\Users\Tommy2sec\Desktop\New folder\.cursor\debug.log", JsonSerializer.Serialize(logEntry) + "\n", cancellationToken);
            } catch { }
            // #endregion

            if (!smsSent)
            {
                return Result<OtpResponseDto>.Failure("خطا در ارسال پیامک. لطفاً دوباره تلاش کنید");
            }

            var response = new OtpResponseDto
            {
                Success = true,
                Message = $"کد تایید به شماره {request.Request.PhoneNumber} ارسال شد",
                ExpiresAt = otp.ExpiresAt
            };

            // #region agent log
            try {
                var logEntry = new {
                    id = $"log_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    location = "SendOtpCommandHandler.cs:83",
                    message = "SendOtp Handle exit - success",
                    data = new { phoneNumber = request.Request.PhoneNumber },
                    sessionId = "debug-session",
                    runId = "run1",
                    hypothesisId = "A"
                };
                await System.IO.File.AppendAllTextAsync(@"c:\Users\Tommy2sec\Desktop\New folder\.cursor\debug.log", JsonSerializer.Serialize(logEntry) + "\n", cancellationToken);
            } catch { }
            // #endregion

            return Result<OtpResponseDto>.Success(response);
        }

        private string GenerateOtpCode(int length)
        {
            // Ensure minimum length
            if (length < 4) length = 4;

            var code = string.Empty;
            for (int i = 0; i < length; i++)
            { 
                code += Random.Shared.Next(0, 10).ToString(); 
            }
            return code;
        }
    }
}



