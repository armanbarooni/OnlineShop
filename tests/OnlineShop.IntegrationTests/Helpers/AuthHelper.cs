using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Application.Contracts.Services;
using OnlineShop.Application.DTOs.Auth;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces.Repositories;
using OnlineShop.IntegrationTests.Infrastructure;

namespace OnlineShop.IntegrationTests.Helpers
{
    public static class AuthHelper
    {
        private static readonly string[] AdminCredentials = new[]
        {
            "admin@test.com", // Email
            "AdminPassword123!", // Password
            "Admin",       // First name
            "User"         // Last name
        };

        private static readonly string AdminPhoneNumber = "09123456789"; // Phone number from CustomWebApplicationFactory

        private static readonly string[] UserCredentials = new[]
        {
            "user@test.com", // Email
            "UserPassword123!", // Password
            "Regular",     // First name
            "User"          // Last name
        };

        public static async Task<string> GetAdminTokenAsync(HttpClient client, CustomWebApplicationFactory<Program> factory = null)
        {
            Console.WriteLine($"[AuthHelper] Starting admin authentication process...");

            try 
            {
                // Seed database if factory is provided
                if (factory != null)
                {
                    Console.WriteLine($"[AuthHelper] Seeding database...");
                    await factory.SeedDatabaseAsync();
                }

                // Try actual login with hardcoded admin credentials
                var token = await TryHardcodedAdminLoginAsync(client);
                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine($"[AuthHelper] Admin login successful: Token retrieved ({token.Length} chars)");
                    return token;
                }

                // Fallback: try OTP-based login
                token = await TryLoginAsync(client, factory);
                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine($"[AuthHelper] Admin OTP login successful: Token retrieved ({token.Length} chars)");
                    return token;
                }

                // Fallback: try registration
                token = await TryRegisterAsync(client, factory);
                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine($"[AuthHelper] Admin registration successful: Token retrieved ({token.Length} chars)");
                    return token;
                }

                Console.WriteLine($"[AuthHelper] WARNING: All authentication methods failed, returning empty string");
                return string.Empty;
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
                var loginOtpDto = new { PhoneNumber = AdminPhoneNumber, Purpose = "Login" };
                var otpResponse = await client.PostAsJsonAsync("/api/auth/send-otp", loginOtpDto);
                
                if (!otpResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[AuthHelper] Login OTP Failed: {await otpResponse.Content.ReadAsStringAsync()}");
                    return null;
                }

