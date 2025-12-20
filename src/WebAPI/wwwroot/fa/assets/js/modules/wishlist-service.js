// Wishlist Service Module
class WishlistService {
    constructor(apiClient) {
        this.apiClient = apiClient;
        this.wishlist = this.loadWishlistFromStorage();
    }

    /**
     * Load wishlist from localStorage
     * @returns {Array} Product IDs in wishlist
     */
    loadWishlistFromStorage() {
        try {
            const stored = localStorage.getItem('wishlist');
            return stored ? JSON.parse(stored) : [];
        } catch (error) {
            console.error('Failed to load wishlist from storage:', error);
            return [];
        }
    }

    /**
     * Save wishlist to localStorage
     */
    saveWishlistToStorage() {
        try {
            localStorage.setItem('wishlist', JSON.stringify(this.wishlist));
        } catch (error) {
            console.error('Failed to save wishlist to storage:', error);
        }
    }

    /**
     * Add product to wishlist
     * @param {number} productId - Product ID
     */
    async addToWishlist(productId) {
        if (!this.wishlist.includes(productId)) {
            this.wishlist.push(productId);
            this.saveWishlistToStorage();
            await this.syncWithBackend();
            this.showNotification('محصول به علاقه‌مندی‌ها اضافه شد', 'success');
            return true;
        }
        return false;
    }

    /**
     * Remove product from wishlist
     * @param {number} productId - Product ID
     */
    async removeFromWishlist(productId) {
        this.wishlist = this.wishlist.filter(id => id !== productId);
        this.saveWishlistToStorage();
        await this.syncWithBackend();
        this.showNotification('محصول از علاقه‌مندی‌ها حذف شد', 'info');
        return true;
    }

    /**
     * Toggle product in wishlist
     * @param {number} productId - Product ID
     */
    async toggleWishlist(productId) {
        if (this.isInWishlist(productId)) {
            return await this.removeFromWishlist(productId);
        } else {
            return await this.addToWishlist(productId);
        }
    }

    /**
     * Check if product is in wishlist
     * @param {number} productId - Product ID
     * @returns {boolean} True if in wishlist
     */
    isInWishlist(productId) {
        return this.wishlist.includes(productId);
    }

    /**
     * Get wishlist
     * @returns {Array} Product IDs
     */
    getWishlist() {
        return this.wishlist;
    }

    /**
     * Get wishlist count
     * @returns {number} Number of items
     */
    getCount() {
        return this.wishlist.length;
    }

    /**
     * Sync with backend
     */
    async syncWithBackend() {
        if (!this.apiClient.isAuthenticated()) {
            return;
        }

        try {
            // await this.apiClient.post('/wishlist/sync', { productIds: this.wishlist });
        } catch (error) {
            console.error('Failed to sync wishlist:', error);
        }
    }

    /**
     * Load from backend
     */
    async loadFromBackend() {
        if (!this.apiClient.isAuthenticated()) {
            return;
        }

        try {
            const response = await this.apiClient.get('/wishlist');
            if (response.data && Array.isArray(response.data)) {
                this.wishlist = response.data.map(item => item.productId || item.id);
                this.saveWishlistToStorage();
            }
        } catch (error) {
            console.error('Failed to load wishlist from backend:', error);
        }
    }

    /**
     * Render wishlist
     * @param {string} containerId - Container element ID
     */
    async renderWishlist(containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;

        if (this.wishlist.length === 0) {
            container.innerHTML = `
                <div class="text-center py-12">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-24 mx-auto text-gray-400 mb-4">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.22 9 12 9 12s9-4.78 9-12z" />
                    </svg>
                    <h3 class="text-xl font-bold mb-2">لیست علاقه‌مندی‌های شما خالی است</h3>
                    <p class="text-gray-600 mb-4">محصولات مورد علاقه خود را اضافه کنید</p>
                    <a href="shop.html" class="inline-block bg-primary-grad text-white px-6 py-3 rounded-lg hover:opacity-90 transition">
                        مشاهده محصولات
                    </a>
                </div>
            `;
            return;
        }

        // Fetch product details
        const products = [];
        for (const productId of this.wishlist) {
            if (window.productService) {
                const product = await window.productService.getProductById(productId);
                if (product) {
                    products.push(product);
                }
            }
        }

        if (products.length === 0) {
            container.innerHTML = '<p class="text-gray-500 p-4">در حال بارگذاری...</p>';
            return;
        }

        // Render products grid
        if (window.productService) {
            window.productService.renderProductsGrid(containerId, products);
        }
    }

    /**
     * Show notification
     * @param {string} message - Message
     * @param {string} type - Type
     */
    showNotification(message, type = 'info') {
        console.log(`[${type.toUpperCase()}] ${message}`);
        if (window.showToast) {
            window.showToast(message, type);
        }
    }
}

// Export
if (typeof window !== 'undefined') {
    window.WishlistService = WishlistService;
}

// Create global instance
if (typeof window !== 'undefined' && window.apiClient) {
    window.wishlistService = new WishlistService(window.apiClient);
}
