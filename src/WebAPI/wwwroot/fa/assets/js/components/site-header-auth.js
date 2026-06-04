(function () {
  "use strict";

  const HOME_PAGE = "index.html";
  const PROFILE_PAGE = "user-panel-index.html";
  const LOGIN_PAGE = "login.html";
  const AUTH_PAGES = new Set(["login.html", "register.html", "forgot-password.html"]);

  function getCurrentPageName() {
    return window.location.pathname.split("/").pop() || HOME_PAGE;
  }

  function getReturnPath() {
    if (AUTH_PAGES.has(getCurrentPageName())) return HOME_PAGE;
    return (
      window.location.pathname.split("/").pop() +
      window.location.search +
      window.location.hash
    ) || HOME_PAGE;
  }

  function getLoginUrl() {
    return LOGIN_PAGE + "?returnUrl=" + encodeURIComponent(getReturnPath());
  }

  function isAuthenticated() {
    if (window.authService && typeof window.authService.isAuthenticated === "function") {
      return window.authService.isAuthenticated();
    }

    return !!localStorage.getItem("accessToken");
  }

  function updateHeaderAuthLink() {
    const links = document.querySelectorAll("[data-header-auth-link]");
    if (links.length === 0) return;

    const authenticated = isAuthenticated();
    links.forEach(function (link) {
      const text = link.querySelector("[data-header-auth-text]");
      if (authenticated) {
        link.href = PROFILE_PAGE;
        link.setAttribute("aria-label", "پروفایل کاربری");
        if (text) text.textContent = "پروفایل کاربری";
      } else {
        link.href = getLoginUrl();
        link.setAttribute("aria-label", "ورود / ثبت نام");
        if (text) text.textContent = "ورود / ثبت نام";
      }
    });
  }

  function storeReturnUrlOnLoginClick(event) {
    const link = event.target.closest("[data-header-auth-link]");
    if (!link || isAuthenticated()) return;
    localStorage.setItem("intendedUrl", getReturnPath());
  }

  function init() {
    updateHeaderAuthLink();
    document.addEventListener("click", storeReturnUrlOnLoginClick);
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init, { once: true });
  } else {
    init();
  }

  window.addEventListener("layout:ready", updateHeaderAuthLink);
  window.addEventListener("auth:login", updateHeaderAuthLink);
  window.addEventListener("auth:logout", updateHeaderAuthLink);
  window.addEventListener("storage", function (event) {
    if (event.key === "accessToken") updateHeaderAuthLink();
  });
})();
