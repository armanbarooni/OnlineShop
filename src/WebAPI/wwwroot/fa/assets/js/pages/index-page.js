/**
 * Home Page (index.html) API Integration
 */

// Initialize home page
document.addEventListener('DOMContentLoaded', async () => {
    // Wait for all services to load
    if (typeof window.apiClient === 'undefined' || 
        typeof window.productService === 'undefined' || 
        typeof window.categoryService === 'undefined') {
        window.logger.error('Required services not loaded');
        return;
    }

    try {
        // Load categories
        await loadCategories();
        
        // Load featured products
        await loadFeaturedProducts();
        
        // Load new products
        await loadNewProducts();
        
        // Load best selling products
        await loadBestSellingProducts();
        
        // Load brands
        await loadBrands();
        
        // Setup search functionality
        setupSearch();
        
        // Update cart and comparison counts
        updateCartAndComparisonCounts();
    } catch (error) {
        window.logger.error('Error initializing home page:', error);
    }
});

// Load categories
async function loadCategories() {
    try {
        const result = await window.categoryService.getAllCategories();
        if (result.success && result.data) {
            renderCategories(result.data);
        }
    } catch (error) {
        window.logger.error('Error loading categories:', error);
    }
}

// Render categories
function renderCategories(categories) {
    const categoryContainer = document.querySelector('[data-categories]');
    if (!categoryContainer) return;

    const limitedCategories = Array.isArray(categories) ? categories.slice(0, 8) : [];
    if (limitedCategories.length === 0) return;

    const html = limitedCategories.map(category => `
        <a href="shop.html?category=${category.id}" class="lg:col-span-3 sm:col-span-6 col-span-12 w-full block">
            <article class="flex py-2 px-3 rounded-xl border border-gray-200 bg-white drop-shadow-md items-center justify-between dark:bg-gray-800">
                <section class="space-y-2">
                    <h3 class="text-lg font-bold dark:text-white">${category.name || 'دسته‌بندی'}</h3>
                    <span class="text-xs font-light text-neutral-500">${category.description || ''}</span>
                </section>
                <figure>
                    <img src="${category.imageUrl || 'assets/images/category/digitall.png'}" 
                         class="size-20" loading="lazy" alt="${category.name || 'دسته‌بندی'}">
                </figure>
            </article>
        </a>
    `).join('');

    categoryContainer.innerHTML = html;
}

// Load featured products
async function loadFeaturedProducts() {
    try {
        const result = await window.productService.getFeaturedProducts(8);
        if (result.success && result.data) {
            const products = result.data.products || result.data;
            if (Array.isArray(products) && products.length > 0) {
                renderProducts(products, 'featuredProducts');
            }
        }
    } catch (error) {
        window.logger.error('Error loading featured products:', error);
    }
}

// Load new products
async function loadNewProducts() {
    try {
        const result = await window.productService.getNewProducts(8);
        if (result.success && result.data) {
            const products = result.data.products || result.data;
            if (Array.isArray(products) && products.length > 0) {
                renderProducts(products, 'newProducts');
            }
        }
    } catch (error) {
        window.logger.error('Error loading new products:', error);
    }
}

// Load best selling products
async function loadBestSellingProducts() {
    try {
        const result = await window.productService.getBestSellingProducts(8);
        if (result.success && result.data) {
            const products = result.data.products || result.data;
            if (Array.isArray(products) && products.length > 0) {
                renderProducts(products, 'bestSellingProducts');
            }
        }
    } catch (error) {
        window.logger.error('Error loading best selling products:', error);
    }
}

// Render products
function renderProducts(products, containerId) {
    let container = document.getElementById(containerId);
    if (!container) {
        // Try to find by data attribute or class
        container = document.querySelector(`[data-products="${containerId}"]`);
        if (!container) {
            // Try to find swiper wrapper for product sections
            const swipers = document.querySelectorAll('.swiper-wrapper');
            if (swipers.length > 0) {
                // Use first swiper wrapper as fallback
                container = swipers[0];
            } else {
                return;
            }
        }
    }

    if (!Array.isArray(products) || products.length === 0) return;

    const html = products.map(product => createProductCard(product)).join('');
    
    // If it's a swiper wrapper, add slides
    if (container.classList.contains('swiper-wrapper')) {
        container.innerHTML = html;
        // Reinitialize swiper if needed
        if (window.swiperInstances && window.swiperInstances[containerId]) {
            window.swiperInstances[containerId].update();
        }
    } else {
        container.innerHTML = html;
    }
}

