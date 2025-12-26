import { ApiClient } from '../api-client.js';
import CartService from './cart-service.js';
import CategoryService from './category-service.js';
import SearchService from './search-service.js';

class CheckoutPage {
    constructor() {
        this.apiClient = new ApiClient('http://localhost:5000/api');
        this.cartService = new CartService(this.apiClient);
        this.categoryService = new CategoryService(this.apiClient);
        this.searchService = new SearchService(this.apiClient);

        this.selectedDeliveryMethod = 'normal';
        this.deliveryCost = 35000;
        this.selectedDay = null;
        this.selectedTimeSlot = null;

        this.init();
    }

    async init() {
        // 1. Update Cart Count
        this.cartService.updateCartCount();

        // 2. Initialize Search
        const searchInput = document.getElementById('searchInput');
        const searchResults = document.getElementById('searchResults');
        if (searchInput && searchResults) {
            this.searchService.init(searchInput, searchResults);
        }

        // 3. Check if cart is empty
        const cartItems = this.cartService.getCart();
        if (cartItems.length === 0) {
            window.location.href = 'cart.html';
            return;
        }

        // 4. Render cart items in checkout
        this.renderCartItems(cartItems);

        // 5. Render order summary
        this.renderOrderSummary();

        // 6. Setup delivery method listeners
        this.setupDeliveryMethodListeners();

        // 7. Generate day selector
        this.generateDaySelector();

        // 8. Generate time slots
        this.generateTimeSlots();

        // 9. Setup checkout button
        this.setupCheckoutButton();
    }

    renderCartItems(items) {
        const container = document.getElementById('checkout-cart-items');
        if (!container) return;

        container.innerHTML = items.map(item => `
            <div class="flex items-center border-b border-gray-100 pb-4 mb-4 last:mb-0 last:pb-0 last:border-0">
                <img src="${item.image || 'assets/images/product/product-1.jpeg'}" alt="${item.name}" class="w-16 h-16 object-cover rounded-md">
                <div class="ms-3 flex-1">
                    <h3 class="font-medium text-gray-700 dark:text-white">${item.name}</h3>
                    <p class="text-sm text-gray-500 dark:text-white">تعداد: ${item.quantity}</p>
                </div>
                <span class="text-green-500 text-sm">موجود</span>
            </div>
        `).join('');
    }

    renderOrderSummary() {
        const items = this.cartService.getCart();
        const subtotal = this.cartService.getTotal();
        const discount = 0; // Can be calculated based on discount codes
        const total = subtotal + this.deliveryCost - discount;

        // Update summary elements
        const subtotalEl = document.getElementById('checkout-subtotal');
        const discountEl = document.getElementById('checkout-discount');
        const deliveryCostEl = document.getElementById('checkout-delivery-cost');
        const totalEl = document.getElementById('checkout-total');

        if (subtotalEl) subtotalEl.textContent = `${subtotal.toLocaleString()} تومان`;
        if (discountEl) discountEl.textContent = `${discount.toLocaleString()} تومان`;
        if (deliveryCostEl) deliveryCostEl.textContent = `${this.deliveryCost.toLocaleString()} تومان`;
        if (totalEl) totalEl.textContent = `${total.toLocaleString()} تومان`;

        // Update delivery cost summary
        const deliveryCostSummary = document.querySelector('.delivery-cost-summary');
        if (deliveryCostSummary) {
            deliveryCostSummary.textContent = `${this.deliveryCost.toLocaleString()} تومان`;
        }
    }

    setupDeliveryMethodListeners() {
        const deliveryMethods = document.querySelectorAll('.delivery-method');
        deliveryMethods.forEach(method => {
            method.addEventListener('change', (e) => {
                this.selectedDeliveryMethod = e.target.dataset.type;

                // Update delivery cost
                if (this.selectedDeliveryMethod === 'pickup') {
                    this.deliveryCost = 0;
                    // Show pickup location selector
                    const pickupLocation = document.querySelector('.pickup-location');
                    if (pickupLocation) pickupLocation.style.display = 'block';
                } else {
                    this.deliveryCost = 35000;
                    const pickupLocation = document.querySelector('.pickup-location');
                    if (pickupLocation) pickupLocation.style.display = 'none';
                }

                // Update all delivery cost displays
                document.querySelectorAll('.delivery-cost').forEach(el => {
                    el.textContent = this.deliveryCost.toLocaleString();
                });

                this.renderOrderSummary();
            });
        });
    }

