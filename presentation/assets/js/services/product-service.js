/**
 * Product Service
 * Handles all product-related API calls
 */
class ProductService {
    constructor() {
        this.baseUrl = '/api/products';
    }

    /**
     * Get product by ID
     */
    async getProductById(productId) {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/${productId}`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error getting product:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت اطلاعات محصول'
            };
        }
    }

    /**
     * Search products with filters
     */
    async searchProducts(searchCriteria) {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/search`, {
                params: searchCriteria
            });
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error searching products:', error);
            return {
                success: false,
                error: error.message || 'خطا در جستجوی محصولات'
            };
        }
    }

    /**
     * Get product categories
     */
    async getCategories() {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/categories`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error getting categories:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت دسته‌بندی‌ها'
            };
        }
    }

    /**
     * Get product reviews
     */
    async getProductReviews(productId, page = 1, pageSize = 10) {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/${productId}/reviews`, {
                params: { page, pageSize }
            });
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error getting product reviews:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت نظرات محصول'
            };
        }
    }

    /**
     * Get related products
     */
    async getRelatedProducts(productId, limit = 4) {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/${productId}/related`, {
                params: { limit }
            });
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error getting related products:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت محصولات مرتبط'
            };
        }
    }

    /**
     * Get product specifications
     */
    async getProductSpecifications(productId) {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/${productId}/specifications`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error getting product specifications:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت مشخصات محصول'
            };
        }
    }

    /**
     * Get product images
     */
    async getProductImages(productId) {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/${productId}/images`);
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error getting product images:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت تصاویر محصول'
            };
        }
    }

    /**
     * Get featured products
     */
    async getFeaturedProducts(limit = 8) {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/featured`, {
                params: { limit }
            });
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error getting featured products:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت محصولات ویژه'
            };
        }
    }

    /**
     * Get best selling products
     */
    async getBestSellingProducts(limit = 8) {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/bestselling`, {
                params: { limit }
            });
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error getting best selling products:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت پرفروش‌ترین محصولات'
            };
        }
    }

    /**
     * Get new products
     */
    async getNewProducts(limit = 8) {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/new`, {
                params: { limit }
            });
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error getting new products:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت جدیدترین محصولات'
            };
        }
    }

    /**
     * Get products on sale
     */
    async getSaleProducts(limit = 8) {
        try {
            const response = await window.apiClient.get(`${this.baseUrl}/sale`, {
                params: { limit }
            });
            return {
                success: true,
                data: response
            };
        } catch (error) {
            console.error('Error getting sale products:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت محصولات تخفیف‌دار'
            };
        }
    }

    /**
     * Validate product data
     */
    validateProductData(productData) {
        const errors = {};

        if (!productData.name || productData.name.trim().length < 2) {
            errors.name = 'نام محصول باید حداقل ۲ کاراکتر باشد';
        }

        if (!productData.price || productData.price <= 0) {
            errors.price = 'قیمت باید بیشتر از صفر باشد';
        }

        if (!productData.categoryId) {
            errors.categoryId = 'دسته‌بندی محصول الزامی است';
        }

        if (!productData.description || productData.description.trim().length < 10) {
            errors.description = 'توضیحات محصول باید حداقل ۱۰ کاراکتر باشد';
        }

        return {
            isValid: Object.keys(errors).length === 0,
            errors: errors
        };
    }
}

// Create global instance
window.productService = new ProductService();
