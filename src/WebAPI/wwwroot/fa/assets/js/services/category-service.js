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
            const response = await this.apiClient.get('/ProductCategory');
            if (response.success !== undefined && !response.success) {
                return response;
            }
            
            // Handle different response structures
            const data = response.data || response;
            const finalData = data.data || data;
            return {
                success: true,
                data: finalData
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
            const response = await this.apiClient.get(`/ProductCategory/${categoryId}`);
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
            const response = await this.apiClient.get('/ProductCategory/tree');
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
            const response = await this.apiClient.get(`/ProductCategory/${categoryId}/subcategories`);
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

    /**
     * Render categories in mega menu
     * @param {string} containerId - ID of container element
     */
    async renderMegaMenu(containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;

        try {
            const result = await this.getAllCategories();
            if (!result.success || !result.data || result.data.length === 0) {
                container.innerHTML = '<p class="text-gray-500 p-4">دسته‌بندی یافت نشد</p>';
                return;
            }

            const categories = result.data;
            // Render category list
            let html = '<ul class="my-2 space-y-1">';
            categories.forEach((category) => {
                html += `
                    <li data-mega-id="${category.id}" 
                        class="px-4 w-full hover:bg-opacity-70 border-opacity-0 hover:border-opacity-100 rounded-lg dark:hover:text-zinc-950 mega-menu-li">
                        <a href="shop.html?category=${category.id}" class="flex items-center justify-between py-3">
                            <div class="flex items-center">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                    <path fill-rule="evenodd" d="M3 5a2 2 0 012-2h10a2 2 0 012 2v10a2 2 0 01-2 2H5a2 2 0 01-2-2V5zm11 1H6v8l4-2 4 2V6z" clip-rule="evenodd"/>
                                </svg>
                                <div class="ms-1">
                                    <p class="text-xs">${category.name || 'بدون نام'}</p>
                                </div>
                            </div>
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
                            </svg>
                        </a>
                    </li>
                `;
            });
            html += '</ul>';
            container.innerHTML = html;
        } catch (error) {
            if (window.logger) {
                window.logger.error('Error rendering mega menu:', error);
            } else {
                console.error('Error rendering mega menu:', error);
            }
            container.innerHTML = '<p class="text-gray-500 p-4">خطا در بارگذاری دسته‌بندی‌ها</p>';
        }
    }
}

// Create global instance
window.categoryService = new CategoryService();


