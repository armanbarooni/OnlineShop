(function () {
  "use strict";

  let initialized = false;

  function initIndexSlider() {
    if (initialized) return;
    if (typeof window.Swiper === "undefined") return;

    const slider = document.querySelector(".default-carousel");
    if (!slider || slider.classList.contains("swiper-initialized")) return;

    const nextButton =
      slider.querySelector(".custom-swiper-next") ||
      slider.querySelector(".swiper-button-next");
    const prevButton =
      slider.querySelector(".custom-swiper-prev") ||
      slider.querySelector(".swiper-button-prev");

    const swiper = new Swiper(slider, {
      loop: true,
      initialSlide: 0,
      speed: 600,
      grabCursor: true,
      allowTouchMove: true,
      simulateTouch: true,
      watchOverflow: true,
      observer: true,
      observeParents: true,
      resizeObserver: true,
      pagination: {
        el: slider.querySelector(".swiper-pagination"),
        clickable: true,
      },
      navigation: {
        nextEl: nextButton,
        prevEl: prevButton,
      },
    });
    swiper.slideToLoop(0, 0, false);

    slider.setAttribute("data-component-ready", "index-slider");
    slider.swiper = swiper;
    initialized = true;
  }

  function scheduleInit() {
    window.requestAnimationFrame(initIndexSlider);
  }

  if (document.readyState === "complete") {
    scheduleInit();
  } else {
    window.addEventListener("layout:ready", scheduleInit, { once: true });
    window.addEventListener("components:ready", scheduleInit, { once: true });
    window.addEventListener("DOMContentLoaded", scheduleInit, { once: true });
    window.addEventListener("load", scheduleInit, { once: true });
  }
})();
