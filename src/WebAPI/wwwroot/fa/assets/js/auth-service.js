/**
 * Authentication Service for OnlineShop Frontend
 * Handles login, registration, and user management
 */

class AuthService {
    constructor() {
        this.apiClient = window.apiClient;
    }

    /**
     * Login with email and password
     */
    async loginWithPassword(email, password) {
        try {
            // Validate input
            if (!email || !email.trim()) {
                return {
                    success: false,
                    error: 'ایمیل یا نام کاربری الزامی است'
                };
            }

            if (!password) {
                return {
                    success: false,
                    error: 'رمز عبور الزامی است'
                };
            }

            const response = await this.apiClient.post('/auth/login', {
                email: email.trim(),
                password: password
            });

            // Handle response structure - now API client returns data directly for auth endpoints
            let authData = response;

            // Validate that we received tokens
            if (!authData || !authData.accessToken) {
                // Check if response contains error message
                if (authData?.message) {
                    return {
                        success: false,
                        error: authData.message
                    };
                }
                return {
                    success: false,
                    error: 'خطا در دریافت اطلاعات احراز هویت'
                };
            }

            // Store tokens
            this.apiClient.setTokens(authData.accessToken, authData.refreshToken);

            // Dispatch login event
            window.dispatchEvent(new CustomEvent('auth:login', {
                detail: { user: authData }
            }));

            return {
                success: true,
                user: {
                    email: authData.email,
                    roles: authData.roles || []
                }
            };
        } catch (error) {
            window.logger.error('Login error:', error);
            // api-client throws errors, so we need to handle them properly
            const errorMessage = error instanceof Error ? error.message : (error?.message || 'خطا در ورود');
            return {
                success: false,
                error: errorMessage
            };
        }
    }

    /**
     * Send OTP to phone number
     */
    async sendOTP(phone, purpose = 'login') {
        try {
            const response = await this.apiClient.post('/auth/send-otp', {
                phoneNumber: phone,
                purpose: purpose
            });

            return {
                success: true,
                message: 'کد تایید ارسال شد'
            };
        } catch (error) {
            window.logger.error('Send OTP error:', error);
            return {
                success: false,
                error: error.message || 'خطا در اتصال به سرور'
            };
        }
    }

    /**
     * Verify OTP and login
     */
    async verifyOTP(phone, code) {
        try {
            const response = await this.apiClient.post('/auth/verify-otp', {
                phoneNumber: phone,
                code: code
            });

            // Handle response structure - now API client returns data directly for auth endpoints
            let authData = response;

            // Store tokens
            this.apiClient.setTokens(authData.accessToken, authData.refreshToken);

            // Dispatch login event
            window.dispatchEvent(new CustomEvent('auth:login', {
                detail: { user: authData }
            }));

            return {
                success: true,
                user: {
                    email: authData.email,
                    phone: phone,
                    roles: authData.roles || []
                }
            };
        } catch (error) {
            window.logger.error('Verify OTP error:', error);
            return {
                success: false,
                error: error.message || 'خطا در اتصال به سرور'
            };
        }
    }

