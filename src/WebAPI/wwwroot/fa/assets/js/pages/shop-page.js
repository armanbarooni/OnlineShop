/**
 * Shop Page (shop.html) API Integration
 */

// Initialize shop page
document.addEventListener("DOMContentLoaded", async () => {
  // Wait for all services to load
  if (
    typeof window.apiClient === "undefined" ||
    typeof window.productService === "undefined" ||
    typeof window.categoryService === "undefined"
  ) {
    return;
  }

  try {
    // Load categories
    await loadCategories();

    // Load mega menu categories
    const megaMenuContainer = document.getElementById(
      "mega-menu-list-container",
    );
    if (megaMenuContainer && window.categoryService) {
      await window.categoryService.renderMegaMenu("mega-menu-list-container");
    }
    // Parse URL Parameters
    const urlParams = new URLSearchParams(window.location.search);
    const categoryId = urlParams.get("category");
    const searchQuery = urlParams.get("search") || urlParams.get("q");

    // Load products
    await loadProducts(categoryId, searchQuery);
  } catch (error) {
    if (window.logger) {
      window.logger.error("Error initializing shop page:", error);
    } else {
      console.error("Error initializing shop page:", error);
    }
  }
});

// Load products based on category or search
async function loadProducts(categoryId, searchQuery) {
  const gridContainer = document.getElementById("shop-products-grid");

  if (!gridContainer) {
    // #region agent log

    // #endregion
    return;
  }

  // Show loading
  gridContainer.innerHTML =
    '<div class="col-span-full text-center p-10"><div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div><p class="mt-4 text-gray-500">در حال بارگذاری محصولات...</p></div>';

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
        pageSize: 20,
      });
    } else {
      // Load all products
      result = await window.productService.getAllProducts();
    }

    if (result.success !== undefined) {
      // Result has success property
      if (result.success && result.data) {
        // Handle different response structures

        console.log(result.data, "1-data");
        if (result.data.availableColors) {
          renderColors(result.data.availableColors);
        }

        if (result.data.availableSizes) {
          renderSizes(result.data.availableSizes);
        }

        if (result.data.priceRanges) {
          renderPriceFilter(result.data.priceRanges);
          initPriceSlider();
        }

        let products = [];
        if (result.data.products) {
          // products is an object with items property
          if (
            result.data.products.items &&
            Array.isArray(result.data.products.items)
          ) {
            products = result.data.products.items;
          } else if (Array.isArray(result.data.products)) {
            products = result.data.products;
          }
        } else if (result.data.items && Array.isArray(result.data.items)) {
          products = result.data.items;
        } else if (Array.isArray(result.data)) {
          products = result.data;
        }

        if (Array.isArray(products)) {
          renderProducts(products, gridContainer);
        } else {
          showError("فرمت محصولات نامعتبر است", gridContainer);
        }
      } else {
        showError(result.error || "خطا در دریافت محصولات", gridContainer);
      }
    } else if (Array.isArray(result)) {
      // Result is directly an array
      renderProducts(result, gridContainer);
    } else if (result.data) {
      // Result has data property - handle paginated response
      let products = [];
      if (
        result.data.products &&
        result.data.products.items &&
        Array.isArray(result.data.products.items)
      ) {
        products = result.data.products.items;
      } else if (result.data.products && Array.isArray(result.data.products)) {
        products = result.data.products;
      } else if (result.data.items && Array.isArray(result.data.items)) {
        products = result.data.items;
      } else if (Array.isArray(result.data)) {
        products = result.data;
      }

      if (Array.isArray(products) && products.length > 0) {
        renderProducts(products, gridContainer);
      } else {
        showError("محصولی یافت نشد", gridContainer);
      }
    } else {
      showError("فرمت پاسخ نامعتبر است", gridContainer);
    }
  } catch (error) {
    if (window.logger) {
      window.logger.error("Error loading products:", error);
    } else {
      console.error("Error loading products:", error);
    }
    showError("خطا در اتصال به سرور", gridContainer);
  }
}

// Render products in grid
function renderProducts(products, container) {
  const visibleProducts = (Array.isArray(products) ? products : []).filter(
    isVisibleProduct,
  );
  if (visibleProducts.length === 0) {
    container.innerHTML =
      '<div class="col-span-full text-center py-10"><p class="text-gray-500">محصولی یافت نشد</p></div>';
    return;
  }

  const html = visibleProducts
    .map((product) => createProductCard(product))
    .join("");
  container.innerHTML = html;
}

function isVisibleProduct(product) {
  return !!product && product.deleted !== true;
}

