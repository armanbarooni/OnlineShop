using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.IntegrationTests.Infrastructure;

namespace OnlineShop.IntegrationTests.Helpers
{
    public static class AuthHelper
    {
        private static readonly string[] AdminCredentials = new[]
        {
            "09123456789", // Phone number
            "",            // OTP code (will be retrieved from TestSmsService)
            "Admin",       // First name
            "User"         // Last name
        };

        public static async Task<string> GetAdminTokenAsync(HttpClient client, CustomWebApplicationFactory<Program> factory = null)
        {
            Console.WriteLine($"[AuthHelper] Starting authentication process...");

            try 
            {
                // Seed database if factory is provided
                if (factory != null)
                {
                    Console.WriteLine($"[AuthHelper] Seeding database...");
                    await factory.SeedDatabaseAsync();
                }

                // Try password-based login first (for pre-seeded admin user)
                var hardcodedToken = await TryHardcodedAdminLoginAsync(client);
                if (!string.IsNullOrEmpty(hardcodedToken))
                {
                    Console.WriteLine($"[AuthHelper] Successfully obtained token via hardcoded admin login");
                    return hardcodedToken;
                }

                // If hardcoded login fails, try OTP flow
                Console.WriteLine($"[AuthHelper] Hardcoded login failed, trying OTP flow...");
                var loginToken = await TryLoginAsync(client, factory);
                if (!string.IsNullOrEmpty(loginToken))
                {
                    Console.WriteLine($"[AuthHelper] Successfully obtained token via OTP login");
                    return loginToken;
                }

                // If login fails, try registration
                Console.WriteLine($"[AuthHelper] OTP login failed, trying registration...");
                var registrationToken = await TryRegisterAsync(client, factory);
                if (!string.IsNullOrEmpty(registrationToken))
                {
                    Console.WriteLine($"[AuthHelper] Successfully obtained token via registration");
                    return registrationToken;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthHelper] Critical Authentication Error: {ex.Message}");
                Console.WriteLine($"[AuthHelper] Stack Trace: {ex.StackTrace}");
            }

            Console.WriteLine($"[AuthHelper] WARNING: Unable to obtain authentication token, returning empty string");
            return string.Empty;
        }

        private static async Task<string> TryLoginAsync(HttpClient client, CustomWebApplicationFactory<Program> factory)
        {
            try 
            {
                // Send OTP for login
                var loginOtpDto = new { PhoneNumber = AdminCredentials[0], Purpose = "login" };
                var otpResponse = await client.PostAsJsonAsync("/api/auth/send-otp", loginOtpDto);
                
                if (!otpResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[AuthHelper] Login OTP Failed: {await otpResponse.Content.ReadAsStringAsync()}");
                    return null;
                }

                // Get the OTP code from TestSmsService
                string? otpCode = null;
                for (int i = 0; i < 5 && string.IsNullOrEmpty(otpCode); i++)
                {
                    await Task.Delay(150);
                    otpCode = TestSmsService.GetLastOtpCode(AdminCredentials[0]);
                }
                // Fallback: try to retrieve from database if still null
                if (string.IsNullOrEmpty(otpCode) && factory != null)
                {
                    try
                    {
                        using var scope = factory.Services.CreateScope();
                        var repo = scope.ServiceProvider.GetRequiredService<IOtpRepository>();
                        var otp = await repo.GetValidOtpByPhoneAsync(AdminCredentials[0]);
                        otpCode = otp?.Code;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[AuthHelper] OTP DB fallback failed: {ex.Message}");
                    }
                }
                
                if (string.IsNullOrEmpty(otpCode))
                {
                    Console.WriteLine($"[AuthHelper] Could not retrieve OTP code from TestSmsService");
                    return null;
                }

                Console.WriteLine($"[AuthHelper] Retrieved OTP code: {otpCode}");

                // Attempt login
                var loginDto = new { PhoneNumber = AdminCredentials[0], Code = otpCode };
                var loginResponse = await client.PostAsJsonAsync("/api/auth/login-phone", loginDto);

                if (loginResponse.IsSuccessStatusCode)
                {
                    var content = await loginResponse.Content.ReadAsStringAsync();
                    // Phone login returns AuthResponseDto directly
                    var token = JsonHelper.GetNestedProperty(content, "accessToken")
                                ?? JsonHelper.GetNestedProperty(content, "data", "accessToken");
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        Console.WriteLine($"[AuthHelper] Login Successful: Token retrieved");
                        return token;
                    }
                }
                else 
                {
                    Console.WriteLine($"[AuthHelper] Login Failed: {await loginResponse.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthHelper] Login Exception: {ex.Message}");
            }
            return null;
        }

