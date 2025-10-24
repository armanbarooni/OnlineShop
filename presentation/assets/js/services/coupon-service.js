/**
 * Coupon Service for OnlineShop Frontend
 * Handles coupon operations
 */

class CouponService {
    constructor() {
        this.apiClient = window.apiClient;
    }

    /**
     * Get user coupons
     */
    async getUserCoupons() {
        try {
            const response = await this.apiClient.get('/coupon/user');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error fetching user coupons:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get available coupons
     */
    async getAvailableCoupons() {
        try {
            const response = await this.apiClient.get('/coupon/available');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error fetching available coupons:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get coupon by code
     */
    async getCouponByCode(code) {
        try {
            const response = await this.apiClient.get(`/coupon/code/${code}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error fetching coupon by code:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Validate coupon
     */
    async validateCoupon(code, cartTotal = 0) {
        try {
            const response = await this.apiClient.post('/coupon/validate', {
                code: code,
                cartTotal: cartTotal
            });
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error validating coupon:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Apply coupon
     */
    async applyCoupon(code, cartItems = []) {
        try {
            const response = await this.apiClient.post('/coupon/apply', {
                code: code,
                cartItems: cartItems
            });
            return {
                success: true,
                data: response.data || response,
                message: 'کوپن با موفقیت اعمال شد'
            };
        } catch (error) {
            console.error('Error applying coupon:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Remove coupon
     */
    async removeCoupon(code) {
        try {
            const response = await this.apiClient.delete(`/coupon/remove/${code}`);
            return {
                success: true,
                message: 'کوپن حذف شد'
            };
        } catch (error) {
            console.error('Error removing coupon:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get coupon statistics
     */
    async getCouponStatistics() {
        try {
            const response = await this.apiClient.get('/coupon/statistics');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error fetching coupon statistics:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get coupon usage history
     */
    async getCouponUsageHistory() {
        try {
            const response = await this.apiClient.get('/coupon/usage-history');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error fetching coupon usage history:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Search coupons
     */
    async searchCoupons(query, filters = {}) {
        try {
            const searchCriteria = {
                searchTerm: query,
                ...filters
            };

            const response = await this.apiClient.post('/coupon/search', searchCriteria);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error searching coupons:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get coupons by category
     */
    async getCouponsByCategory(categoryId) {
        try {
            const response = await this.apiClient.get(`/coupon/category/${categoryId}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error fetching coupons by category:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get active coupons
     */
    async getActiveCoupons() {
        try {
            const response = await this.apiClient.get('/coupon/active');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error fetching active coupons:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get expired coupons
     */
    async getExpiredCoupons() {
        try {
            const response = await this.apiClient.get('/coupon/expired');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error fetching expired coupons:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Validate coupon data
     */
    validateCouponData(couponData) {
        const errors = {};

        if (!couponData.code || couponData.code.trim().length === 0) {
            errors.code = 'کد کوپن الزامی است';
        } else if (couponData.code.trim().length < 3) {
            errors.code = 'کد کوپن باید حداقل ۳ کاراکتر باشد';
        }

        if (!couponData.discountType) {
            errors.discountType = 'نوع تخفیف الزامی است';
        }

        if (!couponData.discountValue || couponData.discountValue <= 0) {
            errors.discountValue = 'مقدار تخفیف باید بیشتر از صفر باشد';
        }

        if (couponData.minimumAmount && couponData.minimumAmount < 0) {
            errors.minimumAmount = 'حداقل مبلغ نمی‌تواند منفی باشد';
        }

        if (couponData.maximumDiscount && couponData.maximumDiscount < 0) {
            errors.maximumDiscount = 'حداکثر تخفیف نمی‌تواند منفی باشد';
        }

        return {
            isValid: Object.keys(errors).length === 0,
            errors: errors
        };
    }

    /**
     * Format coupon type
     */
    formatCouponType(type) {
        const typeMap = {
            'Percentage': 'درصدی',
            'Fixed': 'مبلغ ثابت'
        };
        
        return typeMap[type] || type;
    }

    /**
     * Get coupon type color
     */
    getCouponTypeColor(type) {
        const colorMap = {
            'Percentage': 'text-blue-600 bg-blue-100',
            'Fixed': 'text-green-600 bg-green-100'
        };
        
        return colorMap[type] || 'text-gray-600 bg-gray-100';
    }

    /**
     * Format coupon status
     */
    formatCouponStatus(status) {
        const statusMap = {
            'Active': 'فعال',
            'Inactive': 'غیرفعال',
            'Expired': 'منقضی شده',
            'Used': 'استفاده شده'
        };
        
        return statusMap[status] || status;
    }

    /**
     * Get coupon status color
     */
    getCouponStatusColor(status) {
        const colorMap = {
            'Active': 'text-green-600 bg-green-100',
            'Inactive': 'text-gray-600 bg-gray-100',
            'Expired': 'text-red-600 bg-red-100',
            'Used': 'text-blue-600 bg-blue-100'
        };
        
        return colorMap[status] || 'text-gray-600 bg-gray-100';
    }

    /**
     * Calculate coupon discount
     */
    calculateCouponDiscount(coupon, cartTotal) {
        if (!coupon || !cartTotal) return 0;
        
        let discount = 0;
        
        if (coupon.discountType === 'Percentage') {
            discount = (cartTotal * coupon.discountValue) / 100;
        } else if (coupon.discountType === 'Fixed') {
            discount = coupon.discountValue;
        }
        
        // Apply maximum discount limit
        if (coupon.maximumDiscount && discount > coupon.maximumDiscount) {
            discount = coupon.maximumDiscount;
        }
        
        // Ensure discount doesn't exceed cart total
        if (discount > cartTotal) {
            discount = cartTotal;
        }
        
        return Math.round(discount);
    }

    /**
     * Check if coupon is valid
     */
    isCouponValid(coupon, cartTotal = 0) {
        if (!coupon) return false;
        
        const now = new Date();
        const startDate = new Date(coupon.startDate);
        const endDate = new Date(coupon.endDate);
        
        // Check date validity
        if (now < startDate || now > endDate) {
            return false;
        }
        
        // Check minimum amount requirement
        if (coupon.minimumAmount && cartTotal < coupon.minimumAmount) {
            return false;
        }
        
        // Check usage limit
        if (coupon.usageLimit && coupon.usedCount >= coupon.usageLimit) {
            return false;
        }
        
        return true;
    }

    /**
     * Get coupon discount text
     */
    getCouponDiscountText(coupon) {
        if (!coupon) return '';
        
        if (coupon.discountType === 'Percentage') {
            return `${coupon.discountValue}% تخفیف`;
        } else if (coupon.discountType === 'Fixed') {
            return `${window.utils.formatPrice(coupon.discountValue)} تخفیف`;
        }
        
        return '';
    }

    /**
     * Get coupon requirements text
     */
    getCouponRequirementsText(coupon) {
        if (!coupon) return '';
        
        const requirements = [];
        
        if (coupon.minimumAmount) {
            requirements.push(`حداقل خرید ${window.utils.formatPrice(coupon.minimumAmount)}`);
        }
        
        if (coupon.maximumDiscount) {
            requirements.push(`حداکثر تخفیف ${window.utils.formatPrice(coupon.maximumDiscount)}`);
        }
        
        if (coupon.usageLimit) {
            requirements.push(`حداکثر ${coupon.usageLimit} بار استفاده`);
        }
        
        return requirements.join(' - ');
    }

    /**
     * Get coupon summary
     */
    getCouponSummary(coupons) {
        if (!Array.isArray(coupons)) {
            return {
                total: 0,
                active: 0,
                expired: 0,
                used: 0
            };
        }

        const summary = {
            total: coupons.length,
            active: 0,
            expired: 0,
            used: 0
        };

        coupons.forEach(coupon => {
            switch (coupon.status) {
                case 'Active':
                    summary.active++;
                    break;
                case 'Expired':
                    summary.expired++;
                    break;
                case 'Used':
                    summary.used++;
                    break;
            }
        });

        return summary;
    }

    /**
     * Copy coupon code
     */
    async copyCouponCode(code) {
        try {
            await window.utils.copyToClipboard(code);
            return {
                success: true,
                message: 'کد کوپن کپی شد'
            };
        } catch (error) {
            console.error('Error copying coupon code:', error);
            return {
                success: false,
                error: 'خطا در کپی کردن کد کوپن'
            };
        }
    }
}

// Create global instance
window.couponService = new CouponService();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = CouponService;
}
