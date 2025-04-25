/**
 * API Service for MyGallery
 * Handles all API calls to the backend
 */
class ApiService {
    constructor() {
        this.baseUrl = '/api';
    }

    /**
     * Generic API call handler with error handling
     */
    async apiCall(endpoint, method = 'GET', data = null) {
        const url = `${this.baseUrl}/${endpoint}`;

        const options = {
            method,
            headers: {
                'Content-Type': 'application/json'
            }
        };

        if (data && (method === 'POST' || method === 'PUT')) {
            options.body = JSON.stringify(data);
        }

        try {
            const response = await fetch(url, options);

            // Handle non-2xx responses
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({
                    message: 'An unknown error occurred'
                }));

                throw new Error(errorData.message || `API Error: ${response.status} ${response.statusText}`);
            }

            // Check if the response has content
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            }

            return null;
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    }

    // Photos API
    async getPhotos() {
        return this.apiCall('photos');
    }

    async getPhotoById(id) {
        return this.apiCall(`photos/${id}`);
    }

    async createPhoto(photoData) {
        return this.apiCall('photos', 'POST', photoData);
    }

    async updatePhoto(id, photoData) {
        return this.apiCall(`photos/${id}`, 'PUT', photoData);
    }

    async deletePhoto(id) {
        return this.apiCall(`photos/${id}`, 'DELETE');
    }

    // Categories API
    async getCategories() {
        return this.apiCall('categories');
    }

    async getCategoryById(id) {
        return this.apiCall(`categories/${id}`);
    }

    async createCategory(categoryData) {
        return this.apiCall('categories', 'POST', categoryData);
    }

    async deleteCategory(id) {
        return this.apiCall(`categories/${id}`, 'DELETE');
    }
}

// Export a singleton instance
const api = new ApiService(); 