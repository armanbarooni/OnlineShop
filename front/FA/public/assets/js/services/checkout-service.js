/**
 * Checkout Service
 * Handles all checkout-related API calls
 */
class CheckoutService {
    constructor() {
        this.baseUrl = '/api/checkout';
    }

    /**
     * Get checkout data
     */
    async getCheckoutData() {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/data`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error getting checkout data:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت اطلاعات تسویه حساب'
            };
        }
    }

    /**
     * Get available addresses
     */
    async getAvailableAddresses() {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/addresses`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error getting addresses:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت آدرس‌ها'
            };
        }
    }

    /**
     * Get available payment methods
     */
    async getPaymentMethods() {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/payment-methods`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error getting payment methods:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت روش‌های پرداخت'
            };
        }
    }

    /**
     * Get shipping methods
     */
    async getShippingMethods(addressId) {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/shipping-methods`, {
                params: { addressId }
            });
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error getting shipping methods:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت روش‌های ارسال'
            };
        }
    }

    /**
     * Calculate shipping cost
     */
    async calculateShipping(shippingMethodId, addressId) {
        try {
            const response = await window.apiClient.post(`${this.baseUrl}/calculate-shipping`, {
                shippingMethodId: shippingMethodId,
                addressId: addressId
            });
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error calculating shipping:', error);
            return {
                success: false,
                error: error.message || 'خطا در محاسبه هزینه ارسال'
            };
        }
    }

    /**
     * Validate checkout data
     */
    async validateCheckout(checkoutData) {
        try {
            const response = await window.apiClient.post(`${this.baseUrl}/validate`, checkoutData);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error validating checkout:', error);
            return {
                success: false,
                error: error.message || 'خطا در اعتبارسنجی اطلاعات تسویه حساب'
            };
        }
    }

    /**
     * Create order
     */
    async createOrder(orderData) {
        try {
            const response = await window.apiClient.post(`${this.baseUrl}/create-order`, orderData);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error creating order:', error);
            return {
                success: false,
                error: error.message || 'خطا در ایجاد سفارش'
            };
        }
    }

    /**
     * Get order summary
     */
    async getOrderSummary() {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/order-summary`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error getting order summary:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت خلاصه سفارش'
            };
        }
    }

    /**
     * Apply coupon to checkout
     */
    async applyCouponToCheckout(couponCode) {
        try {
            const response = await window.apiClient.post(`${this.baseUrl}/apply-coupon`, {
                couponCode: couponCode
            });
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error applying coupon to checkout:', error);
            return {
                success: false,
                error: error.message || 'خطا در اعمال کد تخفیف'
            };
        }
    }

    /**
     * Remove coupon from checkout
     */
    async removeCouponFromCheckout() {
        try {
            const response = await window.apiClient.delete(`${this.baseUrl}/remove-coupon`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error removing coupon from checkout:', error);
            return {
                success: false,
                error: error.message || 'خطا در حذف کد تخفیف'
            };
        }
    }

    /**
     * Validate checkout form data
     */
    validateCheckoutData(checkoutData) {
        const errors = {};

        if (!checkoutData.addressId) {
            errors.addressId = 'انتخاب آدرس الزامی است';
        }

        if (!checkoutData.shippingMethodId) {
            errors.shippingMethodId = 'انتخاب روش ارسال الزامی است';
        }

        if (!checkoutData.paymentMethodId) {
            errors.paymentMethodId = 'انتخاب روش پرداخت الزامی است';
        }

        if (checkoutData.notes && checkoutData.notes.length > 500) {
            errors.notes = 'توضیحات نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد';
        }

        return {
            isValid: Object.keys(errors).length === 0,
            errors: errors
        };
    }

    /**
     * Format checkout data for submission
     */
    formatCheckoutData(formData) {
        return {
            addressId: parseInt(formData.get('addressId')),
            shippingMethodId: parseInt(formData.get('shippingMethodId')),
            paymentMethodId: parseInt(formData.get('paymentMethodId')),
            notes: formData.get('notes') || '',
            couponCode: formData.get('couponCode') || null,
            acceptTerms: formData.get('acceptTerms') === 'on'
        };
    }
}

// Create global instance
window.checkoutService = new CheckoutService();
