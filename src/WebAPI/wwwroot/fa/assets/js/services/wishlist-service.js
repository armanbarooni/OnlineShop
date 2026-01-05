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
     * @param {string} userId - Optional user ID. If not provided, will be extracted from token
     */
    async getWishlist(userId = null) {
        try {
            let finalUserId = userId;

            // If userId not provided, try to get from token (fallback for backward compatibility)
            if (!finalUserId) {
                const token = localStorage.getItem('accessToken');
                if (!token) {
                    return {
                        success: false,
                        error: 'User not authenticated'
                    };
                }

                // Decode token to get user ID
                try {
                    const payload = JSON.parse(atob(token.split('.')[1]));
                    finalUserId = payload.nameidentifier;
                } catch (e) {
                    return {
                        success: false,
                        error: 'Invalid token format'
                    };
                }
            }

            if (!finalUserId) {
                return {
                    success: false,
                    error: 'User ID is required'
                };
            }

            const response = await this.apiClient.get(`/wishlist/user/${finalUserId}`);
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
                    message: 'محصول به علاقه‌مندی‌ها اضافه شد'
                };
            }
            
            return {
                success: true,
                data: response.data || response,
                message: 'محصول به علاقه‌مندی‌ها اضافه شد'
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
                    message: 'محصول از علاقه‌مندی‌ها حذف شد'
                };
            }
            
            return {
                success: true,
                message: 'محصول از علاقه‌مندی‌ها حذف شد'
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
                return { success: false, error: 'ابتدا وارد حساب کاربری شوید' };
            }

            // Decode token to get user ID
            const payload = JSON.parse(atob(token.split('.')[1]));
            const userId = payload.sub || payload.nameid;

            const response = await this.apiClient.delete(`/wishlist/user/${userId}/product/${productId}`);
            
            // Handle Result<T> wrapper
            if (response.isSuccess !== undefined) {
                return {
                    success: response.isSuccess,
                    message: 'محصول از علاقه‌مندی‌ها حذف شد'
                };
            }
            
            return {
                success: true,
                message: 'محصول از علاقه‌مندی‌ها حذف شد'
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
                message: 'لیست علاقه‌مندی‌ها پاک شد'
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
                message: 'محصول به سبد خرید اضافه شد'
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
                message: 'تمام محصولات به سبد خرید اضافه شدند'
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
            errors.productId = 'شناسه محصول الزامی است';
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
        // Get user wishlist with userId from criteria if provided
        const userId = criteria.userId || null;
        const response = await this.getWishlist(userId);
        
        if (!response.success) {
            return response;
        }
        
        let items = response.data;
        
        // Handle different response structures
        // If data is already an array, use it directly
        if (Array.isArray(items)) {
            // items is already an array, use it as is
        } else if (items && items.data) {
            // If data has a nested data property
            items = Array.isArray(items.data) ? items.data : (items.data.items || []);
        } else if (items && Array.isArray(items.items)) {
            // If data has an items property that is an array
            items = items.items;
        } else {
            // Fallback to empty array
            items = [];
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