    /**
     * Register new user
     */
    async register(userData) {
        try {
            // Validate email - don't create fake email
            if (!userData.email || !userData.email.trim()) {
                return {
                    success: false,
                    error: 'ایمیل الزامی است'
                };
            }

            // Validate email format
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(userData.email.trim())) {
                return {
                    success: false,
                    error: 'فرمت ایمیل معتبر نیست'
                };
            }

            // Use standard register endpoint
            const response = await this.apiClient.post('/auth/register', {
                firstName: userData.firstName,
                lastName: userData.lastName,
                phoneNumber: userData.phone,
                email: userData.email.trim(), // Use trimmed email - no fake email creation
                password: userData.password,
                confirmPassword: userData.password
            });

            // Handle response structure - now API client returns data directly for auth endpoints
            let authData = response;

            // Validate that we received tokens - if not, response might be an error
            if (!authData || !authData.accessToken) {
                // Check if response contains error message
                if (authData?.message) {
                    return {
                        success: false,
                        error: authData.message
                    };
                }
                return {
                    success: false,
                    error: 'خطا در دریافت اطلاعات احراز هویت'
                };
            }

            // Store tokens
            this.apiClient.setTokens(authData.accessToken, authData.refreshToken);

            return {
                success: true,
                user: {
                    email: authData.email,
                    roles: authData.roles || []
                }
            };
        } catch (error) {
            window.logger.error('Registration error:', error);
            // api-client throws errors, so we need to handle them properly
            let errorMessage = 'خطا در ثبت‌نام';

            if (error instanceof Error) {
                errorMessage = error.message;

                // Parse specific error messages
                if (errorMessage.includes('EMAIL_EXISTS') || errorMessage.includes('ایمیل قبلاً') || errorMessage.includes('کاربری با این ایمیل')) {
                    errorMessage = 'کاربری با این ایمیل قبلاً ثبت‌نام کرده است';
                } else if (errorMessage.includes('PHONE_EXISTS') || errorMessage.includes('شماره تلفن') || errorMessage.includes('شماره موبایل')) {
                    errorMessage = 'کاربری با این شماره موبایل قبلاً ثبت‌نام کرده است';
                } else if (errorMessage.includes('VALIDATION_ERROR') || errorMessage.includes('اعتبارسنجی')) {
                    errorMessage = errorMessage.replace('VALIDATION_ERROR:', '').trim();
                } else if (errorMessage.includes('405') || errorMessage.includes('Method Not Allowed')) {
                    errorMessage = 'خطا در ارسال درخواست. لطفاً دوباره تلاش کنید.';
                }
            } else if (error?.message) {
                errorMessage = error.message;
            }

            return {
                success: false,
                error: errorMessage
            };
        }
    }

    /**
     * Logout user
     */
    async logout() {
        try {
            // Call logout endpoint if authenticated
            if (this.apiClient.isAuthenticated()) {
                await this.apiClient.post('/auth/logout', {
                    refreshToken: this.apiClient.refreshToken
                });
            }
        } catch (error) {
            window.logger.error('Logout error:', error);
        } finally {
            // Always clear tokens locally
            this.apiClient.clearTokens();

            // Dispatch logout event
            window.dispatchEvent(new CustomEvent('auth:logout'));

            window.location.href = 'login.html';
        }
    }

    /**
     * Get current user information
     */
    async getCurrentUser() {
        try {
            if (!this.apiClient.isAuthenticated()) {
                return null;
            }

            const response = await this.apiClient.get('/auth/me');
            return response.data || response;
        } catch (error) {
            window.logger.error('Get current user error:', error);
            return null;
        }
    }

    /**
     * Check if user is authenticated
     */
    isAuthenticated() {
        return this.apiClient.isAuthenticated();
    }

    /**
     * Refresh access token
     */
    async refreshToken() {
        return await this.apiClient.refreshAccessToken();
    }

    /**
     * Change password
     */
    async changePassword(currentPassword, newPassword) {
        try {
            const response = await this.apiClient.post('/auth/change-password', {
                currentPassword: currentPassword,
                newPassword: newPassword
            });

            return {
                success: true,
                message: 'رمز عبور با موفقیت تغییر یافت'
            };
        } catch (error) {
            window.logger.error('Change password error:', error);
            return {
                success: false,
                error: error.message || 'خطا در اتصال به سرور'
            };
        }
    }

    /**
     * Forgot password - send OTP to phone number
     */
    async forgotPassword(phoneNumber) {
        try {
            const response = await this.apiClient.post('/auth/forgot-password', {
                phoneNumber: phoneNumber
            });

            return {
                success: true,
                message: 'کد بازیابی رمز عبور به شماره موبایل شما ارسال شد'
            };
        } catch (error) {
            window.logger.error('Forgot password error:', error);
            return {
                success: false,
                error: error.message || 'خطا در اتصال به سرور'
            };
        }
    }

    /**
     * Reset password with OTP
     */
    async resetPassword(phoneNumber, otpCode, newPassword) {
        try {
            const response = await this.apiClient.post('/auth/reset-password', {
                phoneNumber: phoneNumber,
                otpCode: otpCode,
                newPassword: newPassword
            });

            return {
                success: true,
                message: 'رمز عبور با موفقیت بازنشانی شد'
            };
        } catch (error) {
            window.logger.error('Reset password error:', error);
            return {
                success: false,
                error: error.message || 'خطا در اتصال به سرور'
            };
        }
    }
}

// Create global instance
window.authService = new AuthService();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AuthService;
}

