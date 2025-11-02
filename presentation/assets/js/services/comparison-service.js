/**
 * Comparison Service
 * Handles product comparison functionality - connected to API
 */
class ComparisonService {
    constructor() {
        this.apiClient = window.apiClient;
        this.baseUrl = '/api/ProductComparison';
        this.maxItems = 4; // Maximum products for comparison
        // Keep localStorage as fallback for guest users
        this.storageKey = 'productComparison';
    }

    /**
     * Add product to comparison - API version
     */
    async addToComparison(productId) {
        try {
            // Check if user is authenticated
            if (!this.apiClient.getCurrentUser()) {
                // Fallback to localStorage for guest users
                return this.addToComparisonLocalStorage(productId);
            }

            const response = await this.apiClient.post(`${this.baseUrl}/add`, {
                productId: productId
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
            window.logger.error('Error adding to comparison:', error);
            // Fallback to localStorage if API fails
            if (error.message && error.message.includes('401')) {
                return this.addToComparisonLocalStorage(productId);
            }
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط§ظپط²ظˆط¯ظ† ط¨ظ‡ ظ„غŒط³طھ ظ…ظ‚ط§غŒط³ظ‡'
            };
        }
    }

    /**
     * Add product to comparison - localStorage fallback for guests
     */
    addToComparisonLocalStorage(productId) {
        try {
            let comparisonList = this.getComparisonList();
            
            // Check if product already exists
            if (comparisonList.some(item => item.id === productId)) {
                return {
                    success: false,
                    error: 'ط§غŒظ† ظ…ط­طµظˆظ„ ظ‚ط¨ظ„ط§ظ‹ ط¯ط± ظ„غŒط³طھ ظ…ظ‚ط§غŒط³ظ‡ ظ…ظˆط¬ظˆط¯ ط§ط³طھ'
                };
            }

            // Check maximum items limit
            if (comparisonList.length >= this.maxItems) {
                return {
                    success: false,
                    error: `ط­ط¯ط§ع©ط«ط± ${this.maxItems} ظ…ط­طµظˆظ„ ظ‚ط§ط¨ظ„ ظ…ظ‚ط§غŒط³ظ‡ ط§ط³طھ`
                };
            }

            // Add product ID to comparison
            comparisonList.push({
                id: productId,
                addedAt: new Date().toISOString()
            });

            this.saveComparisonList(comparisonList);

            return {
                success: true,
                data: comparisonList
            };
        } catch (error) {
            window.logger.error('Error adding to comparison:', error);
            return {
                success: false,
                error: 'ط®ط·ط§ ط¯ط± ط§ظپط²ظˆط¯ظ† ط¨ظ‡ ظ„غŒط³طھ ظ…ظ‚ط§غŒط³ظ‡'
            };
        }
    }

    /**
     * Remove product from comparison - API version
     */
    async removeFromComparison(productId) {
        try {
            // Check if user is authenticated
            if (!this.apiClient.getCurrentUser()) {
                // Fallback to localStorage
                return this.removeFromComparisonLocalStorage(productId);
            }

            const response = await this.apiClient.delete(`${this.baseUrl}/remove/${productId}`);
            
            if (response.success !== undefined && !response.success) {
                return response;
            }

            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error removing from comparison:', error);
            // Fallback to localStorage if API fails
            if (error.message && error.message.includes('401')) {
                return this.removeFromComparisonLocalStorage(productId);
            }
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط­ط°ظپ ط§ط² ظ„غŒط³طھ ظ…ظ‚ط§غŒط³ظ‡'
            };
        }
    }

    /**
     * Remove product from comparison - localStorage fallback
     */
    removeFromComparisonLocalStorage(productId) {
        try {
            let comparisonList = this.getComparisonList();
            comparisonList = comparisonList.filter(item => item.id !== productId);
            this.saveComparisonList(comparisonList);

            return {
                success: true,
                data: comparisonList
            };
        } catch (error) {
            window.logger.error('Error removing from comparison:', error);
            return {
                success: false,
                error: 'ط®ط·ط§ ط¯ط± ط­ط°ظپ ط§ط² ظ„غŒط³طھ ظ…ظ‚ط§غŒط³ظ‡'
            };
        }
    }

