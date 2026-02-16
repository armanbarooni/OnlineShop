/**
 * Product Page (product.html) API Integration
 */

let normalizedVariants = [];

function isVisibleProduct(product) {
  return !!product && product.deleted !== true;
}

// Initialize product page
document.addEventListener("DOMContentLoaded", async () => {
  // Wait for all services to load
  if (
    typeof window.apiClient === "undefined" ||
    typeof window.productService === "undefined"
  ) {
    if (window.logger) {
      window.logger.error("Required services not loaded");
    } else {
      console.error("Required services not loaded");
    }
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

    // Parse product ID from URL
    const urlParams = new URLSearchParams(window.location.search);
    const productId = urlParams.get("id");

    if (!productId) {
      // No id in query string: load a default product from API.
      await loadDefaultProduct();
      return;
    }

    // Load product data
    await loadProduct(productId);
  } catch (error) {
    if (window.logger) {
      window.logger.error("Error initializing product page:", error);
    } else {
      console.error("Error initializing product page:", error);
    }
    showError("خطا در بارگذاری صفحه محصول");
  }
});

async function loadDefaultProduct() {
  try {
    showLoading();

    const result = await window.productService.getAllProducts();
    const products = Array.isArray(result.data)
      ? result.data
      : result.data && Array.isArray(result.data.items)
        ? result.data.items
        : result.data && Array.isArray(result.data.products)
          ? result.data.products
          : [];
    const visibleProducts = products.filter(isVisibleProduct);
    if (!result.success || visibleProducts.length === 0) {
      showError(result.error || "No products available");
      return;
    }
    const withImages = visibleProducts.find(
      (p) => Array.isArray(p.productImages) && p.productImages.length > 0,
    );
    renderProduct(withImages || visibleProducts[0]);
  } catch (error) {
    if (window.logger) {
      window.logger.error("Error loading default product:", error);
    } else {
      console.error("Error loading default product:", error);
    }
    showError("Error loading default product");
  } finally {
    hideLoading();
  }
}
// Load product by ID
async function loadProduct(productId) {
  try {
    showLoading();

    const result = await window.productService.getProductById(productId);

    if (result.success && result.data && isVisibleProduct(result.data)) {
      renderProduct(result.data);
      hideLoading(); // ✅ فقط بعد از render کامل
    } else {
      showError(result.error || "محصول یافت نشد");
    }
  } catch (error) {
    if (window.logger) {
      window.logger.error("Error loading product:", error);
    } else {
      console.error("Error loading product:", error);
    }
    showError("خطا در دریافت اطلاعات محصول");
  } finally {
    hideLoading();
  }
}

// Render product data
function renderProduct(product) {
  // Product title
  const titleFa = document.getElementById("product-title-fa");
  if (titleFa) titleFa.textContent = product.name || "محصول بدون نام";

  const titleEn = document.getElementById("product-title-en");
  if (titleEn) titleEn.textContent = product.englishTitle || product.name || "";

  // Breadcrumb
  const breadcrumbTitle = document.getElementById("breadcrumb-product-title");
  if (breadcrumbTitle) breadcrumbTitle.textContent = product.name || "محصول";

  // Price
  renderPrice(product);

  // Images/Gallery
  renderGallery(product);

  // Description
  renderDescription(product);

  // Variant selectors
  renderVariantSelectors(product);

  // Specifications
  renderSpecifications(product);
}

// Render product price
function renderPrice(product) {
  const currentPriceEl = document.getElementById("product-current-price");
  const oldPriceEl = document.getElementById("product-old-price");
  const discountBadge = document.getElementById("product-discount-badge");

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
      oldPriceEl.classList.remove("hidden");
    }
    if (discountBadge) {
      const discountPercent = Math.round(((price - salePrice) / price) * 100);
      discountBadge.textContent = `${discountPercent}%`;
      discountBadge.classList.remove("hidden");
    }
  } else {
    if (oldPriceEl) oldPriceEl.classList.add("hidden");
    if (discountBadge) discountBadge.classList.add("hidden");
  }
}

