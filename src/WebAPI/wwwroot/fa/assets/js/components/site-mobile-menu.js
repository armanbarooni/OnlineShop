(function () {
  "use strict";

  function initializeMobileMenu() {
    var offcanvasList = document.querySelector(
      "#offcanvas-right nav ul.space-y-2.text-sm",
    );
    if (!offcanvasList) return;

    offcanvasList.innerHTML =
      '<li class="bg-ul-f7 border border-gray-100 dark:bg-zinc-800 dark:text-white p-2">' +
      '<a href="/" class="block">صفحه اصلی</a>' +
      "</li>" +
      '<li class="bg-ul-f7 border border-gray-100 dark:bg-zinc-800 dark:text-white p-2">' +
      '<button id="manual-category-button" class="flex justify-between w-full text-start" aria-expanded="false" aria-controls="manual-category-list">' +
      '<span class="flex-1">دسته‌بندی</span>' +
      '<svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 transition-transform transform" id="icon-manual-category-list" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">' +
      '<path fill-rule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clip-rule="evenodd" />' +
      "</svg>" +
      "</button>" +
      '<ul id="manual-category-list" class="hidden space-y-2 mt-2 pr-4">' +
      '<li class="px-6 py-2 border-b border-gray-200"><a href="shop.html?search=%D8%B4%D9%84%D9%88%D8%A7%D8%B1" class="block">شلوار</a></li>' +
      '<li class="px-6 py-2 border-b border-gray-200"><a href="shop.html?search=%D8%AA%DB%8C%D8%B4%D8%B1%D8%AA" class="block">تیشرت</a></li>' +
      "</ul>" +
      "</li>";

    var categoryButton = document.getElementById("manual-category-button");
    var categoryMenu = document.getElementById("manual-category-list");
    var categoryIcon = document.getElementById("icon-manual-category-list");
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

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initializeMobileMenu);
  } else {
    initializeMobileMenu();
  }
})();
