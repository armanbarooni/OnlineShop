// API Client for Online Shop
class ApiClient {
    constructor() {
        this.baseURL = 'https://your-api-domain.com/api'; // تغییر دهید به آدرس API شما
        this.token = localStorage.getItem('accessToken');
        this.refreshToken = localStorage.getItem('refreshToken');
    }

    // Set base URL
    setBaseURL(url) {
        this.baseURL = url;
    }

    // Get headers
    getHeaders() {
        const headers = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        };

        if (this.token) {
            headers['Authorization'] = `Bearer ${this.token}`;
        }

        return headers;
    }

    // Make HTTP request
    async request(endpoint, options = {}) {
        const url = `${this.baseURL}${endpoint}`;
        const config = {
            headers: this.getHeaders(),
            ...options
        };

        try {
            const response = await fetch(url, config);
            
            // Handle token refresh
            if (response.status === 401 && this.refreshToken) {
                const refreshed = await this.refreshAccessToken();
                if (refreshed) {
                    // Retry the original request
                    config.headers = this.getHeaders();
                    return await fetch(url, config);
                }
            }

            const data = await response.json();
            
            if (!response.ok) {
                throw new Error(data.message || `HTTP error! status: ${response.status}`);
            }

            return {
                success: true,
                data: data,
                status: response.status
            };
        } catch (error) {
            console.error('API request failed:', error);
            return {
                success: false,
                error: error.message,
                status: 0
            };
        }
    }

    // Refresh access token
    async refreshAccessToken() {
        if (!this.refreshToken) {
            return false;
        }

        try {
            const response = await fetch(`${this.baseURL}/auth/refresh`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    refreshToken: this.refreshToken
                })
            });

            if (response.ok) {
                const data = await response.json();
                this.token = data.accessToken;
                localStorage.setItem('accessToken', data.accessToken);
                localStorage.setItem('refreshToken', data.refreshToken);
                return true;
            } else {
                // Refresh failed, redirect to login
                this.logout();
                return false;
            }
        } catch (error) {
            console.error('Token refresh failed:', error);
            this.logout();
            return false;
        }
    }

    // GET request
    async get(endpoint) {
        return await this.request(endpoint, { method: 'GET' });
    }

    // POST request
    async post(endpoint, data) {
        return await this.request(endpoint, {
            method: 'POST',
            body: JSON.stringify(data)
        });
    }

    // PUT request
    async put(endpoint, data) {
        return await this.request(endpoint, {
            method: 'PUT',
            body: JSON.stringify(data)
        });
    }

    // DELETE request
    async delete(endpoint) {
        return await this.request(endpoint, { method: 'DELETE' });
    }

    // Upload file
    async uploadFile(endpoint, file, additionalData = {}) {
        const formData = new FormData();
        formData.append('file', file);
        
        Object.keys(additionalData).forEach(key => {
            formData.append(key, additionalData[key]);
        });

        const url = `${this.baseURL}${endpoint}`;
        const headers = {};

        if (this.token) {
            headers['Authorization'] = `Bearer ${this.token}`;
        }

        try {
            const response = await fetch(url, {
                method: 'POST',
                headers: headers,
                body: formData
            });

            const data = await response.json();
            
            if (!response.ok) {
                throw new Error(data.message || `HTTP error! status: ${response.status}`);
            }

            return {
                success: true,
                data: data,
                status: response.status
            };
        } catch (error) {
            console.error('File upload failed:', error);
            return {
                success: false,
                error: error.message,
                status: 0
            };
        }
    }

    // Set token
    setToken(token, refreshToken = null) {
        this.token = token;
        localStorage.setItem('accessToken', token);
        
        if (refreshToken) {
            this.refreshToken = refreshToken;
            localStorage.setItem('refreshToken', refreshToken);
        }
    }

    // Clear tokens
    clearTokens() {
        this.token = null;
        this.refreshToken = null;
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
    }

    // Logout
    logout() {
        this.clearTokens();
        window.location.href = '/login.html';
    }

    // Check if user is authenticated
    isAuthenticated() {
        return !!this.token;
    }
}

// Create global instance
window.apiClient = new ApiClient();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ApiClient;
}
