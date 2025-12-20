// Product Service Module
class ProductService {
    constructor(apiClient) {
        this.apiClient = apiClient;
        this.products = null;
        this.cacheTime = null;
        this.CACHE_DURATION = 5 * 60 * 1000; // 5 minutes

        // Placeholder images (5 rotating images)
        this.placeholderImages = [
            '/fa/assets/images/products/placeholders/product-1.webp',
            '/fa/assets/images/products/placeholders/product-2.webp',
            '/fa/assets/images/products/placeholders/product-3.webp',
            '/fa/assets/images/products/placeholders/product-4.webp',
            '/fa/assets/images/products/placeholders/product-5.webp'
        ];
    }

    /**
     * Get placeholder image for a product
     * @param {number} productId - Product ID
     * @returns {string} Image URL
     */
    getPlaceholderImage(productId) {
        const index = productId % 5;
        return this.placeholderImages[index];
    }

    /**
     * Get product image (placeholder if no image exists)
     * @param {Object} product - Product object
     * @returns {string} Image URL
     */
    getProductImage(product) {
        // Check if product has images
        if (product.images && product.images.length > 0) {
            return product.images[0].imageUrl || product.images[0].url;
        }

        // Check if product has a single image property
        if (product.imageUrl || product.image) {
            return product.imageUrl || product.image;
        }

        // Return placeholder
        return this.getPlaceholderImage(product.id);
    }

    /**
     * Get all products from API or cache
     * @param {boolean} forceRefresh - Force refresh from API
     * @returns {Promise<Array>} Array of products
     */
    async getProducts(forceRefresh = false) {
        if (!forceRefresh && this.products && this.cacheTime &&
            (Date.now() - this.cacheTime < this.CACHE_DURATION)) {
            return this.products;
        }

        try {
            const response = await this.apiClient.get('/product');
            this.products = response.data || [];
            this.cacheTime = Date.now();
            return this.products;
        } catch (error) {
            console.error('Failed to fetch products:', error);
            if (this.products) {
                return this.products;
            }
            return [];
        }
    }

    /**
     * Get single product by ID
     * @param {number} id - Product ID
     * @returns {Promise<Object|null>} Product object or null
     */
    async getProductById(id) {
        try {
            const response = await this.apiClient.get(`/product/${id}`);
            return response.data;
        } catch (error) {
            console.error(`Failed to fetch product ${id}:`, error);
            if (this.products) {
                return this.products.find(p => p.id === id) || null;
            }
            return null;
        }
    }

    /**
     * Get products by category
     * @param {number} categoryId - Category ID
     * @returns {Promise<Array>} Array of products
     */
    async getProductsByCategory(categoryId) {
        try {
            const response = await this.apiClient.get(`/product?categoryId=${categoryId}`);
            return response.data || [];
        } catch (error) {
            console.error(`Failed to fetch products for category ${categoryId}:`, error);
            // Fallback to filtering cached products
            const allProducts = await this.getProducts();
            return allProducts.filter(p => p.categoryId === categoryId || p.productCategoryId === categoryId);
        }
    }

    /**
     * Search products
     * @param {string} query - Search query
     * @returns {Promise<Array>} Array of matching products
     */
    async searchProducts(query) {
        if (!query || query.trim() === '') {
            return [];
        }

        try {
            const response = await this.apiClient.get(`/product/search?q=${encodeURIComponent(query)}`);
            return response.data || [];
        } catch (error) {
            console.error('Search failed:', error);
            // Fallback to client-side search
            const allProducts = await this.getProducts();
            const searchTerm = query.toLowerCase();
            return allProducts.filter(p =>
                (p.name && p.name.toLowerCase().includes(searchTerm)) ||
                (p.title && p.title.toLowerCase().includes(searchTerm)) ||
                (p.description && p.description.toLowerCase().includes(searchTerm))
            );
        }
    }

    /**
     * Format price in Toman
     * @param {number} price - Price in Rials or Tomans
     * @returns {string} Formatted price
     */
    formatPrice(price) {
        if (!price) return '0 تومان';

        // Assuming price is in Tomans
        const formatted = new Intl.NumberFormat('fa-IR').format(price);
        return `${formatted} تومان`;
    }

