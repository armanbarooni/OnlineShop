/**
 * Cart Service
 * Supports authenticated carts via API and guest carts via localStorage.
 */
class CartService {
    constructor() {
        this.apiClient = window.apiClient;
        this.baseUrl = "/cart";
        this.storageKey = (window.config && window.config.storage && window.config.storage.cartItems) || "cartItems";
        this._guestCartTransferPromise = null;
    }

    isAuthenticated() {
        return !!(this.apiClient && this.apiClient.isAuthenticated && this.apiClient.isAuthenticated());
    }

    getGuestCartItems() {
        try {
            const raw = localStorage.getItem(this.storageKey);
            const items = raw ? JSON.parse(raw) : [];
            return Array.isArray(items) ? items : [];
        } catch (error) {
            window.logger?.error("Error loading guest cart:", error);
            return [];
        }
    }

    saveGuestCartItems(items) {
        localStorage.setItem(this.storageKey, JSON.stringify(items));
        this.notifyCartUpdated();
    }

    async transferGuestCartToUser() {
        if (!this.isAuthenticated()) {
            return {
                success: false,
                transferredCount: 0,
                remainingCount: this.getGuestCartItems().length,
                message: "کاربر احراز هویت نشده است"
            };
        }

        if (this._guestCartTransferPromise) {
            return this._guestCartTransferPromise;
        }

        this._guestCartTransferPromise = (async () => {
            const guestItems = this.getGuestCartItems();
            if (!guestItems.length) {
                return {
                    success: true,
                    transferredCount: 0,
                    remainingCount: 0
                };
            }

            const remainingItems = [];
            let transferredCount = 0;

            for (const item of guestItems) {
                try {
                    const variantId = this.normalizeVariantKey(item.variantId) || null;
                    const quantity = Number(item.quantity || 0);

                    if (!item.productId || quantity <= 0) {
                        remainingItems.push(item);
                        continue;
                    }

                    const result = await this.addToCart(
                        item.productId,
                        quantity,
                        variantId ? item.variantId : null
                    );

                    if (result?.success) {
                        transferredCount += 1;
                    } else {
                        remainingItems.push(item);
                    }
                } catch (error) {
                    window.logger?.error("Error transferring guest cart item:", error);
                    remainingItems.push(item);
                }
            }

            if (remainingItems.length === 0) {
                localStorage.removeItem(this.storageKey);
            } else {
                localStorage.setItem(this.storageKey, JSON.stringify(remainingItems));
            }

            this.notifyCartUpdated();

            return {
                success: remainingItems.length === 0,
                transferredCount,
                remainingCount: remainingItems.length
            };
        })();

        try {
            return await this._guestCartTransferPromise;
        } finally {
            this._guestCartTransferPromise = null;
        }
    }

    notifyCartUpdated() {
        this.updateCartCount();
        this.renderCartOffcanvas();
        window.dispatchEvent(new CustomEvent("cart-updated"));
    }

    buildGuestCart(items) {
        const normalizedItems = Array.isArray(items) ? items : [];
        const subtotal = normalizedItems.reduce((sum, item) => sum + ((item.totalPrice ?? (item.unitPrice || 0) * (item.quantity || 0)) || 0), 0);
        const shippingCost = 0;

        return {
            id: null,
            userId: null,
            items: normalizedItems,
            subtotal,
            discountAmount: 0,
            shippingCost,
            totalAmount: subtotal + shippingCost,
            totalItems: normalizedItems.reduce((sum, item) => sum + (item.quantity || 0), 0)
        };
    }

    async getUserCart() {
        if (!this.isAuthenticated()) {
            const guestItems = await this.refreshGuestCartPrices();
            const guestCart = this.buildGuestCart(guestItems);
            return {
                success: true,
                data: guestCart,
                items: guestCart.items,
                isGuest: true
            };
        }

        try {
            const response = await this.apiClient.get(this.baseUrl);
            if (response.success !== undefined && !response.success) {
                return response;
            }

            const data = response.data || response;
            const cart = data.data || data;

            return {
                success: true,
                data: cart,
                items: cart?.items || [],
                isGuest: false
            };
        } catch (error) {
            window.logger?.error("Error getting user cart:", error);
            return {
                success: false,
                error: error.message || "خطا در دریافت سبد خرید"
            };
        }
    }

