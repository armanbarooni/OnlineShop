
import { ApiClient } from '../api-client.js';
import CategoryService from './category-service.js';
import ProductService from './product-service.js';
import CartService from './cart-service.js';
import SearchService from './search-service.js';

class HomePage {
    constructor() {
        this.apiClient = new ApiClient('http://localhost:5000/api');
        this.categoryService = new CategoryService(this.apiClient);
        this.productService = new ProductService(this.apiClient);
        this.cartService = new CartService(this.apiClient);
        this.searchService = new SearchService(this.apiClient);

        this.init();
    }

    async init() {
        console.log('Initializing Home Page...');

        // 1. Initialize Cart (Update count)
        this.cartService.updateCartCount();

        // 2. Initialize Search
        // Assuming search input has id="searchInput" and results id="searchResults"
        // If not, we might need to add them or use existing classes
        const searchInput = document.querySelector('input[type="search"]') || document.getElementById('searchInput');
        const searchResults = document.getElementById('searchResults');

        if (searchInput && searchResults) {
            this.searchService.init(searchInput, searchResults);
        }

        // 3. Render Mega Menu
        // We need to target the container. We'll use the existing one or clear it.
        const megaMenuTarget = document.getElementById('mega-menu-fire-target');
        if (megaMenuTarget) {
            // Clear existing static content if we haven't already
            // megaMenuTarget.innerHTML = ''; 

            // We need a specific structure for renderMegaMenu.
            // Our service expects to inject UL into a container.
            // Let's create a clean container structure inside it.
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

        // 4. Render Featured Products (Amazing Section)
        const amazingSection = document.querySelector('.amazing-carousel .swiper-wrapper');
        if (amazingSection) {
            // Replace static content
            await this.productService.renderProductSection(amazingSection, 'featured', 8);
        }

        // 5. Render New Arrivals
        const newArrivalsSection = document.querySelector('.product-carousel .swiper-wrapper');
        if (newArrivalsSection) {
            await this.productService.renderProductSection(newArrivalsSection, 'latest', 8);
        }

        // 6. Fix Links
        // We can do a pass to update any static links if needed
        // But the services should generate correct links.

        console.log('Home Page Initialized');
    }
}

export function startHomePage() {
    new HomePage();
}
