/**
 * Order Service for OnlineShop Frontend
 * Handles order operations
 */

class OrderService {
    constructor() {
        this.apiClient = window.apiClient;
    }

    /**
     * Get user orders with pagination and filters
     */
    async getOrders(pageNumber = 1, pageSize = 10, filters = {}) {
        try {
            const searchCriteria = {
                pageNumber: pageNumber,
                pageSize: pageSize,
                ...filters
            };

            const response = await this.apiClient.post('/userorder/search', searchCriteria);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching orders:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get order by ID
     */
    async getOrderById(orderId) {
        try {
            const response = await this.apiClient.get(`/userorder/${orderId}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching order:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get recent orders
     */
    async getRecentOrders(limit = 5) {
        try {
            const response = await this.apiClient.get(`/userorder/recent?limit=${limit}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching recent orders:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Track order
     */
    async trackOrder(orderId) {
        try {
            const response = await this.apiClient.get(`/userorder/${orderId}/track`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error tracking order:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get order statistics
     */
    async getOrderStatistics() {
        try {
            const response = await this.apiClient.get('/userorder/statistics');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching order statistics:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Cancel order
     */
    async cancelOrder(orderId, reason = '') {
        try {
            const response = await this.apiClient.post(`/userorder/${orderId}/cancel`, {
                reason: reason
            });
            return {
                success: true,
                message: 'ط³ظپط§ط±ط´ ط¨ط§ ظ…ظˆظپظ‚غŒطھ ظ„ط؛ظˆ ط´ط¯.'
            };
        } catch (error) {
            window.logger.error('Error cancelling order:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Download order invoice
     */
    async downloadInvoice(orderId) {
        try {
            const response = await fetch(`${this.apiClient.baseURL}/userorder/${orderId}/invoice`, {
                method: 'GET',
                headers: this.apiClient.getHeaders()
            });

            if (!response.ok) {
                throw new Error('Failed to download invoice');
            }

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `invoice-${orderId}.pdf`;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            document.body.removeChild(a);

            return {
                success: true,
                message: 'طµظˆط±طھط­ط³ط§ط¨ ط¨ط§ ظ…ظˆظپظ‚غŒطھ ط¯ط§ظ†ظ„ظˆط¯ ط´ط¯.'
            };
        } catch (error) {
            window.logger.error('Error downloading invoice:', error);
            return {
                success: false,
                error: 'ط¯ط§ظ†ظ„ظˆط¯ طµظˆط±طھط­ط³ط§ط¨ ط¨ط§ ط®ط·ط§ ظ…ظˆط§ط¬ظ‡ ط´ط¯.'
            };
        }
    }

    /**
     * Get order items
     */
    async getOrderItems(orderId) {
        try {
            const response = await this.apiClient.get(`/userorder/${orderId}/items`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching order items:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get order status history
     */
    async getOrderStatusHistory(orderId) {
        try {
            const response = await this.apiClient.get(`/userorder/${orderId}/status-history`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching order status history:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Search orders
     */
    async searchOrders(query, filters = {}) {
        try {
            const searchCriteria = {
                searchTerm: query,
                ...filters
            };

            const response = await this.apiClient.post('/userorder/search', searchCriteria);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error searching orders:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get orders by status
     */
    async getOrdersByStatus(status, pageNumber = 1, pageSize = 10) {
        try {
            const filters = { status: status };
            return await this.getOrders(pageNumber, pageSize, filters);
        } catch (error) {
            window.logger.error('Error fetching orders by status:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get orders by date range
     */
    async getOrdersByDateRange(startDate, endDate, pageNumber = 1, pageSize = 10) {
        try {
            const filters = {
                startDate: startDate,
                endDate: endDate
            };
            return await this.getOrders(pageNumber, pageSize, filters);
        } catch (error) {
            window.logger.error('Error fetching orders by date range:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Validate order data
     */
    validateOrderData(orderData) {
        const errors = {};

        if (!orderData.orderNumber || orderData.orderNumber.trim().length === 0) {
            errors.orderNumber = 'ط´ظ…ط§ط±ظ‡ ط³ظپط§ط±ط´ ط§ظ„ط²ط§ظ…غŒ ط§ط³طھ.';
        }

        if (!orderData.status) {
            errors.status = 'ظˆط¶ط¹غŒطھ ط³ظپط§ط±ط´ ط§ظ„ط²ط§ظ…غŒ ط§ط³طھ.';
        }

        if (orderData.totalAmount && orderData.totalAmount < 0) {
            errors.totalAmount = 'ظ…ط¨ظ„ط؛ ط³ظپط§ط±ط´ ظ†ظ…غŒâ€Œطھظˆط§ظ†ط¯ ظ…ظ†ظپغŒ ط¨ط§ط´ط¯.';
        }

        return {
            isValid: Object.keys(errors).length === 0,
            errors: errors
        };
    }

    /**
     * Format order status
     */
    formatOrderStatus(status) {
        return window.utils.formatOrderStatus(status);
    }

    /**
     * Get order status color
     */
    getOrderStatusColor(status) {
        return window.utils.getOrderStatusColor(status);
    }

    /**
     * Calculate order total
     */
    calculateOrderTotal(order) {
        if (!order || !order.items) return 0;
        
        return order.items.reduce((total, item) => {
            return total + (item.price * item.quantity);
        }, 0);
    }

    /**
     * Check if order can be cancelled
     */
    canCancelOrder(order) {
        const cancellableStatuses = ['Pending', 'Processing'];
        return cancellableStatuses.includes(order.status);
    }

    /**
     * Check if order can be returned
     */
    canReturnOrder(order) {
        const returnableStatuses = ['Delivered'];
        return returnableStatuses.includes(order.status) && 
               new Date(order.deliveredAt) > new Date(Date.now() - 7 * 24 * 60 * 60 * 1000); // Within 7 days
    }
}

// Create global instance
window.orderService = new OrderService();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = OrderService;
}

