/**
 * Common JavaScript for User Panel pages
 * Handles common functionality like logout button
 */

(function() {
    'use strict';

    // Initialize logout button when DOM is ready
    function initLogoutButton() {
        const logoutButton = document.getElementById('logoutButton');
        if (logoutButton) {
            logoutButton.addEventListener('click', function(e) {
                e.preventDefault();
                if (window.authService) {
                    window.authService.logout();
                } else {
                    // Fallback if authService is not loaded
                    localStorage.removeItem('accessToken');
                    localStorage.removeItem('refreshToken');
                    localStorage.removeItem('userData');
                    window.location.href = 'login.html';
                }
            });
        }
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initLogoutButton);
    } else {
        // DOM is already ready
        initLogoutButton();
    }
})();

