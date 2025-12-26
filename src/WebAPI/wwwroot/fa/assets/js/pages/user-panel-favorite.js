const FavoriteManager = {
    state: {
        currentPage: 1,
        pageSize: 12,
        searchTerm: ''
    },

    init: async function () {
        this.initUI();
        await this.checkAuthAndLoad();
        this.setupEventListeners();
    },

    initUI: function () {
        // Initializes Header components
        if (window.headerComponent) window.headerComponent.init();

        // Dark Mode Toggle
        if (window.utils) {
            window.utils.initDarkMode('dark-mode-toggle');
        }

        // Standard offcanvas and dropdown toggles
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

        // Logout
        const handleLogout = (e) => {
            e.preventDefault();
            if (confirm('آیا مطمئن هستید که می‌خواهید خارج شوید؟')) {
                if (window.authService) window.authService.logout();
                else window.location.href = 'login.html';
            }
        };
        document.querySelectorAll('#logoutButton').forEach(btn => btn.addEventListener('click', handleLogout));
    },

    setupEventListeners: function () {
        // Search
        const searchForm = document.getElementById('searchForm');
        if (searchForm) {
            searchForm.addEventListener('submit', (e) => {
                e.preventDefault();
                const term = document.getElementById('searchInput')?.value || '';
                this.state.searchTerm = term;
                this.state.currentPage = 1;
                this.loadFavorites();
            });
        }
    },

    checkAuthAndLoad: async function () {
        if (window.authService && !window.authService.isAuthenticated()) {
            window.location.href = 'login.html';
            return;
        }

        // Load User Profile Data
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

        await this.loadFavorites();
    },

    loadFavorites: async function () {
        const container = document.getElementById('wishlistContainer');
        const stats = document.getElementById('wishlistStats');

        if (container) {
            container.innerHTML = `
                <div class="flex flex-col items-center justify-center py-12">
                    <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mb-4"></div>
                    <p class="text-gray-500">در حال بارگذاری...</p>
                </div>
             `;
        }

        try {
            const result = await window.wishlistService.getWishlistItems({
                pageNumber: this.state.currentPage,
                pageSize: this.state.pageSize,
                searchTerm: this.state.searchTerm
            });

            if (result.success && result.data) {
                const { items, totalCount } = result.data;

                // Update header stats
                if (stats && stats.querySelector('p')) {
                    stats.querySelector('p').textContent = `${totalCount} محصول در لیست علاقه‌مندی‌ها`;
                }

                this.renderFavorites(items);
                this.renderPagination(totalCount);
            } else {
                if (container) container.innerHTML = '<div class="text-center py-12 text-red-500">خطا در بارگذاری اطلاعات</div>';
            }
        } catch (error) {
            console.error('Favorites load error', error);
            if (container) container.innerHTML = '<div class="text-center py-12 text-red-500">خطا در ارتباط با سرور</div>';
        }
    },

    renderFavorites: function (items) {
        const container = document.getElementById('wishlistContainer');
        if (!container) return;

        if (!items || items.length === 0) {
            container.innerHTML = `
                <div class="text-center py-16 bg-white dark:bg-card-dark rounded-xl shadow-sm border border-gray-100 dark:border-gray-700">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 mx-auto text-gray-300 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4.318 6.318a4.5 4.5 0 016.364 0L12 7.636l1.318-1.318a4.5 4.5 0 116.364 6.364L12 21l-7.682-7.318a4.5 4.5 0 010-6.364z" />
                    </svg>
                    <p class="text-gray-500 dark:text-gray-400 text-lg">لیست علاقه‌مندی‌های شما خالی است</p>
                    <a href="index.html" class="inline-block mt-4 px-6 py-2 bg-primary text-white rounded-lg hover:bg-primary-dark transition-colors">
                        مشاهده محصولات
                    </a>
                </div>
            `;
            return;
        }

        // Render Grid
        const grid = document.createElement('div');
        grid.className = 'grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6';

        items.forEach(item => {
            // Handle different API response structures (item wrapper vs direct product)
            const product = item.product || item;
            const card = document.createElement('div');
            card.className = 'bg-white dark:bg-card-dark rounded-xl shadow-soft dark:shadow-soft-dark overflow-hidden border border-gray-100 dark:border-gray-700 hover:shadow-lg transition-shadow duration-300 group';

            const priceDisplay = window.utils.formatPrice(product.price);
            // Assume discount logic if available, otherwise just price

            card.innerHTML = `
                <div class="relative aspect-auto p-4 flex items-center justify-center bg-gray-50 dark:bg-gray-800">
                    <img src="${product.imageUrl || 'assets/images/placeholder.png'}" 
                         alt="${product.name}" 
                         class="object-contain h-48 w-full group-hover:scale-105 transition-transform duration-300">
                    
                    <button onclick="FavoriteManager.removeFromFavorites('${item.id || product.id}')" 
                            class="absolute top-2 right-2 p-2 bg-white/80 dark:bg-gray-900/80 rounded-full text-red-500 hover:bg-red-50 dark:hover:bg-red-900/30 transition-colors tooltip" 
                            title="حذف از لیست">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                            <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
                        </svg>
                    </button>
                </div>
                
                <div class="p-4">
                    <h3 class="font-bold text-gray-900 dark:text-light mb-2 line-clamp-2 h-12 text-sm">
                        <a href="product-details.html?id=${product.id}">${product.name}</a>
                    </h3>
                    
                    <div class="flex items-center justify-between mb-4">
                         <span class="text-primary font-bold">${priceDisplay} تومان</span>
                    </div>
                    
                    <button onclick="FavoriteManager.addToCart('${product.id}')" 
                            class="w-full py-2 flex items-center justify-center bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-primary hover:text-white transition-colors gap-2">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                             <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
                        </svg>
                        افزودن به سبد
                    </button>
                </div>
             `;
            grid.appendChild(card);
        });

        container.innerHTML = '';
        container.appendChild(grid);
    },

    renderPagination: function (totalItems) {
        const totalPages = Math.ceil(totalItems / this.state.pageSize);
        const container = document.getElementById('pagination');

        if (!container || totalPages <= 1) {
            if (container) container.innerHTML = '';
            return;
        }

        let html = '';
        html += `<button onclick="FavoriteManager.changePage(${this.state.currentPage - 1})" ${this.state.currentPage === 1 ? 'disabled class="px-3 py-1 mx-1 border rounded opacity-50 cursor-not-allowed"' : 'class="px-3 py-1 mx-1 border rounded hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-600 dark:text-gray-300"'}>Prev</button>`;

        for (let i = 1; i <= totalPages; i++) {
            if (i === this.state.currentPage) {
                html += `<button class="px-3 py-1 mx-1 border rounded bg-primary text-white">${i}</button>`;
            } else {
                html += `<button onclick="FavoriteManager.changePage(${i})" class="px-3 py-1 mx-1 border rounded hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-600 dark:text-gray-300">${i}</button>`;
            }
        }

        html += `<button onclick="FavoriteManager.changePage(${this.state.currentPage + 1})" ${this.state.currentPage === totalPages ? 'disabled class="px-3 py-1 mx-1 border rounded opacity-50 cursor-not-allowed"' : 'class="px-3 py-1 mx-1 border rounded hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-600 dark:text-gray-300"'}>Next</button>`;

        container.innerHTML = html;
    },

    changePage: function (page) {
        if (page < 1) return;
        this.state.currentPage = page;
        this.loadFavorites();
    },

    removeFromFavorites: async function (id) {
        if (!confirm('آیا از حذف این محصول مطمئن هستید؟')) return;

        // Try deleting by Item ID first, then by Product ID if needed (based on Service capabilities)
        // The service has `removeFromWishlist` (byId) and `removeProductFromWishlist` (byProductId)
        // Since we might have either depending on the data, let's try generic approach or assume ID passes correctly

        try {
            window.utils.showToast('در حال حذف...', 'info');
            let result = await window.wishlistService.removeFromWishlist(id);

            if (!result.success) {
                // Fallback: maybe it passed a product ID?
                result = await window.wishlistService.removeProductFromWishlist(id);
            }

            if (result.success) {
                window.utils.showToast('محصول با موفقیت حذف شد', 'success');
                this.loadFavorites();
            } else {
                window.utils.showToast('خطا در حذف محصول', 'error');
            }
        } catch (e) {
            window.utils.showToast('خطا در عملیات', 'error');
        }
    },

    addToCart: async function (productId) {
        try {
            window.utils.showToast('در حال افزودن به سبد...', 'info');
            if (window.cartService) {
                const result = await window.cartService.addToCart(productId, 1);
                if (result.success) {
                    window.utils.showToast('محصول به سبد خرید اضافه شد', 'success');
                    // Optional: Update cart count if a global function exists
                } else {
                    window.utils.showToast(result.error || 'خطا در افزودن به سبد', 'error');
                }
            }
        } catch (e) {
            window.utils.showToast('خطا در عملیات', 'error');
        }
    },

    clearAllWishlist: async function () {
        if (!confirm('آیا مطمئن هستید که می‌خواهید تمام لیست را پاک کنید؟')) return;

        try {
            const result = await window.wishlistService.clearWishlist();
            if (result.success) {
                window.utils.showToast('لیست علاقه‌مندی‌ها پاک شد', 'success');
                this.loadFavorites();
            } else {
                window.utils.showToast('خطا در پاکسازی لیست', 'error');
            }
        } catch (e) {
            console.error(e);
        }
    }
};

window.FavoriteManager = FavoriteManager;
window.clearAllWishlist = () => FavoriteManager.clearAllWishlist();

document.addEventListener('DOMContentLoaded', () => {
    FavoriteManager.init();
});
