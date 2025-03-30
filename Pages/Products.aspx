<%@ Page Title="Products" 
         Language="C#" 
         MasterPageFile="~/Pages/AdminMaster.master" 
         AutoEventWireup="true" 
         CodeBehind="Products.aspx.cs" 
         Inherits="OnlinePastryShop.Pages.Products" %>
<asp:Content ID="Content1" ContentPlaceHolderID="AdminContent" runat="server">
    <form id="form1" runat="server" novalidate>
        <!-- ScriptManager for AJAX -->
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        
        <!-- Add this right after your ScriptManager -->
        <div id="debugInfo" class="bg-gray-100 p-4 mb-4 hidden">
            <h3 class="font-bold">Debug Information</h3>
            <div class="flex justify-between items-center mb-2">
                <span class="text-sm text-gray-500">Debug logs will appear below:</span>
                <button type="button" onclick="hideDebugInfo()" class="text-gray-500 hover:text-gray-700">
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path d="M6 18L18 6M6 6l12 12"></path>
                    </svg>
                </button>
            </div>
            <pre id="debugOutput" class="whitespace-pre-wrap"></pre>
        </div>
        
        <!-- Message Label -->
        <asp:Label ID="lblMessage" runat="server" CssClass="text-red-500 mb-4" Visible="false"></asp:Label>
        
        <!-- Dashboard Stats -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
            <div class="bg-white p-4 rounded-lg shadow">
                <h3 class="text-gray-500 text-sm">Total Products</h3>
                <p class="text-2xl font-bold text-[#D43B6A]" id="totalProducts">0</p>
            </div>
            <div class="bg-white p-4 rounded-lg shadow">
                <h3 class="text-gray-500 text-sm">Low Stock Items</h3>
                <p class="text-2xl font-bold text-orange-500" id="lowStockCount">0</p>
            </div>
            <div class="bg-white p-4 rounded-lg shadow">
                <h3 class="text-gray-500 text-sm">Out of Stock</h3>
                <p class="text-2xl font-bold text-red-500" id="outOfStockCount">0</p>
            </div>
        </div>
        
        <!-- Search and Filters -->
        <div class="bg-white p-4 rounded-lg shadow mb-6">
            <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                <div class="relative">
                    <input type="text" id="searchInput" placeholder="Search products..." 
                           class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-[#D43B6A] focus:border-transparent" />
                    <span class="absolute right-3 top-2.5 text-gray-400">
                        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path>
                        </svg>
                    </span>
                </div>
                <select id="categoryFilter" class="px-4 py-2 border rounded-lg focus:ring-2 focus:ring-[#D43B6A] focus:border-transparent">
                    <option value="">All Categories</option>
                </select>
                <div class="flex items-center gap-4">
                    <select id="sortOptions" class="px-4 py-2 border rounded-lg focus:ring-2 focus:ring-[#D43B6A] focus:border-transparent flex-grow">
                        <option value="newest">Newest First</option>
                        <option value="name_asc">Name (A to Z)</option>
                        <option value="name_desc">Name (Z to A)</option>
                        <option value="price_asc">Price (Low to High)</option>
                        <option value="price_desc">Price (High to Low)</option>
                        <option value="stock_asc">Stock (Low to High)</option>
                        <option value="stock_desc">Stock (High to Low)</option>
                    </select>
                </div>
                <button type="button" onclick="openAddModal()" 
                        class="bg-green-600 text-white px-6 py-2 rounded-lg hover:bg-green-700 transition-colors">
                    Add Product
                </button>
            </div>
        </div>
        
        <!-- Products List -->
        <div class="bg-white rounded-lg shadow">
            <!-- Filter indicator for live search -->
            <div id="liveSearchIndicator" class="px-4 py-2 text-sm text-blue-700 bg-blue-50 border-b border-blue-100 hidden">
                <span class="flex items-center">
                    <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z"></path>
                    </svg>
                    <span id="filterMessage">Filtering products as you type...</span>
                </span>
            </div>
            <div class="overflow-x-auto">
                <table class="min-w-full divide-y divide-gray-200">
                    <thead class="bg-gray-50">
                        <tr>
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Product</th>
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Category</th>
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Cost</th>
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Price</th>
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Profit</th>
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Stock</th>
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                            <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                        </tr>
                    </thead>
                    <tbody id="productsList" class="bg-white divide-y divide-gray-200">
                        <!-- Products will be dynamically inserted here -->
                    </tbody>
                </table>
            </div>
            <!-- Pagination -->
            <div class="bg-white border-t border-gray-200 px-4 py-3 flex items-center justify-between">
                <div class="flex-1 flex justify-between sm:hidden">
                    <button type="button" id="prevPageMobile" class="relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50">&laquo; Previous</button>
                    <button type="button" id="nextPageMobile" class="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50">Next &raquo;</button>
                </div>
                <div class="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
                    <div>
                        <p class="text-sm text-gray-700">
                            <span>Showing page </span>
                            <span id="currentPageDisplay" class="font-medium">1</span>
                            <span> of </span>
                            <span id="totalPagesDisplay" class="font-medium">1</span>
                        </p>
                    </div>
                    <div>
                        <nav class="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" id="pagination" aria-label="Pagination">
                            <!-- Pagination buttons will be dynamically inserted here -->
                        </nav>
                    </div>
                </div>
            </div>
        </div>
        
        <!-- Add/Edit Product Modal - Updated with better UI/UX and scrolling -->
        <div id="productModal" class="fixed inset-0 bg-black bg-opacity-50 hidden flex items-center justify-center z-50">
            <div class="bg-white rounded-lg p-6 w-full max-w-2xl shadow-2xl max-h-[90vh] flex flex-col">
                <div class="flex justify-between items-center mb-4">
                    <h3 class="text-2xl font-bold text-gray-800" id="modalTitle">Add Product</h3>
                    <button type="button" onclick="closeModal()" class="text-gray-400 hover:text-gray-600 transition-colors">
                        <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path d="M6 18L18 6M6 6l12 12"></path>
                        </svg>
                    </button>
                </div>
                <!-- Modal error message container -->
                <div id="modalError" class="mb-4 px-4 py-3 rounded-lg text-red-700 bg-red-100 hidden">
                    <p id="modalErrorMessage" class="text-sm"></p>
                </div>
                <div class="overflow-y-auto pr-2 flex-grow" style="max-height: calc(90vh - 130px);">
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div class="col-span-2">
                            <label class="block text-sm font-semibold text-gray-700 mb-1">Product Name <span class="text-red-500 required-indicator">*</span></label>
                            <input type="text" id="productName" class="w-full px-3 py-2 border rounded-lg shadow-sm focus:ring-2 focus:ring-[#D43B6A] focus:border-transparent" minlength="3" maxlength="100" required>
                        </div>
                        <div class="col-span-2">
                            <label class="block text-sm font-semibold text-gray-700 mb-1">Description</label>
                            <textarea id="productDescription" class="w-full px-3 py-2 border rounded-lg shadow-sm focus:ring-2 focus:ring-[#D43B6A] focus:border-transparent" rows="2" maxlength="500"></textarea>
                            <p class="text-xs text-gray-500 mt-1">Maximum 50 words</p>
                        </div>
                        <div>
                            <label class="block text-sm font-semibold text-gray-700 mb-1">Price (₱) <span class="text-red-500 required-indicator">*</span></label>
                            <input type="number" id="productPrice" class="w-full px-3 py-2 border rounded-lg shadow-sm focus:ring-2 focus:ring-[#D43B6A] focus:border-transparent" min="0.01" max="10000" step="0.01" required>
                        </div>
                        <div>
                            <label class="block text-sm font-semibold text-gray-700 mb-1">Cost Price (₱) <span class="text-red-500 required-indicator">*</span></label>
                            <input type="number" id="productCostPrice" class="w-full px-3 py-2 border rounded-lg shadow-sm focus:ring-2 focus:ring-[#D43B6A] focus:border-transparent" min="0.01" max="10000" step="0.01" required>
                            <p class="text-xs text-gray-500 mt-1">The purchase price (should be less than selling price)</p>
                        </div>
                        <div>
                            <label class="block text-sm font-semibold text-gray-700 mb-1">Stock Quantity <span class="text-red-500 required-indicator">*</span></label>
                            <input type="number" id="productStock" class="w-full px-3 py-2 border rounded-lg shadow-sm focus:ring-2 focus:ring-[#D43B6A] focus:border-transparent" min="0" max="1000" required>
                            <p class="text-xs text-gray-500 mt-1">Maximum 1000 items</p>
                        </div>
                        <div>
                            <label class="block text-sm font-semibold text-gray-700 mb-1">Category <span class="text-red-500 required-indicator">*</span></label>
                            <select id="productCategory" class="w-full px-3 py-2 border rounded-lg shadow-sm focus:ring-2 focus:ring-[#D43B6A] focus:border-transparent" required>
                                <!-- Categories will be dynamically loaded -->
                            </select>
                        </div>
                        <div>
                            <label class="block text-sm font-semibold text-gray-700 mb-1">Latest Product</label>
                            <div class="flex items-center mt-1">
                                <input type="checkbox" id="isLatest" class="form-checkbox h-5 w-5 text-[#D43B6A] rounded">
                                <label for="isLatest" class="ml-2 text-sm text-gray-600">Mark as Latest Product</label>
                            </div>
                        </div>
                        <div class="col-span-2">
                            <label class="block text-sm font-semibold text-gray-700 mb-1">Product Image</label>
                            <div class="mt-1 flex justify-center px-4 pt-3 pb-4 border-2 border-gray-300 border-dashed rounded-lg hover:border-[#D43B6A] transition-colors">
                                <div class="space-y-1 text-center">
                                    <div class="relative mx-auto" style="width: 96px; height: 96px;">
                                        <img id="imagePreview" class="h-24 w-24 object-cover hidden rounded-lg shadow-md cursor-pointer" 
                                             onclick="document.getElementById('productImage').click();"
                                             title="Click to change image">
                                        <div id="imageOverlay" class="absolute inset-0 hidden flex items-center justify-center bg-black bg-opacity-40 rounded-lg cursor-pointer transition-opacity"
                                             onclick="document.getElementById('productImage').click();">
                                            <span class="text-white text-xs font-bold">Click to change</span>
                                        </div>
                                    </div>
                                    <div class="flex text-sm text-gray-600">
                                        <label for="productImage" class="relative cursor-pointer bg-white rounded-md font-medium text-[#D43B6A] hover:text-[#bf3660] transition-colors">
                                            <span>Upload a file</span>
                                            <input id="productImage" type="file" class="sr-only" accept="image/jpeg,image/png,image/jpg">
                                        </label>
                                        <p class="pl-1">or drag and drop</p>
                                    </div>
                                    <p class="text-xs text-gray-500">PNG, JPG, JPEG up to 5MB</p>
                                </div>
                            </div>
                    </div>
                    </div>
                </div>
                <div class="mt-4 pt-3 flex justify-end gap-3 border-t">
                    <button type="button" onclick="closeModal()" class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors">
                        Cancel
                    </button>
                    <button type="button" onclick="saveProduct()" class="px-4 py-2 text-sm font-medium text-white bg-green-600 rounded-lg hover:bg-green-700 transition-colors shadow-sm">
                        Save Product
                    </button>
                </div>
            </div>
        </div>
        
        <!-- Delete Confirmation Modal -->
        <div id="deleteModal" class="fixed inset-0 bg-black bg-opacity-50 hidden flex items-center justify-center z-50">
            <div class="bg-white rounded-lg p-6 w-full max-w-md">
                <h3 class="text-xl font-bold text-gray-900 mb-4">Confirm Permanent Deletion</h3>
                <p class="text-gray-500 mb-2">Are you sure you want to permanently delete this product?</p>
                <p class="text-red-500 font-medium mb-6">This action cannot be undone and will completely remove the product from the database.</p>
                <div class="flex justify-end gap-4">
                    <button type="button" onclick="closeDeleteModal()" class="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200">
                        Cancel
                    </button>
                    <button type="button" onclick="deleteProduct()" class="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700">
                        Permanently Delete
                    </button>
                </div>
            </div>
        </div>
        
        <!-- Toast Notification -->
        <div id="toast" class="fixed bottom-4 right-4 px-6 py-4 rounded-lg shadow-lg hidden">
            <div class="flex items-center">
                <span id="toastMessage" class="text-white"></span>
            </div>
        </div>
        
        <!-- Developer Debug Tools (hidden in footer) -->
        <div class="fixed bottom-0 left-0 p-2 opacity-30 hover:opacity-100 transition-opacity z-40">
            <button type="button" id="devModeToggle" class="text-xs bg-gray-100 p-1 rounded mr-1" onclick="toggleDevMode()">Dev: OFF</button>
            <button type="button" class="text-xs bg-gray-100 p-1 rounded" onclick="showDebugInfo()">Show Debug</button>
        </div>
    </form>
    
    <script type="text/javascript">
        let currentPage = 1;
        const pageSize = 10;
        let debounceTimer;
        let selectedProductId = null;
        let devMode = false; // Development mode flag
        // Image cache management
        const IMAGE_CACHE_MAX_SIZE = 20; // Maximum number of images to keep in cache
        const imageLoadPriority = []; // Queue to track image load order for LRU cache

        // Load products when page loads
        document.addEventListener('DOMContentLoaded', function () {
            console.log("Products page DOM loaded");

            // Check URL parameters and localStorage for sorting preference
            checkForSortPreference();

            loadCategories();
            loadProducts();

            // Setup event listeners
            document.getElementById('searchInput').addEventListener('keyup', function (e) {
                debounceSearch();
            });
            document.getElementById('categoryFilter').addEventListener('change', function () {
                currentPage = 1;
                loadProducts();
            });
            document.getElementById('sortOptions').addEventListener('change', function () {
                loadProducts();
            });

            // Setup drag and drop for image upload
            const dropZone = document.querySelector('.border-dashed');
            dropZone.addEventListener('dragover', handleDragOver);
            dropZone.addEventListener('drop', handleDrop);

            // Setup image upload handler
            document.getElementById('productImage').addEventListener('change', handleImageUpload);
        });

        function loadProducts() {
            const search = document.getElementById('searchInput').value;
            const categoryId = document.getElementById('categoryFilter').value;
            const sort = document.getElementById('sortOptions').value;

            // Show loading state
            document.getElementById('productsList').innerHTML = `
                <tr>
                    <td colspan="8" class="px-6 py-4 text-center">
                        Loading products...
                    </td>
                </tr>
            `;

            // Display active filters if any
            let activeFilters = [];
            if (search) activeFilters.push(`search: "${search}"`);
            if (categoryId) {
                const categoryName = document.getElementById('categoryFilter').options[document.getElementById('categoryFilter').selectedIndex].text;
                activeFilters.push(`category: "${categoryName}"`);
            }
            if (sort && sort !== "newest") {
                const sortName = document.getElementById('sortOptions').options[document.getElementById('sortOptions').selectedIndex].text;
                activeFilters.push(`sort: "${sortName}"`);
            }

            if (activeFilters.length > 0) {
                debugLog('Active filters and sorting:', activeFilters.join(', '));
            }

            debugLog('Fetching products with params:', {
                search,
                categoryId,
                sort,
                page: currentPage,
                pageSize,
                url: 'Products.aspx/GetProducts'
            });

            // Return the promise so we can chain .then() after loadProducts()
            return fetch('Products.aspx/GetProducts', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    search: search || "",
                    categoryId: categoryId || "",
                    sort: sort || "",
                    page: currentPage,
                    pageSize: pageSize,
                    includeDeleted: false
                })
            })
                .then(response => {
                    debugLog('Response status:', response.status);
                    return response.json();
                })
                .then(data => {
                    debugLog('Server response:', data);

                    if (!data.d) {
                        throw new Error('Invalid server response');
                    }

                    const result = data.d;
                    debugLog('Parsed result:', result);

                    if (!result.Products) {
                        throw new Error('No products array in response');
                    }

                    debugLog('Products array:', result.Products);
                    renderProducts(result.Products);
                    updatePagination(result.TotalCount);
                    loadDashboardStats();

                    // Return the result so we can chain further processing
                    return result;
                })
                .catch(error => {
                    console.error('Error:', error);
                    debugLog('Error loading products:', error.message, true);
                    document.getElementById('productsList').innerHTML = `
                    <tr>
                        <td colspan="8" class="px-6 py-4 text-center text-red-500">
                            Error loading products: ${error.message}
                        </td>
                    </tr>
                `;

                    // Re-throw the error to be handled by the caller if needed
                    throw error;
                });
        }

        function loadCategories() {
            fetch('Products.aspx/GetCategories', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            })
                .then(response => response.json())
                .then(data => {
                    const categories = data.d;
                    const categoryFilter = document.getElementById('categoryFilter');
                    const productCategory = document.getElementById('productCategory');

                    // Clear existing options
                    categoryFilter.innerHTML = '<option value="">All Categories</option>';
                    productCategory.innerHTML = '<option value="">Select Category</option>';

                    categories.forEach(category => {
                        categoryFilter.add(new Option(category.Name, category.CategoryId));
                        productCategory.add(new Option(category.Name, category.CategoryId));
                    });
                })
                .catch(error => {
                    console.error('Error loading categories:', error);
                    showToast('Error loading categories', 'error');
                });
        }

        function renderProducts(products) {
            console.log('RENDER PRODUCTS FUNCTION CALLED', { productsArray: products });

            const tbody = document.getElementById('productsList');

            // Use document fragment for better performance
            const fragment = document.createDocumentFragment();

            if (!products || products.length === 0) {
                console.log('NO PRODUCTS TO RENDER - Showing empty state');
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td colspan="6" class="px-6 py-4 text-center text-gray-500">
                        No products found. Please add some products.
                    </td>
                `;
                fragment.appendChild(tr);
                tbody.innerHTML = '';
                tbody.appendChild(fragment);
                return;
            }

            console.log('RENDERING ' + products.length + ' PRODUCTS');

            products.forEach((product, index) => {
                console.log(`Processing product ${index + 1}/${products.length}:`, {
                    id: product.ProductId,
                    name: product.Name,
                    price: product.Price,
                    category: product.CategoryName
                });

                const tr = document.createElement('tr');

                // Create image placeholder - don't load images immediately
                const imageCell = `
                    <div class="h-10 w-10 rounded-full bg-gray-200 mr-3 flex items-center justify-center overflow-hidden">
                        ${product.HasImage ?
                        `<img src="data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7" 
                                 data-product-id="${product.ProductId}"
                                 data-lazy-load="true"
                                 alt="${product.Name}" 
                                 class="h-10 w-10 object-cover rounded-full">` :
                        '<span class="text-gray-500 text-xs">No image</span>'}
                    </div>
                `;

                try {
                    // Calculate profit and margin
                    const profit = product.Price - product.CostPrice;
                    const profitMargin = product.Price > 0 ? ((profit / product.Price) * 100).toFixed(1) : 0;

                    const tr = document.createElement('tr');
                    tr.classList.add('hover:bg-gray-50');
                    tr.dataset.productId = product.ProductId;
                    tr.innerHTML = `
                        <td class="px-6 py-4 whitespace-nowrap">
                            <div class="flex items-center">
                                ${imageCell}
                                <div>
                                    <div class="text-sm font-medium text-gray-900">${product.Name}</div>
                                    <div class="text-xs text-gray-500">${product.Description || ''}</div>
                                </div>
                            </div>
                        </td>
                        <td class="px-6 py-4">${product.CategoryName}</td>
                        <td class="px-6 py-4">₱${product.CostPrice.toFixed(2)}</td>
                        <td class="px-6 py-4">₱${product.Price.toFixed(2)}</td>
                        <td class="px-6 py-4">
                            <div>₱${profit.toFixed(2)}</div>
                            <div class="text-xs text-gray-500">${profitMargin}% margin</div>
                        </td>
                        <td class="px-6 py-4">${product.StockQuantity}</td>
                        <td class="px-6 py-4">${getStockStatusBadge(product.StockQuantity)}</td>
                        <td class="px-6 py-4 text-right">
                            <button type="button" class="text-blue-600 hover:text-blue-900 mr-3" onclick="openEditModal(${product.ProductId})">
                                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"></path>
                                </svg>
                            </button>
                            <button type="button" class="text-red-600 hover:text-red-900" onclick="confirmDelete(${product.ProductId}, '${product.Name.replace(/'/g, "\\'")}')">
                                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path>
                                </svg>
                            </button>
                        </td>
                    `;
                    fragment.appendChild(tr);
                    console.log(`Product ${product.ProductId} row created successfully`);
                } catch (error) {
                    console.error(`Error creating row for product ${product.ProductId}:`, error);
                    debugLog(`Error rendering product ${product.ProductId}`, error, true);
                }
            });

            console.log('Replacing tbody content with new rows');

            // Replace content in a single DOM operation
            tbody.innerHTML = '';
            tbody.appendChild(fragment);
            console.log('Table updated successfully');

            // Initialize lazy loading of images that are in the viewport
            initLazyLoadImages();
        }

        // New function to observe images and load them when visible
        function initLazyLoadImages() {
            // Use IntersectionObserver for better performance
            if ('IntersectionObserver' in window) {
                const lazyImageObserver = new IntersectionObserver((entries, observer) => {
                    entries.forEach(entry => {
                        if (entry.isIntersecting) {
                            const img = entry.target;
                            const productId = img.dataset.productId;

                            // Load the image
                            loadProductImage(productId, img);

                            // Stop watching this image
                            observer.unobserve(img);
                        }
                    });
                });

                // Observe all lazy load images
                document.querySelectorAll('img[data-lazy-load="true"]').forEach(img => {
                    lazyImageObserver.observe(img);
                });
            } else {
                // Fallback for browsers that don't support IntersectionObserver
                document.querySelectorAll('img[data-lazy-load="true"]').forEach(img => {
                    loadProductImage(img.dataset.productId, img);
                });
            }
        }

        function loadProductImage(productId, imgElement) {
            // Check cache first
            try {
                const cachedImage = sessionStorage.getItem(`product_image_${productId}`);
                if (cachedImage) {
                    // Update this image's priority in cache (mark as recently used)
                    updateImageCachePriority(productId);

                    // If in cache, use it immediately
                    imgElement.src = `data:image/jpeg;base64,${cachedImage}`;
                    imgElement.onerror = function () {
                        // If JPEG format fails, try as PNG
                        imgElement.src = `data:image/png;base64,${cachedImage}`;
                        imgElement.onerror = function () {
                            // If PNG fails too, try as a generic image
                            imgElement.src = `data:image;base64,${cachedImage}`;
                            imgElement.onerror = function () {
                                // If all attempts fail, show placeholder
                                imgElement.src = "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgdmlld0JveD0iMCAwIDI0IDI0IiBmaWxsPSJub25lIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxwYXRoIGQ9Ik0zIDh2MTFjMCAxLjEuOSAyIDIgMmgxNGMxLjEgMCAyLS45IDItMlY4TDMgOHpoLS41Yy0uMiAwLS4zLS4xLS4zLS4zVjRjMC0xLjEuOS0yIDItMmg0YzAgMCAxIDAgMSAuNS0uNC4zLS42LjgtLjYgMS4zVjZjMCAwIDAgLjUuNC44aDEuNGMuNCAwIC43LjQuOC45bC41IDEuM2g2LjN6IiBmaWxsPSIjZTllOWU5Ii8+PHBhdGggZD0iTTExIDE0bC03LTQgMTQgMC03IDR6IiBmaWxsPSIjZTllOWU5Ii8+PC9zdmc+";
                                console.error(`Failed to render image for product ${productId}`);
                            };
                        };
                    };
                    return;
                }
            } catch (e) {
                console.warn(`Cache read error for product ${productId}:`, e);
                // Continue with fetching the image
            }

            // Not in cache, fetch from server
            fetch('Products.aspx/GetProductImage', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ productId: productId })
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`HTTP error! Status: ${response.status}`);
                    }
                    return response.json();
                })
                .then(data => {
                    if (data.d && data.d.ImageBase64) {
                        // Cache the image with quota management
                        try {
                            // Handle storage quota by implementing LRU (Least Recently Used) cache
                            manageImageCache(productId, data.d.ImageBase64);
                        } catch (error) {
                            console.warn("Cache storage error:", error);
                            debugLog("Image caching failed", error.message);
                            // We'll still display the image even if caching fails
                        }

                        // Determine if imgElement is provided
                        const elementsToUpdate = imgElement ?
                            [imgElement] :
                            Array.from(document.querySelectorAll(`img[data-product-id="${productId}"]`));

                        // Update all relevant images
                        elementsToUpdate.forEach(img => {
                            // Try as JPEG first
                            img.src = `data:image/jpeg;base64,${data.d.ImageBase64}`;

                            // Setup fallback chain
                            img.onerror = function () {
                                // If JPEG fails, try as PNG
                                img.src = `data:image/png;base64,${data.d.ImageBase64}`;
                                img.onerror = function () {
                                    // If PNG fails too, try as a generic image
                                    img.src = `data:image;base64,${data.d.ImageBase64}`;
                                    img.onerror = function () {
                                        // If all attempts fail, show placeholder
                                        img.src = "data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTAwIiBoZWlnaHQ9IjEwMCIgdmlld0JveD0iMCAwIDI0IDI0IiBmaWxsPSJub25lIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxwYXRoIGQ9Ik0zIDh2MTFjMCAxLjEuOSAyIDIgMmgxNGMxLjEgMCAyLS45IDItMlY4TDMgOHpoLS41Yy0uMiAwLS4zLS4xLS4zLS4zVjRjMC0xLjEuOS0yIDItMmg0YzAgMCAxIDAgMSAuNS0uNC4zLS42LjgtLjYgMS4zVjZjMCAwIDAgLjUuNC44aDEuNGMuNCAwIC43LjQuOC45bC41IDEuM2g2LjN6IiBmaWxsPSIjZTllOWU5Ii8+PHBhdGggZD0iTTExIDE0bC03LTQgMTQgMC03IDR6IiBmaWxsPSIjZTllOWU5Ii8+PC9zdmc+";
                                        console.error(`Failed to render image for product ${productId}`);
                                    };
                                };
                            };
                        });
                    } else if (data.d && data.d.Error) {
                        console.error(`Error retrieving image for product ${productId}: ${data.d.Error}`);
                        debugLog(`Image load error for product ${productId}`, data.d.Error, true);
                    }
                })
                .catch(error => {
                    console.error(`Error loading image for product ${productId}:`, error);
                    debugLog(`Failed to load image for product ${productId}`, error.message, true);

                    // For user feedback, update the image placeholder
                    if (imgElement) {
                        imgElement.parentElement.innerHTML = `
                        <span class="text-gray-500 text-xs">Error loading image</span>
                    `;
                    }
                });
        }

        // Function to manage image cache using LRU strategy
        function manageImageCache(productId, imageBase64) {
            // Check if we need to remove items from cache before adding new one
            if (imageLoadPriority.length >= IMAGE_CACHE_MAX_SIZE) {
                // Remove least recently used image from cache
                while (imageLoadPriority.length >= IMAGE_CACHE_MAX_SIZE) {
                    try {
                        const oldestImageId = imageLoadPriority.shift(); // Remove oldest item
                        sessionStorage.removeItem(`product_image_${oldestImageId}`);
                        debugLog(`Removed image ${oldestImageId} from cache to free space`);
                    } catch (e) {
                        console.warn("Error removing item from cache:", e);
                        break; // Prevent infinite loop if removal fails
                    }
                }
            }

            try {
                // Now try to add the new image to cache
                sessionStorage.setItem(`product_image_${productId}`, imageBase64);
                // Add to image priority list (most recently used)
                updateImageCachePriority(productId);
            } catch (e) {
                // Still failed after clearing - maybe the image is too large for even an empty cache
                // or localStorage is disabled
                console.warn(`Image for product ${productId} too large to cache:`, e);
                throw e; // Re-throw for caller to handle
            }
        }

        // Update image cache priority (for LRU implementation)
        function updateImageCachePriority(productId) {
            // Remove if already exists in the array
            const index = imageLoadPriority.indexOf(productId);
            if (index > -1) {
                imageLoadPriority.splice(index, 1);
            }

            // Add to the end (most recently used)
            imageLoadPriority.push(productId);
        }

        function getStockStatusBadge(quantity) {
            if (quantity <= 0) {
                return '<span class="px-2 py-1 text-xs font-semibold rounded-full bg-red-100 text-red-800">Out of Stock</span>';
            } else if (quantity <= 10) {
                return '<span class="px-2 py-1 text-xs font-semibold rounded-full bg-yellow-100 text-yellow-800">Low Stock</span>';
            } else {
                return '<span class="px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800">In Stock</span>';
            }
        }

        function debounceSearch() {
            clearTimeout(debounceTimer);

            const searchInput = document.getElementById('searchInput');
            const searchValue = searchInput ? searchInput.value.trim() : '';
            const searchIcon = document.querySelector('#searchInput + span');
            const liveSearchIndicator = document.getElementById('liveSearchIndicator');
            const filterMessage = document.getElementById('filterMessage');

            // Only show filtering UI if there's actually something in the search box
            if (searchValue.length > 0) {
                // Show the live search indicator with custom message
                if (liveSearchIndicator && filterMessage) {
                    liveSearchIndicator.classList.remove('hidden');
                    filterMessage.textContent = `Filtering products for "${searchValue}"...`;
                }

                // Add a visual indication that search is in progress
                if (searchInput) {
                    searchInput.classList.add('bg-blue-50', 'border-blue-300');
                }

                if (searchIcon) {
                    searchIcon.innerHTML = `
                        <svg class="w-5 h-5 animate-spin text-blue-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                        </svg>
                    `;
                }
            } else {
                // Hide the indicator if search is empty
                if (liveSearchIndicator) {
                    liveSearchIndicator.classList.add('hidden');
                }
            }

            debounceTimer = setTimeout(() => {
                currentPage = 1;
                loadProducts().then(() => {
                    // Restore the search input and icon after loading
                    if (searchInput) {
                        searchInput.classList.remove('bg-blue-50', 'border-blue-300');
                    }

                    if (searchIcon) {
                        searchIcon.innerHTML = `
                            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path>
                            </svg>
                        `;
                    }

                    // Keep the filter indicator visible if there's a search term
                    if (searchValue.length === 0 && liveSearchIndicator) {
                        liveSearchIndicator.classList.add('hidden');
                    } else if (liveSearchIndicator && filterMessage) {
                        // Update message to show the search is complete
                        filterMessage.textContent = `Showing results for "${searchValue}"`;
                    }
                }).catch(() => {
                    // Restore styling on error
                    if (searchInput) {
                        searchInput.classList.remove('bg-blue-50', 'border-blue-300');
                    }

                    if (searchIcon) {
                        searchIcon.innerHTML = `
                            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path>
                            </svg>
                        `;
                    }

                    // Hide the filter indicator on error
                    if (liveSearchIndicator) {
                        liveSearchIndicator.classList.add('hidden');
                    }
                });
            }, 300);
        }

        function openAddModal() {
            document.getElementById('modalTitle').textContent = 'Add Product';
            document.getElementById('productModal').classList.remove('hidden');
            document.getElementById('imagePreview').classList.add('hidden');
            hideModalError();
            resetForm();
            // Show required field indicators for Add mode
            document.querySelectorAll('.required-indicator').forEach(el => {
                el.style.display = 'inline';
            });
        }

        function openEditModal(productId) {
            // Prevent default behavior that might cause page refresh
            if (event) {
                event.preventDefault();
                event.stopPropagation();
            }

            selectedProductId = productId;
            document.getElementById('modalTitle').textContent = 'Edit Product';
            hideModalError();

            // Hide required field indicators for Edit mode
            document.querySelectorAll('.required-indicator').forEach(el => {
                el.style.display = 'none';
            });

            fetch('Products.aspx/GetProductDetails', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ productId: productId })
            })
                .then(response => response.json())
                .then(data => {
                    if (!data.d) {
                        throw new Error('Invalid response from server');
                    }

                    const product = data.d;
                    populateForm(product);
                    document.getElementById('productModal').classList.remove('hidden');
                })
                .catch(error => {
                    console.error('Error:', error);
                    showToast('Error loading product details', 'error');
                });

            return false; // Prevent default
        }

        function openDeleteModal(productId) {
            // Prevent default behavior that might cause page refresh
            event.preventDefault();
            event.stopPropagation();

            selectedProductId = productId;
            document.getElementById('deleteModal').classList.remove('hidden');

            return false; // Prevent default
        }

        function resetForm() {
            document.getElementById('productName').value = '';
            document.getElementById('productDescription').value = '';
            document.getElementById('productPrice').value = '';
            document.getElementById('productCostPrice').value = '';
            document.getElementById('productStock').value = '';
            document.getElementById('productCategory').value = '';
            document.getElementById('isLatest').checked = false;
            document.getElementById('productImage').value = '';

            // Reset image and overlay
            const imagePreview = document.getElementById('imagePreview');
            const imageOverlay = document.getElementById('imageOverlay');
            imagePreview.src = '';
            imagePreview.classList.add('hidden');
            imageOverlay.classList.add('hidden');

            // Clear any hover events
            const imageContainer = imagePreview.parentElement;
            imageContainer.onmouseenter = null;
            imageContainer.onmouseleave = null;

            selectedProductId = null;
        }

        function populateForm(product) {
            document.getElementById('productName').value = product.Name || '';
            document.getElementById('productDescription').value = product.Description || '';
            document.getElementById('productPrice').value = product.Price || 0;
            document.getElementById('productCostPrice').value = product.CostPrice || 0;
            document.getElementById('productStock').value = product.StockQuantity || 0;
            document.getElementById('productCategory').value = product.CategoryId || ''; // Make sure to convert 0 to empty string if needed
            document.getElementById('isLatest').checked = product.IsLatest || false;

            const imagePreview = document.getElementById('imagePreview');
            const imageOverlay = document.getElementById('imageOverlay');

            if (product.ImageBase64) {
                // Try different image formats
                imagePreview.onerror = function () {
                    // If JPEG fails, try as PNG
                    imagePreview.src = 'data:image/png;base64,' + product.ImageBase64;
                    imagePreview.onerror = function () {
                        // If PNG fails too, try as a generic image
                        imagePreview.src = 'data:image;base64,' + product.ImageBase64;
                    };
                };

                imagePreview.src = 'data:image/jpeg;base64,' + product.ImageBase64;
                imagePreview.classList.remove('hidden');

                // Set up hover behavior for the image
                const imageContainer = imagePreview.parentElement;
                imageContainer.onmouseenter = function () {
                    imageOverlay.classList.remove('hidden');
                };
                imageContainer.onmouseleave = function () {
                    imageOverlay.classList.add('hidden');
                };
            } else {
                imagePreview.classList.add('hidden');
                imageOverlay.classList.add('hidden');
            }

            // Log the form values for debugging
            console.log('Form populated with:', {
                name: document.getElementById('productName').value,
                description: document.getElementById('productDescription').value,
                price: document.getElementById('productPrice').value,
                costPrice: document.getElementById('productCostPrice').value,
                stock: document.getElementById('productStock').value,
                category: document.getElementById('productCategory').value,
                isLatest: document.getElementById('isLatest').checked,
                hasImage: product.ImageBase64 ? true : false
            });
        }

        function validateForm(data) {
            // Hide any previous error message
            hideModalError();

            if (!data.name || data.name.length < 3) {
                showModalError('Product name must be at least 3 characters');
                return false;
            }
            if (isNaN(data.price) || data.price <= 0 || data.price > 10000) {
                showModalError('Price must be between 0 and 10,000');
                return false;
            }
            if (isNaN(data.stockQuantity) || data.stockQuantity < 0 || data.stockQuantity > 1000) {
                showModalError('Stock quantity must be between 0 and 1,000');
                return false;
            }
            if (!data.categoryId && data.categoryId !== 0) {
                showModalError('Please select a category');
                return false;
            }

            // Check description word count
            if (data.description && data.description.trim()) {
                const wordCount = data.description.trim().split(/\s+/).length;
                if (wordCount > 50) {
                    showModalError(`Description cannot exceed 50 words (current: ${wordCount} words)`);
                    return false;
                }
            }

            return true;
        }

        function showModalError(message) {
            const modalError = document.getElementById('modalError');
            const modalErrorMessage = document.getElementById('modalErrorMessage');

            modalErrorMessage.textContent = message;
            modalError.classList.remove('hidden');

            // Scroll to the top of the modal to ensure error is visible
            const modalContent = document.querySelector('#productModal .overflow-y-auto');
            if (modalContent) {
                modalContent.scrollTop = 0;
            }
        }

        function hideModalError() {
            document.getElementById('modalError').classList.add('hidden');
            document.getElementById('modalErrorMessage').textContent = '';
        }

        function saveProduct() {
            // Get values from form
            const name = document.getElementById('productName').value;
            const description = document.getElementById('productDescription').value;
            const price = document.getElementById('productPrice').value; // Keep as string for new method
            const costPrice = document.getElementById('productCostPrice').value; // Keep as string for new method
            const stockQuantity = document.getElementById('productStock').value; // Keep as string for new method
            const categoryId = document.getElementById('productCategory').value; // Keep as string for new method
            const isLatest = document.getElementById('isLatest').checked;

            // Create form data object with all required fields
            const formData = {
                name,
                description,
                price,  // Keep as string for simple method
                costPrice,  // Keep as string for simple method
                stockQuantity, // Keep as string for simple method
                categoryId, // Keep as string for simple method
                isLatest,
                imageBase64: null // Always include imageBase64, even if null
            };

            // Add productId for update operations
            if (selectedProductId) {
                formData.productId = selectedProductId;

                // For update operations, we need numeric values
                formData.price = parseFloat(price);
                formData.costPrice = parseFloat(costPrice);
                formData.stockQuantity = parseInt(stockQuantity);
                formData.categoryId = categoryId ? parseInt(categoryId) : 0;
            }

            // Extract image data
            const imageSrc = document.getElementById('imagePreview').src;
            if (imageSrc && imageSrc.indexOf('base64,') > -1) {
                // For new products or if the image input has a file selected (meaning it was changed)
                const imageInput = document.getElementById('productImage');
                const imageWasChanged = imageInput.files && imageInput.files.length > 0;

                if (!selectedProductId || imageWasChanged) {
                    // New product or image was changed - include the full image data
                    formData.imageBase64 = imageSrc;
                    debugLog('Including full image data', { reason: selectedProductId ? 'Image changed' : 'New product' });
                } else if (selectedProductId) {
                    // Editing existing product with image but not changing it
                    // Just send a placeholder to keep the existing image
                    formData.imageBase64 = "keep-existing";
                    debugLog('Including placeholder for existing image', { reason: 'Editing with existing image' });
                }
            }

            // Log data being sent (without the full image for readability)
            const logData = { ...formData };
            if (logData.imageBase64 && logData.imageBase64.length > 20) {
                logData.imageBase64 = '[Image data present]';
            }
            debugLog('Saving product with data:', logData);

            // Enhanced validation
            // Check if product name is empty
            if (!name || name.trim().length < 3) {
                showModalError('Product name must be at least 3 characters');
                return;
            }

            // Check if product name contains at least one letter
            if (!/[a-zA-Z]/.test(name)) {
                showModalError('Product name must contain at least one letter');
                return;
            }

            // Check if product name contains any numbers
            if (/[0-9]/.test(name)) {
                showModalError('Product name cannot contain any numbers');
                return;
            }

            // Check if price is valid
            if (!price || isNaN(parseFloat(price)) || parseFloat(price) <= 0) {
                showModalError('Please enter a valid price greater than zero');
                return;
            }

            // Check if cost price is valid
            if (!costPrice || isNaN(parseFloat(costPrice)) || parseFloat(costPrice) <= 0) {
                showModalError('Please enter a valid cost price greater than zero');
                return;
            }

            // Check if cost price is less than selling price
            if (parseFloat(costPrice) >= parseFloat(price)) {
                showModalError('Cost price must be less than selling price');
                return;
            }

            // Check if stock quantity is valid
            if (!stockQuantity || isNaN(parseInt(stockQuantity)) || parseInt(stockQuantity) < 0) {
                showModalError('Please enter a valid stock quantity');
                return;
            }

            // Check if category is selected
            if (!categoryId || categoryId === '') {
                showModalError('Please select a category for the product');
                return;
            }

            // Show loading state
            const saveButton = document.querySelector('button[onclick="saveProduct()"]');
            const originalButtonText = saveButton.textContent;
            saveButton.textContent = 'Saving...';
            saveButton.disabled = true;

            // Determine which method to call based on whether we're adding or updating
            const url = selectedProductId ?
                'Products.aspx/UpdateProduct' :  // Use existing method for updates
                'Products.aspx/AddProductSimple'; // Use new simple method for adds

            debugLog(`Using endpoint: ${url}`, { isUpdate: !!selectedProductId });

            // Send request to server
            fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(formData)
            })
                .then(response => {
                    debugLog('Server response status:', response.status);
                    if (!response.ok) {
                        return response.text().then(text => {
                            debugLog('Error response body:', text, true);
                            throw new Error('Server returned status ' + response.status);
                        });
                    }
                    return response.json();
                })
                .then(data => {
                    debugLog('Save product response:', data);

                    // Reset button state
                    saveButton.textContent = originalButtonText;
                    saveButton.disabled = false;

                    if (data.d && data.d.status === 'success') {
                        showToast(data.d.message, 'success');
                        closeModal();
                        loadProducts();
                    } else {
                        const errorMsg = data.d && data.d.message ? data.d.message : 'Unknown error occurred';
                        showModalError(errorMsg);
                        debugLog('Error saving product:', data, true);
                    }
                })
                .catch(error => {
                    // Reset button state
                    saveButton.textContent = originalButtonText;
                    saveButton.disabled = false;

                    console.error('Error:', error);
                    debugLog('Network/parsing error:', error.message, true);
                    showModalError('Error saving product: ' + error.message);
                });
        }

        function confirmDelete(productId, productName) {
            // If event exists, prevent default behavior
            if (window.event) {
                window.event.preventDefault();
                window.event.stopPropagation();
            }

            // Set the product ID to the global variable
            selectedProductId = productId;

            // Update the modal text if we have a product name
            if (productName) {
                const modalText = document.querySelector('#deleteModal p.text-gray-500');
                if (modalText) {
                    modalText.textContent = `Are you sure you want to permanently delete "${productName}"?`;
                }
            }

            // Show the delete modal
            document.getElementById('deleteModal').classList.remove('hidden');

            return false; // Prevent default
        }

        function deleteProduct() {
            if (!selectedProductId) return;

            const deleteButton = document.querySelector('#deleteModal button[onclick="deleteProduct()"]');
            const originalText = deleteButton.textContent;
            deleteButton.textContent = 'Deleting...';
            deleteButton.disabled = true;

            fetch('Products.aspx/DeleteProduct', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ productId: selectedProductId })
            })
                .then(response => response.json())
                .then(data => {
                    deleteButton.textContent = originalText;
                    deleteButton.disabled = false;

                    if (data.d && data.d.status === 'success') {
                        showToast(data.d.message, 'success');
                        closeDeleteModal();
                        loadProducts(); // Refresh the product list
                    } else {
                        showToast(data.d.message || 'Error deleting product', 'error');
                        closeDeleteModal();
                    }
                })
                .catch(error => {
                    deleteButton.textContent = originalText;
                    deleteButton.disabled = false;

                    console.error('Error:', error);
                    showToast('Error deleting product', 'error');
                    closeDeleteModal();
                });
        }

        function showToast(message, type = 'success') {
            const toast = document.getElementById('toast');
            const toastMessage = document.getElementById('toastMessage');

            toast.className = `fixed bottom-4 right-4 px-6 py-4 rounded-lg shadow-lg ${type === 'success' ? 'bg-green-500' : 'bg-red-500'
                }`;

            toastMessage.textContent = message;
            toast.classList.remove('hidden');

            setTimeout(() => {
                toast.classList.add('hidden');
            }, 3000);
        }

        function loadDashboardStats() {
            fetch('Products.aspx/GetDashboardStats', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            })
                .then(response => response.json())
                .then(data => {
                    if (data.d) {
                        document.getElementById('totalProducts').textContent = data.d.TotalProducts || 0;
                        document.getElementById('lowStockCount').textContent = data.d.LowStockCount || 0;
                        document.getElementById('outOfStockCount').textContent = data.d.OutOfStockCount || 0;
                    }
                })
                .catch(error => {
                    console.error('Error loading dashboard stats:', error);
                });
        }

        function handleImageUpload(event) {
            const file = event.target.files[0];
            if (!file) return;

            // Validate file type
            const allowedTypes = ['image/jpeg', 'image/png', 'image/jpg'];
            if (!allowedTypes.includes(file.type)) {
                showModalError('Only JPEG, JPG, and PNG images are allowed');
                // Clear the file input
                event.target.value = '';
                return;
            }

            // Validate file size
            if (file.size > 5 * 1024 * 1024) {
                showModalError('Image size should not exceed 5MB');
                // Clear the file input
                event.target.value = '';
                return;
            }

            const reader = new FileReader();
            reader.onload = function (e) {
                const imagePreview = document.getElementById('imagePreview');
                const imageOverlay = document.getElementById('imageOverlay');

                imagePreview.src = e.target.result;
                imagePreview.classList.remove('hidden');

                // Setup hover behavior
                const imageContainer = imagePreview.parentElement;
                imageContainer.onmouseenter = function () {
                    imageOverlay.classList.remove('hidden');
                };
                imageContainer.onmouseleave = function () {
                    imageOverlay.classList.add('hidden');
                };

                // Hide any error messages since image upload succeeded
                hideModalError();
            };
            reader.readAsDataURL(file);
        }

        function handleDragOver(e) {
            e.preventDefault();
            e.stopPropagation();
        }

        function handleDrop(e) {
            e.preventDefault();
            e.stopPropagation();

            const file = e.dataTransfer.files[0];
            if (!file) return;

            // Validate file type
            const allowedTypes = ['image/jpeg', 'image/png', 'image/jpg'];
            if (!allowedTypes.includes(file.type)) {
                showModalError('Only JPEG, JPG, and PNG images are allowed');
                return;
            }

            // Validate file size
            if (file.size > 5 * 1024 * 1024) {
                showModalError('Image size should not exceed 5MB');
                return;
            }

            // Set the file in the file input element
            const fileInput = document.getElementById('productImage');

            // Create a new DataTransfer object and add our file
            const dataTransfer = new DataTransfer();
            dataTransfer.items.add(file);
            fileInput.files = dataTransfer.files;

            // Trigger the handleImageUpload function
            handleImageUpload({ target: fileInput });
        }

        function closeModal() {
            document.getElementById('productModal').classList.add('hidden');
            hideModalError();
            resetForm();
        }

        function closeDeleteModal() {
            document.getElementById('deleteModal').classList.add('hidden');
            selectedProductId = null;
        }

        function updatePagination(totalCount) {
            const totalPages = Math.ceil(totalCount / pageSize);
            const paginationContainer = document.getElementById('pagination');

            // Update page display
            document.getElementById('currentPageDisplay').textContent = currentPage;
            document.getElementById('totalPagesDisplay').textContent = totalPages;

            // Clear existing pagination
            paginationContainer.innerHTML = '';

            // First page button (like in Users page)
            const firstPageButton = createPaginationButton('&laquo;', currentPage > 1, false, 'rounded-l-md');
            firstPageButton.onclick = () => {
                if (currentPage > 1) {
                    currentPage = 1;
                    loadProducts();
                }
            };
            paginationContainer.appendChild(firstPageButton);

            // Previous button
            const prevButton = createPaginationButton('', currentPage > 1);
            prevButton.innerHTML = '<svg class="h-5 w-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true"><path fill-rule="evenodd" d="M12.707 5.293a1 1 0 010 1.414L9.414 10l3.293 3.293a1 1 0 01-1.414 1.414l-4-4a1 1 0 010-1.414l4-4a1 1 0 011.414 0z" clip-rule="evenodd" /></svg>';
            prevButton.onclick = () => {
                if (currentPage > 1) {
                    currentPage--;
                    loadProducts();
                }
            };
            paginationContainer.appendChild(prevButton);

            // Page numbers
            let startPage = Math.max(1, currentPage - 2);
            let endPage = Math.min(totalPages, startPage + 4);

            if (endPage - startPage < 4) {
                startPage = Math.max(1, endPage - 4);
            }

            for (let i = startPage; i <= endPage; i++) {
                const pageButton = createPaginationButton(i.toString(), true, i === currentPage);
                pageButton.onclick = () => {
                    currentPage = i;
                    loadProducts();
                };
                paginationContainer.appendChild(pageButton);
            }

            // Next button
            const nextButton = createPaginationButton('', currentPage < totalPages);
            nextButton.innerHTML = '<svg class="h-5 w-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true"><path fill-rule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clip-rule="evenodd" /></svg>';
            nextButton.onclick = () => {
                if (currentPage < totalPages) {
                    currentPage++;
                    loadProducts();
                }
            };
            paginationContainer.appendChild(nextButton);

            // Last page button (like in Users page)
            const lastPageButton = createPaginationButton('&raquo;', currentPage < totalPages, false, 'rounded-r-md');
            lastPageButton.onclick = () => {
                if (currentPage < totalPages) {
                    currentPage = totalPages;
                    loadProducts();
                }
            };
            paginationContainer.appendChild(lastPageButton);

            // Update mobile pagination buttons
            const prevPageMobile = document.getElementById('prevPageMobile');
            const nextPageMobile = document.getElementById('nextPageMobile');

            prevPageMobile.disabled = currentPage <= 1;
            nextPageMobile.disabled = currentPage >= totalPages;

            prevPageMobile.onclick = () => {
                if (currentPage > 1) {
                    currentPage--;
                    loadProducts();
                }
            };

            nextPageMobile.onclick = () => {
                if (currentPage < totalPages) {
                    currentPage++;
                    loadProducts();
                }
            };
        }

        function createPaginationButton(text, enabled, isCurrentPage = false, extraClasses = '') {
            const button = document.createElement('button');
            button.type = 'button';

            let className = 'relative inline-flex items-center px-4 py-2 border ';

            if (isCurrentPage) {
                className += 'border-gray-300 bg-indigo-50 text-sm font-medium text-indigo-600 hover:bg-gray-50';
            } else if (enabled) {
                if (text === '&laquo;' || text === '&raquo;') {
                    className += 'border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 px-2 py-2';
                } else {
                    className += 'border-gray-300 bg-white text-sm font-medium text-gray-700 hover:bg-gray-50';
                }
            } else {
                className += 'border-gray-300 bg-gray-100 text-gray-400 cursor-not-allowed';
            }

            if (extraClasses) {
                className += ' ' + extraClasses;
            }

            button.className = className;
            button.disabled = !enabled;
            button.innerHTML = text; // Using innerHTML for &laquo; and &raquo;

            return button;
        }

        // Debug function - enhanced to log but not show by default
        function debugLog(message, data, isError = false) {
            // Always log to console for developers
            console.log(message, data);

            // Add to the debug output div regardless of visibility
            const debugOutput = document.getElementById('debugOutput');
            const debugInfo = document.getElementById('debugInfo');

            // Only show debug info panel if:
            // 1. It's an error, or
            // 2. Developer mode is on, or
            // 3. It's already visible
            if ((isError || devMode) && debugInfo.classList.contains('hidden')) {
                debugInfo.classList.remove('hidden');
            }

            let formattedData = '';
            try {
                formattedData = data ? JSON.stringify(data, null, 2) : '';
            } catch (error) {
                formattedData = '[Error stringifying data: ' + error.message + ']';
            }

            const timestamp = new Date().toLocaleTimeString();
            const logEntry = document.createElement('div');
            logEntry.className = isError ? 'text-red-600' : '';
            logEntry.innerHTML = `[${timestamp}] ${message}\n${formattedData}\n\n`;

            // Insert at the top for newest logs first
            if (debugOutput.firstChild) {
                debugOutput.insertBefore(logEntry, debugOutput.firstChild);
            } else {
                debugOutput.appendChild(logEntry);
            }

            // Limit the number of log entries to avoid memory issues
            while (debugOutput.children.length > 100) {
                debugOutput.removeChild(debugOutput.lastChild);
            }
        }

        function showDebugInfo() {
            document.getElementById('debugInfo').classList.remove('hidden');
        }

        function hideDebugInfo() {
            document.getElementById('debugInfo').classList.add('hidden');
        }

        // Toggle development mode
        function toggleDevMode() {
            devMode = !devMode;
            const devModeToggle = document.getElementById('devModeToggle');

            if (devMode) {
                devModeToggle.textContent = 'Dev: ON';
                devModeToggle.classList.add('bg-green-100');
                devModeToggle.classList.remove('bg-gray-100');
                showDebugInfo();
            } else {
                devModeToggle.textContent = 'Dev: OFF';
                devModeToggle.classList.add('bg-gray-100');
                devModeToggle.classList.remove('bg-green-100');
                hideDebugInfo();
            }
        }

        // Add this function to check for sort preferences
        function checkForSortPreference() {
            // First check URL parameters
            const urlParams = new URLSearchParams(window.location.search);
            const sortOption = urlParams.get('sortOption');

            // Then check localStorage (this takes precedence if both exist)
            const storedSort = localStorage.getItem('productSort');

            console.log("Checking sort preferences - URL:", sortOption, "localStorage:", storedSort);

            if (storedSort) {
                // Set the dropdown to the stored value
                const sortDropdown = document.getElementById('sortOptions');
                if (sortDropdown) {
                    for (let i = 0; i < sortDropdown.options.length; i++) {
                        if (sortDropdown.options[i].value === storedSort) {
                            sortDropdown.selectedIndex = i;
                            debugLog('Applied sort preference from localStorage:', storedSort);
                            break;
                        }
                    }
                }

                // Clear the localStorage value after we've used it
                localStorage.removeItem('productSort');
            } else if (sortOption) {
                // Set the dropdown to the URL parameter value
                const sortDropdown = document.getElementById('sortOptions');
                if (sortDropdown) {
                    for (let i = 0; i < sortDropdown.options.length; i++) {
                        if (sortDropdown.options[i].value === sortOption) {
                            sortDropdown.selectedIndex = i;
                            debugLog('Applied sort preference from URL:', sortOption);
                            break;
                        }
                    }
                }
            }
        }
    </script>
</asp:Content>