        private static async Task<string> TryRegisterAsync(HttpClient client, CustomWebApplicationFactory<Program> factory)
        {
            try 
            {
                // Send OTP for registration
                var registerOtpDto = new { PhoneNumber = AdminCredentials[0], Purpose = "register" };
                var otpResponse = await client.PostAsJsonAsync("/api/auth/send-otp", registerOtpDto);
                
                if (!otpResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[AuthHelper] Registration OTP Failed: {await otpResponse.Content.ReadAsStringAsync()}");
                    return null;
                }

                // Get the OTP code from TestSmsService
                string? otpCode = null;
                for (int i = 0; i < 5 && string.IsNullOrEmpty(otpCode); i++)
                {
                    await Task.Delay(150);
                    otpCode = TestSmsService.GetLastOtpCode(AdminCredentials[0]);
                }
                // Fallback: try to retrieve from database if still null
                if (string.IsNullOrEmpty(otpCode) && factory != null)
                {
                    try
                    {
                        using var scope = factory.Services.CreateScope();
                        var repo = scope.ServiceProvider.GetRequiredService<IOtpRepository>();
                        var otp = await repo.GetValidOtpByPhoneAsync(AdminCredentials[0]);
                        otpCode = otp?.Code;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[AuthHelper] OTP DB fallback (register) failed: {ex.Message}");
                    }
                }
                
                if (string.IsNullOrEmpty(otpCode))
                {
                    Console.WriteLine($"[AuthHelper] Could not retrieve OTP code from TestSmsService");
                    return null;
                }

                Console.WriteLine($"[AuthHelper] Retrieved OTP code: {otpCode}");

                // Attempt registration
                var registerDto = new 
                {
                    PhoneNumber = AdminCredentials[0],
                    Code = otpCode,
                    FirstName = AdminCredentials[2],
                    LastName = AdminCredentials[3]
                };
                var registerResponse = await client.PostAsJsonAsync("/api/auth/register-phone", registerDto);

                if (registerResponse.IsSuccessStatusCode)
                {
                    var content = await registerResponse.Content.ReadAsStringAsync();
                    // Phone registration might return Result<AuthResponseDto> or AuthResponseDto
                    var token = JsonHelper.GetNestedProperty(content, "data", "accessToken")
                                ?? JsonHelper.GetNestedProperty(content, "accessToken");
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        Console.WriteLine($"[AuthHelper] Registration Successful: Token retrieved");
                        return token;
                    }
                }
                else 
                {
                    Console.WriteLine($"[AuthHelper] Registration Failed: {await registerResponse.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthHelper] Registration Exception: {ex.Message}");
            }
            return null;
        }

        private static async Task<string> TryHardcodedAdminLoginAsync(HttpClient client)
        {
            try 
            {
                // Hardcoded admin login attempt (matches the admin user created in CustomWebApplicationFactory)
                var hardcodedLoginDto = new 
                { 
                    Email = "admin@test.com", 
                    Password = "AdminPassword123!" 
                };
                var loginResponse = await client.PostAsJsonAsync("/api/auth/login", hardcodedLoginDto);

                if (loginResponse.IsSuccessStatusCode)
                {
                    var content = await loginResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"[AuthHelper] Login response: {content}");
                    
                    // AuthController returns AuthResponseDto directly (not wrapped in Result)
                    // Try different property paths for token
                    var token = JsonHelper.GetNestedProperty(content, "accessToken")
                                ?? JsonHelper.GetNestedProperty(content, "access_token")
                                ?? JsonHelper.GetNestedProperty(content, "data", "accessToken");
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        Console.WriteLine($"[AuthHelper] Hardcoded Admin Login Successful: Token retrieved");
                        return token;
                    }
                    else
                    {
                        Console.WriteLine($"[AuthHelper] Token not found in response. Full response: {content}");
                    }
                }
                else 
                {
                    Console.WriteLine($"[AuthHelper] Hardcoded Admin Login Failed ({loginResponse.StatusCode}): {await loginResponse.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthHelper] Hardcoded Admin Login Exception: {ex.Message}");
            }
            return null;
        }
    }
}