    async getCartItems() {
        const result = await this.getUserCart();
        if (result.success && result.items) {
            return {
                success: true,
                data: result.items
            };
        }

        return result;
    }

    async addToCart(productId, quantity = 1, variantId = null, _cartId = null, productData = null) {
        if (!this.isAuthenticated()) {
            return await this.addToGuestCart(productId, quantity, variantId, productData);
        }

        try {
            const payload = {
                productId,
                quantity,
                variantId: variantId || "00000000-0000-0000-0000-000000000000"
            };

            const response = await this.apiClient.post(`${this.baseUrl}/add`, payload);
            if (response.success !== undefined && !response.success) {
                return response;
            }

            this.notifyCartUpdated();

            const data = response.data || response;
            return {
                success: true,
                data: data.data || data
            };
        } catch (error) {
            window.logger?.error("Error adding to cart:", error);
            return {
                success: false,
                error: error.message || "خطا در افزودن به سبد خرید"
            };
        }
    }

    async addToGuestCart(productId, quantity = 1, variantId = null, productData = null) {
        try {
            const resolvedProduct = productData || await this.fetchProduct(productId);
            if (!resolvedProduct) {
                return {
                    success: false,
                    error: "محصول یافت نشد"
                };
            }

            const items = this.getGuestCartItems();
            const productKey = String(productId);
            const variantKey = this.normalizeVariantKey(variantId);
            const existingItem = items.find(item => String(item.productId) === productKey && String(item.variantId || "") === variantKey);
            const availableStock = this.getAvailableStock(resolvedProduct, variantId) ?? 999;

            const unitPrice = this.getProductPayablePrice(resolvedProduct);

            const productName = resolvedProduct.name || resolvedProduct.productName || "محصول";
            const productImage =
                resolvedProduct.productImage ||
                resolvedProduct.imageUrl ||
                resolvedProduct.productImages?.find(image => image?.isPrimary)?.imageUrl ||
                resolvedProduct.productImages?.[0]?.imageUrl ||
                resolvedProduct.images?.find(image => image?.isPrimary)?.imageUrl ||
                resolvedProduct.images?.[0]?.imageUrl ||
                null;

            if (existingItem) {
                if (existingItem.quantity + quantity > availableStock) {
                    this.notifyCartUpdated();
                    return {
                        success: true,
                        data: this.buildGuestCart(items),
                        alreadyAtMaxStock: true,
                        message: "این رنگ و سایز قبلا به سبد خرید اضافه شده و موجودی بیشتری ندارد"
                    };
                }

                existingItem.quantity += quantity;
                existingItem.totalPrice = existingItem.unitPrice * existingItem.quantity;
            } else {
                if (quantity > availableStock) {
                    return {
                        success: false,
                        error: `موجودی کافی نیست. موجودی فعلی: ${availableStock}`
                    };
                }

                items.push({
                    id: `${productKey}:${variantKey || "default"}`,
                    productId,
                    productName,
                    productImage,
                    variantId: variantId || null,
                    variantInfo: this.getVariantInfo(resolvedProduct, variantId),
                    originalUnitPrice: Number(resolvedProduct.price ?? unitPrice) || unitPrice,
                    hasDiscount: (Number(resolvedProduct.price2) || 0) > 0 || unitPrice < (Number(resolvedProduct.price) || unitPrice),
                    unitPrice,
                    quantity,
                    totalPrice: unitPrice * quantity,
                    availableStock,
                    isAvailable: true
                });
            }

            this.saveGuestCartItems(items);

            return {
                success: true,
                data: this.buildGuestCart(items)
            };
        } catch (error) {
            window.logger?.error("Error adding guest cart item:", error);
            return {
                success: false,
                error: error.message || "خطا در افزودن به سبد خرید"
            };
        }
    }

    async updateCartItem(_cartId, itemId, quantity) {
        if (!this.isAuthenticated()) {
            try {
                const items = this.getGuestCartItems();
                const updatedItems = items
                    .map(item => item.id === itemId ? {
                        ...item,
                        quantity,
                        totalPrice: (item.unitPrice || 0) * quantity
                    } : item)
                    .filter(item => (item.quantity || 0) > 0);

                this.saveGuestCartItems(updatedItems);
                return {
                    success: true,
                    data: this.buildGuestCart(updatedItems)
                };
            } catch (error) {
                window.logger?.error("Error updating guest cart item:", error);
                return {
                    success: false,
                    error: error.message || "خطا در به‌روزرسانی آیتم سبد خرید"
                };
            }
        }

        try {
            const response = await this.apiClient.put(`${this.baseUrl}/update`, {
                cartItemId: itemId,
                quantity
            });

            if (response.success !== undefined && !response.success) {
                return response;
            }

            this.notifyCartUpdated();

            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger?.error("Error updating cart item:", error);
            return {
                success: false,
                error: error.message || "خطا در به‌روزرسانی آیتم سبد خرید"
            };
        }
    }

