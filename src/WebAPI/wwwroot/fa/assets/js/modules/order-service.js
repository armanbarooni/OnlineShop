// Order Service Module
class OrderService {
    constructor(apiClient) {
        this.apiClient = apiClient;
    }

    /**
     * Get user orders
     * @returns {Promise<Array>} Array of orders
     */
    async getOrders() {
        try {
            const response = await this.apiClient.get('/userorder');
            return response.data || [];
        } catch (error) {
            console.error('Failed to fetch orders:', error);
            return [];
        }
    }

    /**
     * Get single order by ID
     * @param {number} id - Order ID
     * @returns {Promise<Object|null>} Order object or null
     */
    async getOrderById(id) {
        try {
            const response = await this.apiClient.get(`/userorder/${id}`);
            return response.data;
        } catch (error) {
            console.error(`Failed to fetch order ${id}:`, error);
            return null;
        }
    }

    /**
     * Create order (checkout)
     * @param {Object} orderData - Order data
     * @returns {Promise<Object|null>} Created order or null
     */
    async createOrder(orderData) {
        try {
            const response = await this.apiClient.post('/checkout', orderData);
            return response.data;
        } catch (error) {
            console.error('Failed to create order:', error);
            throw error;
        }
    }

    /**
     * Get order status badge HTML
     * @param {string} status - Order status
     * @returns {string} HTML badge
     */
    getStatusBadge(status) {
        const statusMap = {
            'Pending': { text: 'در انتظار', class: 'bg-yellow-100 text-yellow-800' },
            'Processing': { text: 'در حال پردازش', class: 'bg-blue-100 text-blue-800' },
            'Shipped': { text: 'ارسال شده', class: 'bg-purple-100 text-purple-800' },
            'Delivered': { text: 'تحویل داده شده', class: 'bg-green-100 text-green-800' },
            'Cancelled': { text: 'لغو شده', class: 'bg-red-100 text-red-800' },
            'Returned': { text: 'مرجوع شده', class: 'bg-gray-100 text-gray-800' }
        };

        const statusInfo = statusMap[status] || { text: status, class: 'bg-gray-100 text-gray-800' };
        return `<span class="px-3 py-1 rounded-full text-sm ${statusInfo.class}">${statusInfo.text}</span>`;
    }

