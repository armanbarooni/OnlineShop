/**
 * API Client for OnlineShop Frontend
 * Handles HTTP requests with authentication, error handling, and interceptors
 */

class ApiClient {
    constructor() {
        this.baseURL = API_CONFIG.baseURL;
        this.timeout = API_CONFIG.timeout;
        this.defaultHeaders = { ...API_CONFIG.defaultHeaders };
    }

    /**
     * Make HTTP request with authentication and error handling
     */
    async request(endpoint, options = {}) {
        const url = endpoint.startsWith('http') ? endpoint : `${this.baseURL}${endpoint}`;
        
        // Prepare headers
        const headers = {
            ...this.defaultHeaders,
            ...options.headers
        };

        // Add authentication header if available
        const authHeader = authManager.getAuthHeader();
        if (authHeader) {
            headers['Authorization'] = authHeader;
        }

        // Prepare request options
        const requestOptions = {
            method: options.method || 'GET',
            headers,
            ...options
        };

        // Add timeout
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), this.timeout);
        requestOptions.signal = controller.signal;

        try {
            if (API_CONFIG.debug) {
                console.log(`[ApiClient] ${requestOptions.method} ${url}`, requestOptions);
            }

            const response = await fetch(url, requestOptions);
            clearTimeout(timeoutId);

            // Handle authentication errors
            if (response.status === 401) {
                // Try to refresh token
                const refreshed = await authManager.refreshToken();
                if (refreshed) {
                    // Retry request with new token
                    const newAuthHeader = authManager.getAuthHeader();
                    if (newAuthHeader) {
                        headers['Authorization'] = newAuthHeader;
                        return await fetch(url, { ...requestOptions, headers });
                    }
                } else {
                    // Redirect to login
                    this.handleUnauthorized();
                    throw new Error('Authentication failed');
                }
            }

            // Parse response
            const contentType = response.headers.get('content-type');
            let data;
            
            if (contentType && contentType.includes('application/json')) {
                data = await response.json();
            } else {
                data = await response.text();
            }

            if (API_CONFIG.debug) {
                console.log(`[ApiClient] Response:`, response.status, data);
            }

            // Handle errors
            if (!response.ok) {
                throw new ApiError(
                    data.message || `HTTP ${response.status}`,
                    response.status,
                    data
                );
            }

            return {
                success: true,
                data,
                status: response.status,
                headers: response.headers
            };

        } catch (error) {
            clearTimeout(timeoutId);
            
            if (error.name === 'AbortError') {
                throw new ApiError('Request timeout', 408);
            }
            
            if (error instanceof ApiError) {
                throw error;
            }
            
            throw new ApiError('Network error: ' + error.message, 0);
        }
    }

    /**
     * GET request
     */
    async get(endpoint, options = {}) {
        return this.request(endpoint, { ...options, method: 'GET' });
    }

    /**
     * POST request
     */
    async post(endpoint, data, options = {}) {
        return this.request(endpoint, {
            ...options,
            method: 'POST',
            body: JSON.stringify(data)
        });
    }

    /**
     * PUT request
     */
    async put(endpoint, data, options = {}) {
        return this.request(endpoint, {
            ...options,
            method: 'PUT',
            body: JSON.stringify(data)
        });
    }

    /**
     * DELETE request
     */
    async delete(endpoint, options = {}) {
        return this.request(endpoint, { ...options, method: 'DELETE' });
    }

    /**
     * PATCH request
     */
    async patch(endpoint, data, options = {}) {
        return this.request(endpoint, {
            ...options,
            method: 'PATCH',
            body: JSON.stringify(data)
        });
    }

    /**
     * Handle unauthorized access
     */
    handleUnauthorized() {
        authManager.logout();
        
        // Redirect to login page
        const currentPath = window.location.pathname;
        if (!currentPath.includes('/login')) {
            window.location.href = '/fa/login.html';
        }
    }

    /**
     * Upload file
     */
    async uploadFile(endpoint, file, options = {}) {
        const formData = new FormData();
        formData.append('file', file);

        const headers = { ...options.headers };
        delete headers['Content-Type']; // Let browser set it for FormData

        return this.request(endpoint, {
            ...options,
            method: 'POST',
            body: formData,
            headers
        });
    }

    /**
     * Download file
     */
    async downloadFile(endpoint, filename, options = {}) {
        const response = await this.request(endpoint, {
            ...options,
            method: 'GET'
        });

        if (response.success) {
            const blob = new Blob([response.data]);
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = filename;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            document.body.removeChild(a);
        }

        return response;
    }
}

/**
 * Custom API Error class
 */
class ApiError extends Error {
    constructor(message, status, data = null) {
        super(message);
        this.name = 'ApiError';
        this.status = status;
        this.data = data;
    }
}

/**
 * Utility functions for common API operations
 */
const ApiUtils = {
    /**
     * Handle API response with loading state
     */
    async withLoading(apiCall, loadingCallback) {
        try {
            if (loadingCallback) loadingCallback(true);
            const result = await apiCall();
            return result;
        } finally {
            if (loadingCallback) loadingCallback(false);
        }
    },

    /**
     * Retry failed requests
     */
    async withRetry(apiCall, maxRetries = 3, delay = 1000) {
        let lastError;
        
        for (let i = 0; i < maxRetries; i++) {
            try {
                return await apiCall();
            } catch (error) {
                lastError = error;
                
                if (i < maxRetries - 1) {
                    await new Promise(resolve => setTimeout(resolve, delay * Math.pow(2, i)));
                }
            }
        }
        
        throw lastError;
    },

    /**
     * Debounce API calls
     */
    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
};

// Create global instance
window.apiClient = new ApiClient();
window.ApiError = ApiError;
window.ApiUtils = ApiUtils;

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { ApiClient, ApiError, ApiUtils };
}
