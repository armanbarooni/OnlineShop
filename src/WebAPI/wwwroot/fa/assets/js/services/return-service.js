/**
 * Return Request Service for OnlineShop Frontend
 * Handles return request operations
 */

class ReturnService {
    constructor() {
        this.apiClient = window.apiClient;
    }

    /**
     * Get user return requests
     */
    async getReturnRequests() {
        try {
            const response = await this.apiClient.get('/userreturnrequest');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching return requests:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get return request by ID
     */
    async getReturnRequestById(returnRequestId) {
        try {
            const response = await this.apiClient.get(`/userreturnrequest/${returnRequestId}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching return request:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Create new return request
     */
    async createReturnRequest(returnRequestData) {
        try {
            const response = await this.apiClient.post('/userreturnrequest', returnRequestData);
            return {
                success: true,
                data: response.data || response,
                message: 'درخواست مرجوعی با موفقیت ثبت شد'
            };
        } catch (error) {
            window.logger.error('Error creating return request:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Update return request
     */
    async updateReturnRequest(returnRequestId, returnRequestData) {
        try {
            const response = await this.apiClient.put(`/userreturnrequest/${returnRequestId}`, returnRequestData);
            return {
                success: true,
                data: response.data || response,
                message: 'درخواست مرجوعی با موفقیت به‌روزرسانی شد'
            };
        } catch (error) {
            window.logger.error('Error updating return request:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Delete return request
     */
    async deleteReturnRequest(returnRequestId) {
        try {
            const response = await this.apiClient.delete(`/userreturnrequest/${returnRequestId}`);
            return {
                success: true,
                message: 'درخواست مرجوعی حذف شد'
            };
        } catch (error) {
            window.logger.error('Error deleting return request:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get return requests by status
     */
    async getReturnRequestsByStatus(status) {
        try {
            const response = await this.apiClient.get(`/userreturnrequest/status/${status}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching return requests by status:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get return requests by order ID
     */
    async getReturnRequestsByOrderId(orderId) {
        try {
            const response = await this.apiClient.get(`/userreturnrequest/order/${orderId}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching return requests by order:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Search return requests
     */
    async searchReturnRequests(query, filters = {}) {
        try {
            const searchCriteria = {
                searchTerm: query,
                ...filters
            };

            const response = await this.apiClient.post('/userreturnrequest/search', searchCriteria);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error searching return requests:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get return request statistics
     */
    async getReturnRequestStatistics() {
        try {
            const response = await this.apiClient.get('/userreturnrequest/statistics');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching return request statistics:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get return request status history
     */
    async getReturnRequestStatusHistory(returnRequestId) {
        try {
            const response = await this.apiClient.get(`/userreturnrequest/${returnRequestId}/status-history`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching return request status history:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Cancel return request
     */
    async cancelReturnRequest(returnRequestId, reason = '') {
        try {
            const response = await this.apiClient.post(`/userreturnrequest/${returnRequestId}/cancel`, {
                reason: reason
            });
            return {
                success: true,
                message: 'درخواست مرجوعی لغو شد'
            };
        } catch (error) {
            window.logger.error('Error cancelling return request:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get return request items
     */
    async getReturnRequestItems(returnRequestId) {
        try {
            const response = await this.apiClient.get(`/userreturnrequest/${returnRequestId}/items`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching return request items:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Add item to return request
     */
    async addItemToReturnRequest(returnRequestId, itemData) {
        try {
            const response = await this.apiClient.post(`/userreturnrequest/${returnRequestId}/items`, itemData);
            return {
                success: true,
                data: response.data || response,
                message: 'محصول به درخواست مرجوعی اضافه شد'
            };
        } catch (error) {
            window.logger.error('Error adding item to return request:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Remove item from return request
     */
    async removeItemFromReturnRequest(returnRequestId, itemId) {
        try {
            const response = await this.apiClient.delete(`/userreturnrequest/${returnRequestId}/items/${itemId}`);
            return {
                success: true,
                message: 'محصول از درخواست مرجوعی حذف شد'
            };
        } catch (error) {
            window.logger.error('Error removing item from return request:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Validate return request data
     */
    validateReturnRequestData(returnRequestData) {
        const errors = {};

        if (!returnRequestData.orderId) {
            errors.orderId = 'شناسه سفارش الزامی است';
        }

        if (!returnRequestData.reason || returnRequestData.reason.trim().length === 0) {
            errors.reason = 'دلیل مرجوعی الزامی است';
        } else if (returnRequestData.reason.trim().length < 10) {
            errors.reason = 'دلیل مرجوعی باید حداقل ۱۰ کاراکتر باشد';
        }

        if (!returnRequestData.items || !Array.isArray(returnRequestData.items) || returnRequestData.items.length === 0) {
            errors.items = 'حداقل یک محصول باید انتخاب شود';
        }

        return {
            isValid: Object.keys(errors).length === 0,
            errors: errors
        };
    }

    /**
     * Format return request status
     */
    formatReturnRequestStatus(status) {
        return window.utils.formatReturnStatus(status);
    }

    /**
     * Get return request status color
     */
    getReturnRequestStatusColor(status) {
        return window.utils.getReturnStatusColor(status);
    }

    /**
     * Check if return request can be cancelled
     */
    canCancelReturnRequest(returnRequest) {
        const cancellableStatuses = ['Pending', 'Approved'];
        return cancellableStatuses.includes(returnRequest.status);
    }

    /**
     * Check if return request can be edited
     */
    canEditReturnRequest(returnRequest) {
        return returnRequest.status === 'Pending';
    }

    /**
     * Get return request reasons
     */
    getReturnRequestReasons() {
        return [
            { value: 'defective', label: 'محصول معیوب' },
            { value: 'wrong_item', label: 'محصول اشتباه' },
            { value: 'not_as_described', label: 'مطابق توضیحات نبود' },
            { value: 'changed_mind', label: 'تغییر نظر' },
            { value: 'damaged_shipping', label: 'آسیب در حمل و نقل' },
            { value: 'other', label: 'ط³ط§غŒط±' }
        ];
    }

    /**
     * Get return request reason label
     */
    getReturnRequestReasonLabel(reason) {
        const reasons = this.getReturnRequestReasons();
        const foundReason = reasons.find(r => r.value === reason);
        return foundReason ? foundReason.label : reason;
    }

    /**
     * Calculate return request total
     */
    calculateReturnRequestTotal(returnRequest) {
        if (!returnRequest || !returnRequest.items) return 0;
        
        return returnRequest.items.reduce((total, item) => {
            return total + (item.price * item.quantity);
        }, 0);
    }

    /**
     * Get return request summary
     */
    getReturnRequestSummary(returnRequests) {
        if (!Array.isArray(returnRequests)) {
            return {
                total: 0,
                pending: 0,
                approved: 0,
                rejected: 0,
                completed: 0
            };
        }

        const summary = {
            total: returnRequests.length,
            pending: 0,
            approved: 0,
            rejected: 0,
            completed: 0
        };

        returnRequests.forEach(request => {
            switch (request.status) {
                case 'Pending':
                    summary.pending++;
                    break;
                case 'Approved':
                    summary.approved++;
                    break;
                case 'Rejected':
                    summary.rejected++;
                    break;
                case 'Completed':
                    summary.completed++;
                    break;
            }
        });

        return summary;
    }

    /**
     * Get return request by order and product
     */
    async getReturnRequestByOrderAndProduct(orderId, productId) {
        try {
            const response = await this.apiClient.get(`/userreturnrequest/order/${orderId}/product/${productId}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching return request by order and product:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Check if product can be returned
     */
    async canReturnProduct(orderId, productId) {
        try {
            const response = await this.apiClient.get(`/userreturnrequest/can-return/${orderId}/${productId}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error checking if product can be returned:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }
}

// Create global instance
window.returnService = new ReturnService();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ReturnService;
}