    async removeFromCart(_cartId, itemId) {
        if (!this.isAuthenticated()) {
            try {
                const items = this.getGuestCartItems().filter(item => item.id !== itemId);
                this.saveGuestCartItems(items);
                return {
                    success: true,
                    data: this.buildGuestCart(items)
                };
            } catch (error) {
                window.logger?.error("Error removing guest cart item:", error);
                return {
                    success: false,
                    error: error.message || "خطا در حذف از سبد خرید"
                };
            }
        }

        try {
            const response = await this.apiClient.delete(`${this.baseUrl}/remove/${itemId}`);
            this.notifyCartUpdated();

            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger?.error("Error removing cart item:", error);
            return {
                success: false,
                error: error.message || "خطا در حذف از سبد خرید"
            };
        }
    }

    async clearCart() {
        if (!this.isAuthenticated()) {
            this.saveGuestCartItems([]);
            return {
                success: true,
                data: { items: [] }
            };
        }

        try {
            const cartResult = await this.getUserCart();
            if (!cartResult.success || !cartResult.data?.items?.length) {
                return {
                    success: true,
                    data: { items: [] }
                };
            }

            for (const item of cartResult.data.items) {
                await this.apiClient.delete(`${this.baseUrl}/remove/${item.id}`);
            }

            this.notifyCartUpdated();

            return {
                success: true,
                data: { items: [] }
            };
        } catch (error) {
            window.logger?.error("Error clearing cart:", error);
            return {
                success: false,
                error: error.message || "خطا در پاک کردن سبد خرید"
            };
        }
    }

    async getCartSummary() {
        const result = await this.getUserCart();
        if (!result.success) {
            return result;
        }

        return {
            success: true,
            data: result.data
        };
    }

    updateCartCount() {
        const updateFromItems = (items) => {
            const totalItems = (items || []).reduce((sum, item) => sum + (item.quantity || 0), 0);
            const badges = document.querySelectorAll("#cartCount, .cart-count, [data-cart-count]");

            badges.forEach(badge => {
                badge.textContent = totalItems;
                if ("classList" in badge) {
                    const isMobileNavBadge = Boolean(badge.closest(".site-mobile-nav"));
                    badge.classList.toggle("hidden", totalItems === 0 && !isMobileNavBadge);
                }
            });
        };

        if (!this.isAuthenticated()) {
            updateFromItems(this.getGuestCartItems());
            return;
        }

        this.getUserCart()
            .then(result => {
                if (result.success) {
                    updateFromItems(result.items || result.data?.items || []);
                }
            })
            .catch(error => window.logger?.error("Error updating cart count:", error));
    }

