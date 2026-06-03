(function () {
  "use strict";

  const inactiveDesktop =
    "block px-4 py-3 text-sm font-medium text-gray-900 rounded-lg hover:bg-gray-100 dark:text-white dark:hover:bg-gray-700";
  const activeDesktop =
    "flex items-center px-4 py-3 text-sm font-medium rounded-lg bg-primary-50 dark:bg-primary-900/30 text-primary-600 dark:text-primary-400";
  const inactiveMobile =
    "flex items-center px-4 py-3 text-sm font-medium rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white";
  const activeMobile =
    "flex items-center px-4 py-3 text-sm font-medium rounded-lg bg-primary/10 dark:bg-primary/20 text-primary dark:text-primary-dark";

  const pageToNavKey = [
    [/^user-panel-index\.html$/, "index"],
    [/^user-panel-profile\.html$/, "profile"],
    [/^user-panel-change-password\.html$/, "security"],
    [/^user-panel-favorite\.html$/, "favorite"],
    [/^user-panel-last-viewd\.html$/, "last-viewed"],
    [/^user-panel-order/, "order"],
    [/^user-panel-address\.html$/, "address"],
    [/^user-panel-edit-address\.html$/, "address"],
    [/^user-panel-wallet\.html$/, "wallet"],
    [/^user-panel-increase-money\.html$/, "wallet"],
    [/^user-panel-transfer-money\.html$/, "wallet"],
    [/^user-panel-ticket/, "ticket"],
    [/^user-panel-comment\.html$/, "comment"],
    [/^user-panel-edit-comment\.html$/, "comment"],
    [/^user-panel-discount\.html$/, "discount"],
    [/^user-panel-discous\.html$/, "discount"],
    [/^user-panel-get-discount\.html$/, "discount"],
    [/^user-panel-gift-cart\.html$/, "discount"],
    [/^user-panel-activiti\.html$/, "notification"],
    [/^user-panel-site-notification\.html$/, "notification"],
  ];

  function getActiveKey() {
    const page = (window.location.pathname.split("/").pop() || "user-panel-index.html").toLowerCase();
    const match = pageToNavKey.find(function ([pattern]) {
      return pattern.test(page);
    });
    return match ? match[1] : "";
  }

  function setIndicator(link, isActive) {
    link.querySelectorAll("[data-user-panel-active-indicator]").forEach(function (indicator) {
      indicator.remove();
    });

    if (!isActive) return;

    const indicator = document.createElement("span");
    indicator.setAttribute("data-user-panel-active-indicator", "true");
    indicator.className = "ms-auto w-2 h-2 rounded-full bg-primary-600 dark:bg-primary-400";
    link.appendChild(indicator);
  }

  function applyActiveState() {
    const activeKey = getActiveKey();
    if (!activeKey) return;

    document.querySelectorAll("[data-user-panel-nav]").forEach(function (nav) {
      const isMobile = nav.getAttribute("data-user-panel-nav") === "mobile";
      nav.querySelectorAll("[data-user-panel-nav-item]").forEach(function (link) {
        const isActive = link.getAttribute("data-user-panel-nav-item") === activeKey;
        link.className = isActive
          ? isMobile ? activeMobile : activeDesktop
          : isMobile ? inactiveMobile : inactiveDesktop;
        setIndicator(link, isActive);
      });
    });
  }

  window.UserPanelNav = { applyActiveState: applyActiveState };
  window.addEventListener("layout:ready", applyActiveState);

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", applyActiveState);
  } else {
    applyActiveState();
  }
})();
