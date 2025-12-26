// Cart Service Module
class CartService {
    constructor(apiClient) {
        this.apiClient = apiClient;
        this.cart = this.loadCartFromStorage();
        this.listeners = [];
    }

    /**
     * Load cart from localStorage
     * @returns {Array} Cart items
     */
    loadCartFromStorage() {
        try {
            const stored = localStorage.getItem('cart');
            return stored ? JSON.parse(stored) : [];
        } catch (error) {
            console.error('Failed to load cart from storage:', error);
            return [];
        }
    }

    /**
     * Save cart to localStorage
     */
    saveCartToStorage() {
        try {
            localStorage.setItem('cart', JSON.stringify(this.cart));
            this.notifyListeners();
        } catch (error) {
            console.error('Failed to save cart to storage:', error);
        }
    }

    /**
     * Add listener for cart changes
     * @param {Function} callback - Callback function
     */
    addListener(callback) {
        this.listeners.push(callback);
    }

    /**
     * Notify all listeners of cart changes
     */
    notifyListeners() {
        this.listeners.forEach(callback => {
            try {
                callback(this.cart);
            } catch (error) {
                console.error('Cart listener error:', error);
            }
        });

        // Update cart count in UI
        this.updateCartCount();
    }

    /**
     * Update cart count badge
     */
    updateCartCount() {
        const count = this.getItemCount();
        const badges = document.querySelectorAll('.cart-count, #cart-count');
        badges.forEach(badge => {
            badge.textContent = count;
            badge.style.display = count > 0 ? 'block' : 'none';
        });
    }

    /**
     * Add item to cart
     * @param {number} productId - Product ID
     * @param {number} quantity - Quantity to add
     * @param {Object} productData - Optional product data
     */
    async addToCart(productId, quantity = 1, productData = null) {
        try {
            // Find existing item
            const existingItem = this.cart.find(item => item.productId === productId);

            if (existingItem) {
                existingItem.quantity += quantity;
            } else {
                // Fetch product data if not provided
                if (!productData && window.productService) {
                    productData = await window.productService.getProductById(productId);
                }

                this.cart.push({
                    productId: productId,
                    quantity: quantity,
                    price: productData?.price || productData?.unitPrice || 0,
                    name: productData?.name || productData?.title || 'محصول',
                    image: productData ? window.productService?.getProductImage(productData) : null
                });
            }

            this.saveCartToStorage();

            // Try to sync with backend
            await this.syncWithBackend();

            // Show success message
            this.showNotification('محصول به سبد خرید اضافه شد', 'success');

            return true;
        } catch (error) {
            console.error('Failed to add to cart:', error);
            this.showNotification('خطا در افزودن به سبد خرید', 'error');
            return false;
        }
    }

    /**
     * Remove item from cart
     * @param {number} productId - Product ID
     */
    removeFromCart(productId) {
        this.cart = this.cart.filter(item => item.productId !== productId);
        this.saveCartToStorage();
        this.syncWithBackend();
        this.showNotification('محصول از سبد خرید حذف شد', 'info');
    }

    /**
     * Update item quantity
     * @param {number} productId - Product ID
     * @param {number} quantity - New quantity
     */
    updateQuantity(productId, quantity) {
        const item = this.cart.find(item => item.productId === productId);
        if (item) {
            if (quantity <= 0) {
                this.removeFromCart(productId);
            } else {
                item.quantity = quantity;
                this.saveCartToStorage();
                this.syncWithBackend();
            }
        }
    }

    /**
     * Clear cart
     */
    clearCart() {
        this.cart = [];
        this.saveCartToStorage();
        this.syncWithBackend();
    }

    /**
     * Get cart items
     * @returns {Array} Cart items
     */
    getCart() {
        return this.cart;
    }

    /**
     * Get total item count
     * @returns {number} Total items
     */
    getItemCount() {
        return this.cart.reduce((total, item) => total + item.quantity, 0);
    }

    /**
     * Get cart total price
     * @returns {number} Total price
     */
    getTotal() {
        return this.cart.reduce((total, item) => total + (item.price * item.quantity), 0);
    }

    /**
     * Format price
     * @param {number} price - Price
     * @returns {string} Formatted price
     */
    formatPrice(price) {
        const formatted = new Intl.NumberFormat('fa-IR').format(price);
        return `${formatted} تومان`;
    }

    /**
     * Sync cart with backend API
     */
    async syncWithBackend() {
        if (!this.apiClient.isAuthenticated()) {
            return; // Only sync for logged-in users
        }

        try {
            // This would sync with your backend cart API
            // await this.apiClient.post('/cart/sync', { items: this.cart });
        } catch (error) {
            console.error('Failed to sync cart with backend:', error);
        }
    }

