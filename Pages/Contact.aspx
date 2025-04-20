<%@ Page Title="Contact Us - Royal Pastries" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="OnlinePastryShop.Pages.Contact" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Animation keyframes */
        @keyframes fadeIn {
            from { opacity: 0; }
            to { opacity: 1; }
        }
        
        @keyframes slideInUp {
            from { 
                opacity: 0;
                transform: translateY(50px);
            }
            to { 
                opacity: 1;
                transform: translateY(0);
            }
        }
        
        @keyframes slideInLeft {
            from { 
                opacity: 0;
                transform: translateX(-50px);
            }
            to { 
                opacity: 1;
                transform: translateX(0);
            }
        }
        
        @keyframes slideInRight {
            from { 
                opacity: 0;
                transform: translateX(50px);
            }
            to { 
                opacity: 1;
                transform: translateX(0);
            }
        }
        
        @keyframes zoomIn {
            from {
                opacity: 0;
                transform: scale(0.9);
            }
            to {
                opacity: 1;
                transform: scale(1);
            }
        }
        
        /* Scroll animation classes */
        .animate-on-scroll {
            opacity: 0;
        }
        
        .animate-fade-in {
            animation: fadeIn 1s ease forwards;
        }
        
        .animate-slide-up {
            animation: slideInUp 1s ease forwards;
        }
        
        .animate-slide-left {
            animation: slideInLeft 1s ease forwards;
        }
        
        .animate-slide-right {
            animation: slideInRight 1s ease forwards;
        }
        
        .animate-zoom-in {
            animation: zoomIn 1s ease forwards;
        }
        
        /* Animation delays */
        .delay-100 { animation-delay: 100ms; }
        .delay-200 { animation-delay: 200ms; }
        .delay-300 { animation-delay: 300ms; }
        .delay-400 { animation-delay: 400ms; }
        .delay-500 { animation-delay: 500ms; }
        .delay-600 { animation-delay: 600ms; }
        .delay-700 { animation-delay: 700ms; }
        .delay-800 { animation-delay: 800ms; }
        
        /* Custom styles */
        .hero-overlay {
            background-color: rgba(150, 116, 79, 0.7);
        }
        
        .contact-card {
            transition: all 0.3s ease;
            overflow: hidden;
        }
        
        .contact-card:hover {
            transform: translateY(-10px);
            box-shadow: 0 15px 30px rgba(0, 0, 0, 0.1);
        }
        
        .contact-card::before {
            content: "";
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 0;
            background: linear-gradient(to bottom, rgba(150, 116, 79, 0.1), transparent);
            transition: height 0.5s ease;
        }
        
        .contact-card:hover::before {
            height: 100%;
        }
        
        .contact-icon {
            transition: transform 0.3s ease;
        }
        
        .contact-card:hover .contact-icon {
            transform: scale(1.2);
        }
        
        .input-wrapper {
            position: relative;
            overflow: hidden;
        }
        
        .fancy-input {
            transition: all 0.3s ease;
            border: 2px solid transparent;
            background: #f9f9f9;
        }
        
        .fancy-input:focus {
            border-color: #96744F;
            box-shadow: 0 5px 15px rgba(150, 116, 79, 0.1);
            background: #fff;
        }
        
        .fancy-label {
            position: absolute;
            left: 16px;
            top: 14px;
            color: #6b7280;
            transition: all 0.3s ease;
            pointer-events: none;
            font-size: 14px;
        }
        
        .fancy-input:focus + .fancy-label,
        .fancy-input:not(:placeholder-shown) + .fancy-label {
            top: -10px;
            left: 10px;
            font-size: 12px;
            background: #fff;
            padding: 0 8px;
            color: #96744F;
            font-weight: 500;
        }
        
        .scroll-indicator {
            animation: bounce 2s infinite;
        }
        
        @keyframes bounce {
            0%, 20%, 50%, 80%, 100% {
                transform: translateY(0) translateX(-50%);
            }
            40% {
                transform: translateY(-10px) translateX(-50%);
            }
            60% {
                transform: translateY(-5px) translateX(-50%);
            }
        }
        
        .faq-item {
            border-radius: 10px;
            overflow: hidden;
            transition: all 0.3s ease;
        }
        
        .faq-item:hover {
            box-shadow: 0 10px 25px rgba(0, 0, 0, 0.05);
        }
        
        .faq-toggle {
            transition: all 0.3s ease;
        }
        
        .faq-toggle-icon {
            font-size: 20px;
            display: inline-block;
            transition: transform 0.3s ease;
            color: #96744F;
            margin-left: 8px;
            text-align: center;
            width: 24px;
            height: 24px;
        }
        
        .faq-toggle[aria-expanded="true"] .faq-toggle-icon {
            transform: rotate(180deg);
        }
        
        .faq-content {
            max-height: 0;
            overflow: hidden;
            transition: max-height 0.5s ease;
        }
        
        .faq-content.active {
            max-height: 1000px;
        }
        
        .checkbox-custom {
            position: relative;
            cursor: pointer;
        }
        
        input[type="checkbox"]:checked + .checkbox-custom {
            background-color: #96744F;
            border-color: #96744F;
        }
        
        input[type="checkbox"]:checked + .checkbox-custom svg {
            display: block;
        }
        
        .checkbox-custom svg {
            display: none;
        }
    </style>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main>
        <!-- Hero Section -->
        <section class="relative h-[90vh] overflow-hidden">
            <!-- Background Image -->
            <div class="absolute inset-0 bg-cover bg-center" style="background-image: url('https://source.unsplash.com/random/1600x900/?bakery,pastry'); z-index: -10;"></div>
            
            <!-- Overlay -->
            <div class="absolute inset-0 hero-overlay z-0"></div>
            
            <!-- Decorative Elements -->
            <div class="absolute top-[20%] left-[10%] w-20 h-20 bg-white rounded-full opacity-10 animate-pulse"></div>
            <div class="absolute bottom-[30%] right-[15%] w-32 h-32 bg-white rounded-full opacity-10 animate-pulse" style="animation-delay: 1s;"></div>
            <div class="absolute top-[40%] right-[20%] w-16 h-16 bg-white rounded-full opacity-10 animate-pulse" style="animation-delay: 0.5s;"></div>
            
            <!-- Content -->
            <div class="relative h-full container mx-auto px-6 z-10 flex flex-col items-center justify-center text-center">
                <h1 class="text-5xl md:text-7xl font-bold text-white mb-6 animate-slide-up" style="font-family: 'Playfair Display', serif;">Connect With Us</h1>
                <p class="text-xl text-white max-w-3xl animate-slide-up delay-200">We'd love to hear from you! Whether you have questions about our delicious Filipino pastries or want to place a special order.</p>
                
                <a href="#contact-section" class="mt-12 text-white border-2 border-white rounded-full px-8 py-3 hover:bg-white hover:text-[#96744F] transition-all duration-200 animate-slide-up delay-400">
                    Get In Touch
                </a>
                
                <!-- Scroll Indicator -->
                <div class="absolute bottom-10 left-1/2 transform -translate-x-1/2 text-white scroll-indicator">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-10 w-10" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 14l-7 7m0 0l-7-7m7 7V3" />
                    </svg>
                </div>
            </div>
        </section>

        <!-- Contact Information Section -->
        <section class="py-16 bg-[#F9F5F0]">
            <div class="container mx-auto px-4">
                <h2 class="text-4xl font-bold text-center text-[#96744F] mb-12" style="font-family: 'Playfair Display', serif;">Connect with Royal Pastries</h2>
                
                <div class="flex flex-wrap justify-center gap-6 mb-12">
                    <div class="p-6 bg-white rounded-xl shadow-lg max-w-sm w-full">
                        <div class="text-center mb-4">
                            <div class="inline-block p-3 rounded-full bg-[#F9F5F0]">
                                <i class="fas fa-map-marker-alt text-3xl text-[#96744F]"></i>
                            </div>
                        </div>
                        <h3 class="text-xl font-bold mb-2 text-center text-[#96744F]">Visit Us</h3>
                        <p class="text-[#96744F] font-bold mb-1 text-center">Royal Pastries Flagship Store</p>
                        <p class="text-gray-600 text-center">123 Delicious Street</p>
                        <p class="text-gray-600 text-center">Makati City, Metro Manila</p>
                        <p class="text-gray-600 text-center">Philippines</p>
                    </div>
                    
                    <div class="p-6 bg-white rounded-xl shadow-lg max-w-sm w-full">
                        <div class="text-center mb-4">
                            <div class="inline-block p-3 rounded-full bg-[#F9F5F0]">
                                <i class="fas fa-phone-alt text-3xl text-[#96744F]"></i>
                            </div>
                        </div>
                        <h3 class="text-xl font-bold mb-2 text-center text-[#96744F]">Call Us</h3>
                        <p class="text-gray-600 text-center">Customer Service:</p>
                        <p class="text-[#96744F] font-bold mb-2 text-center">+63 (2) 8123 4567</p>
                        <p class="text-gray-600 text-center">Orders & Inquiries:</p>
                        <p class="text-[#96744F] font-bold text-center">+63 (2) 8765 4321</p>
                    </div>
                    
                    <div class="p-6 bg-white rounded-xl shadow-lg max-w-sm w-full">
                        <div class="text-center mb-4">
                            <div class="inline-block p-3 rounded-full bg-[#F9F5F0]">
                                <i class="fas fa-envelope text-3xl text-[#96744F]"></i>
                            </div>
                        </div>
                        <h3 class="text-xl font-bold mb-2 text-center text-[#96744F]">Email Us</h3>
                        <p class="text-gray-600 text-center">Customer Support:</p>
                        <p class="text-[#96744F] font-bold mb-2 text-center">info@royalpastries.com</p>
                        <p class="text-gray-600 text-center">Bulk Orders:</p>
                        <p class="text-[#96744F] font-bold text-center">orders@royalpastries.com</p>
                    </div>
                </div>
                
                <div class="max-w-4xl mx-auto text-center">
                    <h3 class="text-2xl font-bold text-[#96744F] mb-4">Business Hours</h3>
                    <div class="flex justify-center space-x-8 flex-wrap">
                        <div class="mb-4">
                            <p class="font-bold text-[#96744F] mb-2">Weekdays</p>
                            <p class="text-gray-600">7:00 AM - 9:00 PM</p>
                        </div>
                        <div class="mb-4">
                            <p class="font-bold text-[#96744F] mb-2">Weekends</p>
                            <p class="text-gray-600">8:00 AM - 10:00 PM</p>
                        </div>
                        <div class="mb-4">
                            <p class="font-bold text-[#96744F] mb-2">Holidays</p>
                            <p class="text-gray-600">9:00 AM - 8:00 PM</p>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <!-- Contact Form Section -->
        <section id="contact-section" class="py-16 bg-white">
            <div class="container mx-auto px-4">
                <div class="max-w-5xl mx-auto flex flex-col lg:flex-row bg-white rounded-xl overflow-hidden shadow-lg">
                    <div class="lg:w-1/2 bg-[#96744F] p-10">
                        <h3 class="text-2xl font-bold text-white mb-6" style="font-family: 'Playfair Display', serif;">Get in Touch</h3>
                        <p class="text-white mb-6">
                            We value your feedback and are always here to help with any questions or special requests you might have.
                        </p>
                        
                        <div class="px-8 py-10">
                            <h4 class="mb-2 font-medium text-[#F9F5F0]">About Royal Pastries</h4>
                            <p class="text-white">
                                Royal Pastries brings joy through our delicious pastries and cakes made with premium ingredients and traditional baking techniques.
                            </p>
                        </div>
                        
                        <div class="mt-8">
                            <h4 class="mb-4 font-medium text-[#F9F5F0]">Follow Us</h4>
                            <div class="flex space-x-4">
                                <a href="#" class="text-white hover:text-[#D9C5B2] transition-colors">
                                    <i class="fab fa-facebook-f text-xl"></i>
                                </a>
                                <a href="#" class="text-white hover:text-[#D9C5B2] transition-colors">
                                    <i class="fab fa-instagram text-xl"></i>
                                </a>
                                <a href="#" class="text-white hover:text-[#D9C5B2] transition-colors">
                                    <i class="fab fa-twitter text-xl"></i>
                                </a>
                            </div>
                        </div>
                    </div>
                    
                    <div class="lg:w-1/2 p-10">
                        <h3 class="text-2xl font-bold text-[#96744F] mb-6" style="font-family: 'Playfair Display', serif;">Send a Message</h3>
                        <div class="space-y-4">
                            <div>
                                <label for="name" class="block mb-2 text-[#96744F]">Your Name</label>
                                <input type="text" id="name" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#96744F]" runat="server" />
                            </div>
                            <div>
                                <label for="email" class="block mb-2 text-[#96744F]">Email Address</label>
                                <input type="email" id="email" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#96744F]" runat="server" />
                            </div>
                            <div>
                                <label for="subject" class="block mb-2 text-[#96744F]">Subject</label>
                                <input type="text" id="subject" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#96744F]" runat="server" />
                            </div>
                            <div>
                                <label for="message" class="block mb-2 text-[#96744F]">Message</label>
                                <textarea id="message" rows="4" class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#96744F]" runat="server"></textarea>
                            </div>
                            <div>
                                <asp:Button ID="btnSubmit" Text="Send Message" CssClass="w-full bg-[#96744F] text-white font-bold py-3 px-4 rounded-lg hover:bg-[#7D5F3F] transition-colors duration-300" runat="server" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <!-- Map Section -->
        <section class="py-16 bg-[#F9F5F0]">
            <div class="container mx-auto px-4">
                <h2 class="text-3xl font-bold text-center text-[#96744F] mb-8" style="font-family: 'Playfair Display', serif;">Our Location</h2>
                <div class="max-w-5xl mx-auto">
                    <div class="rounded-xl overflow-hidden shadow-lg">
                        <!-- Replace with actual map embed code -->
                        <div class="w-full h-[400px] bg-gray-200 flex items-center justify-center">
                            <p class="text-gray-600">Google Maps Embed Will Be Placed Here</p>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <!-- FAQs Section -->
        <div class="container mx-auto my-20 px-4">
            <div id="faqSection" class="max-w-4xl mx-auto mb-16">
                <h2 class="text-3xl font-bold text-center text-[#96744F] mb-10" style="font-family: 'Playfair Display', serif;">Frequently Asked Questions</h2>
                <div class="space-y-4">
                    <div class="border-b border-gray-200 pb-4">
                        <button class="flex justify-between items-center w-full text-left font-medium text-[#96744F] focus:outline-none">
                            <span>What areas do Royal Pastries deliver to?</span>
                            <i class="fas fa-plus"></i>
                        </button>
                        <div class="mt-2 hidden">
                            <p class="text-gray-600">Currently, we deliver to all areas within Metro Manila. For areas outside Metro Manila, please contact our customer service for availability and additional delivery fees.</p>
                        </div>
                    </div>
                    <div class="border-b border-gray-200 pb-4">
                        <button class="flex justify-between items-center w-full text-left font-medium text-[#96744F] focus:outline-none">
                            <span>How can I place special orders with Royal Pastries?</span>
                            <i class="fas fa-plus"></i>
                        </button>
                        <div class="mt-2 hidden">
                            <p class="text-gray-600">For special orders, please call us at +63 (2) 8765 4321 or email us at orders@royalpastries.com at least 48 hours in advance. For larger events or custom designs, we recommend 5-7 days notice.</p>
                        </div>
                    </div>
                    <div class="border-b border-gray-200 pb-4">
                        <button class="flex justify-between items-center w-full text-left font-medium text-[#96744F] focus:outline-none">
                            <span>Does Royal Pastries offer any loyalty programs?</span>
                            <i class="fas fa-plus"></i>
                        </button>
                        <div class="mt-2 hidden">
                            <p class="text-gray-600">Yes, we have a Sweet Rewards program where you earn points for every purchase. These points can be redeemed for discounts, free pastries, or special offers. Ask about it in store or register online.</p>
                        </div>
                    </div>
                    <div class="border-b border-gray-200 pb-4">
                        <button class="flex justify-between items-center w-full text-left font-medium text-[#96744F] focus:outline-none">
                            <span>Do you offer dietary-specific options?</span>
                            <i class="fas fa-plus"></i>
                        </button>
                        <div class="mt-2 hidden">
                            <p class="text-gray-600">Yes, we offer gluten-free, sugar-free, and vegan options for selected items. Please call us in advance to inquire about availability or to place custom orders with specific dietary requirements.</p>
                        </div>
                    </div>
                    <div class="border-b border-gray-200 pb-4">
                        <button class="flex justify-between items-center w-full text-left font-medium text-[#96744F] focus:outline-none">
                            <span>Can I request a custom cake design?</span>
                            <i class="fas fa-plus"></i>
                        </button>
                        <div class="mt-2 hidden">
                            <p class="text-gray-600">Absolutely! We specialize in custom cake designs. Please contact us with your ideas at least one week in advance. Our pastry artists will work with you to create a unique cake for your special occasion.</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
    
    <script>
        // FAQ Toggle Functionality
        document.addEventListener('DOMContentLoaded', function () {
            const faqButtons = document.querySelectorAll('#faqSection button');
            
            faqButtons.forEach(button => {
                button.addEventListener('click', function() {
                    const content = this.nextElementSibling;
                    const icon = this.querySelector('i');
                    
                    // Toggle content visibility
                    content.classList.toggle('hidden');
                    
                    // Toggle icon
                    if (content.classList.contains('hidden')) {
                        icon.classList.remove('fa-minus');
                        icon.classList.add('fa-plus');
                    } else {
                        icon.classList.remove('fa-plus');
                        icon.classList.add('fa-minus');
                    }
                });
            });
        });
    </script>
</asp:Content>
