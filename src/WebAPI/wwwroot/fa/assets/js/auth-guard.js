/**
 * Authentication Guard for OnlineShop Frontend
 * Protects routes and handles authentication state
 */

class AuthGuard {
    constructor() {
        this.authService = window.authService;
        this.protectedRoutes = [
            'user-panel-index.html',
            'user-panel-profile.html',
            'user-panel-change-password.html',
            'user-panel-address.html',
            'user-panel-edit-address.html',
            'user-panel-order.html',
            'user-panel-order-detail.html',
            'user-panel-order-return-step-one.html',
            'user-panel-order-return-step-two.html',
            'user-panel-order-return-step-three.html',
            'user-panel-favorite.html',
            'user-panel-comment.html',
            'user-panel-edit-comment.html',
            'user-panel-discount.html',
            'user-panel-gift-cart.html',
            'user-panel-increase-money.html',
            'user-panel-last-viewd.html',
            'user-panel-site-notification.html',
            'user-panel-ticket.html',
            'user-panel-ticket-chat.html',
            'user-panel-ticket-form.html',
            'user-panel-transfer-money.html',
            'user-panel-wallet.html',
            'user-panel-activiti.html',
            'user-panel-blank.html',
            'user-panel-discous.html',
            'user-panel-get-discount.html',
            'cart.html',
            'checkout.html',
            'payment.html'
        ];

        this.publicRoutes = [
            'login.html',
            'register.html',
            'forgot-password.html',
            'index.html',
            'shop.html',
            'product.html',
            'compare.html',
            'about-us.html',
            'contact-us.html',
            'faq.html',
            'terms-and-rules.html',
            'privacy-policy.html',
            'return-procedure.html'
        ];
    }

    /**
     * Initialize auth guard
     */
    init() {
        this.checkCurrentRoute();
        this.setupAuthStateListeners();
    }

    /**
     * Check current route and redirect if necessary
     */
    checkCurrentRoute() {
        const currentPage = this.getCurrentPageName();
        
        if (this.isProtectedRoute(currentPage)) {
            if (!this.authService.isAuthenticated()) {
                this.redirectToLogin();
                return;
            }
        } else if (this.isPublicRoute(currentPage)) {
            if (this.authService.isAuthenticated() && this.isLoginPage(currentPage)) {
                this.redirectToDashboard();
                return;
            }
        }

        // If authenticated, update UI elements
        if (this.authService.isAuthenticated()) {
            this.updateAuthenticatedUI();
        } else {
            this.updateUnauthenticatedUI();
        }
    }

    /**
     * Get current page name
     */
    getCurrentPageName() {
        const path = window.location.pathname;
        return path.split('/').pop() || 'index.html';
    }

    /**
     * Check if route is protected
     */
    isProtectedRoute(pageName) {
        return this.protectedRoutes.includes(pageName);
    }

    /**
     * Check if route is public
     */
    isPublicRoute(pageName) {
        return this.publicRoutes.includes(pageName);
    }

    /**
     * Check if current page is login page
     */
    isLoginPage(pageName) {
        return pageName === 'login.html' || pageName === 'register.html';
    }

    /**
     * Redirect to login page
     */
    redirectToLogin() {
        // Store intended destination
        const currentUrl = window.location.href;
        localStorage.setItem('intendedUrl', currentUrl);
        
        window.location.href = 'login.html';
    }

    /**
     * Redirect to dashboard
     */
    redirectToDashboard() {
        const intendedUrl = localStorage.getItem('intendedUrl');
        if (intendedUrl && intendedUrl !== window.location.href) {
            localStorage.removeItem('intendedUrl');
            window.location.href = intendedUrl;
        } else {
            window.location.href = 'user-panel-index.html';
        }
    }

    /**
     * Setup authentication state listeners
     */
    setupAuthStateListeners() {
        // Listen for storage changes (token updates from other tabs)
        window.addEventListener('storage', (e) => {
            if (e.key === 'accessToken') {
                this.checkCurrentRoute();
            }
        });

        // Listen for custom auth events
        window.addEventListener('auth:login', () => {
            this.updateAuthenticatedUI();
        });

        window.addEventListener('auth:logout', () => {
            this.updateUnauthenticatedUI();
        });
    }

    /**
     * Update UI for authenticated users
     */
    updateAuthenticatedUI() {
        // Show authenticated user elements
        const authElements = document.querySelectorAll('[data-auth="required"]');
        authElements.forEach(el => el.style.display = 'block');

        const guestElements = document.querySelectorAll('[data-auth="guest"]');
        guestElements.forEach(el => el.style.display = 'none');

        // Update user info if available
        this.updateUserInfo();
    }

    /**
     * Update UI for unauthenticated users
     */
    updateUnauthenticatedUI() {
        // Show guest user elements
        const authElements = document.querySelectorAll('[data-auth="required"]');
        authElements.forEach(el => el.style.display = 'none');

        const guestElements = document.querySelectorAll('[data-auth="guest"]');
        guestElements.forEach(el => el.style.display = 'block');
    }

    /**
     * Update user information in UI
     */
    async updateUserInfo() {
        try {
            const user = await this.authService.getCurrentUser();
            if (user) {
                // Update user name/email in UI
                const nameElements = document.querySelectorAll('[data-user="name"], [data-user-name]');
                nameElements.forEach(el => {
                    el.textContent = user.firstName && user.lastName 
                        ? `${user.firstName} ${user.lastName}` 
                        : user.email;
                });

                const emailElements = document.querySelectorAll('[data-user="email"], [data-user-email]');
                emailElements.forEach(el => {
                    el.textContent = user.email;
                });
            }
        } catch (error) {
            window.logger.error('Error updating user info:', error);
        }
    }

    /**
     * Check if user has required role
     */
    async hasRole(requiredRole) {
        if (!this.authService.isAuthenticated()) {
            return false;
        }

        try {
            const user = await this.authService.getCurrentUser();
            if (!user) {
                return false;
            }
            
            // Check if user has roles array
            if (!user.roles) {
                return false;
            }
            
            // Handle both array and single role
            const roles = Array.isArray(user.roles) ? user.roles : [user.roles];
            return roles.includes(requiredRole);
        } catch (error) {
            window.logger.error('Error checking role:', error);
            // Fallback to cached user data
            try {
                const cachedUser = this.authService.apiClient.getCurrentUser();
                if (cachedUser && cachedUser.roles) {
                    const roles = Array.isArray(cachedUser.roles) ? cachedUser.roles : [cachedUser.roles];
                    return roles.includes(requiredRole);
                }
            } catch (fallbackError) {
                window.logger.error('Error in fallback role check:', fallbackError);
            }
            return false;
        }
    }

    /**
     * Require specific role for current page
     */
    async requireRole(requiredRole) {
        const hasRole = await this.hasRole(requiredRole);
        if (!hasRole) {
            this.redirectToLogin();
            return false;
        }
        return true;
    }

    /**
     * Add logout functionality to logout buttons
     */
    setupLogoutButtons() {
        const logoutButtons = document.querySelectorAll('[data-action="logout"], #logout, #logoutButton, .logout-btn, .logout-button');
        logoutButtons.forEach(button => {
            button.addEventListener('click', (e) => {
                e.preventDefault();
                this.authService.logout();
            });
        });
    }
}

// Initialize auth guard when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.authGuard = new AuthGuard();
    window.authGuard.init();
    window.authGuard.setupLogoutButtons();
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AuthGuard;
}

