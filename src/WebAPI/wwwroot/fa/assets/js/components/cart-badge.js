(function () {
  const getStorageKey = () =>
    (window.config && window.config.storage && window.config.storage.cartItems) ||
    "cartItems";

  const readItems = (key) => {
    try {
      const raw = localStorage.getItem(key);
      const value = raw ? JSON.parse(raw) : [];
      return Array.isArray(value) ? value : [];
    } catch (_error) {
      return [];
    }
  };

  const getCartItemCount = () => {
    const guestItems = readItems(getStorageKey());
    const legacyItems = guestItems.length ? guestItems : readItems("cart");

    return legacyItems.reduce(
      (sum, item) => sum + (Number(item.quantity) || 0),
      0,
    );
  };

  const updateCartBadges = () => {
    const count = getCartItemCount();

    document.querySelectorAll("#cartCount, .cart-count, [data-cart-count]").forEach((badge) => {
      badge.style.backgroundColor = "#f97316";
      badge.style.color = "#ffffff";
      badge.style.top = "-18px";
      badge.style.insetInlineEnd = "-24px";
      badge.style.transform = "none";
      badge.textContent = count;
      const isMobileNavBadge = Boolean(badge.closest(".site-mobile-nav"));
      badge.classList.toggle("hidden", count === 0 && !isMobileNavBadge);
    });
  };

  document.addEventListener("DOMContentLoaded", updateCartBadges);
  window.addEventListener("cart-updated", updateCartBadges);
  window.addEventListener("storage", (event) => {
    if (event.key === getStorageKey() || event.key === "cart") {
      updateCartBadges();
    }
  });

  window.updateCartBadges = updateCartBadges;
})();
