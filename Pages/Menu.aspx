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
    <section class="relative bg-cover bg-center h-[300px] flex items-center justify-center text-white text-center mb-8" style="background-image: linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5)), url('../Images/hero-section.jpg');">
        <div class="max-w-3xl px-5">
            <h1 class="text-4xl font-semibold mb-4">Our Delicious Pastries</h1>
            <p class="text-xl">Explore our wide selection of freshly baked pastries made with love and the finest ingredients</p>
        </div>
    </section>

    <!-- Category Tabs -->
    <section class="mb-8">
        <div class="container mx-auto px-4 lg:px-[160px]">
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
        <div class="container mx-auto px-4 lg:px-[160px]">
            <div id="productsLoading" class="flex flex-col items-center justify-center py-12">
                <div class="w-10 h-10 border-4 border-gray-200 border-t-[#96744F] rounded-full"></div>
                <p class="text-lg text-gray-600 mt-4">Loading products...</p>
            </div>
            <div id="productsGrid" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-8 hidden"></div>
            <div id="noProductsMessage" class="text-center py-8 text-lg text-gray-600 hidden">
                <p>No products found. Please try another category.</p>
            </div>
        </div>
    </section>

    <script type="text/javascript">
        // Main array to store all products
        let allProducts = [];
        let categories = [];
        let currentCategory = 'all';

        // Wait for the DOM to be fully loaded
        document.addEventListener('DOMContentLoaded', function () {
            // Create toast element
            const toastContainer = document.createElement('div');
            toastContainer.id = 'toastNotification';
            toastContainer.className = 'fixed bottom-5 right-5 bg-[#96744F] text-white py-4 px-5 rounded shadow-lg z-50 transform translate-y-full opacity-0 transition-all duration-300 max-w-[300px] hidden';
            document.body.appendChild(toastContainer);
            
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
            populateProductsGrid(filteredProducts);
        }

        // Function to populate products grid
        function populateProductsGrid(products) {
            const productsGrid = document.getElementById('productsGrid');
            productsGrid.innerHTML = '';

            if (products.length === 0) {
                productsGrid.innerHTML = '<div class="col-span-full text-center py-8 text-lg text-gray-600 bg-gray-100 rounded-lg">No products found</div>';
                document.getElementById('productsLoading').style.display = 'none';
                productsGrid.style.display = 'grid';
                return;
            }

            products.forEach(product => {
                const productCard = document.createElement('div');
                productCard.className = 'rounded-lg overflow-hidden shadow-md transition-all duration-300 hover:shadow-lg hover:-translate-y-1 bg-white flex flex-col h-full';
                
                // Determine image source - use base64 if available, otherwise use default
                let imgSrc = '';
                if (product.ImageBase64) {
                    console.log(`Product ${product.ProductId} has base64 image data`);
                    imgSrc = `data:image/jpeg;base64,${product.ImageBase64}`;
                } else {
                    console.log(`Product ${product.ProductId} has no image data, using default`);
                    // Use absolute path for default image
                    imgSrc = '../Images/product-placeholder.svg';
                }
                
                const stockClass = product.StockQuantity > 0 ? 'text-green-600' : 'text-red-600';
                const stockText = product.StockQuantity > 0 ? 'In Stock' : 'Out of Stock';
                
                productCard.innerHTML = `
                    <div class="h-[200px] overflow-hidden relative">
                        <img src="${imgSrc}" alt="${product.Name}" onerror="this.src='../Images/product-placeholder.svg'" class="w-full h-full object-cover transition-transform duration-300 hover:scale-105">
                    </div>
                    <div class="p-6 flex flex-col flex-grow">
                        <h3 class="text-lg text-gray-800 mb-2">${product.Name}</h3>
                        <p class="text-gray-600 text-sm mb-4 line-clamp-2 leading-relaxed">${product.Description || 'No description available'}</p>
                        <div class="text-[#96744F] font-bold text-lg mb-2">₱${parseFloat(product.Price).toFixed(2)}</div>
                        <div class="${stockClass} text-sm mb-4">${stockText}</div>
                        <button class="mt-auto bg-[#96744F] hover:bg-[#7a5f3e] text-white font-bold py-3 px-4 rounded transition-colors duration-300 disabled:bg-gray-300 disabled:cursor-not-allowed" 
                                onclick="viewProduct(${product.ProductId})" 
                                ${product.StockQuantity <= 0 ? 'disabled' : ''}>
                            View Product
                        </button>
                    </div>
                `;
                
                productsGrid.appendChild(productCard);
            });
            
            // Make sure products grid is visible and loading spinner is hidden
            productsGrid.style.display = 'grid';
            document.getElementById('productsLoading').style.display = 'none';
        }

        // Function to view product details (will be implemented later)
        function viewProduct(productId) {
            // Just log the action for now
            console.log(`View product: ${productId}`);
            // Redirect to product detail page
            window.location.href = `/Pages/ProductDetails.aspx?id=${productId}`;
        }
        
        // Function to add product to cart
        function addToCart(productId, productName, price) {
            console.log(`Adding product to cart: ${productId} - ${productName} - $${price}`);
            
            // TODO: Implement actual cart functionality with server-side storage
            
            // For now, just display a toast notification
            showToast(`${productName} added to your cart!`);
            
            // You can add additional logic here when the cart functionality is implemented
        }
        
        // Function to show toast notification
        function showToast(message) {
            const toast = document.getElementById('toastNotification');
            toast.textContent = message;
            toast.classList.remove('hidden');
            
            // Show the toast
            setTimeout(() => {
                toast.classList.remove('translate-y-full', 'opacity-0');
            }, 10);
            
            // Hide after 3 seconds
            setTimeout(() => {
                toast.classList.add('translate-y-full', 'opacity-0');
                
                // After animation completes, hide the element
                setTimeout(() => {
                    toast.classList.add('hidden');
                }, 300);
            }, 3000);
        }
    </script>
</asp:Content>
