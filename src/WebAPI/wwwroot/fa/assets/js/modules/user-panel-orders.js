import { ApiClient } from '../api-client.js';

class UserPanelOrders {
    constructor() {
        this.apiClient = new ApiClient('http://localhost:5000/api');
        this.orders = [];
        this.currentPage = 1;
        this.pageSize = 10;
        this.totalOrders = 0;

        this.init();
    }

    async init() {
        // Check if user is authenticated
        if (!this.apiClient.isAuthenticated()) {
            window.location.href = 'index.html';
            return;
        }

        // Load orders
        await this.loadOrders();
    }

    async loadOrders(page = 1) {
        try {
            this.currentPage = page;

            // Show loading state
            this.showLoading();

            // Fetch orders from API
            const response = await this.apiClient.get(`/order?page=${page}&pageSize=${this.pageSize}`);

            if (response && response.data) {
                this.orders = response.data.items || response.data || [];
                this.totalOrders = response.data.totalCount || this.orders.length;

                // Render orders
                this.renderOrders();
                this.renderPagination();
            } else {
                this.showEmptyState();
            }

        } catch (error) {
            console.error('Failed to load orders:', error);
            this.showError('خطا در بارگذاری سفارشات. لطفا دوباره تلاش کنید.');
        }
    }

    showLoading() {
        const tbody = document.querySelector('#orders-list-view tbody');
        if (!tbody) return;

        tbody.innerHTML = `
            <tr>
                <td colspan="5" class="px-6 py-12 text-center">
                    <div class="flex justify-center items-center">
                        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
                    </div>
                    <p class="mt-4 text-gray-500 dark:text-gray-400">در حال بارگذاری...</p>
                </td>
            </tr>
        `;
    }

    showEmptyState() {
        const tbody = document.querySelector('#orders-list-view tbody');
        if (!tbody) return;

        tbody.innerHTML = `
            <tr>
                <td colspan="5" class="px-6 py-12 text-center">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 mx-auto text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                    </svg>
                    <p class="mt-4 text-lg font-medium text-gray-900 dark:text-white">هنوز سفارشی ثبت نکرده‌اید</p>
                    <p class="mt-2 text-gray-500 dark:text-gray-400">برای خرید محصولات به فروشگاه بروید</p>
                    <a href="shop.html" class="mt-4 inline-block bg-primary-grad text-white px-6 py-2 rounded-lg hover:opacity-90 transition">
                        مشاهده محصولات
                    </a>
                </td>
            </tr>
        `;

        const countEl = document.getElementById('orders-count');
        if (countEl) countEl.textContent = 'سفارشی یافت نشد';
    }

    showError(message) {
        const tbody = document.querySelector('#orders-list-view tbody');
        if (!tbody) return;

        tbody.innerHTML = `
            <tr>
                <td colspan="5" class="px-6 py-12 text-center">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 mx-auto text-red-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    <p class="mt-4 text-lg font-medium text-gray-900 dark:text-white">${message}</p>
                    <button onclick="location.reload()" class="mt-4 inline-block bg-primary-grad text-white px-6 py-2 rounded-lg hover:opacity-90 transition">
                        تلاش مجدد
                    </button>
                </td>
            </tr>
        `;
    }

    renderOrders() {
        const tbody = document.querySelector('#orders-list-view tbody');
        if (!tbody) return;

        if (this.orders.length === 0) {
            this.showEmptyState();
            return;
        }

        tbody.innerHTML = this.orders.map(order => this.renderOrderRow(order)).join('');

        // Update count
        const countEl = document.getElementById('orders-count');
        if (countEl) {
            countEl.textContent = `نمایش ${this.orders.length} سفارش از ${this.totalOrders}`;
        }
    }

    renderOrderRow(order) {
        const statusBadge = this.getStatusBadge(order.status || order.orderStatus);
        const orderDate = this.formatDate(order.createdAt || order.orderDate);
        const orderNumber = order.orderNumber || order.id || 'N/A';
        const totalAmount = order.totalAmount || order.total || 0;

        return `
            <tr class="hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors">
                <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm font-medium text-gray-900 dark:text-white">#${orderNumber}</div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm text-gray-500 dark:text-gray-400">${orderDate}</div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                    <div class="text-sm font-medium text-gray-900 dark:text-white">${totalAmount.toLocaleString()} تومان</div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                    ${statusBadge}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <button onclick="window.userPanelOrders.viewOrderDetails(${order.id})" class="text-primary hover:text-primary-dark transition-colors">
                        جزئیات
                    </button>
                </td>
            </tr>
        `;
    }

