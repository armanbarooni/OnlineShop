(function initializeLogger(global) {
    if (!global) {
        return;
    }

    if (global.logger) {
        return;
    }

    const consoleRef = global.console || {};
    const history = [];
    const levels = ['log', 'info', 'warn', 'error', 'debug'];

    const getEnvironment = () => {
        const envName = global.config?.environment?.name;
        if (typeof envName === 'string' && envName.length > 0) {
            return envName.toLowerCase();
        }
        const host = global.location?.hostname || '';
        if (!host || host === 'localhost' || host === '127.0.0.1') {
            return 'development';
        }
        if (host.includes('staging') || host.includes('test')) {
            return 'staging';
        }
        return 'production';
    };

    const environment = getEnvironment();
    const isProduction = environment === 'production';

    const emitToConsole = (level, args) => {
        const consoleMethod = level === 'log' || level === 'debug' ? 'log' : level;
        const target = consoleRef[consoleMethod];
        if (typeof target === 'function') {
            target.apply(consoleRef, args);
        }
    };

    const logger = {
        history,
        environment,
        isEnabled: true,
        isSilentInProduction: true,
        clearHistory: () => {
            history.length = 0;
        },
        enable: () => {
            logger.isEnabled = true;
        },
        disable: () => {
            logger.isEnabled = false;
        },
        allowProductionLogs: () => {
            logger.isSilentInProduction = false;
        },
        muteProductionLogs: () => {
            logger.isSilentInProduction = true;
        }
    };

    levels.forEach((level) => {
        logger[level] = (...args) => {
            const timestamp = new Date().toISOString();
            const entry = { level, timestamp, args };
            history.push(entry);

            const shouldEmit =
                logger.isEnabled &&
                (!isProduction || !logger.isSilentInProduction || level === 'error');

            if (shouldEmit) {
                emitToConsole(level, args);
            }

            return entry;
        };
    });

    global.logger = logger;
})(typeof window !== 'undefined' ? window : undefined);
