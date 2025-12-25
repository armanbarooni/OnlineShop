
import { ApiClient } from '../api-client.js';
import CartService from './cart-service.js';
import CategoryService from './category-service.js';
import SearchService from './search-service.js';

class CartPage {
    constructor() {
        this.apiClient = new ApiClient('http://localhost:5000/api');
        this.cartService = new CartService(this.apiClient);
        this.categoryService = new CategoryService(this.apiClient);
        this.searchService = new SearchService(this.apiClient);

        this.init();
    }

    async init() {
        // Init header services
        this.cartService.updateCartCount();
        const searchInput = document.getElementById('searchInput');
        const searchResults = document.getElementById('searchResults');
        if (searchInput && searchResults) {
            this.searchService.init(searchInput, searchResults);
        }

        // Render Cart
        this.renderCart();

        // Listen for cart updates (if any event is emitted, or just rely on render)
        window.addEventListener('cart-updated', () => this.renderCart());
    }

    renderCart() {
        const container = document.getElementById('cart-items-container');
        const items = this.cartService.getCart();

        if (!container) return;

        if (items.length === 0) {
            container.innerHTML = '<div class="text-center p-10"><h2 class="text-xl">سبد خرید شما خالی است</h2></div>';
            this.updateSummary(0, 0);
            return;
        }

        container.innerHTML = items.map(item => this.renderCartItem(item)).join('');

        // Attach event listeners for increment/decrement/remove
        items.forEach(item => {
            const incBtn = document.getElementById(`inc-${item.productId}`);
            const decBtn = document.getElementById(`dec-${item.productId}`);
            const removeBtn = document.getElementById(`remove-${item.productId}`);

            if (incBtn) incBtn.onclick = () => {
                this.cartService.updateQuantity(item.productId, item.quantity + 1);
                this.renderCart();
            };

            if (decBtn) decBtn.onclick = () => {
                this.cartService.updateQuantity(item.productId, item.quantity - 1);
                this.renderCart();
            };

            if (removeBtn) removeBtn.onclick = (e) => {
                e.preventDefault();
                this.cartService.removeFromCart(item.productId);
                this.renderCart();
            };
        });

        const total = this.cartService.getTotal();
        // Assuming no discount logic in CartService yet, or we can add it.
        // For now Subtotal = Total.
        this.updateSummary(total, 0);
    }

    renderCartItem(item) {
        return `
            <li>
                <div class="grid grid-cols-4 gap-4 dark:bg-gray-800 dark:text-white bg-white rounded-lg drop-shadow-lg border-gray-300 border-1 p-4">
                    <div class="lg:col-span-3 col-span-4 w-full">
                        <div class="flex flex-wrap">
                            <figure>
                                <img class="size-40 object-cover rounded-lg" src="${item.image || 'assets/images/product/product-1.jpeg'}" alt="${item.name}">
                            </figure>
                            <div class="space-y-5 ms-4 flex-1">
                                <div class="flex sm:space-y-0 space-y-2 item-center flex-wrap">
                                    <h3 class="sm:w-auto w-full font-bold">${item.name}</h3>
                                </div>
                                <div class="flex items-center">
                                    <div class="inline-flex items-center space-x-2 border rounded-full px-4 py-2 dark:bg-zinc-800 bg-white shadow">
                                        <button id="inc-${item.productId}" class="bg-gray-500 text-white w-8 h-8 rounded-full flex items-center justify-center text-lg hover:bg-gray-600 transition">+</button>
                                        <span class="text-lg px-5 inline-block">${item.quantity}</span>
                                        <button id="dec-${item.productId}" class="bg-gray-200 text-gray-600 w-8 h-8 rounded-full flex items-center justify-center text-lg hover:bg-gray-300 transition">-</button>
                                    </div>
                                    <a href="#" id="remove-${item.productId}" class="block p-2 bg-red-500 rounded-full text-white ms-2 mt-1 hover:bg-red-600 transition">
                                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-6">
                                            <path stroke-linecap="round" stroke-linejoin="round" d="m14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0" />
                                        </svg>
                                    </a>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="lg:col-span-1 col-span-4 w-full">
                        <div class="space-y-5 flex flex-col xl:items-end xl:justify-end h-full">
                            <span class="text-xl block font-bold dark:text-white">${(item.price * item.quantity).toLocaleString()} <span class="text-xs">تومان</span></span>
                        </div>
                    </div>
                </div>
            </li>
        `;
    }

    updateSummary(total, discount) {
        const subtotalEl = document.getElementById('cart-subtotal');
        const discountEl = document.getElementById('cart-discount');
        const totalEl = document.getElementById('cart-total');

        if (subtotalEl) subtotalEl.textContent = `${(total + discount).toLocaleString()} تومان`;
        if (discountEl) discountEl.textContent = `${discount.toLocaleString()} تومان`;
        if (totalEl) totalEl.textContent = `${total.toLocaleString()} تومان`;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    new CartPage();
});