    /**
     * Clear comparison list - API version
     */
    async clearComparison() {
        try {
            // Check if user is authenticated
            if (!this.apiClient.getCurrentUser()) {
                // Fallback to localStorage
                localStorage.removeItem(this.storageKey);
                return {
                    success: true,
                    data: []
                };
            }

            const response = await this.apiClient.delete(`${this.baseUrl}/clear`);
            
            if (response.success !== undefined && !response.success) {
                return response;
            }

            // Also clear localStorage
            localStorage.removeItem(this.storageKey);

            return {
                success: true,
                data: []
            };
        } catch (error) {
            window.logger.error('Error clearing comparison:', error);
            // Fallback to localStorage
            localStorage.removeItem(this.storageKey);
            return {
                success: true,
                data: []
            };
        }
    }

    /**
     * Get comparison list - API version
     */
    async getComparisonFromAPI() {
        try {
            if (!this.apiClient.getCurrentUser()) {
                // Fallback to localStorage
                return {
                    success: true,
                    data: this.getComparisonList()
                };
            }

            const response = await this.apiClient.get(this.baseUrl);
            
            if (response.success !== undefined && !response.success) {
                return response;
            }

            const data = response.data || response;
            return {
                success: true,
                data: data.data || data || []
            };
        } catch (error) {
            window.logger.error('Error getting comparison from API:', error);
            // Fallback to localStorage
            if (error.message && error.message.includes('401')) {
                return {
                    success: true,
                    data: this.getComparisonList()
                };
            }
            return {
                success: false,
                error: error.message || 'ط®ط·ط§ ط¯ط± ط¯ط±غŒط§ظپطھ ظ„غŒط³طھ ظ…ظ‚ط§غŒط³ظ‡'
            };
        }
    }

    /**
     * Get comparison list - localStorage fallback
     */
    getComparisonList() {
        try {
            const stored = localStorage.getItem(this.storageKey);
            return stored ? JSON.parse(stored) : [];
        } catch (error) {
            window.logger.error('Error getting comparison list:', error);
            return [];
        }
    }

    /**
     * Get comparison count - async method (main method)
     */
    async getComparisonCount() {
        try {
            const result = await this.getComparisonFromAPI();
            if (result.success && Array.isArray(result.data)) {
                return result.data.length;
            }
            // Fallback to localStorage count
            return this.getComparisonList().length;
        } catch (error) {
            // Fallback to localStorage count on error
            return this.getComparisonList().length;
        }
    }

    /**
     * Get comparison count synchronously (localStorage only - for quick UI updates)
     * Use this only when async is not possible, otherwise use async getComparisonCount()
     */
    getLocalComparisonCount() {
        return this.getComparisonList().length;
    }

    /**
     * Save comparison list
     */
    saveComparisonList(comparisonList) {
        try {
            localStorage.setItem(this.storageKey, JSON.stringify(comparisonList));
        } catch (error) {
            window.logger.error('Error saving comparison list:', error);
        }
    }

    /**
     * Check if product is in comparison
     */
    isInComparison(productId) {
        const comparisonList = this.getComparisonList();
        return comparisonList.some(item => item.id === productId);
    }

    /**
     * Get detailed comparison data - using API
     */
    async getDetailedComparison() {
        try {
            // Get comparison from API
            const comparisonResult = await this.getComparisonFromAPI();
            
            if (!comparisonResult.success || !comparisonResult.data || comparisonResult.data.length === 0) {
                return {
                    success: true,
                    data: []
                };
            }

            const comparisonList = comparisonResult.data;

            // If user is authenticated, try to get detailed comparison from API
            if (this.apiClient.getCurrentUser()) {
                try {
                    const response = await this.apiClient.get(`${this.baseUrl}/compare`);
                    if (response.success !== undefined && !response.success) {
                        // Fall back to getting individual products
                        return await this.getDetailedComparisonFromProducts(comparisonList);
                    }
                    
                    const data = response.data || response;
                    const comparisonData = data.data || data;
                    
                    if (comparisonData && comparisonData.products) {
                        return {
                            success: true,
                            data: comparisonData.products,
                            specifications: comparisonData.specifications || []
                        };
                    }
                } catch (error) {
                    window.logger.error('Error getting detailed comparison from API:', error);
                    // Fall back to getting individual products
                }
            }

            // Fallback: Get detailed product information for each product
            return await this.getDetailedComparisonFromProducts(comparisonList);
        } catch (error) {
            window.logger.error('Error getting detailed comparison:', error);
            return {
                success: false,
                error: 'ط®ط·ط§ ط¯ط± ط¯ط±غŒط§ظپطھ ط§ط·ظ„ط§ط¹ط§طھ ظ…ظ‚ط§غŒط³ظ‡'
            };
        }
    }

