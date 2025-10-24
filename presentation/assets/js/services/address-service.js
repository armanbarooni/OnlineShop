/**
 * Address Service for OnlineShop Frontend
 * Handles user address operations
 */

class AddressService {
    constructor() {
        this.apiClient = window.apiClient;
    }

    /**
     * Get user addresses
     */
    async getAddresses() {
        try {
            const response = await this.apiClient.get('/useraddress');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error fetching addresses:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get address by ID
     */
    async getAddressById(addressId) {
        try {
            const response = await this.apiClient.get(`/useraddress/${addressId}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error fetching address:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Create new address
     */
    async createAddress(addressData) {
        try {
            const response = await this.apiClient.post('/useraddress', addressData);
            return {
                success: true,
                data: response.data || response,
                message: 'آدرس با موفقیت اضافه شد'
            };
        } catch (error) {
            console.error('Error creating address:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Update address
     */
    async updateAddress(addressId, addressData) {
        try {
            const response = await this.apiClient.put(`/useraddress/${addressId}`, addressData);
            return {
                success: true,
                data: response.data || response,
                message: 'آدرس با موفقیت بروزرسانی شد'
            };
        } catch (error) {
            console.error('Error updating address:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Delete address
     */
    async deleteAddress(addressId) {
        try {
            const response = await this.apiClient.delete(`/useraddress/${addressId}`);
            return {
                success: true,
                message: 'آدرس با موفقیت حذف شد'
            };
        } catch (error) {
            console.error('Error deleting address:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Set address as default
     */
    async setDefaultAddress(addressId) {
        try {
            const response = await this.apiClient.post(`/useraddress/${addressId}/set-default`);
            return {
                success: true,
                message: 'آدرس پیش‌فرض با موفقیت تنظیم شد'
            };
        } catch (error) {
            console.error('Error setting default address:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get default address
     */
    async getDefaultAddress() {
        try {
            const response = await this.apiClient.get('/useraddress/default');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error fetching default address:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Validate address data
     */
    validateAddressData(addressData) {
        const errors = {};

        if (!addressData.title || addressData.title.trim().length === 0) {
            errors.title = 'عنوان آدرس الزامی است';
        }

        if (!addressData.address || addressData.address.trim().length === 0) {
            errors.address = 'آدرس الزامی است';
        }

        if (!addressData.city || addressData.city.trim().length === 0) {
            errors.city = 'شهر الزامی است';
        }

        if (!addressData.province || addressData.province.trim().length === 0) {
            errors.province = 'استان الزامی است';
        }

        if (!addressData.postalCode || addressData.postalCode.trim().length === 0) {
            errors.postalCode = 'کد پستی الزامی است';
        } else if (!/^\d{10}$/.test(addressData.postalCode)) {
            errors.postalCode = 'کد پستی باید ۱۰ رقم باشد';
        }

        if (!addressData.phone || addressData.phone.trim().length === 0) {
            errors.phone = 'شماره تلفن الزامی است';
        } else if (!window.utils.isValidPhone(addressData.phone)) {
            errors.phone = 'شماره تلفن نامعتبر است';
        }

        return {
            isValid: Object.keys(errors).length === 0,
            errors: errors
        };
    }

    /**
     * Format address for display
     */
    formatAddress(address) {
        if (!address) return '';
        
        const parts = [
            address.title,
            address.address,
            address.city,
            address.province,
            `کد پستی: ${address.postalCode}`,
            `تلفن: ${address.phone}`
        ].filter(part => part && part.trim().length > 0);
        
        return parts.join(' - ');
    }

    /**
     * Get address type label
     */
    getAddressTypeLabel(type) {
        const typeMap = {
            'Home': 'منزل',
            'Work': 'محل کار',
            'Other': 'سایر'
        };
        
        return typeMap[type] || type;
    }

    /**
     * Check if address can be deleted
     */
    canDeleteAddress(address) {
        return !address.isDefault;
    }

    /**
     * Get address type color
     */
    getAddressTypeColor(type) {
        const colorMap = {
            'Home': 'text-green-600 bg-green-100',
            'Work': 'text-blue-600 bg-blue-100',
            'Other': 'text-gray-600 bg-gray-100'
        };
        
        return colorMap[type] || 'text-gray-600 bg-gray-100';
    }

    /**
     * Search addresses
     */
    async searchAddresses(query) {
        try {
            const response = await this.apiClient.get(`/useraddress/search?q=${encodeURIComponent(query)}`);
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error searching addresses:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Get address statistics
     */
    async getAddressStatistics() {
        try {
            const response = await this.apiClient.get('/useraddress/statistics');
            return {
                success: true,
                data: response.data || response
            };
        } catch (error) {
            console.error('Error fetching address statistics:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    /**
     * Duplicate address
     */
    async duplicateAddress(addressId) {
        try {
            const addressResponse = await this.getAddressById(addressId);
            if (!addressResponse.success) {
                return addressResponse;
            }

            const address = addressResponse.data;
            const duplicatedAddress = {
                ...address,
                title: `${address.title} (کپی)`,
                isDefault: false
            };

            delete duplicatedAddress.id;
            delete duplicatedAddress.createdAt;
            delete duplicatedAddress.updatedAt;

            return await this.createAddress(duplicatedAddress);
        } catch (error) {
            console.error('Error duplicating address:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }
}

// Create global instance
window.addressService = new AddressService();

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AddressService;
}