    /**
     * Format date
     * @param {string} dateString - Date string
     * @returns {string} Formatted date
     */
    formatDate(dateString) {
        try {
            const date = new Date(dateString);
            return new Intl.DateTimeFormat('fa-IR').format(date);
        } catch (error) {
            return dateString;
        }
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
     * Render orders list
     * @param {string} containerId - Container element ID
     */
    async renderOrders(containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;

        container.innerHTML = '<p class="text-center py-4">در حال بارگذاری...</p>';

        const orders = await this.getOrders();

        if (!orders || orders.length === 0) {
            container.innerHTML = `
                <div class="text-center py-12">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-24 mx-auto text-gray-400 mb-4">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 10.5V6a3.75 3.75 0 1 0-7.5 0v4.5m11.356-1.993 1.263 12c.07.665-.45 1.243-1.119 1.243H4.25a1.125 1.125 0 0 1-1.12-1.243l1.264-12A1.125 1.125 0 0 1 5.513 7.5h12.974c.576 0 1.059.435 1.119 1.007ZM8.625 10.5a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Zm7.5 0a.375.375 0 1 1-.75 0 .375.375 0 0 1 .75 0Z" />
                    </svg>
                    <h3 class="text-xl font-bold mb-2">شما هنوز سفارشی ندارید</h3>
                    <p class="text-gray-600 mb-4">برای ثبت سفارش، محصولات را به سبد خرید اضافه کنید</p>
                    <a href="shop.html" class="inline-block bg-primary-grad text-white px-6 py-3 rounded-lg hover:opacity-90 transition">
                        مشاهده محصولات
                    </a>
                </div>
            `;
            return;
        }

        let html = '<div class="space-y-4">';

        orders.forEach(order => {
            const orderDate = this.formatDate(order.createdAt || order.orderDate);
            const total = this.formatPrice(order.totalAmount || order.total || 0);
            const status = order.status || 'Pending';
            const orderId = order.id;
            const orderNumber = order.orderNumber || `#${orderId}`;

            html += `
                <div class="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
                    <div class="flex items-center justify-between mb-4">
                        <div>
                            <h3 class="font-bold text-lg">سفارش ${orderNumber}</h3>
                            <p class="text-sm text-gray-600 dark:text-gray-400">${orderDate}</p>
                        </div>
                        ${this.getStatusBadge(status)}
                    </div>
                    <div class="flex items-center justify-between">
                        <div>
                            <p class="text-sm text-gray-600 dark:text-gray-400">مبلغ کل:</p>
                            <p class="text-xl font-bold text-primary">${total}</p>
                        </div>
                        <a href="user-panel-order-detail.html?id=${orderId}" class="bg-primary-grad text-white px-6 py-2 rounded-lg hover:opacity-90 transition">
                            مشاهده جزئیات
                        </a>
                    </div>
                </div>
            `;
        });

        html += '</div>';

        container.innerHTML = html;
    }

    /**
     * Render order details
     * @param {string} containerId - Container element ID
     * @param {number} orderId - Order ID
     */
    async renderOrderDetails(containerId, orderId) {
        const container = document.getElementById(containerId);
        if (!container) return;

        container.innerHTML = '<p class="text-center py-4">در حال بارگذاری...</p>';

        const order = await this.getOrderById(orderId);

        if (!order) {
            container.innerHTML = '<p class="text-center py-4 text-red-500">سفارش یافت نشد</p>';
            return;
        }

        const orderDate = this.formatDate(order.createdAt || order.orderDate);
        const total = this.formatPrice(order.totalAmount || order.total || 0);
        const status = order.status || 'Pending';
        const orderNumber = order.orderNumber || `#${order.id}`;

        let html = `
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow p-6 mb-6">
                <div class="flex items-center justify-between mb-6">
                    <div>
                        <h2 class="text-2xl font-bold mb-2">سفارش ${orderNumber}</h2>
                        <p class="text-gray-600 dark:text-gray-400">${orderDate}</p>
                    </div>
                    ${this.getStatusBadge(status)}
                </div>
                
                <div class="grid md:grid-cols-2 gap-6 mb-6">
                    <div>
                        <h3 class="font-bold mb-2">آدرس تحویل</h3>
                        <p class="text-gray-600 dark:text-gray-400">${order.shippingAddress || 'آدرسی ثبت نشده'}</p>
                    </div>
                    <div>
                        <h3 class="font-bold mb-2">روش پرداخت</h3>
                        <p class="text-gray-600 dark:text-gray-400">${order.paymentMethod || 'پرداخت آنلاین'}</p>
                    </div>
                </div>
                
                <div class="border-t dark:border-gray-700 pt-6">
                    <h3 class="font-bold mb-4">اقلام سفارش</h3>
                    <div class="space-y-3">
        `;

        // Render order items
        if (order.items && order.items.length > 0) {
            order.items.forEach(item => {
                const itemPrice = this.formatPrice(item.price || item.unitPrice || 0);
                const itemTotal = this.formatPrice((item.price || item.unitPrice || 0) * (item.quantity || 1));

                html += `
                    <div class="flex items-center gap-4 p-3 bg-gray-50 dark:bg-gray-700 rounded">
                        <div class="flex-1">
                            <p class="font-medium">${item.productName || item.name || 'محصول'}</p>
                            <p class="text-sm text-gray-600 dark:text-gray-400">تعداد: ${item.quantity || 1} × ${itemPrice}</p>
                        </div>
                        <p class="font-bold text-primary">${itemTotal}</p>
                    </div>
                `;
            });
        }

        html += `
                    </div>
                </div>
                
                <div class="border-t dark:border-gray-700 mt-6 pt-6">
                    <div class="flex justify-between items-center text-xl font-bold">
                        <span>جمع کل:</span>
                        <span class="text-primary">${total}</span>
                    </div>
                </div>
            </div>
        `;

        container.innerHTML = html;
    }
}

// Export
if (typeof window !== 'undefined') {
    window.OrderService = OrderService;
}

// Create global instance
if (typeof window !== 'undefined' && window.apiClient) {
    window.orderService = new OrderService(window.apiClient);
}
