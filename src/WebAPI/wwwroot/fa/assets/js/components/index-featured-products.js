(function () {
  "use strict";

  const CONTAINER_ID = "featuredProducts";
  const MAHAK_CONTENT_BASE_URL =
    window.config?.content?.mahakBaseURL ||
    window.resolveMahakContentBaseURL?.() ||
    "https://mahakacc.mahaksoft.com";
  const DEPENDENCY_RETRY_DELAY_MS = 100;
  const DEPENDENCY_MAX_ATTEMPTS = 50;
  let hasLoadedProducts = false;

  function isDevelopmentHost() {
    const hostname = window.location?.hostname?.toLowerCase() || "";
    return hostname === "localhost" || hostname === "127.0.0.1";
  }

  function proxiedImageUrl(url) {
    const apiBaseUrl = (window.config?.api?.baseURL || "/api").replace(
      /\/$/,
      "",
    );
    return apiBaseUrl + "/ImageProxy?url=" + encodeURIComponent(url);
  }

  function getAbsoluteMahakImageUrl(path) {
    return path.startsWith("http")
      ? path
      : MAHAK_CONTENT_BASE_URL + (path.startsWith("/") ? path : "/" + path);
  }

  function resolveMahakImageUrl(path) {
    const absoluteUrl = getAbsoluteMahakImageUrl(path);
    return isDevelopmentHost() ? proxiedImageUrl(absoluteUrl) : absoluteUrl;
  }

  function findContainer() {
    return (
      document.getElementById(CONTAINER_ID) ||
      document.querySelector('[data-products="featuredProducts"]') ||
      document.querySelector(".product-carousel .swiper-wrapper")
    );
  }

  function extractProducts(result) {
    if (!result) return [];

    let payload = result;
    if (result.data !== undefined) payload = result.data;

    if (
      payload &&
      payload.data !== undefined &&
      (payload.isSuccess === true || payload.success === true)
    ) {
      payload = payload.data;
    }

    if (result.success === false || result.isSuccess === false) return [];
    if (!payload) return [];

    let products = [];
    if (Array.isArray(payload)) products = payload;
    else if (Array.isArray(payload.products)) products = payload.products;
    else if (payload.products && Array.isArray(payload.products.items))
      products = payload.products.items;
    else if (Array.isArray(payload.items)) products = payload.items;
    else if (payload.data && Array.isArray(payload.data))
      products = payload.data;

    return products.filter(isVisibleProduct);
  }

  function isVisibleProduct(product) {
    return (
      !!product &&
      product.deleted !== true &&
      product.Deleted !== true &&
      product.deletedByMahak !== true &&
      product.DeletedByMahak !== true
    );
  }

  function getProductVariantList(product) {
    return [
      ...(Array.isArray(product?.productVariants) ? product.productVariants : []),
      ...(Array.isArray(product?.ProductVariants) ? product.ProductVariants : []),
      ...(Array.isArray(product?.variants) ? product.variants : []),
      ...(Array.isArray(product?.Variants) ? product.Variants : [])
    ];
  }

  function getVariantStock(variant) {
    const stock = Number(
      variant?.stockQuantity ??
      variant?.StockQuantity ??
      variant?.stock ??
      variant?.Stock ??
      variant?.quantity ??
      variant?.Quantity ??
      0
    );
    return Number.isFinite(stock) ? stock : 0;
  }

  function isProductInStock(product) {
    if (getProductVariantList(product).some((variant) => getVariantStock(variant) > 0)) {
      return true;
    }

    const stock = Number(
      product?.stockQuantity ??
      product?.StockQuantity ??
      product?.quantity ??
      product?.Quantity ??
      0
    );
    return Number.isFinite(stock) && stock > 0;
  }

  function formatPrice(price) {
    return new Intl.NumberFormat("fa-IR").format(Math.round(price || 0));
  }

  function getProductImageData(product) {
    const primaryImage = Array.isArray(product.images)
      ? product.images.find(function (image) {
          return image && image.isPrimary;
        }) || product.images[0]
      : null;
    const galleryImage =
      product.productImages && product.productImages.length > 0
        ? product.productImages[0]
        : null;
    const rawImageUrl =
      primaryImage?.imageUrl ||
      galleryImage?.imageUrl ||
      product.imageUrl ||
      "";

    if (!rawImageUrl) {
      return { src: "assets/images/product/nophoto.png", fallback: "" };
    }

    const normalizedImageUrl = rawImageUrl.trim();
    if (!normalizedImageUrl) {
      return { src: "assets/images/product/nophoto.png", fallback: "" };
    }

    if (
      normalizedImageUrl.startsWith("http://") ||
      normalizedImageUrl.startsWith("https://")
    ) {
      if (normalizedImageUrl.includes("mahaksoft.com")) {
        return {
          src: resolveMahakImageUrl(normalizedImageUrl),
          fallback: isDevelopmentHost()
            ? getAbsoluteMahakImageUrl(normalizedImageUrl)
            : "",
        };
      }
      return { src: normalizedImageUrl, fallback: "" };
    }

    if (normalizedImageUrl.startsWith("/api/v3/Content/Images/")) {
      return {
        src: resolveMahakImageUrl(normalizedImageUrl),
        fallback: isDevelopmentHost()
          ? getAbsoluteMahakImageUrl(normalizedImageUrl)
          : "",
      };
    }

    if (normalizedImageUrl.startsWith("api/v3/Content/Images/")) {
      return {
        src: resolveMahakImageUrl(normalizedImageUrl),
        fallback: isDevelopmentHost()
          ? getAbsoluteMahakImageUrl(normalizedImageUrl)
          : "",
      };
    }

    return normalizedImageUrl.startsWith("/")
      ? { src: proxiedImageUrl(normalizedImageUrl), fallback: "" }
      : { src: normalizedImageUrl, fallback: "" };
  }

  function createProductCard(product) {
    const image = getProductImageData(product);
    const imageUrl = image.src;
    const fallbackImageUrl =
      image.fallback || "assets/images/product/nophoto.png";
    const imageErrorHandler = image.fallback
      ? `this.onerror=function(){this.onerror=null; this.src='assets/images/product/nophoto.png'}; this.src='${fallbackImageUrl}'`
      : "this.onerror=null; this.src='assets/images/product/nophoto.png'";
    const hasStock = isProductInStock(product);
    const price = hasStock ? (product.price || 0) : null;
    const originalPrice = hasStock ? (product.originalPrice || price) : null;
    const discount =
      hasStock && originalPrice > price
        ? Math.round(((originalPrice - price) / originalPrice) * 100)
        : 0;
    const productUrl = "product.html?id=" + product.id;
    const name = product.name || "نام محصول";
    const wishlistClick =
      "event.preventDefault(); event.stopPropagation(); addToWishlist('" +
      product.id +
      "')";

    return `
      <div class="swiper-slide px-1.5 py-2">
        <article class="bg-white product-box-item drop-shadow-md rounded-xl p-3 sm:p-4 dark:bg-gray-800 dark:border-white dark:border-1">
          <header class="flex items-center relative justify-between">
            ${discount > 0 ? `<span class="absolute top-1 end-1 bg-red-500 text-white text-xs px-2 py-1 rounded">${discount}%</span>` : ""}
          </header>
          <figure class="relative overflow-hidden rounded-lg mb-3">
            <a href="${productUrl}" class="block">
              <img src="${imageUrl}" alt="${name}" class="w-full h-40 sm:h-48 object-contain" loading="lazy" decoding="async" onerror="${imageErrorHandler}">
            </a>
            <button type="button" data-wishlist-product-id="${product.id}" onclick="${wishlistClick}" class="absolute top-2 start-2 z-30 p-1.5 sm:p-2 bg-white rounded-full shadow-md hover:bg-primary hover:text-white transition dark:bg-gray-800 dark:text-white" aria-label="افزودن به علاقه‌مندی‌ها">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-4 sm:size-5 pointer-events-none">
                <path stroke-linecap="round" stroke-linejoin="round" d="M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.22 9 12 9 12s9-4.78 9-12z"/>
              </svg>
            </button>
          </figure>
                <a href="${productUrl}">
                    <h3 class="text-xs sm:text-sm font-bold mb-2 line-clamp-2 dark:text-white">${name}</h3>
                </a>
                <div class="flex items-center justify-between mt-3">
                    <div class="flex flex-col">
                        ${hasStock && discount > 0 ? `<span class="text-[11px] sm:text-xs text-gray-400 line-through">${formatPrice(originalPrice)}</span>` : ""}
                        <span class="text-base sm:text-lg font-bold ${hasStock ? "text-primary" : "text-red-600"}">${hasStock ? `${formatPrice(price)} ریال` : "ناموجود"}</span>
                    </div>
                    <button onclick="addToCart('${product.id}')" class="bg-primary text-white p-1.5 sm:p-2 rounded-lg hover:bg-primary/90 transition ${hasStock ? "" : "opacity-60 cursor-not-allowed"}" aria-label="افزودن به سبد خرید" ${hasStock ? "" : "disabled"}>
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-4 sm:size-5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M2.25 3h1.386c.51 0 .955.343 1.087.835l.383 1.437M7.5 14.25a3 3 0 00-3 3h15.75m-12.75-3h11.218c1.121-2.3 2.1-4.684 2.924-7.138a60.114 60.114 0 00-16.536-1.84M7.5 14.25L5.106 5.272M6 20.25a.75.75 0 11-1.5 0 .75.75 0 011.5 0zm12.75 0a.75.75 0 11-1.5 0 .75.75 0 011.5 0z"/>
              </svg>
            </button>
          </div>
        </article>
      </div>
    `;
  }

  function initializeCarousel() {
    const carousel = document.querySelector(".product-carousel");
    if (!carousel || typeof window.Swiper === "undefined") return;

    window.swiperInstances = window.swiperInstances || {};

    if (carousel.swiper) {
      window.swiperInstances.featuredProducts = carousel.swiper;
      carousel.swiper.update();
      carousel.setAttribute("data-component-ready", "index-featured-products");
      return;
    }

    window.swiperInstances.featuredProducts = new Swiper(carousel, {
      slidesPerView: 5,
      spaceBetween: 10,
      navigation: {
        nextEl: carousel.querySelector(".swiper-button-next"),
        prevEl: carousel.querySelector(".swiper-button-prev"),
      },
      breakpoints: {
        100: { slidesPerView: 1 },
        576: { slidesPerView: 2 },
        768: { slidesPerView: 3 },
        1024: { slidesPerView: 4 },
        1400: { slidesPerView: 5 },
      },
    });
    carousel.setAttribute("data-component-ready", "index-featured-products");
  }

  function setWishlistButtonElementActive(button, isActive) {
    button.classList.toggle("text-red-500", isActive);
    button.classList.toggle("dark:text-white", !isActive);
    button.setAttribute(
      "aria-label",
      isActive ? "حذف از علاقه‌مندی‌ها" : "افزودن به علاقه‌مندی‌ها",
    );

    const icon = button.querySelector("svg");
    if (icon) {
      icon.setAttribute("fill", isActive ? "currentColor" : "none");
    }
  }

  async function syncWishlistButtons() {
    if (!window.authService?.isAuthenticated() || !window.wishlistService)
      return;

    try {
      const result = await window.wishlistService.getWishlistProductIds();
      if (!result.success || !Array.isArray(result.data)) return;

      const wishlistProductIds = new Set(result.data);
      document
        .querySelectorAll("[data-wishlist-product-id]")
        .forEach((button) => {
          const productId = String(
            button.getAttribute("data-wishlist-product-id") || "",
          ).toLowerCase();
          setWishlistButtonElementActive(
            button,
            wishlistProductIds.has(productId),
          );
        });
    } catch (error) {
      window.logger?.error("Error syncing wishlist buttons:", error);
    }
  }

  function renderProducts(products) {
    const container = findContainer();
    if (!container) return;

    const visibleProducts = (Array.isArray(products) ? products : []).filter(
      isVisibleProduct,
    );
    if (visibleProducts.length === 0) {
      renderState("محصولی برای نمایش وجود ندارد");
      return;
    }

    container.innerHTML = visibleProducts.map(createProductCard).join("");
    initializeCarousel();
    syncWishlistButtons();
  }

  function renderState(message) {
    const container = findContainer();
    if (!container) return;

    container.innerHTML = `
      <div class="swiper-slide px-1.5 py-2">
        <article class="bg-white product-box-item drop-shadow-md rounded-xl p-3 sm:p-4 dark:bg-gray-800 dark:border-white dark:border-1">
          <p class="text-center text-gray-500 dark:text-gray-300 py-12 sm:py-16">${message}</p>
        </article>
      </div>
    `;
    initializeCarousel();
  }

  function delay(ms) {
    return new Promise(function (resolve) {
      window.setTimeout(resolve, ms);
    });
  }

  function hasProductDependencies() {
    return !!(
      window.apiClient &&
      window.productService &&
      typeof window.productService.getNewProducts === "function"
    );
  }

  async function waitForProductDependencies() {
    for (let attempt = 0; attempt < DEPENDENCY_MAX_ATTEMPTS; attempt += 1) {
      if (hasProductDependencies()) return true;
      await delay(DEPENDENCY_RETRY_DELAY_MS);
    }

    return false;
  }

  async function loadFeaturedProducts() {
    const container = findContainer();
    if (!container) return;

    if (hasLoadedProducts) return;

    const dependenciesReady = await waitForProductDependencies();
    if (!dependenciesReady) {
      renderState("Ø³Ø±ÙˆÛŒØ³ Ù…Ø­ØµÙˆÙ„Ø§Øª Ø¯Ø± Ø¯Ø³ØªØ±Ø³ Ù†ÛŒØ³Øª");
      return;
    }

    hasLoadedProducts = true;

    if (!window.productService || !window.apiClient) {
      renderState("سرویس محصولات در دسترس نیست");
      return;
    }

    try {
      const result = await window.productService.getNewProducts(8);
      const products = extractProducts(result);
      if (products.length > 0) {
        renderProducts(products);
        return;
      }

      const fallback = await window.apiClient.get(
        "/Product/search?sortBy=CreatedAt&sortDescending=true&pageNumber=1&pageSize=8",
      );
      const fallbackProducts = extractProducts(fallback);
      if (fallbackProducts.length > 0) {
        renderProducts(fallbackProducts);
        return;
      }

      renderState("محصولی برای نمایش وجود ندارد");
    } catch (error) {
      if (window.logger && typeof window.logger.error === "function") {
        window.logger.error("Error loading featured products:", error);
      }
      renderState("خطا در دریافت محصولات");
    }
  }

  initializeCarousel();
  loadFeaturedProducts();
})();