// Create product card HTML
function createProductCard(product) {
    const imageUrl = (product.productImages && product.productImages.length > 0) 
        ? product.productImages[0].imageUrl 
        : (product.imageUrl || 'assets/images/product/mobile-1.png');
    const price = product.price || 0;
    const originalPrice = product.originalPrice || price;
    const discount = originalPrice > price ? Math.round(((originalPrice - price) / originalPrice) * 100) : 0;
    const productUrl = `product.html?id=${product.id}`;
    const name = product.name || 'نام محصول';

    return `
        <div class="swiper-slide px-1.5 py-2">
            <article class="bg-white product-box-item drop-shadow-md rounded-xl p-4 dark:bg-gray-800 dark:border-white dark:border-1">
                <header class="flex items-center relative justify-between">
                    ${discount > 0 ? `<span class="absolute top-1 end-1 bg-red-500 text-white text-xs px-2 py-1 rounded">${discount}%</span>` : ''}
                    <div class="flex flex-col absolute top-1 start-0 p-1 rounded space-y-3">
                        <button onclick="addToWishlist('${product.id}')" class="p-2 bg-white rounded-full shadow-md hover:bg-primary hover:text-white transition">
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-5">
                                <path stroke-linecap="round" stroke-linejoin="round" d="M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.22 9 12 9 12s9-4.78 9-12z"/>
                            </svg>
                        </button>
                    </div>
                </header>
                <a href="${productUrl}">
                    <figure class="relative overflow-hidden rounded-lg mb-3">
                        <img src="${imageUrl}" alt="${name}" class="w-full h-48 object-contain">
                    </figure>
                </a>
                <a href="${productUrl}">
                    <h3 class="text-sm font-bold mb-2 line-clamp-2 dark:text-white">${name}</h3>
                </a>
                <div class="flex items-center justify-between mt-3">
                    <div class="flex flex-col">
                        ${discount > 0 ? `<span class="text-xs text-gray-400 line-through">${formatPrice(originalPrice)}</span>` : ''}
                        <span class="text-lg font-bold text-primary">${formatPrice(price)} طھظˆظ…ط§ظ†</span>
                    </div>
                    <button onclick="addToCart('${product.id}')" class="bg-primary text-white p-2 rounded-lg hover:bg-primary/90 transition">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-5">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M2.25 3h1.386c.51 0 .955.343 1.087.835l.383 1.437M7.5 14.25a3 3 0 00-3 3h15.75m-12.75-3h11.218c1.121-2.3 2.1-4.684 2.924-7.138a60.114 60.114 0 00-16.536-1.84M7.5 14.25L5.106 5.272M6 20.25a.75.75 0 11-1.5 0 .75.75 0 011.5 0zm12.75 0a.75.75 0 11-1.5 0 .75.75 0 011.5 0z"/>
                        </svg>
                    </button>
                </div>
            </article>
        </div>
    `;
}

// Format price
function formatPrice(price) {
    return new Intl.NumberFormat('fa-IR').format(Math.round(price));
}

// Load brands
async function loadBrands() {
    try {
        const response = await window.apiClient.get('/api/Brand');
        if (response && response.data) {
            const brands = Array.isArray(response.data.data) ? response.data.data : (Array.isArray(response.data) ? response.data : []);
            if (brands.length > 0) {
                renderBrands(brands);
            }
        }
    } catch (error) {
        window.logger.error('Error loading brands:', error);
    }
}

// Render brands
function renderBrands(brands) {
    const brandContainer = document.querySelector('[data-brands]') || document.querySelector('.brand-swiper .swiper-wrapper');
    if (!brandContainer) return;

    const limitedBrands = brands.slice(0, 6);
    const html = limitedBrands.map(brand => `
        <div class="swiper-slide">
            <div class="flex items-center justify-center p-4 bg-white rounded-lg shadow-md dark:bg-gray-800">
                <img src="${brand.logoUrl || 'assets/images/brand/brand1-1.png'}" 
                     alt="${brand.name || 'ط¨ط±ظ†ط¯'}" class="max-h-16 object-contain">
            </div>
        </div>
    `).join('');

    brandContainer.innerHTML = html;
}