// Create product card HTML
function createProductCard(product) {
  const primaryImage = Array.isArray(product.images)
    ? product.images.find((i) => i && i.isPrimary) || product.images[0]
    : null;
  const galleryImage =
    product.productImages && product.productImages.length > 0
      ? product.productImages[0]
      : null;
  const rawImageUrl =
    primaryImage?.imageUrl || galleryImage?.imageUrl || product.imageUrl || "";
  const imageUrl = rawImageUrl
    ? rawImageUrl.startsWith("http")
      ? rawImageUrl
      : `https://mahakacc.mahaksoft.com${rawImageUrl}`
    : "assets/images/product/nophoto.png";
  const price = product.price || product.unitPrice || 0;
  const salePrice = product.salePrice || null;
  const finalPrice = salePrice || price;
  const discount =
    salePrice && price > salePrice
      ? Math.round(((price - salePrice) / price) * 100)
      : 0;
  const productUrl = `product.html?id=${product.id}`;
  const name = product.name || "نام محصول";

  return `
        <div class="lg:col-span-4 md:col-span-6 col-span-12 w-full">
            <article class="bg-white product-box-item drop-shadow-md rounded-xl p-4 dark:bg-gray-800 dark:border-white dark:border-1 h-full flex flex-col">
                <header class="flex items-center relative justify-between mb-3">
                    ${discount > 0 ? `<span class="absolute top-1 end-1 bg-red-500 text-white text-xs px-2 py-1 rounded z-10">${discount}%</span>` : ""}
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
                        <img src="${imageUrl}" alt="${name}" class="w-full h-48 object-contain" onerror="this.src='assets/images/product/nophoto.png'">
                    </figure>
                    <h3 class="text-sm font-bold mb-2 line-clamp-2 dark:text-white">${name}</h3>
                </a>
                <div class="flex items-center justify-between mt-auto">
                    <div class="flex flex-col">
                        ${discount > 0 ? `<span class="text-xs text-gray-400 line-through">${formatPrice(price)}</span>` : ""}
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
  return new Intl.NumberFormat("fa-IR").format(Math.round(price));
}

// Add to cart function (global)
window.addToCart = async function (productId) {
  if (!window.authService || !window.authService.isAuthenticated()) {
    window.location.href = "login.html";
    return;
  }

  try {
    const result = await window.cartService.addToCart(productId, 1);
    if (result.success) {
      if (window.utils) {
        window.utils.showToast("محصول به سبد خرید اضافه شد", "success");
      }
    } else {
      if (window.utils) {
        window.utils.showToast(
          result.error || "خطا در افزودن به سبد خرید",
          "error",
        );
      }
    }
  } catch (error) {
    if (window.logger) {
      window.logger.error("Error adding to cart:", error);
    } else {
      console.error("Error adding to cart:", error);
    }
    if (window.utils) {
      window.utils.showToast("خطا در اتصال به سرور", "error");
    }
  }
};

// Add to wishlist function (global)
window.addToWishlist = async function (productId) {
  if (!window.authService || !window.authService.isAuthenticated()) {
    window.location.href = "login.html";
    return;
  }

  try {
    if (window.wishlistService) {
      const result = await window.wishlistService.toggleWishlist(productId);
      if (result.success) {
        if (window.utils) {
          window.utils.showToast("به علاقه‌مندی‌ها اضافه شد", "success");
        }
      }
    }
  } catch (error) {
    if (window.logger) {
      window.logger.error("Error adding to wishlist:", error);
    } else {
      console.error("Error adding to wishlist:", error);
    }
  }
};

// Show error message
function showError(message, container) {
  container.innerHTML = `<div class="col-span-full text-center py-10"><p class="text-red-500">${message}</p></div>`;
}

// Load categories
async function loadCategories() {
  try {
    if (!window.categoryService) return;
    const result = await window.categoryService.getAllCategories();
    if (result.success && result.data) {
      renderCategories(result.data);
    }
  } catch (error) {
    if (window.logger) {
      window.logger.error("Error loading categories:", error);
    } else {
      console.error("Error loading categories:", error);
    }
  }
}

// Render categories
function renderCategories(categories) {
  const categoryContainer = document.querySelector("[data-categories]");
  if (!categoryContainer) return;

  const limitedCategories = Array.isArray(categories)
    ? categories.slice(0, 8)
    : [];
  if (limitedCategories.length === 0) return;

  const html = limitedCategories
    .map(
      (category) => `
        <a href="shop.html?category=${category.id}" class="lg:col-span-3 sm:col-span-6 col-span-12 w-full block">
            <article class="flex py-2 px-3 rounded-xl border border-gray-200 bg-white drop-shadow-md items-center justify-between dark:bg-gray-800">
                <section class="space-y-2">
                    <h3 class="text-lg font-bold dark:text-white">${category.name || "دسته‌بندی"}</h3>
                    <span class="text-xs font-light text-neutral-500">${category.description || ""}</span>
                </section>
                <figure>
                    <img src="${category.imageUrl || "assets/images/category/digitall.png"}" 
                         class="size-20" loading="lazy" alt="${category.name || "دسته‌بندی"}">
                </figure>
            </article>
        </a>
    `,
    )
    .join("");

  categoryContainer.innerHTML = html;
}

function renderColors(sizes) {
  const container = document.getElementById("product-colors-container");
  if (!container) return;

  if (!Array.isArray(sizes) || sizes.length === 0) {
    container.innerHTML = `<p class="text-sm text-gray-400">سایزی برای این محصول موجود نیست</p>`;
    return;
  }

  const html = sizes
    .map((size, index) => {
      const id = `size-${size.id || index}`;
      const name = size.name || size.title || size;

      return `
        <div class="flex items-center">
          <input
            type="radio"
            name="productSize"
            id="${id}"
            value="${name}"
            class="hidden peer"
          />
          <label
            for="${id}"
            class="select-none dark:!text-white cursor-pointer flex items-center justify-center rounded-full border-2 border-gray-200 py-1 px-3 text-gray-700 transition-colors duration-200 ease-in-out
                   peer-checked:text-gray-900 peer-checked:border-primary-500"
          >
            <span class="dir-ltr">${name}</span>
          </label>
        </div>
      `;
    })
    .join("");

  container.innerHTML = html;
}

function renderSizes(sizes) {
  const container = document.getElementById("product-sizes-container");
  if (!container) return;

  if (!Array.isArray(sizes) || sizes.length === 0) {
    container.innerHTML = `<p class="text-sm text-gray-400">سایزی برای این محصول موجود نیست</p>`;
    return;
  }

  const html = sizes
    .map((size, index) => {
      const id = `product-size-${size.id || index}`;
      const title = size.name || size.title || size;

      return `
        <div class="relative space-x-2 flex-wrap flex items-center">
          <label class="inline-flex items-center space-x-3 cursor-pointer">
            <input
              type="checkbox"
              id="${id}"
              name="productSizes"
              value="${title}"
              class="hidden peer"
            />
            <div
              class="w-5 h-5 border rounded bg-white border-gray-400
                     peer-checked:bg-blue-600 peer-checked:border-blue-600
                     flex items-center justify-center transition-all shadow-sm"
            >
              <svg
                class="w-4 h-4 text-white hidden peer-checked:block"
                fill="currentColor"
                viewBox="0 0 16 16"
                xmlns="http://www.w3.org/2000/svg"
              >
                <path
                  fill-rule="evenodd"
                  d="M10.97 4.97a.75.75 0 0 1 1.07 1.05l-4 4.5a.75.75 0 0 1-1.08.02l-2-2a.75.75 0 0 1 1.08-1.04l1.47 1.47 3.46-3.98z"
                ></path>
              </svg>
            </div>
            <span class="me-2 text-gray-700 dark:text-white">
              ${title}
            </span>
          </label>
        </div>
      `;
    })
    .join("");

  container.innerHTML = html;
}

function renderPriceFilter(priceRanges) {
  const container = document.getElementById("price-filter-container");
  if (!container) return;

  if (!Array.isArray(priceRanges) || priceRanges.length === 0) {
    container.innerHTML = `<p class="text-sm text-gray-400">فیلتر قیمتی موجود نیست</p>`;
    return;
  }

  const minPrice = Math.min(...priceRanges.map((p) => p.minPrice));
  const maxPrice = Math.max(...priceRanges.map((p) => p.maxPrice));

  container.innerHTML = `
  

    <div class="p-4 rounded-lg space-y-4 mx-auto">
        <div class="flex items-baseline gap-4">
          <div class="flex-1">
            <input
              type="text"
              id="min-price-input"
              value="${formatPrice(minPrice)}"
              class="w-full px-3 py-2 border border-gray-300 rounded-md bg-gray-100 dark:bg-zinc-900 text-center"
              disabled
            />
            <strong class="block text-center mt-3">تومان</strong>
          </div>

          <span class="text-gray-500 block">تا</span>

          <div class="flex-1">
            <input
              type="text"
              id="max-price-input"
              value="${formatPrice(maxPrice)}"
              class="w-full px-3 py-2 border border-gray-300 rounded-md bg-gray-100 dark:bg-zinc-900 text-center"
              disabled
            />
            <strong class="block text-center mt-3">تومان</strong>
          </div>
        </div>

        <div
          class="slider-container"
          data-min="${minPrice}"
          data-max="${maxPrice}"
        >
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

  // ✅ جلوگیری از crash (علت اصلی ارور قبلی)
  if (!minThumb || !maxThumb || !range || !minInput || !maxInput) {
    console.warn("initPriceSlider: required elements not found");
    return;
  }

  const min = Number(slider.dataset.min);
  const max = Number(slider.dataset.max);

  if (Number.isNaN(min) || Number.isNaN(max)) {
    console.warn("initPriceSlider: invalid min/max values");
    return;
  }

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
    function onMove(e) {
      const rect = slider.getBoundingClientRect();
      const x = Math.min(Math.max(e.clientX - rect.left, 0), rect.width);

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

  minThumb.addEventListener("mousedown", () => startDrag(true));
  maxThumb.addEventListener("mousedown", () => startDrag(false));

  // ✅ مقداردهی اولیه
  updateUI();
}

