(function () {
  "use strict";

  const CONTAINER_ID = "featuredProducts";
  const MAHAK_CONTENT_BASE_URL = "https://mahakacc.mahaksoft.com";

  function isDevelopmentHost() {
    const hostname = window.location?.hostname?.toLowerCase() || "";
    return hostname === "localhost" || hostname === "127.0.0.1";
  }

  function proxiedImageUrl(url) {
    const apiBaseUrl = (window.config?.api?.baseURL || "/api").replace(/\/$/, "");
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
    else if (payload.data && Array.isArray(payload.data)) products = payload.data;

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
      primaryImage?.imageUrl || galleryImage?.imageUrl || product.imageUrl || "";

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
    const fallbackImageUrl = image.fallback || "assets/images/product/nophoto.png";
    const imageErrorHandler = image.fallback
      ? `this.onerror=function(){this.onerror=null; this.src='assets/images/product/nophoto.png'}; this.src='${fallbackImageUrl}'`
      : "this.onerror=null; this.src='assets/images/product/nophoto.png'";
    const price = product.price || 0;
    const originalPrice = product.originalPrice || price;
    const discount =
      originalPrice > price
        ? Math.round(((originalPrice - price) / originalPrice) * 100)
        : 0;
    const productUrl = "product.html?id=" + product.id;
    const name = product.name || "نام محصول";

    return `
      <div class="swiper-slide px-1.5 py-2">
        <article class="bg-white product-box-item drop-shadow-md rounded-xl p-4 dark:bg-gray-800 dark:border-white dark:border-1">
          <header class="flex items-center relative justify-between">
            ${discount > 0 ? `<span class="absolute top-1 end-1 bg-red-500 text-white text-xs px-2 py-1 rounded">${discount}%</span>` : ""}
            <div class="flex flex-col absolute top-1 start-0 p-1 rounded space-y-3">
              <button onclick="addToWishlist('${product.id}')" class="p-2 bg-white rounded-full shadow-md hover:bg-primary hover:text-white transition" aria-label="افزودن به علاقه‌مندی‌ها">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-5">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.22 9 12 9 12s9-4.78 9-12z"/>
                </svg>
              </button>
            </div>
          </header>
          <a href="${productUrl}">
            <figure class="relative overflow-hidden rounded-lg mb-3">
              <img src="${imageUrl}" alt="${name}" class="w-full h-48 object-contain" loading="lazy" decoding="async" onerror="${imageErrorHandler}">
            </figure>
          </a>
          <a href="${productUrl}">
            <h3 class="text-sm font-bold mb-2 line-clamp-2 dark:text-white">${name}</h3>
          </a>
          <div class="flex items-center justify-between mt-3">
            <div class="flex flex-col">
              ${discount > 0 ? `<span class="text-xs text-gray-400 line-through">${formatPrice(originalPrice)}</span>` : ""}
              <span class="text-lg font-bold text-primary">${formatPrice(price)} تومان</span>
            </div>
            <button onclick="addToCart('${product.id}')" class="bg-primary text-white p-2 rounded-lg hover:bg-primary/90 transition" aria-label="افزودن به سبد خرید">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-5">
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
  }

  function renderState(message) {
    const container = findContainer();
    if (!container) return;

    container.innerHTML = `
      <div class="swiper-slide px-1.5 py-2">
        <article class="bg-white product-box-item drop-shadow-md rounded-xl p-4 dark:bg-gray-800 dark:border-white dark:border-1">
          <p class="text-center text-gray-500 dark:text-gray-300 py-16">${message}</p>
        </article>
      </div>
    `;
    initializeCarousel();
  }

  async function loadFeaturedProducts() {
    const container = findContainer();
    if (!container) return;

    if (!window.productService || !window.apiClient) {
      renderState("سرویس محصولات در دسترس نیست");
      return;
    }

    try {
      const result = await window.productService.getFeaturedProducts(8);
      const products = extractProducts(result);
      if (products.length > 0) {
        renderProducts(products);
        return;
      }

      const fallback = await window.apiClient.get(
        "/Product/search?sortBy=Sales&sortDescending=true&pageNumber=1&pageSize=8",
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
