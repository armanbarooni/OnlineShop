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

    const resolveApiBaseUrl = (runtimeConfig, environmentName) => {
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

    const buildConfig = (runtimeConfig) => {
        const environmentName = runtimeConfig?.environment ?? detectEnvironment(hostname);

        return {
            environment: {
                name: environmentName,
                hostname
            },
            api: {
                baseURL: resolveApiBaseUrl(runtimeConfig, environmentName),
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
    };

    const applyRuntimeConfig = (runtimeConfig) => {
        if (runtimeConfig && typeof runtimeConfig === 'object') {
            window.__APP_RUNTIME_CONFIG__ = runtimeConfig;
        }

        window.config = buildConfig(window.__APP_RUNTIME_CONFIG__ ?? null);
        window.dispatchEvent(new CustomEvent('app:config-ready', { detail: window.config }));
        return window.config;
    };

    applyRuntimeConfig(window.__APP_RUNTIME_CONFIG__ ?? null);
    window.configReady = Promise.resolve(window.config);

    if (!window.__APP_RUNTIME_CONFIG__ && typeof window.fetch === 'function') {
        window.configReady = window
            .fetch('config.runtime.json', { cache: 'no-store', credentials: 'same-origin' })
            .then((response) => (response.ok ? response.json() : null))
            .then((runtimeConfig) => (runtimeConfig ? applyRuntimeConfig(runtimeConfig) : window.config))
            .catch((error) => {
                console.warn('Runtime config fetch failed, using default config.', error);
                return window.config;
            });
    }
})();
