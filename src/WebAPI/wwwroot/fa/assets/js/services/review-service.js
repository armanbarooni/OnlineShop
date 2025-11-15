/**
 * Review Service for OnlineShop Frontend
 * Handles product review operations
 */

class ReviewService {
    constructor() {
        this.apiClient = window.apiClient;
    }

    /**
     * Get user reviews
     */
    async getUserReviews() {
        try {
            const response = await this.apiClient.get('/productreview/user');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching user reviews:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get review by ID
     */
    async getReviewById(reviewId) {
        try {
            const response = await this.apiClient.get(`/productreview/${reviewId}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching review:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get reviews by product ID
     */
    async getReviewsByProductId(productId) {
        try {
            const response = await this.apiClient.get(`/productreview/product/${productId}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching product reviews:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Create new review
     */
    async createReview(reviewData) {
        try {
            const response = await this.apiClient.post('/productreview', reviewData);
            return {
                success: true,
                data: response.data || response,
                message: 'ظ†ط¸ط± ط´ظ…ط§ ط¨ط§ ظ…ظˆظپظ‚غŒطھ ط«ط¨طھ ط´ط¯'
            };
        } catch (error) {
            window.logger.error('Error creating review:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Update review
     */
    async updateReview(reviewId, reviewData) {
        try {
            const response = await this.apiClient.put(`/productreview/${reviewId}`, reviewData);
            return {
                success: true,
                data: response.data || response,
                message: 'ظ†ط¸ط± ط´ظ…ط§ ط¨ط§ ظ…ظˆظپظ‚غŒطھ ط¨ط±ظˆط²ط±ط³ط§ظ†غŒ ط´ط¯'
            };
        } catch (error) {
            window.logger.error('Error updating review:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Delete review
     */
    async deleteReview(reviewId) {
        try {
            const response = await this.apiClient.delete(`/productreview/${reviewId}`);
            return {
                success: true,
                message: 'ظ†ط¸ط± ط´ظ…ط§ ط­ط°ظپ ط´ط¯'
            };
        } catch (error) {
            window.logger.error('Error deleting review:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get review statistics
     */
    async getReviewStatistics() {
        try {
            const response = await this.apiClient.get('/productreview/statistics');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching review statistics:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get reviews with pagination
     */
    async getReviewsPaginated(pageNumber = 1, pageSize = 10, filters = {}) {
        try {
            const searchCriteria = {
                pageNumber: pageNumber,
                pageSize: pageSize,
                ...filters
            };

            const response = await this.apiClient.post('/productreview/search', searchCriteria);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching paginated reviews:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Search reviews
     */
    async searchReviews(query, filters = {}) {
        try {
            const searchCriteria = {
                searchTerm: query,
                ...filters
            };

            const response = await this.apiClient.post('/productreview/search', searchCriteria);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error searching reviews:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get reviews by rating
     */
    async getReviewsByRating(rating, pageNumber = 1, pageSize = 10) {
        try {
            const filters = { rating: rating };
            return await this.getReviewsPaginated(pageNumber, pageSize, filters);
        } catch (error) {
            window.logger.error('Error fetching reviews by rating:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get reviews by status
     */
    async getReviewsByStatus(status, pageNumber = 1, pageSize = 10) {
        try {
            const filters = { status: status };
            return await this.getReviewsPaginated(pageNumber, pageSize, filters);
        } catch (error) {
            window.logger.error('Error fetching reviews by status:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Validate review data
     */
    validateReviewData(reviewData) {
        const errors = {};

        if (!reviewData.productId) {
            errors.productId = 'ط´ظ†ط§ط³ظ‡ ظ…ط­طµظˆظ„ ط§ظ„ط²ط§ظ…غŒ ط§ط³طھ';
        }

        if (!reviewData.rating || reviewData.rating < 1 || reviewData.rating > 5) {
            errors.rating = 'ط§ظ…طھغŒط§ط² ط¨ط§غŒط¯ ط¨غŒظ† غ± طھط§ غµ ط¨ط§ط´ط¯';
        }

        if (!reviewData.comment || reviewData.comment.trim().length === 0) {
            errors.comment = 'ظ†ط¸ط± ط§ظ„ط²ط§ظ…غŒ ط§ط³طھ';
        } else if (reviewData.comment.trim().length < 10) {
            errors.comment = 'ظ†ط¸ط± ط¨ط§غŒط¯ ط­ط¯ط§ظ‚ظ„ غ±غ° ع©ط§ط±ط§ع©طھط± ط¨ط§ط´ط¯';
        }

        if (reviewData.title && reviewData.title.trim().length > 100) {
            errors.title = 'ط¹ظ†ظˆط§ظ† ظ†ط¸ط± ظ†ظ…غŒâ€Œطھظˆط§ظ†ط¯ ط¨غŒط´ ط§ط² غ±غ°غ° ع©ط§ط±ط§ع©طھط± ط¨ط§ط´ط¯';
        }

        return {
            isValid: Object.keys(errors).length === 0,
            errors: errors
        };
    }

    /**
     * Format review status
     */
    formatReviewStatus(status) {
        const statusMap = {
            'Pending': 'ط¯ط± ط§ظ†طھط¸ط§ط± طھط§غŒغŒط¯',
            'Approved': 'طھط§غŒغŒط¯ ط´ط¯ظ‡',
            'Rejected': 'ط±ط¯ ط´ط¯ظ‡'
        };
        
        return statusMap[status] || status;
    }

    /**
     * Get review status color
     */
    getReviewStatusColor(status) {
        const colorMap = {
            'Pending': 'text-yellow-600 bg-yellow-100',
            'Approved': 'text-green-600 bg-green-100',
            'Rejected': 'text-red-600 bg-red-100'
        };
        
        return colorMap[status] || 'text-gray-600 bg-gray-100';
    }

    /**
     * Format rating stars
     */
    formatRatingStars(rating) {
        return window.utils.formatRatingStars(rating);
    }

    /**
     * Check if review can be edited
     */
    canEditReview(review) {
        return review.status === 'Pending' || review.status === 'Rejected';
    }

    /**
     * Check if review can be deleted
     */
    canDeleteReview(review) {
        return review.status === 'Pending' || review.status === 'Rejected';
    }

    /**
     * Get review summary
     */
    getReviewSummary(reviews) {
        if (!Array.isArray(reviews) || reviews.length === 0) {
            return {
                totalReviews: 0,
                averageRating: 0,
                ratingDistribution: {}
            };
        }

        const totalReviews = reviews.length;
        const totalRating = reviews.reduce((sum, review) => sum + review.rating, 0);
        const averageRating = totalRating / totalReviews;

        const ratingDistribution = {};
        for (let i = 1; i <= 5; i++) {
            ratingDistribution[i] = reviews.filter(review => review.rating === i).length;
        }

        return {
            totalReviews,
            averageRating: Math.round(averageRating * 10) / 10,
            ratingDistribution
        };
    }

    /**
     * Get review by product and user
     */
    async getReviewByProductAndUser(productId) {
        try {
            const response = await this.apiClient.get(`/productreview/product/${productId}/user`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching review by product and user:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Check if user can review product
     */
    async canUserReviewProduct(productId) {
        try {
            const response = await this.apiClient.get(`/productreview/can-review/${productId}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error checking if user can review product:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get review helpfulness
     */
    async getReviewHelpfulness(reviewId) {
        try {
            const response = await this.apiClient.get(`/productreview/${reviewId}/helpfulness`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching review helpfulness:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Mark review as helpful
     */
    async markReviewAsHelpful(reviewId) {
        try {
            const response = await this.apiClient.post(`/productreview/${reviewId}/helpful`);
            return {
                success: true,
                message: 'ظ†ط¸ط± ط¨ظ‡ ط¹ظ†ظˆط§ظ† ظ…ظپغŒط¯ ط¹ظ„ط§ظ…طھâ€Œع¯ط°ط§ط±غŒ ط´ط¯'
            };
        } catch (error) {
            window.logger.error('Error marking review as helpful:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Report review
     */
    async reportReview(reviewId, reason) {
        try {
            const response = await this.apiClient.post(`/productreview/${reviewId}/report`, {
                reason: reason
            });
            return {
                success: true,
                message: 'ع¯ط²ط§ط±ط´ ط´ظ…ط§ ط§ط±ط³ط§ظ„ ط´ط¯'
            };
        } catch (error) {
            window.logger.error('Error reporting review:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }
}

// Create global instance
window.reviewService = new ReviewService();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ReviewService;
}

