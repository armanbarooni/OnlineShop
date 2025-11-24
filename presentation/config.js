(function () {
    const hostname = window.location?.hostname ?? 'localhost';

    const detectEnvironment = (host) => {
        const normalizedHost = (host || '').toLowerCase();
        if (!normalizedHost || normalizedHost === 'localhost' || normalizedHost === '127.0.0.1') {
            return 'development';
        }
        if (normalizedHost.includes('staging') || normalizedHost.includes('test')) {
            return 'staging';
        }
        return 'production';
    };

    const environmentName = detectEnvironment(hostname);

    const resolveApiBaseUrl = () => {
        if (window.__API_BASE_URL__) {
            return window.__API_BASE_URL__;
        }

        const metaApiBase = document.querySelector('meta[name="api-base-url"]');
        if (metaApiBase?.content) {
            return metaApiBase.content;
        }

        const normalizedOrigin = (window.location?.origin || '').replace(/\/$/, '');
        
        // Always use current origin + /api
        if (normalizedOrigin) {
            return `${normalizedOrigin}/api`;
        }

        // Fallback for development
        if (environmentName === 'development') {
            return 'http://localhost:5000/api';
        }

        return 'https://api.example.com/api';
    };

    window.config = {
        environment: {
            name: environmentName,
            hostname
        },
        api: {
            baseURL: resolveApiBaseUrl(),
            timeout: 30000,
            retryAttempts: 3
        },
        auth: {
            tokenKey: 'accessToken',
            refreshTokenKey: 'refreshToken',
            userKey: 'userData'
        },
        pagination: {
            defaultPageSize: 10,
            maxPageSize: 100
        },
        upload: {
            maxFileSize: 5 * 1024 * 1024,
            allowedTypes: ['image/jpeg', 'image/png', 'image/gif', 'image/webp']
        },
        storage: {
            lastViewedProducts: 'lastViewedProducts',
            comparisonList: 'comparisonList',
            cartItems: 'cartItems'
        },
        ui: {
            toastDuration: 3000,
            loadingText: 'در حال بارگذاری...',
            successText: 'عملیات با موفقیت انجام شد.',
            errorText: 'خطایی رخ داد.'
        }
    };
})();
