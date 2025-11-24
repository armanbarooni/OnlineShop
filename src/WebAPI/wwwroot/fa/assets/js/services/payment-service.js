/**
 * Payment Service
 * Handles all payment-related API calls
 */
class PaymentService {
    constructor() {
        this.apiClient = window.apiClient;
        this.baseUrl = '/api/UserPayment';
    }

    /**
     * Create payment - initialize payment
     */
    async initializePayment(paymentData) {
        try {
            const response = await this.apiClient.post(this.baseUrl, paymentData);
            
            // Handle the new API response format from backend
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            // Backend returns Result<UserPaymentDto> format
            // The response structure: { success: true, data: { id, paymentUrl, ... } }
            const data = response.data || response;
            const paymentDto = data.data || data;
            
            // Ensure PaymentUrl is accessible (support both camelCase and PascalCase)
            const paymentUrl = paymentDto.paymentUrl || paymentDto.PaymentUrl;
            
            return {
                success: true,
                data: paymentDto,
                paymentUrl: paymentUrl
            };
        } catch (error) {
            window.logger.error('Error initializing payment:', error);
            return {
                success: false,
                error: error.message || 'خطا در شروع پرداخت'
            };
        }
    }

    /**
     * Get payment by ID
     */
    async getPaymentById(paymentId) {
        try {
            const response = await this.apiClient.get(`${this.baseUrl}/${paymentId}`);
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            const data = response.data || response;
            return {
                success: true,
                data: data.data || data
            };
        } catch (error) {
            window.logger.error('Error getting payment:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط¯ط±غŒط§ظپطھ ط§ط·ظ„ط§ط¹ط§طھ ظ¾ط±ط¯ط§ط®طھ'
            };
        }
    }

    /**
     * Get payments by order ID
     */
    async getPaymentsByOrderId(orderId) {
        try {
            const response = await this.apiClient.get(`${this.baseUrl}/order/${orderId}`);
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            const data = response.data || response;
            return {
                success: true,
                data: data.data || data
            };
        } catch (error) {
            window.logger.error('Error getting payments by order:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط¯ط±غŒط§ظپطھ ط§ط·ظ„ط§ط¹ط§طھ ظ¾ط±ط¯ط§ط®طھ ط³ظپط§ط±ط´'
            };
        }
    }

    /**
     * Process payment
     */
    async processPayment(paymentId, paymentData) {
        try {
            const response = await window.apiClient.post(`${this.baseUrl}/${paymentId}/process`, paymentData);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            window.logger.error('Error processing payment:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ظ¾ط±ط¯ط§ط²ط´ ظ¾ط±ط¯ط§ط®طھ'
            };
        }
    }

    /**
     * Verify payment
     */
    async verifyPayment(paymentId, verificationData) {
        try {
            const response = await window.apiClient.post(`${this.baseUrl}/${paymentId}/verify`, verificationData);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            window.logger.error('Error verifying payment:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± طھط§غŒغŒط¯ ظ¾ط±ط¯ط§ط®طھ'
            };
        }
    }

    /**
     * Get payment status - use getPaymentById
     */
    async getPaymentStatus(paymentId) {
        const result = await this.getPaymentById(paymentId);
        if (result.success && result.data) {
            // Support both 'status' and 'paymentStatus' field names
            const status = result.data.status || result.data.paymentStatus || result.data.PaymentStatus;
            return {
                success: true,
                data: {
                    status: status,
                    ...result.data
                }
            };
        }
        return result;
    }

    /**
     * Get payment details - alias for getPaymentById
     */
    async getPaymentDetails(paymentId) {
        return await this.getPaymentById(paymentId);
    }

