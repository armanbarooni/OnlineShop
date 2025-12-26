/**
 * API Configuration for OnlineShop Frontend
 * Handles environment detection and API endpoints
 */

const API_CONFIG = {
    // Base URL detection based on environment
    baseURL: (() => {
        const hostname = window.location.hostname;
        const protocol = window.location.protocol;
        const port = window.location.port;
        
        // Development environment
        if (hostname === 'localhost' || hostname === '127.0.0.1') {
            return port === '7001' || port === '7000' 
                ? `${protocol}//${hostname}:${port}`
                : `${protocol}//${hostname}:7001`; // Default to 7001 for API
        }
        
        // Production environment: استفاده از همان hostname (نه subdomain)
        // این باعث می‌شود frontend و API از همان دامنه استفاده کنند
        return `${protocol}//${hostname}${port ? ':' + port : ''}`;
    })(),
    
    // Request timeout in milliseconds
    timeout: 30000,
    
    // API endpoints
    endpoints: {
        auth: {
            login: '/api/auth/login',
            loginPhone: '/api/auth/login-phone',
            sendOtp: '/api/auth/send-otp',
            register: '/api/auth/register-phone',
            refresh: '/api/auth/refresh',
            logout: '/api/auth/logout'
        },
        user: {
            profile: '/api/user/profile',
            addresses: '/api/useraddress',
            orders: '/api/userorder',
            wishlist: '/api/wishlist',
            cart: '/api/cart'
        },
        products: {
            list: '/api/product',
            search: '/api/product/search',
            details: '/api/product',
            categories: '/api/productcategory',
            brands: '/api/brand'
        },
        orders: {
            create: '/api/userorder',
            list: '/api/userorder',
            details: '/api/userorder'
        },
        payments: {
            create: '/api/payment',
            process: '/api/payment/process',
            callback: '/api/payment/callback'
        }
    },
    
    // Default headers
    defaultHeaders: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
    },
    
    // Environment detection
    isDevelopment: () => {
        return window.location.hostname === 'localhost' || 
               window.location.hostname === '127.0.0.1';
    },
    
    isProduction: () => {
        return !API_CONFIG.isDevelopment();
    },
    
    // Debug mode (only in development)
    debug: API_CONFIG?.isDevelopment?.() || false
};

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = API_CONFIG;
}

// Make available globally
window.API_CONFIG = API_CONFIG;
