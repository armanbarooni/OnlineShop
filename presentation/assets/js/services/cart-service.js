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
                    error: 'ع©ط§ط±ط¨ط± ط§ط­ط±ط§ط² ظ‡ظˆغŒطھ ظ†ط´ط¯ظ‡ ط§ط³طھ'
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
            window.logger.error('Error getting user cart:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط¯ط±غŒط§ظپطھ ط³ط¨ط¯ ط®ط±غŒط¯'
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
            window.logger.error('Error adding to cart:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط§ظپط²ظˆط¯ظ† ط¨ظ‡ ط³ط¨ط¯ ط®ط±غŒط¯'
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
            window.logger.error('Error creating cart:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط§غŒط¬ط§ط¯ ط³ط¨ط¯ ط®ط±غŒط¯'
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
            window.logger.error('Error updating cart:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط¨ظ‡â€Œط±ظˆط²ط±ط³ط§ظ†غŒ ط³ط¨ط¯ ط®ط±غŒط¯'
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
                    error: 'ط¢غŒطھظ…غŒ ط¯ط± ط³ط¨ط¯ ط®ط±غŒط¯ غŒط§ظپطھ ظ†ط´ط¯'
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
            window.logger.error('Error updating cart item:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط¨ظ‡â€Œط±ظˆط²ط±ط³ط§ظ†غŒ ط¢غŒطھظ… ط³ط¨ط¯ ط®ط±غŒط¯'
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
            window.logger.error('Error removing from cart:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط­ط°ظپ ط§ط² ط³ط¨ط¯ ط®ط±غŒط¯'
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
            window.logger.error('Error deleting cart:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط­ط°ظپ ط³ط¨ط¯ ط®ط±غŒط¯'
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
            window.logger.error('Error clearing cart:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ظ¾ط§ع© ع©ط±ط¯ظ† ط³ط¨ط¯ ط®ط±غŒط¯'
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
            window.logger.error('Error applying coupon:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط§ط¹ظ…ط§ظ„ ع©ط¯ طھط®ظپغŒظپ'
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
            window.logger.error('Error removing coupon:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط­ط°ظپ ع©ط¯ طھط®ظپغŒظپ'
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
            window.logger.error('Error getting cart summary:', error);
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط¯ط±غŒط§ظپطھ ط®ظ„ط§طµظ‡ ط³ط¨ط¯ ط®ط±غŒط¯'
            };
        }
    }

    /**
     * Validate cart data
     */
    validateCartData(cartData) {
        const errors = {};

        if (!cartData.productId) {
            errors.productId = 'ط´ظ†ط§ط³ظ‡ ظ…ط­طµظˆظ„ ط§ظ„ط²ط§ظ…غŒ ط§ط³طھ';
        }

        if (!cartData.quantity || cartData.quantity <= 0) {
            errors.quantity = 'طھط¹ط¯ط§ط¯ ط¨ط§غŒط¯ ط¨غŒط´طھط± ط§ط² طµظپط± ط¨ط§ط´ط¯';
        }

        if (cartData.quantity > 100) {
            errors.quantity = 'طھط¹ط¯ط§ط¯ ظ†ظ…غŒâ€Œطھظˆط§ظ†ط¯ ط¨غŒط´طھط± ط§ط² غ±غ°غ° ط¨ط§ط´ط¯';
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

