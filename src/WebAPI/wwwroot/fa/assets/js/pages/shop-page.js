/**
 * Shop Page (shop.html) API Integration
 */
(function () {
  "use strict";

  const MAHAK_CONTENT_BASE_URL = "https://mahakacc.mahaksoft.com";
  const DEPENDENCY_RETRY_DELAY_MS = 100;
  const DEPENDENCY_MAX_ATTEMPTS = 50;
  const DEFAULT_PAGE_SIZE = 20;
  const NO_PHOTO = "assets/images/product/nophoto.png";

  let cachedCategories = [];
  let currentProductsById = new Map();
  let currentQuery = { categoryId: null, searchQuery: "" };

  function ready(callback) {
    if (document.readyState === "loading") {
      document.addEventListener("DOMContentLoaded", callback);
    } else {
      callback();
    }
  }

  function delay(ms) {
    return new Promise(function (resolve) {
      setTimeout(resolve, ms);
    });
  }

  async function waitForDependencies() {
    for (let attempt = 0; attempt < DEPENDENCY_MAX_ATTEMPTS; attempt += 1) {
      if (
        window.apiClient &&
        window.productService &&
        window.categoryService &&
        window.cartService &&
        window.wishlistService
      ) {
        return true;
      }

      await delay(DEPENDENCY_RETRY_DELAY_MS);
    }

    return false;
  }

  async function initializeShopPage() {
    const gridContainer = document.getElementById("shop-products-grid");
    bindProductGridEvents();
    showLoading(gridContainer);

    const dependenciesReady = await waitForDependencies();
    if (!dependenciesReady) {
      showError(
        "امکان بارگذاری فروشگاه وجود ندارد. فایل‌های مورد نیاز صفحه کامل لود نشده‌اند.",
        gridContainer,
      );
      return;
    }

    try {
      await loadCategories();
      await renderMegaMenu();

      const urlParams = new URLSearchParams(window.location.search);
      const categoryId = urlParams.get("category");
      const searchQuery = urlParams.get("search") || urlParams.get("q") || "";
      const resolvedCategoryId =
        categoryId || (await resolveCategoryIdFromSearch(searchQuery));

      currentQuery = { categoryId: resolvedCategoryId, searchQuery };

      await updateShopCategoryContext(resolvedCategoryId, searchQuery);
      await loadProducts(resolvedCategoryId, searchQuery);
    } catch (error) {
      logError("Error initializing shop page:", error);
      showError(
        "خطا در بارگذاری صفحه فروشگاه. لطفا چند لحظه دیگر دوباره تلاش کنید.",
        gridContainer,
      );
    }
  }

  async function renderMegaMenu() {
    const megaMenuContainer = document.getElementById(
      "mega-menu-list-container",
    );

    if (megaMenuContainer && window.categoryService) {
      await window.categoryService.renderMegaMenu("mega-menu-list-container");
    }
  }

  async function updateShopCategoryContext(categoryId, searchQuery) {
    const categoryNameElement = document.getElementById(
      "shop-current-category-name",
    );
    if (!categoryNameElement) return;

    if (categoryId && window.categoryService) {
      try {
        const categoryResult =
          await window.categoryService.getCategoryById(categoryId);
        const category = unwrapData(categoryResult);
        const categoryName = category?.name || category?.title;
        if (categoryName) {
          categoryNameElement.textContent = categoryName;
          return;
        }
      } catch (error) {
        logWarn("Failed to resolve category name for breadcrumb", error);
      }
    }

    if (searchQuery) {
      categoryNameElement.textContent = `نتایج جستجو: ${searchQuery}`;
      return;
    }

    categoryNameElement.textContent = "همه محصولات";
  }

  function normalizeCategoryName(value) {
    if (!value) return "";

    return String(value)
      .replace(/\u200c/g, " ")
      .replace(/ي/g, "ی")
      .replace(/ك/g, "ک")
      .replace(/\s+/g, " ")
      .trim()
      .toLowerCase();
  }

  async function resolveCategoryIdFromSearch(searchQuery) {
    if (!searchQuery || !window.categoryService) return null;

    const normalizedSearch = normalizeCategoryName(searchQuery);
    if (!normalizedSearch) return null;

    let categories = cachedCategories;

    if (!Array.isArray(categories) || categories.length === 0) {
      try {
        const result = await window.categoryService.getAllCategories();
        categories = extractArray(unwrapData(result));
        cachedCategories = categories;
      } catch (error) {
        logWarn("Failed to resolve category from search query", error);
        return null;
      }
    }

    const exactMatch = categories.find(function (category) {
      const categoryName = normalizeCategoryName(category.name || category.title);
      return categoryName === normalizedSearch;
    });

    return exactMatch ? exactMatch.id : null;
  }

  async function loadProducts(categoryId, searchQuery) {
    const gridContainer = document.getElementById("shop-products-grid");
    if (!gridContainer) return;

    showLoading(gridContainer);

    try {
      let result;

      if (categoryId) {
        result = await window.productService.getProductsByCategory(
          categoryId,
          1,
          DEFAULT_PAGE_SIZE,
        );
      } else if (searchQuery) {
        result = await window.productService.searchProducts({
          searchTerm: searchQuery,
          pageNumber: 1,
          pageSize: DEFAULT_PAGE_SIZE,
        });
      } else {
        result = await window.productService.searchProducts({
          pageNumber: 1,
          pageSize: DEFAULT_PAGE_SIZE,
        });
      }

      if (!isSuccessfulResult(result)) {
        showError(
          getResultError(
            result,
            "خطا در دریافت لیست محصولات. لطفا دوباره تلاش کنید.",
          ),
          gridContainer,
        );
        return;
      }

      const payload = unwrapData(result);
      if (payload?.isSuccess === false || payload?.success === false) {
        showError(
          getResultError(
            payload,
            "خطا در دریافت لیست محصولات. لطفا دوباره تلاش کنید.",
          ),
          gridContainer,
        );
        return;
      }

      renderAvailableFilters(payload);
      renderProducts(extractProducts(payload), gridContainer);
    } catch (error) {
      logError("Error loading products:", error);
      showError(
        "ارتباط با سرور برقرار نشد. لطفا اتصال اینترنت یا وضعیت سرور را بررسی کنید.",
        gridContainer,
      );
    }
  }

  function renderAvailableFilters(payload) {
    const data = unwrapData(payload);

    if (data?.availableColors) {
      renderColors(data.availableColors);
    }

    if (data?.availableSizes) {
      renderSizes(data.availableSizes);
    }

    if (data?.priceRanges) {
      renderPriceFilter(data.priceRanges);
      initPriceSlider();
    }
  }

  function renderProducts(products, container) {
    const visibleProducts = extractArray(products).filter(isVisibleProduct);
    currentProductsById = new Map(
      visibleProducts.map(function (product) {
        return [String(product.id), product];
      }),
    );

    if (visibleProducts.length === 0) {
      showEmpty(container);
      return;
    }

    container.innerHTML = visibleProducts.map(createProductCard).join("");
    syncWishlistButtons();
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

  function createProductCard(product) {
    const image = getProductImageData(product);
    const imageErrorHandler = image.fallback
      ? `this.onerror=function(){this.onerror=null; this.src='${NO_PHOTO}'}; this.src='${escapeAttribute(image.fallback)}'`
      : `this.onerror=null; this.src='${NO_PHOTO}'`;
    const id = product.id || product.productId || "";
    const name = product.name || product.productName || "محصول بدون نام";
    const productUrl = `product.html?id=${encodeURIComponent(id)}`;
    const price = getProductPrice(product);
    const originalPrice = getProductOriginalPrice(product, price);
    const discount =
      originalPrice > price
        ? Math.round(((originalPrice - price) / originalPrice) * 100)
        : 0;

    return `
      <div class="lg:col-span-4 md:col-span-6 col-span-12 w-full">
        <article class="bg-white product-box-item drop-shadow-md rounded-xl p-4 dark:bg-gray-800 dark:border-white dark:border-1 h-full flex flex-col" itemscope itemtype="http://schema.org/Product">
          <figure class="relative overflow-hidden rounded-lg mb-3">
            <a href="${productUrl}" class="block" itemprop="url">
              <img src="${escapeAttribute(image.src)}" alt="${escapeAttribute(name)}" class="w-full h-48 object-contain" loading="lazy" decoding="async" itemprop="image" onerror="${imageErrorHandler}">
            </a>
            ${
              discount > 0
                ? `<span class="absolute top-2 end-2 bg-red-500 text-white text-xs px-2 py-1 rounded z-20">${discount}%</span>`
                : ""
            }
            <button type="button" data-wishlist-product-id="${escapeAttribute(id)}" class="absolute top-2 start-2 z-30 p-2 bg-white rounded-full shadow-md hover:bg-primary hover:text-white transition dark:bg-gray-800 dark:text-white" aria-label="افزودن به علاقه‌مندی‌ها">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-5 pointer-events-none">
                <path stroke-linecap="round" stroke-linejoin="round" d="M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.22 9 12 9 12s9-4.78 9-12z"></path>
              </svg>
            </button>
          </figure>
          <a href="${productUrl}" class="block flex-1">
            <h3 class="text-sm font-bold mb-2 line-clamp-2 dark:text-white" itemprop="name">${escapeHtml(name)}</h3>
          </a>
          <div class="flex items-center justify-between mt-auto" itemprop="offers" itemscope itemtype="http://schema.org/Offer">
            <meta itemprop="priceCurrency" content="IRR">
            <div class="flex flex-col">
              ${
                discount > 0
                  ? `<span class="text-xs text-gray-400 line-through">${formatPrice(originalPrice)}</span>`
                  : ""
              }
              <span class="text-lg font-bold text-primary" itemprop="price" content="${price}">${formatPrice(price)} ریال</span>
            </div>
            <button type="button" data-add-to-cart-product-id="${escapeAttribute(id)}" class="bg-primary text-white p-2 rounded-lg hover:bg-primary/90 transition" aria-label="افزودن به سبد خرید">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-5 pointer-events-none">
                <path stroke-linecap="round" stroke-linejoin="round" d="M2.25 3h1.386c.51 0 .955.343 1.087.835l.383 1.437M7.5 14.25a3 3 0 0 0-3 3h15.75m-12.75-3h11.218c1.121-2.3 2.1-4.684 2.924-7.138a60.114 60.114 0 0 0-16.536-1.84M7.5 14.25 5.106 5.272M6 20.25a.75.75 0 1 1-1.5 0 .75.75 0 0 1 1.5 0Zm12.75 0a.75.75 0 1 1-1.5 0 .75.75 0 0 1 1.5 0Z"></path>
              </svg>
            </button>
          </div>
        </article>
      </div>
    `;
  }

  function bindProductGridEvents() {
    const gridContainer = document.getElementById("shop-products-grid");
    if (!gridContainer || gridContainer.dataset.shopEventsBound === "true") {
      return;
    }

    gridContainer.dataset.shopEventsBound = "true";
    gridContainer.addEventListener("click", function (event) {
      const wishlistButton = event.target.closest("[data-wishlist-product-id]");
      if (wishlistButton) {
        event.preventDefault();
        event.stopPropagation();
        toggleWishlist(wishlistButton.dataset.wishlistProductId);
        return;
      }

      const cartButton = event.target.closest("[data-add-to-cart-product-id]");
      if (cartButton) {
        event.preventDefault();
        event.stopPropagation();
        addToCart(cartButton.dataset.addToCartProductId);
      }
    });
  }

  function getProductPrice(product) {
    return (
      Number(
        product.salePrice ??
          product.price ??
          product.unitPrice ??
          product.finalPrice ??
          0,
      ) || 0
    );
  }

  function getProductOriginalPrice(product, price) {
    return (
      Number(
        product.originalPrice ??
          product.oldPrice ??
          product.basePrice ??
          product.unitPrice ??
          product.price ??
          price,
      ) || price
    );
  }

  function getProductImageData(product) {
    const primaryImage = Array.isArray(product.images)
      ? product.images.find(function (image) {
          return image && image.isPrimary;
        }) || product.images[0]
      : null;
    const galleryImage =
      Array.isArray(product.productImages) && product.productImages.length > 0
        ? product.productImages.find(function (image) {
            return image && image.isPrimary;
          }) || product.productImages[0]
        : null;
    const rawImageUrl =
      primaryImage?.imageUrl ||
      galleryImage?.imageUrl ||
      product.productImageUrl ||
      product.productImage ||
      product.imageUrl ||
      "";

    if (!rawImageUrl || !String(rawImageUrl).trim()) {
      return { src: NO_PHOTO, fallback: "" };
    }

    const normalizedImageUrl = String(rawImageUrl).trim();
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

    if (
      normalizedImageUrl.startsWith("/api/v3/Content/Images/") ||
      normalizedImageUrl.startsWith("api/v3/Content/Images/")
    ) {
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

  async function addToCart(productId) {
    if (!window.cartService) {
      showToast("سرویس سبد خرید آماده نیست. لطفا صفحه را رفرش کنید.", "error");
      return;
    }

    try {
      const product = currentProductsById.get(String(productId)) || null;
      const result = await window.cartService.addToCart(
        productId,
        1,
        null,
        null,
        product,
      );

      if (result?.success) {
        showToast(
          result.alreadyAtMaxStock
            ? "این محصول قبلا به سبد خرید اضافه شده و موجودی بیشتری ندارد."
            : "محصول با موفقیت به سبد خرید اضافه شد.",
          "success",
        );
        window.dispatchEvent(new CustomEvent("cart:updated"));
      } else {
        showToast(
          getResultError(result, "خطا در افزودن محصول به سبد خرید."),
          "error",
        );
      }
    } catch (error) {
      logError("Error adding to cart:", error);
      showToast("خطا در ارتباط با سرور.", "error");
    }
  }

  function markWishlistButtonActive(productId) {
    setWishlistButtonActive(productId, true);
  }

  function markWishlistButtonInactive(productId) {
    setWishlistButtonActive(productId, false);
  }

  function setWishlistButtonActive(productId, isActive) {
    document
      .querySelectorAll(`[data-wishlist-product-id="${cssEscape(productId)}"]`)
      .forEach(function (button) {
        setWishlistButtonElementActive(button, isActive);
      });
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

  function isWishlistButtonActive(productId) {
    const button = document.querySelector(
      `[data-wishlist-product-id="${cssEscape(productId)}"]`,
    );
    return !!button?.classList.contains("text-red-500");
  }

  async function syncWishlistButtons() {
    if (!window.authService?.isAuthenticated() || !window.wishlistService) {
      return;
    }

    try {
      const result = await window.wishlistService.getWishlistProductIds();
      if (!result.success || !Array.isArray(result.data)) return;

      const wishlistProductIds = new Set(result.data);
      document
        .querySelectorAll("[data-wishlist-product-id]")
        .forEach(function (button) {
          const productId = String(
            button.getAttribute("data-wishlist-product-id") || "",
          ).toLowerCase();
          setWishlistButtonElementActive(
            button,
            wishlistProductIds.has(productId),
          );
        });
    } catch (error) {
      logError("Error syncing wishlist buttons:", error);
    }
  }

  async function toggleWishlist(productId) {
    if (!window.authService || !window.authService.isAuthenticated()) {
      const returnUrl =
        window.location.pathname.split("/").pop() +
        window.location.search +
        window.location.hash;
      localStorage.setItem("intendedUrl", returnUrl || "index.html");
      window.location.href =
        "login.html?returnUrl=" + encodeURIComponent(returnUrl || "index.html");
      return;
    }

    if (!window.wishlistService) {
      showToast("سرویس علاقه‌مندی‌ها آماده نیست. لطفا صفحه را رفرش کنید.", "error");
      return;
    }

    try {
      const isActive = isWishlistButtonActive(productId);
      const result = isActive
        ? await window.wishlistService.removeProductFromWishlist(productId)
        : await window.wishlistService.addToWishlist(productId);

      if (result.success) {
        if (isActive) {
          markWishlistButtonInactive(productId);
          showToast("محصول از علاقه‌مندی‌ها حذف شد.", "success");
        } else {
          markWishlistButtonActive(productId);
          showToast("محصول به علاقه‌مندی‌ها اضافه شد.", "success");
        }
        return;
      }

      const errorMessage = String(result.error || "").toLowerCase();
      if (!isActive && errorMessage.includes("already in wishlist")) {
        markWishlistButtonActive(productId);
        showToast("این محصول قبلا در علاقه‌مندی‌ها قرار گرفته است.", "success");
        return;
      }

      showToast(
        getResultError(
          result,
          isActive
            ? "خطا در حذف محصول از علاقه‌مندی‌ها."
            : "خطا در افزودن محصول به علاقه‌مندی‌ها.",
        ),
        "error",
      );
    } catch (error) {
      logError("Error updating wishlist:", error);
      showToast("خطا در ارتباط با سرور.", "error");
    }
  }

  async function loadCategories() {
    try {
      if (!window.categoryService) return;
      const result = await window.categoryService.getAllCategories();
      if (isSuccessfulResult(result)) {
        cachedCategories = extractArray(unwrapData(result));
        renderCategories(cachedCategories);
      }
    } catch (error) {
      logError("Error loading categories:", error);
    }
  }

  function renderCategories(categories) {
    const categoryContainer = document.querySelector("[data-categories]");
    if (!categoryContainer) return;

    const limitedCategories = extractArray(categories).slice(0, 8);
    if (limitedCategories.length === 0) return;

    categoryContainer.innerHTML = limitedCategories
      .map(function (category) {
        const name = category.name || category.title || "دسته‌بندی";
        return `
          <a href="shop.html?category=${encodeURIComponent(category.id)}" class="lg:col-span-3 sm:col-span-6 col-span-12 w-full block">
            <article class="flex py-2 px-3 rounded-xl border border-gray-200 bg-white drop-shadow-md items-center justify-between dark:bg-gray-800">
              <section class="space-y-2">
                <h3 class="text-lg font-bold dark:text-white">${escapeHtml(name)}</h3>
                <span class="text-xs font-light text-neutral-500">${escapeHtml(category.description || "")}</span>
              </section>
              <figure>
                <img src="${escapeAttribute(category.imageUrl || "assets/images/category/digitall.png")}" class="size-20" loading="lazy" alt="${escapeAttribute(name)}">
              </figure>
            </article>
          </a>
        `;
      })
      .join("");
  }

  function renderColors(colors) {
    const container = document.getElementById("product-colors-container");
    const mobileColorSelect = document.getElementById("mobile-color-select");
    if (!container) return;

    const colorList = extractArray(colors);
    if (colorList.length === 0) {
      container.innerHTML =
        '<p class="text-sm text-gray-400">رنگی برای فیلتر محصولات موجود نیست.</p>';
      if (mobileColorSelect) {
        mobileColorSelect.innerHTML = '<option value="">همه رنگ‌ها</option>';
      }
      return;
    }

    container.innerHTML = colorList
      .map(function (color, index) {
        const id = `product-color-${color.id || index}`;
        const name = color.name || color.title || color;
        return `
          <div class="flex items-center">
            <input type="radio" name="productColor" id="${escapeAttribute(id)}" value="${escapeAttribute(name)}" class="hidden peer">
            <label for="${escapeAttribute(id)}" class="select-none dark:!text-white cursor-pointer flex items-center justify-center rounded-full border-2 border-gray-200 py-1 px-3 text-gray-700 transition-colors duration-200 ease-in-out peer-checked:text-gray-900 peer-checked:border-primary-500">
              <span class="dir-ltr">${escapeHtml(name)}</span>
            </label>
          </div>
        `;
      })
      .join("");

    if (mobileColorSelect) {
      mobileColorSelect.innerHTML =
        '<option value="">همه رنگ‌ها</option>' +
        colorList
          .map(function (color, index) {
            const name = color.name || color.title || color;
            const value = color.id || index;
            return `<option value="${escapeAttribute(value)}">${escapeHtml(name)}</option>`;
          })
          .join("");
    }
  }

  function renderSizes(sizes) {
    const container = document.getElementById("product-sizes-container");
    const mobileSizeSelect = document.getElementById("mobile-size-select");
    if (!container) return;

    const sizeList = extractArray(sizes);
    if (sizeList.length === 0) {
      container.innerHTML =
        '<p class="text-sm text-gray-400">سایزی برای فیلتر محصولات موجود نیست.</p>';
      if (mobileSizeSelect) {
        mobileSizeSelect.innerHTML = '<option value="">همه سایزها</option>';
      }
      return;
    }

    container.innerHTML = sizeList
      .map(function (size, index) {
        const id = `product-size-${size.id || index}`;
        const title = size.name || size.title || size;
        return `
          <div class="relative space-x-2 flex-wrap flex items-center">
            <label class="inline-flex items-center space-x-3 cursor-pointer">
              <input type="checkbox" id="${escapeAttribute(id)}" name="productSizes" value="${escapeAttribute(title)}" class="hidden peer">
              <div class="w-5 h-5 border rounded bg-white border-gray-400 peer-checked:bg-blue-600 peer-checked:border-blue-600 flex items-center justify-center transition-all shadow-sm">
                <svg class="w-4 h-4 text-white hidden peer-checked:block" fill="currentColor" viewBox="0 0 16 16" xmlns="http://www.w3.org/2000/svg">
                  <path fill-rule="evenodd" d="M10.97 4.97a.75.75 0 0 1 1.07 1.05l-4 4.5a.75.75 0 0 1-1.08.02l-2-2a.75.75 0 0 1 1.08-1.04l1.47 1.47 3.46-3.98z"></path>
                </svg>
              </div>
              <span class="me-2 text-gray-700 dark:text-white">${escapeHtml(title)}</span>
            </label>
          </div>
        `;
      })
      .join("");

    if (mobileSizeSelect) {
      mobileSizeSelect.innerHTML =
        '<option value="">همه سایزها</option>' +
        sizeList
          .map(function (size, index) {
            const title = size.name || size.title || size;
            const value = size.id || index;
            return `<option value="${escapeAttribute(value)}">${escapeHtml(title)}</option>`;
          })
          .join("");
    }
  }

  function renderPriceFilter(priceRanges) {
    const container = document.getElementById("price-filter-container");
    if (!container) return;

    const ranges = extractArray(priceRanges);
    if (ranges.length === 0) {
      container.innerHTML =
        '<p class="text-sm text-gray-400">فیلتر قیمتی موجود نیست.</p>';
      return;
    }

    const minPrice = Math.min(...ranges.map((range) => Number(range.minPrice)));
    const maxPrice = Math.max(...ranges.map((range) => Number(range.maxPrice)));

    if (!Number.isFinite(minPrice) || !Number.isFinite(maxPrice)) return;

    container.innerHTML = `
      <div class="p-4 rounded-lg space-y-4 mx-auto">
        <div class="flex items-baseline gap-4">
          <div class="flex-1">
            <input type="text" id="min-price-input" value="${formatPrice(minPrice)}" class="min-input w-full px-3 py-2 border border-gray-300 rounded-md bg-gray-100 dark:bg-zinc-900 text-center" disabled>
            <strong class="block text-center mt-3">ریال</strong>
          </div>
          <span class="text-gray-500 block">تا</span>
          <div class="flex-1">
            <input type="text" id="max-price-input" value="${formatPrice(maxPrice)}" class="max-input w-full px-3 py-2 border border-gray-300 rounded-md bg-gray-100 dark:bg-zinc-900 text-center" disabled>
            <strong class="block text-center mt-3">ریال</strong>
          </div>
        </div>
        <div class="slider-container" data-min="${minPrice}" data-max="${maxPrice}">
          <div class="slider-track"></div>
          <div class="slider-range"></div>
          <div class="slider-thumb min-thumb"></div>
          <div class="slider-thumb max-thumb"></div>
        </div>
      </div>
    `;
  }

  function initPriceSlider() {
    const slider = document.querySelector(".slider-container");
    if (!slider) return;

    const minThumb = slider.querySelector(".min-thumb");
    const maxThumb = slider.querySelector(".max-thumb");
    const range = slider.querySelector(".slider-range");
    const minInput = document.querySelector(".min-input");
    const maxInput = document.querySelector(".max-input");

    if (!minThumb || !maxThumb || !range || !minInput || !maxInput) return;

    const min = Number(slider.dataset.min);
    const max = Number(slider.dataset.max);
    if (Number.isNaN(min) || Number.isNaN(max) || min === max) return;

    let minVal = min;
    let maxVal = max;

    function percent(value) {
      return ((value - min) / (max - min)) * 100;
    }

    function updateUI() {
      const minPercent = percent(minVal);
      const maxPercent = percent(maxVal);

      minThumb.style.left = minPercent + "%";
      maxThumb.style.left = maxPercent + "%";
      range.style.left = minPercent + "%";
      range.style.right = 100 - maxPercent + "%";
      minInput.value = formatPrice(Math.round(minVal));
      maxInput.value = formatPrice(Math.round(maxVal));
    }

    function startDrag(isMin) {
      function onMove(event) {
        const rect = slider.getBoundingClientRect();
        const x = Math.min(Math.max(event.clientX - rect.left, 0), rect.width);
        const value = min + (x / rect.width) * (max - min);

        if (isMin) {
          minVal = Math.min(Math.max(min, value), maxVal);
        } else {
          maxVal = Math.max(Math.min(max, value), minVal);
        }

        updateUI();
      }

      function onUp() {
        document.removeEventListener("mousemove", onMove);
        document.removeEventListener("mouseup", onUp);
      }

      document.addEventListener("mousemove", onMove);
      document.addEventListener("mouseup", onUp);
    }

    minThumb.addEventListener("mousedown", function () {
      startDrag(true);
    });
    maxThumb.addEventListener("mousedown", function () {
      startDrag(false);
    });

    updateUI();
  }

  function showLoading(container) {
    if (!container) return;

    container.innerHTML = `
      <div class="lg:col-span-4 md:col-span-6 col-span-12 w-full">
        <div class="rounded-xl bg-white dark:bg-gray-800 p-4 shadow-sm animate-pulse">
          <div class="h-48 rounded-lg bg-gray-200 dark:bg-gray-700 mb-4"></div>
          <div class="h-4 rounded bg-gray-200 dark:bg-gray-700 mb-3"></div>
          <div class="h-4 w-2/3 rounded bg-gray-200 dark:bg-gray-700 mb-6"></div>
          <div class="flex items-center justify-between">
            <div class="h-6 w-28 rounded bg-gray-200 dark:bg-gray-700"></div>
            <div class="h-10 w-10 rounded-lg bg-gray-200 dark:bg-gray-700"></div>
          </div>
        </div>
      </div>
      <div class="lg:col-span-4 md:col-span-6 col-span-12 w-full hidden md:block">
        <div class="rounded-xl bg-white dark:bg-gray-800 p-4 shadow-sm animate-pulse">
          <div class="h-48 rounded-lg bg-gray-200 dark:bg-gray-700 mb-4"></div>
          <div class="h-4 rounded bg-gray-200 dark:bg-gray-700 mb-3"></div>
          <div class="h-4 w-2/3 rounded bg-gray-200 dark:bg-gray-700 mb-6"></div>
          <div class="flex items-center justify-between">
            <div class="h-6 w-28 rounded bg-gray-200 dark:bg-gray-700"></div>
            <div class="h-10 w-10 rounded-lg bg-gray-200 dark:bg-gray-700"></div>
          </div>
        </div>
      </div>
      <div class="lg:col-span-4 md:col-span-6 col-span-12 w-full hidden lg:block">
        <div class="rounded-xl bg-white dark:bg-gray-800 p-4 shadow-sm animate-pulse">
          <div class="h-48 rounded-lg bg-gray-200 dark:bg-gray-700 mb-4"></div>
          <div class="h-4 rounded bg-gray-200 dark:bg-gray-700 mb-3"></div>
          <div class="h-4 w-2/3 rounded bg-gray-200 dark:bg-gray-700 mb-6"></div>
          <div class="flex items-center justify-between">
            <div class="h-6 w-28 rounded bg-gray-200 dark:bg-gray-700"></div>
            <div class="h-10 w-10 rounded-lg bg-gray-200 dark:bg-gray-700"></div>
          </div>
        </div>
      </div>
    `;
  }

  function showEmpty(container) {
    container.innerHTML = `
      <div class="col-span-full w-full rounded-xl border border-gray-200 bg-white p-8 text-center dark:bg-gray-800 dark:border-gray-700">
        <h2 class="text-lg font-bold text-gray-800 dark:text-white">محصولی برای نمایش پیدا نشد.</h2>
        <p class="mt-2 text-sm text-gray-500 dark:text-gray-300">فیلترها یا عبارت جستجو را تغییر دهید و دوباره تلاش کنید.</p>
      </div>
    `;
  }

  function showError(message, container) {
    if (!container) return;

    container.innerHTML = `
      <div class="col-span-full w-full rounded-xl border border-red-200 bg-red-50 p-8 text-center dark:bg-red-950/30 dark:border-red-900" role="alert">
        <h2 class="text-lg font-bold text-red-700 dark:text-red-300">لیست محصولات بارگذاری نشد.</h2>
        <p class="mt-2 text-sm text-red-600 dark:text-red-200">${escapeHtml(message)}</p>
        <button type="button" data-shop-retry class="mt-5 rounded-lg bg-primary px-5 py-2 text-white hover:bg-primary/90">تلاش دوباره</button>
      </div>
    `;

    container.querySelector("[data-shop-retry]")?.addEventListener("click", function () {
      loadProducts(currentQuery.categoryId, currentQuery.searchQuery);
    });
  }

  function extractProducts(data) {
    const payload = unwrapData(data);
    if (!payload) return [];

    if (Array.isArray(payload)) return payload;
    if (Array.isArray(payload.products)) return payload.products;
    if (Array.isArray(payload.products?.items)) return payload.products.items;
    if (Array.isArray(payload.items)) return payload.items;
    if (Array.isArray(payload.data)) return payload.data;
    if (Array.isArray(payload.data?.items)) return payload.data.items;

    return [];
  }

  function extractArray(data) {
    const payload = unwrapData(data);
    if (Array.isArray(payload)) return payload;
    if (Array.isArray(payload?.items)) return payload.items;
    if (Array.isArray(payload?.data)) return payload.data;
    return [];
  }

  function unwrapData(result) {
    let payload = result;
    if (payload?.data !== undefined) payload = payload.data;
    if (
      payload &&
      payload.data !== undefined &&
      (payload.isSuccess === true || payload.success === true)
    ) {
      payload = payload.data;
    }
    return payload;
  }

  function isSuccessfulResult(result) {
    return !!result && result?.success !== false && result?.isSuccess !== false;
  }

  function getResultError(result, fallback) {
    return (
      result?.error ||
      result?.errorMessage ||
      result?.message ||
      result?.title ||
      result?.data?.errorMessage ||
      result?.data?.message ||
      result?.data?.title ||
      fallback
    );
  }

  function formatPrice(price) {
    return new Intl.NumberFormat("fa-IR").format(Math.round(Number(price) || 0));
  }

  function escapeHtml(value) {
    return String(value ?? "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function escapeAttribute(value) {
    return escapeHtml(value);
  }

  function cssEscape(value) {
    if (window.CSS?.escape) return window.CSS.escape(String(value));
    return String(value).replace(/"/g, '\\"');
  }

  function showToast(message, type) {
    if (window.utils?.showToast) {
      window.utils.showToast(message, type);
      return;
    }

    if (type === "error") {
      console.error(message);
    } else {
      console.log(message);
    }
  }

  function logError(message, error) {
    if (window.logger?.error) {
      window.logger.error(message, error);
    } else {
      console.error(message, error);
    }
  }

  function logWarn(message, error) {
    if (window.logger?.warn) {
      window.logger.warn(message, error);
    } else {
      console.warn(message, error);
    }
  }

  window.addToCart = addToCart;
  window.addToWishlist = toggleWishlist;
  window.retryShopProducts = function () {
    return loadProducts(currentQuery.categoryId, currentQuery.searchQuery);
  };

  ready(initializeShopPage);
})();
