/**
 * Cart Page (cart.html) - Full cart management
 * Uses window.apiClient and window.cartService (loaded as global scripts)
 */

class CartPage {
  constructor() {
    this.init();
  }

  async init() {
    // Load categories in mega menu
    if (window.categoryService) {
      try {
        const megaMenuContainer = document.getElementById("mega-menu-list-container");
        if (megaMenuContainer) {
          await window.categoryService.renderMegaMenu("mega-menu-list-container");
        }
      } catch (e) {
        console.warn("Failed to load mega menu:", e);
      }
    }

    // Render cart
    await this.renderCart();

    // Listen for cart updates
    window.addEventListener("cart-updated", () => this.renderCart());

    // Setup checkout button
    this.setupCheckoutButton();
  }

  async renderCart() {
    const container = document.getElementById("cart-items-container");
    if (!container) return;

    // Show loading
    container.innerHTML = `
      <div class="flex justify-center items-center py-10">
        <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-green-500"></div>
      </div>
    `;

    try {
      const response = await window.cartService.getUserCart();

      if (!response || !response.success) {
        container.innerHTML = `
          <div class="text-center p-10 space-y-4">
            <svg xmlns="http://www.w3.org/2000/svg" class="mx-auto h-16 w-16 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
            </svg>
            <h2 class="text-xl font-bold text-gray-600 dark:text-gray-300">سبد خرید شما خالی است</h2>
            <a href="/shop.html" class="inline-block bg-green-600 text-white px-6 py-3 rounded-lg hover:bg-green-700 transition">مشاهده محصولات</a>
          </div>
        `;
        this.updateSummary(0, 0, 0);
        return;
      }

      const cartData = response.data?.data || response.data;
      const items = cartData?.items || [];

      if (items.length === 0) {
        container.innerHTML = `
          <div class="text-center p-10 space-y-4">
            <svg xmlns="http://www.w3.org/2000/svg" class="mx-auto h-16 w-16 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
            </svg>
            <h2 class="text-xl font-bold text-gray-600 dark:text-gray-300">سبد خرید شما خالی است</h2>
            <a href="/shop.html" class="inline-block bg-green-600 text-white px-6 py-3 rounded-lg hover:bg-green-700 transition">مشاهده محصولات</a>
          </div>
        `;
        this.updateSummary(0, 0, 0);
        return;
      }

      // Render items
      container.innerHTML = items.map(item => this.renderCartItem(item)).join("");

      // Attach event listeners
      items.forEach(item => {
        const incBtn = document.getElementById(`inc-${item.id}`);
        const decBtn = document.getElementById(`dec-${item.id}`);
        const removeBtn = document.getElementById(`remove-${item.id}`);

        if (incBtn) {
          incBtn.onclick = () => this.updateItemQuantity(item.id, item.quantity + 1);
        }
        if (decBtn) {
          decBtn.onclick = () => {
            if (item.quantity <= 1) {
              this.removeItem(item.id);
            } else {
              this.updateItemQuantity(item.id, item.quantity - 1);
            }
          };
        }
        if (removeBtn) {
          removeBtn.onclick = (e) => {
            e.preventDefault();
            this.removeItem(item.id);
          };
        }
      });

      // Update summary
      const subtotal = cartData.subtotal || items.reduce((sum, i) => sum + i.totalPrice, 0);
      const discount = cartData.discountAmount || 0;
      const shipping = cartData.shippingCost || 0;
      this.updateSummary(subtotal, discount, shipping);

    } catch (error) {
      console.error("Error loading cart:", error);
      container.innerHTML = `
        <div class="text-center p-10 space-y-4">
          <h2 class="text-xl font-bold text-red-500">خطا در بارگذاری سبد خرید</h2>
          <p class="text-gray-500">${error.message || "لطفاً دوباره تلاش کنید"}</p>
          <button onclick="location.reload()" class="inline-block bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition">تلاش مجدد</button>
        </div>
      `;
    }
  }

