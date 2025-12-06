/**
 * User Profile Service for OnlineShop Frontend
 * Handles user profile operations
 */

class UserProfileService {
    constructor() {
        this.apiClient = window.apiClient;
    }

    /**
     * Get current user profile
     */
    async getUserProfile() {
        try {
            const response = await this.apiClient.get('/auth/me');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching user profile:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Update user profile
     */
    async updateProfile(profileData) {
        try {
            const response = await this.apiClient.put('/userprofile', profileData);
            return {
                success: true,
                data: response.data || response,
                message: 'پروفایل با موفقیت بروزرسانی شد'
            };
        } catch (error) {
            window.logger.error('Error updating profile:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Change user password
     */
    async changePassword(currentPassword, newPassword) {
        try {
            const response = await this.apiClient.post('/auth/change-password', {
                currentPassword: currentPassword,
                newPassword: newPassword
            });
            return {
                success: true,
                message: 'رمز عبور با موفقیت تغییر یافت'
            };
        } catch (error) {
            window.logger.error('Error changing password:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Upload profile picture
     */
    async uploadProfilePicture(file) {
        try {
            const formData = new FormData();
            formData.append('file', file);

            const response = await fetch(`${this.apiClient.baseURL}/userprofile/upload-picture`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${this.apiClient.accessToken}`
                },
                body: formData
            });

            if (!response.ok) {
                throw new Error('Upload failed');
            }

            const data = await response.json();
            return {
                success: true,
                data: data,
                message: 'تصویر پروفایل با موفقیت آپلود شد'
            };
        } catch (error) {
            window.logger.error('Error uploading profile picture:', error);
            return {
                success: false,
                error: 'خطا در آپلود تصویر پروفایل'
            };
        }
    }

    /**
     * Get user statistics
     */
    async getUserStatistics() {
        try {
            const [ordersResponse, wishlistResponse, reviewsResponse] = await Promise.all([
                this.apiClient.get('/userorder'),
                this.apiClient.get('/wishlist'),
                this.apiClient.get('/productreview/user')
            ]);

            const orders = ordersResponse.data || ordersResponse;
            const wishlist = wishlistResponse.data || wishlistResponse;
            const reviews = reviewsResponse.data || reviewsResponse;

            return {
                success: true,
                data: {
                    totalOrders: Array.isArray(orders) ? orders.length : 0,
                    totalWishlist: Array.isArray(wishlist) ? wishlist.length : 0,
                    totalReviews: Array.isArray(reviews) ? reviews.length : 0
                }
            };
        } catch (error) {
            window.logger.error('Error fetching user statistics:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get user activity
     */
    async getUserActivity() {
        try {
            const response = await this.apiClient.get('/userprofile/activity');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching user activity:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get user notifications
     */
    async getNotifications() {
        try {
            const response = await this.apiClient.get('/userprofile/notifications');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching notifications:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Mark notification as read
     */
    async markNotificationAsRead(notificationId) {
        try {
            const response = await this.apiClient.put(`/userprofile/notifications/${notificationId}/read`);
            return {
                success: true,
                message: 'اعلان به عنوان خوانده شده علامت‌گذاری شد'
            };
        } catch (error) {
            window.logger.error('Error marking notification as read:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Delete notification
     */
    async deleteNotification(notificationId) {
        try {
            const response = await this.apiClient.delete(`/userprofile/notifications/${notificationId}`);
            return {
                success: true,
                message: 'اعلان حذف شد'
            };
        } catch (error) {
            window.logger.error('Error deleting notification:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Update user preferences
     */
    async updatePreferences(preferences) {
        try {
            const response = await this.apiClient.put('/userprofile/preferences', preferences);
            return {
                success: true,
                data: response.data || response,
                message: 'تنظیمات با موفقیت بروزرسانی شد'
            };
        } catch (error) {
            window.logger.error('Error updating preferences:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get user preferences
     */
    async getPreferences() {
        try {
            const response = await this.apiClient.get('/userprofile/preferences');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching preferences:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Validate profile data
     */
    validateProfileData(data) {
        const errors = {};

        if (data.firstName && data.firstName.trim().length < 2) {
            errors.firstName = 'نام باید حداقل ۲ کاراکتر باشد';
        }

        if (data.lastName && data.lastName.trim().length < 2) {
            errors.lastName = 'نام خانوادگی باید حداقل ۲ کاراکتر باشد';
        }

        if (data.email && !window.utils.isValidEmail(data.email)) {
            errors.email = 'ایمیل نامعتبر است';
        }

        if (data.phone && !window.utils.isValidPhone(data.phone)) {
            errors.phone = 'شماره موبایل نامعتبر است';
        }

        return {
            isValid: Object.keys(errors).length === 0,
            errors: errors
        };
    }

    /**
     * Get wallet balance
     */
    async getWalletBalance() {
        try {
            const response = await this.apiClient.get('/userpayment/wallet/balance');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching wallet balance:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get wallet transactions
     */
    async getWalletTransactions(searchCriteria = {}) {
        try {
            const response = await this.apiClient.post('/userpayment/wallet/transactions', searchCriteria);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching wallet transactions:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Deposit to wallet
     */
    async depositToWallet(amount) {
        try {
            const response = await this.apiClient.post('/userpayment/wallet/deposit', { amount });
            return {
                success: true,
                data: response.data || response,
                message: 'واریز با موفقیت انجام شد'
            };
        } catch (error) {
            window.logger.error('Error depositing to wallet:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Withdraw from wallet
     */
    async withdrawFromWallet(amount) {
        try {
            const response = await this.apiClient.post('/userpayment/wallet/withdraw', { amount });
            return {
                success: true,
                data: response.data || response,
                message: 'برداشت با موفقیت انجام شد'
            };
        } catch (error) {
            window.logger.error('Error withdrawing from wallet:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }
}

// Create global instance
window.userProfileService = new UserProfileService();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = UserProfileService;
}