// Setup search functionality
function setupSearch() {
    const searchInput = document.getElementById('searchInput');
    const searchButton = searchInput?.nextElementSibling;
    const searchResults = document.getElementById('searchResults');

    if (!searchInput) return;

    let searchTimeout;
    searchInput.addEventListener('input', (e) => {
        clearTimeout(searchTimeout);
        const query = e.target.value.trim();

        if (query.length < 2) {
            if (searchResults) searchResults.classList.add('hidden');
            return;
        }

        searchTimeout = setTimeout(async () => {
            await performSearch(query);
        }, 500);
    });

    if (searchButton) {
        searchButton.addEventListener('click', async () => {
            const query = searchInput.value.trim();
            if (query) {
                window.location.href = `shop.html?search=${encodeURIComponent(query)}`;
            }
        });
    }

    searchInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            const query = searchInput.value.trim();
            if (query) {
                window.location.href = `shop.html?search=${encodeURIComponent(query)}`;
            }
        }
    });
}

// Perform search
async function performSearch(query) {
    try {
        const result = await window.productService.searchProducts({
            searchTerm: query,
            pageSize: 5
        });

        if (result.success && result.data) {
            const products = result.data.products || result.data;
            renderSearchResults(Array.isArray(products) ? products : []);
        }
    } catch (error) {
        window.logger.error('Error searching:', error);
    }
}

// Render search results
function renderSearchResults(products) {
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
        return `
            <a href="product.html?id=${product.id}" class="flex items-center p-3 hover:bg-gray-100 dark:hover:bg-gray-600 border-b border-gray-200 dark:border-gray-600">
                <img src="${imageUrl}" alt="${product.name || 'محصول'}" class="w-16 h-16 object-contain rounded me-3">
                <div class="flex-1">
                    <h4 class="font-semibold text-sm dark:text-white">${product.name || 'محصول'}</h4>
                    <p class="text-primary font-bold text-sm">${formatPrice(product.price || 0)} طھظˆظ…ط§ظ†</p>
                </div>
            </a>
        `;
    }).join('');

    searchResults.innerHTML = html;
    searchResults.classList.remove('hidden');
}

// Add to cart function (global)
window.addToCart = async function(productId) {
    if (!window.authService || !window.authService.isAuthenticated()) {
        window.location.href = 'login.html';
        return;
    }

    try {
        const result = await window.cartService.addToCart(productId, 1);
        if (result.success) {
            if (window.utils) {
                window.utils.showToast('محصول به سبد خرید اضافه شد', 'success');
            }
            updateCartAndComparisonCounts();
        } else {
            if (window.utils) {
                window.utils.showToast(result.error || 'خطا در افزودن به سبد خرید', 'error');
            }
        }
    } catch (error) {
        window.logger.error('Error adding to cart:', error);
        if (window.utils) {
            window.utils.showToast('خطا در اتصال به سرور', 'error');
        }
    }
};

// Add to wishlist function (global)
window.addToWishlist = async function(productId) {
    if (!window.authService || !window.authService.isAuthenticated()) {
        window.location.href = 'login.html';
        return;
    }

    try {
        if (window.wishlistService) {
            const result = await window.wishlistService.addToWishlist(productId);
            if (result.success) {
                if (window.utils) {
                    window.utils.showToast('به علاقه‌مندی‌ها اضافه شد', 'success');
                }
            }
        }
    } catch (error) {
        window.logger.error('Error adding to wishlist:', error);
    }
};

// Update cart and comparison counts
async function updateCartAndComparisonCounts() {
    // Update cart count
    if (window.authService && window.authService.isAuthenticated()) {
        try {
            const cartResult = await window.cartService.getUserCart();
            if (cartResult.success && cartResult.data) {
                const itemCount = cartResult.data.items ? cartResult.data.items.length : 0;
                const cartCountEl = document.getElementById('cartCount');
                if (cartCountEl) cartCountEl.textContent = itemCount;
            }
        } catch (error) {
            window.logger.error('Error updating cart count:', error);
        }
    }

    // Update comparison count
    try {
        if (window.comparisonService) {
            const count = await window.comparisonService.getComparisonCount();
            const comparisonCountEl = document.getElementById('comparisonCount');
            if (comparisonCountEl) comparisonCountEl.textContent = count;
        }
    } catch (error) {
        window.logger.error('Error updating comparison count:', error);
    }
}

// Hide search results when clicking outside
document.addEventListener('click', (e) => {
    const searchResults = document.getElementById('searchResults');
    const searchInput = document.getElementById('searchInput');
    if (searchResults && searchInput && !searchResults.contains(e.target) && !searchInput.contains(e.target)) {
        searchResults.classList.add('hidden');
    }
});


