(function () {
  "use strict";

  const PARTIAL_BASE_PATH = "partials/";

  function getPartialName(element) {
    return element.getAttribute("data-layout-partial");
  }

  function loadPartial(name) {
    const request = new XMLHttpRequest();
    request.open("GET", PARTIAL_BASE_PATH + name + ".html", false);
    request.send(null);

    if (
      (request.status >= 200 && request.status < 300) ||
      (request.status === 0 && request.responseText)
    ) {
      return request.responseText;
    }

    throw new Error("Could not load layout partial: " + name);
  }

  function applyPartials() {
    const placeholders = document.querySelectorAll("[data-layout-partial]");

    placeholders.forEach(function (placeholder) {
      const partialName = getPartialName(placeholder);
      if (!partialName) return;

      try {
        placeholder.outerHTML = loadPartial(partialName);
      } catch (error) {
        if (window.logger && typeof window.logger.error === "function") {
          window.logger.error(error.message, error);
        } else {
          console.error(error);
        }
      }
    });

    window.dispatchEvent(new CustomEvent("layout:ready"));
  }

  applyPartials();

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", applyPartials, { once: true });
  }
})();