// Render product gallery
function renderGallery(product) {
  const images = pickImages(product);

  if (images.length === 0) {
    // Use placeholder if no images
    images.push({
      imageUrl: "assets/images/product/nophotos.png",
    });
  }

  const gallery1 = document.querySelector("#productGalleryOne .swiper-wrapper");
  const gallery2 = document.querySelector("#productGalleryTwo .swiper-wrapper");

  const slidesHtml = images
    .map((img) => {
      const url = normalizeImageUrl(img.imageUrl || img);
      return `
        <div class="swiper-slide !pe-1 cursor-pointer">
            <img src="${url}" alt="${product.name || "محصول"}" 
                 class="rounded-lg border border-gray-300 p-2 w-full object-cover"
                 onerror="this.src='assets/images/product/nophoto.png'">
        </div>
    `;
    })
    .join("");

  if (gallery1) gallery1.innerHTML = slidesHtml;
  if (gallery2) gallery2.innerHTML = slidesHtml;

  // Reinitialize swiper if needed
  if (window.swiperInstances) {
    Object.values(window.swiperInstances).forEach((swiper) => {
      if (swiper && typeof swiper.update === "function") {
        swiper.update();
      }
    });
  }
}

function pickImages(product) {
  if (Array.isArray(product.productImages) && product.productImages.length)
    return product.productImages;
  if (Array.isArray(product.images) && product.images.length)
    return product.images;
  return [];
}

function normalizeImageUrl(url) {
  if (!url) return "";
  if (url.startsWith("http://") || url.startsWith("https://")) return url;
  if (url.startsWith("/")) return `https://mahakacc.mahaksoft.com${url}`;
  return url;
}

function getProductColor(product) {
  if (product.color) return product.color;
  const variant = Array.isArray(product.variants) ? product.variants[0] : null;
  if (variant?.color) return variant.color;
  if (variant?.attributes?.color) return variant.attributes.color;
  return null;
}

function getProductSize(product) {
  if (product.size) return product.size;
  const variant = Array.isArray(product.variants) ? product.variants[0] : null;
  if (variant?.size) return variant.size;
  if (variant?.attributes?.size) return variant.attributes.size;
  return null;
}

function toArray(value) {
  return Array.isArray(value) ? value : [];
}

function toNumber(value) {
  if (value === null || value === undefined || value === "") return null;
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : null;
}

function stringOrEmpty(value) {
  return value === null || value === undefined ? "" : String(value).trim();
}

function extractVariantAttributes(row, product) {
  let color =
    stringOrEmpty(row.color) ||
    stringOrEmpty(row.Color) ||
    stringOrEmpty(row.colorName) ||
    stringOrEmpty(row.ColorName) ||
    stringOrEmpty(row.attributes?.color) ||
    stringOrEmpty(row.attributes?.Color);

  let size =
    stringOrEmpty(row.size) ||
    stringOrEmpty(row.Size) ||
    stringOrEmpty(row.attributes?.size) ||
    stringOrEmpty(row.attributes?.Size);

  const rowProperties = [
    ...toArray(row.productProperties),
    ...toArray(row.ProductProperties),
    ...toArray(row.properties),
    ...toArray(row.Properties),
  ];
  const productProperties = [
    ...toArray(product.productProperties),
    ...toArray(product.ProductProperties),
  ];

  let feature8Title = "";
  let feature8Value = "";
  let feature9Title = "";
  let feature9Value = "";
  [...rowProperties, ...productProperties].forEach((prop) => {
    const code = String(
      prop.propertyDescriptionCode ??
        prop.PropertyDescriptionCode ??
        prop.code ??
        prop.Code ??
        "",
    ).replace(/^0+/, "");

    const title = stringOrEmpty(
      prop.title ??
        prop.Title ??
        prop.name ??
        prop.Name ??
        prop.propertyDescriptionTitle ??
        prop.PropertyDescriptionTitle,
    );

    const value = stringOrEmpty(
      prop.value ??
        prop.Value ??
        prop.text ??
        prop.Text ??
        prop.valueTitle ??
        prop.ValueTitle ??
        prop.propertyDescriptionValueTitle ??
        prop.PropertyDescriptionValueTitle,
    );

    const normalizedTitle = title.toLowerCase();

    if (!color && (code === "3" || title.includes("رنگ") || normalizedTitle === "color")) {
      color = value || title;
    }
    if (!size && (code === "4" || title.includes("سایز") || normalizedTitle === "size")) {
      size = value || title;
    }

    if (code === "8" && !feature8Title) feature8Title = title || "ویژگی ۸";
    if (code === "9" && !feature9Title) feature9Title = title || "ویژگی ۹";
    if (code === "8" && !feature8Value) feature8Value = value || "";
    if (code === "9" && !feature9Value) feature9Value = value || "";
  });

  return {
    color,
    size,
    feature8Title: feature8Title || "ویژگی ۸",
    feature8Value,
    feature9Title: feature9Title || "ویژگی ۹",
    feature9Value,
  };
}