    /**
     * Get detailed comparison by fetching individual products
     */
    async getDetailedComparisonFromProducts(comparisonList) {
        try {
            // Get detailed product information for each product
            const detailedProducts = await Promise.all(
                comparisonList.map(async (product) => {
                    try {
                        const productId = product.id || product.productId;
                        const response = await window.productService.getProductById(productId);
                        if (response.success) {
                            return {
                                ...product,
                                ...response.data,
                                specifications: response.data.productDetails || [],
                                features: response.data.features || [],
                                images: response.data.productImages || []
                            };
                        }
                        return product;
                    } catch (error) {
                        window.logger.error(`Error getting details for product ${product.id}:`, error);
                        return product;
                    }
                })
            );

            return {
                success: true,
                data: detailedProducts
            };
        } catch (error) {
            window.logger.error('Error getting detailed comparison from products:', error);
            return {
                success: false,
                error: 'ط®ط·ط§ ط¯ط± ط¯ط±غŒط§ظپطھ ط§ط·ظ„ط§ط¹ط§طھ ظ…ظ‚ط§غŒط³ظ‡'
            };
        }
    }

    /**
     * Get comparison specifications
     */
    getComparisonSpecifications(products) {
        if (!products || products.length === 0) {
            return [];
        }

        // Get all unique specification keys
        const allSpecs = new Set();
        products.forEach(product => {
            if (product.specifications) {
                product.specifications.forEach(spec => {
                    allSpecs.add(spec.name);
                });
            }
        });

        // Create comparison matrix
        const specifications = Array.from(allSpecs).map(specName => {
            const spec = {
                name: specName,
                values: {}
            };

            products.forEach(product => {
                const productSpec = product.specifications?.find(s => s.name === specName);
                spec.values[product.id] = productSpec ? productSpec.value : '-';
            });

            return spec;
        });

        return specifications;
    }

    /**
     * Export comparison to PDF
     */
    async exportToPDF(products) {
        try {
            // This would typically call a backend service to generate PDF
            const response = await window.apiClient.post('/api/comparison/export-pdf', {
                products: products
            });
            
            return {
                success: true,
                data: response
            };
        } catch (error) {
            window.logger.error('Error exporting comparison to PDF:', error);
            return {
                success: false,
                error: 'ط®ط·ط§ ط¯ط± طµط§ط¯ط± ع©ط±ط¯ظ† ظپط§غŒظ„ PDF'
            };
        }
    }

    /**
     * Share comparison
     */
    async shareComparison(products) {
        try {
            const comparisonData = {
                products: products.map(p => ({
                    id: p.id,
                    name: p.name,
                    price: p.price
                })),
                shareUrl: window.location.origin + '/compare.html?shared=true'
            };

            // This would typically call a backend service to create shareable link
            const response = await window.apiClient.post('/api/comparison/share', comparisonData);
            
            return {
                success: true,
                data: response
            };
        } catch (error) {
            window.logger.error('Error sharing comparison:', error);
            return {
                success: false,
                error: 'ط®ط·ط§ ط¯ط± ط§ط´طھط±ط§ع©â€Œع¯ط°ط§ط±غŒ ظ…ظ‚ط§غŒط³ظ‡'
            };
        }
    }

    /**
     * Validate comparison data
     */
    validateComparisonData(comparisonData) {
        const errors = {};

        if (!comparisonData.products || comparisonData.products.length === 0) {
            errors.products = 'ظ„غŒط³طھ ظ…ط­طµظˆظ„ط§طھ ط®ط§ظ„غŒ ط§ط³طھ';
        }

        if (comparisonData.products && comparisonData.products.length > this.maxItems) {
            errors.products = `ط­ط¯ط§ع©ط«ط± ${this.maxItems} ظ…ط­طµظˆظ„ ظ‚ط§ط¨ظ„ ظ…ظ‚ط§غŒط³ظ‡ ط§ط³طھ`;
        }

        return {
            isValid: Object.keys(errors).length === 0,
            errors: errors
        };
    }
}

// Create global instance
window.comparisonService = new ComparisonService();

