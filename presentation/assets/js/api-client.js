// API Client for Online Shop
class ApiClient {
    constructor() {
        this.baseURL = window.config?.api?.baseURL || 'http://localhost:5000/api';
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
                    const retryResponse = await fetch(url, config);
                    const retryData = await retryResponse.json();
                    
                    if (!retryResponse.ok) {
                        // Parse error message from different response structures
                        let errorMessage = '';
                        if (typeof retryData === 'string') {
                            errorMessage = retryData;
                        } else if (retryData?.message) {
                            errorMessage = retryData.message;
                        } else if (Array.isArray(retryData)) {
                            errorMessage = retryData.join(', ');
                        } else if (retryData?.errors && Array.isArray(retryData.errors)) {
                            errorMessage = retryData.errors.join(', ');
                        } else if (retryData?.title) {
                            errorMessage = retryData.title;
                        } else {
                            errorMessage = `HTTP error! status: ${retryResponse.status}`;
                        }
                        throw new Error(errorMessage);
                    }

                    // Return data directly for auth endpoints, wrapped for others
                    if (endpoint.startsWith('/auth/')) {
                        return retryData;
                    }

                    return {
                        success: true,
                        data: retryData,
                        status: retryResponse.status
                    };
                }
            }

            // Parse response - handle non-JSON responses
            let data;
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                try {
                    data = await response.json();
                } catch (e) {
                    // If JSON parsing fails, treat as text
                    const text = await response.text();
                    data = text || {};
                }
            } else {
                const text = await response.text();
                data = text || {};
            }
            
            if (!response.ok) {
                // Parse error message from different response structures
                let errorMessage = '';
                
                if (typeof data === 'string') {
                    errorMessage = data;
                } else if (data?.message) {
                    errorMessage = data.message;
                } else if (Array.isArray(data)) {
                    errorMessage = data.join(', ');
                } else if (data?.errors && Array.isArray(data.errors)) {
                    errorMessage = data.errors.join(', ');
                } else if (data?.title) {
                    errorMessage = data.title;
                } else if (typeof data === 'object' && Object.keys(data).length === 0) {
                    errorMessage = `HTTP error! status: ${response.status}`;
                } else {
                    // Try to extract any error information
                    const errorText = JSON.stringify(data);
                    errorMessage = errorText.length > 200 ? `HTTP error! status: ${response.status}` : errorText;
                }
                
                throw new Error(errorMessage);
            }

            // Return data directly for auth endpoints, wrapped for others
            if (endpoint.startsWith('/auth/')) {
                return data;
            }

            return {
                success: true,
                data: data,
                status: response.status
            };
        } catch (error) {
            console.error('API request failed:', error);
            // Handle network errors (Failed to fetch)
            // In some browsers, the error might be a TypeError or have different messages
            if (error instanceof TypeError && (
                error.message === 'Failed to fetch' || 
                error.message.includes('fetch') ||
                error.message.includes('NetworkError') ||
                error.message.includes('Network error')
            )) {
                throw new Error('خطا در اتصال به سرور. لطفا بررسی کنید که سرور در حال اجرا است و آدرس درست است.');
            }
            // Handle other network-related errors
            if (error.message && (
                error.message.includes('ERR_') ||
                error.message.includes('net::') ||
                error.message.includes('Network request failed')
            )) {
                throw new Error('خطا در اتصال به سرور. لطفا بررسی کنید که سرور در حال اجرا است و آدرس درست است.');
            }
            // Re-throw error so auth-service can handle it properly
            throw error;
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
                this.refreshToken = data.refreshToken;
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

            // Parse response - handle non-JSON responses
            let data;
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                try {
                    data = await response.json();
                } catch (e) {
                    const text = await response.text();
                    data = text || {};
                }
            } else {
                const text = await response.text();
                data = text || {};
            }
            
            if (!response.ok) {
                // Parse error message from different response structures
                let errorMessage = '';
                
                if (typeof data === 'string') {
                    errorMessage = data;
                } else if (data?.message) {
                    errorMessage = data.message;
                } else if (Array.isArray(data)) {
                    errorMessage = data.join(', ');
                } else if (data?.errors && Array.isArray(data.errors)) {
                    errorMessage = data.errors.join(', ');
                } else if (data?.title) {
                    errorMessage = data.title;
                } else {
                    errorMessage = `HTTP error! status: ${response.status}`;
                }
                
                throw new Error(errorMessage);
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

    // Set tokens (plural) - for compatibility with auth-service
    setTokens(accessToken, refreshToken = null) {
        this.setToken(accessToken, refreshToken);
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

    // Handle API errors - compatible with fetch API
    handleError(error) {
        // For fetch API, error is typically an Error object or a thrown value
        if (error instanceof Error) {
            return error.message || 'خطای نامشخص';
        } else if (typeof error === 'string') {
            return error;
        } else if (error && typeof error === 'object') {
            // Handle structured error objects
            if (error.message) {
                return error.message;
            } else if (error.errors && Array.isArray(error.errors)) {
                return error.errors.join(', ');
            } else {
                return 'خطای نامشخص';
            }
        } else {
            return 'خطای نامشخص';
        }
    }
}

// Create global instance
window.apiClient = new ApiClient();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ApiClient;
}
