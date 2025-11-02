/**
 * Checkout Service
 * Handles all checkout-related API calls
 */
class CheckoutService {
    constructor() {
        this.apiClient = window.apiClient;
        this.baseUrl = '/api/Checkout';
    }

    /**
     * Process checkout - main checkout method
     */
    async processCheckout(checkoutData) {
        try {
            const response = await this.apiClient.post(this.baseUrl, checkoutData);
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            const data = response.data || response;
            return {
                success: true,
                data: data.data || data
            };
        } catch (error) {
            window.logger.error('Error processing checkout:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ظ¾ط±ط¯ط§ط²ط´ طھط³ظˆغŒظ‡ ط­ط³ط§ط¨'
            };
        }
    }

    /**
     * Get checkout data - get cart and addresses for checkout
     */
    async getCheckoutData() {
        try {
            // Get cart and addresses
            const [cartResult, addressesResult] = await Promise.all([
                window.cartService.getUserCart(),
                window.addressService.getAddresses()
            ]);

            return {
                success: true,
                data: {
                    cart: cartResult.success ? cartResult.data : null,
                    addresses: addressesResult.success ? addressesResult.data : [],
                    cartError: cartResult.success ? null : cartResult.error,
                    addressesError: addressesResult.success ? null : addressesResult.error
                }
            };
        } catch (error) {
            window.logger.error('Error getting checkout data:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط¯ط±غŒط§ظپطھ ط§ط·ظ„ط§ط¹ط§طھ طھط³ظˆغŒظ‡ ط­ط³ط§ط¨'
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
            window.logger.error('Error getting addresses:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط¯ط±غŒط§ظپطھ ط¢ط¯ط±ط³â€Œظ‡ط§'
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
            window.logger.error('Error getting payment methods:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط¯ط±غŒط§ظپطھ ط±ظˆط´â€Œظ‡ط§غŒ ظ¾ط±ط¯ط§ط®طھ'
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
            window.logger.error('Error getting shipping methods:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط¯ط±غŒط§ظپطھ ط±ظˆط´â€Œظ‡ط§غŒ ط§ط±ط³ط§ظ„'
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
            window.logger.error('Error calculating shipping:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ظ…ط­ط§ط³ط¨ظ‡ ظ‡ط²غŒظ†ظ‡ ط§ط±ط³ط§ظ„'
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
            window.logger.error('Error validating checkout:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط§ط¹طھط¨ط§ط±ط³ظ†ط¬غŒ ط§ط·ظ„ط§ط¹ط§طھ طھط³ظˆغŒظ‡ ط­ط³ط§ط¨'
            };
        }
    }

    /**
     * Create order - alias for processCheckout
     */
    async createOrder(orderData) {
        return await this.processCheckout(orderData);
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
            window.logger.error('Error getting order summary:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط¯ط±غŒط§ظپطھ ط®ظ„ط§طµظ‡ ط³ظپط§ط±ط´'
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
            window.logger.error('Error applying coupon to checkout:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط§ط¹ظ…ط§ظ„ ع©ط¯ طھط®ظپغŒظپ'
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
            window.logger.error('Error removing coupon from checkout:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط­ط°ظپ ع©ط¯ طھط®ظپغŒظپ'
            };
        }
    }

    /**
     * Validate checkout form data
     */
    validateCheckoutData(checkoutData) {
        const errors = {};

        if (!checkoutData.addressId) {
            errors.addressId = 'ط§ظ†طھط®ط§ط¨ ط¢ط¯ط±ط³ ط§ظ„ط²ط§ظ…غŒ ط§ط³طھ';
        }

        if (!checkoutData.shippingMethodId) {
            errors.shippingMethodId = 'ط§ظ†طھط®ط§ط¨ ط±ظˆط´ ط§ط±ط³ط§ظ„ ط§ظ„ط²ط§ظ…غŒ ط§ط³طھ';
        }

        if (!checkoutData.paymentMethodId) {
            errors.paymentMethodId = 'ط§ظ†طھط®ط§ط¨ ط±ظˆط´ ظ¾ط±ط¯ط§ط®طھ ط§ظ„ط²ط§ظ…غŒ ط§ط³طھ';
        }

        if (checkoutData.notes && checkoutData.notes.length > 500) {
            errors.notes = 'طھظˆط¶غŒط­ط§طھ ظ†ظ…غŒâ€Œطھظˆط§ظ†ط¯ ط¨غŒط´طھط± ط§ط² غµغ°غ° ع©ط§ط±ط§ع©طھط± ط¨ط§ط´ط¯';
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

