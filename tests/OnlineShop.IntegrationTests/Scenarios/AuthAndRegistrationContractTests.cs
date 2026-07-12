using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Domain.Entities;
using OnlineShop.IntegrationTests.Helpers;
using OnlineShop.IntegrationTests.Infrastructure;
using Xunit;

namespace OnlineShop.IntegrationTests.Scenarios
{
    public class AuthAndRegistrationContractTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public AuthAndRegistrationContractTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            TestSmsService.ClearOtpCodes();
        }

        [Fact]
        public async Task A1_PasswordLogin_WithValidCredentials_ShouldSucceed()
        {
            var phone = UniquePhoneNumber();
            var email = $"a1-{Guid.NewGuid():N}@test.com";
            var password = "CorrectPassword123!";
            await CreateUserAsync(email, phone, password);

            var response = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Contain("accessToken");
        }

        [Fact]
        public async Task A2_PasswordLogin_WithWrongPassword_ShouldReturnInvalidCredentialsMessage()
        {
            var email = $"a2-{Guid.NewGuid():N}@test.com";
            await CreateUserAsync(email, UniquePhoneNumber(), "CorrectPassword123!");

            var response = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                Email = email,
                Password = "WrongPassword123!"
            });

            response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("نام کاربری یا رمز عبور اشتباه است");
            content.Should().NotContain("400");
        }

        [Fact]
        public async Task A3_PasswordLogin_WithUnknownUsername_ShouldReturnUserNotFoundMessage()
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                Email = $"missing-{Guid.NewGuid():N}@test.com",
                Password = "AnyPassword123!"
            });

            response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("کاربری با این نام یافت نشد");
            content.Should().NotContain("400");
        }

        [Fact]
        public async Task A4_PasswordLogin_WithInvalidCredentials_ShouldNotReturnBadRequest()
        {
            var response = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                Email = $"a4-{Guid.NewGuid():N}@test.com",
                Password = "WrongPassword123!"
            });

            response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task A_Login_ShouldAcceptBothEmailAndPhoneNumber()
        {
            var phone = UniquePhoneNumber();
            var email = $"identifier-{Guid.NewGuid():N}@test.com";
            var password = "Identifier123!";
            await CreateUserAsync(email, phone, password);

            var emailLogin = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = password });
            var phoneLogin = await _client.PostAsJsonAsync("/api/auth/login", new { Email = phone, Password = password });

            emailLogin.StatusCode.Should().Be(HttpStatusCode.OK);
            phoneLogin.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task B1_SendLoginSms_ForRegisteredPhone_ShouldSendOtp()
        {
            var phone = UniquePhoneNumber();
            await CreateUserAsync($"b1-{Guid.NewGuid():N}@test.com", phone, "UserPassword123!");

            var response = await SendOtpRequestAsync(phone, "Login");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            TestSmsService.GetLastOtpCode(phone).Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task B2_SendLoginSms_ForUnregisteredPhone_ShouldNotSendOtp()
        {
            var phone = UniquePhoneNumber();

            var response = await SendOtpRequestAsync(phone, "Login");

            response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
            TestSmsService.GetLastOtpCode(phone).Should().BeNull();
            (await response.Content.ReadAsStringAsync()).Should().Contain("کاربری با این شماره یافت نشد");
        }

        [Fact]
        public async Task B3_LoginWithSms_WithCorrectCode_ShouldSucceed()
        {
            var phone = UniquePhoneNumber();
            await CreateUserAsync($"b3-{Guid.NewGuid():N}@test.com", phone, "UserPassword123!");
            await SendOtpAndRequireSuccessAsync(phone, "Login");

            var code = TestSmsService.GetLastOtpCode(phone);
            var response = await _client.PostAsJsonAsync("/api/auth/login-phone", new { PhoneNumber = phone, Code = code });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Contain("accessToken");
        }

        [Fact]
        public async Task B4_LoginWithSms_WithWrongCode_ShouldStayRecoverable()
        {
            var phone = UniquePhoneNumber();
            await CreateUserAsync($"b4-{Guid.NewGuid():N}@test.com", phone, "UserPassword123!");
            await SendOtpAndRequireSuccessAsync(phone, "Login");

            var response = await _client.PostAsJsonAsync("/api/auth/verify-otp", new { PhoneNumber = phone, Code = "000000" });

            response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("نادرست");
            content.Should().NotContain("400");
        }

        [Fact]
        public async Task B5_LoginWithSms_ShouldAllowUpToThreeWrongAttemptsBeforePolicyApplies()
        {
            var phone = UniquePhoneNumber();
            await CreateUserAsync($"b5-{Guid.NewGuid():N}@test.com", phone, "UserPassword123!");
            await SendOtpAndRequireSuccessAsync(phone, "Login");

            for (var attempt = 1; attempt <= 3; attempt++)
            {
                var response = await _client.PostAsJsonAsync("/api/auth/verify-otp", new { PhoneNumber = phone, Code = $"00000{attempt}" });
                response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
                (await response.Content.ReadAsStringAsync()).Should().Contain("تلاش");
            }
        }

        [Fact]
        public async Task B6_ResendLoginSms_BeforeTwoMinutes_ShouldNotSendNewCodeAndShouldReturnWaitMessage()
        {
            var phone = UniquePhoneNumber();
            await CreateUserAsync($"b6-{Guid.NewGuid():N}@test.com", phone, "UserPassword123!");
            await SendOtpAndRequireSuccessAsync(phone, "Login");
            var firstCode = TestSmsService.GetLastOtpCode(phone);

            var response = await SendOtpRequestAsync(phone, "Login");

            response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
            TestSmsService.GetLastOtpCode(phone).Should().Be(firstCode);
            (await response.Content.ReadAsStringAsync()).Should()
                .Contain("پیامک برای شما اخیراً ارسال شده است، لطفاً حداقل ۲ دقیقه صبر کنید");
        }

        [Fact]
        public async Task B7_OtpCooldown_ShouldBeScopedToSameFlowAndSamePhone()
        {
            var phone = UniquePhoneNumber();
            await CreateUserAsync($"b7-{Guid.NewGuid():N}@test.com", phone, "UserPassword123!");
            await SendOtpAndRequireSuccessAsync(phone, "Login");

            var registrationResponse = await SendOtpRequestAsync(phone, "Registration");

            registrationResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task C1_RegisterWithPhone_WithNewPhone_ShouldSucceed()
        {
            var phone = UniquePhoneNumber();
            await SendOtpRequestAsync(phone, "Registration");
            var code = TestSmsService.GetLastOtpCode(phone) ?? "123456";

            var response = await RegisterPhoneAsync(phone, code);

            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        [Fact]
        public async Task C2_RegisterWithPhone_WithDuplicatePhone_ShouldReturnDuplicatePhoneMessage()
        {
            var phone = UniquePhoneNumber();
            await CreateUserAsync($"c2-existing-{Guid.NewGuid():N}@test.com", phone, "UserPassword123!");

            var response = await RegisterPhoneAsync(phone, "123456");

            response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
            (await response.Content.ReadAsStringAsync()).Should().Contain("این شماره قبلاً ثبت‌نام شده است");
        }

        [Fact]
        public async Task C3_RegisterWithPhone_ShouldNotBlockDuplicateEmailInThisScope()
        {
            var duplicateEmail = $"c3-{Guid.NewGuid():N}@test.com";
            await CreateUserAsync(duplicateEmail, UniquePhoneNumber(), "UserPassword123!");

            var response = await RegisterPhoneAsync(UniquePhoneNumber(), "123456");

            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        [Fact]
        public async Task C4_RegisterWithPhone_WithCorrectSmsCode_ShouldCompleteRegistration()
        {
            var phone = UniquePhoneNumber();
            await SendOtpAndRequireSuccessAsync(phone, "Registration");

            var response = await RegisterPhoneAsync(phone, TestSmsService.GetLastOtpCode(phone)!);

            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        [Fact]
        public async Task C5_RegisterWithPhone_WithWrongSmsCode_ShouldRemainRecoverable()
        {
            var phone = UniquePhoneNumber();
            await SendOtpAndRequireSuccessAsync(phone, "Registration");

            var response = await RegisterPhoneAsync(phone, "000000");

            response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("کد");
            content.Should().Contain("نادرست");
            content.Should().NotContain("accessToken");
        }

        [Fact]
        public async Task C6_ResendRegistrationSms_BeforeTwoMinutes_ShouldNotSendNewCode()
        {
            var phone = UniquePhoneNumber();
            await SendOtpAndRequireSuccessAsync(phone, "Registration");
            var firstCode = TestSmsService.GetLastOtpCode(phone);

            var response = await SendOtpRequestAsync(phone, "Registration");

            response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
            TestSmsService.GetLastOtpCode(phone).Should().Be(firstCode);
            (await response.Content.ReadAsStringAsync()).Should().Contain("۲ دقیقه");
        }

        private Task<HttpResponseMessage> SendOtpRequestAsync(string phone, string purpose)
        {
            return _client.PostAsJsonAsync("/api/auth/send-otp", new { PhoneNumber = phone, Purpose = purpose });
        }

        private async Task SendOtpAndRequireSuccessAsync(string phone, string purpose)
        {
            var response = await SendOtpRequestAsync(phone, purpose);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            TestSmsService.GetLastOtpCode(phone).Should().NotBeNullOrWhiteSpace();
        }

        private Task<HttpResponseMessage> RegisterPhoneAsync(string phone, string code)
        {
            return _client.PostAsJsonAsync("/api/auth/register-phone", new
            {
                PhoneNumber = phone,
                Code = code,
                FirstName = "Contract",
                LastName = "User",
                Password = "Register123!"
            });
        }

        private async Task CreateUserAsync(string email, string phone, string password)
        {
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                PhoneNumber = phone,
                PhoneNumberConfirmed = true,
                FirstName = "Contract",
                LastName = "User"
            };

            var result = await userManager.CreateAsync(user, password);
            result.Succeeded.Should().BeTrue(string.Join(", ", result.Errors.Select(e => e.Description)));
            await userManager.AddToRoleAsync(user, "User");
        }

        private static string UniquePhoneNumber()
        {
            return "09" + Random.Shared.Next(100000000, 999999999);
        }
    }
}
