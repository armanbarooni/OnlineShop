/**
 * Category Service
 * Handles all category-related API calls
 */
class CategoryService {
    constructor() {
        this.apiClient = window.apiClient;
    }

    /**
     * Get all categories
     */
    async getAllCategories() {
        try {
            const response = await this.apiClient.get('/api/ProductCategory');
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            // Handle different response structures
            const data = response.data || response;
            return {
                success: true,
                data: data.data || data
            };
        } catch (error) {
            window.logger.error('Error getting categories:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت دسته‌بندی‌ها'
            };
        }
    }

    /**
     * Get category by ID
     */
    async getCategoryById(categoryId) {
        try {
            const response = await this.apiClient.get(`/api/ProductCategory/${categoryId}`);
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            const data = response.data || response;
            return {
                success: true,
                data: data.data || data
            };
        } catch (error) {
            window.logger.error('Error getting category:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت اطلاعات دسته‌بندی'
            };
        }
    }

    /**
     * Get category tree
     */
    async getCategoryTree() {
        try {
            const response = await this.apiClient.get('/api/ProductCategory/tree');
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            const data = response.data || response;
            return {
                success: true,
                data: data.data || data
            };
        } catch (error) {
            window.logger.error('Error getting category tree:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت درخت دسته‌بندی‌ها'
            };
        }
    }

    /**
     * Get subcategories for a category
     */
    async getSubCategories(categoryId) {
        try {
            const response = await this.apiClient.get(`/api/ProductCategory/${categoryId}/subcategories`);
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            const data = response.data || response;
            return {
                success: true,
                data: data.data || data
            };
        } catch (error) {
            window.logger.error('Error getting subcategories:', error);
            return {
                success: false,
                error: error.message || 'خطا در دریافت زیردسته‌بندی‌ها'
            };
        }
    }

    /**
     * Build category hierarchy from flat list
     */
    buildCategoryHierarchy(categories) {
        if (!Array.isArray(categories)) {
            return [];
        }

        const categoryMap = new Map();
        const rootCategories = [];

        // First pass: create map of all categories
        categories.forEach(category => {
            categoryMap.set(category.id, {
                ...category,
                children: []
            });
        });

        // Second pass: build hierarchy
        categories.forEach(category => {
            const categoryNode = categoryMap.get(category.id);
            if (category.parentCategoryId) {
                const parent = categoryMap.get(category.parentCategoryId);
                if (parent) {
                    parent.children.push(categoryNode);
                } else {
                    rootCategories.push(categoryNode);
                }
            } else {
                rootCategories.push(categoryNode);
            }
        });

        return rootCategories;
    }

    /**
     * Get category breadcrumb
     */
    getCategoryBreadcrumb(categories, categoryId) {
        const breadcrumb = [];
        const categoryMap = new Map(categories.map(c => [c.id, c]));
        
        let current = categoryMap.get(categoryId);
        while (current) {
            breadcrumb.unshift(current);
            if (current.parentCategoryId) {
                current = categoryMap.get(current.parentCategoryId);
            } else {
                break;
            }
        }
        
        return breadcrumb;
    }

    /**
     * Find category by slug or name
     */
    findCategoryBySlug(categories, slug) {
        return categories.find(c => 
            c.slug === slug || 
            c.name.toLowerCase().replace(/\s+/g, '-') === slug.toLowerCase()
        );
    }

    /**
     * Get all category IDs in hierarchy (including children)
     */
    getAllCategoryIdsInHierarchy(categories, categoryId) {
        const ids = [categoryId];
        const children = categories.filter(c => c.parentCategoryId === categoryId);
        
        children.forEach(child => {
            ids.push(...this.getAllCategoryIdsInHierarchy(categories, child.id));
        });
        
        return ids;
    }
}

// Create global instance
window.categoryService = new CategoryService();


