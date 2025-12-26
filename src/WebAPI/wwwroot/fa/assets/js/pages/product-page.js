/**
 * Product Page (product.html) API Integration
 */

// Initialize product page
document.addEventListener('DOMContentLoaded', async () => {
    // Wait for all services to load
    if (typeof window.apiClient === 'undefined' || 
        typeof window.productService === 'undefined') {
        if (window.logger) {
            window.logger.error('Required services not loaded');
        } else {
            console.error('Required services not loaded');
        }
        return;
    }

    try {
        // Parse product ID from URL
        const urlParams = new URLSearchParams(window.location.search);
        const productId = urlParams.get('id');

        if (!productId) {
            showError('شناسه محصول یافت نشد');
            return;
        }

        // Load product data
        await loadProduct(productId);
    } catch (error) {
        if (window.logger) {
            window.logger.error('Error initializing product page:', error);
        } else {
            console.error('Error initializing product page:', error);
        }
        showError('خطا در بارگذاری صفحه محصول');
    }
});

// Load product by ID
async function loadProduct(productId) {
    try {
        showLoading();
        
        const result = await window.productService.getProductById(productId);
        
        if (result.success && result.data) {
            renderProduct(result.data);
        } else {
            showError(result.error || 'محصول یافت نشد');
        }
    } catch (error) {
        if (window.logger) {
            window.logger.error('Error loading product:', error);
        } else {
            console.error('Error loading product:', error);
        }
        showError('خطا در دریافت اطلاعات محصول');
    } finally {
        hideLoading();
    }
}

// Render product data
function renderProduct(product) {
    // Product title
    const titleFa = document.getElementById('product-title-fa');
    if (titleFa) titleFa.textContent = product.name || 'محصول بدون نام';

    const titleEn = document.getElementById('product-title-en');
    if (titleEn) titleEn.textContent = product.englishTitle || product.name || '';

    // Breadcrumb
    const breadcrumbTitle = document.getElementById('breadcrumb-product-title');
    if (breadcrumbTitle) breadcrumbTitle.textContent = product.name || 'محصول';

    // Price
    renderPrice(product);

    // Images/Gallery
    renderGallery(product);

    // Description
    renderDescription(product);

    // Specifications
    renderSpecifications(product);
}

// Render product price
function renderPrice(product) {
    const currentPriceEl = document.getElementById('product-current-price');
    const oldPriceEl = document.getElementById('product-old-price');
    const discountBadge = document.getElementById('product-discount-badge');

    const price = product.price || 0;
    const salePrice = product.salePrice || null;
    const finalPrice = salePrice || price;

    if (currentPriceEl) {
        currentPriceEl.textContent = formatPrice(finalPrice);
    }

    // Show discount if sale price exists
    if (salePrice && salePrice < price) {
        if (oldPriceEl) {
            oldPriceEl.textContent = formatPrice(price);
            oldPriceEl.classList.remove('hidden');
        }
        if (discountBadge) {
            const discountPercent = Math.round(((price - salePrice) / price) * 100);
            discountBadge.textContent = `${discountPercent}%`;
            discountBadge.classList.remove('hidden');
        }
    } else {
        if (oldPriceEl) oldPriceEl.classList.add('hidden');
        if (discountBadge) discountBadge.classList.add('hidden');
    }
}

// Render product gallery
function renderGallery(product) {
    const images = product.productImages || [];
    
    if (images.length === 0) {
        // Use placeholder if no images
        images.push({
            imageUrl: 'assets/images/product/mobile-1.png'
        });
    }

    const gallery1 = document.querySelector('#productGalleryOne .swiper-wrapper');
    const gallery2 = document.querySelector('#productGalleryTwo .swiper-wrapper');

    const slidesHtml = images.map(img => `
        <div class="swiper-slide !pe-1 cursor-pointer">
            <img src="${img.imageUrl || img}" alt="${product.name || 'محصول'}" 
                 class="rounded-lg border border-gray-300 p-2 w-full object-cover"
                 onerror="this.src='assets/images/product/mobile-1.png'">
        </div>
    `).join('');

    if (gallery1) gallery1.innerHTML = slidesHtml;
    if (gallery2) gallery2.innerHTML = slidesHtml;

    // Reinitialize swiper if needed
    if (window.swiperInstances) {
        Object.values(window.swiperInstances).forEach(swiper => {
            if (swiper && typeof swiper.update === 'function') {
                swiper.update();
            }
        });
    }
}

// Render product description
function renderDescription(product) {
    // Update Intro tab content
    const introTab = document.getElementById('Intro');
    if (introTab && product.description) {
        const descriptionPara = introTab.querySelector('p');
        if (descriptionPara) {
            descriptionPara.textContent = product.description;
        }
    }

    // Update features list
    const featuresList = document.getElementById('product-features-list');
    if (featuresList && product.description) {
        // Split description by newlines or create list items
        const features = product.description.split('\n').filter(f => f.trim());
        if (features.length > 0) {
            featuresList.innerHTML = features.map(feature => `
                <li class="flex items-center space-x-3">
                    <span class="inline-block text-base">${feature.trim()}</span>
                </li>
            `).join('');
        }
    }
}

// Render product specifications
function renderSpecifications(product) {
    // Update Specifications tab content
    const specsTab = document.getElementById('Specifications');
    if (!specsTab) return;

    const specs = [];
    
    if (product.category) {
        specs.push({ label: 'دسته‌بندی', value: product.category.name || product.category });
    }
    if (product.brand) {
        specs.push({ label: 'برند', value: product.brand.name || product.brand });
    }
    if (product.sku) {
        specs.push({ label: 'کد محصول', value: product.sku });
    }
    if (product.weight) {
        specs.push({ label: 'وزن', value: `${product.weight} گرم` });
    }
    if (product.dimensions) {
        specs.push({ label: 'ابعاد', value: product.dimensions });
    }
    if (product.stockQuantity !== undefined) {
        specs.push({ label: 'موجودی', value: product.stockQuantity > 0 ? `${product.stockQuantity} عدد` : 'ناموجود' });
    }

    if (specs.length > 0) {
        const specsDiv = specsTab.querySelector('div.space-y-5 > div');
        if (specsDiv) {
            specsDiv.innerHTML = `
                <h2 class="text-2xl pb-3 font-black text-zinc-800 relative before:absolute before:bottom-0 before:start-0 before:h-1 before:w-22 before:bg-primary-500 before:rounded dark:text-white">مشخصات محصول</h2>
                <div class="space-y-3">
                    ${specs.map(spec => `
                        <div class="flex justify-between py-2 border-b border-gray-200 dark:border-gray-600">
                            <span class="font-medium text-gray-700 dark:text-gray-300">${spec.label}:</span>
                            <span class="text-gray-600 dark:text-gray-400">${spec.value}</span>
                        </div>
                    `).join('')}
                </div>
            `;
        }
    }
}

// Format price
function formatPrice(price) {
    return new Intl.NumberFormat('fa-IR').format(Math.round(price));
}

// Show loading state
function showLoading() {
    const loadingEl = document.getElementById('product-loading');
    if (loadingEl) loadingEl.classList.remove('hidden');
}

// Hide loading state
function hideLoading() {
    const loadingEl = document.getElementById('product-loading');
    if (loadingEl) loadingEl.classList.add('hidden');
}

// Show error message
function showError(message) {
    const errorEl = document.getElementById('product-error');
    if (errorEl) {
        errorEl.textContent = message;
        errorEl.classList.remove('hidden');
    } else if (window.utils && window.utils.showToast) {
        window.utils.showToast(message, 'error');
    } else {
        alert(message);
    }
}

