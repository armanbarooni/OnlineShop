
import { ApiClient } from '../api-client.js';
import CategoryService from './category-service.js';
import ProductService from './product-service.js';
import CartService from './cart-service.js';
import SearchService from './search-service.js';
import WishlistService from './wishlist-service.js';

class ShopPage {
    constructor() {
        this.apiClient = new ApiClient('http://localhost:5000/api');
        this.categoryService = new CategoryService(this.apiClient);
        this.productService = new ProductService(this.apiClient);
        this.cartService = new CartService(this.apiClient);
        this.searchService = new SearchService(this.apiClient);
        this.wishlistService = new WishlistService(this.apiClient);

        this.init();
    }

    async init() {
        console.log('Initializing Shop Page...');

        // 1. Initialize Cart
        this.cartService.updateCartCount();

        // 2. Initialize Search
        const searchInput = document.querySelector('input[type="search"]') || document.getElementById('searchInput');
        const searchResults = document.getElementById('searchResults');
        if (searchInput && searchResults) {
            this.searchService.init(searchInput, searchResults);
        }

        // 3. Render Mega Menu
        const megaMenuTarget = document.getElementById('mega-menu-fire-target');
        if (megaMenuTarget) {
            megaMenuTarget.innerHTML = `
                <div class="grid grid-cols-12">
                     <div id="mega-menu-list-container" class="col-span-2 h-[400px] overflow-y-scroll border-e border-gray-400">
                     </div>
                     <div class="col-span-10 p-5">
                        <p class="text-gray-500">برای مشاهده محصولات کلیک کنید</p>
                     </div>
                </div>
            `;
            await this.categoryService.renderMegaMenu('mega-menu-list-container');
        }

        // 4. Parse URL Parameters
        const urlParams = new URLSearchParams(window.location.search);
        const categoryId = urlParams.get('category');
        const searchQuery = urlParams.get('q');

        // 5. Render Products
        const gridContainer = document.getElementById('shop-products-grid');
        if (gridContainer) {
            // Clear static content
            gridContainer.innerHTML = '<div class="col-span-full text-center p-10"><div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div></div>';

            try {
                let products = [];
                if (categoryId) {
                    console.log(`Fetching products for category ${categoryId}`);
                    products = await this.productService.getProductsByCategory(parseInt(categoryId));
                } else if (searchQuery) {
                    console.log(`Searching products for ${searchQuery}`);
                    products = await this.productService.searchProducts(searchQuery);
                } else {
                    console.log('Fetching all products');
                    products = await this.productService.getProducts();
                }

                this.renderGrid(gridContainer, products);
            } catch (error) {
                console.error('Error rendering shop grid:', error);
                gridContainer.innerHTML = '<p class="col-span-full text-red-500 text-center">خطا در دریافت محصولات</p>';
            }
        }
    }

    renderGrid(container, products) {
        if (!products || products.length === 0) {
            container.innerHTML = '<p class="col-span-full text-gray-500 text-center py-10">محصولی یافت نشد</p>';
            return;
        }

        // Use ProductService render logic but adapted for grid layout
        // The container is grid-cols-12. Items should be col-span.

        const html = products.map(product => {
            // Re-use renderProductCard but wrap it in the grid column div
            const cardHtml = this.productService.renderProductCard(product);
            return `
                <div class="lg:col-span-4 md:col-span-6 col-span-12 w-full">
                    ${cardHtml}
                </div>
            `;
        }).join('');

        container.innerHTML = html;
    }
}

// Start
document.addEventListener('DOMContentLoaded', () => {
    new ShopPage();
});
