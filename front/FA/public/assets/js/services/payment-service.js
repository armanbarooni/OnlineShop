/**
 * Payment Service
 * Handles all payment-related API calls
 */
class PaymentService {
    constructor() {
        this.baseUrl = '/api/payments';
    }

    /**
     * Initialize payment
     */
    async initializePayment(paymentData) {
        try {
            const response = await window.apiClient.post(`${this.baseUrl}/initialize`, paymentData);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error initializing payment:', error);
            return {
                success: false,
                error: error.message || 'خطا در شروع پرداخت'
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
            console.error('Error processing payment:', error);
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
            console.error('Error verifying payment:', error);
            return {
                success: false,
                error: error.message || 'خطا در تایید پرداخت'
            };
        }
    }

    /**
     * Get payment status
     */
    async getPaymentStatus(paymentId) {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/${paymentId}/status`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error getting payment status:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت وضعیت پرداخت'
            };
        }
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
            console.error('Error canceling payment:', error);
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
            console.error('Error getting payment methods:', error);
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
            console.error('Error getting payment history:', error);
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
            console.error('Error getting payment details:', error);
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
            console.error('Error refunding payment:', error);
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
            errors.amount = 'مبلغ پرداخت باید بیشتر از صفر باشد';
        }

        if (!paymentData.paymentMethodId) {
            errors.paymentMethodId = 'انتخاب روش پرداخت الزامی است';
        }

        if (!paymentData.orderId) {
            errors.orderId = 'شناسه سفارش الزامی است';
        }

        if (paymentData.description && paymentData.description.length > 500) {
            errors.description = 'توضیحات نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد';
        }

        return {
            isValid: Object.keys(errors).length === 0,
            errors: errors
        };
    }

    /**
     * Format payment data
     */
    formatPaymentData(formData) {
        return {
            amount: parseFloat(formData.get('amount')),
            paymentMethodId: parseInt(formData.get('paymentMethodId')),
            orderId: formData.get('orderId'),
            description: formData.get('description') || '',
            returnUrl: formData.get('returnUrl') || window.location.origin + '/success-payment.html',
            cancelUrl: formData.get('cancelUrl') || window.location.origin + '/fail-payment.html'
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
