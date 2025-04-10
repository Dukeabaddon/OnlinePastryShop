<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="ProductDetails.aspx.cs" Inherits="OnlinePastryShop.Pages.ProductDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .text-black {
            color: #000000;
        }
        
        .breadcrumb {
            padding: 1.5rem 0;
            margin-bottom: 1rem;
        }
        
        .breadcrumb-item {
            display: inline-block;
            font-size: 0.95rem;
            color: #333;
        }
        
        .breadcrumb-item a {
            color: #96744F;
            text-decoration: none;
        }
        
        .breadcrumb-item a:hover {
            text-decoration: underline;
        }
        
        .breadcrumb-separator {
            margin: 0 0.5rem;
            color: #666;
        }
        
        /* Product image */
        .product-image-container {
            overflow: hidden;
            border-radius: 0.5rem;
            cursor: zoom-in;
            width: 100%;
            aspect-ratio: 1/1;
            background-color: #f9f9f9;
            box-shadow: 0 4px 6px rgba(0,0,0,0.05);
        }
        
        .product-image-container img {
            width: 100%;
            height: 100%;
            object-fit: cover;
            transition: transform 0.4s ease;
        }
        
        .product-image-container:hover img {
            transform: scale(1.1);
        }
        
        /* Quantity control styling */
        .quantity-control {
            display: flex;
            align-items: center;
            max-width: 120px;
            border: 1px solid #e5e5e5;
            border-radius: 4px;
            overflow: hidden;
        }
        
        .quantity-control button {
            width: 36px;
            height: 36px;
            display: flex;
            align-items: center;
            justify-content: center;
            background-color: #f9f9f9;
            color: #333;
            border: none;
            font-size: 18px;
            font-weight: normal;
        }
        
        .quantity-control button:hover {
            background-color: #efefef;
        }
        
        .quantity-control input {
            width: 50px;
            height: 36px;
            text-align: center;
            border: none;
            border-left: 1px solid #e5e5e5;
            border-right: 1px solid #e5e5e5;
            padding: 0;
            margin: 0;
        }
        
        /* Hide number input spinner */
        input[type=number]::-webkit-inner-spin-button, 
        input[type=number]::-webkit-outer-spin-button { 
            -webkit-appearance: none;
            margin: 0;
        }
        
        input[type=number] {
            -moz-appearance: textfield;
        }
        
        /* Key selling points */
        .selling-points {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 10px;
            margin-top: 20px;
        }
        
        .selling-point {
            padding: 15px;
            background-color: #F8F3EC;
            border-radius: 8px;
            text-align: center;
        }
        
        /* Related products */
        .related-products {
            margin-top: 40px;
            overflow: hidden;
        }
        
        .product-carousel {
            display: flex;
            overflow-x: auto;
            scroll-behavior: smooth;
            scrollbar-width: none;
            gap: 20px;
            padding: 20px 0;
        }
        
        .product-carousel::-webkit-scrollbar {
            display: none;
        }
        
        .product-card {
            min-width: 280px;
            flex: 0 0 auto;
            background: white;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            transition: transform 0.3s;
        }
        
        .product-card:hover {
            transform: translateY(-5px);
        }
        
        /* Success modal */
        .success-modal {
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%) scale(0.8);
            background: white;
            border-radius: 10px;
            box-shadow: 0 5px 20px rgba(0,0,0,0.2);
            z-index: 1000;
            padding: 20px;
            text-align: center;
            max-width: 400px;
            width: 100%;
            opacity: 0;
            visibility: hidden;
            transition: all 0.3s;
        }
        
        .success-modal.active {
            opacity: 1;
            visibility: visible;
            transform: translate(-50%, -50%) scale(1);
        }
        
        .modal-overlay {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0,0,0,0.5);
            z-index: 999;
            opacity: 0;
            visibility: hidden;
            transition: all 0.3s;
        }
        
        .modal-overlay.active {
            opacity: 1;
            visibility: visible;
        }
        
        /* Page entrance animation */
        @keyframes fadeIn {
            from {
                opacity: 0;
            }
            to {
                opacity: 1;
            }
        }
        
        .animate-fadeIn {
            animation: fadeIn 0.4s ease forwards;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="bg-[#FBF7F1] min-h-screen animate-fadeIn">
        <div class="container mx-auto px-6 lg:px-24">
            <!-- Breadcrumb Navigation -->
            <div class="breadcrumb">
                <div class="breadcrumb-item">
                    <a href="/Pages/Default.aspx">Home</a>
                </div>
                <span class="breadcrumb-separator">/</span>
                <div class="breadcrumb-item">
                    <a href="/Pages/Menu.aspx">Menu</a>
                </div>
                <span class="breadcrumb-separator">/</span>
                <div class="breadcrumb-item text-black">
                    <asp:Literal ID="litBreadcrumbProduct" runat="server"></asp:Literal>
                </div>
            </div>

            <!-- Product Details Section -->
            <div class="bg-white rounded-lg p-8 shadow-sm mb-12">
                <div class="grid grid-cols-1 md:grid-cols-2 gap-12">
                    <!-- Left Column - Product Image -->
                    <div>
                        <div class="product-image-container">
                            <asp:Image ID="imgProduct" runat="server" CssClass="product-image" AlternateText="Product Image" />
                        </div>
                    </div>
                    
                    <!-- Right Column - Product Details -->
                    <div>
                        <!-- Category -->
                        <div class="text-[#96744F] text-sm uppercase tracking-wider mb-2">
                            <asp:Literal ID="litCategory" runat="server"></asp:Literal>
                        </div>
                        
                        <!-- Product Name -->
                        <h1 class="text-3xl text-black font-semibold mb-2">
                            <asp:Literal ID="litProductName" runat="server"></asp:Literal>
                        </h1>
                        
                        <!-- Ratings -->
                        <div class="mb-4">
                            <asp:Literal ID="litRating" runat="server"></asp:Literal>
                        </div>
                        
                        <!-- Price -->
                        <div class="text-2xl font-bold text-black mb-6">
                            <asp:Literal ID="litPrice" runat="server"></asp:Literal>
                        </div>
                        
                        <!-- Description -->
                        <div class="text-gray-700 mb-8">
                            <asp:Literal ID="litDescription" runat="server"></asp:Literal>
                        </div>
                        
                        <!-- Stock Status -->
                        <div ID="divStockStatus" runat="server" class="mb-6"></div>
                        
                        <!-- Quantity and Add to Cart -->
                        <div class="flex flex-col sm:flex-row items-start sm:items-center gap-6 mb-8">
                            <div>
                                <label for="quantity" class="block text-sm font-medium text-gray-700 mb-2">Quantity:</label>
                                <div class="quantity-control">
                                    <button type="button" id="btnDecrement" onclick="decrementQuantity()">
                                        −
                                    </button>
                                    <input type="number" id="inputQuantity" value="1" min="1" class="quantity-input" readonly />
                                    <button type="button" id="btnIncrement" onclick="incrementQuantity()">
                                        +
                                    </button>
                                </div>
                                <asp:HiddenField ID="hiddenQuantity" runat="server" Value="1" />
                            </div>
                            
                            <div class="w-full sm:w-auto">
                                <asp:Button ID="btnAddToCart" runat="server" Text="Add to Cart" 
                                    CssClass="w-full sm:w-auto bg-[#96744F] hover:bg-[#7a5f3e] text-white py-3 px-8 rounded transition-colors duration-300"
                                    OnClick="btnAddToCart_Click" />
                            </div>
                        </div>
                        
                        <!-- Key Selling Points -->
                        <div>
                            <h3 class="text-lg font-medium text-black mb-4">Why You'll Love It</h3>
                            <div class="selling-points">
                                <div class="selling-point">
                                    <p class="font-medium">Made fresh daily</p>
                                </div>
                                <div class="selling-point">
                                    <p class="font-medium">No preservatives</p>
                                </div>
                                <div class="selling-point">
                                    <p class="font-medium">Local ingredients</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            
            <!-- Related Products Section -->
            <div class="mb-16">
                <h2 class="text-2xl font-semibold text-black mb-6">You May Also Like</h2>
                <div class="related-products">
                    <div class="product-carousel" id="relatedProductsContainer">
                        <!-- Loading message that will be replaced by product cards -->
                        <div class="w-full text-center py-4">
                            <div class="inline-block animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-[#96744F]"></div>
                            <p class="mt-2 text-gray-600">Loading related products...</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    
    <!-- Success Modal -->
    <div class="modal-overlay" id="modalOverlay"></div>
    <div class="success-modal" id="successModal">
        <div class="text-green-600 text-6xl mb-4">
            <i class="fas fa-check-circle"></i>
        </div>
        <h3 class="text-xl font-bold mb-2">Added to Cart!</h3>
        <p class="mb-6 text-gray-600" id="modalProductName"></p>
        <button id="modalCloseBtn" class="bg-[#96744F] hover:bg-[#7a5f3e] text-white font-semibold py-2 px-6 rounded transition-colors duration-300">
            Continue Shopping
        </button>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script type="text/javascript">
        // Log when page is loaded
        document.addEventListener('DOMContentLoaded', function() {
            console.log('ProductDetails page loaded');
            const relatedContainer = document.getElementById('relatedProductsContainer');
            if (relatedContainer) {
                console.log('Related products container found in DOM');
            } else {
                console.error('Related products container NOT found in DOM');
            }
            
            // Ensure related products section is visible
            const relatedSection = document.querySelector('.related-products').parentElement;
            if (relatedSection) {
                relatedSection.style.display = 'block';
                console.log('Related products section visibility ensured');
            }
            
            // Safety timeout - hide loading message after 5 seconds if products don't load
            setTimeout(function() {
                const loadingMsg = document.querySelector('#relatedProductsContainer .animate-spin');
                if (loadingMsg && loadingMsg.parentElement) {
                    console.log('Safety timeout: Hiding loading message after 5 seconds');
                    
                    // Create a message for no products found
                    const noProductsMsg = document.createElement('p');
                    noProductsMsg.className = 'w-full text-center py-4 text-gray-600';
                    noProductsMsg.innerText = 'No related products available.';
                    
                    // Replace the loading message with the no products message
                    loadingMsg.parentElement.parentElement.replaceWith(noProductsMsg);
                }
            }, 5000);
        });
        
        // Get stock quantity from server-side variable
        const stockQuantity = <%= StockQuantity > 0 ? StockQuantity : 0 %>;
        
        // Quantity selector functions
        function incrementQuantity() {
            const input = document.getElementById('inputQuantity');
            const hidden = document.getElementById('<%= hiddenQuantity.ClientID %>');
            
            let currentValue = parseInt(input.value);
            
            // Don't increment beyond stock quantity
            if (currentValue < stockQuantity) {
                currentValue++;
                input.value = currentValue;
                hidden.value = currentValue;
            }
            
            // Disable increment button if max reached
            if (currentValue >= stockQuantity) {
                document.getElementById('btnIncrement').classList.add('opacity-50', 'cursor-not-allowed');
            }
            
            // Enable decrement button since value is now > 1
            document.getElementById('btnDecrement').classList.remove('opacity-50', 'cursor-not-allowed');
        }
        
        function decrementQuantity() {
            const input = document.getElementById('inputQuantity');
            const hidden = document.getElementById('<%= hiddenQuantity.ClientID %>');
            
            let currentValue = parseInt(input.value);
            
            // Don't decrement below 1
            if (currentValue > 1) {
                currentValue--;
                input.value = currentValue;
                hidden.value = currentValue;
            }
            
            // Disable decrement button if min reached
            if (currentValue <= 1) {
                document.getElementById('btnDecrement').classList.add('opacity-50', 'cursor-not-allowed');
            }
            
            // Enable increment button since we've decremented
            document.getElementById('btnIncrement').classList.remove('opacity-50', 'cursor-not-allowed');
        }
        
        // Add to cart success animation with modal
        function showCartSuccessAnimation() {
            const productName = document.querySelector('h1').innerText;
            document.getElementById('modalProductName').innerText = productName;
            
            // Show modal and overlay
            document.getElementById('modalOverlay').classList.add('active');
            document.getElementById('successModal').classList.add('active');
            
            // Auto-hide after 3 seconds
            setTimeout(function() {
                closeModal();
            }, 3000);
        }
        
        function closeModal() {
            document.getElementById('modalOverlay').classList.remove('active');
            document.getElementById('successModal').classList.remove('active');
        }
        
        // Initialize quantity controls and events on page load
        document.addEventListener('DOMContentLoaded', function() {
            // Initialize state of decrement button (disabled if quantity is 1)
            if (parseInt(document.getElementById('inputQuantity').value) <= 1) {
                document.getElementById('btnDecrement').classList.add('opacity-50', 'cursor-not-allowed');
            }
            
            // Initialize state of increment button (disabled if quantity equals stock)
            if (parseInt(document.getElementById('inputQuantity').value) >= stockQuantity) {
                document.getElementById('btnIncrement').classList.add('opacity-50', 'cursor-not-allowed');
            }
            
            // Set up close button for modal
            document.getElementById('modalCloseBtn').addEventListener('click', closeModal);
            document.getElementById('modalOverlay').addEventListener('click', closeModal);
        });
    </script>
</asp:Content>
