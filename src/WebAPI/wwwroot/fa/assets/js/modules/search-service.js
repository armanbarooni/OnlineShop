// Search Service Module
class SearchService {
    constructor(apiClient, productService) {
        this.apiClient = apiClient;
        this.productService = productService;
        this.recentSearches = this.loadRecentSearches();
        this.searchTimeout = null;
    }

    /**
     * Load recent searches from localStorage
     * @returns {Array} Recent search terms
     */
    loadRecentSearches() {
        try {
            const stored = localStorage.getItem('recentSearches');
            return stored ? JSON.parse(stored) : [];
        } catch (error) {
            return [];
        }
    }

    /**
     * Save recent searches
     */
    saveRecentSearches() {
        try {
            localStorage.setItem('recentSearches', JSON.stringify(this.recentSearches));
        } catch (error) {
            console.error('Failed to save recent searches:', error);
        }
    }

    /**
     * Add to recent searches
     * @param {string} query - Search query
     */
    addToRecentSearches(query) {
        if (!query || query.trim() === '') return;

        // Remove if already exists
        this.recentSearches = this.recentSearches.filter(q => q !== query);

        // Add to beginning
        this.recentSearches.unshift(query);

        // Keep only last 10
        this.recentSearches = this.recentSearches.slice(0, 10);

        this.saveRecentSearches();
    }

    /**
     * Search products with debounce
     * @param {string} query - Search query
     * @param {Function} callback - Callback with results
     * @param {number} delay - Debounce delay in ms
     */
    searchWithDebounce(query, callback, delay = 300) {
        clearTimeout(this.searchTimeout);

        if (!query || query.trim() === '') {
            callback([]);
            return;
        }

        this.searchTimeout = setTimeout(async () => {
            const results = await this.productService.searchProducts(query);
            callback(results);
        }, delay);
    }

    /**
     * Initialize search input
     * @param {string} inputId - Search input element ID
     * @param {string} resultsId - Results container ID
     */
    initializeSearchInput(inputId, resultsId) {
        const input = document.getElementById(inputId);
        const results = document.getElementById(resultsId);

        if (!input || !results) return;

        // Handle input
        input.addEventListener('input', (e) => {
            const query = e.target.value;

            this.searchWithDebounce(query, (products) => {
                this.renderSearchResults(results, products, query);
            });
        });

        // Handle enter key
        input.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                const query = input.value;
                if (query && query.trim() !== '') {
                    this.addToRecentSearches(query);
                    window.location.href = `shop.html?search=${encodeURIComponent(query)}`;
                }
            }
        });

        // Handle focus - show recent searches
        input.addEventListener('focus', () => {
            if (input.value.trim() === '' && this.recentSearches.length > 0) {
                this.renderRecentSearches(results);
            }
        });

        // Handle click outside
        document.addEventListener('click', (e) => {
            if (!input.contains(e.target) && !results.contains(e.target)) {
                results.classList.add('hidden');
            }
        });
    }

    /**
     * Render search results
     * @param {HTMLElement} container - Results container
     * @param {Array} products - Search results
     * @param {string} query - Search query
     */
    renderSearchResults(container, products, query) {
        if (!products || products.length === 0) {
            if (query && query.trim() !== '') {
                container.innerHTML = `
                    <div class="p-4 text-center text-gray-500">
                        نتیجه‌ای برای "${query}" یافت نشد
                    </div>
                `;
                container.classList.remove('hidden');
            } else {
                container.classList.add('hidden');
            }
            return;
        }

        let html = '<div class="max-h-96 overflow-y-auto">';

        products.slice(0, 5).forEach(product => {
            const image = this.productService.getProductImage(product);
            const price = this.productService.formatPrice(product.price || product.unitPrice);
            const name = product.name || product.title || 'محصول';

            html += `
                <a href="product.html?id=${product.id}" class="flex items-center gap-3 p-3 hover:bg-gray-100 dark:hover:bg-gray-700 transition">
                    <img src="${image}" alt="${name}" class="w-12 h-12 object-cover rounded" onerror="this.src='${this.productService.getPlaceholderImage(product.id)}'">
                    <div class="flex-1">
                        <p class="font-medium text-sm line-clamp-1">${name}</p>
                        <p class="text-primary text-sm">${price}</p>
                    </div>
                </a>
            `;
        });

        if (products.length > 5) {
            html += `
                <a href="shop.html?search=${encodeURIComponent(query)}" class="block p-3 text-center text-primary hover:bg-gray-100 dark:hover:bg-gray-700 transition">
                    مشاهده همه نتایج (${products.length})
                </a>
            `;
        }

        html += '</div>';

        container.innerHTML = html;
        container.classList.remove('hidden');
    }

    /**
     * Render recent searches
     * @param {HTMLElement} container - Results container
     */
    renderRecentSearches(container) {
        if (this.recentSearches.length === 0) {
            container.classList.add('hidden');
            return;
        }

        let html = '<div class="p-2">';
        html += '<p class="px-2 py-1 text-xs text-gray-500">جستجوهای اخیر</p>';

        this.recentSearches.forEach(query => {
            html += `
                <a href="shop.html?search=${encodeURIComponent(query)}" class="block px-3 py-2 hover:bg-gray-100 dark:hover:bg-gray-700 rounded transition">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-4 inline-block ml-2 text-gray-400">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M12 6v6h4.5m4.5 0a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" />
                    </svg>
                    ${query}
                </a>
            `;
        });

        html += '</div>';

        container.innerHTML = html;
        container.classList.remove('hidden');
    }
}

// Export
if (typeof window !== 'undefined') {
    window.SearchService = SearchService;
}

// Create global instance
if (typeof window !== 'undefined' && window.apiClient && window.productService) {
    window.searchService = new SearchService(window.apiClient, window.productService);

    // Initialize search on page load
    document.addEventListener('DOMContentLoaded', () => {
        window.searchService.initializeSearchInput('searchInput', 'searchResults');
    });
}
