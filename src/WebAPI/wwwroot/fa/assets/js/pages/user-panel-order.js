const OrderManager = {
  state: {
    currentPage: 1,
    pageSize: 10,
    filters: {
      orderStatus: null,
    },
  },

  init: async function () {
    this.initUI();
    await this.checkAuthAndLoad();
  },

  initUI: function () {
    if (window.headerComponent) window.headerComponent.init();
    if (window.utils) window.utils.initDarkMode("dark-mode-toggle");

    const handleLogout = function (event) {
      event.preventDefault();
      if (confirm("آیا مطمئن هستید که می‌خواهید خارج شوید؟")) {
        if (window.authService) window.authService.logout();
        else window.location.href = "login.html";
      }
    };

    document
      .querySelectorAll("#logoutButton")
      .forEach((btn) => btn.addEventListener("click", handleLogout));

    document.querySelectorAll("[data-filter]").forEach((btn) => {
      btn.addEventListener("click", (event) => this.handleFilterClick(event));
    });
  },

  checkAuthAndLoad: async function () {
    if (window.authService && !window.authService.isAuthenticated()) {
      window.location.href = "login.html";
      return;
    }

    await this.loadUserProfile();
    await this.loadOrders();
  },

  loadUserProfile: async function () {
    if (!window.userProfileService) return;

    try {
      const profile = await window.userProfileService.getUserProfile();
      if (!profile.success || !profile.data) return;

      const fullName =
        `${profile.data.firstName || ""} ${profile.data.lastName || ""}`.trim() ||
        "کاربر گرامی";

      document.querySelectorAll('[data-user-name="true"]').forEach((el) => {
        el.textContent = fullName;
      });

      if (profile.data.profilePictureUrl) {
        document
          .querySelectorAll('img[alt="پروفایل کاربر"]')
          .forEach((img) => {
            img.src = profile.data.profilePictureUrl;
          });
      }
    } catch (error) {
      console.error("Profile load error", error);
    }
  },

  loadOrders: async function () {
    const tbody = document.querySelector("#orders-list-view tbody");
    if (!tbody) return;

    tbody.innerHTML =
      '<tr><td colspan="5" class="px-6 py-4 text-center text-sm text-gray-500">در حال بارگذاری...</td></tr>';

    try {
      const result = await window.orderService.searchOrders(
        {},
        {
          pageNumber: this.state.currentPage,
          pageSize: this.state.pageSize,
          orderStatus: this.state.filters.orderStatus,
          sortBy: "OrderDate",
          sortDescending: true,
        },
      );

      if (!result.success) {
        this.renderError(result.error || "خطا در بارگذاری سفارش‌ها");
        return;
      }

      const pageData = this.normalizePagedResult(result.data);
      this.renderOrders(pageData.items);
      this.renderPagination(pageData.totalCount);
    } catch (error) {
      console.error("Orders load error:", error);
      this.renderError("خطا در ارتباط با سرور");
    }
  },

  normalizePagedResult: function (data) {
    const payload = data && data.data ? data.data : data;

    if (Array.isArray(payload)) {
      return { items: payload, totalCount: payload.length };
    }

    if (!payload || typeof payload !== "object") {
      return { items: [], totalCount: 0 };
    }

    const items = payload.items || payload.data || payload.results || [];
    const normalizedItems = Array.isArray(items) ? items : [];
    const totalCount =
      payload.totalCount ||
      payload.totalItems ||
      payload.count ||
      normalizedItems.length;

    return { items: normalizedItems, totalCount };
  },

  renderError: function (message) {
    const tbody = document.querySelector("#orders-list-view tbody");
    if (tbody) {
      tbody.innerHTML = `<tr><td colspan="5" class="px-6 py-4 text-center text-sm text-red-500">${message}</td></tr>`;
    }

    const countContainer = document.getElementById("orders-count");
    if (countContainer) countContainer.textContent = "خطا در دریافت سفارش‌ها";
  },

  renderOrders: function (orders) {
    const tbody = document.querySelector("#orders-list-view tbody");
    if (!tbody) return;

    if (!orders || orders.length === 0) {
      tbody.innerHTML =
        '<tr><td colspan="5" class="px-6 py-4 text-center text-sm text-gray-500">هیچ سفارشی یافت نشد.</td></tr>';
      return;
    }

    tbody.innerHTML = orders
      .map((order) => {
        const status = order.orderStatus || order.status;
        const total = order.finalAmount ?? order.totalAmount ?? 0;

        return `
          <tr class="hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors">
            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-white">
              ${order.orderNumber || order.id || "-"}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
              ${this.formatDate(order.orderDate || order.createdAt)}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 dark:text-white font-bold">
              ${this.formatMoney(total)}
            </td>
            <td class="px-6 py-4 whitespace-nowrap">
              <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${this.getStatusClass(status)}">
                ${this.getStatusText(status)}
              </span>
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
              <button onclick="OrderManager.showDetails('${order.id}')" class="text-primary hover:text-primary-dark ml-2">جزئیات</button>
              ${this.canTrack(status) ? `<button onclick="OrderManager.trackOrder('${order.id}')" class="text-blue-600 hover:text-blue-800 ml-2">رهگیری</button>` : ""}
            </td>
          </tr>
        `;
      })
      .join("");
  },

  formatDate: function (value) {
    if (!value) return "-";
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "-";
    return date.toLocaleDateString("fa-IR");
  },

  formatMoney: function (value) {
    return `${new Intl.NumberFormat("fa-IR").format(Number(value) || 0)} ریال`;
  },

  getStatusClass: function (status) {
    switch (status) {
      case "Pending":
        return "bg-yellow-100 text-yellow-800";
      case "Processing":
        return "bg-blue-100 text-blue-800";
      case "Shipped":
        return "bg-purple-100 text-purple-800";
      case "Delivered":
        return "bg-green-100 text-green-800";
      case "Cancelled":
        return "bg-red-100 text-red-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  },

  getStatusText: function (status) {
    const map = {
      Pending: "در انتظار پرداخت",
      Processing: "در حال پردازش",
      Shipped: "ارسال شده",
      Delivered: "تحویل شده",
      Cancelled: "لغو شده",
      Returned: "مرجوع شده",
    };

    return map[status] || status || "نامشخص";
  },

  canTrack: function (status) {
    return ["Shipped", "Delivered"].includes(status);
  },

  renderPagination: function (totalItems) {
    const totalPages = Math.ceil(totalItems / this.state.pageSize);
    const container = document.getElementById("pagination");
    const countContainer = document.getElementById("orders-count");

    if (countContainer) {
      if (totalItems <= 0) {
        countContainer.textContent = "سفارشی یافت نشد";
      } else {
        countContainer.textContent = `نمایش ${(this.state.currentPage - 1) * this.state.pageSize + 1} تا ${Math.min(this.state.currentPage * this.state.pageSize, totalItems)} از ${totalItems} سفارش`;
      }
    }

    if (!container) return;
    if (totalPages <= 1) {
      container.innerHTML = "";
      return;
    }

    let html = "";
    html += `<button onclick="OrderManager.changePage(${this.state.currentPage - 1})" ${this.state.currentPage === 1 ? 'disabled class="opacity-50 cursor-not-allowed px-3 py-1 border rounded"' : 'class="px-3 py-1 border rounded hover:bg-gray-100 dark:hover:bg-gray-700"'}>قبلی</button>`;

    for (let i = 1; i <= totalPages; i++) {
      html +=
        i === this.state.currentPage
          ? `<button class="px-3 py-1 border rounded bg-primary text-white">${i}</button>`
          : `<button onclick="OrderManager.changePage(${i})" class="px-3 py-1 border rounded hover:bg-gray-100 dark:hover:bg-gray-700">${i}</button>`;
    }

    html += `<button onclick="OrderManager.changePage(${this.state.currentPage + 1})" ${this.state.currentPage === totalPages ? 'disabled class="opacity-50 cursor-not-allowed px-3 py-1 border rounded"' : 'class="px-3 py-1 border rounded hover:bg-gray-100 dark:hover:bg-gray-700"'}>بعدی</button>`;
    container.innerHTML = html;
  },

  changePage: function (page) {
    if (page < 1) return;
    this.state.currentPage = page;
    this.loadOrders();
  },

  handleFilterClick: function (event) {
    const btn = event.target.closest("button");
    if (!btn) return;

    const status = btn.getAttribute("data-filter");
    this.state.filters.orderStatus = status === "all" || !status ? null : status;
    this.state.currentPage = 1;
    this.loadOrders();
  },

  showDetails: async function (orderId) {
    const listEl = document.getElementById("orders-list-view");
    const detailsEl = document.getElementById("order-details-view");
    const contentEl = document.getElementById("order-details-content");

    if (listEl) listEl.classList.add("hidden");
    if (detailsEl) detailsEl.classList.remove("hidden");
    if (contentEl) contentEl.innerHTML = '<div class="text-center py-8">در حال بارگذاری جزئیات...</div>';

    try {
      const result = await window.orderService.getOrderById(orderId);
      if (result.success && result.data) {
        this.renderDetails(result.data);
      } else if (contentEl) {
        contentEl.innerHTML = '<div class="text-red-500 text-center py-8">خطا در دریافت جزئیات سفارش</div>';
      }
    } catch (error) {
      console.error(error);
      if (contentEl) contentEl.innerHTML = '<div class="text-red-500 text-center py-8">خطا در ارتباط</div>';
    }
  },

  renderDetails: function (order) {
    const contentEl = document.getElementById("order-details-content");
    if (!contentEl) return;

    contentEl.innerHTML = `
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <div>
          <p class="text-sm text-gray-500 mb-1">شماره سفارش</p>
          <p class="font-bold text-lg dark:text-white">${order.orderNumber || order.id || "-"}</p>
        </div>
        <div>
          <p class="text-sm text-gray-500 mb-1">تاریخ ثبت</p>
          <p class="font-bold text-lg dark:text-white">${this.formatDate(order.orderDate || order.createdAt)}</p>
        </div>
        <div>
          <p class="text-sm text-gray-500 mb-1">وضعیت</p>
          <span class="px-3 py-1 inline-flex text-sm font-semibold rounded-full ${this.getStatusClass(order.orderStatus || order.status)}">
            ${this.getStatusText(order.orderStatus || order.status)}
          </span>
        </div>
        <div>
          <p class="text-sm text-gray-500 mb-1">مبلغ کل</p>
          <p class="font-bold text-lg text-primary">${this.formatMoney(order.finalAmount ?? order.totalAmount ?? 0)}</p>
        </div>
      </div>
    `;
  },

  backToOrders: function () {
    const listEl = document.getElementById("orders-list-view");
    const detailsEl = document.getElementById("order-details-view");

    if (detailsEl) detailsEl.classList.add("hidden");
    if (listEl) listEl.classList.remove("hidden");
  },

  trackOrder: async function (orderId) {
    try {
      const result = await window.orderService.trackOrder(orderId);
      if (result.success && result.data) {
        alert(`وضعیت سفارش: ${result.data[0]?.status || result.data.status || "نامشخص"}`);
      } else if (window.utils) {
        window.utils.showToast("اطلاعات رهگیری یافت نشد", "error");
      }
    } catch (error) {
      if (window.utils) window.utils.showToast("خطا در رهگیری سفارش", "error");
    }
  },
};

window.OrderManager = OrderManager;
window.backToOrders = () => OrderManager.backToOrders();

document.addEventListener("DOMContentLoaded", () => {
  OrderManager.init();
});
