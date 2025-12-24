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
                error: error.message || 'خطا در دریافت اطلاعات پرداخت'
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
                error: error.message || 'خطا در دریافت اطلاعات پرداخت سفارش'
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
                error: error.message || 'خطا در پردازش پرداخت'
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
                error: error.message || 'خطا در تایید پرداخت'
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
                error: error.message || 'خطا در لغو پرداخت'
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
                error: error.message || 'خطا در دریافت روش‌های پرداخت'
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
                error: error.message || 'خطا در دریافت تاریخچه پرداخت‌ها'
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
                error: error.message || 'خطا در دریافت جزئیات پرداخت'
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
                error: error.message || 'خطا در بازپرداخت'
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
            errors.paymentMethodId = 'انتخاب روش پرداخت الزامی است';
        }

        if (!paymentData.orderId) {
            errors.orderId = 'شناسه سفارش الزامی است';
        }

        if (paymentData.description && paymentData.description.length > 500) {
            errors.description = 'توضیحات نمی‌تواند بیش‌تر از ۵۰۰ کاراکتر باشد';
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
            'Pending': 'در انتظار پرداخت',
            'Processing': 'در حال پردازش',
            'Completed': 'تکمیل شده',
            'Failed': 'ناموفق',
            'Cancelled': 'لغو شده',
            'Refunded': 'بازپرداخت شده'
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

