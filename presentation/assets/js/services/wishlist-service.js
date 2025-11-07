/**
 * Wishlist Service for OnlineShop Frontend
 * Handles wishlist operations
 */

class WishlistService {
    constructor() {
        this.apiClient = window.apiClient;
    }

    /**
     * Get user wishlist
     */
    async getWishlist() {
        try {
            // Get current user ID from token
            const token = localStorage.getItem('accessToken');
            if (!token) {
                return {
                    success: false,
                    error: 'User not authenticated'
                };
            }

            // Decode token to get user ID
            const payload = JSON.parse(atob(token.split('.')[1]));
            const userId = payload.sub || payload.nameid;

            const response = await this.apiClient.get(`/wishlist/user/${userId}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching wishlist:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Add product to wishlist
     */
    async addToWishlist(productId) {
        try {
            const response = await this.apiClient.post('/wishlist', {
                productId: productId
            });
            
            // Handle Result<T> wrapper
            if (response.isSuccess !== undefined) {
                return {
                    success: response.isSuccess,
                    data: response.data || response.data?.data,
                    message: 'ظ…ط­طµظˆظ„ ط¨ظ‡ ط¹ظ„ط§ظ‚ظ‡â€Œظ…ظ†ط¯غŒâ€Œظ‡ط§ ط§ط¶ط§ظپظ‡ ط´ط¯'
                };
            }
            
            return {
                success: true,
                data: response.data || response,
                message: 'ظ…ط­طµظˆظ„ ط¨ظ‡ ط¹ظ„ط§ظ‚ظ‡â€Œظ…ظ†ط¯غŒâ€Œظ‡ط§ ط§ط¶ط§ظپظ‡ ط´ط¯'
            };
        } catch (error) {
            window.logger.error('Error adding to wishlist:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Remove product from wishlist by wishlist item ID
     */
    async removeFromWishlist(wishlistId) {
        try {
            const response = await this.apiClient.delete(`/wishlist/${wishlistId}`);
            
            // Handle Result<T> wrapper
            if (response.isSuccess !== undefined) {
                return {
                    success: response.isSuccess,
                    message: 'ظ…ط­طµظˆظ„ ط§ط² ط¹ظ„ط§ظ‚ظ‡â€Œظ…ظ†ط¯غŒâ€Œظ‡ط§ ط­ط°ظپ ط´ط¯'
                };
            }
            
            return {
                success: true,
                message: 'ظ…ط­طµظˆظ„ ط§ط² ط¹ظ„ط§ظ‚ظ‡â€Œظ…ظ†ط¯غŒâ€Œظ‡ط§ ط­ط°ظپ ط´ط¯'
            };
        } catch (error) {
            window.logger.error('Error removing from wishlist:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Remove product from wishlist by product ID
     */
    async removeProductFromWishlist(productId) {
        try {
            // Get current user ID from token
            const token = localStorage.getItem('accessToken');
            if (!token) {
                return { success: false, error: 'ط§ط¨طھط¯ط§ ظˆط§ط±ط¯ ط­ط³ط§ط¨ ع©ط§ط±ط¨ط±غŒ ط´ظˆغŒط¯' };
            }

            // Decode token to get user ID
            const payload = JSON.parse(atob(token.split('.')[1]));
            const userId = payload.sub || payload.nameid;

            const response = await this.apiClient.delete(`/wishlist/user/${userId}/product/${productId}`);
            
            // Handle Result<T> wrapper
            if (response.isSuccess !== undefined) {
                return {
                    success: response.isSuccess,
                    message: 'ظ…ط­طµظˆظ„ ط§ط² ط¹ظ„ط§ظ‚ظ‡â€Œظ…ظ†ط¯غŒâ€Œظ‡ط§ ط­ط°ظپ ط´ط¯'
                };
            }
            
            return {
                success: true,
                message: 'ظ…ط­طµظˆظ„ ط§ط² ط¹ظ„ط§ظ‚ظ‡â€Œظ…ظ†ط¯غŒâ€Œظ‡ط§ ط­ط°ظپ ط´ط¯'
            };
        } catch (error) {
            window.logger.error('Error removing product from wishlist:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Check if product is in wishlist
     */
    async checkInWishlist(productId) {
        try {
            const response = await this.apiClient.get(`/api/Wishlist/check/${productId}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error checking wishlist:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get wishlist count
     */
    async getWishlistCount() {
        try {
            const response = await this.getWishlist();
            if (response.success) {
                const wishlist = response.data;
                return {
                    success: true,
                    count: Array.isArray(wishlist) ? wishlist.length : 0
                };
            }
            return response;
        } catch (error) {
            window.logger.error('Error getting wishlist count:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Clear entire wishlist
     */
    async clearWishlist() {
        try {
            const response = await this.apiClient.delete('/api/Wishlist/clear');
            return {
                success: true,
                message: 'ظ„غŒط³طھ ط¹ظ„ط§ظ‚ظ‡â€Œظ…ظ†ط¯غŒâ€Œظ‡ط§ ظ¾ط§ع© ط´ط¯'
            };
        } catch (error) {
            window.logger.error('Error clearing wishlist:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Move wishlist item to cart
     */
    async moveToCart(wishlistId) {
        try {
            const response = await this.apiClient.post(`/api/Wishlist/${wishlistId}/move-to-cart`);
            return {
                success: true,
                message: 'ظ…ط­طµظˆظ„ ط¨ظ‡ ط³ط¨ط¯ ط®ط±غŒط¯ ط§ط¶ط§ظپظ‡ ط´ط¯'
            };
        } catch (error) {
            window.logger.error('Error moving to cart:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Move all wishlist items to cart
     */
    async moveAllToCart() {
        try {
            const response = await this.apiClient.post('/api/Wishlist/move-all-to-cart');
            return {
                success: true,
                message: 'طھظ…ط§ظ… ظ…ط­طµظˆظ„ط§طھ ط¨ظ‡ ط³ط¨ط¯ ط®ط±غŒط¯ ط§ط¶ط§ظپظ‡ ط´ط¯ظ†ط¯'
            };
        } catch (error) {
            window.logger.error('Error moving all to cart:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get wishlist statistics
     */
    async getWishlistStatistics() {
        try {
            const response = await this.apiClient.get('/api/Wishlist/statistics');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching wishlist statistics:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Search wishlist items
     */
    async searchWishlist(query) {
        try {
            const response = await this.apiClient.get(`/api/Wishlist/search?q=${encodeURIComponent(query)}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error searching wishlist:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get wishlist by category
     */
    async getWishlistByCategory(categoryId) {
        try {
            const response = await this.apiClient.get(`/api/Wishlist/category/${categoryId}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching wishlist by category:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Sort wishlist
     */
    async sortWishlist(sortBy, sortOrder = 'asc') {
        try {
            const response = await this.apiClient.get(`/api/Wishlist/sort?sortBy=${sortBy}&sortOrder=${sortOrder}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error sorting wishlist:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get wishlist items with pagination
     */
    async getWishlistPaginated(pageNumber = 1, pageSize = 10) {
        try {
            const response = await this.apiClient.get(`/api/Wishlist?pageNumber=${pageNumber}&pageSize=${pageSize}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching paginated wishlist:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Validate wishlist data
     */
    validateWishlistData(wishlistData) {
        const errors = {};

        if (!wishlistData.productId) {
            errors.productId = 'ط´ظ†ط§ط³ظ‡ ظ…ط­طµظˆظ„ ط§ظ„ط²ط§ظ…غŒ ط§ط³طھ';
        }

        return {
            isValid: Object.keys(errors).length === 0,
            errors: errors
        };
    }

    /**
     * Format wishlist item for display
     */
    formatWishlistItem(item) {
        if (!item || !item.product) return null;
        
        return {
            id: item.id,
            productId: item.product.id,
            productName: item.product.name,
            productPrice: item.product.price,
            productImage: item.product.imageUrl,
            productCategory: item.product.category,
            addedAt: item.createdAt,
            isAvailable: item.product.isAvailable,
            isInStock: item.product.isInStock
        };
    }

    /**
     * Calculate wishlist total value
     */
    calculateWishlistTotal(wishlist) {
        if (!Array.isArray(wishlist)) return 0;
        
        return wishlist.reduce((total, item) => {
            if (item.product && item.product.price) {
                return total + item.product.price;
            }
            return total;
        }, 0);
    }

    /**
     * Get wishlist item by product ID
     */
    getWishlistItemByProductId(wishlist, productId) {
        if (!Array.isArray(wishlist)) return null;
        
        return wishlist.find(item => item.product && item.product.id === productId);
    }

    /**
     * Check if product is available in wishlist
     */
    isProductAvailableInWishlist(wishlist, productId) {
        const item = this.getWishlistItemByProductId(wishlist, productId);
        return item && item.product && item.product.isAvailable && item.product.isInStock;
    }

    /**
     * Get wishlist categories
     */
    async getWishlistCategories() {
        try {
            const response = await this.apiClient.get('/api/Wishlist/categories');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching wishlist categories:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }
}

// Create global instance
window.wishlistService = new WishlistService();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = WishlistService;
}

/**
 * Additional helper: getWishlistItems with search/filter support
 */
WishlistService.prototype.getWishlistItems = async function(criteria = {}) {
    try {
        // Get user wishlist
        const response = await this.getWishlist();
        
        if (!response.success) {
            return response;
        }
        
        let items = response.data;
        
        // Handle different response structures
        if (items && items.data) {
            items = Array.isArray(items.data) ? items.data : (items.data.items || []);
        } else if (!Array.isArray(items)) {
            items = items.items || [];
        }
        
        // Apply filters if provided
        if (criteria.searchTerm) {
            const searchTerm = criteria.searchTerm.toLowerCase();
            items = items.filter(item => {
                const product = item.product || item;
                const name = (product.name || '').toLowerCase();
                return name.includes(searchTerm);
            });
        }
        
        // Pagination
        const pageNumber = criteria.pageNumber || criteria.page || 1;
        const pageSize = criteria.pageSize || 20;
        const startIndex = (pageNumber - 1) * pageSize;
        const endIndex = startIndex + pageSize;
        const paginatedItems = items.slice(startIndex, endIndex);
        
        return {
            success: true,
            data: {
                items: paginatedItems,
                totalCount: items.length,
                pageNumber: pageNumber,
                pageSize: pageSize,
                totalPages: Math.ceil(items.length / pageSize)
            }
        };
    } catch (error) {
        window.logger.error('Error getting wishlist items:', error);
        return {
            success: false,
            error: this.apiClient.handleError(error)
        };
    }
};

