/**
 * User Panel Common Functionality
 * Handles logout and other common user panel features
 */

document.addEventListener('DOMContentLoaded', function () {
    // Handle logout button clicks
    const logoutButtons = document.querySelectorAll('#logoutButton, [data-logout]');

    logoutButtons.forEach(button => {
        button.addEventListener('click', async function (e) {
            e.preventDefault();

            // Confirm logout
            if (confirm('آیا مطمئن هستید که می‌خواهید از حساب کاربری خود خارج شوید?')) {
                try {
                    // Call logout from authService
                    await window.authService.logout();
                } catch (error) {
                    console.error('Logout error:', error);
                    // Even if API call fails, clear tokens and redirect
                    window.apiClient.clearTokens();
                    window.location.href = 'login.html';
                }
            }
        });
    });

    // Load user info if available
    loadUserInfo();
});

/**
 * Load and display user information
 */
async function loadUserInfo() {
    try {
        if (!window.authService.isAuthenticated()) {
            // Not logged in, redirect to login
            window.location.href = 'login.html';
            return;
        }

        const user = await window.authService.getCurrentUser();
        if (user) {
            // Update user name displays
            const userNameElements = document.querySelectorAll('[data-user-name]');
            userNameElements.forEach(el => {
                el.textContent = `${user.firstName || ''} ${user.lastName || ''}`.trim() || user.email;
            });

            // Update user email displays
            const userEmailElements = document.querySelectorAll('[data-user-email]');
            userEmailElements.forEach(el => {
                el.textContent = user.email;
            });
        }
    } catch (error) {
        console.error('Error loading user info:', error);
    }
}