    /**
     * Load cart from backend
     */
    async loadFromBackend() {
        if (!this.apiClient.isAuthenticated()) {
            return;
        }

        try {
            const response = await this.apiClient.get('/cart');
            if (response.data && Array.isArray(response.data)) {
                this.cart = response.data;
                this.saveCartToStorage();
            }
        } catch (error) {
            console.error('Failed to load cart from backend:', error);
        }
    }

    /**
     * Render cart items
     * @param {string} containerId - Container element ID
     */
    renderCart(containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;

        if (this.cart.length === 0) {
            container.innerHTML = `
                <div class="text-center py-12">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-24 mx-auto text-gray-400 mb-4">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M2.25 3h1.386c.51 0 .955.343 1.087.835l.383 1.437M7.5 14.25a3 3 0 0 0-3 3h15.75m-12.75-3h11.218c1.121-2.3 2.1-4.684 2.924-7.138a60.114 60.114 0 0 0-16.536-1.84M7.5 14.25 5.106 5.272M6 20.25a.75.75 0 1 1-1.5 0 .75.75 0 0 1 1.5 0Zm12.75 0a.75.75 0 1 1-1.5 0 .75.75 0 0 1 1.5 0Z" />
                    </svg>
                    <h3 class="text-xl font-bold mb-2">سبد خرید شما خالی است</h3>
                    <p class="text-gray-600 mb-4">برای افزودن محصول به سبد خرید، به صفحه محصولات بروید</p>
                    <a href="shop.html" class="inline-block bg-primary-grad text-white px-6 py-3 rounded-lg hover:opacity-90 transition">
                        مشاهده محصولات
                    </a>
                </div>
            `;
            return;
        }

        let html = '<div class="space-y-4">';

        this.cart.forEach(item => {
            html += `
                <div class="flex items-center gap-4 bg-white dark:bg-gray-800 p-4 rounded-lg shadow">
                    ${item.image ? `<img src="${item.image}" alt="${item.name}" class="w-20 h-20 object-cover rounded" onerror="this.src='${window.productService?.getPlaceholderImage(item.productId) || ''}'">` : ''}
                    <div class="flex-1">
                        <h4 class="font-bold mb-1">${item.name}</h4>
                        <p class="text-primary font-bold">${this.formatPrice(item.price)}</p>
                    </div>
                    <div class="flex items-center gap-2">
                        <button onclick="window.cartService.updateQuantity(${item.productId}, ${item.quantity - 1})" class="w-8 h-8 bg-gray-200 dark:bg-gray-700 rounded hover:bg-gray-300 dark:hover:bg-gray-600">-</button>
                        <span class="w-12 text-center font-bold">${item.quantity}</span>
                        <button onclick="window.cartService.updateQuantity(${item.productId}, ${item.quantity + 1})" class="w-8 h-8 bg-gray-200 dark:bg-gray-700 rounded hover:bg-gray-300 dark:hover:bg-gray-600">+</button>
                    </div>
                    <button onclick="window.cartService.removeFromCart(${item.productId})" class="text-red-500 hover:text-red-700">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-6">
                            <path stroke-linecap="round" stroke-linejoin="round" d="m14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0" />
                        </svg>
                    </button>
                </div>
            `;
        });

        html += '</div>';

        // Add total
        html += `
            <div class="mt-6 bg-gray-100 dark:bg-gray-800 p-4 rounded-lg">
                <div class="flex justify-between items-center mb-4">
                    <span class="text-lg">جمع کل:</span>
                    <span class="text-2xl font-bold text-primary">${this.formatPrice(this.getTotal())}</span>
                </div>
                <a href="checkout.html" class="block w-full bg-primary-grad text-white text-center px-6 py-3 rounded-lg hover:opacity-90 transition">
                    ادامه خرید
                </a>
            </div>
        `;

        container.innerHTML = html;
    }

    /**
     * Show notification
     * @param {string} message - Message to show
     * @param {string} type - Type (success, error, info)
     */
    showNotification(message, type = 'info') {
        // Simple notification - you can enhance this
        console.log(`[${type.toUpperCase()}] ${message}`);

        // You can add a toast notification here
        if (window.showToast) {
            window.showToast(message, type);
        }
    }
}

// Export for use in other modules
if (typeof window !== 'undefined') {
    window.CartService = CartService;
}

// Create global instance
if (typeof window !== 'undefined' && window.apiClient) {
    window.cartService = new CartService(window.apiClient);

    // Initialize cart count on page load
    document.addEventListener('DOMContentLoaded', () => {
        window.cartService.updateCartCount();
    });
}