    async renderCartOffcanvas() {
        const offcanvas = document.getElementById("offcanvas-left");
        if (!offcanvas) return;

        const body = offcanvas.querySelector("main");
        const footer = offcanvas.querySelector("footer");
        if (!body || !footer) return;

        const cartResult = await this.getUserCart();
        const items = cartResult.success ? (cartResult.items || cartResult.data?.items || []) : [];

        if (!items.length) {
            body.innerHTML = `
                <div class="py-10 text-center text-gray-500 dark:text-gray-300">
                    <p class="font-bold">سبد خرید شما خالی است</p>
                    <a href="/fa/shop.html" class="inline-block mt-4 bg-primary-grad text-white py-2 px-4 rounded-lg">مشاهده محصولات</a>
                </div>
            `;
            footer.innerHTML = `
                <div class="flex items-center justify-between">
                    <span class="inline-block text-lg">جمع کل</span>
                    <h3 class="font-bold text-xl">0 ریال</h3>
                </div>
            `;
            return;
        }

        body.innerHTML = items.map(item => `
            <div class="py-3 last:mb-35">
                <article class="flex gap-3 rounded-2xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-zinc-900 p-3 shadow-sm">
                    <div class="shrink-0">
                        <img
                            class="h-24 w-24 rounded-xl object-contain bg-gray-50 dark:bg-zinc-800 p-2"
                            src="${this.normalizeImageUrl(item.productImage)}"
                            alt="${this.escapeHtml(item.productName)}"
                            loading="lazy"
                            onerror="this.src='assets/images/product/nophoto.png'"
                        />
                    </div>
                    <div class="min-w-0 flex-1 flex flex-col gap-3">
                        <div>
                            <h3 class="font-bold leading-7 text-sm text-gray-900 dark:text-white line-clamp-2">${this.escapeHtml(item.productName)}</h3>
                            ${item.variantInfo ? `<p class="mt-1 text-xs text-gray-500 dark:text-gray-300">${this.escapeHtml(item.variantInfo)}</p>` : ""}
                        </div>
                        <div class="flex items-center justify-between gap-3">
                            <div class="flex flex-col gap-1">
                                <ins class="no-underline text-lg text-green-600 font-bold">
                                    ${this.formatPrice(item.totalPrice)}
                                    <span class="text-sm font-normal text-gray-700 dark:text-white">ریال</span>
                                </ins>
                                <div class="text-xs text-gray-500 dark:text-gray-300">تعداد: ${item.quantity}</div>
                            </div>
                            <button
                                type="button"
                                class="inline-flex items-center justify-center rounded-xl border border-red-200 bg-red-50 px-3 py-2 text-red-700 transition hover:bg-red-100 dark:border-red-900/40 dark:bg-transparent dark:text-red-300"
                                data-remove-cart-item="${this.escapeHtml(item.id)}"
                                aria-label="حذف محصول از سبد خرید"
                            >
                                حذف
                            </button>
                        </div>
                    </div>
                </article>
            </div>
        `).join("");

        body.querySelectorAll("[data-remove-cart-item]").forEach(button => {
            button.addEventListener("click", async () => {
                await this.removeFromCart(null, button.dataset.removeCartItem);
                this.renderCartOffcanvas();
            });
        });

        const subtotal = items.reduce((sum, item) => sum + (item.totalPrice || (item.unitPrice || 0) * (item.quantity || 0)), 0);
        footer.innerHTML = `
            <div class="flex items-center justify-between">
                <div class="space-y-2">
                    <span class="inline-block text-lg">جمع کل</span>
                    <h3 class="font-bold text-xl">${this.formatPrice(subtotal)} ریال</h3>
                </div>
                <div class="text-end">
                    <a href="/fa/cart.html" class="bg-primary-grad hover:bg-primary-600 text-white py-2 px-4 rounded-lg">
                        مشاهده سبد
                    </a>
                </div>
            </div>
        `;
    }

    validateCartData(cartData) {
        const errors = {};

        if (!cartData.productId) {
            errors.productId = "شناسه محصول الزامی است";
        }

        if (!cartData.quantity || cartData.quantity <= 0) {
            errors.quantity = "تعداد باید بیش‌تر از صفر باشد";
        }

        if (cartData.quantity > 100) {
            errors.quantity = "تعداد نمی‌تواند بیش‌تر از ۱۰۰ باشد";
        }

        return {
            isValid: Object.keys(errors).length === 0,
            errors
        };
    }

    calculateTotals(cartItems, coupon = null) {
        let subtotal = 0;
        let discount = 0;
        let shipping = 0;
        let tax = 0;

        cartItems.forEach(item => {
            subtotal += (item.unitPrice || 0) * (item.quantity || 0);
        });

        if (coupon) {
            if (coupon.discountType === "Percentage") {
                discount = (subtotal * coupon.discountValue) / 100;
                if (coupon.maximumDiscount && discount > coupon.maximumDiscount) {
                    discount = coupon.maximumDiscount;
                }
            } else {
                discount = coupon.discountValue;
            }
        }

        const taxableAmount = subtotal - discount;
        tax = (taxableAmount * 9) / 100;

        return {
            subtotal,
            discount,
            shipping,
            tax,
            total: subtotal - discount + shipping + tax
        };
    }

    formatPrice(price) {
        return new Intl.NumberFormat("fa-IR").format(Math.round(price || 0));
    }

    normalizeImageUrl(url) {
        if (!url) return "assets/images/product/nophoto.png";
        if (url.startsWith("http://") || url.startsWith("https://")) return url;
        if (url.startsWith("/")) return `/api/ImageProxy?url=${encodeURIComponent(url)}`;
        return url;
    }

