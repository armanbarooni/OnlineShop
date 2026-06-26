/**
 * Product Page (product.html) API Integration
 */

let normalizedVariants = [];
let availableVariants = [];

function isVisibleProduct(product) {
  return (
    !!product &&
    product.deleted !== true &&
    product.Deleted !== true &&
    product.deletedByMahak !== true &&
    product.DeletedByMahak !== true
  );
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
  // Store product data globally for add-to-cart
  window.__currentProduct = product;

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

  // Setup add-to-cart button
  setupAddToCartButton(product);
}

// Setup add-to-cart button handler
function setupAddToCartButton(product) {
  const addToCartBtn = document.getElementById("product-add-to-cart-btn");
  if (!addToCartBtn) return;

  addToCartBtn.onclick = async function() {
    if (availableVariants.length === 0) {
      showToast("این کالا ناموجود است", "warning");
      return;
    }

    // Get selected variant
    const colorSelect = document.getElementById("product-color-select");
    const sizeSelect = document.getElementById("product-size-select");
    const colors = extractUniqueValues(availableVariants, "color");
    const sizes = extractUniqueValues(availableVariants, "size");
    const needColor = colors.length > 0;
    const needSize = sizes.length > 0;

    const selectedColor = colorSelect ? stringOrEmpty(colorSelect.value) : "";
    const selectedSize = sizeSelect ? stringOrEmpty(sizeSelect.value) : "";

    // Validate variant selection
    if (needColor && !selectedColor) {
      showToast("لطفاً رنگ مورد نظر را انتخاب کنید", "warning");
      if (colorSelect) colorSelect.focus();
      return;
    }
    if (needSize && !selectedSize) {
      showToast("لطفاً سایز مورد نظر را انتخاب کنید", "warning");
      if (sizeSelect) sizeSelect.focus();
      return;
    }

    // Find the matching variant ID from product data
    let variantId = resolveVariantId(product, selectedColor, selectedSize);
    if ((needColor || needSize) && !variantId) {
      showToast("ترکیب رنگ و سایز انتخاب‌شده معتبر نیست", "warning");
      return;
    }

    // Build request body
    const requestBody = {
      productId: product.id,
      quantity: 1,
      variantId: variantId || "00000000-0000-0000-0000-000000000000"
    };

    // Disable button and show loading
    addToCartBtn.disabled = true;
    const originalText = addToCartBtn.textContent;
    addToCartBtn.textContent = "در حال افزودن...";

    try {
      const response = await window.cartService.addToCart(
        requestBody.productId,
        requestBody.quantity,
        requestBody.variantId === "00000000-0000-0000-0000-000000000000" ? null : requestBody.variantId,
        null,
        product
      );

      if (response && response.success) {
        if (response.alreadyAtMaxStock) {
          showToast(response.message || "این رنگ و سایز قبلا در سبد خرید شماست و موجودی بیشتری ندارد", "warning");
        } else {
          showToast("محصول با موفقیت به سبد خرید اضافه شد", "success");
        }

        // Update cart count in header if available
        updateCartBadge(response.data);
      } else {
        const errorMsg = response?.data?.errorMessage || response?.error || "خطا در افزودن به سبد خرید";
        showToast(errorMsg, "error");
      }
    } catch (error) {
      console.error("Add to cart error:", error);
      showToast(error.message || "خطا در افزودن به سبد خرید", "error");
    } finally {
      addToCartBtn.disabled = false;
      addToCartBtn.textContent = originalText;
    }
  };
}

// Resolve variant ID from product data based on selected color/size
function resolveVariantId(product, selectedColor, selectedSize) {
  // Check productDetails first
  const detailRows = [
    ...toArray(product.productDetails),
    ...toArray(product.ProductDetails),
    ...toArray(product.details),
  ];

  // Check variants
  const variantRows = [
    ...toArray(product.variants),
    ...toArray(product.Variants),
    ...toArray(product.productVariants),
    ...toArray(product.ProductVariants),
  ];

  // Search in variants (they usually have IDs)
  for (const v of variantRows) {
    const attrs = extractVariantAttributes(v, product);
    const colorMatch = !selectedColor || valuesMatch(attrs.color, selectedColor);
    const sizeMatch = !selectedSize || valuesMatch(attrs.size, selectedSize);
    const variantId = getEntityId(v);
    if (colorMatch && sizeMatch && variantId) {
      return variantId;
    }
  }

  // Search in details
  for (const d of detailRows) {
    const attrs = extractVariantAttributes(d, product);
    const colorMatch = !selectedColor || valuesMatch(attrs.color, selectedColor);
    const sizeMatch = !selectedSize || valuesMatch(attrs.size, selectedSize);
    const variantId = getEntityId(d);
    if (colorMatch && sizeMatch && variantId) {
      return variantId;
    }
  }

  // If only one variant exists, use it
  if (variantRows.length === 1) {
    return getEntityId(variantRows[0]);
  }

  return null;
}

