/**
 * Comparison Service
 * Handles product comparison functionality
 */
class ComparisonService {
    constructor() {
        this.storageKey = 'productComparison';
        this.maxItems = 4; // Maximum products for comparison
    }

    /**
     * Add product to comparison
     */
    addToComparison(product) {
        try {
            let comparisonList = this.getComparisonList();
            
            // Check if product already exists
            if (comparisonList.some(item => item.id === product.id)) {
                return {
                    success: false,
                    error: 'این محصول قبلاً در لیست مقایسه موجود است'
                };
            }

            // Check maximum items limit
            if (comparisonList.length >= this.maxItems) {
                return {
                    success: false,
                    error: `حداکثر ${this.maxItems} محصول قابل مقایسه است`
                };
            }

            // Add product to comparison
            comparisonList.push({
                id: product.id,
                name: product.name,
                image: product.image,
                price: product.price,
                originalPrice: product.originalPrice,
                category: product.category,
                brand: product.brand,
                rating: product.rating,
                addedAt: new Date().toISOString()
            });

            this.saveComparisonList(comparisonList);

            return {
                success: true,
                data: comparisonList
            };
        } catch (error) {
            console.error('Error adding to comparison:', error);
            return {
                success: false,
                error: 'خطا در افزودن به لیست مقایسه'
            };
        }
    }

    /**
     * Remove product from comparison
     */
    removeFromComparison(productId) {
        try {
            let comparisonList = this.getComparisonList();
            comparisonList = comparisonList.filter(item => item.id !== productId);
            this.saveComparisonList(comparisonList);

            return {
                success: true,
                data: comparisonList
            };
        } catch (error) {
            console.error('Error removing from comparison:', error);
            return {
                success: false,
                error: 'خطا در حذف از لیست مقایسه'
            };
        }
    }

    /**
     * Clear comparison list
     */
    clearComparison() {
        try {
            localStorage.removeItem(this.storageKey);
            return {
                success: true,
                data: []
            };
        } catch (error) {
            console.error('Error clearing comparison:', error);
            return {
                success: false,
                error: 'خطا در پاک کردن لیست مقایسه'
            };
        }
    }

    /**
     * Get comparison list
     */
    getComparisonList() {
        try {
            const stored = localStorage.getItem(this.storageKey);
            return stored ? JSON.parse(stored) : [];
        } catch (error) {
            console.error('Error getting comparison list:', error);
            return [];
        }
    }

    /**
     * Save comparison list
     */
    saveComparisonList(comparisonList) {
        try {
            localStorage.setItem(this.storageKey, JSON.stringify(comparisonList));
        } catch (error) {
            console.error('Error saving comparison list:', error);
        }
    }

    /**
     * Get comparison count
     */
    getComparisonCount() {
        return this.getComparisonList().length;
    }

    /**
     * Check if product is in comparison
     */
    isInComparison(productId) {
        const comparisonList = this.getComparisonList();
        return comparisonList.some(item => item.id === productId);
    }

    /**
     * Get detailed comparison data
     */
    async getDetailedComparison() {
        try {
            const comparisonList = this.getComparisonList();
            
            if (comparisonList.length === 0) {
                return {
                    success: true,
                    data: []
                };
            }

            // Get detailed product information for each product
            const detailedProducts = await Promise.all(
                comparisonList.map(async (product) => {
                    try {
                        const response = await window.productService.getProductById(product.id);
                        if (response.success) {
                            return {
                                ...product,
                                ...response.data,
                                specifications: response.data.specifications || [],
                                features: response.data.features || [],
                                images: response.data.images || []
                            };
                        }
                        return product;
                    } catch (error) {
                        console.error(`Error getting details for product ${product.id}:`, error);
                        return product;
                    }
                })
            );

            return {
                success: true,
                data: detailedProducts
            };
        } catch (error) {
            console.error('Error getting detailed comparison:', error);
            return {
                success: false,
                error: 'خطا در دریافت اطلاعات مقایسه'
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
            console.error('Error exporting comparison to PDF:', error);
            return {
                success: false,
                error: 'خطا در صادر کردن فایل PDF'
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
            console.error('Error sharing comparison:', error);
            return {
                success: false,
                error: 'خطا در اشتراک‌گذاری مقایسه'
            };
        }
    }

    /**
     * Validate comparison data
     */
    validateComparisonData(comparisonData) {
        const errors = {};

        if (!comparisonData.products || comparisonData.products.length === 0) {
            errors.products = 'لیست محصولات خالی است';
        }

        if (comparisonData.products && comparisonData.products.length > this.maxItems) {
            errors.products = `حداکثر ${this.maxItems} محصول قابل مقایسه است`;
        }

        return {
            isValid: Object.keys(errors).length === 0,
            errors: errors
        };
    }
}

// Create global instance
window.comparisonService = new ComparisonService();