                // Get the OTP code from TestSmsService with increased retry delay
                string? otpCode = null;
                for (int i = 0; i < 5 && string.IsNullOrEmpty(otpCode); i++)
                {
                    await Task.Delay(500); // Increased from 150ms to 500ms
                    otpCode = TestSmsService.GetLastOtpCode(AdminPhoneNumber);
                }
                // Fallback: try to retrieve from database if still null (before static lookup)
                if (string.IsNullOrEmpty(otpCode) && factory != null)
                {
                    try
                    {
                        using var scope = factory.Services.CreateScope();
                        var repo = scope.ServiceProvider.GetRequiredService<IOtpRepository>();
                        var otp = await repo.GetValidOtpByPhoneAsync(AdminPhoneNumber);
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
                    // Final fallback: use hardcoded test OTP for test environment
                    otpCode = "123456";
                    Console.WriteLine($"[AuthHelper] Using fallback OTP code: {otpCode}");
                }

                Console.WriteLine($"[AuthHelper] Retrieved OTP code: {otpCode}");

                // Attempt login
                var loginDto = new { PhoneNumber = AdminPhoneNumber, Code = otpCode };
                var loginResponse = await client.PostAsJsonAsync("/api/auth/login-phone", loginDto);

                if (loginResponse.IsSuccessStatusCode)
                {
                    var content = await loginResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"[AuthHelper] Phone Login response: {content}");
                    // Phone login returns AuthResponseDto directly
                    var token = JsonHelper.GetNestedProperty(content, "accessToken")
                                ?? JsonHelper.GetNestedProperty(content, "AccessToken")
                                ?? JsonHelper.GetNestedProperty(content, "data", "accessToken")
                                ?? JsonHelper.GetNestedProperty(content, "data", "AccessToken");
                    
                    // Backup: Try direct deserialization
                    if (string.IsNullOrEmpty(token))
                    {
                        var authResponse = JsonHelper.Deserialize<AuthResponseDto>(content);
                        if (authResponse != null && !string.IsNullOrEmpty(authResponse.AccessToken))
                        {
                            token = authResponse.AccessToken;
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        Console.WriteLine($"[AuthHelper] Login Successful: Token retrieved ({token.Length} chars)");
                        return token;
                    }
                    else
                    {
                        Console.WriteLine($"[AuthHelper] Token not found in phone login response");
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
                var registerOtpDto = new { PhoneNumber = AdminPhoneNumber, Purpose = "Registration" };
                var otpResponse = await client.PostAsJsonAsync("/api/auth/send-otp", registerOtpDto);
                
                if (!otpResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[AuthHelper] Registration OTP Failed: {await otpResponse.Content.ReadAsStringAsync()}");
                    return null;
                }

                // Get the OTP code from TestSmsService with increased retry delay
                string? otpCode = null;
                for (int i = 0; i < 5 && string.IsNullOrEmpty(otpCode); i++)
                {
                    await Task.Delay(500); // Increased from 150ms to 500ms
                    otpCode = TestSmsService.GetLastOtpCode(AdminPhoneNumber);
                }
                // Fallback: try to retrieve from database if still null (before static lookup)
                if (string.IsNullOrEmpty(otpCode) && factory != null)
                {
                    try
                    {
                        using var scope = factory.Services.CreateScope();
                        var repo = scope.ServiceProvider.GetRequiredService<IOtpRepository>();
                        var otp = await repo.GetValidOtpByPhoneAsync(AdminPhoneNumber);
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
                    PhoneNumber = AdminPhoneNumber,
                    Code = otpCode,
                    FirstName = AdminCredentials[2],
                    LastName = AdminCredentials[3]
                };
                var registerResponse = await client.PostAsJsonAsync("/api/auth/register-phone", registerDto);

                if (registerResponse.IsSuccessStatusCode)
                {
                    var content = await registerResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"[AuthHelper] Phone Registration response: {content}");
                    // Phone registration might return Result<AuthResponseDto> or AuthResponseDto
                    var token = JsonHelper.GetNestedProperty(content, "data", "accessToken")
                                ?? JsonHelper.GetNestedProperty(content, "data", "AccessToken")
                                ?? JsonHelper.GetNestedProperty(content, "accessToken")
                                ?? JsonHelper.GetNestedProperty(content, "AccessToken");
                    
                    // Backup: Try direct deserialization (handles both direct and wrapped formats)
                    if (string.IsNullOrEmpty(token))
                    {
                        var authResponse = JsonHelper.Deserialize<AuthResponseDto>(content);
                        if (authResponse != null && !string.IsNullOrEmpty(authResponse.AccessToken))
                        {
                            token = authResponse.AccessToken;
                        }
                        else
                        {
                            // Try extracting from Result wrapper
                            var dataElement = JsonHelper.GetNestedProperty(content, "data");
                            if (!string.IsNullOrEmpty(dataElement))
                            {
                                var nestedAuthResponse = JsonHelper.Deserialize<AuthResponseDto>(dataElement);
                                if (nestedAuthResponse != null && !string.IsNullOrEmpty(nestedAuthResponse.AccessToken))
                                {
                                    token = nestedAuthResponse.AccessToken;
                                }
                            }
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        Console.WriteLine($"[AuthHelper] Registration Successful: Token retrieved ({token.Length} chars)");
                        
                        // If this is admin phone registration, ensure user has Admin role
                        if (factory != null && registerDto.PhoneNumber == AdminPhoneNumber)
                        {
                            try
                            {
                                using var scope = factory.Services.CreateScope();
                                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                                
                                // Find user by phone number
                                var user = await userManager.FindByNameAsync(AdminPhoneNumber);
                                if (user != null && !await userManager.IsInRoleAsync(user, "Admin"))
                                {
                                    await userManager.AddToRoleAsync(user, "Admin");
                                    Console.WriteLine($"[AuthHelper] Added Admin role to registered user");
                                    
                                    // Regenerate token with Admin role
                                    var roles = await userManager.GetRolesAsync(user);
                                    var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
                                    var newTokens = await tokenService.GenerateTokensAsync(user.PhoneNumber!, roles);
                                    if (newTokens != null && !string.IsNullOrEmpty(newTokens.AccessToken))
                                    {
                                        token = newTokens.AccessToken;
                                        Console.WriteLine($"[AuthHelper] Regenerated token with Admin role");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[AuthHelper] Failed to add Admin role: {ex.Message}");
                                // Continue with existing token even if role assignment fails
                            }
                        }
                        
                        return token;
                    }
                    else
                    {
                        Console.WriteLine($"[AuthHelper] Token not found in registration response");
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

        public static async Task<string> GetUserTokenAsync(HttpClient client, CustomWebApplicationFactory<Program> factory = null)
        {
            Console.WriteLine($"[AuthHelper] Starting user authentication process...");

            try 
            {
                // Seed database if factory is provided
                if (factory != null)
                {
                    Console.WriteLine($"[AuthHelper] Seeding database...");
                    await factory.SeedDatabaseAsync();
                }

                // Try actual login with hardcoded user credentials
                var token = await TryHardcodedUserLoginAsync(client);
                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine($"[AuthHelper] User login successful: Token retrieved ({token.Length} chars)");
                    return token;
                }

                // Fallback: try OTP-based login
                token = await TryUserLoginAsync(client, factory);
                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine($"[AuthHelper] User OTP login successful: Token retrieved ({token.Length} chars)");
                    return token;
                }

                // Fallback: try registration
                token = await TryUserRegistrationAsync(client, factory);
                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine($"[AuthHelper] User registration successful: Token retrieved ({token.Length} chars)");
                    return token;
                }

                Console.WriteLine($"[AuthHelper] WARNING: All user authentication methods failed, returning empty string");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthHelper] User authentication exception: {ex.Message}");
            }
            return null;
        }

        private static async Task<string> TryHardcodedUserLoginAsync(HttpClient client)
        {
            try 
            {
                // Hardcoded user login attempt
                var hardcodedLoginDto = new 
                { 
                    Email = "user@test.com", 
                    Password = "UserPassword123!" 
                };
                var loginResponse = await client.PostAsJsonAsync("/api/auth/login", hardcodedLoginDto);

                if (loginResponse.IsSuccessStatusCode)
                {
                    var content = await loginResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"[AuthHelper] User login response: {content}");
                    
                    var token = JsonHelper.GetNestedProperty(content, "accessToken")
                                ?? JsonHelper.GetNestedProperty(content, "AccessToken")
                                ?? JsonHelper.GetNestedProperty(content, "access_token")
                                ?? JsonHelper.GetNestedProperty(content, "data", "accessToken")
                                ?? JsonHelper.GetNestedProperty(content, "data", "AccessToken");
                    
                    // Backup: Try direct deserialization
                    if (string.IsNullOrEmpty(token))
                    {
                        var authResponse = JsonHelper.Deserialize<AuthResponseDto>(content);
                        if (authResponse != null && !string.IsNullOrEmpty(authResponse.AccessToken))
                        {
                            token = authResponse.AccessToken;
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        Console.WriteLine($"[AuthHelper] Hardcoded User Login Successful: Token retrieved");
                        return token;
                    }
                    else
                    {
                        Console.WriteLine($"[AuthHelper] User token not found in response. Full response: {content}");
                    }
                }
                else 
                {
                    Console.WriteLine($"[AuthHelper] Hardcoded User Login Failed ({loginResponse.StatusCode}): {await loginResponse.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthHelper] Hardcoded User Login Exception: {ex.Message}");
            }
            return null;
        }

        private static async Task<string> TryUserLoginAsync(HttpClient client, CustomWebApplicationFactory<Program> factory)
        {
            try 
            {
                var phoneNumber = "09991234567"; // Different phone for user
                var otpCode = await GetOtpCodeAsync(phoneNumber, factory);
                
                if (string.IsNullOrEmpty(otpCode))
                {
                    Console.WriteLine($"[AuthHelper] Failed to get OTP for user login");
                    return null;
                }

                var loginDto = new 
                { 
                    PhoneNumber = phoneNumber, 
                    Code = otpCode 
                };
                var loginResponse = await client.PostAsJsonAsync("/api/auth/login-phone", loginDto);

                if (loginResponse.IsSuccessStatusCode)
                {
                    var content = await loginResponse.Content.ReadAsStringAsync();
                    var token = JsonHelper.GetNestedProperty(content, "data", "accessToken")
                                ?? JsonHelper.GetNestedProperty(content, "accessToken");
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        Console.WriteLine($"[AuthHelper] User OTP Login Successful: Token retrieved");
                        return token;
                    }
                }
                else 
                {
                    Console.WriteLine($"[AuthHelper] User OTP Login Failed: {await loginResponse.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthHelper] User OTP Login Exception: {ex.Message}");
            }
            return null;
        }

        private static async Task<string> TryUserRegistrationAsync(HttpClient client, CustomWebApplicationFactory<Program> factory)
        {
            try 
            {
                var phoneNumber = "09991234567"; // Different phone for user
                var otpCode = await GetOtpCodeAsync(phoneNumber, factory);
                
                if (string.IsNullOrEmpty(otpCode))
                {
                    Console.WriteLine($"[AuthHelper] Failed to get OTP for user registration");
                    return null;
                }

                var registerDto = new 
                { 
                    PhoneNumber = phoneNumber, 
                    Code = otpCode, 
                    FirstName = "Test", 
                    LastName = "User" 
                };
                var registerResponse = await client.PostAsJsonAsync("/api/auth/register-phone", registerDto);

                if (registerResponse.IsSuccessStatusCode)
                {
                    var content = await registerResponse.Content.ReadAsStringAsync();
                    var token = JsonHelper.GetNestedProperty(content, "data", "accessToken")
                                ?? JsonHelper.GetNestedProperty(content, "accessToken");
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        Console.WriteLine($"[AuthHelper] User Registration Successful: Token retrieved");
                        return token;
                    }
                }
                else 
                {
                    Console.WriteLine($"[AuthHelper] User Registration Failed: {await registerResponse.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthHelper] User Registration Exception: {ex.Message}");
            }
            return null;
        }

        private static async Task<string> GetOtpCodeAsync(string phoneNumber, CustomWebApplicationFactory<Program> factory)
        {
            try
            {
                if (factory != null)
                {
                    var testSmsService = factory.Services.GetService<TestSmsService>();
                    if (testSmsService != null)
                    {
                        // Try to get OTP from database first
                        var dbOtp = await testSmsService.GetLastOtpCodeFromDatabaseAsync(phoneNumber);
                        if (!string.IsNullOrEmpty(dbOtp))
                        {
                            return dbOtp;
                        }
                        
                        // Fallback to static dictionary
                        var staticOtp = TestSmsService.GetLastOtpCode(phoneNumber);
                        if (!string.IsNullOrEmpty(staticOtp))
                        {
                            return staticOtp;
                        }
                    }
                }
                
                // Fallback to hardcoded OTP for testing
                return "123456";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthHelper] GetOtpCodeAsync Exception: {ex.Message}");
                return "123456"; // Fallback
            }
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
                    
                    // Try direct property extraction first (handles camelCase/UpperCase variations)
                    var token = JsonHelper.GetNestedProperty(content, "accessToken")
                                ?? JsonHelper.GetNestedProperty(content, "AccessToken")
                                ?? JsonHelper.GetNestedProperty(content, "access_token")
                                ?? JsonHelper.GetNestedProperty(content, "data", "accessToken")
                                ?? JsonHelper.GetNestedProperty(content, "data", "AccessToken");
                    
                    // Backup: Try direct deserialization to AuthResponseDto
                    if (string.IsNullOrEmpty(token))
                    {
                        var authResponse = JsonHelper.Deserialize<AuthResponseDto>(content);
                        if (authResponse != null && !string.IsNullOrEmpty(authResponse.AccessToken))
                        {
                            token = authResponse.AccessToken;
                            Console.WriteLine($"[AuthHelper] Token extracted via direct deserialization");
                        }
                    }
                    
                    // Also check if wrapped in Result<T> structure
                    if (string.IsNullOrEmpty(token))
                    {
                        var dataElement = JsonHelper.GetNestedProperty(content, "data");
                        if (!string.IsNullOrEmpty(dataElement))
                        {
                            var nestedAuthResponse = JsonHelper.Deserialize<AuthResponseDto>(dataElement);
                            if (nestedAuthResponse != null && !string.IsNullOrEmpty(nestedAuthResponse.AccessToken))
                            {
                                token = nestedAuthResponse.AccessToken;
                                Console.WriteLine($"[AuthHelper] Token extracted from Result wrapper");
                            }
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        Console.WriteLine($"[AuthHelper] Hardcoded Admin Login Successful: Token retrieved ({token.Length} chars)");
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
                Console.WriteLine($"[AuthHelper] Stack Trace: {ex.StackTrace}");
            }
            return null;
        }
    }
}

