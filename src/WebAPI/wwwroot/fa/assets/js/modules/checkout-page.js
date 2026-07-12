(function () {
    "use strict";

    const NO_PHOTO = "assets/images/product/nophoto.png";
    const MAHAK_CONTENT_BASE_URL =
        window.config?.content?.mahakBaseURL ||
        window.resolveMahakContentBaseURL?.() ||
        "https://mahakacc.mahaksoft.com";

    const state = {
        cart: null,
        items: [],
        addresses: [],
        selectedAddress: null,
        isAddressFormOpen: false,
        isSavingAddress: false,
        province: "",
        city: "",
        mahakRegions: [],
        mahakCityId: null
    };

    const IRAN_LOCATION_DATA = {
        "تهران": ["تهران", "اسلامشهر", "ری", "ورامین", "شهریار", "دماوند", "قدس", "رباط کریم", "پرند"],
        "البرز": ["کرج", "فردیس", "نظرآباد", "هشتگرد", "ماهدشت", "اشتهارد"],
        "اصفهان": ["اصفهان", "کاشان", "خمینی‌شهر", "نجف‌آباد", "شاهین‌شهر", "فولادشهر", "آران و بیدگل"],
        "فارس": ["شیراز", "مرودشت", "جهرم", "کازرون", "فسا", "لار", "داراب", "اقلید"],
        "خراسان رضوی": ["مشهد", "نیشابور", "سبزوار", "تربت حیدریه", "چناران", "طرقبه", "کاشمر"],
        "مازندران": ["ساری", "آمل", "بابل", "قائم‌شهر", "بابلسر", "بهشهر", "نوشهر", "چالوس"],
        "گیلان": ["رشت", "لاهیجان", "بندر انزلی", "رودسر", "آستارا", "صومعه‌سرا", "فومن"],
        "آذربایجان شرقی": ["تبریز", "مراغه", "مرند", "اهر", "میانه", "شبستر", "سراب"],
        "آذربایجان غربی": ["ارومیه", "خوی", "مهاباد", "بوکان", "سلماس", "پیرانشهر", "میاندوآب"],
        "خوزستان": ["اهواز", "آبادان", "خرمشهر", "دزفول", "ماهشهر", "شادگان", "اندیمشک"],
        "کرمان": ["کرمان", "رفسنجان", "سیرجان", "جیرفت", "بم", "زرند"],
        "یزد": ["یزد", "میبد", "اردکان", "بافق", "مهریز"],
        "قم": ["قم"],
        "قزوین": ["قزوین", "الوند", "تاکستان", "آبیک"],
        "همدان": ["همدان", "ملایر", "نهاوند", "تویسرکان", "اسدآباد"],
        "کرمانشاه": ["کرمانشاه", "اسلام‌آباد غرب", "سنقر", "هرسین", "کنگاور"],
        "هرمزگان": ["بندرعباس", "قشم", "کیش", "بندر لنگه", "میناب"],
        "سیستان و بلوچستان": ["زاهدان", "زابل", "ایرانشهر", "چابهار", "خاش", "سراوان"],
        "بوشهر": ["بوشهر", "برازجان", "گناوه", "کنگان", "دیلم"],
        "خراسان شمالی": ["بجنورد", "شیروان", "اسفراین", "جاجرم"],
        "خراسان جنوبی": ["بیرجند", "قائن", "نهبندان", "فردوس", "طبس"],
        "اردبیل": ["اردبیل", "پارس‌آباد", "مشگین‌شهر", "خلخال", "نمین"],
        "زنجان": ["زنجان", "ابهر", "خرمدره", "ماه‌نشان"],
        "سمنان": ["سمنان", "شاهرود", "دامغان", "گرمسار", "مهدی‌شهر"],
        "لرستان": ["خرم‌آباد", "بروجرد", "دورود", "الیگودرز", "ازنا"],
        "ایلام": ["ایلام", "دهلران", "مهران", "آبدانان"],
        "کردستان": ["سنندج", "مریوان", "بانه", "قروه", "سقز"],
        "چهارمحال و بختیاری": ["شهرکرد", "بروجن", "فارسان", "لردگان"],
        "کهگیلویه و بویراحمد": ["یاسوج", "دهدشت", "گچساران"],
        "مرکزی": ["اراک", "ساوه", "خمین", "محلات", "دلیجان"]
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

    function getCheckoutAddressPayload(address) {
        if (!address) return "";

        const parts = [
            address.state,
            address.city,
            address.addressLine1,
            address.addressLine2,
            address.postalCode
        ].filter(Boolean);

        return parts.join("، ");
    }

    function getCheckoutAddressId(address) {
        return address?.id ?? address?.addressId ?? address?.AddressId ?? null;
    }

    function normalizeSearchText(value) {
        return String(value || "").trim().toLowerCase();
    }

    function normalizeLocationName(value) {
        return String(value || "")
            .replace(/ي/g, "ی")
            .replace(/ك/g, "ک")
            .replace(/\s+/g, " ")
            .trim();
    }

    function renderOptionList(containerId, inputId, valuesOrGetter, onSelect, placeholder) {
        const container = document.getElementById(containerId);
        const input = document.getElementById(inputId);
        if (!container || !input) return;

        const values = typeof valuesOrGetter === "function" ? valuesOrGetter() : valuesOrGetter;
        const query = normalizeSearchText(input.value);
        const filtered = values.filter(value => normalizeSearchText(value).includes(query));

        if (!filtered.length) {
            container.innerHTML = `<div class="px-4 py-3 text-sm text-gray-500 dark:text-gray-300">${placeholder}</div>`;
        } else {
            container.innerHTML = filtered.map(value => `
                <button type="button" data-value="${escapeHtml(value)}" class="w-full text-start px-4 py-3 text-sm hover:bg-gray-100 dark:hover:bg-gray-700">
                    ${escapeHtml(value)}
                </button>
            `).join("");
        }

        container.classList.toggle("hidden", false);
    }

    function bindSearchableField(inputId, listId, valuesOrGetter, onSelect, placeholder) {
        const input = document.getElementById(inputId);
        const list = document.getElementById(listId);
        if (!input || !list) return;

        input.addEventListener("focus", () => renderOptionList(listId, inputId, valuesOrGetter, onSelect, placeholder));
        input.addEventListener("input", () => {
            renderOptionList(listId, inputId, valuesOrGetter, onSelect, placeholder);
        });

        document.addEventListener("click", event => {
            if (!list.contains(event.target) && event.target !== input) {
                list.classList.add("hidden");
            }
        });

        list.addEventListener("click", event => {
            const button = event.target.closest("[data-value]");
            if (!button) return;
            const value = button.dataset.value;
            input.value = value;
            list.classList.add("hidden");
            onSelect(value);
        });
    }

    function resolveMahakCityId(provinceName, cityName) {
        const province = normalizeLocationName(provinceName);
        const city = normalizeLocationName(cityName);
        const match = state.mahakRegions.find(region =>
            region.provinceName === province &&
            region.cityName === city);
        return match?.cityId || null;
    }

    async function initProvinceCitySelectors() {
        const provinceInput = document.getElementById("checkout-address-state");
        const cityInput = document.getElementById("checkout-address-city");
        if (!provinceInput || !cityInput) return;
        if (provinceInput.dataset.mahakBound === "true") return;
        provinceInput.dataset.mahakBound = "true";

        const regionResult = await window.addressService.getMahakRegions();
        if (regionResult.success && Array.isArray(regionResult.data) && regionResult.data.length) {
            state.mahakRegions = regionResult.data
                .map(region => ({
                    ...region,
                    provinceName: normalizeLocationName(region.provinceName),
                    cityName: normalizeLocationName(region.cityName)
                }))
                .filter(region =>
                    Number(region.cityId) > 0 &&
                    region.provinceName &&
                    region.cityName &&
                    !region.cityName.startsWith("استان "));
        } else {
            window.utils?.showToast?.("لیست استان و شهر محک دریافت نشد", "warning");
        }

        state.mahakRegions = state.mahakRegions
            .filter(region => !region.cityName.startsWith("\u0627\u0633\u062A\u0627\u0646 "));

        const provinceListId = "checkout-mahak-province-options";
        const cityListId = "checkout-mahak-city-options";
        let provinceList = document.getElementById(provinceListId);
        if (!provinceList) {
            provinceList = document.createElement("datalist");
            provinceList.id = provinceListId;
            document.body.appendChild(provinceList);
        }

        let cityList = document.getElementById(cityListId);
        if (!cityList) {
            cityList = document.createElement("datalist");
            cityList.id = cityListId;
            document.body.appendChild(cityList);
        }

        provinceInput.setAttribute("list", provinceListId);
        cityInput.setAttribute("list", cityListId);

        const provinceNames = [...new Set(state.mahakRegions.map(region => region.provinceName).filter(Boolean))]
            .sort((a, b) => a.localeCompare(b, "fa"));
        provinceList.innerHTML = provinceNames.map(name => `<option value="${escapeHtml(name)}"></option>`).join("");

        const getSelectedProvince = () => {
            const province = normalizeLocationName(provinceInput.value);
            return provinceNames.includes(province) ? province : "";
        };

        const refreshCities = () => {
            const selectedProvince = getSelectedProvince();
            const cities = state.mahakRegions
                .filter(region => region.provinceName === selectedProvince)
                .sort((a, b) => a.cityName.localeCompare(b.cityName, "fa"));

            cityInput.disabled = !selectedProvince;
            cityInput.placeholder = selectedProvince ? "شهر را انتخاب کنید" : "ابتدا استان را انتخاب کنید";
            if (!selectedProvince) {
                cityInput.value = "";
            }

            cityList.innerHTML = cities
                .map(region => `<option value="${escapeHtml(region.cityName)}" data-city-id="${region.cityId}"></option>`)
                .join("");

            state.province = selectedProvince;
            state.city = normalizeLocationName(cityInput.value);
            state.mahakCityId = resolveMahakCityId(selectedProvince, cityInput.value);
        };

        provinceInput.addEventListener("input", () => {
            state.mahakCityId = null;
            cityInput.value = "";
            refreshCities();
        });

        cityInput.addEventListener("input", () => {
            state.city = normalizeLocationName(cityInput.value);
            state.mahakCityId = resolveMahakCityId(state.province, state.city);
        });

        refreshCities();
        return;

        const provinces = () => [...new Set(state.mahakRegions.map(region => region.provinceName).filter(Boolean))]
            .sort((a, b) => a.localeCompare(b, "fa"));
        const selectedProvince = () => {
            const province = normalizeLocationName(provinceInput.value);
            return provinces().includes(province) ? province : "";
        };
        const citiesForSelectedProvince = () => state.mahakRegions
            .filter(region => region.provinceName === selectedProvince())
            .map(region => region.cityName)
            .sort((a, b) => a.localeCompare(b, "fa"));

        bindSearchableField("checkout-address-state", "checkout-state-options", provinces, (province) => {
            state.province = normalizeLocationName(province);
            state.city = "";
            state.mahakCityId = null;
            cityInput.value = "";
            cityInput.disabled = false;
            cityInput.placeholder = "جستجوی شهر";
            cityInput.focus();
        }, "استانی پیدا نشد");

        bindSearchableField("checkout-address-city", "checkout-city-options", citiesForSelectedProvince, (city) => {
            state.city = normalizeLocationName(city);
            state.mahakCityId = resolveMahakCityId(state.province, city);
        }, "شهری پیدا نشد");

        provinceInput.addEventListener("input", () => {
            const province = selectedProvince();
            if (!province || province !== state.province) {
                state.province = "";
                state.city = "";
                state.mahakCityId = null;
                cityInput.value = "";
                cityInput.disabled = true;
                cityInput.placeholder = "ابتدا استان را انتخاب کنید";
            }
        });

        cityInput.addEventListener("input", () => {
            state.city = normalizeLocationName(cityInput.value);
            state.mahakCityId = resolveMahakCityId(state.province, state.city);
        });

        cityInput.disabled = true;
        cityInput.placeholder = "ابتدا استان را انتخاب کنید";
    }
    function renderSelectedAddress(address) {
        const addressDetails = document.getElementById("checkout-address-details");
        const changeButton = document.getElementById("change-address-btn");

        if (!address) {
            if (addressDetails) {
                addressDetails.innerHTML = `
                    <span class="text-red-500">لطفا آدرس را وارد کنید</span>
                    <button type="button" id="checkout-add-address-inline" class="text-primary font-medium mt-2 inline-block text-start">افزودن آدرس جدید</button>
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
                    <button type="button" id="checkout-show-address-form" class="inline-block mt-4 px-4 py-2 rounded-lg bg-primary text-white">افزودن آدرس جدید</button>
                </div>
            `;
        } else {
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

        const formSection = document.getElementById("checkout-address-form-section");
        if (formSection) {
            formSection.classList.toggle("hidden", !state.isAddressFormOpen && state.addresses.length > 0);
        }
    }

    function openAddressModal() {
        state.isAddressFormOpen = state.addresses.length === 0;
        renderAddressModal();
        document.getElementById("addressSelectionModal")?.classList.remove("hidden");
        if (state.isAddressFormOpen) {
            document.getElementById("checkout-address-form-section")?.scrollIntoView({ behavior: "smooth", block: "start" });
        }
    }

    function closeAddressModal() {
        document.getElementById("addressSelectionModal")?.classList.add("hidden");
        state.isAddressFormOpen = false;
        renderAddressModal();
    }

    function openAddressForm() {
        state.isAddressFormOpen = true;
        renderAddressModal();
        document.getElementById("checkout-address-form-section")?.scrollIntoView({ behavior: "smooth", block: "start" });
    }

    function resetAddressForm() {
        const form = document.getElementById("checkout-address-form");
        form?.reset();
    }

    async function saveAddressFromCheckout() {
        if (state.isSavingAddress) return;

        const title = document.getElementById("checkout-address-title")?.value || "";
        const stateValue = document.getElementById("checkout-address-state")?.value || "";
        const city = document.getElementById("checkout-address-city")?.value || "";
        const addressLine1 = document.getElementById("checkout-address-line1")?.value || "";
        const postalCode = document.getElementById("checkout-address-postal")?.value || "";
        const firstName = document.getElementById("checkout-address-firstname")?.value || "";
        const lastName = document.getElementById("checkout-address-lastname")?.value || "";
        const phoneNumber = document.getElementById("checkout-address-phone")?.value || "";
        const isDefault = document.getElementById("checkout-address-default")?.checked || false;

        const addressData = {
            title: title.trim(),
            firstName: firstName.trim(),
            lastName: lastName.trim() || "-",
            addressLine1: addressLine1.trim(),
            city: city.trim(),
            state: stateValue.trim(),
            postalCode: postalCode.trim(),
            phoneNumber: phoneNumber.trim(),
            mahakCityId: state.mahakCityId || resolveMahakCityId(stateValue, city),
            country: "Iran",
            isDefault,
            isShippingAddress: true,
            isBillingAddress: isDefault
        };

        const validation = window.addressService.validateAddressData(addressData);
        if (!validation.isValid) {
            window.utils?.showToast?.(Object.values(validation.errors)[0] || "لطفا اطلاعات آدرس را کامل کنید", "error");
            return;
        }

        const btn = document.getElementById("checkout-save-address-btn");
        const originalText = btn?.textContent || "ذخیره آدرس";

        try {
            state.isSavingAddress = true;
            if (btn) {
                btn.textContent = "در حال ذخیره...";
                btn.disabled = true;
            }

            const response = await window.addressService.createAddress(addressData);
            if (!response.success) {
                throw new Error(response.error || "خطا در ذخیره آدرس");
            }

            window.utils?.showToast?.("آدرس جدید با موفقیت اضافه شد", "success");
            await loadAddresses();
            state.isAddressFormOpen = false;
            renderAddressModal();
            resetAddressForm();
        } catch (error) {
            window.logger?.error("Error saving checkout address:", error);
            window.utils?.showToast?.(error.message || "خطا در ذخیره آدرس", "error");
        } finally {
            state.isSavingAddress = false;
            if (btn) {
                btn.textContent = originalText;
                btn.disabled = false;
            }
        }
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
        initProvinceCitySelectors();
        document.getElementById("change-address-btn")?.addEventListener("click", openAddressModal);
        document.getElementById("checkout-save-address-btn")?.addEventListener("click", saveAddressFromCheckout);
        document.getElementById("checkout-address-cancel-btn")?.addEventListener("click", () => {
            state.isAddressFormOpen = false;
            renderAddressModal();
        });
        
        document.querySelectorAll("[data-modal-close]").forEach(btn => {
            btn.addEventListener("click", closeAddressModal);
        });

        document.getElementById("address-modal-backdrop")?.addEventListener("click", closeAddressModal);
        document.getElementById("address-modal-close")?.addEventListener("click", closeAddressModal);
        document.getElementById("addressSelectionModal")?.addEventListener("click", event => {
            if (event.target.closest("#checkout-show-address-form")) {
                event.preventDefault();
                openAddressForm();
                return;
            }

            if (event.target.closest("#checkout-add-address-inline")) {
                event.preventDefault();
                openAddressModal();
                return;
            }

            if (event.target.closest("#checkout-address-cancel-btn")) {
                event.preventDefault();
                state.isAddressFormOpen = false;
                renderAddressModal();
            }
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
                    shippingAddressId: getCheckoutAddressId(state.selectedAddress),
                    shippingAddressText: getCheckoutAddressPayload(state.selectedAddress)
                });

                if (!checkoutResult.success || !checkoutResult.data?.isSuccess || !checkoutResult.data?.data?.order?.id) {
                    throw new Error(checkoutResult.data?.errorMessage || "خطا در ثبت سفارش");
                }

                const orderId = checkoutResult.data.data.order.id;

                // Step 2: Initiate Payment via API using OrderId
                const paymentResult = await window.apiClient.post('/Payment/initiate', {
                    orderId: orderId,
                    gateway: "ZarinPal"
                });

                if (paymentResult.success && paymentResult.data?.isSuccess && paymentResult.data?.data?.paymentUrl) {
                    // Update cart count UI proactively before redirect
                    if (window.cartService && typeof window.cartService.updateCartCount === "function") {
                        await window.cartService.updateCartCount();
                    }
                    window.location.href = paymentResult.data.data.paymentUrl;
                } else {
                    throw new Error(paymentResult.data?.errorMessage || "خطا در اتصال به درگاه پرداخت");
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
            if (!state.addresses.length) {
                openAddressModal();
            }
            window.cartService?.updateCartCount?.();
            window.cartService?.renderCartOffcanvas?.();
        } catch (error) {
            window.logger?.error("Checkout initialization failed:", error);
            window.utils?.showToast?.(error.message || "خطا در دریافت اطلاعات تسویه حساب", "error");
        }
    }

    document.addEventListener("DOMContentLoaded", init);
})();