function resolveVariantStock(row, product) {
  const directStock = toNumber(
    row.count1 ??
      row.Count1 ??
      row.stockQuantity ??
      row.StockQuantity ??
      row.quantity ??
      row.Quantity,
  );
  if (directStock !== null) return directStock;

  const fallbackStock = toNumber(
    product.stockQuantity ?? product.StockQuantity ?? product.quantity,
  );
  return fallbackStock ?? 0;
}

function buildSizeVariants(product) {
  const rows = [
    ...toArray(product.productDetails),
    ...toArray(product.ProductDetails),
    ...toArray(product.details),
    ...toArray(product.variants),
    ...toArray(product.Variants),
  ];
  const sourceRows = rows.length > 0 ? rows : [product];

  return sourceRows.map((row) => {
    const attrs = extractVariantAttributes(row, product);
    return {
      size: attrs.size || "",
      color: attrs.color || "",
      feature8Title: attrs.feature8Title || "ویژگی ۸",
      feature8Value: attrs.feature8Value || "",
      feature9Title: attrs.feature9Title || "ویژگی ۹",
      feature9Value: attrs.feature9Value || "",
      stock: resolveVariantStock(row, product),
    };
  });
}

function extractUniqueValues(items, key) {
  const set = new Set();
  items.forEach((item) => {
    const value = stringOrEmpty(item[key]);
    if (value) set.add(value);
  });
  return Array.from(set);
}

function fillSelect(selectEl, values, defaultLabel) {
  const options = [`<option value="">${defaultLabel}</option>`];
  values.forEach((value) => {
    options.push(`<option value="${escapeHtml(value)}">${escapeHtml(value)}</option>`);
  });
  selectEl.innerHTML = options.join("");
}

function toggleAddToCartByStock(inStock) {
  const addToCartBtn = document.getElementById("product-add-to-cart-btn");
  if (!addToCartBtn) return;
  addToCartBtn.disabled = !inStock;
  addToCartBtn.classList.toggle("opacity-60", !inStock);
  addToCartBtn.classList.toggle("cursor-not-allowed", !inStock);
}

function updateStockBySelectedVariant() {
  const colorSelect = document.getElementById("product-color-select");
  const sizeSelect = document.getElementById("product-size-select");
  const statusEl = document.getElementById("product-stock-status");

  const colors = extractUniqueValues(normalizedVariants, "color");
  const sizes = extractUniqueValues(normalizedVariants, "size");
  const needColor = colors.length > 0;
  const needSize = sizes.length > 0;

  const selectedColor = colorSelect ? stringOrEmpty(colorSelect.value) : "";
  const selectedSize = sizeSelect ? stringOrEmpty(sizeSelect.value) : "";

  if ((needColor && !selectedColor) || (needSize && !selectedSize)) {
    if (statusEl) statusEl.classList.add("hidden");
    toggleAddToCartByStock(false);
    return;
  }

  const matched = normalizedVariants.filter((variant) => {
    const colorOk = !needColor || variant.color === selectedColor;
    const sizeOk = !needSize || variant.size === selectedSize;
    return colorOk && sizeOk;
  });

  const stockCount = matched.reduce((sum, item) => sum + (toNumber(item.stock) || 0), 0);

  if (!statusEl) {
    toggleAddToCartByStock(stockCount > 0);
    return;
  }

  statusEl.classList.remove("hidden", "text-red-600", "text-green-600");
  if (stockCount > 0) {
    statusEl.textContent = "موجود است";
    statusEl.classList.add("text-green-600");
    toggleAddToCartByStock(true);
  } else {
    statusEl.textContent = "موجود نیست";
    statusEl.classList.add("text-red-600");
    toggleAddToCartByStock(false);
  }
}

