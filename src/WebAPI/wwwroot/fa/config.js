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


    const normalizeApiBaseUrl = (value) => {
        if (!value || typeof value !== 'string') {
            return null;
        }

        const trimmed = value.trim();
        if (!trimmed) {
            return null;
        }

        if (trimmed.startsWith('http://') || trimmed.startsWith('https://') || trimmed.startsWith('//')) {
            return trimmed;
        }

        return trimmed.startsWith('/') ? trimmed : `/${trimmed}`;
    };

    const resolveApiBaseUrl = (runtimeConfig, environmentName) => {
        const configuredApiBaseUrl =
            runtimeConfig?.apiBaseUrl ||
            window.__API_BASE_URL__ ||
            document.querySelector('meta[name="api-base-url"]')?.content;

        const normalizedConfiguredUrl = normalizeApiBaseUrl(configuredApiBaseUrl);
        if (normalizedConfiguredUrl) {
            return normalizedConfiguredUrl;
        }

        if (environmentName === 'development') {
            return 'http://localhost:5000/api';
        }

        return '/api';
    };

    const resolveMahakContentBaseUrl = (runtimeConfig) => {
        const configuredBaseUrl =
            runtimeConfig?.mahakContentBaseUrl ||
            window.__MAHAK_CONTENT_BASE_URL__ ||
            document.querySelector('meta[name="mahak-content-base-url"]')?.content;

        const normalizedConfiguredUrl = normalizeApiBaseUrl(configuredBaseUrl);
        if (normalizedConfiguredUrl) {
            return normalizedConfiguredUrl;
        }

        return 'https://mahakacc.mahaksoft.com';
    };

    const getResolvedApiBaseUrl = (runtimeConfig = window.__APP_RUNTIME_CONFIG__ ?? null) => {
        const environmentName = runtimeConfig?.environment ?? detectEnvironment(hostname);
        return resolveApiBaseUrl(runtimeConfig, environmentName);
    };

    const getResolvedMahakContentBaseUrl = (runtimeConfig = window.__APP_RUNTIME_CONFIG__ ?? null) => {
        return resolveMahakContentBaseUrl(runtimeConfig);
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
            content: {
                mahakBaseURL: getResolvedMahakContentBaseUrl(runtimeConfig)
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

        window.resolveApiBaseURL = getResolvedApiBaseUrl;
        window.resolveMahakContentBaseURL = getResolvedMahakContentBaseUrl;
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
