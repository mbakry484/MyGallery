/**
 * MyGallery Application
 * Handles UI interactions and rendering
 */
class App {
    constructor() {
        // DOM Elements
        this.homePageEl = document.getElementById('home-page');
        this.adminPageEl = document.getElementById('admin-page');
        this.navLinks = document.querySelectorAll('.nav-link');
        this.photosContainer = document.getElementById('photos-container');
        this.categoryFilterEl = document.getElementById('category-filter');
        this.filterButtonEl = document.getElementById('filter-button');
        this.photoCategoryEl = document.getElementById('photo-category');
        this.categoriesListEl = document.getElementById('categories-list');
        this.photosTableEl = document.getElementById('photos-table');
        this.addPhotoFormEl = document.getElementById('add-photo-form');
        this.addCategoryFormEl = document.getElementById('add-category-form');
        this.photoUrlEl = document.getElementById('photo-url');
        this.categoryNameEl = document.getElementById('category-name');

        // Toast elements
        this.toastEl = document.getElementById('toast');
        this.toastTitleEl = document.getElementById('toast-title');
        this.toastMessageEl = document.getElementById('toast-message');
        this.toast = new bootstrap.Toast(this.toastEl);

        // Current state
        this.currentPage = 'home';
        this.currentCategoryFilter = 0;
        this.allPhotos = [];
        this.categories = [];

        // Initialize the app
        this.initEventListeners();
        this.loadInitialData();
    }

