<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="OnlinePastryShop.Pages.Default" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .parallax {
            background-attachment: fixed;
            background-position: center;
            background-repeat: no-repeat;
            background-size: cover;
        }
        
        /* Animation keyframes */
        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translateY(20px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
        
        @keyframes fadeInLeft {
            from {
                opacity: 0;
                transform: translateX(-50px);
            }
            to {
                opacity: 1;
                transform: translateX(0);
            }
        }
        
        @keyframes fadeInRight {
            from {
                opacity: 0;
                transform: translateX(50px);
            }
            to {
                opacity: 1;
                transform: translateX(0);
            }
        }
        
        @keyframes fadeInBlur {
            from {
                opacity: 0;
                filter: blur(5px);
                transform: scale(0.95);
            }
            to {
                opacity: 1;
                filter: blur(0);
                transform: scale(1);
            }
        }
        
        /* Animation classes */
        .animate-fadeInUp {
            animation: fadeInUp 0.8s ease forwards;
        }
        
        .animate-fadeInLeft {
            animation: fadeInLeft 0.8s ease forwards;
        }
        
        .animate-fadeInRight {
            animation: fadeInRight 0.8s ease forwards;
        }
        
        .animate-fadeInBlur {
            animation: fadeInBlur 1s ease forwards;
        }
        
        /* Hidden state classes */
        .hidden-up {
            opacity: 0;
            transform: translateY(20px);
        }
        
        .hidden-left {
            opacity: 0;
            transform: translateX(-50px);
        }
        
        .hidden-right {
            opacity: 0;
            transform: translateX(50px);
        }
        
        .hidden-blur {
            opacity: 0;
            filter: blur(5px);
            transform: scale(0.95);
        }
        
        .hero-btn {
            transition: all 0.3s ease;
        }
        
        .primary-btn:hover {
            background-color: #e9ca5d;
            transform: translateY(-2px);
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }
        
        .secondary-btn:hover {
            background-color: rgba(255, 255, 255, 0.15);
            transform: translateY(-2px);
        }
        
        .category-card {
            transition: all 0.3s ease;
            position: relative;
            overflow: hidden;
        }
        
        .category-card:hover {
            box-shadow: 0 10px 15px rgba(0, 0, 0, 0.1);
        }
        
        .category-card img {
            transition: transform 0.5s ease;
            width: 100%;
            height: 100%;
            object-fit: cover;
        }
        
        .category-card:hover img {
            transform: scale(1.1);
        }
        
        .category-card:hover .category-overlay {
            opacity: 0.7;
        }
        
        .category-overlay {
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0, 0, 0, 0.5);
            opacity: 0.6;
            transition: all 0.3s ease;
        }
        
        .product-card {
            transition: all 0.3s ease;
            border: 1px solid #f0f0f0;
        }
        
        .product-card:hover {
            box-shadow: 0 10px 15px rgba(0, 0, 0, 0.05);
            transform: translateY(-3px);
        }
        
        .product-card img {
            transition: all 0.5s ease;
        }
        
        .product-card:hover img {
            transform: scale(1.03);
        }
        
        .add-btn {
            transition: all 0.2s ease;
            background-color: #96744F;
        }
        
        .add-btn:hover {
            background-color: #7d6142;
        }
    </style>
    <script type="text/javascript">
        function viewProduct(productId) {
            // Redirect to the product details page
            window.location.href = "/Pages/ProductDetails.aspx?id=" + productId;
        }
        
        function addToCart(productId) {
            // You can replace this with the actual implementation
            // for now it will just show an alert
            alert("Product " + productId + " has been added to cart!");
            
            // Uncomment the following code when you have the actual cart implementation
            /*
            $.ajax({
                type: "POST",
                url: "Cart.aspx/AddToCart",
                data: JSON.stringify({ productId: productId, quantity: 1 }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (response) {
                    // Show success message
                    alert("Product added to cart successfully!");
                },
                error: function (error) {
                    console.log(error);
                    alert("Error adding product to cart. Please try again.");
                }
            });
            */
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Hero Section -->
    <section class="relative h-screen w-full overflow-hidden">
        <!-- Parallax Background -->
        <div class="absolute inset-0 parallax" style="background-image: url('../Images/hero-section.jpg');"></div>
        
        <!-- Brown Overlay -->
        <div class="absolute inset-0 bg-[#96744F]/70"></div>
        
        <!-- Content -->
        <div class="relative z-10 max-w-7xl mx-auto px-4 h-full flex flex-col justify-center">
            <div class="max-w-3xl">
                <h1 class="text-5xl md:text-6xl lg:text-7xl text-white font-bold mb-6" style="font-family: 'Playfair Display', serif;">
                    Handcrafted Pastries for Every Occasion
                </h1>
                
                <p class="text-white text-lg mb-10">
                    Discover our delicious selection of freshly baked pastries, cakes, and bread made with the finest ingredients.
                </p>
                
                <div class="flex flex-wrap gap-4">
                    <a href="/Menu" class="hero-btn primary-btn bg-yellow-300 text-gray-800 px-8 py-3 rounded font-medium inline-block">
                        Explore Our Menu
                    </a>
                    <a href="/About" class="hero-btn secondary-btn border-2 border-white text-white px-8 py-3 rounded font-medium inline-block">
                        Our Story
                    </a>
                </div>
            </div>
        </div>
    </section>

    <!-- Our Categories Section -->
    <section class="py-16 px-4 bg-[#FDF7ED]">
        <div class="max-w-7xl mx-auto">
            <div class="text-center mb-12">
                <h2 class="text-4xl mb-2 text-[#96744F]" style="font-family: 'Playfair Display', serif;">Our Categories</h2>
                <p class="text-[#96744F]/80">Explore our delightful selections</p>
            </div>
            
            <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-6">
                <asp:Repeater ID="rptCategories" runat="server">
                    <ItemTemplate>
                        <div class="category-card rounded-lg overflow-hidden cursor-pointer">
                            <div class="relative h-52">
                                <img src='<%# Eval("ImagePath") %>' alt='<%# Eval("CategoryName") %>' class="w-full h-full object-cover" />
                                <div class="category-overlay"></div>
                                <div class="absolute inset-0 flex flex-col items-center justify-center text-white z-10">
                                    <h3 class="text-xl font-semibold" style="font-family: 'Playfair Display', serif;"><%# Eval("CategoryName") %></h3>
                                    <p class="text-sm mt-1"><%# Eval("ItemCount") %> items</p>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </section>

    <!-- Best Seller Section -->
    <section class="py-16 px-4 bg-white">
        <div class="max-w-7xl mx-auto">
            <div class="text-center mb-12">
                <h2 class="text-4xl mb-2 text-[#96744F] font-bold" style="font-family: 'Playfair Display', serif;">Try Our Best Seller</h2>
                <p class="text-[#96744F]/80">Experience the perfect blend of flavor and sweetness</p>
            </div>
            
            <div class="flex flex-col lg:flex-row items-center justify-between bg-[#FDF7ED] rounded-lg overflow-hidden shadow-lg best-seller-container">
                <!-- Content Section -->
                <div class="w-full lg:w-3/5 p-8">
                    <h3 class="text-3xl font-bold text-[#96744F]" style="font-family: 'Playfair Display', serif;">Strawberry Shortcake</h3>
                    <div class="mt-2 flex">
                        <span class="text-yellow-400">★★★★★</span>
                        <span class="ml-2 text-sm text-gray-500">(124 reviews)</span>
                    </div>
                    <p class="mt-4 text-gray-700">
                        Experience the vibrant flavors of summer with our Strawberry Shortcake. We use only the freshest, locally sourced strawberries, carefully selected for their sweetness and ripeness.
                    </p>
                    <div class="mt-6">
                        <span class="text-2xl font-bold text-[#96744F]">₱1,099</span>
                        <span class="ml-2 text-gray-500">(10-inch round)</span>
                    </div>
                    <button onclick="viewProduct(0); return false;" class="mt-4 px-6 py-3 bg-[#96744F] text-white rounded-lg hover:bg-[#7d6142] transition duration-300 ease-in-out">
                        View Product
                    </button>
                </div>
                
                <!-- Image Section -->
                <div class="w-full lg:w-2/5 p-8 flex items-center justify-center">
                    <div class="relative w-64 h-64">
                        <img src="../Images/shortcake.png" alt="Strawberry Shortcake" class="w-full h-full object-contain rounded-lg shadow-lg" />
                        <div class="absolute -top-3 -right-3 bg-[#96744F] text-white rounded-full w-16 h-16 flex items-center justify-center transform rotate-12">
                            <span class="font-bold text-sm">TOP<br/>PICK</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- Featured Pastries Section -->
    <section class="py-16 px-4 bg-white">
        <div class="max-w-7xl mx-auto">
            <div class="flex justify-between items-center mb-12">
                <div>
                    <h2 class="text-4xl text-[#96744F]" style="font-family: 'Playfair Display', serif;">Featured Products</h2>
                    <p class="text-[#96744F]/80">Our best selling creations</p>
                </div>
                <a href="Menu.aspx" class="text-[#96744F] border border-[#96744F] px-6 py-2 rounded hover:bg-[#96744F] hover:text-white transition-all duration-300">
                    View All
                </a>
            </div>
            
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
                <asp:Repeater ID="rptFeaturedProducts" runat="server" DataSource='<%# GetTopSellingProducts() %>'>
                    <ItemTemplate>
                        <div class="product-card bg-white rounded-lg overflow-hidden shadow-sm">
                            <div class="h-52 overflow-hidden">
                                <%# GetProductImage(Eval("IMAGE"), Eval("ProductName").ToString()) %>
                            </div>
                            <div class="p-5">
                                <p class="text-xs font-medium text-gray-500 uppercase tracking-wider">TOP SELLER</p>
                                <h3 class="mt-1 text-xl font-semibold text-gray-900" style="font-family: 'Playfair Display', serif;"><%# Eval("ProductName") %></h3>
                                <p class="mt-2 text-gray-600 text-sm h-12 overflow-hidden"><%# Eval("DESCRIPTION") %></p>
                                <div class="mt-4 flex justify-between items-center">
                                    <span class="text-xl font-semibold text-gray-900">₱<%# Eval("PRICE", "{0:N2}") %></span>
                                    <button class="add-btn text-white rounded-lg px-3 py-2 flex items-center justify-center shadow-sm" onclick="viewProduct(<%# Eval("PRODUCTID") %>); return false;">
                                        View Product
                                    </button>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </section>

    <!-- Crafted with Love and Tradition Section -->
    <section class="py-16 px-4 bg-[#FDF7ED]">
        <div class="max-w-7xl mx-auto">
            <div class="flex flex-col lg:flex-row items-center gap-12">
                <!-- Image Section -->
                <div class="w-full lg:w-1/2 mb-8 lg:mb-0 overflow-hidden rounded-lg shadow-xl">
                    <img src="../Images/crafting.jpg" alt="Fresh pastries" class="w-full h-full object-cover transform hover:scale-105 transition-transform duration-700" style="min-height: 400px;" data-placeholder="Pastry Tray Image" />
                </div>
                
                <!-- Content Section -->
                <div class="w-full lg:w-1/2 craft-content">
                    <h2 class="text-4xl md:text-5xl font-bold mb-6 text-[#96744F]" style="font-family: 'Playfair Display', serif;">Crafted with Love and Tradition</h2>
                    
                    <p class="text-lg text-gray-700 mb-6">
                        At our pastry shop, we believe that every dessert tells a story. Our master bakers combine traditional techniques with innovative flavors to create memorable experiences for your palate.
                    </p>
                    
                    <p class="text-lg text-gray-700 mb-6">
                        Each morning, our kitchen comes alive with the aroma of freshly baked goods. We select only premium ingredients - from locally sourced fruits to imported Belgian chocolate - ensuring every bite delivers pure joy.
                    </p>
                    
                    <div class="flex flex-wrap gap-4 mt-8">
                        <div class="flex items-center bg-white rounded-full px-4 py-2 shadow-md">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-[#D43B6A] mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                            </svg>
                            <span class="text-[#96744F]">Artisanal Quality</span>
                        </div>
                        <div class="flex items-center bg-white rounded-full px-4 py-2 shadow-md">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-[#D43B6A] mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                            </svg>
                            <span class="text-[#96744F]">Premium Ingredients</span>
                        </div>
                        <div class="flex items-center bg-white rounded-full px-4 py-2 shadow-md">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-[#D43B6A] mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
                            </svg>
                            <span class="text-[#96744F]">Handcrafted Daily</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- Testimonials Section -->
    <section class="py-16 px-4 bg-white testimonials-section">
        <div class="max-w-7xl mx-auto">
            <div class="text-center mb-12 reveal-section">
                <h2 class="text-4xl mb-2 text-[#96744F]" style="font-family: 'Playfair Display', serif;">What Our Customers Say</h2>
                <p class="text-[#96744F]/80">Don't just take our word for it</p>
            </div>
            
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                <!-- Testimonial 1 -->
                <div class="bg-[#FDF7ED] p-6 rounded-lg shadow-md transform transition-all duration-300 hover:-translate-y-2 hover:shadow-xl reveal-item">
                    <div class="text-[#D43B6A] mb-4">
                        <span class="text-xl">★★★★★</span>
                    </div>
                    <p class="text-gray-700 mb-6 italic">
                        "The pastries here are absolutely divine! Every bite feels like a celebration. Their attention to detail and quality ingredients really sets them apart from other bakeries."
                    </p>
                    <div class="flex items-center">
                        <div class="w-12 h-12 bg-[#96744F]/20 rounded-full flex items-center justify-center">
                            <span class="text-[#96744F] font-bold">MC</span>
                        </div>
                        <div class="ml-4">
                            <h4 class="font-semibold text-[#96744F]">Maria Cruz</h4>
                            <p class="text-sm text-gray-500">Regular Customer</p>
                        </div>
                    </div>
                </div>
                
                <!-- Testimonial 2 -->
                <div class="bg-[#FDF7ED] p-6 rounded-lg shadow-md transform transition-all duration-300 hover:-translate-y-2 hover:shadow-xl reveal-item">
                    <div class="text-[#D43B6A] mb-4">
                        <span class="text-xl">★★★★★</span>
                    </div>
                    <p class="text-gray-700 mb-6 italic">
                        "I ordered a birthday cake for my daughter and it exceeded all expectations. Not only was it beautiful, but it tasted amazing! Everyone at the party was asking where I got it."
                    </p>
                    <div class="flex items-center">
                        <div class="w-12 h-12 bg-[#96744F]/20 rounded-full flex items-center justify-center">
                            <span class="text-[#96744F] font-bold">JR</span>
                        </div>
                        <div class="ml-4">
                            <h4 class="font-semibold text-[#96744F]">Juan Reyes</h4>
                            <p class="text-sm text-gray-500">Verified Purchase</p>
                        </div>
                    </div>
                </div>
                
                <!-- Testimonial 3 -->
                <div class="bg-[#FDF7ED] p-6 rounded-lg shadow-md transform transition-all duration-300 hover:-translate-y-2 hover:shadow-xl reveal-item">
                    <div class="text-[#D43B6A] mb-4">
                        <span class="text-xl">★★★★★</span>
                    </div>
                    <p class="text-gray-700 mb-6 italic">
                        "Their bread is honestly the best I've had in years. I can tell they use traditional methods and high-quality ingredients. I'm now a regular customer and can't imagine going anywhere else!"
                    </p>
                    <div class="flex items-center">
                        <div class="w-12 h-12 bg-[#96744F]/20 rounded-full flex items-center justify-center">
                            <span class="text-[#96744F] font-bold">AS</span>
                        </div>
                        <div class="ml-4">
                            <h4 class="font-semibold text-[#96744F]">Ana Santos</h4>
                            <p class="text-sm text-gray-500">Food Blogger</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script>
        // Animation on scroll functions
        document.addEventListener('DOMContentLoaded', function() {
            // Initialize elements with hidden classes
            initAnimationStates();
            
            // Initial check for elements in viewport
            checkVisibility();
            
            // Add scroll event listener to check visibility continuously
            window.addEventListener('scroll', function() {
                checkVisibility();
            });
        });
        
        function initAnimationStates() {
            // Add hidden state classes to category cards
            const categoryCards = document.querySelectorAll('.category-card');
            categoryCards.forEach(card => {
                card.classList.add('hidden-left');
            });
            
            // Add hidden state to testimonials
            const testimonials = document.querySelectorAll('.reveal-item');
            testimonials.forEach(item => {
                item.classList.add('hidden-up');
            });
            
            // Add hidden state to best seller container
            const bestSeller = document.querySelector('.best-seller-container');
            if (bestSeller) {
                bestSeller.classList.add('hidden-blur');
            }
            
            // Add hidden state to craft content
            const craftContent = document.querySelector('.craft-content');
            if (craftContent) {
                craftContent.classList.add('hidden-right');
            }
        }
        
        function checkVisibility() {
            // Check if category cards are in viewport (at 10vh threshold)
            const categoryCards = document.querySelectorAll('.category-card');
            categoryCards.forEach((card, index) => {
                if (isElementInViewport(card, 0.1)) {
                    // Add animation with delay based on index
                    setTimeout(() => {
                        card.classList.remove('hidden-left');
                        card.classList.add('animate-fadeInLeft');
                    }, index * 100);
                } else if (!isElementInViewport(card, 1.0)) {
                    // Reset animation when completely out of viewport
                    card.classList.remove('animate-fadeInLeft');
                    card.classList.add('hidden-left');
                }
            });
            
            // Check if testimonials are in viewport
            const testimonials = document.querySelectorAll('.reveal-item');
            testimonials.forEach((item, index) => {
                if (isElementInViewport(item, 0.2)) {
                    // Add animation with delay based on index
                    setTimeout(() => {
                        item.classList.remove('hidden-up');
                        item.classList.add('animate-fadeInUp');
                    }, index * 150);
                } else if (!isElementInViewport(item, 1.0)) {
                    // Reset animation when completely out of viewport
                    item.classList.remove('animate-fadeInUp');
                    item.classList.add('hidden-up');
                }
            });
            
            // Check if best seller is in viewport
            const bestSeller = document.querySelector('.best-seller-container');
            if (bestSeller) {
                if (isElementInViewport(bestSeller, 0.1)) {
                    bestSeller.classList.remove('hidden-blur');
                    bestSeller.classList.add('animate-fadeInBlur');
                } else if (!isElementInViewport(bestSeller, 1.0)) {
                    // Reset animation when completely out of viewport
                    bestSeller.classList.remove('animate-fadeInBlur');
                    bestSeller.classList.add('hidden-blur');
                }
            }
            
            // Check if craft content is in viewport
            const craftContent = document.querySelector('.craft-content');
            if (craftContent) {
                if (isElementInViewport(craftContent, 0.2)) {
                    craftContent.classList.remove('hidden-right');
                    craftContent.classList.add('animate-fadeInRight');
                } else if (!isElementInViewport(craftContent, 1.0)) {
                    // Reset animation when completely out of viewport
                    craftContent.classList.remove('animate-fadeInRight');
                    craftContent.classList.add('hidden-right');
                }
            }
        }
        
        function isElementInViewport(el, threshold = 0) {
            const rect = el.getBoundingClientRect();
            const windowHeight = window.innerHeight || document.documentElement.clientHeight;
            
            // Check if element is within the threshold of viewport
            const isVisible = (
                rect.top <= windowHeight * (1 - threshold) &&
                rect.bottom >= windowHeight * threshold
            );
            
            return isVisible;
        }
    </script>
</asp:Content>