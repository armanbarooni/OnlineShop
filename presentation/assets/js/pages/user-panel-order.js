const OrderManager = {
    state: {
        currentPage: 1,
        pageSize: 10,
        filters: {
            orderStatus: null
        }
    },

    init: async function () {
        this.initUI();
        await this.checkAuthAndLoad();
    },

    initUI: function () {
        // Header Init
        if (window.headerComponent) window.headerComponent.init();

        // Sidebar & Dropdown Toggles (Centralized)
        window.toggleUserDropdown = () => {
            const menu = document.getElementById('user-dropdown-menu');
            const icon = document.getElementById('user-dropdown-icon');
            if (menu) menu.classList.toggle('hidden');
            if (icon) icon.classList.toggle('rotate-180');
        };

        window.toggleOffcanvas = (id) => {
            const el = document.getElementById(id);
            const overlay = document.querySelector('.overlay');
            if (el) el.classList.remove('invisible', 'opacity-0', '-translate-x-full');
            if (overlay) overlay.classList.remove('hidden');
        };

        window.closeOffcanvas = () => {
            document.querySelectorAll('.offcanvas').forEach(el => {
                el.classList.add('invisible', 'opacity-0', '-translate-x-full');
            });
            const overlay = document.querySelector('.overlay');
            if (overlay) overlay.classList.add('hidden');
        };

        // Close dropdown on outside click
        document.addEventListener('click', (event) => {
            const menu = document.getElementById('user-dropdown-menu');
            const btn = document.getElementById('user-dropdown-button');
            if (menu && !menu.classList.contains('hidden') && !btn.contains(event.target) && !menu.contains(event.target)) {
                menu.classList.add('hidden');
                const icon = document.getElementById('user-dropdown-icon');
                if (icon) icon.classList.remove('rotate-180');
            }
        });

        // Logout Handler
        const handleLogout = (e) => {
            e.preventDefault();
            if (confirm('آیا مطمئن هستید که می‌خواهید خارج شوید؟')) {
                if (window.authService) window.authService.logout();
                else window.location.href = 'login.html';
            }
        };
        document.querySelectorAll('#logoutButton').forEach(btn => btn.addEventListener('click', handleLogout));

        // Filter Buttons
        document.querySelectorAll('[data-filter]').forEach(btn => {
            btn.addEventListener('click', (e) => this.handleFilterClick(e));
        });
    },

    checkAuthAndLoad: async function () {
        if (window.authService && !window.authService.isAuthenticated()) {
            window.location.href = 'login.html';
            return;
        }

        // Load User Data into Sidebar
        if (window.userProfileService) {
            try {
                const profile = await window.userProfileService.getUserProfile();
                if (profile.success && profile.data) {
                    document.querySelectorAll('[data-user-name="true"]').forEach(el => {
                        el.textContent = `${profile.data.firstName || ''} ${profile.data.lastName || ''}`.trim() || 'کاربر گرامی';
                    });
                    if (profile.data.profilePictureUrl) {
                        document.querySelectorAll('img[alt="پروفایل کاربر"]').forEach(img => img.src = profile.data.profilePictureUrl);
                    }
                }
            } catch (e) { console.error('Profile load error', e); }
        }

        await this.loadOrders();
    },

    loadOrders: async function () {
        const tbody = document.querySelector('tbody');
        if (!tbody) return;

        tbody.innerHTML = '<tr><td colspan="5" class="px-6 py-4 text-center text-sm text-gray-500">در حال بارگذاری...</td></tr>';

        try {
            const result = await window.orderService.searchOrders({}, {
                pageNumber: this.state.currentPage,
                pageSize: this.state.pageSize,
                status: this.state.filters.orderStatus
            });

            if (result.success && result.data) {
                // Handle different data structures (PagedResult or direct array)
                let items = [];
                let total = 0;

                if (result.data.items) {
                    items = result.data.items;
                    total = result.data.totalCount || result.data.totalItems || items.length;
                } else if (Array.isArray(result.data)) {
                    items = result.data;
                    total = items.length;
                } else if (result.data.data && result.data.data.items) { // Wrapper
                    items = result.data.data.items;
                    total = result.data.data.totalCount || items.length;
                }

                this.renderOrders(items);
                this.renderPagination(total);
            } else {
                tbody.innerHTML = '<tr><td colspan="5" class="px-6 py-4 text-center text-sm text-red-500">خطا در بارگذاری سفارشات</td></tr>';
            }
        } catch (error) {
            console.error('Orders load error:', error);
            tbody.innerHTML = '<tr><td colspan="5" class="px-6 py-4 text-center text-sm text-red-500">خطا در ارتباط با سرور</td></tr>';
        }
    },

    renderOrders: function (orders) {
        const tbody = document.querySelector('tbody');
        if (!orders || orders.length === 0) {
            tbody.innerHTML = '<tr><td colspan="5" class="px-6 py-4 text-center text-sm text-gray-500">هیچ سفارشی یافت نشد.</td></tr>';
            return;
        }

        tbody.innerHTML = orders.map(order => `
            <tr class="hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors">
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white">
                    ${order.orderNumber || order.id}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                    ${new Date(order.orderDate).toLocaleDateString('fa-IR')}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white font-bold">
                    ${new Intl.NumberFormat('fa-IR').format(order.totalAmount)} تومان
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                     <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${this.getStatusClass(order.status)}">
                        ${this.getStatusText(order.status)}
                     </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                    <button onclick="OrderManager.showDetails('${order.id}')" class="text-primary hover:text-primary-dark ml-2">جزئیات</button>
                    ${this.canTrack(order.status) ? `<button onclick="OrderManager.trackOrder('${order.id}')" class="text-blue-600 hover:text-blue-800 ml-2">رهگیری</button>` : ''}
                </td>
            </tr>
        `).join('');
    },

    getStatusClass: function (status) {
        switch (status) {
            case 'Pending': return 'bg-yellow-100 text-yellow-800';
            case 'Processing': return 'bg-blue-100 text-blue-800';
            case 'Shipped': return 'bg-purple-100 text-purple-800';
            case 'Delivered': return 'bg-green-100 text-green-800';
            case 'Cancelled': return 'bg-red-100 text-red-800';
            default: return 'bg-gray-100 text-gray-800';
        }
    },

    getStatusText: function (status) {
        const map = {
            'Pending': 'در انتظار پرداخت',
            'Processing': 'در حال پردازش',
            'Shipped': 'ارسال شده',
            'Delivered': 'تحویل شده',
            'Cancelled': 'لغو شده',
            'Returned': 'مرجوع شده'
        };
        return map[status] || status;
    },

    canTrack: function (status) {
        return ['Shipped', 'Delivered'].includes(status);
    },

    renderPagination: function (totalItems) {
        const totalPages = Math.ceil(totalItems / this.state.pageSize);
        const container = document.getElementById('pagination');
        const countContainer = document.getElementById('orders-count');

        if (countContainer) countContainer.textContent = `نمایش ${(this.state.currentPage - 1) * this.state.pageSize + 1} تا ${Math.min(this.state.currentPage * this.state.pageSize, totalItems)} از ${totalItems} سفارش`;

        if (totalPages <= 1) {
            container.innerHTML = '';
            return;
        }

        let html = '';
        // Prev
        html += `<button onclick="OrderManager.changePage(${this.state.currentPage - 1})" ${this.state.currentPage === 1 ? 'disabled class="opacity-50 cursor-not-allowed px-3 py-1 border rounded"' : 'class="px-3 py-1 border rounded hover:bg-gray-100 dark:hover:bg-gray-700"'}><</button>`;

        // Simplified pages logic could be improved for many pages, but simple loop for now
        const startPage = Math.max(1, this.state.currentPage - 2);
        const endPage = Math.min(totalPages, this.state.currentPage + 2);

        if (startPage > 1) {
            html += `<button onclick="OrderManager.changePage(1)" class="px-3 py-1 border rounded hover:bg-gray-100 dark:hover:bg-gray-700">1</button>`;
            if (startPage > 2) html += `<span class="px-2">...</span>`;
        }

        for (let i = startPage; i <= endPage; i++) {
            if (i === this.state.currentPage) {
                html += `<button class="px-3 py-1 border rounded bg-primary text-white">${i}</button>`;
            } else {
                html += `<button onclick="OrderManager.changePage(${i})" class="px-3 py-1 border rounded hover:bg-gray-100 dark:hover:bg-gray-700">${i}</button>`;
            }
        }

        if (endPage < totalPages) {
            if (endPage < totalPages - 1) html += `<span class="px-2">...</span>`;
            html += `<button onclick="OrderManager.changePage(${totalPages})" class="px-3 py-1 border rounded hover:bg-gray-100 dark:hover:bg-gray-700">${totalPages}</button>`;
        }

        // Next
        html += `<button onclick="OrderManager.changePage(${this.state.currentPage + 1})" ${this.state.currentPage === totalPages ? 'disabled class="opacity-50 cursor-not-allowed px-3 py-1 border rounded"' : 'class="px-3 py-1 border rounded hover:bg-gray-100 dark:hover:bg-gray-700"'}>></button>`;

        container.innerHTML = html;
    },

    changePage: function (page) {
        if (page < 1) return;
        this.state.currentPage = page;
        this.loadOrders();
    },

    handleFilterClick: function (e) {
        const btn = e.target.closest('button');
        if (!btn) return;
        const status = btn.getAttribute('data-filter'); // Changed to match data-filter attribute in HTML

        // Update active state
        document.querySelectorAll('[data-filter]').forEach(b => {
            // Reset styles (assuming classes from original HTML)
            b.classList.remove('bg-primary/10', 'text-primary');
            b.classList.add('text-gray-600', 'hover:bg-gray-100');
        });
        btn.classList.remove('text-gray-600', 'hover:bg-gray-100');
        btn.classList.add('bg-primary/10', 'text-primary');

        this.state.filters.orderStatus = (status === 'all' || !status) ? null : status;
        this.state.currentPage = 1;
        this.loadOrders();
    },

    showDetails: async function (orderId) {
        const listEl = document.getElementById('orders-list-view');
        const detailsEl = document.getElementById('order-details-view');
        const contentEl = document.getElementById('order-details-content');

        if (listEl) listEl.classList.add('hidden');
        if (detailsEl) detailsEl.classList.remove('hidden');
        if (contentEl) contentEl.innerHTML = '<div class="text-center py-8">در حال بارگذاری جزئیات...</div>';

        try {
            const result = await window.orderService.getOrderById(orderId);
            if (result.success && result.data) {
                this.renderDetails(result.data);
            } else {
                contentEl.innerHTML = '<div class="text-red-500 text-center py-8">خطا در دریافت جزئیات سفارش</div>';
            }
        } catch (e) { console.error(e); contentEl.innerHTML = '<div class="text-red-500 text-center py-8">خطا در ارتباط</div>'; }
    },

    renderDetails: function (order) {
        const contentEl = document.getElementById('order-details-content');

        // Helper for items
        const itemsHtml = (order.items || []).map(item => `
            <tr class="border-b dark:border-gray-700 last:border-0">
                <td class="py-4">
                     <div class="flex items-center">
                         ${item.imageUrl ? `<img src="${item.imageUrl}" class="w-12 h-12 rounded ml-3 object-cover">` : ''}
                         <div>
                            <div class="font-medium text-gray-900 dark:text-white">${item.productName || item.productTitle || 'محصول'}</div>
                         </div>
                    </div>
                </td>
                <td class="py-4 text-center text-gray-500">${item.quantity}</td>
                <td class="py-4 text-center text-gray-500">${new Intl.NumberFormat('fa-IR').format(item.unitPrice)} تومان</td>
                <td class="py-4 text-left font-bold text-gray-900 dark:text-white">
                    ${new Intl.NumberFormat('fa-IR').format(item.totalPrice || (item.price * item.quantity))} تومان
                </td>
            </tr>
         `).join('');

        contentEl.innerHTML = `
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
                <div>
                    <p class="text-sm text-gray-500 mb-1">شماره سفارش</p>
                    <p class="font-bold text-lg dark:text-white">${order.orderNumber || order.id}</p>
                </div>
                <div>
                    <p class="text-sm text-gray-500 mb-1">تاریخ ثبت</p>
                    <p class="font-bold text-lg dark:text-white">${new Date(order.orderDate).toLocaleDateString('fa-IR')}</p>
                </div>
                 <div>
                    <p class="text-sm text-gray-500 mb-1">وضعیت</p>
                    <span class="px-3 py-1 inline-flex text-sm font-semibold rounded-full ${this.getStatusClass(order.status)}">
                        ${this.getStatusText(order.status)}
                     </span>
                </div>
                <div>
                    <p class="text-sm text-gray-500 mb-1">مبلغ کل</p>
                    <p class="font-bold text-lg text-primary">${new Intl.NumberFormat('fa-IR').format(order.totalAmount)} تومان</p>
                </div>
            </div>
            
            <h4 class="font-bold text-lg mb-4 dark:text-gray-200">اقلام سفارش</h4>
            <div class="overflow-x-auto">
                <table class="w-full">
                    <thead class="bg-gray-50 dark:bg-gray-800 text-xs uppercase text-gray-500 font-medium">
                        <tr>
                            <th class="px-4 py-2 text-right rounded-r-lg">محصول</th>
                            <th class="px-4 py-2 text-center">تعداد</th>
                            <th class="px-4 py-2 text-center">قیمت واحد</th>
                            <th class="px-4 py-2 text-left rounded-l-lg">مبلغ کل</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${itemsHtml}
                    </tbody>
                </table>
            </div>
         `;
    },

    backToOrders: function () {
        const listEl = document.getElementById('orders-list-view');
        const detailsEl = document.getElementById('order-details-view');

        if (detailsEl) detailsEl.classList.add('hidden');
        if (listEl) listEl.classList.remove('hidden');
        this.loadOrders(); // Refresh to be safe
    },

    trackOrder: async function (orderId) {
        try {
            window.utils.showToast('در حال دریافت اطلاعات رهگیری...', 'info');
            const result = await window.orderService.trackOrder(orderId);
            if (result.success && result.data) {
                this.showTrackingModal(Array.isArray(result.data) ? result.data : [result.data]);
            } else {
                window.utils.showToast('اطلاعات رهگیری یافت نشد', 'error');
            }
        } catch (e) {
            window.utils.showToast('خطا در رهگیری سفارش', 'error');
        }
    },

    showTrackingModal: function (data) {
        // Implementation of modal display...
        alert(`وضعیت سفارش: ${data[0]?.status || 'نامشخص'}`); // Simplified for now, or implement full modal if needed within this object
    }
};

// Global Exposure
window.OrderManager = OrderManager;
window.backToOrders = () => OrderManager.backToOrders();

document.addEventListener('DOMContentLoaded', () => {
    OrderManager.init();
});
