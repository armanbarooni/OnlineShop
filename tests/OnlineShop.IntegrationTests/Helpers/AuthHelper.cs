using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

        public static async Task<string> GetAdminTokenAsync(HttpClient client)
        {
            Console.WriteLine($"[AuthHelper] Starting authentication process...");

            try 
            {
                // Try password-based login first (for pre-seeded admin user)
                var hardcodedToken = await TryHardcodedAdminLoginAsync(client);
                if (!string.IsNullOrEmpty(hardcodedToken))
                    return hardcodedToken;

                // Attempt to login with OTP
                var loginToken = await TryLoginAsync(client);
                if (!string.IsNullOrEmpty(loginToken))
                    return loginToken;

                // If login fails, try registration
                var registrationToken = await TryRegisterAsync(client);
                if (!string.IsNullOrEmpty(registrationToken))
                    return registrationToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthHelper] Critical Authentication Error: {ex.Message}");
                Console.WriteLine($"[AuthHelper] Stack Trace: {ex.StackTrace}");
            }

            Console.WriteLine($"[AuthHelper] WARNING: Unable to obtain authentication token, returning empty string");
            return string.Empty;
        }

        private static async Task<string> TryLoginAsync(HttpClient client)
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
                await Task.Delay(100); // Small delay to ensure OTP is captured
                var otpCode = TestSmsService.GetLastOtpCode(AdminCredentials[0]);
                
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

        private static async Task<string> TryRegisterAsync(HttpClient client)
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
                await Task.Delay(100); // Small delay to ensure OTP is captured
                var otpCode = TestSmsService.GetLastOtpCode(AdminCredentials[0]);
                
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

