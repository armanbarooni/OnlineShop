/**
 * Cart Service
 * Handles all cart-related API calls
 */
class CartService {
    constructor() {
        this.apiClient = window.apiClient;
        this.baseUrl = '/api/Cart';
    }

    /**
     * Get user cart
     */
    async getUserCart() {
        try {
            // Get current user ID from token
            const user = this.apiClient.getCurrentUser();
            if (!user || !user.id) {
                return {
                    success: false,
                    error: 'کاربر احراز هویت نشده است'
                };
            }

            const response = await this.apiClient.get(`${this.baseUrl}/user/${user.id}`);
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            const data = response.data || response;
            const cart = data.data || data;
            
            return {
                success: true,
                data: cart,
                items: cart?.items || []
            };
        } catch (error) {
            console.error('Error getting user cart:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت سبد خرید'
            };
        }
    }

    /**
     * Get cart items (alias for getUserCart for backward compatibility)
     */
    async getCartItems() {
        const result = await this.getUserCart();
        if (result.success && result.items) {
            return {
                success: true,
                data: result.items
            };
        }
        return result;
    }

    /**
     * Add item to cart
     */
    async addToCart(productId, quantity = 1, variantId = null, cartId = null) {
        try {
            // If cartId not provided, get user cart first
            if (!cartId) {
                const userCart = await this.getUserCart();
                if (userCart.success && userCart.data) {
                    cartId = userCart.data.id;
                } else if (userCart.success && !userCart.data) {
                    // Cart doesn't exist, create one
                    const createResult = await this.createCart();
                    if (!createResult.success) {
                        return createResult;
                    }
                    cartId = createResult.data.id;
                } else {
                    return userCart;
                }
            }

            const cartItem = {
                cartId: cartId,
                productId: productId,
                quantity: quantity,
                productVariantId: variantId
            };

            const response = await this.apiClient.post(`${this.baseUrl}/items`, cartItem);
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            const data = response.data || response;
            return {
                success: true,
                data: data.data || data
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
     * Create cart
     */
    async createCart() {
        try {
            const cartData = {
                // Empty cart initially
            };

            const response = await this.apiClient.post(this.baseUrl, cartData);
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            const data = response.data || response;
            return {
                success: true,
                data: data.data || data
            };
        } catch (error) {
            console.error('Error creating cart:', error);
            return {
                success: false,
                error: error.message || 'خطا در ایجاد سبد خرید'
            };
        }
    }

    /**
     * Update cart
     */
    async updateCart(cartId, cartData) {
        try {
            const response = await this.apiClient.put(`${this.baseUrl}/${cartId}`, {
                id: cartId,
                ...cartData
            });
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            const data = response.data || response;
            return {
                success: true,
                data: data.data || data
            };
        } catch (error) {
            console.error('Error updating cart:', error);
            return {
                success: false,
                error: error.message || 'خطا در به‌روزرسانی سبد خرید'
            };
        }
    }

    /**
     * Update cart item quantity (through cart update)
     */
    async updateCartItem(cartId, itemId, quantity) {
        try {
            // Get current cart
            const cartResult = await this.getUserCart();
            if (!cartResult.success) {
                return cartResult;
            }

            const cart = cartResult.data;
            if (!cart.items || cart.items.length === 0) {
                return {
                    success: false,
                    error: 'آیتمی در سبد خرید یافت نشد'
                };
            }

            // Update the item quantity
            const updatedItems = cart.items.map(item => {
                if (item.id === itemId) {
                    return { ...item, quantity: quantity };
                }
                return item;
            });

            // Update cart with new items
            return await this.updateCart(cartId, {
                items: updatedItems
            });
        } catch (error) {
            console.error('Error updating cart item:', error);
            return {
                success: false,
                error: error.message || 'خطا در به‌روزرسانی آیتم سبد خرید'
            };
        }
    }

    /**
     * Remove item from cart (through cart update)
     */
    async removeFromCart(cartId, itemId) {
        try {
            // Get current cart
            const cartResult = await this.getUserCart();
            if (!cartResult.success) {
                return cartResult;
            }

            const cart = cartResult.data;
            
            // Remove the item
            const updatedItems = cart.items.filter(item => item.id !== itemId);

            // Update cart
            return await this.updateCart(cartId, {
                items: updatedItems
            });
        } catch (error) {
            console.error('Error removing from cart:', error);
            return {
                success: false,
                error: error.message || 'خطا در حذف از سبد خرید'
            };
        }
    }

    /**
     * Delete cart
     */
    async deleteCart(cartId) {
        try {
            const response = await this.apiClient.delete(`${this.baseUrl}/${cartId}`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error deleting cart:', error);
            return {
                success: false,
                error: error.message || 'خطا در حذف سبد خرید'
            };
        }
    }

    /**
     * Clear cart (delete all items)
     */
    async clearCart() {
        try {
            const cartResult = await this.getUserCart();
            if (!cartResult.success || !cartResult.data) {
                return {
                    success: true,
                    data: { items: [] }
                };
            }

            const cart = cartResult.data;
            return await this.updateCart(cart.id, {
                items: []
            });
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