    generateDaySelector() {
        const container = document.getElementById('day-selector');
        if (!container) return;

        const days = [];
        const persianDays = ['یکشنبه', 'دوشنبه', 'سه‌شنبه', 'چهارشنبه', 'پنج‌شنبه', 'جمعه', 'شنبه'];
        const today = new Date();

        for (let i = 1; i <= 7; i++) {
            const date = new Date(today);
            date.setDate(today.getDate() + i);
            const dayName = persianDays[date.getDay()];
            const dayNumber = date.getDate();

            days.push({
                date: date,
                dayName: dayName,
                dayNumber: dayNumber,
                isFirst: i === 1
            });
        }

        container.innerHTML = days.map((day, index) => `
            <button 
                class="day-btn flex-shrink-0 px-6 py-3 rounded-lg border-2 transition-all ${day.isFirst ? 'border-primary bg-primary text-white' : 'border-gray-200 hover:border-primary'}"
                data-day="${day.date.toISOString()}"
                data-index="${index}"
            >
                <div class="text-center">
                    <div class="font-bold">${day.dayName}</div>
                    <div class="text-sm mt-1">${day.dayNumber}</div>
                </div>
            </button>
        `).join('');

        // Set first day as selected
        this.selectedDay = days[0].date;

        // Add click listeners
        container.querySelectorAll('.day-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                // Remove active class from all
                container.querySelectorAll('.day-btn').forEach(b => {
                    b.classList.remove('border-primary', 'bg-primary', 'text-white');
                    b.classList.add('border-gray-200');
                });

                // Add active class to clicked
                btn.classList.add('border-primary', 'bg-primary', 'text-white');
                btn.classList.remove('border-gray-200');

                this.selectedDay = new Date(btn.dataset.day);
                this.updateDeliveryTimeSummary();
            });
        });
    }

    generateTimeSlots() {
        const container = document.getElementById('time-slots');
        if (!container) return;

        const slots = [
            { start: '9', end: '12', label: '۹ صبح تا ۱۲ ظهر' },
            { start: '12', end: '15', label: '۱۲ ظهر تا ۳ بعدازظهر' },
            { start: '15', end: '18', label: '۳ بعدازظهر تا ۶ عصر' },
            { start: '18', end: '21', label: '۶ عصر تا ۹ شب' }
        ];

        container.innerHTML = slots.map((slot, index) => `
            <button 
                class="time-slot-btn p-4 rounded-lg border-2 transition-all text-center ${index === 0 ? 'border-primary bg-primary text-white' : 'border-gray-200 hover:border-primary'}"
                data-slot="${slot.label}"
            >
                <div class="font-medium">${slot.label}</div>
            </button>
        `).join('');

        // Set first slot as selected
        this.selectedTimeSlot = slots[0].label;
        this.updateDeliveryTimeSummary();

        // Add click listeners
        container.querySelectorAll('.time-slot-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                // Remove active class from all
                container.querySelectorAll('.time-slot-btn').forEach(b => {
                    b.classList.remove('border-primary', 'bg-primary', 'text-white');
                    b.classList.add('border-gray-200');
                });

                // Add active class to clicked
                btn.classList.add('border-primary', 'bg-primary', 'text-white');
                btn.classList.remove('border-gray-200');

                this.selectedTimeSlot = btn.dataset.slot;
                this.updateDeliveryTimeSummary();
            });
        });
    }

    updateDeliveryTimeSummary() {
        const summaryEl = document.querySelector('.delivery-time-summary');
        if (!summaryEl || !this.selectedDay || !this.selectedTimeSlot) return;

        const persianDays = ['یکشنبه', 'دوشنبه', 'سه‌شنبه', 'چهارشنبه', 'پنج‌شنبه', 'جمعه', 'شنبه'];
        const dayName = persianDays[this.selectedDay.getDay()];

        summaryEl.textContent = `${dayName} - ${this.selectedTimeSlot}`;
    }

    setupCheckoutButton() {
        const checkoutBtn = document.getElementById('checkout-submit-btn');
        if (!checkoutBtn) return;

        checkoutBtn.addEventListener('click', async (e) => {
            e.preventDefault();

            // Validate selections
            if (!this.selectedDay || !this.selectedTimeSlot) {
                alert('لطفا زمان تحویل را انتخاب کنید');
                return;
            }

            // Prepare order data
            const orderData = {
                items: this.cartService.getCart(),
                deliveryMethod: this.selectedDeliveryMethod,
                deliveryDate: this.selectedDay,
                deliveryTimeSlot: this.selectedTimeSlot,
                deliveryCost: this.deliveryCost,
                subtotal: this.cartService.getTotal(),
                total: this.cartService.getTotal() + this.deliveryCost
            };

            try {
                // Here you would normally send the order to the backend
                // const response = await this.apiClient.post('/order', orderData);

                // For now, just clear cart and redirect
                console.log('Order submitted:', orderData);

                // Clear cart
                this.cartService.clearCart();

                // Redirect to success page or user panel
                alert('سفارش شما با موفقیت ثبت شد!');
                window.location.href = 'user-panel-order.html';

            } catch (error) {
                console.error('Error submitting order:', error);
                alert('خطا در ثبت سفارش. لطفا دوباره تلاش کنید.');
            }
        });
    }
}

document.addEventListener('DOMContentLoaded', () => {
    new CheckoutPage();
});
