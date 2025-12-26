/**
 * Header Component
 * Manages common header functionality across all pages
 */

class HeaderComponent {
    constructor() {
        this.init();
    }

    async init() {
        // Update cart count on load
        await this.updateCartCount();
        await this.updateComparisonCount();
        await this.updateUserMenu();
        await this.loadCategoryMenu();
        this.setupEventListeners();
    }

    setupEventListeners() {
        // Search functionality
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            let searchTimeout;
            searchInput.addEventListener('input', (e) => {
                clearTimeout(searchTimeout);
                const query = e.target.value.trim();

                if (query.length < 2) {
                    const searchResults = document.getElementById('searchResults');
                    if (searchResults) searchResults.classList.add('hidden');
                    return;
                }

                searchTimeout = setTimeout(async () => {
                    await this.performSearch(query);
                }, 500);
            });

            searchInput.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    const query = searchInput.value.trim();
                    if (query) {
                        window.location.href = `shop.html?search=${encodeURIComponent(query)}`;
                    }
                }
            });

            // Search button click
            const searchButton = searchInput.nextElementSibling;
            if (searchButton) {
                searchButton.addEventListener('click', () => {
                    const query = searchInput.value.trim();
                    if (query) {
                        window.location.href = `shop.html?search=${encodeURIComponent(query)}`;
                    }
                });
            }
        }

        // Hide search results when clicking outside
        document.addEventListener('click', (e) => {
            const searchResults = document.getElementById('searchResults');
            const searchInput = document.getElementById('searchInput');
            if (searchResults && searchInput && 
                !searchResults.contains(e.target) && 
                !searchInput.contains(e.target) &&
                !e.target.closest('#searchInput')) {
                searchResults.classList.add('hidden');
            }
        });

        // Cart icon click
        const cartIcon = document.getElementById('cartIcon');
        if (cartIcon) {
            cartIcon.addEventListener('click', () => {
                if (window.authService && window.authService.isAuthenticated()) {
                    window.location.href = 'cart.html';
                } else {
                    window.location.href = 'login.html';
                }
            });
        }

        // Comparison icon click
        const comparisonIcon = document.getElementById('comparisonIcon');
        if (comparisonIcon) {
            comparisonIcon.addEventListener('click', () => {
                window.location.href = 'compare.html';
            });
        }
    }

    async performSearch(query) {
        try {
            if (!window.productService) return;

            const result = await window.productService.searchProducts({
                searchTerm: query,
                pageSize: 5
            });

            if (result.success && result.data) {
                const products = result.data.products || result.data.items || (Array.isArray(result.data) ? result.data : []);
                this.renderSearchResults(Array.isArray(products) ? products : []);
            }
        } catch (error) {
            window.logger.error('Error searching:', error);
        }
    }

    renderSearchResults(products) {
        const searchResults = document.getElementById('searchResults');
        if (!searchResults) return;

        if (products.length === 0) {
            searchResults.innerHTML = '<div class="p-4 text-center text-gray-500">محصولی یافت نشد</div>';
            searchResults.classList.remove('hidden');
            return;
        }

        const html = products.map(product => {
            const imageUrl = (product.productImages && product.productImages.length > 0) 
                ? product.productImages[0].imageUrl 
                : 'assets/images/product/mobile-1.png';
            const price = product.price || 0;
            return `
                <a href="product.html?id=${product.id}" class="flex items-center p-3 hover:bg-gray-100 dark:hover:bg-gray-600 border-b border-gray-200 dark:border-gray-600">
                    <img src="${imageUrl}" alt="${product.name || 'محصول'}" class="w-16 h-16 object-contain rounded me-3">
                    <div class="flex-1">
                        <h4 class="font-semibold text-sm dark:text-white">${product.name || 'محصول'}</h4>
                        <p class="text-primary font-bold text-sm">${this.formatPrice(price)} تومان</p>
                    </div>
                </a>
            `;
        }).join('');

        searchResults.innerHTML = html;
        searchResults.classList.remove('hidden');
    }

    async updateCartCount() {
        try {
            if (!window.cartService || !window.authService || !window.authService.isAuthenticated()) {
                return;
            }

            const result = await window.cartService.getUserCart();
            if (result.success && result.data) {
                const cartItems = result.data.items || result.data || [];
                const cartCount = document.getElementById('cartCount');
                if (cartCount) {
                    const totalItems = cartItems.reduce((sum, item) => sum + (item.quantity || 1), 0);
                    cartCount.textContent = totalItems;
                }
            }
        } catch (error) {
            window.logger.error('Error updating cart count:', error);
        }
    }

    async updateComparisonCount() {
        try {
            if (!window.comparisonService) return;

            const count = await window.comparisonService.getComparisonCount();
            const comparisonCountEl = document.getElementById('comparisonCount');
            if (comparisonCountEl) comparisonCountEl.textContent = count || 0;
        } catch (error) {
            window.logger.error('Error updating comparison count:', error);
        }
    }

    async updateUserMenu() {
        try {
            if (!window.authService) return;

            const isAuthenticated = window.authService.isAuthenticated();
            const userMenu = document.getElementById('userMenu');
            const loginButton = document.getElementById('loginButton');

            if (isAuthenticated) {
                const user = await window.authService.getCurrentUser();
                if (user && userMenu) {
                    const userNameEl = userMenu.querySelector('[data-user-name]');
                    if (userNameEl) userNameEl.textContent = user.userName || user.email || 'کاربر';
                    userMenu.style.display = 'block';
                }
                if (loginButton) loginButton.style.display = 'none';
            } else {
                if (userMenu) userMenu.style.display = 'none';
                if (loginButton) loginButton.style.display = 'block';
            }
        } catch (error) {
            window.logger.error('Error updating user menu:', error);
            if (userMenu) userMenu.style.display = 'none';
            const loginButton = document.getElementById('loginButton');
            if (loginButton) loginButton.style.display = 'block';
        }
    }

    formatPrice(price) {
        return new Intl.NumberFormat('fa-IR').format(Math.round(price));
    }

    /**
     * Load and render category menu dynamically
     */
    async loadCategoryMenu() {
        try {
            if (!window.categoryService) {
                window.logger.warn('CategoryService not available');
                return;
            }

            const result = await window.categoryService.getCategoryTree();
            if (result.success && result.data) {
                this.renderCategoryMenu(result.data);
            } else {
                // Fallback: try to get all categories if tree is not available
                const allCategoriesResult = await window.categoryService.getAllCategories();
                if (allCategoriesResult.success && allCategoriesResult.data) {
                    this.renderCategoryMenuSimple(allCategoriesResult.data);
                }
            }
        } catch (error) {
            window.logger.error('Error loading category menu:', error);
        }
    }

    /**
     * Render category menu with tree structure
     */
    renderCategoryMenu(categories) {
        // Main navigation menu
        const mainNavContainer = document.querySelector('[data-category-menu-main]');
        if (mainNavContainer && Array.isArray(categories) && categories.length > 0) {
            const limitedCategories = categories.slice(0, 8);
            const html = limitedCategories.map(category => this.renderCategoryMenuItem(category, true)).join('');
            mainNavContainer.innerHTML = html;
        }

        // Offcanvas mobile menu
        const offcanvasMenuContainer = document.querySelector('[data-category-menu-offcanvas]');
        if (offcanvasMenuContainer && Array.isArray(categories) && categories.length > 0) {
            const html = categories.map(category => this.renderOffcanvasCategoryItem(category)).join('');
            offcanvasMenuContainer.innerHTML = html;
        }

        // Fallback: populate mega menu list if present
        const megaList = document.querySelector('#mega-menu-fire-target ul') || document.querySelector('#mega-menu-fire-target .col-span-2 ul');
        if (megaList && Array.isArray(categories) && categories.length > 0) {
            const items = categories.slice(0, 20).map((cat, idx) => `
                <li data-mega-id="${idx + 1}"
                    class="px-4 w-full hover:bg-opacity-70 border-opacity-0 hover:border-opacity-100 rounded-lg dark:hover:text-zinc-950 mega-menu-li">
                    <a href="shop.html?category=${cat.id}" class="flex items-center justify-between py-3">
                        <div class="flex items-center">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                <path fill-rule="evenodd" d="M3 5a2 2 0 012-2h10a2 2 0 012 2v10a2 2 0 01-2 2H5a2 2 0 01-2-2V5zm11 1H6v8l4-2 4 2V6z" clip-rule="evenodd"/>
                            </svg>
                            <div class="ms-1">
                                <p class="text-xs">${cat.name || 'دسته‌بندی'}</p>
                            </div>
                        </div>
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
                        </svg>
                    </a>
                </li>
            `).join('');
            megaList.innerHTML = items;
        }
    }

    /**
     * Render simple category menu (flat list)
     */
    renderCategoryMenuSimple(categories) {
        if (!Array.isArray(categories) || categories.length === 0) return;

        const mainNavContainer = document.querySelector('[data-category-menu-main]');
        if (mainNavContainer) {
            const limitedCategories = categories.slice(0, 8);
            const html = limitedCategories.map(category => `
                <li class="relative group">
                    <a href="shop.html?category=${category.id}" 
                       class="flex items-center space-x-3 hover:text-primary transition">
                        <span>${category.name || 'دسته‌بندی'}</span>
                    </a>
                </li>
            `).join('');
            mainNavContainer.innerHTML = html;
        }

        // Fallback: mega menu simple list
        const megaList = document.querySelector('#mega-menu-fire-target ul') || document.querySelector('#mega-menu-fire-target .col-span-2 ul');
        if (megaList) {
            const items = categories.slice(0, 20).map((cat, idx) => `
                <li data-mega-id="${idx + 1}"
                    class="px-4 w-full hover:bg-opacity-70 border-opacity-0 hover:border-opacity-100 rounded-lg dark:hover:text-zinc-950 mega-menu-li">
                    <a href="shop.html?category=${cat.id}" class="flex items-center justify-between py-3">
                        <div class="flex items-center">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                <path fill-rule="evenodd" d="M3 5a2 2 0 012-2h10a2 2 0 012 2v10a2 2 0 01-2 2H5a2 2 0 01-2-2V5zm11 1H6v8l4-2 4 2V6z" clip-rule="evenodd"/>
                            </svg>
                            <div class="ms-1">
                                <p class="text-xs">${cat.name || 'دسته‌بندی'}</p>
                            </div>
                        </div>
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
                        </svg>
                    </a>
                </li>
            `).join('');
            megaList.innerHTML = items;
        }
    }

    /**
     * Render a category menu item with subcategories
     */
    renderCategoryMenuItem(category, isMainNav = false) {
        const hasSubcategories = category.subCategories && category.subCategories.length > 0;
        const categoryUrl = `shop.html?category=${category.id}`;

        if (hasSubcategories) {
            // Category with submenu
            return `
                <li class="relative group">
                    <a href="${categoryUrl}" 
                       class="flex items-center space-x-3 hover:text-primary transition">
                        <span>${category.name || 'دسته‌بندی'}</span>
                        ${hasSubcategories ? '<svg class="w-4 h-4 transform group-hover:rotate-180 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/></svg>' : ''}
                    </a>
                    ${hasSubcategories ? `
                        <ul class="absolute right-0 top-full mt-2 w-64 bg-white dark:bg-gray-800 shadow-lg rounded-lg p-4 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all z-50 border border-gray-200">
                            ${category.subCategories.slice(0, 10).map(sub => `
                                <li class="mb-2">
                                    <a href="shop.html?category=${sub.id}" 
                                       class="block px-3 py-2 rounded hover:bg-gray-100 dark:hover:bg-gray-700 transition">
                                        ${sub.name || 'زیردسته'}
                                    </a>
                                </li>
                            `).join('')}
                        </ul>
                    ` : ''}
                </li>
            `;
        } else {
            // Simple category link
            return `
                <li>
                    <a href="${categoryUrl}" 
                       class="flex items-center space-x-3 hover:text-primary transition">
                        <span>${category.name || 'دسته‌بندی'}</span>
                    </a>
                </li>
            `;
        }
    }

    /**
     * Render category item for offcanvas menu (mobile)
     */
    renderOffcanvasCategoryItem(category) {
        const hasSubcategories = category.subCategories && category.subCategories.length > 0;
        const categoryUrl = `shop.html?category=${category.id}`;
        const menuId = `menu-${category.id}`;

        if (hasSubcategories) {
            return `
                <li class="bg-ul-f7 border border-gray-100 dark:bg-zinc-800 dark:text-white p-2">
                    <button class="flex justify-between w-full text-start" 
                            aria-expanded="false"
                            aria-controls="${menuId}"
                            onclick="toggleDropdown('${menuId}')">
                        <a href="${categoryUrl}" class="flex-1">${category.name || 'دسته‌بندی'}</a>
                        <svg xmlns="http://www.w3.org/2000/svg"
                             class="h-5 w-5 transition-transform transform"
                             id="icon-${menuId}"
                             viewBox="0 0 20 20"
                             fill="currentColor"
                             aria-hidden="true">
                            <path fill-rule="evenodd"
                                  d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z"
                                  clip-rule="evenodd"/>
                        </svg>
                    </button>
                    <ul id="${menuId}" class="hidden space-y-2 mt-2 pr-4">
                        ${category.subCategories.map(sub => `
                            <li class="px-6 py-2 border-b border-gray-200">
                                <a href="shop.html?category=${sub.id}" class="block">${sub.name || 'زیردسته'}</a>
                            </li>
                        `).join('')}
                    </ul>
                </li>
            `;
        } else {
            return `
                <li class="bg-ul-f7 border border-gray-100 dark:bg-zinc-800 dark:text-white p-2">
                    <a href="${categoryUrl}" class="block">${category.name || 'دسته‌بندی'}</a>
                </li>
            `;
        }
    }
}

// Toggle dropdown function for offcanvas menu (must be global)
window.toggleDropdown = function(menuId) {
    const menu = document.getElementById(menuId);
    const icon = document.getElementById(`icon-${menuId}`);
    if (menu && icon) {
        const isHidden = menu.classList.contains('hidden');
        if (isHidden) {
            menu.classList.remove('hidden');
            icon.classList.add('rotate-180');
        } else {
            menu.classList.add('hidden');
            icon.classList.remove('rotate-180');
        }
    }
};

// Initialize header component when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    if (typeof window.HeaderComponent === 'undefined') {
        window.headerComponent = new HeaderComponent();
    }
});


