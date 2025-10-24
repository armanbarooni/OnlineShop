/**
 * Cart Service
 * Handles all cart-related API calls
 */
class CartService {
    constructor() {
        this.baseUrl = '/api/cart';
    }

    /**
     * Get cart items
     */
    async getCartItems() {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/items`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error getting cart items:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت آیتم‌های سبد خرید'
            };
        }
    }

    /**
     * Add item to cart
     */
    async addToCart(productId, quantity = 1, variantId = null) {
        try {
            const cartData = {
                productId: productId,
                quantity: quantity,
                variantId: variantId
            };

            const response = await window.apiClient.post(`${this.baseUrl}/add`, cartData);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error adding to cart:', error);
            return {
                success: false,
                error: error.message || 'خطا در افزودن به سبد خرید'
            };
        }
    }

    /**
     * Update cart item quantity
     */
    async updateCartItem(cartItemId, quantity) {
        try {
            const updateData = {
                quantity: quantity
            };

            const response = await window.apiClient.put(`${this.baseUrl}/items/${cartItemId}`, updateData);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error updating cart item:', error);
            return {
                success: false,
                error: error.message || 'خطا در به‌روزرسانی آیتم سبد خرید'
            };
        }
    }

    /**
     * Remove item from cart
     */
    async removeFromCart(cartItemId) {
        try {
            const response = await window.apiClient.delete(`${this.baseUrl}/items/${cartItemId}`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error removing from cart:', error);
            return {
                success: false,
                error: error.message || 'خطا در حذف از سبد خرید'
            };
        }
    }

    /**
     * Clear cart
     */
    async clearCart() {
        try {
            const response = await window.apiClient.delete(`${this.baseUrl}/clear`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error clearing cart:', error);
            return {
                success: false,
                error: error.message || 'خطا در پاک کردن سبد خرید'
            };
        }
    }

    /**
     * Apply coupon to cart
     */
    async applyCoupon(couponCode) {
        try {
            const couponData = {
                couponCode: couponCode
            };

            const response = await window.apiClient.post(`${this.baseUrl}/apply-coupon`, couponData);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error applying coupon:', error);
            return {
                success: false,
                error: error.message || 'خطا در اعمال کد تخفیف'
            };
        }
    }

    /**
     * Remove coupon from cart
     */
    async removeCoupon() {
        try {
            const response = await window.apiClient.delete(`${this.baseUrl}/remove-coupon`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error removing coupon:', error);
            return {
                success: false,
                error: error.message || 'خطا در حذف کد تخفیف'
            };
        }
    }

    /**
     * Get cart summary
     */
    async getCartSummary() {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/summary`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error getting cart summary:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت خلاصه سبد خرید'
            };
        }
    }

    /**
     * Validate cart data
     */
    validateCartData(cartData) {
        const errors = {};

        if (!cartData.productId) {
            errors.productId = 'شناسه محصول الزامی است';
        }

        if (!cartData.quantity || cartData.quantity <= 0) {
            errors.quantity = 'تعداد باید بیشتر از صفر باشد';
        }

        if (cartData.quantity > 100) {
            errors.quantity = 'تعداد نمی‌تواند بیشتر از ۱۰۰ باشد';
        }

        return {
            isValid: Object.keys(errors).length === 0,
            errors: errors
        };
    }

    /**
     * Calculate cart totals
     */
    calculateTotals(cartItems, coupon = null) {
        let subtotal = 0;
        let discount = 0;
        let shipping = 0;
        let tax = 0;

        // Calculate subtotal
        cartItems.forEach(item => {
            subtotal += item.unitPrice * item.quantity;
        });

        // Apply coupon discount
        if (coupon) {
            if (coupon.discountType === 'Percentage') {
                discount = (subtotal * coupon.discountValue) / 100;
                if (coupon.maximumDiscount && discount > coupon.maximumDiscount) {
                    discount = coupon.maximumDiscount;
                }
            } else {
                discount = coupon.discountValue;
            }
        }

        // Calculate shipping (free shipping threshold)
        const freeShippingThreshold = 500000; // 500,000 Toman
        if (subtotal < freeShippingThreshold) {
            shipping = 30000; // 30,000 Toman
        }

        // Calculate tax (9% VAT)
        const taxableAmount = subtotal - discount;
        tax = (taxableAmount * 9) / 100;

        const total = subtotal - discount + shipping + tax;

        return {
            subtotal: subtotal,
            discount: discount,
            shipping: shipping,
            tax: tax,
            total: total
        };
    }
}

// Create global instance
window.cartService = new CartService();