    /**
     * Render product card
     * @param {Object} product - Product object
     * @returns {string} HTML string
     */
    renderProductCard(product) {
        const image = this.getProductImage(product);
        const price = this.formatPrice(product.price || product.unitPrice);
        const name = product.name || product.title || 'محصول بدون نام';
        const productId = product.id;

        return `
            <div class="product-card bg-white dark:bg-gray-800 rounded-lg shadow hover:shadow-xl transition-all duration-300">
                <a href="product.html?id=${productId}" class="block">
                    <div class="relative overflow-hidden rounded-t-lg">
                        <img src="${image}" alt="${name}" class="w-full h-64 object-cover hover:scale-110 transition-transform duration-300" loading="lazy" onerror="this.src='${this.getPlaceholderImage(productId)}'">
                        ${product.discount ? `<span class="absolute top-2 right-2 bg-red-500 text-white px-2 py-1 rounded text-sm">${product.discount}% تخفیف</span>` : ''}
                    </div>
                    <div class="p-4">
                        <h3 class="text-lg font-bold mb-2 line-clamp-2 dark:text-white">${name}</h3>
                        ${product.description ? `<p class="text-sm text-gray-600 dark:text-gray-400 mb-3 line-clamp-2">${product.description}</p>` : ''}
                        <div class="flex items-center justify-between">
                            <span class="text-xl font-bold text-primary">${price}</span>
                            <button onclick="window.cartService?.addToCart(${productId}, 1)" class="bg-primary-grad text-white px-4 py-2 rounded-lg hover:opacity-90 transition">
                                افزودن به سبد
                            </button>
                        </div>
                    </div>
                </a>
                <div class="px-4 pb-4">
                    <button onclick="window.wishlistService?.toggleWishlist(${productId})" class="w-full border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 px-4 py-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-5 inline-block ml-2">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.22 9 12 9 12s9-4.78 9-12z" />
                        </svg>
                        افزودن به علاقه‌مندی‌ها
                    </button>
                </div>
            </div>
        `;
    }

    /**
     * Render products grid
     * @param {string} containerId - Container element ID
     * @param {Array} products - Array of products
     * @param {number} limit - Maximum number of products
     */
    renderProductsGrid(containerId, products, limit = null) {
        const container = document.getElementById(containerId);
        if (!container) return;

        if (!products || products.length === 0) {
            container.innerHTML = '<p class="text-gray-500 p-4 col-span-full text-center">محصولی یافت نشد</p>';
            return;
        }

        const displayProducts = limit ? products.slice(0, limit) : products;
        const html = displayProducts.map(product => this.renderProductCard(product)).join('');
        container.innerHTML = html;
    }

    /**
     * Render featured products
     * @param {string} containerId - Container element ID
     * @param {number} limit - Number of products to show
     */
    async renderFeaturedProducts(containerId, limit = 8) {
        const products = await this.getProducts();
        // For now, just show first N products
        // In future, you can filter by featured flag
        this.renderProductsGrid(containerId, products, limit);
    }

    /**
     * Render products by category
     * @param {string} containerId - Container element ID
     * @param {number} categoryId - Category ID
     * @param {number} limit - Maximum number of products
     */
    async renderProductsByCategory(containerId, categoryId, limit = null) {
        const products = await this.getProductsByCategory(categoryId);
        this.renderProductsGrid(containerId, products, limit);
    }

    /**
     * Render search results
     * @param {string} containerId - Container element ID
     * @param {string} query - Search query
     */
    async renderSearchResults(containerId, query) {
        const products = await this.searchProducts(query);
        this.renderProductsGrid(containerId, products);
    }
}

// Export for use in other modules
if (typeof window !== 'undefined') {
    window.ProductService = ProductService;
}

// Create global instance
if (typeof window !== 'undefined' && window.apiClient) {
    window.productService = new ProductService(window.apiClient);
}
