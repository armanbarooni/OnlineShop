/**
 * Product Service
 * Handles all product-related API calls
 */
class ProductService {
    constructor() {
        this.apiClient = window.apiClient;
        this.baseUrl = '/api/Product';
    }

    /**
     * Get all products
     */
    async getAllProducts() {
        try {
            const response = await this.apiClient.get(this.baseUrl);
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            const data = response.data || response;
            return {
                success: true,
                data: data.data || data
            };
        } catch (error) {
            window.logger.error('Error getting all products:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت محصولات'
            };
        }
    }

    /**
     * Get product by ID
     */
    async getProductById(productId) {
        try {
            const response = await this.apiClient.get(`${this.baseUrl}/${productId}`);
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            const data = response.data || response;
            return {
                success: true,
                data: data.data || data
            };
        } catch (error) {
            window.logger.error('Error getting product:', error);
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
            // Build query string from criteria
            const params = new URLSearchParams();
            if (searchCriteria.searchTerm) params.append('searchTerm', searchCriteria.searchTerm);
            if (searchCriteria.categoryId) params.append('categoryId', searchCriteria.categoryId);
            if (searchCriteria.brandId) params.append('brandId', searchCriteria.brandId);
            if (searchCriteria.minPrice) params.append('minPrice', searchCriteria.minPrice);
            if (searchCriteria.maxPrice) params.append('maxPrice', searchCriteria.maxPrice);
            if (searchCriteria.inStock !== undefined && searchCriteria.inStock !== null) params.append('inStock', searchCriteria.inStock);
            if (searchCriteria.sortBy) params.append('sortBy', searchCriteria.sortBy);
            if (searchCriteria.sortDescending !== undefined) params.append('sortDescending', searchCriteria.sortDescending);
            if (searchCriteria.pageNumber) params.append('pageNumber', searchCriteria.pageNumber);
            if (searchCriteria.pageSize) params.append('pageSize', searchCriteria.pageSize);

            const queryString = params.toString();
            const url = queryString ? `${this.baseUrl}/search?${queryString}` : `${this.baseUrl}/search`;
            
            const response = await this.apiClient.get(url);
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            const data = response.data || response;
            return {
                success: true,
                data: data.data || data
            };
        } catch (error) {
            window.logger.error('Error searching products:', error);
            return {
                success: false,
                error: error.message || 'خطا در جستجوی محصولات'
            };
        }
    }

    /**
     * Search products with POST (for complex criteria)
     */
    async searchProductsPost(searchCriteria) {
        try {
            const response = await this.apiClient.post(`${this.baseUrl}/search`, searchCriteria);
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            const data = response.data || response;
            return {
                success: true,
                data: data.data || data
            };
        } catch (error) {
            window.logger.error('Error searching products:', error);
            return {
                success: false,
                error: error.message || 'خطا در جستجوی محصولات'
            };
        }
    }

    /**
     * Get products by category
     */
    async getProductsByCategory(categoryId, pageNumber = 1, pageSize = 20) {
        try {
            return await this.searchProducts({
                categoryId: categoryId,
                pageNumber: pageNumber,
                pageSize: pageSize
            });
        } catch (error) {
            window.logger.error('Error getting products by category:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت محصولات دسته‌بندی'
            };
        }
    }

    /**
     * Get product reviews
     */
    async getProductReviews(productId, page = 1, pageSize = 10) {
        try {
            const response = await this.apiClient.get(`/api/ProductReview/product/${productId}`);
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            const data = response.data || response;
            const reviews = Array.isArray(data.data) ? data.data : (Array.isArray(data) ? data : []);
            
            // Filter approved reviews only for public display
            const approvedReviews = reviews.filter(r => r.isApproved !== false);
            
            // Simple pagination on client side
            const startIndex = (page - 1) * pageSize;
            const paginatedReviews = approvedReviews.slice(startIndex, startIndex + pageSize);
            
            return {
                success: true,
                data: paginatedReviews,
                totalCount: approvedReviews.length,
                page: page,
                pageSize: pageSize,
                totalPages: Math.ceil(approvedReviews.length / pageSize)
            };
        } catch (error) {
            window.logger.error('Error getting product reviews:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت نظرات محصول'
            };
        }
    }

    /**
     * Get related products
     */
    async getRelatedProducts(productId, limit = 4, relationType = 'Similar') {
        try {
            const response = await this.apiClient.get(`${this.baseUrl}/${productId}/related?limit=${limit}&relationType=${relationType}`);
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            const data = response.data || response;
            return {
                success: true,
                data: data.data || data
            };
        } catch (error) {
            window.logger.error('Error getting related products:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت محصولات مرتبط'
            };
        }
    }

    /**
     * Get frequently bought together products
     */
    async getFrequentlyBoughtTogether(productId, limit = 4) {
        try {
            const response = await this.apiClient.get(`${this.baseUrl}/${productId}/frequently-bought-together?limit=${limit}`);
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            const data = response.data || response;
            return {
                success: true,
                data: data.data || data
            };
        } catch (error) {
            window.logger.error('Error getting frequently bought together products:', error);
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
            window.logger.error('Error getting product specifications:', error);
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
            window.logger.error('Error getting product images:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت تصاویر محصول'
            };
        }
    }

    /**
     * Get featured products - using getAllProducts with limit
     */
    async getFeaturedProducts(limit = 8) {
        try {
            const result = await this.getAllProducts();
            if (result.success && Array.isArray(result.data)) {
                return {
                    success: true,
                    data: result.data.slice(0, limit)
                };
            }
            return result;
        } catch (error) {
            window.logger.error('Error getting featured products:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت محصولات ویژه'
            };
        }
    }

    /**
     * Get best selling products - using search with sort
     */
    async getBestSellingProducts(limit = 8) {
        try {
            return await this.searchProducts({
                sortBy: 'Sales',
                sortDescending: true,
                pageNumber: 1,
                pageSize: limit
            });
        } catch (error) {
            window.logger.error('Error getting best selling products:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت پرفروش‌ترین محصولات'
            };
        }
    }

    /**
     * Get new products - using search with sort
     */
    async getNewProducts(limit = 8) {
        try {
            return await this.searchProducts({
                sortBy: 'CreatedAt',
                sortDescending: true,
                pageNumber: 1,
                pageSize: limit
            });
        } catch (error) {
            window.logger.error('Error getting new products:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت جدیدترین محصولات'
            };
        }
    }

    /**
     * Get products on sale - filter by discount
     */
    async getSaleProducts(limit = 8) {
        try {
            const result = await this.searchProducts({
                pageNumber: 1,
                pageSize: limit
            });
            
            if (result.success && result.data && result.data.products) {
                // Filter products that have discount
                const saleProducts = result.data.products.filter(p => 
                    p.discountPercent > 0 || 
                    (p.originalPrice && p.price && p.originalPrice > p.price)
                );
                
                return {
                    success: true,
                    data: {
                        ...result.data,
                        products: saleProducts.slice(0, limit)
                    }
                };
            }
            
            return result;
        } catch (error) {
            window.logger.error('Error getting sale products:', error);
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
            errors.price = 'قیمت باید بیش‌تر از صفر باشد';
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

