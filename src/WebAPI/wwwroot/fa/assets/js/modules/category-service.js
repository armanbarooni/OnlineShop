// Category Service Module
class CategoryService {
    constructor(apiClient) {
        this.apiClient = apiClient;
        this.categories = null;
        this.cacheTime = null;
        this.CACHE_DURATION = 5 * 60 * 1000; // 5 minutes
    }

    /**
     * Get all categories from API or cache
     * @returns {Promise<Array>} Array of categories
     */
    async getCategories(forceRefresh = false) {
        // Return cached data if available and not expired
        if (!forceRefresh && this.categories && this.cacheTime &&
            (Date.now() - this.cacheTime < this.CACHE_DURATION)) {
            return this.categories;
        }

        try {
            const response = await this.apiClient.get('/productcategory');
            this.categories = response.data || [];
            this.cacheTime = Date.now();
            return this.categories;
        } catch (error) {
            console.error('Failed to fetch categories:', error);
            // Return cached data if available, even if expired
            if (this.categories) {
                return this.categories;
            }
            return [];
        }
    }

    /**
     * Get single category by ID
     * @param {number} id - Category ID
     * @returns {Promise<Object|null>} Category object or null
     */
    async getCategoryById(id) {
        try {
            const response = await this.apiClient.get(`/productcategory/${id}`);
            return response.data;
        } catch (error) {
            console.error(`Failed to fetch category ${id}:`, error);
            // Try to find in cached categories
            if (this.categories) {
                return this.categories.find(cat => cat.id === id) || null;
            }
            return null;
        }
    }

    /**
     * Render categories in mega menu
     * @param {string} containerId - ID of container element
     */
    async renderMegaMenu(containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;

        const categories = await this.getCategories();
        if (!categories || categories.length === 0) {
            container.innerHTML = '<p class="text-gray-500 p-4">دسته‌بندی یافت نشد</p>';
            return;
        }

        // Render category list
        let html = '<ul class="my-2 space-y-1">';
        categories.forEach((category, index) => {
            html += `
                <li data-mega-id="${category.id}" 
                    class="px-4 w-full hover:bg-opacity-70 border-opacity-0 hover:border-opacity-100 rounded-lg dark:hover:text-zinc-950 mega-menu-li">
                    <a href="shop.html?category=${category.id}" class="flex items-center justify-between py-3">
                        <div class="flex items-center">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                <path fill-rule="evenodd" d="M3 5a2 2 0 012-2h10a2 2 0 012 2v10a2 2 0 01-2 2H5a2 2 0 01-2-2V5zm11 1H6v8l4-2 4 2V6z" clip-rule="evenodd"/>
                            </svg>
                            <div class="ms-1">
                                <p class="text-xs">${category.name || 'بدون نام'}</p>
                            </div>
                        </div>
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
                        </svg>
                    </a>
                </li>
            `;
        });
        html += '</ul>';
        container.innerHTML = html;
    }

    /**
     * Render category cards
     * @param {string} containerId - ID of container element
     * @param {number} limit - Maximum number of categories to show
     */
    async renderCategoryCards(containerId, limit = null) {
        const container = document.getElementById(containerId);
        if (!container) return;

        const categories = await this.getCategories();
        if (!categories || categories.length === 0) {
            container.innerHTML = '<p class="text-gray-500 p-4">دسته‌بندی یافت نشد</p>';
            return;
        }

        const displayCategories = limit ? categories.slice(0, limit) : categories;

        let html = '';
        displayCategories.forEach(category => {
            html += `
                <div class="category-card">
                    <a href="shop.html?category=${category.id}" class="block p-4 bg-white dark:bg-gray-800 rounded-lg shadow hover:shadow-lg transition">
                        <h3 class="text-lg font-bold mb-2">${category.name || 'بدون نام'}</h3>
                        ${category.description ? `<p class="text-sm text-gray-600 dark:text-gray-400">${category.description}</p>` : ''}
                    </a>
                </div>
            `;
        });

        container.innerHTML = html;
    }

    /**
     * Get category breadcrumb
     * @param {number} categoryId - Category ID
     * @returns {Promise<string>} Breadcrumb HTML
     */
    async getCategoryBreadcrumb(categoryId) {
        const category = await this.getCategoryById(categoryId);
        if (!category) return '';

        return `<a href="index.html">خانه</a> / <a href="shop.html?category=${category.id}">${category.name}</a>`;
    }
}

// Export for use in other modules
if (typeof window !== 'undefined') {
    window.CategoryService = CategoryService;
}

// Create global instance
if (typeof window !== 'undefined' && window.apiClient) {
    window.categoryService = new CategoryService(window.apiClient);
}
