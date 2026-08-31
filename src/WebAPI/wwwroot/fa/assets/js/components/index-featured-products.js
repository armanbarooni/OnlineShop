(function () {
  "use strict";

  const CONTAINER_ID = "featuredProducts";
  const BEST_SELLING_CONTAINER_ID = "bestSellingProducts";
  const NEWEST_CONTAINER_ID = "newProducts";
  const MAX_VISIBLE_PRODUCTS = 5;
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

  function findContainer(containerId = CONTAINER_ID) {
    return (
      document.getElementById(containerId) ||
      document.querySelector(`[data-products="${containerId}"]`) ||
      document.querySelector(`.${containerId === BEST_SELLING_CONTAINER_ID ? "best-selling-carousel" : containerId === NEWEST_CONTAINER_ID ? "newest-products-carousel" : "product-carousel"} .swiper-wrapper`)
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
    const variants = getProductVariantList(product);
    if (variants.length > 0) {
      return variants.some((variant) => getVariantStock(variant) > 0);
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
    const prices = hasStock ? getProductDisplayPrices(product) : null;
    const discount =
      hasStock && prices?.hasDiscount ? prices.discountPercent : 0;
    const productUrl = "product.html?id=" + product.id;
    const name = product.name || "\u0645\u062d\u0635\u0648\u0644";
    const wishlistClick =
      "event.preventDefault(); event.stopPropagation(); addToWishlist('" +
      product.id +
      "')";

    return `
      <div class="swiper-slide index-featured-slide px-1.5 py-2">
        <article class="index-featured-card bg-white product-box-item rounded-lg p-3 sm:p-4 shadow-sm border border-gray-100 dark:bg-gray-900 dark:border-white/20">
          <header class="flex items-center relative justify-between">
          </header>
                <figure class="index-featured-figure relative overflow-hidden rounded-lg mb-2 p-1 bg-gray-50 dark:bg-gray-800" style="aspect-ratio: 4 / 5;">
            <a href="${productUrl}" class="block h-full">
              <img src="${imageUrl}" alt="${name}" class="index-featured-image w-full h-full object-contain rounded-md" loading="lazy" decoding="async" onerror="${imageErrorHandler}">
            </a>
            ${!hasStock ? `<span class="absolute top-2 end-2 bg-red-600 text-white text-xs font-bold px-3 py-1 rounded z-20 shadow-sm">\u0646\u0627\u0645\u0648\u062c\u0648\u062f</span>` : ""}
            <button type="button" data-wishlist-product-id="${product.id}" onclick="${wishlistClick}" class="absolute top-2 start-2 z-30 p-1.5 sm:p-2 bg-white rounded-full shadow-sm hover:bg-primary hover:text-white transition dark:bg-gray-800 dark:text-white" aria-label="\u0627\u0641\u0632\u0648\u062f\u0646 \u0628\u0647 \u0639\u0644\u0627\u0642\u0647\u200c\u0645\u0646\u062f\u06cc\u200c\u0647\u0627">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-4 sm:size-5 pointer-events-none">
                <path stroke-linecap="round" stroke-linejoin="round" d="M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.22 9 12 9 12s9-4.78 9-12z"/>
              </svg>
            </button>
          </figure>
                <div class="index-featured-meta product-price-panel flex items-center justify-between mt-2 border border-gray-200 dark:border-gray-700 rounded-xl p-3" style="height: 132px;">
                    <div class="flex flex-col">
                        <a href="${productUrl}" class="block">
                            <h3 class="index-featured-name text-sm sm:text-sm font-extrabold leading-6 mb-3 line-clamp-2 text-gray-900 dark:text-white">${name}</h3>
                        </a>
                        ${hasStock && prices?.hasDiscount ? `<div class="flex items-center gap-2"><span class="text-[11px] sm:text-xs text-gray-400 opacity-60 line-through">${formatPrice(prices.basePrice)}</span>${discount > 0 ? `<span class="bg-red-500 text-white text-xs px-2 py-1 rounded">${discount}%</span>` : ""}</div>` : ""}
                        <span class="index-featured-price text-base sm:text-lg font-extrabold ${hasStock ? "text-gray-900 dark:text-gray-100" : "text-red-600 dark:text-white"}">${hasStock && prices ? `${formatPrice(prices.finalPrice)} \u0631\u06cc\u0627\u0644` : "\u0646\u0627\u0645\u0648\u062c\u0648\u062f"}</span>
                    </div>
          </div>
        </article>
      </div>
    `;
  }

  function getProductDisplayPrices(product) {
    const basePrice = Number(
      product?.price1 ??
      product?.originalPrice ??
      product?.price ??
      product?.unitPrice ??
      0,
    ) || 0;
    const candidateFinalPrice = Number(
      product?.price2 ??
      product?.salePrice ??
      product?.discountPrice ??
      0,
    ) || 0;
    const finalPrice = candidateFinalPrice > 0 ? candidateFinalPrice : basePrice;
    const hasDiscount = basePrice > 0 && candidateFinalPrice > 0;
    const discountPercent = hasDiscount
      ? Math.round(((basePrice - finalPrice) / basePrice) * 100)
      : 0;

    return { basePrice, finalPrice, hasDiscount, discountPercent };
  }

  function initializeCarousel() {
    if (typeof window.Swiper === "undefined") return;

    window.swiperInstances = window.swiperInstances || {};
    document.querySelectorAll(".product-carousel").forEach((carousel) => {
      const containerId = carousel.querySelector("[data-products]")?.id || CONTAINER_ID;
      const instanceKey = containerId === BEST_SELLING_CONTAINER_ID ? BEST_SELLING_CONTAINER_ID : containerId === NEWEST_CONTAINER_ID ? NEWEST_CONTAINER_ID : CONTAINER_ID;

      if (carousel.swiper) {
        window.swiperInstances[instanceKey] = carousel.swiper;
        carousel.swiper.update();
        carousel.setAttribute("data-component-ready", "index-featured-products");
        return;
      }

      window.swiperInstances[instanceKey] = new Swiper(carousel, {
      slidesPerView: 5,
      spaceBetween: 10,
      centerInsufficientSlides: false,
      navigation: {
        nextEl: carousel.querySelector(".swiper-button-next"),
        prevEl: carousel.querySelector(".swiper-button-prev"),
      },
      breakpoints: {
        100: { slidesPerView: 2, spaceBetween: 4 },
        576: { slidesPerView: 2 },
        768: { slidesPerView: 3 },
        1024: { slidesPerView: 4 },
        1400: { slidesPerView: 5 },
      },
      });
      carousel.setAttribute("data-component-ready", "index-featured-products");
    });
  }

  function setWishlistButtonElementActive(button, isActive) {
    button.classList.toggle("text-red-500", isActive);
    button.classList.toggle("dark:text-white", !isActive);
    button.setAttribute(
      "aria-label",
      isActive ? "\u062d\u0630\u0641 \u0627\u0632 \u0639\u0644\u0627\u0642\u0647\u200c\u0645\u0646\u062f\u06cc\u200c\u0647\u0627" : "\u0627\u0641\u0632\u0648\u062f\u0646 \u0628\u0647 \u0639\u0644\u0627\u0642\u0647\u200c\u0645\u0646\u062f\u06cc\u200c\u0647\u0627",
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

  function renderProducts(products, containerId = CONTAINER_ID) {
    const container = findContainer(containerId);
    if (!container) return;

    const visibleProducts = (Array.isArray(products) ? products : [])
      .filter(isVisibleProduct)
      .filter(isProductInStock)
      .slice(0, MAX_VISIBLE_PRODUCTS);
    if (visibleProducts.length === 0) {
      renderState("\u0645\u062d\u0635\u0648\u0644\u06cc \u0628\u0631\u0627\u06cc \u0646\u0645\u0627\u06cc\u0634 \u0648\u062c\u0648\u062f \u0646\u062f\u0627\u0631\u062f", containerId);
      return;
    }

    container.innerHTML = visibleProducts.map(createProductCard).join("");
    initializeCarousel();
    syncWishlistButtons();
  }

  function renderState(message, containerId = CONTAINER_ID) {
    const container = findContainer(containerId);
    if (!container) return;

    container.innerHTML = `
      <div class="swiper-slide px-1.5 py-2">
        <article class="bg-white product-box-item rounded-lg p-3 sm:p-4 shadow-sm border border-gray-100 dark:bg-gray-900 dark:border-white/20">
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
      typeof window.productService.getNewProducts === "function" &&
      typeof window.productService.getSaleProducts === "function" &&
      typeof window.Swiper === "function"
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
      renderState("\u0633\u0631\u0648\u06cc\u0633 \u0645\u062d\u0635\u0648\u0644\u0627\u062a \u062f\u0631 \u062f\u0633\u062a\u0631\u0633 \u0646\u06cc\u0633\u062a", CONTAINER_ID);
      return;
    }

    hasLoadedProducts = true;

    if (!window.productService || !window.apiClient) {
      renderState("\u0633\u0631\u0648\u06cc\u0633 \u0645\u062d\u0635\u0648\u0644\u0627\u062a \u062f\u0631 \u062f\u0633\u062a\u0631\u0633 \u0646\u06cc\u0633\u062a", CONTAINER_ID);
      return;
    }

    try {
      const result = await window.productService.getSaleProducts(MAX_VISIBLE_PRODUCTS);
      const products = extractProducts(result)
        .filter((product) => getProductDisplayPrices(product).hasDiscount)
        .slice(0, MAX_VISIBLE_PRODUCTS);
      if (products.length > 0) {
        renderProducts(products, CONTAINER_ID);
        return;
      }
      renderState("\u0645\u062d\u0635\u0648\u0644 \u062a\u062e\u0641\u06cc\u0641 \u062f\u0627\u0631\u06cc \u0628\u0631\u0627\u06cc \u0646\u0645\u0627\u06cc\u0634 \u0648\u062c\u0648\u062f \u0646\u062f\u0627\u0631\u062f", CONTAINER_ID);
    } catch (error) {
      if (window.logger && typeof window.logger.error === "function") {
        window.logger.error("Error loading featured products:", error);
      }
      renderState("\u062e\u0637\u0627 \u062f\u0631 \u062f\u0631\u06cc\u0627\u0641\u062a \u0645\u062d\u0635\u0648\u0644\u0627\u062a", CONTAINER_ID);
    }
  }

  async function loadBestSellingProducts() {
    const container = findContainer(BEST_SELLING_CONTAINER_ID);
    if (!container) return;

    try {
      if (!(await waitForProductDependencies())) {
        renderState("\u0633\u0631\u0648\u06cc\u0633 \u0645\u062d\u0635\u0648\u0644\u0627\u062a \u062f\u0631 \u062f\u0633\u062a\u0631\u0633 \u0646\u06cc\u0633\u062a", BEST_SELLING_CONTAINER_ID);
        return;
      }
      const result = await window.productService.getBestSellingProducts(MAX_VISIBLE_PRODUCTS);
      let products = extractProducts(result).slice(0, MAX_VISIBLE_PRODUCTS);
      if (products.length === 0) {
        const fallback = await window.productService.getNewProducts(MAX_VISIBLE_PRODUCTS);
        products = extractProducts(fallback).slice(0, MAX_VISIBLE_PRODUCTS);
      }
      if (products.length > 0) {
        renderProducts(products, BEST_SELLING_CONTAINER_ID);
      } else {
        renderState("محصولی برای نمایش وجود ندارد", BEST_SELLING_CONTAINER_ID);
      }
    } catch (error) {
      window.logger?.error("Error loading best selling products:", error);
      renderState("محصولی برای نمایش وجود ندارد", BEST_SELLING_CONTAINER_ID);
    }
  }

  async function loadNewestProducts() {
    const container = findContainer(NEWEST_CONTAINER_ID);
    if (!container) return;

    try {
      if (!(await waitForProductDependencies())) {
        renderState("\u0633\u0631\u0648\u06cc\u0633 \u0645\u062d\u0635\u0648\u0644\u0627\u062a \u062f\u0631 \u062f\u0633\u062aر\u0633 \u0646\u06cc\u0633\u062a", NEWEST_CONTAINER_ID);
        return;
      }
      const result = await window.productService.getNewProducts(MAX_VISIBLE_PRODUCTS);
      const products = extractProducts(result).slice(0, MAX_VISIBLE_PRODUCTS);
      if (products.length > 0) {
        renderProducts(products, NEWEST_CONTAINER_ID);
      } else {
        renderState("محصولی برای نمایش وجود ندارد", NEWEST_CONTAINER_ID);
      }
    } catch (error) {
      window.logger?.error("Error loading newest products:", error);
      renderState("خطا در دریافت محصولات", NEWEST_CONTAINER_ID);
    }
  }

  initializeCarousel();
  loadFeaturedProducts();
  loadBestSellingProducts();
  loadNewestProducts();
})();
