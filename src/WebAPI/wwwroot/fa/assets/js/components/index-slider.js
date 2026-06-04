(function () {
  "use strict";

  if (typeof window.Swiper === "undefined") return;

  const slider = document.querySelector(".default-carousel");
  if (!slider || slider.classList.contains("swiper-initialized")) return;

  const swiper = new Swiper(slider, {
    loop: true,
    pagination: {
      el: slider.querySelector(".swiper-pagination"),
      clickable: true,
    },
    navigation: {
      nextEl: slider.querySelector(".swiper-button-next"),
      prevEl: slider.querySelector(".swiper-button-prev"),
    },
  });
  slider.setAttribute("data-component-ready", "index-slider");

  const customSwiperNext = document.querySelector(".custom-swiper-next");
  const customSwiperPrev = document.querySelector(".custom-swiper-prev");

  if (customSwiperNext) {
    customSwiperNext.addEventListener("click", function () {
      swiper.slideNext();
    });
  }

  if (customSwiperPrev) {
    customSwiperPrev.addEventListener("click", function () {
      swiper.slidePrev();
    });
  }
})();
