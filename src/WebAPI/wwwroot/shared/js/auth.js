/**
 * Authentication Manager for OnlineShop Frontend
 * Handles JWT token storage, refresh, and authentication state
 */

class AuthManager {
    constructor() {
        this.tokenKey = 'onlineshop_access_token';
        this.refreshTokenKey = 'onlineshop_refresh_token';
        this.userKey = 'onlineshop_user';
        this.tokenRefreshTimer = null;
        this.refreshThreshold = 5 * 60 * 1000; // 5 minutes before expiry
    }

    /**
     * Store authentication tokens
     */
    setTokens(accessToken, refreshToken, user = null) {
        localStorage.setItem(this.tokenKey, accessToken);
        if (refreshToken) {
            localStorage.setItem(this.refreshTokenKey, refreshToken);
        }
        if (user) {
            localStorage.setItem(this.userKey, JSON.stringify(user));
        }
        
        // Start token refresh timer
        this.startTokenRefreshTimer();
        
        if (API_CONFIG.debug) {
            console.log('[AuthManager] Tokens stored successfully');
        }
    }

    /**
     * Get access token
     */
    getAccessToken() {
        return localStorage.getItem(this.tokenKey);
    }

    /**
     * Get refresh token
     */
    getRefreshToken() {
        return localStorage.getItem(this.refreshTokenKey);
    }

    /**
     * Get current user
     */
    getCurrentUser() {
        const userStr = localStorage.getItem(this.userKey);
        return userStr ? JSON.parse(userStr) : null;
    }

    /**
     * Check if user is authenticated
     */
    isAuthenticated() {
        const token = this.getAccessToken();
        if (!token) return false;

        try {
            // Check if token is expired
            const payload = this.parseJwtPayload(token);
            const now = Math.floor(Date.now() / 1000);
            return payload.exp > now;
        } catch (error) {
            if (API_CONFIG.debug) {
                console.error('[AuthManager] Token validation error:', error);
            }
            return false;
        }
    }

    /**
     * Parse JWT payload without verification
     */
    parseJwtPayload(token) {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        return JSON.parse(jsonPayload);
    }

    /**
     * Get token expiry time
     */
    getTokenExpiry() {
        const token = this.getAccessToken();
        if (!token) return null;

        try {
            const payload = this.parseJwtPayload(token);
            return new Date(payload.exp * 1000);
        } catch (error) {
            return null;
        }
    }

    /**
     * Check if token needs refresh
     */
    needsRefresh() {
        const expiry = this.getTokenExpiry();
        if (!expiry) return false;

        const now = new Date();
        const timeUntilExpiry = expiry.getTime() - now.getTime();
        return timeUntilExpiry < this.refreshThreshold;
    }

    /**
     * Start token refresh timer
     */
    startTokenRefreshTimer() {
        this.clearTokenRefreshTimer();
        
        const checkInterval = 60 * 1000; // Check every minute
        
        this.tokenRefreshTimer = setInterval(() => {
            if (this.isAuthenticated() && this.needsRefresh()) {
                this.refreshToken();
            }
        }, checkInterval);
    }

    /**
     * Clear token refresh timer
     */
    clearTokenRefreshTimer() {
        if (this.tokenRefreshTimer) {
            clearInterval(this.tokenRefreshTimer);
            this.tokenRefreshTimer = null;
        }
    }

    /**
     * Refresh access token
     */
    async refreshToken() {
        const refreshToken = this.getRefreshToken();
        if (!refreshToken) {
            this.logout();
            return false;
        }

        try {
            const response = await fetch(`${API_CONFIG.baseURL}${API_CONFIG.endpoints.auth.refresh}`, {
                method: 'POST',
                headers: API_CONFIG.defaultHeaders,
                body: JSON.stringify({ refreshToken })
            });

            if (response.ok) {
                const data = await response.json();
                this.setTokens(data.accessToken, data.refreshToken, data.user);
                return true;
            } else {
                this.logout();
                return false;
            }
        } catch (error) {
            if (API_CONFIG.debug) {
                console.error('[AuthManager] Token refresh failed:', error);
            }
            this.logout();
            return false;
        }
    }

    /**
     * Login with email and password
     */
    async loginWithEmail(email, password) {
        try {
            const response = await fetch(`${API_CONFIG.baseURL}${API_CONFIG.endpoints.auth.login}`, {
                method: 'POST',
                headers: API_CONFIG.defaultHeaders,
                body: JSON.stringify({ email, password })
            });

            const data = await response.json();
            
            if (response.ok) {
                this.setTokens(data.accessToken, data.refreshToken, {
                    email: data.email,
                    roles: data.roles
                });
                return { success: true, data };
            } else {
                return { success: false, error: data.message || 'Login failed' };
            }
        } catch (error) {
            return { success: false, error: 'Network error: ' + error.message };
        }
    }

    /**
     * Login with phone and OTP
     */
    async loginWithPhone(phone, otp) {
        try {
            const response = await fetch(`${API_CONFIG.baseURL}${API_CONFIG.endpoints.auth.loginPhone}`, {
                method: 'POST',
                headers: API_CONFIG.defaultHeaders,
                body: JSON.stringify({ phone, otp })
            });

            const data = await response.json();
            
            if (response.ok) {
                this.setTokens(data.accessToken, data.refreshToken, {
                    phone: data.phone,
                    roles: data.roles
                });
                return { success: true, data };
            } else {
                return { success: false, error: data.message || 'Login failed' };
            }
        } catch (error) {
            return { success: false, error: 'Network error: ' + error.message };
        }
    }

    /**
     * Send OTP to phone
     */
    async sendOtp(phone) {
        try {
            const response = await fetch(`${API_CONFIG.baseURL}${API_CONFIG.endpoints.auth.sendOtp}`, {
                method: 'POST',
                headers: API_CONFIG.defaultHeaders,
                body: JSON.stringify({ phone })
            });

            const data = await response.json();
            return { success: response.ok, data, error: data.message };
        } catch (error) {
            return { success: false, error: 'Network error: ' + error.message };
        }
    }

    /**
     * Register with phone
     */
    async registerWithPhone(phone, otp, firstName, lastName) {
        try {
            const response = await fetch(`${API_CONFIG.baseURL}${API_CONFIG.endpoints.auth.register}`, {
                method: 'POST',
                headers: API_CONFIG.defaultHeaders,
                body: JSON.stringify({ phone, otp, firstName, lastName })
            });

            const data = await response.json();
            
            if (response.ok) {
                this.setTokens(data.accessToken, data.refreshToken, {
                    phone: data.phone,
                    roles: data.roles
                });
                return { success: true, data };
            } else {
                return { success: false, error: data.message || 'Registration failed' };
            }
        } catch (error) {
            return { success: false, error: 'Network error: ' + error.message };
        }
    }

    /**
     * Logout user
     */
    logout() {
        localStorage.removeItem(this.tokenKey);
        localStorage.removeItem(this.refreshTokenKey);
        localStorage.removeItem(this.userKey);
        this.clearTokenRefreshTimer();
        
        if (API_CONFIG.debug) {
            console.log('[AuthManager] User logged out');
        }
    }

    /**
     * Get authorization header
     */
    getAuthHeader() {
        const token = this.getAccessToken();
        return token ? `Bearer ${token}` : null;
    }
}

// Create global instance
window.authManager = new AuthManager();

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AuthManager;
}