function renderVariantSelectors(product) {
  const container = document.getElementById("product-variant-container");
  const colorSelect = document.getElementById("product-color-select");
  const sizeSelect = document.getElementById("product-size-select");
  const colorWrap = document.getElementById("product-color-select-wrap");
  const sizeWrap = document.getElementById("product-size-select-wrap");
  const statusEl = document.getElementById("product-stock-status");

  if (!container || !colorSelect || !sizeSelect) return;

  normalizedVariants = buildSizeVariants(product);
  const colors = extractUniqueValues(normalizedVariants, "color");
  const sizes = extractUniqueValues(normalizedVariants, "size");

  if (colors.length === 0 && sizes.length === 0) {
    container.classList.add("hidden");
    if (statusEl) statusEl.classList.add("hidden");
    toggleAddToCartByStock(true);
    return;
  }

  container.classList.remove("hidden");
  fillSelect(colorSelect, colors, "انتخاب رنگ");
  fillSelect(sizeSelect, sizes, "انتخاب سایز");

  if (colors.length === 1) colorSelect.value = colors[0];
  if (sizes.length === 1) sizeSelect.value = sizes[0];

  if (colorWrap) colorWrap.classList.toggle("hidden", colors.length === 0);
  if (sizeWrap) sizeWrap.classList.toggle("hidden", sizes.length === 0);

  colorSelect.onchange = updateStockBySelectedVariant;
  sizeSelect.onchange = updateStockBySelectedVariant;

  if (statusEl) statusEl.classList.add("hidden");
  updateStockBySelectedVariant();
}

// Render product description
function renderDescription(product) {
  // Update Intro tab content
  const introTab = document.getElementById("Intro");
  const descText =
    product.description && product.description.trim().length > 0
      ? product.description
      : "توضیحات این محصول به‌زودی تکمیل می‌شود.";
  if (introTab) {
    const descriptionPara = introTab.querySelector("p");
    if (descriptionPara) {
      descriptionPara.textContent = descText;
    }
  }

  // Update features list
  const featuresList = document.getElementById("product-features-list");
  if (featuresList && descText) {
    // Split description by newlines or create list items
    const features = descText.split("\n").filter((f) => f.trim());
    if (features.length > 0) {
      featuresList.innerHTML = features
        .map(
          (feature) => `
                <li class="flex items-center space-x-3">
                    <span class="inline-block text-base">${feature.trim()}</span>
                </li>
            `,
        )
        .join("");
    }
  }
}