    escapeHtml(value) {
        return String(value || "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    async fetchProduct(productId) {
        try {
            if (window.productService?.getProductById) {
                const result = await window.productService.getProductById(productId);
                if (result?.success) {
                    return result.data;
                }
            }

            const response = await this.apiClient.get(`/Product/${productId}`);
            return response?.data?.data || response?.data || null;
        } catch (error) {
            window.logger?.error("Error fetching product for guest cart:", error);
            return null;
        }
    }

    getProductPayablePrice(product) {
        const discountedPrice = Number(
            product?.price2 ??
            product?.Price2 ??
            product?.salePrice ??
            product?.SalePrice ??
            0
        ) || 0;

        if (discountedPrice > 0) return discountedPrice;

        return Number(
            product?.price ??
            product?.Price ??
            product?.unitPrice ??
            product?.UnitPrice ??
            0
        ) || 0;
    }

    async refreshGuestCartPrices() {
        const items = this.getGuestCartItems();
        if (!items.length) return items;

        const refreshedItems = await Promise.all(items.map(async item => {
            const product = await this.fetchProduct(item.productId);
            if (!product) return item;

            const unitPrice = this.getProductPayablePrice(product);
            const originalUnitPrice = Number(product.price ?? product.Price ?? unitPrice) || unitPrice;
            const quantity = Number(item.quantity || 0);

            return {
                ...item,
                productName: product.name ?? product.Name ?? item.productName,
                originalUnitPrice,
                unitPrice,
                hasDiscount: (Number(product.price2 ?? product.Price2) || 0) > 0 || unitPrice < originalUnitPrice,
                totalPrice: unitPrice * quantity
            };
        }));

        localStorage.setItem(this.storageKey, JSON.stringify(refreshedItems));
        return refreshedItems;
    }

    normalizeVariantKey(variantId) {
        const value = String(variantId || "").toLowerCase();
        return value === "00000000-0000-0000-0000-000000000000" ? "" : value;
    }

    isSameCartItem(item, productId, variantId) {
        return String(item.productId || "").toLowerCase() === String(productId || "").toLowerCase()
            && this.normalizeVariantKey(item.variantId) === this.normalizeVariantKey(variantId);
    }

    getProductVariants(product) {
        if (!product) return [];

        return [
            ...(Array.isArray(product.productVariants) ? product.productVariants : []),
            ...(Array.isArray(product.ProductVariants) ? product.ProductVariants : []),
            ...(Array.isArray(product.variants) ? product.variants : []),
            ...(Array.isArray(product.Variants) ? product.Variants : [])
        ];
    }

    toStockNumber(value) {
        const stock = Number(value);
        return Number.isFinite(stock) ? stock : null;
    }

    getAvailableStock(product, variantId = null) {
        if (!product) return null;

        if (variantId) {
            const variant = this.getProductVariants(product).find(item =>
                String(item.id ?? item.Id ?? item.variantId ?? item.VariantId) === String(variantId)
            );

            if (!variant) return null;

            return this.toStockNumber(
                variant.stockQuantity ??
                variant.StockQuantity ??
                variant.stock ??
                variant.Stock ??
                variant.quantity ??
                variant.Quantity ??
                variant.count1 ??
                variant.Count1
            );
        }

        return this.toStockNumber(
            product.stockQuantity ??
            product.StockQuantity ??
            product.stock ??
            product.Stock ??
            product.quantity ??
            product.Quantity
        );
    }

    getVariantInfo(product, variantId) {
        if (!variantId || !product) return null;

        const variant = this.getProductVariants(product).find(item => String(item.id ?? item.Id) === String(variantId));
        if (!variant) return null;

        const size = variant.size ?? variant.Size ?? "";
        const color = variant.color ?? variant.Color ?? "";
        const parts = [];
        if (size) parts.push(`سایز: ${size}`);
        if (color) parts.push(`رنگ: ${color}`);
        return parts.length ? parts.join("، ") : null;
    }
}

window.cartService = new CartService();

window.addEventListener("auth:login", () => {
    if (!window.cartService) return;
    window.cartService.transferGuestCartToUser().catch(error => {
        window.logger?.error("Guest cart transfer failed:", error);
    });
});

document.addEventListener("DOMContentLoaded", () => {
    window.cartService.updateCartCount();
    window.cartService.renderCartOffcanvas();
});
