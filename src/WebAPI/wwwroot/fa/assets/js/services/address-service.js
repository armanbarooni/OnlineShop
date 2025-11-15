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
            // Get current user ID from token
            const token = localStorage.getItem('accessToken');
            if (!token) {
                return {
                    success: false,
                    error: 'User not authenticated'
                };
            }

            // Decode token to get user ID
            const payload = JSON.parse(atob(token.split('.')[1]));
            const userId = payload.sub || payload.nameid;

            const response = await this.apiClient.get(`/useraddress/user/${userId}`);
            return {
                success: true,
                data: response.data || response
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
                data: response.data || response
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
                data: response.data || response,
                message: 'ط¢ط¯ط±ط³ ط¨ط§ ظ…ظˆظپظ‚غŒطھ ط§ط¶ط§ظپظ‡ ط´ط¯'
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
                data: response.data || response,
                message: 'ط¢ط¯ط±ط³ ط¨ط§ ظ…ظˆظپظ‚غŒطھ ط¨ط±ظˆط²ط±ط³ط§ظ†غŒ ط´ط¯'
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
                message: 'ط¢ط¯ط±ط³ ط¨ط§ ظ…ظˆظپظ‚غŒطھ ط­ط°ظپ ط´ط¯'
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
                message: 'ط¢ط¯ط±ط³ ظ¾غŒط´â€Œظپط±ط¶ ط¨ط§ ظ…ظˆظپظ‚غŒطھ طھظ†ط¸غŒظ… ط´ط¯'
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
                data: response.data || response
            };
        } catch (error) {
            window.logger.error('Error fetching default address:', error);
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
            errors.title = 'ط¹ظ†ظˆط§ظ† ط¢ط¯ط±ط³ ط§ظ„ط²ط§ظ…غŒ ط§ط³طھ';
        }

        if (!addressData.firstName || addressData.firstName.trim().length === 0) {
            errors.firstName = 'ظ†ط§ظ… ط§ظ„ط²ط§ظ…غŒ ط§ط³طھ';
        }

        if (!addressData.addressLine1 || addressData.addressLine1.trim().length === 0) {
            errors.addressLine1 = 'ط¢ط¯ط±ط³ ط§ظ„ط²ط§ظ…غŒ ط§ط³طھ';
        }

        if (!addressData.city || addressData.city.trim().length === 0) {
            errors.city = 'ط´ظ‡ط± ط§ظ„ط²ط§ظ…غŒ ط§ط³طھ';
        }

        if (!addressData.state || addressData.state.trim().length === 0) {
            errors.state = 'ط§ط³طھط§ظ† ط§ظ„ط²ط§ظ…غŒ ط§ط³طھ';
        }

        if (!addressData.postalCode || addressData.postalCode.trim().length === 0) {
            errors.postalCode = 'ع©ط¯ ظ¾ط³طھغŒ ط§ظ„ط²ط§ظ…غŒ ط§ط³طھ';
        } else if (!/^\d{10}$/.test(addressData.postalCode)) {
            errors.postalCode = 'ع©ط¯ ظ¾ط³طھغŒ ط¨ط§غŒط¯ غ±غ° ط±ظ‚ظ… ط¨ط§ط´ط¯';
        }

        if (addressData.phoneNumber && !window.utils.isValidPhone(addressData.phoneNumber)) {
            errors.phoneNumber = 'ط´ظ…ط§ط±ظ‡ طھظ„ظپظ† ظ†ط§ظ…ط¹طھط¨ط± ط§ط³طھ';
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
            `ع©ط¯ ظ¾ط³طھغŒ: ${address.postalCode}`
        ];
        
        if (address.phoneNumber) {
            parts.push(`طھظ„ظپظ†: ${address.phoneNumber}`);
        }
        
        return parts.filter(part => part && part.trim().length > 0).join(' - ');
    }

    /**
     * Get address type label
     */
    getAddressTypeLabel(type) {
        const typeMap = {
            'Home': 'ظ…ظ†ط²ظ„',
            'Work': 'ظ…ط­ظ„ ع©ط§ط±',
            'Other': 'ط³ط§غŒط±'
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
                data: response.data || response
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
                title: `${address.title} (ع©ظ¾غŒ)`,
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

