<%@ Page Title="Products" 
         Language="C#" 
         MasterPageFile="~/Pages/AdminMaster.master" 
         AutoEventWireup="true" 
         CodeBehind="Products.aspx.cs" 
         Inherits="OnlinePastryShop.Pages.Products" %>
<asp:Content ID="Content1" ContentPlaceHolderID="AdminContent" runat="server">
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
                <div id="searchWrapper" class="w-full">
                    <input type="text" id="searchInput" placeholder="Search products..." 
                        class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-[#D43B6A] focus:border-transparent" 
                        onkeydown="handleSearchKeyDown(event)" />
                    <span class="absolute right-3 top-2.5 text-gray-400">
                        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path>
                        </svg>
                    </span>
                </div>
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
    
    <!-- Toast Notification -->
    <div id="toast" class="fixed bottom-4 right-4 px-6 py-3 rounded-lg text-white z-50 shadow-lg hidden">
        <div class="flex items-center">
            <span id="toastMessage">Operation completed successfully</span>
            </div>
        </div>
        
        <!-- Delete Confirmation Modal -->
        <div id="deleteModal" class="fixed inset-0 bg-black bg-opacity-50 hidden flex items-center justify-center z-50">
            <div class="bg-white rounded-lg p-6 w-full max-w-md">
            <div class="text-center">
                <svg class="mx-auto mb-4 text-red-500 w-14 h-14" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"></path>
                </svg>
                <h3 class="mb-5 text-lg font-bold text-gray-800">Delete Product</h3>
                <p class="mb-5 text-gray-600">Are you sure you want to delete this product? This action cannot be undone.</p>
                <div class="flex justify-center gap-4">
                    <button type="button" onclick="closeDeleteModal()" class="px-4 py-2 bg-gray-200 text-gray-800 rounded-lg hover:bg-gray-300">
                        Cancel
                    </button>
                    <button type="button" onclick="deleteProduct()" class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700">
                        Delete
                    </button>
                </div>
            </div>
            </div>
        </div>
        
        <!-- Developer Debug Tools (hidden in footer) -->
        <div class="fixed bottom-0 left-0 p-2 opacity-30 hover:opacity-100 transition-opacity z-40">
            <button type="button" id="devModeToggle" class="text-xs bg-gray-100 p-1 rounded mr-1" onclick="toggleDevMode()">Dev: OFF</button>
            <button type="button" class="text-xs bg-gray-100 p-1 rounded" onclick="showDebugInfo()">Show Debug</button>
        </div>
    
    <!-- Add the missing JavaScript code -->
    <script type="text/javascript">
        // Global variables
        let currentPage = 1;
        let totalPages = 1;
        let currentProductId = 0;
        let isEditing = false;
        let productImage = null;
        let devMode = false;
        
        // DOM Content Loaded Event
        document.addEventListener('DOMContentLoaded', function() {
            // Initialize the page
            initPage();
            
            // Add event listeners - only using input for live search with debounce
            document.getElementById('searchInput').addEventListener('input', debounce(searchProducts, 500));
            
            // Remove the keypress listener that might be conflicting
            // The direct onkeydown attribute will handle Enter key
            
            document.getElementById('categoryFilter').addEventListener('change', searchProducts);
            document.getElementById('sortOptions').addEventListener('change', searchProducts);
            document.getElementById('productImage').addEventListener('change', handleImageUpload);
            
            // Mobile pagination buttons
            document.getElementById('prevPageMobile').addEventListener('click', () => navigateToPage(currentPage - 1));
            document.getElementById('nextPageMobile').addEventListener('click', () => navigateToPage(currentPage + 1));
        });
        
        // Initialize the page
        function initPage() {
            // Load dashboard stats
            loadDashboardStats();
            
            // Load categories for filter and product form
            loadCategories();
            
            // Load products
            loadProducts();
        }
        
        // Load dashboard statistics
        function loadDashboardStats() {
            PageMethods.GetDashboardStats(
                function(result) {
                    if (result) {
                        document.getElementById('totalProducts').textContent = result.TotalProducts;
                        document.getElementById('lowStockCount').textContent = result.LowStockCount;
                        document.getElementById('outOfStockCount').textContent = result.OutOfStockCount;
                    }
                },
                function(error) {
                    logDebug('Error loading dashboard stats: ' + JSON.stringify(error));
                    showToast('Failed to load dashboard statistics', 'error');
                }
            );
        }
        
        // Load product categories
        function loadCategories() {
            PageMethods.GetCategories(
                function(result) {
                    if (result && result.length) {
                        // Populate category filter dropdown
                        const categoryFilter = document.getElementById('categoryFilter');
                        categoryFilter.innerHTML = '<option value="">All Categories</option>';
                        
                        // Populate product form category dropdown
                        const productCategory = document.getElementById('productCategory');
                        productCategory.innerHTML = '';
                        
                        result.forEach(function(category) {
                            // Add to filter dropdown
                            const filterOption = document.createElement('option');
                            filterOption.value = category.CategoryId;
                            filterOption.textContent = category.Name;
                            categoryFilter.appendChild(filterOption);
                            
                            // Add to product form dropdown
                            const formOption = document.createElement('option');
                            formOption.value = category.CategoryId;
                            formOption.textContent = category.Name;
                            productCategory.appendChild(formOption);
                        });
                    }
                },
                function(error) {
                    logDebug('Error loading categories: ' + JSON.stringify(error));
                    showToast('Failed to load categories', 'error');
                }
            );
        }
        
        // Load products with pagination and filtering
        function loadProducts() {
            // Show loading state
            document.getElementById('productsList').innerHTML = '<tr><td colspan="8" class="px-6 py-4 text-center">Loading products...</td></tr>';
            
            // Get filter values
            const search = document.getElementById('searchInput').value.trim();
            const categoryId = document.getElementById('categoryFilter').value;
            const sort = document.getElementById('sortOptions').value;
            const includeDeleted = false; // Default is to hide deleted products
            
            // If not empty search, show live search indicator
            if (search) {
                document.getElementById('liveSearchIndicator').classList.remove('hidden');
                document.getElementById('filterMessage').textContent = `Showing results for "${search}"`;
            } else {
                document.getElementById('liveSearchIndicator').classList.add('hidden');
            }
            
            // Call the web method to get the products
            PageMethods.GetProducts(search, categoryId, sort, currentPage, 10, includeDeleted,
                function(result) {
                    if (result) {
                        // Update pagination
                        totalPages = Math.ceil(result.TotalCount / 10);
                        updatePagination();
                        
                        // Populate products table
                        renderProducts(result.Products);
                    }
                },
                function(error) {
                    logDebug('Error loading products: ' + JSON.stringify(error));
                    document.getElementById('productsList').innerHTML = '<tr><td colspan="8" class="px-6 py-4 text-center text-red-500">Failed to load products. Please try again.</td></tr>';
                    showToast('Failed to load products', 'error');
                }
            );
        }
        
        // Render products in the table
        function renderProducts(products) {
            const tbody = document.getElementById('productsList');

            if (!products || products.length === 0) {
                tbody.innerHTML = `
                    <tr>
                        <td colspan="8" class="px-6 py-4 text-center">
                            <div class="space-y-3">
                                <p>No products found. Try a different search or filter.</p>
                                <p class="text-sm text-gray-500">If you believe this is an error, you can 
                                    <button type="button" onclick="testDatabaseConnection()" 
                                        class="text-pink-600 hover:text-pink-800 font-medium underline">
                                        check database connection
                                    </button>
                                </p>
                            </div>
                        </td>
                    </tr>`;
                return;
            }

            let html = '';
            
            products.forEach(function(product) {
                // Calculate profit
                const profit = (product.Price - product.CostPrice).toFixed(2);
                const profitMargin = (product.Price > 0) ? ((profit / product.Price) * 100).toFixed(1) : 0;
                
                // Determine stock status class and text
                let stockStatusClass = '';
                let stockStatusText = '';
                
                if (product.StockQuantity === 0) {
                    stockStatusClass = 'bg-red-100 text-red-800';
                    stockStatusText = 'Out of Stock';
                } else if (product.StockQuantity < 10) {
                    stockStatusClass = 'bg-yellow-100 text-yellow-800';
                    stockStatusText = 'Low Stock';
                } else {
                    stockStatusClass = 'bg-green-100 text-green-800';
                    stockStatusText = 'In Stock';
                }
                
                html += `
                <tr>
                        <td class="px-6 py-4 whitespace-nowrap">
                            <div class="flex items-center">
                            <div class="flex-shrink-0 h-10 w-10 rounded-md bg-gray-200 overflow-hidden">
                                ${product.ImageUrl ? `<img src="${product.ImageUrl}" alt="${product.Name}" class="h-10 w-10 object-cover">` : ''}
                            </div>
                            <div class="ml-4">
                                    <div class="text-sm font-medium text-gray-900">${product.Name}</div>
                                ${product.IsLatest ? '<span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-pink-100 text-pink-800">Latest</span>' : ''}
                                </div>
                            </div>
                        </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${product.CategoryName || 'Uncategorized'}</td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">₱${product.CostPrice.toFixed(2)}</td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 font-medium">₱${product.Price.toFixed(2)}</td>
                    <td class="px-6 py-4 whitespace-nowrap">
                        <div class="text-sm text-gray-900">₱${profit}</div>
                        <div class="text-xs text-gray-500">(${profitMargin}%)</div>
                        </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${product.StockQuantity}</td>
                    <td class="px-6 py-4 whitespace-nowrap">
                        <span class="px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full ${stockStatusClass}">
                            ${stockStatusText}
                        </span>
                        </td>
                    <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                        <button onclick="editProduct(${product.ProductId})" class="text-indigo-600 hover:text-indigo-900 mr-3">Edit</button>
                        <button onclick="confirmDelete(${product.ProductId})" class="text-red-600 hover:text-red-900">Delete</button>
                    </td>
                </tr>
                `;
            });
            
            tbody.innerHTML = html;
        }
        
        // Update pagination controls
        function updatePagination() {
            // Update display text
            document.getElementById('currentPageDisplay').textContent = currentPage;
            document.getElementById('totalPagesDisplay').textContent = totalPages;
            
            // Enable/disable prev/next buttons
            document.getElementById('prevPageMobile').disabled = currentPage <= 1;
            document.getElementById('nextPageMobile').disabled = currentPage >= totalPages;
            
            // Generate pagination links
            const paginationContainer = document.getElementById('pagination');
            let paginationHtml = '';
            
            // Previous button
            paginationHtml += `
                <button type="button" onclick="navigateToPage(${currentPage - 1})" class="relative inline-flex items-center px-2 py-2 rounded-l-md border ${currentPage <= 1 ? 'bg-gray-100 cursor-not-allowed' : 'bg-white hover:bg-pink-50 hover:text-pink-700 hover:border-pink-200'} border-gray-300 text-sm font-medium text-gray-500" ${currentPage <= 1 ? 'disabled' : ''}>
                    <span class="sr-only">Previous</span>
                    <svg class="h-5 w-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                        <path fill-rule="evenodd" d="M12.707 5.293a1 1 0 010 1.414L9.414 10l3.293 3.293a1 1 0 01-1.414 1.414l-4-4a1 1 0 010-1.414l4-4a1 1 0 011.414 0z" clip-rule="evenodd" />
                    </svg>
                </button>
            `;
            
            // Page numbers
            const startPage = Math.max(1, currentPage - 2);
            const endPage = Math.min(totalPages, startPage + 4);
            
            for (let i = startPage; i <= endPage; i++) {
                const isActive = i === currentPage;
                paginationHtml += `
                    <button type="button" onclick="navigateToPage(${i})" aria-current="${isActive ? 'page' : 'false'}" class="${isActive ? 'z-10 bg-pink-600 border-pink-600 text-white' : 'bg-white border-gray-300 text-gray-700 hover:bg-gray-50 hover:bg-pink-100 hover:text-pink-700 hover:border-pink-200'} relative inline-flex items-center px-4 py-2 border text-sm font-medium">
                        ${i}
                    </button>
                `;
            }
            
            // Next button
            paginationHtml += `
                <button type="button" onclick="navigateToPage(${currentPage + 1})" class="relative inline-flex items-center px-2 py-2 rounded-r-md border ${currentPage >= totalPages ? 'bg-gray-100 cursor-not-allowed' : 'bg-white hover:bg-pink-50 hover:text-pink-700 hover:border-pink-200'} border-gray-300 text-sm font-medium text-gray-500" ${currentPage >= totalPages ? 'disabled' : ''}>
                    <span class="sr-only">Next</span>
                    <svg class="h-5 w-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
                        <path fill-rule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clip-rule="evenodd" />
                    </svg>
                </button>
            `;
            
            paginationContainer.innerHTML = paginationHtml;
        }
        
        // Navigate to a specific page
        function navigateToPage(page) {
            if (page < 1 || page > totalPages) return;
            
            currentPage = page;
            loadProducts();
            
            // Scroll to top of product list
            document.querySelector('.content').scrollTop = 0;
        }
        
        // Search products (debounced)
        function searchProducts() {
            // Log for debugging
            console.log('Search triggered');
            
            // Show an indicator that search is happening
            const searchInput = document.getElementById('searchInput');
            const searchTerm = searchInput ? searchInput.value.trim() : '';
            console.log('Searching for: ' + searchTerm);
            
            // Reset to first page on new search
                currentPage = 1;
            
            // Show loading indicator
            document.getElementById('productsList').innerHTML = '<tr><td colspan="8" class="px-6 py-4 text-center">Searching products...</td></tr>';
            
            // Call the load function
            loadProducts();
            
            return false; // Prevent default behavior
        }
        
        // Handle image upload
        function handleImageUpload(e) {
            const file = e.target.files[0];
            if (!file) return;
            
            // Validate file type
            const validTypes = ['image/jpeg', 'image/jpg', 'image/png'];
            if (!validTypes.includes(file.type)) {
                showToast('Please select a valid image file (JPG, JPEG or PNG)', 'error');
                return;
            }
            
            // Validate file size (5MB max)
            if (file.size > 5 * 1024 * 1024) {
                showToast('Image must be smaller than 5MB', 'error');
                return;
            }
            
            // Read file as data URL
            const reader = new FileReader();
            reader.onload = function(e) {
                // Store image data
                productImage = e.target.result;
                
                // Show image preview
                const imagePreview = document.getElementById('imagePreview');
                imagePreview.src = e.target.result;
                imagePreview.classList.remove('hidden');
                
                // Show overlay on hover
                document.getElementById('imageOverlay').classList.remove('hidden');
            };
            reader.readAsDataURL(file);
        }
        
        // Open modal for adding a new product
        function openAddModal() {
            // Reset form
            document.getElementById('productName').value = '';
            document.getElementById('productDescription').value = '';
            document.getElementById('productPrice').value = '';
            document.getElementById('productCostPrice').value = '';
            document.getElementById('productStock').value = '';
            document.getElementById('productCategory').value = '';
            document.getElementById('isLatest').checked = false;
            document.getElementById('imagePreview').classList.add('hidden');
            document.getElementById('imageOverlay').classList.add('hidden');
            document.getElementById('modalError').classList.add('hidden');
            
            // Reset state variables
            currentProductId = 0;
            isEditing = false;
            productImage = null;
            
            // Update modal title
            document.getElementById('modalTitle').textContent = 'Add Product';
            
            // Show modal
            document.getElementById('productModal').classList.remove('hidden');
        }
        
        // Open modal for editing an existing product
        function editProduct(productId) {
            // Set state variables
            currentProductId = productId;
            isEditing = true;
            
            // Update modal title
            document.getElementById('modalTitle').textContent = 'Edit Product';
            
            // Reset error message
            document.getElementById('modalError').classList.add('hidden');
            
            // Load product details
            PageMethods.GetProductDetails(productId, 
                function(result) {
                    if (result) {
                        document.getElementById('productName').value = result.Name || '';
                        document.getElementById('productDescription').value = result.Description || '';
                        document.getElementById('productPrice').value = result.Price || '';
                        document.getElementById('productCostPrice').value = result.CostPrice || '';
                        document.getElementById('productStock').value = result.StockQuantity || 0;
                        document.getElementById('productCategory').value = result.CategoryId || '';
                        document.getElementById('isLatest').checked = result.IsLatest || false;
                        
                        // Handle image preview
                        if (result.ImageBase64) {
                            productImage = result.ImageBase64;
                            const imagePreview = document.getElementById('imagePreview');
                            imagePreview.src = 'data:image/jpeg;base64,' + result.ImageBase64;
                            imagePreview.classList.remove('hidden');
                            document.getElementById('imageOverlay').classList.remove('hidden');
            } else {
                            productImage = null;
                            document.getElementById('imagePreview').classList.add('hidden');
                            document.getElementById('imageOverlay').classList.add('hidden');
                        }
                        
                        // Show modal
                        document.getElementById('productModal').classList.remove('hidden');
                    } else {
                        showToast('Failed to load product details', 'error');
                    }
                },
                function(error) {
                    logDebug('Error loading product details: ' + JSON.stringify(error));
                    showToast('Failed to load product details', 'error');
                }
            );
        }
        
        // Close product modal
        function closeModal() {
            document.getElementById('productModal').classList.add('hidden');
        }
        
        // Save product (add or update)
        function saveProduct() {
            // Validate form
            const nameField = document.getElementById('productName');
            const priceField = document.getElementById('productPrice');
            const costPriceField = document.getElementById('productCostPrice');
            const stockField = document.getElementById('productStock');
            const categoryField = document.getElementById('productCategory');
            
            if (!nameField.value.trim()) {
                showModalError('Product name is required');
                nameField.focus();
                return;
            }

            if (!priceField.value || parseFloat(priceField.value) <= 0) {
                showModalError('Please enter a valid price');
                priceField.focus();
                return;
            }

            if (!costPriceField.value || parseFloat(costPriceField.value) <= 0) {
                showModalError('Please enter a valid cost price');
                costPriceField.focus();
                return;
            }

            if (stockField.value === '' || parseInt(stockField.value) < 0) {
                showModalError('Please enter a valid stock quantity');
                stockField.focus();
                return;
            }

            if (!categoryField.value) {
                showModalError('Please select a category');
                categoryField.focus();
                return;
            }

            // Get form values
            const name = nameField.value.trim();
            const description = document.getElementById('productDescription').value.trim();
            const price = parseFloat(priceField.value);
            const costPrice = parseFloat(costPriceField.value);
            const stockQuantity = parseInt(stockField.value);
            const categoryId = parseInt(categoryField.value);
            const isLatest = document.getElementById('isLatest').checked;
            
            // Disable the save button to prevent double submission
            const saveButton = document.querySelector('button[onclick="saveProduct()"]');
            const originalButtonText = saveButton.textContent;
            saveButton.textContent = 'Saving...';
            saveButton.disabled = true;

            if (isEditing) {
                // Update existing product
                PageMethods.UpdateProduct(
                    currentProductId, name, description, price, costPrice, stockQuantity, 
                    productImage || '', categoryId, isLatest,
                    function(result) {
                        if (result && result.Success) {
                            closeModal();
                            showToast('Product updated successfully', 'success');
                            loadProducts(); // Refresh product list
                            loadDashboardStats(); // Refresh stats
                        } else {
                            showModalError(result.Message || 'Failed to update product');
                        }
                    saveButton.textContent = originalButtonText;
                    saveButton.disabled = false;
                    },
                    function(error) {
                        logDebug('Error updating product: ' + JSON.stringify(error));
                        showModalError('An error occurred while updating the product');
                        saveButton.textContent = originalButtonText;
                        saveButton.disabled = false;
                    }
                );
            } else {
                // Add new product
                PageMethods.AddProductSimple(
                    name, description, price.toString(), costPrice.toString(), stockQuantity.toString(), 
                    productImage || '', categoryId.toString(), isLatest,
                    function(result) {
                        if (result && result.Success) {
                        closeModal();
                            showToast('Product added successfully', 'success');
                            loadProducts(); // Refresh product list
                            loadDashboardStats(); // Refresh stats
                    } else {
                            showModalError(result.Message || 'Failed to add product');
                    }
                    saveButton.textContent = originalButtonText;
                    saveButton.disabled = false;
                    },
                    function(error) {
                        logDebug('Error adding product: ' + JSON.stringify(error));
                        showModalError('An error occurred while adding the product');
                        saveButton.textContent = originalButtonText;
                        saveButton.disabled = false;
                    }
                );
            }
        }
        
        // Show delete confirmation modal
        function confirmDelete(productId) {
            currentProductId = productId;
            document.getElementById('deleteModal').classList.remove('hidden');
        }

        // Close delete confirmation modal
        function closeDeleteModal() {
            document.getElementById('deleteModal').classList.add('hidden');
        }

        // Delete product
        function deleteProduct() {
            if (!currentProductId) return;

            // Disable delete button to prevent double click
            const deleteButton = document.querySelector('button[onclick="deleteProduct()"]');
            const originalButtonText = deleteButton.textContent;
            deleteButton.textContent = 'Deleting...';
            deleteButton.disabled = true;

            PageMethods.DeleteProduct(currentProductId,
                function(result) {
                        closeDeleteModal();
                    
                    if (result && result.Success) {
                        showToast('Product deleted successfully', 'success');
                        loadProducts(); // Refresh product list
                        loadDashboardStats(); // Refresh stats
                    } else {
                        showToast(result.Message || 'Failed to delete product', 'error');
                    }
                    
                    deleteButton.textContent = originalButtonText;
                    deleteButton.disabled = false;
                },
                function(error) {
                    logDebug('Error deleting product: ' + JSON.stringify(error));
                    showToast('An error occurred while deleting the product', 'error');
                    closeDeleteModal();
                    
                    deleteButton.textContent = originalButtonText;
                    deleteButton.disabled = false;
                }
            );
        }
        
        // Show modal error message
        function showModalError(message) {
            const modalError = document.getElementById('modalError');
            document.getElementById('modalErrorMessage').textContent = message;
            modalError.classList.remove('hidden');
        }
        
        // Show toast notification
        function showToast(message, type = 'success') {
            const toast = document.getElementById('toast');
            const toastMessage = document.getElementById('toastMessage');

            // Set color based on type
            if (type === 'success') {
                toast.classList.remove('bg-red-600');
                toast.classList.add('bg-green-600');
            } else {
                toast.classList.remove('bg-green-600');
                toast.classList.add('bg-red-600');
            }
            
            // Set message
            toastMessage.textContent = message;
            
            // Show toast
            toast.classList.remove('hidden');

            // Hide after 3 seconds
            setTimeout(() => {
                toast.classList.add('hidden');
            }, 3000);
        }

        // Debug logging
        function logDebug(message) {
            const debugOutput = document.getElementById('debugOutput');
            debugOutput.innerHTML += new Date().toLocaleTimeString() + ': ' + message + '<br>';
            
            // Auto-scroll to bottom
            debugOutput.scrollTop = debugOutput.scrollHeight;
            
            // Also log to console if in dev mode
            if (devMode) {
                console.log(message);
            }
        }
        
        // Show debug info panel
        function showDebugInfo() {
            document.getElementById('debugInfo').classList.remove('hidden');
        }

        // Hide debug info panel
        function hideDebugInfo() {
            document.getElementById('debugInfo').classList.add('hidden');
        }

        // Toggle dev mode
        function toggleDevMode() {
            devMode = !devMode;
            document.getElementById('devModeToggle').textContent = 'Dev: ' + (devMode ? 'ON' : 'OFF');

            if (devMode) {
                logDebug('Developer mode activated');
            }
        }
        
        // Debounce function to limit how often a function is called
        function debounce(func, wait) {
            let timeout;
            return function(...args) {
                const context = this;
                clearTimeout(timeout);
                timeout = setTimeout(() => func.apply(context, args), wait);
            };
        }
        
        // Direct handler for Enter key in search
        function handleSearchKeyDown(event) {
            // Check if the Enter key was pressed
            if (event.key === 'Enter' || event.keyCode === 13) {
                event.preventDefault();
                event.stopPropagation();
                // Directly call search without debounce
                searchProducts();
                return false;
            }
        }
        
        // Test database connection
        function testDatabaseConnection() {
            document.getElementById('productsList').innerHTML = '<tr><td colspan="8" class="px-6 py-4 text-center">Testing database connection...</td></tr>';
            
            PageMethods.TestDatabaseConnection(
                function(result) {
                    if (result && result.Success) {
                        document.getElementById('productsList').innerHTML = `
                            <tr>
                                <td colspan="8" class="px-6 py-4 text-center">
                                    <div class="space-y-3">
                                        <p class="text-green-600 font-semibold">Database connection successful.</p>
                                        <p>${result.Message}</p>
                                        <p class="text-sm text-gray-500">Try adjusting your search terms or filters and try again.</p>
                                        <button onclick="loadProducts()" class="mt-2 inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-pink-600 hover:bg-pink-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-pink-500">
                                            Reload Products
                                        </button>
                                    </div>
                                </td>
                            </tr>`;
                    } else {
                        document.getElementById('productsList').innerHTML = `
                            <tr>
                                <td colspan="8" class="px-6 py-4 text-center">
                                    <div class="space-y-3">
                                        <p class="text-red-600 font-semibold">Database connection failed.</p>
                                        <p>${result.Message || 'Unknown error'}</p>
                                        <p class="text-sm text-gray-500">Please contact your system administrator.</p>
                                    </div>
                                </td>
                            </tr>`;
                    }
                },
                function(error) {
                    logDebug('Error testing database connection: ' + JSON.stringify(error));
                    document.getElementById('productsList').innerHTML = `
                        <tr>
                            <td colspan="8" class="px-6 py-4 text-center">
                                <div class="space-y-3">
                                    <p class="text-red-600 font-semibold">Error testing database connection.</p>
                                    <p>A system error occurred while testing the database connection.</p>
                                    <p class="text-sm text-gray-500">Please check the console for more details.</p>
                                </div>
                            </td>
                        </tr>`;
                }
            );
        }
    </script>
</asp:Content>
