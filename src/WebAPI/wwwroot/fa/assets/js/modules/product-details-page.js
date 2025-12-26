
import { ApiClient } from '../api-client.js';
import ProductService from './product-service.js';
import CartService from './cart-service.js';
import WishlistService from './wishlist-service.js';

class ProductDetailsPage {
    constructor() {
        this.apiClient = new ApiClient('http://localhost:5000/api');
        this.productService = new ProductService(this.apiClient);
        this.cartService = new CartService(this.apiClient);
        this.wishlistService = new WishlistService(this.apiClient);
        this.currentProduct = null;
        this.quantity = 1;

        this.init();
    }

    async init() {
        // 1. Update Cart Count
        this.cartService.updateCartCount();

        // 2. Parse ID
        const urlParams = new URLSearchParams(window.location.search);
        const productId = urlParams.get('id');

        if (!productId) {
            console.error('No product ID found');
            // document.body.innerHTML = '<div class="text-center p-10"><h1 class="text-2xl">محصول یافت نشد</h1></div>';
            return;
        }

        // 3. Fetch Data
        try {
            console.log(`Fetching product ${productId}`);
            const product = await this.productService.getProductById(productId);
            this.currentProduct = product;
            this.renderProduct(product);
        } catch (error) {
            console.error('Error fetching product:', error);
            // Show error
        }

        // 4. Setup Event Listeners
        this.setupEventListeners();
    }

    renderProduct(product) {
        // Title
        const titleFa = document.getElementById('product-title-fa');
        if (titleFa) titleFa.textContent = product.title;

        const titleEn = document.getElementById('product-title-en');
        if (titleEn) titleEn.textContent = product.englishTitle || '';

        // Breadcrumb (simple update)
        const breadcrumbTitle = document.getElementById('breadcrumb-product-title');
        if (breadcrumbTitle) breadcrumbTitle.textContent = product.title;

        // Price
        this.renderPrice(product);

        // Gallery
        this.renderGallery(product);

        // Features (Description)
        const featuresList = document.getElementById('product-features-list');
        if (featuresList && product.description) {
            // Assuming description is text, let's just make it a list item or split by newline
            featuresList.innerHTML = `
                <li class="flex items-center space-x-3">
                     <span class="inline-block text-base">${product.description}</span>
                </li>
            `;
        }
    }

    renderPrice(product) {
        // IDs: product-current-price, product-old-price, product-discount-badge
        const currentPriceEl = document.getElementById('product-current-price');
        const oldPriceEl = document.getElementById('product-old-price');
        const discountBadge = document.getElementById('product-discount-badge');

        if (currentPriceEl) {
            currentPriceEl.textContent = product.price.toLocaleString();
            currentPriceEl.setAttribute('content', product.price);
        }

        // Logic for discount (if API supports it, currently using placeholder logic)
        if (product.oldPrice && product.oldPrice > product.price) {
            if (oldPriceEl) {
                oldPriceEl.innerHTML = `
                    <meta itemprop="priceCurrency" content="IRR">
                    <span itemprop="price">${product.oldPrice.toLocaleString()}</span>
                `;
                oldPriceEl.classList.remove('hidden');
            }
            if (discountBadge) {
                const percent = Math.round(((product.oldPrice - product.price) / product.oldPrice) * 100);
                discountBadge.textContent = `${percent}%`;
                discountBadge.classList.remove('hidden');
            }
        } else {
            if (oldPriceEl) oldPriceEl.classList.add('hidden');
            if (discountBadge) discountBadge.classList.add('hidden');
        }
    }

    renderGallery(product) {
        // Swiper wrappers: #productGalleryTwo .swiper-wrapper, #productGalleryOne .swiper-wrapper
        // For now, use placeholder images logic from ProductService if no images in product object
        const images = product.images || [
            this.productService.getPlaceholderImage(1),
            this.productService.getPlaceholderImage(2),
            this.productService.getPlaceholderImage(3),
            this.productService.getPlaceholderImage(4)
        ];

        const generateSlides = (imgs) => imgs.map(img => `
            <div class="swiper-slide !pe-1 cursor-pointer">
                <img src="${img}" alt="${product.title}" class="rounded-lg border border-gray-300 p-2 w-full object-cover">
            </div>
        `).join('');

        const slidesHtml = generateSlides(images);

        const gallery2 = document.querySelector('#productGalleryTwo .swiper-wrapper');
        const gallery1 = document.querySelector('#productGalleryOne .swiper-wrapper');

        if (gallery2) gallery2.innerHTML = slidesHtml;
        if (gallery1) gallery1.innerHTML = slidesHtml;

        // Swiper needs re-init or it picks up changes automatically if we didn't destroy it? 
        // We might need to dispatch event or manually re-create swipers.
        // Assuming app.js initializes them on load. If we change content AFTER load, we might need update.
        // But app.js runs on DOMContentLoaded. We are also running on DOMContentLoaded. Race condition?
        // Let's dispatch a custom event if needed, but often swapping innerHTML before swiper init works if we are fast, 
        // or Swiper observes mutations.
    }

    setupEventListeners() {
        // Quantity
        const incrementBtn = document.getElementById('product-increment-btn');
        const decrementBtn = document.getElementById('product-decrement-btn');
        const countDisplay = document.getElementById('product-count-display');

        if (incrementBtn) {
            incrementBtn.onclick = () => {
                this.quantity++;
                if (countDisplay) countDisplay.textContent = this.quantity;
            };
        }
        if (decrementBtn) {
            decrementBtn.onclick = () => {
                if (this.quantity > 1) {
                    this.quantity--;
                    if (countDisplay) countDisplay.textContent = this.quantity;
                }
            };
        }

        // Add to Cart
        const addToCartBtn = document.getElementById('product-add-to-cart-btn');
        if (addToCartBtn) {
            addToCartBtn.onclick = () => {
                if (this.currentProduct) {
                    // Add items quantity times
                    // Basic cart service adds 1. We wrap it loop or update service.
                    // For now, simpler: add 1 'quantity' times is inefficient.
                    // Let's assume CartService handles quantity if we added a method, but standard 'addToCart' adds 1.
                    // We'll just call it 'quantity' times or modify CartService later.
                    // Optimization: Just call it once and handle quantity logic later.
                    // Or: Update CartService to accept quantity.

                    // Proceeding with adding once for now to ensure basic flow, logic can be refined.
                    for (let i = 0; i < this.quantity; i++) {
                        this.cartService.addToCart(this.currentProduct);
                    }
                    alert('محصول به سبد خرید اضافه شد'); // Replace with nicer toast if available
                }
            };
        }
    }
}

document.addEventListener('DOMContentLoaded', () => {
    new ProductDetailsPage();
});
