// Configuration for Online Shop Frontend
window.config = {
    // API Configuration
    api: {
        baseURL: 'https://your-api-domain.com/api', // تغییر دهید به آدرس API شما
        timeout: 30000,
        retryAttempts: 3
    },
    
    // Authentication
    auth: {
        tokenKey: 'accessToken',
        refreshTokenKey: 'refreshToken',
        userKey: 'userData'
    },
    
    // Pagination
    pagination: {
        defaultPageSize: 10,
        maxPageSize: 100
    },
    
    // File Upload
    upload: {
        maxFileSize: 5 * 1024 * 1024, // 5MB
        allowedTypes: ['image/jpeg', 'image/png', 'image/gif', 'image/webp']
    },
    
    // Local Storage Keys
    storage: {
        lastViewedProducts: 'lastViewedProducts',
        comparisonList: 'comparisonList',
        cartItems: 'cartItems'
    },
    
    // UI Configuration
    ui: {
        toastDuration: 3000,
        loadingText: 'در حال بارگذاری...',
        successText: 'عملیات با موفقیت انجام شد',
        errorText: 'خطا در انجام عملیات'
    }
};
