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
            let errorMessage = error instanceof Error ? error.message : (error?.message || 'خطا در ورود');
            
            // Try to extract errorMessage from JSON string if error.message is a serialized JSON
            try {
                const parsed = JSON.parse(errorMessage);
                if (parsed && (parsed.errorMessage || parsed.message)) {
                    errorMessage = parsed.errorMessage || parsed.message;
                }
            } catch (parseErr) {
                // If it's not JSON, keep original message
            }
            
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
            
            // Try to extract a friendly error message from possible JSON error payload
            let message = error && error.message ? error.message : 'خطا در اتصال به سرور';
            try {
                // Some backends return serialized JSON as error.message, e.g.:
                // {"isSuccess":false,"data":null,"errorMessage":"کد تایید نامعتبر یا منقضی شده است",...}
                const parsed = JSON.parse(message);
                if (parsed && (parsed.errorMessage || parsed.message)) {
                    message = parsed.errorMessage || parsed.message;
                }
            } catch (parseErr) {
                // If it's not JSON, keep original message
            }
            
            return {
                success: false,
                error: message
            };
        }
    }

    /**
     * Verify OTP only (without login) - for registration flow
     */
    async verifyOTPOnly(phone, code) {
        try {
            const response = await this.apiClient.post('/auth/verify-otp', {
                phoneNumber: phone,
                code: code
            });

            // Handle response structure
            if (response && (response.isSuccess || response.success !== false)) {
                return {
                    success: true,
                    message: 'کد تایید معتبر است'
                };
            }

            return {
                success: false,
                error: response?.errorMessage || response?.message || 'کد تایید نامعتبر است'
            };
        } catch (error) {
            window.logger.error('Verify OTP error:', error);

            // Try to extract a friendly error message from possible JSON error payload
            let message = error && error.message ? error.message : 'خطا در اتصال به سرور';
            try {
                // Some backends return serialized JSON as error.message
                const parsed = JSON.parse(message);
                if (parsed && (parsed.errorMessage || parsed.message)) {
                    message = parsed.errorMessage || parsed.message;
                }
            } catch (parseErr) {
                // If it's not JSON, keep original message
            }

            return {
                success: false,
                error: message
            };
        }
    }

    /**
     * Verify OTP and login
     */
    async verifyOTP(phone, code) {
        try {
            const response = await this.apiClient.post('/auth/login-phone', {
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

            // Try to extract a friendly error message from possible JSON error payload
            let message = error && error.message ? error.message : 'خطا در اتصال به سرور';
            try {
                // Some backends return serialized JSON as error.message, e.g.:
                // {"isSuccess":false,"data":null,"errorMessage":"کد تایید نامعتبر یا منقضی شده است",...}
                const parsed = JSON.parse(message);
                if (parsed && (parsed.errorMessage || parsed.message)) {
                    message = parsed.errorMessage || parsed.message;
                }
            } catch (parseErr) {
                // If it's not JSON, keep original message
            }

            return {
                success: false,
                error: message
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

                // Try to extract errorMessage from JSON string if error.message is a serialized JSON
                try {
                    const parsed = JSON.parse(errorMessage);
                    if (parsed && (parsed.errorMessage || parsed.message)) {
                        errorMessage = parsed.errorMessage || parsed.message;
                    }
                } catch (parseErr) {
                    // If it's not JSON, continue with original message
                }

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
     * Register new user with phone number and OTP
     */
    async registerWithPhone(userData) {
        try {
            // Validate required fields
            if (!userData.phone || !userData.phone.trim()) {
                return {
                    success: false,
                    error: 'شماره موبایل الزامی است'
                };
            }

            if (!userData.otp || userData.otp.length !== 6) {
                return {
                    success: false,
                    error: 'کد تایید الزامی است'
                };
            }

            if (!userData.firstName || !userData.firstName.trim()) {
                return {
                    success: false,
                    error: 'نام الزامی است'
                };
            }

            if (!userData.lastName || !userData.lastName.trim()) {
                return {
                    success: false,
                    error: 'نام خانوادگی الزامی است'
                };
            }

            if (!userData.password) {
                return {
                    success: false,
                    error: 'رمز عبور الزامی است'
                };
            }

            // Use register-phone endpoint
            const response = await this.apiClient.post('/auth/register-phone', {
                phoneNumber: userData.phone.trim(),
                code: userData.otp,
                firstName: userData.firstName.trim(),
                lastName: userData.lastName.trim(),
                password: userData.password
            });

            // Handle response structure
            let authData = response;

            // Validate that we received tokens
            if (!authData || !authData.accessToken) {
                // Check if response contains error message
                if (authData?.errorMessage || authData?.message) {
                    return {
                        success: false,
                        error: authData.errorMessage || authData.message
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
                    phone: userData.phone,
                    roles: authData.roles || []
                }
            };
        } catch (error) {
            window.logger.error('Registration with phone error:', error);
            let errorMessage = 'خطا در ثبت‌نام';

            if (error instanceof Error) {
                errorMessage = error.message;

                // Try to extract errorMessage from JSON string if error.message is a serialized JSON
                try {
                    const parsed = JSON.parse(errorMessage);
                    if (parsed && (parsed.errorMessage || parsed.message)) {
                        errorMessage = parsed.errorMessage || parsed.message;
                    }
                } catch (parseErr) {
                    // If it's not JSON, continue with original message
                }

                // Parse specific error messages
                if (errorMessage.includes('PHONE_EXISTS') || errorMessage.includes('شماره موبایل')) {
                    errorMessage = 'کاربری با این شماره موبایل قبلاً ثبت‌نام کرده است';
                } else if (errorMessage.includes('OTP') || errorMessage.includes('کد تایید')) {
                    errorMessage = 'کد تایید نامعتبر یا منقضی شده است';
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
            let errorMessage = error.message || 'خطا در اتصال به سرور';
            
            // Try to extract errorMessage from JSON string if error.message is a serialized JSON
            try {
                const parsed = JSON.parse(errorMessage);
                if (parsed && (parsed.errorMessage || parsed.message)) {
                    errorMessage = parsed.errorMessage || parsed.message;
                }
            } catch (parseErr) {
                // If it's not JSON, keep original message
            }
            
            return {
                success: false,
                error: errorMessage
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
            let errorMessage = error.message || 'خطا در اتصال به سرور';
            
            // Try to extract errorMessage from JSON string if error.message is a serialized JSON
            try {
                const parsed = JSON.parse(errorMessage);
                if (parsed && (parsed.errorMessage || parsed.message)) {
                    errorMessage = parsed.errorMessage || parsed.message;
                }
            } catch (parseErr) {
                // If it's not JSON, keep original message
            }
            
            return {
                success: false,
                error: errorMessage
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
            let errorMessage = error.message || 'خطا در اتصال به سرور';
            
            // Try to extract errorMessage from JSON string if error.message is a serialized JSON
            try {
                const parsed = JSON.parse(errorMessage);
                if (parsed && (parsed.errorMessage || parsed.message)) {
                    errorMessage = parsed.errorMessage || parsed.message;
                }
            } catch (parseErr) {
                // If it's not JSON, keep original message
            }
            
            return {
                success: false,
                error: errorMessage
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

