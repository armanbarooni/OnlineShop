/**
 * Ticket Service for OnlineShop Frontend
 * Handles ticket operations
 */

class TicketService {
    constructor() {
        this.apiClient = window.apiClient;
    }

    /**
     * Get user tickets
     */
    async getTickets(searchCriteria = {}) {
        try {
            const response = await this.apiClient.post('/ticket/search', searchCriteria);
            return {
                success: true,
                data: response.data || response // Assuming API returns { items: [], totalCount: 0 } or list
            };
        } catch (error) {
            window.logger.error('Error fetching tickets:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get ticket details
     */
    async getTicket(id) {
        try {
            const response = await this.apiClient.get(`/ticket/${id}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching ticket details:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Create a new ticket
     */
    async createTicket(ticketData) {
        try {
            const response = await this.apiClient.post('/ticket', ticketData);
            return {
                success: true,
                data: response.data || response,
                message: 'تیکت با موفقیت ثبت شد'
            };
        } catch (error) {
            window.logger.error('Error creating ticket:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Reply to a ticket
     */
    async replyToTicket(id, message, hasAttachment = false, attachmentId = null) {
        try {
            const payload = { message, hasAttachment, attachmentId };
            const response = await this.apiClient.post(`/ticket/${id}/reply`, payload);
            return {
                success: true,
                data: response.data || response,
                message: 'پاسخ شما با موفقیت ثبت شد'
            };
        } catch (error) {
            window.logger.error('Error replying to ticket:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get ticket departments
     */
    async getDepartments() {
        try {
            const response = await this.apiClient.get('/ticket/departments');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching departments:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Close a ticket
     */
    async closeTicket(id) {
        try {
            const response = await this.apiClient.put(`/ticket/${id}/close`);
            return {
                success: true,
                message: 'تیکت بسته شد'
            };
        } catch (error) {
            window.logger.error('Error closing ticket:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }
}

// Create global instance
window.ticketService = new TicketService();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = TicketService;
}