    /**
     * Set up event listeners
     */
    initEventListeners() {
        // Navigation
        this.navLinks.forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const page = e.target.getAttribute('data-page');
                this.navigateTo(page);
            });
        });

        // Filter button
        this.filterButtonEl.addEventListener('click', () => {
            this.currentCategoryFilter = parseInt(this.categoryFilterEl.value);
            this.renderPhotos();
        });

        // Add photo form
        this.addPhotoFormEl.addEventListener('submit', (e) => {
            e.preventDefault();
            this.handleAddPhoto();
        });

        // Add category form
        this.addCategoryFormEl.addEventListener('submit', (e) => {
            e.preventDefault();
            this.handleAddCategory();
        });
    }

    /**
     * Load initial data from API
     */
    async loadInitialData() {
        try {
            // Load categories first, then photos
            await this.loadCategories();
            await this.loadPhotos();
        } catch (error) {
            this.showToast('Error', `Failed to load data: ${error.message}`, 'danger');
        }
    }

    /**
     * Load photos from API
     */
    async loadPhotos() {
        try {
            this.allPhotos = await api.getPhotos();
            this.renderPhotos();
            this.renderPhotosTable();
        } catch (error) {
            console.error('Error loading photos:', error);
            this.photosContainer.innerHTML = `
                <div class="col-12 text-center">
                    <div class="alert alert-danger">
                        Failed to load photos: ${error.message}
                    </div>
                </div>
            `;
            this.photosTableEl.innerHTML = `
                <tr>
                    <td colspan="4" class="text-center">
                        <div class="alert alert-danger">
                            Failed to load photos: ${error.message}
                        </div>
                    </td>
                </tr>
            `;
        }
    }

    /**
     * Load categories from API
     */
    async loadCategories() {
        try {
            this.categories = await api.getCategories();
            this.renderCategoryOptions();
            this.renderCategoriesList();
        } catch (error) {
            console.error('Error loading categories:', error);
            this.categoriesListEl.innerHTML = `
                <li class="list-group-item text-center">
                    <div class="alert alert-danger">
                        Failed to load categories: ${error.message}
                    </div>
                </li>
            `;
        }
    }

    /**
     * Navigate to a specific page
     */
    navigateTo(page) {
        // Update current page
        this.currentPage = page;

        // Update UI
        if (page === 'home') {
            this.homePageEl.style.display = 'block';
            this.adminPageEl.style.display = 'none';
        } else if (page === 'admin') {
            this.homePageEl.style.display = 'none';
            this.adminPageEl.style.display = 'block';
        }

        // Update active navigation
        this.navLinks.forEach(link => {
            if (link.getAttribute('data-page') === page) {
                link.classList.add('active');
            } else {
                link.classList.remove('active');
            }
        });
    }

    /**
     * Render photos in the gallery
     */
    renderPhotos() {
        // Filter photos if needed
        const photos = this.currentCategoryFilter === 0
            ? this.allPhotos
            : this.allPhotos.filter(p => p.categoryId === this.currentCategoryFilter);

        // Render photos
        if (photos.length === 0) {
            this.photosContainer.innerHTML = `
                <div class="col-12 text-center">
                    <div class="alert alert-info">
                        No photos found. ${this.currentCategoryFilter > 0 ? 'Try another category filter.' : ''}
                    </div>
                </div>
            `;
            return;
        }

        this.photosContainer.innerHTML = photos.map(photo => `
            <div class="col-md-4 col-sm-6">
                <div class="card photo-card">
                    <div class="photo-img-container">
                        <img src="${photo.imageUrl}" alt="Photo" class="photo-img">
                        <span class="badge bg-primary category-badge">${photo.categoryName}</span>
                    </div>
                    <div class="card-body">
                        <p class="card-text text-muted">ID: ${photo.id}</p>
                    </div>
                </div>
            </div>
        `).join('');
    }

    /**
     * Render categories in select dropdowns
     */
    renderCategoryOptions() {
        // Create options HTML
        const options = this.categories.map(category =>
            `<option value="${category.id}">${category.name}</option>`
        ).join('');

        // Update filter dropdown
        this.categoryFilterEl.innerHTML = `
            <option value="0">All Categories</option>
            ${options}
        `;

        // Update photo form dropdown
        this.photoCategoryEl.innerHTML = options;
    }

    /**
     * Render categories in the admin list
     */
    renderCategoriesList() {
        if (this.categories.length === 0) {
            this.categoriesListEl.innerHTML = `
                <li class="list-group-item text-center">
                    <div class="alert alert-info">No categories found.</div>
                </li>
            `;
            return;
        }

        this.categoriesListEl.innerHTML = this.categories.map(category => `
            <li class="list-group-item category-item">
                <span class="category-name">${category.name}</span>
                <button class="btn btn-sm btn-danger delete-category" data-id="${category.id}">
                    <i class="bi bi-trash"></i>
                </button>
            </li>
        `).join('');

        // Add event listeners to delete buttons
        document.querySelectorAll('.delete-category').forEach(button => {
            button.addEventListener('click', (e) => {
                const categoryId = parseInt(e.currentTarget.getAttribute('data-id'));
                this.handleDeleteCategory(categoryId);
            });
        });
    }

    /**
     * Render photos in the admin table
     */
    renderPhotosTable() {
        if (this.allPhotos.length === 0) {
            this.photosTableEl.innerHTML = `
                <tr>
                    <td colspan="4" class="text-center">
                        <div class="alert alert-info">No photos found.</div>
                    </td>
                </tr>
            `;
            return;
        }

        this.photosTableEl.innerHTML = this.allPhotos.map(photo => `
            <tr>
                <td>${photo.id}</td>
                <td>
                    <img src="${photo.imageUrl}" alt="Photo preview" class="photo-preview">
                </td>
                <td>${photo.categoryName}</td>
                <td>
                    <button class="btn btn-sm btn-danger action-btn delete-photo" data-id="${photo.id}">
                        <i class="bi bi-trash"></i> Delete
                    </button>
                </td>
            </tr>
        `).join('');

        // Add event listeners to delete buttons
        document.querySelectorAll('.delete-photo').forEach(button => {
            button.addEventListener('click', (e) => {
                const photoId = parseInt(e.currentTarget.getAttribute('data-id'));
                this.handleDeletePhoto(photoId);
            });
        });
    }

    /**
     * Handle adding a new photo
     */
    async handleAddPhoto() {
        const photoData = {
            imageUrl: this.photoUrlEl.value,
            categoryId: parseInt(this.photoCategoryEl.value)
        };

        try {
            await api.createPhoto(photoData);
            this.showToast('Success', 'Photo added successfully');
            this.photoUrlEl.value = '';
            await this.loadPhotos();
        } catch (error) {
            this.showToast('Error', `Failed to add photo: ${error.message}`, 'danger');
        }
    }

    /**
     * Handle adding a new category
     */
    async handleAddCategory() {
        const categoryData = {
            name: this.categoryNameEl.value
        };

        try {
            await api.createCategory(categoryData);
            this.showToast('Success', 'Category added successfully');
            this.categoryNameEl.value = '';
            await this.loadCategories();
        } catch (error) {
            this.showToast('Error', `Failed to add category: ${error.message}`, 'danger');
        }
    }

    /**
     * Handle deleting a photo
     */
    async handleDeletePhoto(photoId) {
        if (!confirm('Are you sure you want to delete this photo?')) {
            return;
        }

        try {
            await api.deletePhoto(photoId);
            this.showToast('Success', 'Photo deleted successfully');
            await this.loadPhotos();
        } catch (error) {
            this.showToast('Error', `Failed to delete photo: ${error.message}`, 'danger');
        }
    }

    /**
     * Handle deleting a category
     */
    async handleDeleteCategory(categoryId) {
        if (!confirm('Are you sure you want to delete this category?')) {
            return;
        }

        try {
            await api.deleteCategory(categoryId);
            this.showToast('Success', 'Category deleted successfully');
            await this.loadCategories();
            await this.loadPhotos(); // Reload photos as they may have been affected
        } catch (error) {
            this.showToast('Error', `Failed to delete category: ${error.message}`, 'danger');
        }
    }

    /**
     * Show a toast notification
     */
    showToast(title, message, type = 'success') {
        this.toastEl.classList.remove('bg-success', 'bg-danger');
        this.toastEl.classList.add(`bg-${type}`);
        this.toastTitleEl.textContent = title;
        this.toastMessageEl.textContent = message;
        this.toast.show();
    }
}

// Initialize the app when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.app = new App();
}); 