  renderCartItem(item) {
    const imageUrl = this.normalizeImageUrl(item.productImage);
    const unitPrice = this.formatPrice(item.unitPrice);
    const totalPrice = this.formatPrice(item.totalPrice);
    const variantInfo = item.variantInfo || "";

    return `
      <li class="mb-4">
        <div class="grid grid-cols-4 gap-4 dark:bg-gray-800 dark:text-white bg-white rounded-lg drop-shadow-lg border-gray-300 border p-4">
          <div class="lg:col-span-3 col-span-4 w-full">
            <div class="flex flex-wrap">
              <figure>
                <a href="/product.html?id=${item.productId}">
                  <img class="size-32 object-cover rounded-lg" 
                       src="${imageUrl}" 
                       alt="${this.escapeHtml(item.productName)}"
                       onerror="this.src='assets/images/product/nophoto.png'">
                </a>
              </figure>
              <div class="space-y-3 ms-4 flex-1">
                <a href="/product.html?id=${item.productId}" class="font-bold text-base hover:text-green-600 transition">${this.escapeHtml(item.productName)}</a>
                ${variantInfo ? `<p class="text-sm text-gray-500 dark:text-gray-400">${this.escapeHtml(variantInfo)}</p>` : ""}
                <p class="text-sm text-gray-500">قیمت واحد: ${unitPrice} ریال</p>
                <div class="flex items-center mt-2">
                  <div class="inline-flex items-center space-x-2 border rounded-full px-4 py-2 dark:bg-zinc-800 bg-white shadow">
                    <button id="inc-${item.id}" 
                            class="bg-green-500 text-white w-8 h-8 rounded-full flex items-center justify-center text-lg hover:bg-green-600 transition cursor-pointer"
                            ${!item.isAvailable || item.quantity >= item.availableStock ? 'disabled' : ''}>+</button>
                    <span class="text-lg px-4 inline-block font-semibold">${item.quantity}</span>
                    <button id="dec-${item.id}" 
                            class="bg-gray-200 text-gray-600 w-8 h-8 rounded-full flex items-center justify-center text-lg hover:bg-gray-300 transition cursor-pointer">−</button>
                  </div>
                  <button id="remove-${item.id}" class="p-2 bg-red-500 rounded-full text-white ms-3 hover:bg-red-600 transition cursor-pointer" title="حذف">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-5">
                      <path stroke-linecap="round" stroke-linejoin="round" d="m14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0" />
                    </svg>
                  </button>
                </div>
              </div>
            </div>
          </div>
          <div class="lg:col-span-1 col-span-4 w-full flex items-end justify-end">
            <span class="text-xl font-bold dark:text-white">${totalPrice} <span class="text-xs">ریال</span></span>
          </div>
        </div>
      </li>
    `;
  }

  async updateItemQuantity(cartItemId, newQuantity) {
    try {
      await window.cartService.updateCartItem(null, cartItemId, newQuantity);
      await this.renderCart();
    } catch (error) {
      console.error("Update quantity error:", error);
      this.showToast(error.message || "خطا در به‌روزرسانی تعداد", "error");
    }
  }

  async removeItem(cartItemId) {
    try {
      await window.cartService.removeFromCart(null, cartItemId);
      this.showToast("محصول از سبد خرید حذف شد", "success");
      await this.renderCart();
    } catch (error) {
      console.error("Remove item error:", error);
      this.showToast(error.message || "خطا در حذف محصول", "error");
    }
  }

  updateSummary(subtotal, discount, shipping) {
    const subtotalEl = document.getElementById("cart-subtotal");
    const discountEl = document.getElementById("cart-discount");
    const shippingEl = document.getElementById("cart-shipping");
    const totalEl = document.getElementById("cart-total");

    const total = subtotal - discount + shipping;

    if (subtotalEl) subtotalEl.textContent = `${this.formatPrice(subtotal)} ریال`;
    if (discountEl) discountEl.textContent = `${this.formatPrice(discount)} ریال`;
    if (shippingEl) shippingEl.textContent = shipping > 0 ? `${this.formatPrice(shipping)} ریال` : "رایگان";
    if (totalEl) totalEl.textContent = `${this.formatPrice(total)} ریال`;
  }

  setupCheckoutButton() {
    const checkoutBtn = document.getElementById("checkout-btn");
    if (checkoutBtn) {
      checkoutBtn.onclick = () => {
        if (!window.apiClient || !window.apiClient.isAuthenticated()) {
          localStorage.setItem("intendedUrl", "checkout.html");
          window.location.href = `login.html?returnUrl=${encodeURIComponent("checkout.html")}`;
          return;
        }
        window.location.href = "/checkout.html";
      };
    }
  }

  normalizeImageUrl(url) {
    if (!url) return "assets/images/product/nophoto.png";
    if (url.startsWith("http://") || url.startsWith("https://")) return url;
    if (url.startsWith("/")) return `/api/ImageProxy?url=${encodeURIComponent(url)}`;
    return url;
  }

  formatPrice(price) {
    return new Intl.NumberFormat("fa-IR").format(Math.round(price || 0));
  }

  escapeHtml(str) {
    return String(str || "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  showToast(message, type = "info") {
    if (window.utils && typeof window.utils.showToast === "function") {
      window.utils.showToast(message, type);
      return;
    }
    const existing = document.getElementById("custom-toast");
    if (existing) existing.remove();

    const colorMap = { success: "bg-green-600", error: "bg-red-600", warning: "bg-yellow-500", info: "bg-blue-600" };
    const toast = document.createElement("div");
    toast.id = "custom-toast";
    toast.className = `fixed top-5 left-1/2 -translate-x-1/2 z-[9999] ${colorMap[type] || colorMap.info} text-white px-6 py-3 rounded-xl shadow-2xl text-sm font-semibold transition-all duration-300 opacity-0 translate-y-[-20px]`;
    toast.textContent = message;
    document.body.appendChild(toast);
    requestAnimationFrame(() => {
      toast.classList.remove("opacity-0", "translate-y-[-20px]");
      toast.classList.add("opacity-100", "translate-y-0");
    });
    setTimeout(() => {
      toast.classList.remove("opacity-100", "translate-y-0");
      toast.classList.add("opacity-0", "translate-y-[-20px]");
      setTimeout(() => toast.remove(), 300);
    }, 3000);
  }
}

// Initialize when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  new CartPage();
});