    getStatusBadge(status) {
        const statusMap = {
            'Pending': { label: 'در انتظار پرداخت', class: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300' },
            'Processing': { label: 'در حال پردازش', class: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300' },
            'Shipped': { label: 'ارسال شده', class: 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300' },
            'Delivered': { label: 'تحویل داده شده', class: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300' },
            'Cancelled': { label: 'لغو شده', class: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300' },
            'Returned': { label: 'مرجوع شده', class: 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300' }
        };

        const statusInfo = statusMap[status] || { label: status || 'نامشخص', class: 'bg-gray-100 text-gray-800' };

        return `
            <span class="px-3 py-1 inline-flex text-xs leading-5 font-semibold rounded-full ${statusInfo.class}">
                ${statusInfo.label}
            </span>
        `;
    }

    formatDate(dateString) {
        if (!dateString) return 'نامشخص';

        try {
            const date = new Date(dateString);
            const options = { year: 'numeric', month: 'long', day: 'numeric' };
            return new Intl.DateTimeFormat('fa-IR', options).format(date);
        } catch (error) {
            return dateString;
        }
    }

    renderPagination() {
        const paginationEl = document.getElementById('pagination');
        if (!paginationEl) return;

        const totalPages = Math.ceil(this.totalOrders / this.pageSize);

        if (totalPages <= 1) {
            paginationEl.innerHTML = '';
            return;
        }

        let html = '';

        // Previous button
        if (this.currentPage > 1) {
            html += `
                <button onclick="window.userPanelOrders.loadOrders(${this.currentPage - 1})" 
                    class="px-3 py-1 text-sm bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded hover:bg-gray-50 dark:hover:bg-gray-600">
                    قبلی
                </button>
            `;
        }

        // Page numbers
        for (let i = 1; i <= totalPages; i++) {
            if (i === this.currentPage) {
                html += `
                    <button class="px-3 py-1 text-sm bg-primary text-white rounded">
                        ${i}
                    </button>
                `;
            } else if (i === 1 || i === totalPages || (i >= this.currentPage - 1 && i <= this.currentPage + 1)) {
                html += `
                    <button onclick="window.userPanelOrders.loadOrders(${i})" 
                        class="px-3 py-1 text-sm bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded hover:bg-gray-50 dark:hover:bg-gray-600">
                        ${i}
                    </button>
                `;
            } else if (i === this.currentPage - 2 || i === this.currentPage + 2) {
                html += `<span class="px-2">...</span>`;
            }
        }

        // Next button
        if (this.currentPage < totalPages) {
            html += `
                <button onclick="window.userPanelOrders.loadOrders(${this.currentPage + 1})" 
                    class="px-3 py-1 text-sm bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded hover:bg-gray-50 dark:hover:bg-gray-600">
                    بعدی
                </button>
            `;
        }

        paginationEl.innerHTML = html;
    }

    async viewOrderDetails(orderId) {
        try {
            // Show details view
            document.getElementById('orders-list-view').classList.add('hidden');
            document.getElementById('order-details-view').classList.remove('hidden');

            // Fetch order details
            const order = await this.apiClient.get(`/order/${orderId}`);

            if (order && order.data) {
                this.renderOrderDetails(order.data);
            }

        } catch (error) {
            console.error('Failed to load order details:', error);
            alert('خطا در بارگذاری جزئیات سفارش');
            this.backToOrders();
        }
    }

    renderOrderDetails(order) {
        const contentEl = document.getElementById('order-details-content');
        if (!contentEl) return;

        const orderItems = order.items || order.orderItems || [];

        contentEl.innerHTML = `
            <div class="space-y-6">
                <!-- Order Info -->
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                        <h4 class="text-sm font-medium text-gray-500 dark:text-gray-400">شماره سفارش</h4>
                        <p class="mt-1 text-lg font-semibold text-gray-900 dark:text-white">#${order.orderNumber || order.id}</p>
                    </div>
                    <div>
                        <h4 class="text-sm font-medium text-gray-500 dark:text-gray-400">تاریخ ثبت</h4>
                        <p class="mt-1 text-lg text-gray-900 dark:text-white">${this.formatDate(order.createdAt || order.orderDate)}</p>
                    </div>
                    <div>
                        <h4 class="text-sm font-medium text-gray-500 dark:text-gray-400">وضعیت</h4>
                        <div class="mt-1">${this.getStatusBadge(order.status || order.orderStatus)}</div>
                    </div>
                    <div>
                        <h4 class="text-sm font-medium text-gray-500 dark:text-gray-400">مبلغ کل</h4>
                        <p class="mt-1 text-lg font-semibold text-primary">${(order.totalAmount || order.total || 0).toLocaleString()} تومان</p>
                    </div>
                </div>
                
                <!-- Order Items -->
                <div>
                    <h4 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">محصولات سفارش</h4>
                    <div class="space-y-4">
                        ${orderItems.map(item => `
                            <div class="flex items-center gap-4 p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
                                <img src="${item.productImage || 'assets/images/product/product-1.jpeg'}" alt="${item.productName || item.name}" class="w-16 h-16 object-cover rounded">
                                <div class="flex-1">
                                    <h5 class="font-medium text-gray-900 dark:text-white">${item.productName || item.name}</h5>
                                    <p class="text-sm text-gray-500 dark:text-gray-400">تعداد: ${item.quantity}</p>
                                </div>
                                <div class="text-right">
                                    <p class="font-semibold text-gray-900 dark:text-white">${(item.price * item.quantity).toLocaleString()} تومان</p>
                                    <p class="text-sm text-gray-500 dark:text-gray-400">${item.price.toLocaleString()} × ${item.quantity}</p>
                                </div>
                            </div>
                        `).join('')}
                    </div>
                </div>
            </div>
        `;
    }

    backToOrders() {
        document.getElementById('orders-list-view').classList.remove('hidden');
        document.getElementById('order-details-view').classList.add('hidden');
    }
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
    window.userPanelOrders = new UserPanelOrders();
});

// Make backToOrders available globally
window.backToOrders = () => {
    if (window.userPanelOrders) {
        window.userPanelOrders.backToOrders();
    }
};
