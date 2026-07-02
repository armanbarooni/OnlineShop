/**
 * Address Service for OnlineShop Frontend
 * Handles user address operations
 */

class AddressService {
    constructor() {
        this.apiClient = window.apiClient;
    }

    unwrapResponseData(response) {
        const payload = response?.data ?? response;
        return payload?.data ?? payload;
    }

    getCurrentUserId() {
        try {
            const token = localStorage.getItem('accessToken');
            if (token) {
                const payload = JSON.parse(atob(token.split('.')[1]));
                const tokenUserId =
                    payload.nameid ||
                    payload.nameidentifier ||
                    payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];

                if (tokenUserId) {
                    return tokenUserId;
                }
            }

            const cachedUser = this.apiClient?.getCurrentUser?.();
            return cachedUser?.id || null;
        } catch (error) {
            window.logger?.error('Error extracting user id from token:', error);
            return null;
        }
    }

    /**
     * Get user addresses
     */
    async getAddresses() {
        try {
            const userId = this.getCurrentUserId();
            if (!userId) {
                return {
                    success: false,
                    error: 'User not authenticated'
                };
            }

            const response = await this.apiClient.get(`/useraddress/user/${userId}`);
            return {
                success: true,
                data: this.unwrapResponseData(response)
            };
        } catch (error) {
            window.logger.error('Error fetching addresses:', error);
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
                data: this.unwrapResponseData(response)
            };
        } catch (error) {
            window.logger.error('Error fetching address:', error);
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
                data: this.unwrapResponseData(response),
                message: 'آدرس با موفقیت اضافه شد'
            };
        } catch (error) {
            window.logger.error('Error creating address:', error);
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
                data: this.unwrapResponseData(response),
                message: 'آدرس با موفقیت به‌روزرسانی شد'
            };
        } catch (error) {
            window.logger.error('Error updating address:', error);
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
            window.logger.error('Error deleting address:', error);
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
            window.logger.error('Error setting default address:', error);
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
                data: this.unwrapResponseData(response)
            };
        } catch (error) {
            window.logger.error('Error fetching default address:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error)
            };
        }
    }

    async getMahakRegions() {
        try {
            const response = await this.apiClient.get('/mahakregions');
            const data = this.unwrapResponseData(response);
            return {
                success: true,
                data: Array.isArray(data) ? data : []
            };
        } catch (error) {
            window.logger?.error('Error fetching Mahak regions:', error);
            return {
                success: false,
                error: this.apiClient.handleError(error),
                data: []
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

        if (!addressData.firstName || addressData.firstName.trim().length === 0) {
            errors.firstName = 'نام الزامی است';
        }

        if (!addressData.addressLine1 || addressData.addressLine1.trim().length === 0) {
            errors.addressLine1 = 'آدرس الزامی است';
        }

        if (!addressData.city || addressData.city.trim().length === 0) {
            errors.city = 'شهر الزامی است';
        }

        if (!addressData.state || addressData.state.trim().length === 0) {
            errors.state = 'استان الزامی است';
        }

        if (!addressData.mahakCityId || Number(addressData.mahakCityId) <= 0) {
            errors.mahakCityId = 'شهر را از لیست شهرهای محک انتخاب کنید';
        }

        if (addressData.postalCode && addressData.postalCode.trim().length > 0 && !/^\d{10}$/.test(addressData.postalCode)) {
            errors.postalCode = 'کد پستی باید ۱۰ رقم باشد';
        }

        if (addressData.phoneNumber && !window.utils.isValidPhone(addressData.phoneNumber)) {
            errors.phoneNumber = 'شماره تلفن معتبر نیست';
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
        
        const addressLine = address.addressLine1 + (address.addressLine2 ? ` - ${address.addressLine2}` : '');
        const parts = [
            address.title,
            `${address.firstName} ${address.lastName}`,
            addressLine,
            address.city,
            address.state,
            address.postalCode ? `کد پستی: ${address.postalCode}` : ''
        ];
        
        if (address.phoneNumber) {
            parts.push(`تلفن: ${address.phoneNumber}`);
        }
        
        return parts.filter(part => part && part.trim().length > 0).join(' - ');
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
                data: this.unwrapResponseData(response)
            };
        } catch (error) {
            window.logger.error('Error searching addresses:', error);
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
                data: this.unwrapResponseData(response)
            };
        } catch (error) {
            window.logger.error('Error fetching address statistics:', error);
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
            window.logger.error('Error duplicating address:', error);
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