// Update cart badge count in header
function updateCartBadge(cartData) {
  const badgeEls = document.querySelectorAll("[data-cart-count], .cart-count-badge");
  const totalItems = cartData?.data?.totalItems || cartData?.totalItems || 0;
  badgeEls.forEach(el => {
    el.textContent = totalItems;
    el.classList.toggle("hidden", totalItems === 0);
  });
}

// Show toast notification
function showToast(message, type = "info") {
  // Try using existing utils toast
  if (window.utils && typeof window.utils.showToast === "function") {
    window.utils.showToast(message, type);
    return;
  }

  // Fallback: create toast
  const existing = document.getElementById("custom-toast");
  if (existing) existing.remove();

  const colorMap = {
    success: "bg-green-600",
    error: "bg-red-600",
    warning: "bg-yellow-500",
    info: "bg-blue-600"
  };

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

// Render product price
function renderPrice(product) {
  const currentPriceEl = document.getElementById("product-current-price");
  const oldPriceEl = document.getElementById("product-old-price");
  const discountBadge = document.getElementById("product-discount-badge");
  const hasStock = availableVariants.length > 0 || ((toNumber(product.stockQuantity) || toNumber(product.quantity) || 0) > 0);

  if (!hasStock) {
    if (currentPriceEl) {
      currentPriceEl.textContent = "ناموجود";
      currentPriceEl.classList.add("text-red-600");
      currentPriceEl.removeAttribute("content");
    }
    if (oldPriceEl) oldPriceEl.classList.add("hidden");
    if (discountBadge) discountBadge.classList.add("hidden");
    return;
  }

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
  if (url.startsWith("/")) return `/api/ImageProxy?url=${encodeURIComponent(url)}`;
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

function toPropertyArray(value) {
  if (Array.isArray(value)) return value;
  if (!value) return [];

  if (typeof value === "string") {
    const text = value.trim();
    if (!text) return [];
    try {
      const parsed = JSON.parse(text);
      if (Array.isArray(parsed)) return parsed;
      if (parsed && typeof parsed === "object") return [parsed];
      return [];
    } catch (_error) {
      return [];
    }
  }

  if (typeof value === "object") return [value];
  return [];
}

function toNumber(value) {
  if (value === null || value === undefined || value === "") return null;
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : null;
}

function stringOrEmpty(value) {
  return value === null || value === undefined ? "" : String(value).trim();
}

function getEntityId(value) {
  if (!value || typeof value !== "object") return null;

  const rawId = value.id ?? value.Id ?? value.variantId ?? value.VariantId;
  const normalizedId = stringOrEmpty(rawId);
  return normalizedId || null;
}

function valuesMatch(left, right) {
  return stringOrEmpty(left).toLowerCase() === stringOrEmpty(right).toLowerCase();
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
    ...toPropertyArray(row.properties),
    ...toPropertyArray(row.Properties),
  ];
  const productProperties = [
    ...toArray(product.productProperties),
    ...toArray(product.ProductProperties),
  ];
  const productPropertyDescriptions = [
    ...toArray(product.propertyDescriptions),
    ...toArray(product.PropertyDescriptions),
  ];

  let feature8Title = "";
  let feature8Value = stringOrEmpty(row.feature8Value ?? row.Feature8Value);
  let feature9Title = "";
  let feature9Value = stringOrEmpty(row.feature9Value ?? row.Feature9Value);
  [...rowProperties, ...productProperties].forEach((prop) => {
    const code = String(
      prop.propertyDescriptionCode ??
        prop.PropertyDescriptionCode ??
        prop.code ??
        prop.C ??
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
        prop.v ??
        prop.V ??
        prop.text ??
        prop.Text ??
        prop.valueTitle ??
        prop.ValueTitle ??
        prop.propertyDescriptionValueTitle ??
        prop.PropertyDescriptionValueTitle,
    );

    const titleFromDescription = stringOrEmpty(
      productPropertyDescriptions.find((item) => {
        const itemCode = String(
          item.propertyDescriptionCode ??
            item.PropertyDescriptionCode ??
            item.code ??
            item.Code ??
            "",
        ).replace(/^0+/, "");
        return itemCode === code;
      })?.title ??
        productPropertyDescriptions.find((item) => {
          const itemCode = String(
            item.propertyDescriptionCode ??
              item.PropertyDescriptionCode ??
              item.code ??
              item.Code ??
              "",
          ).replace(/^0+/, "");
          return itemCode === code;
        })?.Title,
    );

    const effectiveTitle = title || titleFromDescription;
    const normalizedTitle = effectiveTitle.toLowerCase();

    if (
      !color &&
      (code === "3" || effectiveTitle.includes("رنگ") || normalizedTitle === "color")
    ) {
      color = value || effectiveTitle;
    }
    if (
      !size &&
      (code === "4" || effectiveTitle.includes("سایز") || normalizedTitle === "size")
    ) {
      size = value || effectiveTitle;
    }

    if (code === "8" && !feature8Title) feature8Title = effectiveTitle || "ویژگی ۸";
    if (code === "9" && !feature9Title) feature9Title = effectiveTitle || "ویژگی ۹";
    if (code === "8" && !feature8Value) feature8Value = value || "";
    if (code === "9" && !feature9Value) feature9Value = value || "";
  });

  return {
    color,
    size,
    feature8Title: feature8Title || "عرض سینه/کمر",
    feature8Value,
    feature9Title: feature9Title || "قد",
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

function isVariantInStock(variant) {
  return (toNumber(variant?.stock) || 0) > 0;
}

function getVariantsForColor(color) {
  if (!color) return [];
  return availableVariants.filter((variant) => variant.color === color);
}

function buildSizeVariants(product) {
  const rows = [
    ...toArray(product.productDetails),
    ...toArray(product.ProductDetails),
    ...toArray(product.details),
    ...toArray(product.variants),
    ...toArray(product.Variants),
    ...toArray(product.productVariants),
    ...toArray(product.ProductVariants),
  ];
  const sourceRows = rows.length > 0 ? rows : [product];

  return sourceRows
    .filter((row) => row?.deleted !== true && row?.Deleted !== true)
    .map((row) => {
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

  const colors = extractUniqueValues(availableVariants, "color");
  const selectedColor = colorSelect ? stringOrEmpty(colorSelect.value) : "";
  const colorVariants = selectedColor ? getVariantsForColor(selectedColor) : [];
  const sizes = selectedColor ? extractUniqueValues(colorVariants, "size") : [];
  const needColor = colors.length > 0;
  const needSize = sizes.length > 0;

  const selectedSize = sizeSelect ? stringOrEmpty(sizeSelect.value) : "";

  if ((needColor && !selectedColor) || (needSize && !selectedSize)) {
    if (statusEl) statusEl.classList.add("hidden");
    toggleAddToCartByStock(true);
    return;
  }

  const matched = availableVariants.filter((variant) => {
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
  availableVariants = normalizedVariants.filter(isVariantInStock);
  const colors = extractUniqueValues(availableVariants, "color");
  const selectedColor = colorSelect ? stringOrEmpty(colorSelect.value) : "";
  const sizes = selectedColor
    ? extractUniqueValues(getVariantsForColor(selectedColor), "size")
    : extractUniqueValues(availableVariants, "size");

  if (colors.length === 0 && sizes.length === 0) {
    container.classList.add("hidden");
    if (statusEl) statusEl.classList.add("hidden");
    if (statusEl) {
      statusEl.textContent = "ناموجود";
      statusEl.classList.remove("hidden");
      statusEl.classList.add("text-red-600");
    }
    toggleAddToCartByStock(false);
    return;
  }

  container.classList.remove("hidden");
  fillSelect(colorSelect, colors, "انتخاب رنگ");
  fillSelect(sizeSelect, sizes, colors.length > 0 ? "ابتدا رنگ را انتخاب کنید" : "انتخاب سایز");

  if (colors.length === 1) colorSelect.value = colors[0];

  if (colorWrap) colorWrap.classList.toggle("hidden", colors.length === 0);
  if (sizeWrap) sizeWrap.classList.toggle("hidden", colors.length > 0 && !colorSelect.value);

  colorSelect.onchange = updateStockBySelectedVariant;
  sizeSelect.onchange = updateStockBySelectedVariant;

  if (statusEl) statusEl.classList.add("hidden");
  if (colorSelect.value) {
    const colorSizes = extractUniqueValues(getVariantsForColor(colorSelect.value), "size");
    fillSelect(sizeSelect, colorSizes, "انتخاب سایز");
    if (sizeWrap) sizeWrap.classList.remove("hidden");
  }
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
    const introContainer = introTab.querySelector("div.space-y-5") || introTab;
    const lines = descText
      .split("\n")
      .map((line) => line.trim())
      .filter(Boolean);
    const introHtml =
      lines.length > 0
        ? lines
            .map(
              (line) => `
                <p class="text-sm sm:text-base leading-8 text-gray-700 dark:text-gray-300">
                  ${escapeHtml(line)}
                </p>
              `,
            )
            .join("")
        : `
            <p class="text-sm sm:text-base leading-8 text-gray-700 dark:text-gray-300">
              ${escapeHtml(descText)}
            </p>
          `;
    introContainer.innerHTML = introHtml;
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
                <li class="flex items-start gap-2">
                    <span class="inline-block text-base leading-7 break-words">${escapeHtml(feature.trim())}</span>
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
  let column8Title = "عرض سینه/کمر";
  let column9Title = "قد";

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
        feature8Values: new Set(),
        feature9Values: new Set(),
      });
    }

    const row = rowsBySize.get(sizeLabel);
    if (variant.feature8Value) row.feature8Values.add(variant.feature8Value);
    if (variant.feature9Value) row.feature9Values.add(variant.feature9Value);
  });

  const sizeTableDto = {
    column8Title,
    column9Title,
    rows: Array.from(rowsBySize.values()).map((row) => ({
      size: row.size,
      feature8Values: Array.from(row.feature8Values),
      feature9Values: Array.from(row.feature9Values),
    })),
  };
  const sizeRows = sizeTableDto.rows;
  const hasData = sizeRows.length > 0;

  specsDiv.innerHTML = `
    <h2 class="text-xl sm:text-2xl pb-3 font-black text-zinc-800 relative before:absolute before:bottom-0 before:start-0 before:h-1 before:w-22 before:bg-primary-500 before:rounded dark:text-white">جدول سایز</h2>
    <div class="rounded-xl border border-gray-200 dark:border-zinc-700 shadow-sm overflow-hidden">
      <div class="overflow-x-auto">
        <table class="min-w-full text-xs sm:text-sm">
          <thead class="bg-gray-100 dark:bg-zinc-700">
            <tr>
              <th class="py-3 px-3 sm:px-4 text-start font-bold text-gray-800 dark:text-white whitespace-nowrap">سایز</th>
              <th class="py-3 px-3 sm:px-4 text-start font-bold text-gray-800 dark:text-white whitespace-nowrap">${escapeHtml(sizeTableDto.column8Title)}</th>
              <th class="py-3 px-3 sm:px-4 text-start font-bold text-gray-800 dark:text-white whitespace-nowrap">${escapeHtml(sizeTableDto.column9Title)}</th>
            </tr>
          </thead>
          <tbody>
            ${
              hasData
                ? sizeRows
                    .map((row) => {
                      return `
                        <tr class="border-b border-gray-200 dark:border-zinc-700 hover:bg-gray-50 dark:hover:bg-zinc-800/60">
                          <td class="py-3 px-3 sm:px-4 font-semibold text-primary-700 dark:text-primary-300 whitespace-nowrap">${escapeHtml(row.size)}</td>
                          <td class="py-3 px-3 sm:px-4 text-gray-700 dark:text-gray-300 whitespace-nowrap">${escapeHtml((row.feature8Values || []).join("، "))}</td>
                          <td class="py-3 px-3 sm:px-4 text-gray-700 dark:text-gray-300 whitespace-nowrap">${escapeHtml((row.feature9Values || []).join("، "))}</td>
                        </tr>
                      `;
                    })
                    .join("")
                : `
                  <tr>
                    <td colspan="3" class="py-6 px-4 text-center text-gray-500 dark:text-gray-400">
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

// Format price
function formatPrice(price) {
  const normalizedPrice = toNumber(price) || 0;
  return new Intl.NumberFormat("fa-IR").format(Math.round(normalizedPrice));
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
