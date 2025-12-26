/**
 * Shop Page (shop.html) API Integration
 */

// Initialize shop page
document.addEventListener('DOMContentLoaded', async () => {
    // Wait for all services to load
    if (typeof window.apiClient === 'undefined' || 
        typeof window.productService === 'undefined' ||
        typeof window.categoryService === 'undefined') {
        if (window.logger) {
            window.logger.error('Required services not loaded');
        } else {
            console.error('Required services not loaded');
        }
        return;
    }

    try {
        // Parse URL Parameters
        const urlParams = new URLSearchParams(window.location.search);
        const categoryId = urlParams.get('category');
        const searchQuery = urlParams.get('search') || urlParams.get('q');

        // Load products
        await loadProducts(categoryId, searchQuery);
    } catch (error) {
        if (window.logger) {
            window.logger.error('Error initializing shop page:', error);
        } else {
            console.error('Error initializing shop page:', error);
        }
    }
});

// Load products based on category or search
async function loadProducts(categoryId, searchQuery) {
    const gridContainer = document.getElementById('shop-products-grid');
    if (!gridContainer) return;

    // Show loading
    gridContainer.innerHTML = '<div class="col-span-full text-center p-10"><div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div><p class="mt-4 text-gray-500">در حال بارگذاری محصولات...</p></div>';

    try {
        let result;
        
        if (categoryId) {
            // Load products by category (categoryId is Guid string)
            result = await window.productService.getProductsByCategory(categoryId);
        } else if (searchQuery) {
            // Search products
            result = await window.productService.searchProducts({
                searchTerm: searchQuery,
                pageNumber: 1,
                pageSize: 20
            });
        } else {
            // Load all products
            result = await window.productService.getAllProducts();
        }

        if (result.success !== undefined) {
            // Result has success property
            if (result.success && result.data) {
                const products = result.data.products || result.data.items || (Array.isArray(result.data) ? result.data : []);
                renderProducts(products, gridContainer);
            } else {
                showError(result.error || 'خطا در دریافت محصولات', gridContainer);
            }
        } else if (Array.isArray(result)) {
            // Result is directly an array
            renderProducts(result, gridContainer);
        } else if (result.data) {
            // Result has data property
            const products = result.data.products || result.data.items || (Array.isArray(result.data) ? result.data : []);
            renderProducts(products, gridContainer);
        } else {
            showError('فرمت پاسخ نامعتبر است', gridContainer);
        }
    } catch (error) {
        if (window.logger) {
            window.logger.error('Error loading products:', error);
        } else {
            console.error('Error loading products:', error);
        }
        showError('خطا در اتصال به سرور', gridContainer);
    }
}

// Render products in grid
function renderProducts(products, container) {
    if (!products || products.length === 0) {
        container.innerHTML = '<div class="col-span-full text-center py-10"><p class="text-gray-500">محصولی یافت نشد</p></div>';
        return;
    }

    const html = products.map(product => createProductCard(product)).join('');
    container.innerHTML = html;
}

// Create product card HTML
function createProductCard(product) {
    const imageUrl = (product.productImages && product.productImages.length > 0) 
        ? product.productImages[0].imageUrl 
        : (product.imageUrl || 'assets/images/product/mobile-1.png');
    const price = product.price || product.unitPrice || 0;
    const salePrice = product.salePrice || null;
    const finalPrice = salePrice || price;
    const discount = salePrice && price > salePrice ? Math.round(((price - salePrice) / price) * 100) : 0;
    const productUrl = `product.html?id=${product.id}`;
    const name = product.name || 'نام محصول';

    return `
        <div class="lg:col-span-4 md:col-span-6 col-span-12 w-full">
            <article class="bg-white product-box-item drop-shadow-md rounded-xl p-4 dark:bg-gray-800 dark:border-white dark:border-1 h-full flex flex-col">
                <header class="flex items-center relative justify-between mb-3">
                    ${discount > 0 ? `<span class="absolute top-1 end-1 bg-red-500 text-white text-xs px-2 py-1 rounded z-10">${discount}%</span>` : ''}
                    <div class="flex flex-col absolute top-1 start-0 p-1 rounded space-y-3 z-10">
                        <button onclick="addToWishlist('${product.id}')" class="p-2 bg-white rounded-full shadow-md hover:bg-primary hover:text-white transition">
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-5">
                                <path stroke-linecap="round" stroke-linejoin="round" d="M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.22 9 12 9 12s9-4.78 9-12z"/>
                            </svg>
                        </button>
                    </div>
                </header>
                <a href="${productUrl}" class="block flex-1">
                    <figure class="relative overflow-hidden rounded-lg mb-3">
                        <img src="${imageUrl}" alt="${name}" class="w-full h-48 object-contain" onerror="this.src='assets/images/product/mobile-1.png'">
                    </figure>
                    <h3 class="text-sm font-bold mb-2 line-clamp-2 dark:text-white">${name}</h3>
                </a>
                <div class="flex items-center justify-between mt-auto">
                    <div class="flex flex-col">
                        ${discount > 0 ? `<span class="text-xs text-gray-400 line-through">${formatPrice(price)}</span>` : ''}
                        <span class="text-lg font-bold text-primary">${formatPrice(finalPrice)} تومان</span>
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
        } else {
            if (window.utils) {
                window.utils.showToast(result.error || 'خطا در افزودن به سبد خرید', 'error');
            }
        }
    } catch (error) {
        if (window.logger) {
            window.logger.error('Error adding to cart:', error);
        } else {
            console.error('Error adding to cart:', error);
        }
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
            const result = await window.wishlistService.toggleWishlist(productId);
            if (result.success) {
                if (window.utils) {
                    window.utils.showToast('به علاقه‌مندی‌ها اضافه شد', 'success');
                }
            }
        }
    } catch (error) {
        if (window.logger) {
            window.logger.error('Error adding to wishlist:', error);
        } else {
            console.error('Error adding to wishlist:', error);
        }
    }
};

// Show error message
function showError(message, container) {
    container.innerHTML = `<div class="col-span-full text-center py-10"><p class="text-red-500">${message}</p></div>`;
}

