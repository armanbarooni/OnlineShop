(function () {
    "use strict";

    const NO_PHOTO = "assets/images/product/nophoto.png";
    const MAHAK_CONTENT_BASE_URL = "https://mahakacc.mahaksoft.com";

    const state = {
        cart: null,
        items: [],
        addresses: [],
        selectedAddress: null
    };

    function formatPrice(value) {
        return new Intl.NumberFormat("fa-IR").format(Math.round(Number(value) || 0));
    }

    function escapeHtml(value) {
        return String(value || "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function isDevelopmentHost() {
        const hostname = window.location?.hostname?.toLowerCase() || "";
        return hostname === "localhost" || hostname === "127.0.0.1";
    }

    function proxiedImageUrl(url) {
        if (!url) return NO_PHOTO;
        if (url.startsWith("/")) return `/api/ImageProxy?url=${encodeURIComponent(url)}`;
        return url;
    }

    function normalizeImageUrl(raw) {
        if (!raw) return NO_PHOTO;
        
        if (raw.startsWith("http://") || raw.startsWith("https://")) {
            return raw.includes("mahaksoft.com") && isDevelopmentHost()
                ? proxiedImageUrl(raw)
                : raw;
        }

        if (raw.startsWith("/api/v3/Content/Images/") || raw.startsWith("api/v3/Content/Images/")) {
            const path = raw.startsWith("/") ? raw : "/" + raw;
            const absoluteUrl = MAHAK_CONTENT_BASE_URL + path;
            return isDevelopmentHost() ? proxiedImageUrl(absoluteUrl) : absoluteUrl;
        }

        return raw.startsWith("/") ? proxiedImageUrl(raw) : raw;
    }

    function getItemQuantity(item) {
        return Number(item.quantity ?? item.Quantity ?? 1) || 1;
    }

    function getItemUnitPrice(item) {
        return Number(
            item.unitPrice ??
            item.UnitPrice ??
            item.price ??
            item.Price ??
            item.productPrice ??
            item.ProductPrice ??
            item.product?.price ??
            item.product?.Price ??
            0
        ) || 0;
    }

    function getItemOriginalUnitPrice(item) {
        return Number(
            item.originalPrice ??
            item.OriginalPrice ??
            item.productOriginalPrice ??
            item.ProductOriginalPrice ??
            item.product?.originalPrice ??
            item.product?.OriginalPrice ??
            getItemUnitPrice(item)
        ) || getItemUnitPrice(item);
    }

    function getItemTotal(item) {
        return getItemUnitPrice(item) * getItemQuantity(item);
    }

    function getItemName(item) {
        return (
            item.productName ||
            item.ProductName ||
            item.name ||
            item.Name ||
            item.product?.name ||
            item.product?.Name ||
            "نامشخص"
        );
    }

    function getItemImage(item) {
        return normalizeImageUrl(
            item.productImage ||
            item.ProductImage ||
            item.productImageUrl ||
            item.ProductImageUrl ||
            item.imageUrl ||
            item.ImageUrl ||
            item.product?.imageUrl ||
            item.product?.ImageUrl
        );
    }

    function getItemVariantInfo(item) {
        return (
            item.variantInfo ||
            item.VariantInfo ||
            item.variantName ||
            item.VariantName ||
            ""
        );
    }

    function calculateTotals() {
        const subtotal = state.items.reduce((sum, item) => {
            return sum + getItemOriginalUnitPrice(item) * getItemQuantity(item);
        }, 0);

        const saleTotal = state.items.reduce((sum, item) => sum + getItemTotal(item), 0);
        const cartDiscount = Number(
            state.cart?.discountAmount ??
            state.cart?.DiscountAmount ??
            state.cart?.discount ??
            state.cart?.Discount ??
            0
        ) || 0;
        const itemDiscount = Math.max(subtotal - saleTotal, 0);
        const discount = cartDiscount > 0 ? cartDiscount : itemDiscount;
        const total = Math.max(subtotal - discount, 0);

        return { subtotal, discount, total };
    }

    function setText(id, text) {
        const element = document.getElementById(id);
        if (element) element.textContent = text;
    }

    function getAddressText(address) {
        const parts = [
            address.state,
            address.city,
            address.addressLine1,
            address.addressLine2
        ].filter(Boolean);
        return parts.join("، ");
    }

    function renderSelectedAddress(address) {
        const addressDetails = document.getElementById("checkout-address-details");
        const changeButton = document.getElementById("change-address-btn");

        if (!address) {
            if (addressDetails) {
                addressDetails.innerHTML = `
                    <span class="text-red-500">لطفا آدرس را وارد کنید</span>
                    <a href="user-panel-address.html" class="text-primary font-medium mt-2 inline-block">افزودن آدرس جدید</a>
                `;
            }
            if (changeButton) changeButton.disabled = false;
            return;
        }

        state.selectedAddress = address;
        if (changeButton) changeButton.disabled = false;

        if (addressDetails) {
            addressDetails.innerHTML = `
                <span>${escapeHtml(getAddressText(address))}</span>
                <span>کد پستی: ${escapeHtml(address.postalCode || "-")}</span>
            `;
        }
    }

    function renderAddressModal() {
        const container = document.getElementById("checkout-address-list");
        if (!container) return;

        if (!state.addresses.length) {
            container.innerHTML = `
                <div class="text-center py-8 text-gray-500 dark:text-gray-300">
                    <p>آدرسی برای نمایش وجود ندارد.</p>
                    <a href="user-panel-address.html" class="inline-block mt-4 px-4 py-2 rounded-lg bg-primary text-white">افزودن آدرس جدید</a>
                </div>
            `;
            return;
        }

        container.innerHTML = state.addresses.map(address => {
            const isSelected = state.selectedAddress && address.id === state.selectedAddress.id;
            return `
                <button type="button"
                    data-checkout-address-id="${escapeHtml(address.id)}"
                    class="w-full text-start p-4 rounded-lg border transition ${isSelected ? "border-primary bg-primary/5" : "border-gray-200 dark:border-gray-700 hover:border-primary"}">
                    <div class="flex items-start justify-between gap-4">
                        <div class="space-y-2">
                            <div class="font-bold text-gray-800 dark:text-white">${escapeHtml(address.title || "آدرس")}</div>
                            <div class="text-sm text-gray-600 dark:text-gray-300">${escapeHtml(getAddressText(address))}</div>
                            <div class="text-xs text-gray-500 dark:text-gray-400">کد پستی: ${escapeHtml(address.postalCode || "-")}</div>
                            <div class="text-xs text-gray-500 dark:text-gray-400">${escapeHtml(`${address.firstName || ""} ${address.lastName || ""}`.trim())} ${address.phoneNumber ? " - " + escapeHtml(address.phoneNumber) : ""}</div>
                        </div>
                        ${address.isDefault ? `<span class="shrink-0 text-xs px-2 py-1 rounded-full bg-primary/10 text-primary">پیش‌فرض</span>` : ""}
                    </div>
                </button>
            `;
        }).join("");
    }

    function openAddressModal() {
        renderAddressModal();
        document.getElementById("addressSelectionModal")?.classList.remove("hidden");
    }

    function closeAddressModal() {
        document.getElementById("addressSelectionModal")?.classList.add("hidden");
    }

    function renderCartItems() {
        const container = document.getElementById("checkout-cart-items");
        if (!container) return;

        if (!state.items.length) {
            container.innerHTML = `
                <div class="py-8 text-center text-gray-500 dark:text-gray-300">
                    <p class="font-bold">سبد خرید شما خالی است.</p>
                    <a href="shop.html" class="inline-block mt-4 px-4 py-2 rounded-lg bg-primary text-white">مشاهده محصولات</a>
                </div>
            `;
            return;
        }

        container.innerHTML = state.items.map(item => {
            const quantity = getItemQuantity(item);
            const total = getItemTotal(item);
            const variantInfo = getItemVariantInfo(item);

            return `
                <div class="flex items-center border-b border-gray-100 dark:border-gray-700 pb-4 last:border-0 last:pb-0">
                    <img src="${getItemImage(item)}" alt="${escapeHtml(getItemName(item))}"
                        class="w-16 h-16 object-contain rounded-md bg-gray-50 dark:bg-gray-700"
                        loading="lazy" onerror="this.onerror=null; this.src='${NO_PHOTO}'">
                    <div class="ms-3 flex-1">
                        <h3 class="font-medium text-gray-700 dark:text-white">${escapeHtml(getItemName(item))}</h3>
                        <p class="text-sm text-gray-500 dark:text-gray-300">تعداد: ${formatPrice(quantity)}</p>
                        ${variantInfo ? `<p class="text-xs text-gray-500 dark:text-gray-300">${escapeHtml(variantInfo)}</p>` : ""}
                    </div>
                    <div class="text-end">
                        <span class="block text-green-500 text-sm">موجود</span>
                        <span class="block mt-1 font-bold text-gray-700 dark:text-white">${formatPrice(total)} ریال</span>
                    </div>
                </div>
            `;
        }).join("");
    }

    function renderTotals() {
        const totals = calculateTotals();
        setText("checkout-subtotal", `${formatPrice(totals.subtotal)} ریال`);
        setText("checkout-discount", `${formatPrice(totals.discount)} ریال`);
        setText("checkout-total", `${formatPrice(totals.total)} ریال`);
    }

    async function loadCart() {
        const result = await window.cartService.getUserCart();
        if (!result.success) throw new Error(result.error || "خطا در دریافت سبد خرید");

        state.cart = result.data || {};
        state.items = result.items || state.cart.items || [];
        
        if (!state.items || state.items.length === 0) {
            window.location.href = 'cart.html';
            return;
        }
        
        renderCartItems();
        renderTotals();
    }

    async function loadAddresses() {
        const result = await window.addressService.getAddresses();
        state.addresses = result.success && Array.isArray(result.data) ? result.data : [];
        const defaultAddress = state.addresses.find(address => address.isDefault) || state.addresses[0] || null;
        renderSelectedAddress(defaultAddress);
        renderAddressModal();
    }

    function bindEvents() {
        document.getElementById("change-address-btn")?.addEventListener("click", openAddressModal);
        
        document.querySelectorAll("[data-modal-close]").forEach(btn => {
            btn.addEventListener("click", closeAddressModal);
        });

        document.getElementById("checkout-address-list")?.addEventListener("click", event => {
            const button = event.target.closest("[data-checkout-address-id]");
            if (!button) return;

            const address = state.addresses.find(item => String(item.id) === String(button.dataset.checkoutAddressId));
            if (!address) return;

            renderSelectedAddress(address);
            closeAddressModal();
        });

        document.getElementById("checkout-submit-btn")?.addEventListener("click", async event => {
            event.preventDefault();

            if (!state.items.length) {
                window.utils?.showToast?.("سبد خرید شما خالی است", "error");
                return;
            }

            if (!state.selectedAddress) {
                window.utils?.showToast?.("لطفا آدرس تحویل سفارش را انتخاب کنید", "error");
                return;
            }

            const btn = document.getElementById("checkout-submit-btn");
            const originalText = btn.textContent;
            btn.textContent = "در حال انتقال به درگاه...";
            btn.classList.add("opacity-70", "pointer-events-none");

            try {
                // Step 1: Process Checkout (Creates the order and clears the cart)
                const checkoutResult = await window.apiClient.post('/Checkout', {
                    cartId: state.cart.id,
                    shippingAddressId: state.selectedAddress.id
                });

                if (!checkoutResult.isSuccess || !checkoutResult.data?.order?.id) {
                    throw new Error(checkoutResult.errorMessage || "خطا در ثبت سفارش");
                }

                const orderId = checkoutResult.data.order.id;

                // Step 2: Initiate Payment via API using OrderId
                const paymentResult = await window.apiClient.post('/Payment/initiate', {
                    orderId: orderId,
                    gateway: "ZarinPal"
                });

                if (paymentResult.isSuccess && paymentResult.data?.paymentUrl) {
                    // Update cart count UI proactively before redirect
                    if (window.cartService && typeof window.cartService.updateCartCount === "function") {
                        await window.cartService.updateCartCount();
                    }
                    window.location.href = paymentResult.data.paymentUrl;
                } else {
                    throw new Error(paymentResult.errorMessage || "خطا در اتصال به درگاه پرداخت");
                }
            } catch (error) {
                window.logger?.error("Checkout/Payment error:", error);
                window.utils?.showToast?.(error.message || "خطا در عملیات تسویه حساب", "error");
                btn.textContent = originalText;
                btn.classList.remove("opacity-70", "pointer-events-none");
            }
        });
    }

    async function init() {
        if (!window.authService?.isAuthenticated()) {
            window.location.href = `login.html?returnUrl=${encodeURIComponent(window.location.pathname)}`;
            return;
        }

        bindEvents();

        try {
            await Promise.all([loadCart(), loadAddresses()]);
            window.cartService?.updateCartCount?.();
            window.cartService?.renderCartOffcanvas?.();
        } catch (error) {
            window.logger?.error("Checkout initialization failed:", error);
            window.utils?.showToast?.(error.message || "خطا در دریافت اطلاعات تسویه حساب", "error");
        }
    }

    document.addEventListener("DOMContentLoaded", init);
})();
