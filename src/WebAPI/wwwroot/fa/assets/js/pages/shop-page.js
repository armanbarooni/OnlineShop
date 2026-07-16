/**
 * Shop Page (shop.html) API Integration
 */
(function () {
  "use strict";

  const MAHAK_CONTENT_BASE_URL =
    window.config?.content?.mahakBaseURL ||
    window.resolveMahakContentBaseURL?.() ||
    "https://mahakacc.mahaksoft.com";
  const DEPENDENCY_RETRY_DELAY_MS = 100;
  const DEPENDENCY_MAX_ATTEMPTS = 50;
  const DEFAULT_PAGE_SIZE = 20;
  const FILTER_SEARCH_DEBOUNCE_MS = 450;
  const NO_PHOTO = "assets/images/product/nophoto.png";

  let cachedCategories = [];
  let currentProductsById = new Map();
  let currentQuery = { categoryId: null, searchQuery: "" };
  let currentFilters = {
    searchTerm: "",
    colors: [],
    sizes: [],
    minPrice: null,
    maxPrice: null,
    inStockOnly: false,
  };
  let filterSearchTimer = null;
  let productRequestSequence = 0;

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
    bindShopFilterEvents();
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
      const categoryResolvedFromSearch = !categoryId && !!resolvedCategoryId;
      const productSearchQuery = categoryResolvedFromSearch ? "" : searchQuery;

      currentQuery = { categoryId: resolvedCategoryId, searchQuery: productSearchQuery };
      currentFilters.searchTerm = productSearchQuery;
      syncSearchInput(productSearchQuery);

      await updateShopCategoryContext(resolvedCategoryId, productSearchQuery);
      await loadProducts(resolvedCategoryId, productSearchQuery);
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

    const requestSequence = (productRequestSequence += 1);
    showLoading(gridContainer);

    try {
      const searchCriteria = buildProductSearchCriteria(categoryId, searchQuery);
      const result =
        typeof window.productService.searchProductsPost === "function"
          ? await window.productService.searchProductsPost(searchCriteria)
          : await window.productService.searchProducts(searchCriteria);

      if (requestSequence !== productRequestSequence) {
        return;
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
      restoreFilterControls();
      renderProducts(extractProducts(payload), gridContainer);
    } catch (error) {
      if (requestSequence !== productRequestSequence) {
        return;
      }
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
    const hasStock = isProductInStock(product);
    const price = hasStock ? getProductPrice(product) : null;
    const originalPrice = hasStock ? getProductOriginalPrice(product, price) : null;
    const discount =
      hasStock && originalPrice > price
        ? Math.round(((originalPrice - price) / originalPrice) * 100)
        : 0;

    return `
      <div class="lg:col-span-4 md:col-span-6 col-span-12 w-full">
        <article class="bg-white product-box-item drop-shadow-md rounded-xl p-3 sm:p-4 dark:bg-gray-800 dark:border-white dark:border-1 h-full flex flex-col" itemscope itemtype="http://schema.org/Product">
          <figure class="relative overflow-hidden rounded-lg mb-3">
            <a href="${productUrl}" class="block" itemprop="url">
              <img src="${escapeAttribute(image.src)}" alt="${escapeAttribute(name)}" class="w-full h-40 sm:h-48 object-contain" loading="lazy" decoding="async" itemprop="image" onerror="${imageErrorHandler}">
            </a>
            ${
              discount > 0
                ? `<span class="absolute top-2 end-2 bg-red-500 text-white text-xs px-2 py-1 rounded z-20">${discount}%</span>`
                : ""
            }
            <button type="button" data-wishlist-product-id="${escapeAttribute(id)}" class="absolute top-2 start-2 z-30 p-1.5 sm:p-2 bg-white rounded-full shadow-md hover:bg-primary hover:text-white transition dark:bg-gray-800 dark:text-white" aria-label="افزودن به علاقه‌مندی‌ها">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-4 sm:size-5 pointer-events-none">
                <path stroke-linecap="round" stroke-linejoin="round" d="M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.22 9 12 9 12s9-4.78 9-12z"></path>
              </svg>
            </button>
          </figure>
          <a href="${productUrl}" class="block flex-1">
            <h3 class="text-xs sm:text-sm font-bold mb-2 line-clamp-2 dark:text-white" itemprop="name">${escapeHtml(name)}</h3>
          </a>
          <div class="flex items-center justify-between mt-auto" itemprop="offers" itemscope itemtype="http://schema.org/Offer">
            <meta itemprop="priceCurrency" content="IRR">
            <div class="flex flex-col">
              ${hasStock && discount > 0 ? `<span class="text-[11px] sm:text-xs text-gray-400 line-through">${formatPrice(originalPrice)}</span>` : ""}
              <span class="text-base sm:text-lg font-bold ${hasStock ? "text-primary" : "text-red-600"}" itemprop="price"${hasStock ? ` content="${price}"` : ""}>${hasStock ? `${formatPrice(price)} ریال` : "ناموجود"}</span>
            </div>
            <button type="button" data-add-to-cart-product-id="${escapeAttribute(id)}" class="bg-primary text-white p-1.5 sm:p-2 rounded-lg hover:bg-primary/90 transition ${hasStock ? "" : "opacity-60 cursor-not-allowed"}" aria-label="افزودن به سبد خرید" ${hasStock ? "" : "disabled"}>
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-4 sm:size-5 pointer-events-none">
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

  function bindShopFilterEvents() {
    const searchInput = document.getElementById("shop-filter-search-input");
    const searchButton = document.getElementById("shop-filter-search-button");
    const colorContainer = document.getElementById("product-colors-container");
    const sizeContainer = document.getElementById("product-sizes-container");
    const mobileColorSelect = document.getElementById("mobile-color-select");
    const mobileSizeSelect = document.getElementById("mobile-size-select");
    const inStockCheckbox = document.getElementById("shop-in-stock-checkbox");

    if (searchInput && searchInput.dataset.shopFilterEventsBound !== "true") {
      searchInput.dataset.shopFilterEventsBound = "true";
      searchInput.addEventListener("input", function () {
        currentFilters.searchTerm = normalizeFilterText(searchInput.value);
        clearTimeout(filterSearchTimer);
        filterSearchTimer = setTimeout(function () {
          applyFilters();
        }, FILTER_SEARCH_DEBOUNCE_MS);
      });
      searchInput.addEventListener("keydown", function (event) {
        if (event.key !== "Enter") return;
        event.preventDefault();
        clearTimeout(filterSearchTimer);
        currentFilters.searchTerm = normalizeFilterText(searchInput.value);
        applyFilters();
      });
    }

    if (searchButton && searchButton.dataset.shopFilterEventsBound !== "true") {
      searchButton.dataset.shopFilterEventsBound = "true";
      searchButton.addEventListener("click", function () {
        clearTimeout(filterSearchTimer);
        currentFilters.searchTerm = normalizeFilterText(searchInput?.value);
        applyFilters();
      });
    }

    if (colorContainer && colorContainer.dataset.shopFilterEventsBound !== "true") {
      colorContainer.dataset.shopFilterEventsBound = "true";
      colorContainer.addEventListener("change", function (event) {
        const input = event.target.closest('input[name="productColor"]');
        if (!input) return;

        currentFilters.colors = input.value
          ? [normalizeFilterText(input.value)]
          : [];
        syncMobileColorSelect();
        applyFilters();
      });
    }

    if (sizeContainer && sizeContainer.dataset.shopFilterEventsBound !== "true") {
      sizeContainer.dataset.shopFilterEventsBound = "true";
      sizeContainer.addEventListener("change", function (event) {
        const input = event.target.closest('input[name="productSizes"]');
        if (!input) return;

        currentFilters.sizes = getSelectedDesktopSizes();
        syncMobileSizeSelect();
        applyFilters();
      });
    }

    if (
      mobileColorSelect &&
      mobileColorSelect.dataset.shopFilterEventsBound !== "true"
    ) {
      mobileColorSelect.dataset.shopFilterEventsBound = "true";
      mobileColorSelect.addEventListener("change", function () {
        currentFilters.colors = mobileColorSelect.value
          ? [normalizeFilterText(mobileColorSelect.value)]
          : [];
        syncDesktopColor(currentFilters.colors[0] || "");
        applyFilters();
      });
    }

    if (
      mobileSizeSelect &&
      mobileSizeSelect.dataset.shopFilterEventsBound !== "true"
    ) {
      mobileSizeSelect.dataset.shopFilterEventsBound = "true";
      mobileSizeSelect.addEventListener("change", function () {
        currentFilters.sizes = mobileSizeSelect.value
          ? [normalizeFilterText(mobileSizeSelect.value)]
          : [];
        syncDesktopSizes(currentFilters.sizes);
        applyFilters();
      });
    }

    if (
      inStockCheckbox &&
      inStockCheckbox.dataset.shopFilterEventsBound !== "true"
    ) {
      inStockCheckbox.dataset.shopFilterEventsBound = "true";
      inStockCheckbox.addEventListener("change", function () {
        currentFilters.inStockOnly = inStockCheckbox.checked;
        applyFilters();
      });
    }
  }

  function buildProductSearchCriteria(categoryId, searchQuery) {
    const criteria = {
      pageNumber: 1,
      pageSize: DEFAULT_PAGE_SIZE,
      sortBy: "CreatedAt",
      sortDescending: true,
    };
    const searchTerm = normalizeFilterText(
      currentFilters.searchTerm || searchQuery,
    );
    const minPrice = parseFilterNumber(currentFilters.minPrice);
    const maxPrice = parseFilterNumber(currentFilters.maxPrice);

    if (searchTerm) criteria.searchTerm = searchTerm;
    if (categoryId) criteria.categoryId = categoryId;
    if (currentFilters.colors.length > 0) {
      criteria.colors = currentFilters.colors;
    }
    if (currentFilters.sizes.length > 0) {
      criteria.sizes = currentFilters.sizes;
    }
    if (minPrice !== null) criteria.minPrice = minPrice;
    if (maxPrice !== null) criteria.maxPrice = maxPrice;
    if (currentFilters.inStockOnly) criteria.inStock = true;

    return criteria;
  }

  function normalizeFilterText(value) {
    return String(value ?? "").replace(/\s+/g, " ").trim();
  }

  function slugifyFilterValue(value) {
    return normalizeFilterText(value)
      .replace(/[^\w\u0600-\u06FF-]+/g, "-")
      .replace(/-+/g, "-")
      .replace(/^-|-$/g, "");
  }

  function getFacetLabel(value) {
    return normalizeFilterText(value?.name || value?.title || value);
  }

  function applyFilters() {
    return loadProducts(currentQuery.categoryId, currentQuery.searchQuery);
  }

  function syncSearchInput(value) {
    const searchInput = document.getElementById("shop-filter-search-input");
    if (searchInput) {
      searchInput.value = value || "";
    }
  }

  function restoreFilterControls() {
    syncSearchInput(currentFilters.searchTerm);
    syncDesktopColor(currentFilters.colors[0] || "");
    syncDesktopSizes(currentFilters.sizes);
    syncMobileColorSelect();
    syncMobileSizeSelect();
    const inStockCheckbox = document.getElementById("shop-in-stock-checkbox");
    if (inStockCheckbox) {
      inStockCheckbox.checked = currentFilters.inStockOnly === true;
    }
  }

  function syncDesktopColor(value) {
    const selectedValue = normalizeFilterText(value);
    document
      .querySelectorAll('input[name="productColor"]')
      .forEach(function (input) {
        input.checked = normalizeFilterText(input.value) === selectedValue;
      });
  }

  function syncDesktopSizes(sizes) {
    const selectedSizes = new Set(
      (Array.isArray(sizes) ? sizes : []).map(normalizeFilterText),
    );
    document
      .querySelectorAll('input[name="productSizes"]')
      .forEach(function (input) {
        input.checked = selectedSizes.has(normalizeFilterText(input.value));
      });
  }

  function syncMobileColorSelect() {
    const mobileColorSelect = document.getElementById("mobile-color-select");
    if (mobileColorSelect) {
      mobileColorSelect.value = currentFilters.colors[0] || "";
    }
  }

  function syncMobileSizeSelect() {
    const mobileSizeSelect = document.getElementById("mobile-size-select");
    if (mobileSizeSelect) {
      mobileSizeSelect.value = currentFilters.sizes[0] || "";
    }
  }

  function getSelectedDesktopSizes() {
    return Array.from(
      document.querySelectorAll('input[name="productSizes"]:checked'),
    )
      .map(function (input) {
        return normalizeFilterText(input.value);
      })
      .filter(Boolean);
  }

  function parseFilterNumber(value) {
    if (value === null || value === undefined || value === "") return null;
    const normalized = normalizeLocalizedDigits(value).replace(/[^\d.]/g, "");
    const parsed = Number(normalized);
    return Number.isFinite(parsed) ? parsed : null;
  }

  function normalizeLocalizedDigits(value) {
    const persianDigits = "۰۱۲۳۴۵۶۷۸۹";
    const arabicDigits = "٠١٢٣٤٥٦٧٨٩";
    return String(value ?? "")
      .replace(/[۰-۹]/g, function (digit) {
        return String(persianDigits.indexOf(digit));
      })
      .replace(/[٠-٩]/g, function (digit) {
        return String(arabicDigits.indexOf(digit));
      });
  }

  function setPriceFilter(minValue, maxValue, minBound, maxBound) {
    const selectedMin = Math.round(Number(minValue));
    const selectedMax = Math.round(Number(maxValue));
    const rangeMin = Math.round(Number(minBound));
    const rangeMax = Math.round(Number(maxBound));

    currentFilters.minPrice =
      Number.isFinite(selectedMin) && selectedMin > rangeMin ? selectedMin : null;
    currentFilters.maxPrice =
      Number.isFinite(selectedMax) && selectedMax < rangeMax ? selectedMax : null;
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

  function getProductVariantList(product) {
    return [
      ...(Array.isArray(product?.productVariants) ? product.productVariants : []),
      ...(Array.isArray(product?.ProductVariants) ? product.ProductVariants : []),
      ...(Array.isArray(product?.variants) ? product.variants : []),
      ...(Array.isArray(product?.Variants) ? product.Variants : [])
    ];
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

    const colorList = extractArray(colors).map(getFacetLabel).filter(Boolean);
    if (colorList.length === 0) {
      container.innerHTML =
        '<p class="text-sm text-gray-400">رنگی برای فیلتر محصولات موجود نیست.</p>';
      if (mobileColorSelect) {
        mobileColorSelect.innerHTML = '<option value="">همه رنگ‌ها</option>';
      }
      return;
    }

    const allColorOption = `
      <div class="flex items-center">
        <input type="radio" name="productColor" id="product-color-all" value="" class="hidden peer">
        <label for="product-color-all" class="select-none dark:!text-white cursor-pointer flex items-center justify-center rounded-full border-2 border-gray-200 py-1 px-3 text-gray-700 transition-colors duration-200 ease-in-out peer-checked:text-gray-900 peer-checked:border-primary-500">
          <span>همه</span>
        </label>
      </div>
    `;

    container.innerHTML =
      allColorOption +
      colorList
        .map(function (color, index) {
          const name = color;
          const id = `product-color-${slugifyFilterValue(name) || index}`;
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
            const name = color;
            const value = name || index;
            return `<option value="${escapeAttribute(value)}">${escapeHtml(name)}</option>`;
          })
          .join("");
    }
  }

  function renderSizes(sizes) {
    const container = document.getElementById("product-sizes-container");
    const mobileSizeSelect = document.getElementById("mobile-size-select");
    if (!container) return;

    const sizeList = extractArray(sizes).map(getFacetLabel).filter(Boolean);
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
        const title = size;
        const id = `product-size-${slugifyFilterValue(title) || index}`;
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
            const title = size;
            const value = title || index;
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

    let minVal = parseFilterNumber(currentFilters.minPrice);
    let maxVal = parseFilterNumber(currentFilters.maxPrice);
    minVal = minVal === null ? min : Math.min(Math.max(min, minVal), max);
    maxVal = maxVal === null ? max : Math.min(Math.max(min, maxVal), max);

    if (minVal > maxVal) {
      minVal = min;
      maxVal = max;
    }

    slider.style.touchAction = "none";

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

    function getClientX(event) {
      return event.clientX ?? event.touches?.[0]?.clientX ?? 0;
    }

    function startDrag(isMin, startEvent) {
      if (startEvent?.preventDefault) {
        startEvent.preventDefault();
      }

      function onMove(event) {
        if (event.cancelable) {
          event.preventDefault();
        }

        const rect = slider.getBoundingClientRect();
        const x = Math.min(
          Math.max(getClientX(event) - rect.left, 0),
          rect.width,
        );
        const value = min + (x / rect.width) * (max - min);

        if (isMin) {
          minVal = Math.min(Math.max(min, value), maxVal);
        } else {
          maxVal = Math.max(Math.min(max, value), minVal);
        }

        updateUI();
      }

      function onUp() {
        document.removeEventListener("pointermove", onMove);
        document.removeEventListener("pointerup", onUp);
        document.removeEventListener("mousemove", onMove);
        document.removeEventListener("mouseup", onUp);
        document.removeEventListener("touchmove", onMove);
        document.removeEventListener("touchend", onUp);
        setPriceFilter(minVal, maxVal, min, max);
        applyFilters();
      }

      onMove(startEvent);

      if (window.PointerEvent) {
        document.addEventListener("pointermove", onMove);
        document.addEventListener("pointerup", onUp);
      } else {
        document.addEventListener("mousemove", onMove);
        document.addEventListener("mouseup", onUp);
        document.addEventListener("touchmove", onMove, { passive: false });
        document.addEventListener("touchend", onUp);
      }
    }

    if (window.PointerEvent) {
      minThumb.addEventListener("pointerdown", function (event) {
        startDrag(true, event);
      });
      maxThumb.addEventListener("pointerdown", function (event) {
        startDrag(false, event);
      });
    } else {
      minThumb.addEventListener("mousedown", function (event) {
        startDrag(true, event);
      });
      maxThumb.addEventListener("mousedown", function (event) {
        startDrag(false, event);
      });
      minThumb.addEventListener("touchstart", function (event) {
        startDrag(true, event);
      });
      maxThumb.addEventListener("touchstart", function (event) {
        startDrag(false, event);
      });
    }

    updateUI();
  }

  function showLoading(container) {
    if (!container) return;

    container.innerHTML = `
      <div class="lg:col-span-4 md:col-span-6 col-span-12 w-full">
        <div class="rounded-xl bg-white dark:bg-gray-800 p-3 sm:p-4 shadow-sm animate-pulse">
          <div class="h-40 sm:h-48 rounded-lg bg-gray-200 dark:bg-gray-700 mb-4"></div>
          <div class="h-4 rounded bg-gray-200 dark:bg-gray-700 mb-3"></div>
          <div class="h-4 w-2/3 rounded bg-gray-200 dark:bg-gray-700 mb-6"></div>
          <div class="flex items-center justify-between">
            <div class="h-6 w-28 rounded bg-gray-200 dark:bg-gray-700"></div>
            <div class="h-10 w-10 rounded-lg bg-gray-200 dark:bg-gray-700"></div>
          </div>
        </div>
      </div>
      <div class="lg:col-span-4 md:col-span-6 col-span-12 w-full hidden md:block">
        <div class="rounded-xl bg-white dark:bg-gray-800 p-3 sm:p-4 shadow-sm animate-pulse">
          <div class="h-40 sm:h-48 rounded-lg bg-gray-200 dark:bg-gray-700 mb-4"></div>
          <div class="h-4 rounded bg-gray-200 dark:bg-gray-700 mb-3"></div>
          <div class="h-4 w-2/3 rounded bg-gray-200 dark:bg-gray-700 mb-6"></div>
          <div class="flex items-center justify-between">
            <div class="h-6 w-28 rounded bg-gray-200 dark:bg-gray-700"></div>
            <div class="h-10 w-10 rounded-lg bg-gray-200 dark:bg-gray-700"></div>
          </div>
        </div>
      </div>
      <div class="lg:col-span-4 md:col-span-6 col-span-12 w-full hidden lg:block">
        <div class="rounded-xl bg-white dark:bg-gray-800 p-3 sm:p-4 shadow-sm animate-pulse">
          <div class="h-40 sm:h-48 rounded-lg bg-gray-200 dark:bg-gray-700 mb-4"></div>
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
