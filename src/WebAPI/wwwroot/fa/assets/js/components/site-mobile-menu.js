(function () {
  "use strict";

  var DEPENDENCY_RETRY_DELAY_MS = 150;
  var DEPENDENCY_MAX_ATTEMPTS = 40;

  function delay(ms) {
    return new Promise(function (resolve) {
      setTimeout(resolve, ms);
    });
  }

  async function waitForCategoryService() {
    for (var attempt = 0; attempt < DEPENDENCY_MAX_ATTEMPTS; attempt += 1) {
      if (window.categoryService && typeof window.categoryService.getAllCategories === "function") {
        return true;
      }

      await delay(DEPENDENCY_RETRY_DELAY_MS);
    }

    return false;
  }

  function normalizeCategories(payload) {
    if (Array.isArray(payload)) {
      return payload;
    }

    if (payload && Array.isArray(payload.data)) {
      return payload.data;
    }

    return [];
  }

  function escapeHtml(value) {
    return String(value || "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function renderCategoryList(offcanvasList, categories) {
    var visibleCategories = categories.slice(0, 12);

    if (visibleCategories.length === 0) {
      offcanvasList.innerHTML =
        '<li class="bg-ul-f7 border border-gray-100 dark:bg-zinc-800 dark:text-white p-3 text-sm">دسته‌بندی‌ای برای نمایش وجود ندارد.</li>';
      return;
    }

    var html =
      '<li class="bg-ul-f7 border border-gray-100 dark:bg-zinc-800 dark:text-white p-2">' +
      '<a href="index.html" class="block">صفحه اصلی</a>' +
      "</li>";

    html +=
      '<li class="bg-ul-f7 border border-gray-100 dark:bg-zinc-800 dark:text-white p-2">' +
      '<button id="mobile-category-button" class="flex justify-between w-full text-start" aria-expanded="true" aria-controls="mobile-category-list">' +
      '<span class="flex-1">دسته‌بندی</span>' +
      '<svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 transition-transform transform rotate-180" id="icon-mobile-category-list" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">' +
      '<path fill-rule="evenodd" d="M5.293 7.293a1 1 0 0 1 1.414 0L10 10.586l3.293-3.293a1 1 0 1 1 1.414 1.414l-4 4a1 1 0 0 1-1.414 0l-4-4a1 1 0 0 1 0-1.414z" clip-rule="evenodd" />' +
      "</svg>" +
      "</button>" +
      '<ul id="mobile-category-list" class="space-y-2 mt-2 pr-4">';

    visibleCategories.forEach(function (category) {
      html +=
        '<li class="px-2 py-2 border-b border-gray-200 dark:border-zinc-700">' +
        '<a href="shop.html?category=' +
        encodeURIComponent(category.id) +
        '" class="block">' +
        escapeHtml(category.name || "بدون نام") +
        "</a>" +
        "</li>";
    });

    html += "</ul></li>";

    offcanvasList.innerHTML = html;

    var categoryButton = document.getElementById("mobile-category-button");
    var categoryMenu = document.getElementById("mobile-category-list");
    var categoryIcon = document.getElementById("icon-mobile-category-list");

    if (categoryButton && categoryMenu) {
      categoryButton.addEventListener("click", function (event) {
        event.preventDefault();
        event.stopPropagation();
        categoryMenu.classList.toggle("hidden");
        categoryButton.setAttribute(
          "aria-expanded",
          String(!categoryMenu.classList.contains("hidden")),
        );
        if (categoryIcon) {
          categoryIcon.classList.toggle("rotate-180");
        }
      });
    }
  }

  async function initializeMobileMenu() {
    var offcanvasList = document.querySelector(
      "#offcanvas-right nav ul.space-y-2.text-sm",
    );
    if (!offcanvasList) return;

    var hasService = await waitForCategoryService();
    if (!hasService) {
      offcanvasList.setAttribute("data-component-ready", "site-mobile-menu-fallback");
      return;
    }

    try {
      var result = await window.categoryService.getAllCategories();
      var categories = normalizeCategories(result && result.data ? result.data : result);
      renderCategoryList(offcanvasList, categories);
      offcanvasList.setAttribute("data-component-ready", "site-mobile-menu");
    } catch (error) {
      if (window.logger && typeof window.logger.error === "function") {
        window.logger.error("Error rendering mobile categories:", error);
      }
      offcanvasList.setAttribute("data-component-ready", "site-mobile-menu-error");
    }
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initializeMobileMenu);
  } else {
    initializeMobileMenu();
  }
})();