// Render product specifications
function renderSpecifications(product) {
  const specsTab = document.getElementById("Specifications");
  if (!specsTab) return;

  const specsDiv =
    specsTab.querySelector("div.space-y-5 > div") ||
    specsTab.querySelector("div.space-y-5");
  if (!specsDiv) return;

  const variants = buildSizeVariants(product);
  const rowsBySize = new Map();
  let column8Title = "ویژگی ۸";
  let column9Title = "ویژگی ۹";

  variants.forEach((variant) => {
    if (variant.feature8Title && variant.feature8Title !== "ویژگی ۸") {
      column8Title = variant.feature8Title;
    }
    if (variant.feature9Title && variant.feature9Title !== "ویژگی ۹") {
      column9Title = variant.feature9Title;
    }

    const sizeLabel = variant.size || "تک سایز";
    if (!rowsBySize.has(sizeLabel)) {
      rowsBySize.set(sizeLabel, {
        size: sizeLabel,
        stock: 0,
        feature8Value: "",
        feature9Value: "",
      });
    }

    const row = rowsBySize.get(sizeLabel);
    row.stock += toNumber(variant.stock) || 0;
    if (!row.feature8Value && variant.feature8Value) {
      row.feature8Value = variant.feature8Value;
    }
    if (!row.feature9Value && variant.feature9Value) {
      row.feature9Value = variant.feature9Value;
    }
  });

  const sizeRows = Array.from(rowsBySize.values());
  const hasData = sizeRows.length > 0;

  specsDiv.innerHTML = `
    <h2 class="text-2xl pb-3 font-black text-zinc-800 relative before:absolute before:bottom-0 before:start-0 before:h-1 before:w-22 before:bg-primary-500 before:rounded dark:text-white">جدول سایز</h2>
    <div class="rounded-xl border border-gray-200 dark:border-zinc-700 shadow-sm overflow-hidden">
      <div class="overflow-x-auto">
        <table class="min-w-full text-sm">
          <thead class="bg-gray-100 dark:bg-zinc-700">
            <tr>
              <th class="py-3 px-4 text-start font-bold text-gray-800 dark:text-white">سایز</th>
              <th class="py-3 px-4 text-start font-bold text-gray-800 dark:text-white">${escapeHtml(column8Title)}</th>
              <th class="py-3 px-4 text-start font-bold text-gray-800 dark:text-white">${escapeHtml(column9Title)}</th>
              <th class="py-3 px-4 text-start font-bold text-gray-800 dark:text-white">موجودی</th>
            </tr>
          </thead>
          <tbody>
            ${
              hasData
                ? sizeRows
                    .map((row) => {
                      return `
                        <tr class="border-b border-gray-200 dark:border-zinc-700 hover:bg-gray-50 dark:hover:bg-zinc-800/60">
                          <td class="py-3 px-4 font-semibold text-primary-700 dark:text-primary-300">${escapeHtml(row.size)}</td>
                          <td class="py-3 px-4 text-gray-700 dark:text-gray-300">${escapeHtml(row.feature8Value || "")}</td>
                          <td class="py-3 px-4 text-gray-700 dark:text-gray-300">${escapeHtml(row.feature9Value || "")}</td>
                          <td class="py-3 px-4">
                            <span class="${row.stock > 0 ? "text-green-600" : "text-red-600"} font-semibold">
                              ${row.stock > 0 ? `${formatCount(row.stock)} عدد` : "ناموجود"}
                            </span>
                          </td>
                        </tr>
                      `;
                    })
                    .join("")
                : `
                  <tr>
                    <td colspan="4" class="py-6 px-4 text-center text-gray-500 dark:text-gray-400">
                      اطلاعاتی برای جدول سایز این محصول ثبت نشده است.
                    </td>
                  </tr>
                `
            }
          </tbody>
        </table>
      </div>
    </div>
  `;
}

function escapeHtml(value) {
  return String(value)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/\"/g, "&quot;")
    .replace(/'/g, "&#39;");
}

function formatCount(value) {
  return new Intl.NumberFormat("fa-IR").format(Math.round(value));
}

// Format price
function formatPrice(price) {
  const normalizedPrice = toNumber(price) || 0;
  const rialPrice = normalizedPrice * 10;
  return new Intl.NumberFormat("fa-IR").format(Math.round(rialPrice));
}

// Show loading state
function showLoading() {
  document.getElementById("product-loading")?.classList.remove("hidden");
  document.getElementById("product-content")?.classList.add("hidden");
}

// Hide loading state
function hideLoading() {
  document.getElementById("product-loading")?.classList.add("hidden");
  document.getElementById("product-content")?.classList.remove("hidden");
}

// Show error message
function showError(message) {
  const errorEl = document.getElementById("product-error");
  if (errorEl) {
    errorEl.textContent = message;
    errorEl.classList.remove("hidden");
  } else if (window.utils && window.utils.showToast) {
    window.utils.showToast(message, "error");
  } else {
    alert(message);
  }
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
