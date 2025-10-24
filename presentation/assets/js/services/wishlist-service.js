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
            const response = await this.apiClient.get('/wishlist');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error fetching wishlist:', error);
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
            return {
                success: true,
                data: response.data || response,
                message: 'محصول به علاقه‌مندی‌ها اضافه شد'
            };
        } catch (error) {
            console.error('Error adding to wishlist:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Remove product from wishlist
     */
    async removeFromWishlist(wishlistId) {
        try {
            const response = await this.apiClient.delete(`/wishlist/${wishlistId}`);
            return {
                success: true,
                message: 'محصول از علاقه‌مندی‌ها حذف شد'
            };
        } catch (error) {
            console.error('Error removing from wishlist:', error);
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
            const response = await this.apiClient.delete(`/wishlist/product/${productId}`);
            return {
                success: true,
                message: 'محصول از علاقه‌مندی‌ها حذف شد'
            };
        } catch (error) {
            console.error('Error removing product from wishlist:', error);
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
            const response = await this.apiClient.get(`/wishlist/check/${productId}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error checking wishlist:', error);
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
            console.error('Error getting wishlist count:', error);
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
            const response = await this.apiClient.delete('/wishlist/clear');
            return {
                success: true,
                message: 'لیست علاقه‌مندی‌ها پاک شد'
            };
        } catch (error) {
            console.error('Error clearing wishlist:', error);
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
            const response = await this.apiClient.post(`/wishlist/${wishlistId}/move-to-cart`);
            return {
                success: true,
                message: 'محصول به سبد خرید اضافه شد'
            };
        } catch (error) {
            console.error('Error moving to cart:', error);
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
            const response = await this.apiClient.post('/wishlist/move-all-to-cart');
            return {
                success: true,
                message: 'تمام محصولات به سبد خرید اضافه شدند'
            };
        } catch (error) {
            console.error('Error moving all to cart:', error);
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
            const response = await this.apiClient.get('/wishlist/statistics');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error fetching wishlist statistics:', error);
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
            const response = await this.apiClient.get(`/wishlist/search?q=${encodeURIComponent(query)}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error searching wishlist:', error);
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
            const response = await this.apiClient.get(`/wishlist/category/${categoryId}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error fetching wishlist by category:', error);
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
            const response = await this.apiClient.get(`/wishlist/sort?sortBy=${sortBy}&sortOrder=${sortOrder}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error sorting wishlist:', error);
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
            const response = await this.apiClient.get(`/wishlist?pageNumber=${pageNumber}&pageSize=${pageSize}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error fetching paginated wishlist:', error);
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
            const response = await this.apiClient.get('/wishlist/categories');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error fetching wishlist categories:', error);
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