    /**
     * Cancel payment
     */
    async cancelPayment(paymentId) {
        try {
            const response = await window.apiClient.post(`${this.baseUrl}/${paymentId}/cancel`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            window.logger.error('Error canceling payment:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ظ„ط؛ظˆ ظ¾ط±ط¯ط§ط®طھ'
            };
        }
    }

    /**
     * Get payment methods
     */
    async getPaymentMethods() {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/methods`);
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
     * Get payment history
     */
    async getPaymentHistory(page = 1, pageSize = 10) {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/history`, {
                params: { page, pageSize }
            });
            return {
                success: true,
                data: response
            };
        } catch (error) {
            window.logger.error('Error getting payment history:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط¯ط±غŒط§ظپطھ طھط§ط±غŒط®ع†ظ‡ ظ¾ط±ط¯ط§ط®طھâ€Œظ‡ط§'
            };
        }
    }

    /**
     * Get payment details
     */
    async getPaymentDetails(paymentId) {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/${paymentId}`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            window.logger.error('Error getting payment details:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط¯ط±غŒط§ظپطھ ط¬ط²ط¦غŒط§طھ ظ¾ط±ط¯ط§ط®طھ'
            };
        }
    }

    /**
     * Refund payment
     */
    async refundPayment(paymentId, refundData) {
        try {
            const response = await window.apiClient.post(`${this.baseUrl}/${paymentId}/refund`, refundData);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            window.logger.error('Error refunding payment:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط¨ط§ط²ظ¾ط±ط¯ط§ط®طھ'
            };
        }
    }

    /**
     * Validate payment data
     */
    validatePaymentData(paymentData) {
        const errors = {};

        if (!paymentData.amount || paymentData.amount <= 0) {
            errors.amount = 'ظ…ط¨ظ„ط؛ ظ¾ط±ط¯ط§ط®طھ ط¨ط§غŒط¯ ط¨غŒط´طھط± ط§ط² طµظپط± ط¨ط§ط´ط¯';
        }

        if (!paymentData.paymentMethodId) {
            errors.paymentMethodId = 'ط§ظ†طھط®ط§ط¨ ط±ظˆط´ ظ¾ط±ط¯ط§ط®طھ ط§ظ„ط²ط§ظ…غŒ ط§ط³طھ';
        }

        if (!paymentData.orderId) {
            errors.orderId = 'ط´ظ†ط§ط³ظ‡ ط³ظپط§ط±ط´ ط§ظ„ط²ط§ظ…غŒ ط§ط³طھ';
        }

        if (paymentData.description && paymentData.description.length > 500) {
            errors.description = 'طھظˆط¶غŒط­ط§طھ ظ†ظ…غŒâ€Œطھظˆط§ظ†ط¯ ط¨غŒط´طھط± ط§ط² غµغ°غ° ع©ط§ط±ط§ع©طھط± ط¨ط§ط´ط¯';
        }

        return {
            isValid: Object.keys(errors).length === 0,
            errors: errors
        };
    }

    /**
     * Format payment data for API
     */
    formatPaymentData(formData) {
        return {
            amount: parseFloat(formData.get('amount') || formData.amount || 0),
            paymentMethod: formData.get('paymentMethod') || formData.paymentMethod || 'online',
            orderId: formData.get('orderId') || formData.orderId || null,
            currency: formData.get('currency') || formData.currency || 'IRR'
        };
    }

    /**
     * Get payment status text
     */
    getPaymentStatusText(status) {
        const statusTexts = {
            'Pending': 'ط¯ط± ط§ظ†طھط¸ط§ط± ظ¾ط±ط¯ط§ط®طھ',
            'Processing': 'ط¯ط± ط­ط§ظ„ ظ¾ط±ط¯ط§ط²ط´',
            'Completed': 'طھع©ظ…غŒظ„ ط´ط¯ظ‡',
            'Failed': 'ظ†ط§ظ…ظˆظپظ‚',
            'Cancelled': 'ظ„ط؛ظˆ ط´ط¯ظ‡',
            'Refunded': 'ط¨ط§ط²ظ¾ط±ط¯ط§ط®طھ ط´ط¯ظ‡'
        };
        return statusTexts[status] || status;
    }

    /**
     * Get payment status color
     */
    getPaymentStatusColor(status) {
        const statusColors = {
            'Pending': 'text-yellow-600 bg-yellow-100',
            'Processing': 'text-blue-600 bg-blue-100',
            'Completed': 'text-green-600 bg-green-100',
            'Failed': 'text-red-600 bg-red-100',
            'Cancelled': 'text-gray-600 bg-gray-100',
            'Refunded': 'text-purple-600 bg-purple-100'
        };
        return statusColors[status] || 'text-gray-600 bg-gray-100';
    }
}

// Create global instance
window.paymentService = new PaymentService();

