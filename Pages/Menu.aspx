<%@ Page Title="Menu" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="Menu.aspx.cs" Inherits="OnlinePastryShop.Pages.Menu" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Add ScriptManager for PageMethods -->
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true">
        <Scripts>
            <asp:ScriptReference Name="MicrosoftAjax.js" />
            <asp:ScriptReference Name="MicrosoftAjaxWebForms.js" />
        </Scripts>
    </asp:ScriptManager>

    <!-- Hero Section -->
    <section class="relative bg-cover bg-center h-[300px] flex items-center justify-center text-white text-center mb-8" style="background-image: linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5)), url('/Images/hero-section.jpg');">
        <div class="max-w-3xl px-5">
            <h1 class="text-4xl font-semibold mb-4">Our Delicious Pastries</h1>
            <p class="text-xl">Explore our wide selection of freshly baked pastries made with love and the finest ingredients</p>
        </div>
    </section>

    <!-- Category Tabs -->
    <section class="mb-8">
        <div class="container mx-auto px-[160px]">
            <div class="flex flex-wrap gap-3 justify-center">
                <button type="button" class="px-6 py-2 bg-[#96744F] text-white border border-[#96744F] rounded-full cursor-pointer transition-all duration-300 font-medium active" data-category="all">All</button>
                <button type="button" class="px-6 py-2 bg-white border border-gray-300 rounded-full cursor-pointer transition-all duration-300 font-medium hover:bg-gray-100" data-category="latest">Latest</button>
                <!-- Dynamic category tabs will be added here -->
                <div id="dynamicCategoryTabs" class="flex flex-wrap gap-3"></div>
            </div>
        </div>
    </section>

    <!-- Products Grid -->
    <section class="pb-12">
        <div class="container mx-auto px-[160px]">
            <div id="productsLoading" class="flex flex-col items-center justify-center py-12">
                <div class="spinner mb-4"></div>
                <p class="text-lg text-gray-600">Loading products...</p>
            </div>
            <div id="productsGrid" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-8"></div>
            <div id="noProductsMessage" class="text-center py-8 text-lg text-gray-600 hidden">
                <p>No products found. Please try another category.</p>
            </div>
        </div>
    </section>

    <style>
        /* Essential custom styles */
        .spinner {
            border: 4px solid rgba(0, 0, 0, 0.1);
            border-radius: 50%;
            border-top-color: #96744F;
            width: 40px;
            height: 40px;
            animation: spin 1s linear infinite;
        }
        
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
        
        button.active {
            background-color: #96744F !important;
            color: white !important;
            border-color: #96744F !important;
        }
    </style>

    <script type="text/javascript">
        // Main array to store all products
        let allProducts = [];
        let categories = [];
        let currentCategory = 'all';

        // Wait for the DOM to be fully loaded
        document.addEventListener('DOMContentLoaded', function () {
            // Load products
            loadProducts();

            // Set up event delegation for category tabs
            document.querySelector('.container').addEventListener('click', function (e) {
                if (e.target.classList.contains('px-6') && e.target.tagName === 'BUTTON') {
                    e.preventDefault();
                    
                    // Remove active class from all tabs
                    document.querySelectorAll('.container button[data-category]').forEach(btn => {
                        btn.classList.remove('active');
                        btn.classList.remove('bg-[#96744F]');
                        btn.classList.remove('text-white');
                        btn.classList.remove('border-[#96744F]');
                        btn.classList.add('bg-white');
                        btn.classList.add('border-gray-300');
                    });

                    // Add active class to clicked tab
                    e.target.classList.add('active');
                    e.target.classList.add('bg-[#96744F]');
                    e.target.classList.add('text-white');
                    e.target.classList.add('border-[#96744F]');
                    e.target.classList.remove('bg-white');
                    e.target.classList.remove('border-gray-300');

                    // Filter products by category
                    const category = e.target.getAttribute('data-category');
                    currentCategory = category;
                    filterProductsByCategory(category);
                    
                    // Prevent default action
                    return false;
                }
            });
        });

        // Function to load products from the server
        function loadProducts() {
            // Show loading spinner
            document.getElementById('productsLoading').style.display = 'flex';
            document.getElementById('productsGrid').style.display = 'none';
            document.getElementById('noProductsMessage').style.display = 'none';

            // Call server-side method to get products
            PageMethods.GetProducts(function (response) {
                // Hide loading spinner
                document.getElementById('productsLoading').style.display = 'none';
                console.log("Server response:", response);

                if (response.Success) {
                    // Store all products
                    allProducts = response.Products;
                    console.log(`Loaded ${allProducts.length} products`);

                    // Extract unique categories
                    extractCategories();

                    // Generate category tabs
                    generateCategoryTabs();

                    // Display all products initially
                    filterProductsByCategory(currentCategory);
                } else {
                    // Show error message
                    document.getElementById('noProductsMessage').innerText = response.Message;
                    document.getElementById('noProductsMessage').style.display = 'block';
                    console.error("Failed to load products:", response.Message);
                }
            }, function (error) {
                // Hide loading spinner
                document.getElementById('productsLoading').style.display = 'none';

                // Show error message
                document.getElementById('noProductsMessage').innerText = "Error loading products. Please try again later.";
                document.getElementById('noProductsMessage').style.display = 'block';
                console.error("Error calling GetProducts:", error);
            });
        }

        // Function to extract unique categories from products
        function extractCategories() {
            const uniqueCategories = new Map();

            allProducts.forEach(product => {
                if (product.CategoryId && product.CategoryName) {
                    uniqueCategories.set(product.CategoryId, product.CategoryName);
                    console.log(`Found category: ${product.CategoryName} (ID: ${product.CategoryId})`);
                }
            });

            categories = Array.from(uniqueCategories).map(([id, name]) => ({
                id: id,
                name: name
            }));
            
            console.log(`Extracted ${categories.length} unique categories`);
        }

        // Function to generate category tabs
        function generateCategoryTabs() {
            const tabsContainer = document.getElementById('dynamicCategoryTabs');
            tabsContainer.innerHTML = '';

            categories.forEach(category => {
                const tab = document.createElement('button');
                tab.className = 'px-6 py-2 bg-white border border-gray-300 rounded-full cursor-pointer transition-all duration-300 font-medium hover:bg-gray-100';
                tab.setAttribute('type', 'button');
                tab.setAttribute('data-category', category.id);
                tab.textContent = category.name;
                tabsContainer.appendChild(tab);
                console.log(`Added category tab: ${category.name}`);
            });
        }

        // Function to filter products by category
        function filterProductsByCategory(category) {
            console.log(`Filtering by category: ${category}`);
            let filteredProducts = [];

            if (category === 'all') {
                filteredProducts = allProducts;
            } else if (category === 'latest') {
                filteredProducts = allProducts.filter(product => product.IsLatest === true);
            } else {
                filteredProducts = allProducts.filter(product => product.CategoryId == category);
            }

            console.log(`Found ${filteredProducts.length} products for category ${category}`);
            displayProducts(filteredProducts);
        }

        // Function to display products
        function displayProducts(products) {
            const productsGrid = document.getElementById('productsGrid');
            productsGrid.innerHTML = '';

            if (products.length === 0) {
                document.getElementById('noProductsMessage').style.display = 'block';
                productsGrid.style.display = 'none';
                return;
            }

            document.getElementById('noProductsMessage').style.display = 'none';
            productsGrid.style.display = 'grid';

            products.forEach(product => {
                const productCard = document.createElement('div');
                productCard.className = 'border border-gray-200 rounded-lg overflow-hidden transition-all duration-300 hover:shadow-lg hover:-translate-y-1';

                let stockLabelHtml = '';
                let actionBtnHtml = '';

                if (product.StockQuantity <= 0) {
                    stockLabelHtml = '<span class="inline-block px-2 py-1 text-xs rounded bg-red-100 text-red-800 mb-2">Out of Stock</span>';
                    actionBtnHtml = '<button disabled class="px-4 py-1 bg-gray-300 text-white rounded cursor-not-allowed text-sm">Out of Stock</button>';
                } else if (product.StockQuantity < 5) {
                    stockLabelHtml = `<span class="inline-block px-2 py-1 text-xs rounded bg-amber-100 text-amber-800 mb-2">Low Stock: ${product.StockQuantity} left</span>`;
                    actionBtnHtml = `<button onclick="viewProduct(${product.ProductId})" class="px-4 py-1 bg-[#96744F] text-white rounded hover:bg-[#A27547] transition-colors text-sm">View</button>`;
                } else {
                    // No stock label for in-stock items
                    stockLabelHtml = ''; 
                    actionBtnHtml = `<button onclick="viewProduct(${product.ProductId})" class="px-4 py-1 bg-[#96744F] text-white rounded hover:bg-[#A27547] transition-colors text-sm">View</button>`;
                }

                // For image path, use direct URL with productId parameter
                productCard.innerHTML = `
                    <div class="h-44 bg-gray-100 bg-cover bg-center">
                        <img src="${product.HasImage ? `/GetProductImage.aspx?id=${product.ProductId}` : '/Images/product-placeholder.svg'}" alt="${product.Name}" class="w-full h-full object-cover">
                    </div>
                    <div class="p-4">
                        <h3 class="font-semibold text-lg mb-2">${product.Name}</h3>
                        <p class="text-sm text-gray-600 mb-3 overflow-hidden line-clamp-2">${product.Description}</p>
                        ${stockLabelHtml}
                        <div class="flex justify-between items-center mt-3">
                            <div class="font-semibold text-[#96744F]">$${parseFloat(product.Price).toFixed(2)}</div>
                            ${actionBtnHtml}
                        </div>
                    </div>
                `;

                productsGrid.appendChild(productCard);
            });
        }

        // Function to view product details (will be implemented later)
        function viewProduct(productId) {
            // Just log the action for now
            console.log(`View product: ${productId}`);
            // Redirect to product detail page
            window.location.href = `/ProductDetail.aspx?id=${productId}`;
        }
    </script>
</asp:Content>
