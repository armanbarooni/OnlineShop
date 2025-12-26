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

    const tryLoadRuntimeConfig = () => {
        if (window.__APP_RUNTIME_CONFIG__) {
            return window.__APP_RUNTIME_CONFIG__;
        }

        try {
            const xhr = new XMLHttpRequest();
            xhr.open('GET', 'config.runtime.json', false);
            xhr.send(null);
            if (xhr.status >= 200 && xhr.status < 400 && xhr.responseText) {
                const parsed = JSON.parse(xhr.responseText);
                window.__APP_RUNTIME_CONFIG__ = parsed;
                return parsed;
            }
        } catch (error) {
            console.warn('Runtime config not found, falling back to defaults.', error);
        }
        return null;
    };

    const runtimeConfig = tryLoadRuntimeConfig();
    const environmentName = runtimeConfig?.environment ?? detectEnvironment(hostname);

    const resolveApiBaseUrl = () => {
        if (runtimeConfig?.apiBaseUrl) {
            return runtimeConfig.apiBaseUrl;
        }
        if (window.__API_BASE_URL__) {
            return window.__API_BASE_URL__;
        }
        const metaApiBase = document.querySelector('meta[name="api-base-url"]');
        if (metaApiBase?.content) {
            return metaApiBase.content;
        }
        const normalizedOrigin = (window.location?.origin || '').replace(/\/$/, '');
        if (normalizedOrigin) {
            return `${normalizedOrigin}/api`;
        }
        if (environmentName === 'development') {
            return 'http://localhost:5000/api';
        }
        return 'https://api.example.com/api';
    };

    const defaultAuth = {
        tokenKey: 'accessToken',
        refreshTokenKey: 'refreshToken',
        userKey: 'userData'
    };

    window.config = {
        environment: {
            name: environmentName,
            hostname
        },
        api: {
            baseURL: resolveApiBaseUrl(),
            timeout: runtimeConfig?.apiTimeout ?? 30000,
            retryAttempts: runtimeConfig?.apiRetryAttempts ?? 3
        },
        auth: {
            ...defaultAuth,
            ...(runtimeConfig?.auth ?? {})
        },
        pagination: {
            defaultPageSize: runtimeConfig?.pagination?.defaultPageSize ?? 10,
            maxPageSize: runtimeConfig?.pagination?.maxPageSize ?? 100
        },
        upload: {
            maxFileSize: runtimeConfig?.upload?.maxFileSize ?? 5 * 1024 * 1024,
            allowedTypes: runtimeConfig?.upload?.allowedTypes ?? ['image/jpeg', 'image/png', 'image/gif', 'image/webp']
        },
        storage: {
            lastViewedProducts: runtimeConfig?.storage?.lastViewedProducts ?? 'lastViewedProducts',
            comparisonList: runtimeConfig?.storage?.comparisonList ?? 'comparisonList',
            cartItems: runtimeConfig?.storage?.cartItems ?? 'cartItems'
        },
        ui: {
            toastDuration: runtimeConfig?.ui?.toastDuration ?? 3000,
            loadingText: runtimeConfig?.ui?.loadingText ?? 'در حال بارگذاری...',
            successText: runtimeConfig?.ui?.successText ?? 'عملیات با موفقیت انجام شد.',
            errorText: runtimeConfig?.ui?.errorText ?? 'خطایی رخ داد.'
        }
    };
})();
