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
            const response = await this.apiClient.post('/auth/login', {
                email: email,
                password: password
            });

            // Handle different response structures
            let authData;
            if (response.accessToken) {
                // Direct response
                authData = response;
            } else if (response.data && response.data.accessToken) {
                // Wrapped response
                authData = response.data;
            } else {
                throw new Error('Invalid response format');
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
            console.error('Login error:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Send OTP to phone number
     */
    async sendOTP(phone) {
        try {
            const response = await this.apiClient.post('/auth/send-otp', {
                phoneNumber: phone,
                purpose: 'login'
            });

            return {
                success: true,
                message: 'کد تایید ارسال شد'
            };
        } catch (error) {
            console.error('Send OTP error:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
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

            // Handle different response structures
            let authData;
            if (response.accessToken) {
                authData = response;
            } else if (response.data && response.data.accessToken) {
                authData = response.data;
            } else {
                throw new Error('Invalid response format');
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
                    phone: phone,
                    roles: authData.roles || []
                }
            };
        } catch (error) {
            console.error('Verify OTP error:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Register new user
     */
    async register(userData) {
        try {
            const response = await this.apiClient.post('/auth/register', userData);

            // Handle different response structures
            let authData;
            if (response.accessToken) {
                authData = response;
            } else if (response.data && response.data.accessToken) {
                authData = response.data;
            } else {
                throw new Error('Invalid response format');
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
            console.error('Registration error:', error);
            return {
                success: false,
                error: error.message
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
            console.error('Logout error:', error);
        } finally {
            // Always clear tokens locally
            this.apiClient.clearTokens();
            
            // Dispatch logout event
            window.dispatchEvent(new CustomEvent('auth:logout'));
            
            window.location.href = '/login.html';
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
            console.error('Get current user error:', error);
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
            console.error('Change password error:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Forgot password - send reset email
     */
    async forgotPassword(email) {
        try {
            const response = await this.apiClient.post('/auth/forgot-password', {
                email: email
            });

            return {
                success: true,
                message: 'لینک بازیابی رمز عبور به ایمیل شما ارسال شد'
            };
        } catch (error) {
            console.error('Forgot password error:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Reset password with token
     */
    async resetPassword(token, newPassword) {
        try {
            const response = await this.apiClient.post('/auth/reset-password', {
                token: token,
                newPassword: newPassword
            });

            return {
                success: true,
                message: 'رمز عبور با موفقیت بازنشانی شد'
            };
        } catch (error) {
            console.error('Reset password error:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
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
