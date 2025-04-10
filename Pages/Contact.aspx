<%@ Page Title="Contact Us" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="OnlinePastryShop.Pages.Contact" %>

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
            background: linear-gradient(to right, rgba(150, 116, 79, 0.95), rgba(150, 116, 79, 0.7));
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
                transform: translateY(0);
            }
            40% {
                transform: translateY(-20px);
            }
            60% {
                transform: translateY(-10px);
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

    <!-- Contact Methods Section -->
    <section id="contact-methods" class="py-24 bg-[#FBF7F1]">
        <div class="container mx-auto px-6">
            <!-- Section Title -->
            <div class="text-center max-w-3xl mx-auto mb-16 animate-on-scroll">
                <span class="inline-block px-3 py-1 bg-[#D9C5B2] text-[#96744F] text-sm font-semibold rounded-full mb-3">REACH OUT</span>
                <h2 class="text-4xl font-bold mb-4 text-[#96744F]" style="font-family: 'Playfair Display', serif;">How Can We Help You?</h2>
                <p class="text-gray-600">We're here to answer any questions you have about our products, services, or anything else.</p>
            </div>
            
            <!-- Contact Cards -->
            <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
                <!-- Phone Card -->
                <div class="contact-card relative bg-white rounded-xl p-8 shadow-lg animate-on-scroll">
                    <div class="absolute top-0 right-0 w-24 h-24 bg-[#FBF7F1] rounded-bl-[100px] -mr-2 -mt-2 z-0"></div>
                    <div class="relative z-10">
                        <div class="w-16 h-16 bg-[#D9C5B2] rounded-full flex items-center justify-center mb-6 mx-auto">
                            <i class="fas fa-phone-alt text-2xl text-[#96744F] contact-icon"></i>
                        </div>
                        <h3 class="text-xl font-bold mb-2 text-center text-[#96744F]">Call Us</h3>
                        <p class="text-[#96744F] font-bold mb-1 text-center">+63 (2) 8123-4567</p>
                        <p class="text-gray-500 text-sm text-center">Available Mon-Sat, 9AM-6PM</p>
                        <div class="mt-6 text-center">
                            <a href="tel:+6328123-4567" class="inline-flex items-center text-[#96744F] hover:underline">
                                <span>Call now</span>
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 ml-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                                </svg>
                            </a>
                        </div>
                    </div>
                </div>
                
                <!-- Address Card -->
                <div class="contact-card relative bg-white rounded-xl p-8 shadow-lg animate-on-scroll">
                    <div class="absolute top-0 right-0 w-24 h-24 bg-[#FBF7F1] rounded-bl-[100px] -mr-2 -mt-2 z-0"></div>
                    <div class="relative z-10">
                        <div class="w-16 h-16 bg-[#D9C5B2] rounded-full flex items-center justify-center mb-6 mx-auto">
                            <i class="fas fa-map-marker-alt text-2xl text-[#96744F] contact-icon"></i>
                        </div>
                        <h3 class="text-xl font-bold mb-2 text-center text-[#96744F]">Visit Us</h3>
                        <p class="text-[#96744F] font-bold mb-1 text-center">49 Quirino Hwy, Novaliches</p>
                        <p class="text-gray-500 text-sm text-center">Quezon City, Metro Manila</p>
                        <div class="mt-6 text-center">
                            <a href="#map-section" class="inline-flex items-center text-[#96744F] hover:underline">
                                <span>View on map</span>
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 ml-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                                </svg>
                            </a>
                        </div>
                    </div>
                </div>
                
                <!-- Email Card -->
                <div class="contact-card relative bg-white rounded-xl p-8 shadow-lg animate-on-scroll">
                    <div class="absolute top-0 right-0 w-24 h-24 bg-[#FBF7F1] rounded-bl-[100px] -mr-2 -mt-2 z-0"></div>
                    <div class="relative z-10">
                        <div class="w-16 h-16 bg-[#D9C5B2] rounded-full flex items-center justify-center mb-6 mx-auto">
                            <i class="fas fa-envelope text-2xl text-[#96744F] contact-icon"></i>
                        </div>
                        <h3 class="text-xl font-bold mb-2 text-center text-[#96744F]">Email Us</h3>
                        <p class="text-[#96744F] font-bold mb-1 text-center">info@pastrypalace.ph</p>
                        <p class="text-gray-500 text-sm text-center">We reply within 24 hours</p>
                        <div class="mt-6 text-center">
                            <a href="mailto:info@pastrypalace.ph" class="inline-flex items-center text-[#96744F] hover:underline">
                                <span>Send email</span>
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 ml-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                                </svg>
                            </a>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- Contact Form Section -->
    <section id="contact-section" class="py-24 relative">
        <!-- Background Elements -->
        <div class="absolute left-0 top-0 w-32 h-32 bg-[#FBF7F1] rounded-br-full"></div>
        <div class="absolute right-0 bottom-0 w-32 h-32 bg-[#FBF7F1] rounded-tl-full"></div>
        
        <div class="container mx-auto px-6 relative z-10">
            <!-- Section Title -->
            <div class="text-center max-w-3xl mx-auto mb-16 animate-on-scroll">
                <span class="inline-block px-3 py-1 bg-[#D9C5B2] text-[#96744F] text-sm font-semibold rounded-full mb-3">GET IN TOUCH</span>
                <h2 class="text-4xl font-bold mb-4 text-[#96744F]" style="font-family: 'Playfair Display', serif;">Send Us a Message</h2>
                <p class="text-gray-600">Have a question or feedback? We'd love to hear from you.</p>
            </div>
            
            <!-- Form Container -->
            <div class="max-w-4xl mx-auto">
                <div class="bg-white rounded-2xl shadow-xl overflow-hidden">
                    <div class="grid grid-cols-1 lg:grid-cols-5">
                        <!-- Form Left Decorative Column -->
                        <div class="hidden lg:block lg:col-span-2 bg-[#96744F] relative">
                            <div class="absolute inset-0 bg-opacity-20 bg-[url('https://source.unsplash.com/random/600x900/?pastry,bakery')] bg-cover bg-center"></div>
                            <div class="relative z-10 p-12 h-full flex flex-col justify-center">
                                <h3 class="text-2xl font-bold text-white mb-6">We're waiting to hear from you</h3>
                                <p class="text-white/80 mb-8">Your feedback helps us improve our pastries and service. Let us know what you think!</p>
                                
                                <div class="mt-auto">
                                    <div class="flex items-center mb-6">
                                        <div class="w-10 h-10 rounded-full bg-white/20 flex items-center justify-center mr-4">
                                            <i class="fas fa-clock text-white"></i>
                                        </div>
                                        <div>
                                            <p class="text-white/80 text-sm">Response Time</p>
                                            <p class="text-white">Within 24 hours</p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        <!-- Form Content -->
                        <div class="p-12 lg:col-span-3 animate-on-scroll">
                            <!-- Name Field -->
                            <div class="mb-6 input-wrapper">
                                <asp:TextBox ID="txtName" runat="server" CssClass="fancy-input w-full px-4 py-3 rounded-lg" placeholder=" "></asp:TextBox>
                                <span class="fancy-label">Your Name</span>
                            </div>
                            
                            <!-- Email and Phone Field Row -->
                            <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
                                <div class="input-wrapper">
                                    <asp:TextBox ID="txtEmail" runat="server" CssClass="fancy-input w-full px-4 py-3 rounded-lg" placeholder=" " TextMode="Email"></asp:TextBox>
                                    <span class="fancy-label">Email Address</span>
                                </div>
                                <div class="input-wrapper">
                                    <asp:TextBox ID="txtPhone" runat="server" CssClass="fancy-input w-full px-4 py-3 rounded-lg" placeholder=" "></asp:TextBox>
                                    <span class="fancy-label">Phone (Optional)</span>
                                </div>
                            </div>
                            
                            <!-- Subject Field -->
                            <div class="mb-6 input-wrapper">
                                <asp:TextBox ID="txtSubject" runat="server" CssClass="fancy-input w-full px-4 py-3 rounded-lg" placeholder=" "></asp:TextBox>
                                <span class="fancy-label">Subject</span>
                            </div>
                            
                            <!-- Message Field -->
                            <div class="mb-6 input-wrapper">
                                <asp:TextBox ID="txtMessage" runat="server" CssClass="fancy-input w-full px-4 py-3 rounded-lg" placeholder=" " TextMode="MultiLine" Rows="4"></asp:TextBox>
                                <span class="fancy-label">Your Message</span>
                            </div>
                            
                            <!-- Submit Button -->
                            <div class="text-center">
                                <asp:Button ID="btnSendMessage" runat="server" Text="Send Message" CssClass="bg-[#96744F] hover:bg-[#7d6142] text-white font-semibold px-8 py-3 rounded-lg transition-all duration-300 transform hover:scale-105 hover:shadow-lg" OnClick="btnSendMessage_Click" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- Our Bakery Section -->
    <div class="container mx-auto my-20 px-4">
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
            <div class="rounded-lg overflow-hidden h-full">
                <img src="/Assets/Images/bakery-storefront.jpg" alt="Pastry Palace Storefront" class="w-full h-full object-cover">
            </div>
            <div class="mt-8 lg:mt-0">
                <h2 class="text-3xl font-semibold mb-4 text-[#96744F]">Our Bakery</h2>
                <p class="text-gray-700">Welcome to Pastry Palace - your home for authentic Filipino pastries made with love and tradition. Our flagship store in Novaliches, Quezon City offers a warm, inviting space where you can enjoy freshly baked goods daily.</p>
                
                <div class="mt-6">
                    <h5 class="flex items-center text-lg font-bold text-[#96744F]"><i class="fas fa-clock mr-2 text-[#96744F]"></i> Opening Hours</h5>
                    <ul class="list-none ml-4 mt-2">
                        <li class="mb-1"><span class="font-semibold">Monday - Friday:</span> <span class="text-[#96744F]">7:00 AM - 8:00 PM</span></li>
                        <li class="mb-1"><span class="font-semibold">Saturday - Sunday:</span> <span class="text-[#96744F]">8:00 AM - 7:00 PM</span></li>
                        <li><span class="font-semibold">Special Hours on Holidays</span></li>
                    </ul>
                </div>
                
                <div class="mt-6">
                    <h5 class="flex items-center text-lg font-bold text-[#96744F]"><i class="fas fa-utensils mr-2 text-[#96744F]"></i> Special Services</h5>
                    <ul class="list-none ml-4 mt-2">
                        <li class="mb-1"><span class="font-semibold">Custom Cake Orders</span> - Create your dream cake</li>
                        <li class="mb-1"><span class="font-semibold">Event Catering</span> - Perfect for celebrations</li>
                        <li class="mb-1"><span class="font-semibold">Baking Classes</span> - Learn Filipino pastry techniques</li>
                        <li><span class="font-semibold">Corporate Orders</span> - For meetings and events</li>
                    </ul>
                </div>
            </div>
        </div>
    </div>

    <!-- Map Section -->
    <div id="map-section" class="bg-[#FBF7F1] py-10">
        <div class="container mx-auto px-4">
            <h2 class="text-3xl font-semibold text-center mb-8">Find Us in Quezon City</h2>
            <p class="text-center mb-6">We're conveniently located in Novaliches, right in front of Quezon City University main gate. Come visit us and experience our freshly-baked Filipino pastries!</p>
            
            <div class="relative rounded-lg overflow-hidden shadow-md">
                <div class="grid grid-cols-1 lg:grid-cols-4">
                    <div class="lg:col-span-3 h-[400px]">
                        <iframe src="https://www.google.com/maps/embed?pb=!1m14!1m12!1m3!1d1929.6162674121183!2d121.03340130904716!3d14.699437804363171!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!5e0!3m2!1sen!2sph!4v1744242789632!5m2!1sen!2sph" 
                                width="100%" 
                                height="100%" 
                                style="border:0;" 
                                allowfullscreen="" 
                                loading="lazy" 
                                referrerpolicy="no-referrer-when-downgrade">
                        </iframe>
                    </div>
                    <div class="lg:col-span-1 bg-white p-6">
                        <div class="mb-6">
                            <h3 class="text-xl font-semibold mb-2">Pastry Palace Quezon City</h3>
                            <p class="text-gray-600 mb-1">49 Quirino Hwy, Novaliches,</p>
                            <p class="text-gray-600 mb-3">Quezon City, Metro Manila, Philippines</p>
                            <a href="https://maps.google.com/?q=49+Quirino+Hwy+Novaliches+Quezon+City+Metro+Manila" class="text-[#96744F] flex items-center hover:underline" target="_blank">
                                <i class="fas fa-directions mr-1"></i> Get Directions
                            </a>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- FAQs Section -->
    <div class="container mx-auto my-20 px-4">
        <h2 class="text-4xl font-bold text-center mb-8 text-[#96744F]" style="font-family: 'Playfair Display', serif;">Frequently Asked Questions</h2>
        <p class="text-center mb-8">Find answers to the most common questions about our services and products</p>
        
        <div class="max-w-4xl mx-auto">
            <div class="space-y-4">
                <div class="border border-gray-200 rounded-lg overflow-hidden faq-item">
                    <button class="flex justify-between items-center w-full py-4 px-6 text-left bg-white hover:bg-gray-50 transition-colors duration-300 focus:outline-none faq-toggle" type="button" aria-expanded="false" aria-controls="faq1Content">
                        <div class="flex items-center">
                            <span class="text-[#D9C5B2] mr-3"><i class="far fa-comment-dots"></i></span>
                            <span class="font-medium text-[#96744F]">Do you deliver outside Metro Manila?</span>
                        </div>
                        <i class="fa fa-angle-down faq-toggle-icon text-[#96744F]"></i>
                    </button>
                    <div id="faq1Content" class="faq-content px-6 py-0 bg-white border-t border-gray-200">
                        <div class="py-4">
                            <p>Currently, we only accept orders within Metro Manila. We're working hard to expand our delivery services outside the metro area soon.</p>
                            <p class="text-[#96744F] mt-2">Please check back in a few months as we plan to serve nearby provinces by 2025!</p>
                        </div>
                    </div>
                </div>
                
                <div class="border border-gray-200 rounded-lg overflow-hidden faq-item">
                    <button class="flex justify-between items-center w-full py-4 px-6 text-left bg-white hover:bg-gray-50 transition-colors duration-300 focus:outline-none faq-toggle" type="button" aria-expanded="false" aria-controls="faq2Content">
                        <div class="flex items-center">
                            <span class="text-[#D9C5B2] mr-3"><i class="far fa-calendar-alt"></i></span>
                            <span class="font-medium text-[#96744F]">How far in advance should I place special orders?</span>
                        </div>
                        <i class="fa fa-angle-down faq-toggle-icon text-[#96744F]"></i>
                    </button>
                    <div id="faq2Content" class="faq-content px-6 py-0 bg-white border-t border-gray-200">
                        <div class="py-4">
                            <p>For special orders, we recommend placing them at least 48 hours in advance. For larger orders or custom designs, 5-7 days advance notice is preferred to ensure we can accommodate your request.</p>
                        </div>
                    </div>
                </div>
                
                <div class="border border-gray-200 rounded-lg overflow-hidden faq-item">
                    <button class="flex justify-between items-center w-full py-4 px-6 text-left bg-white hover:bg-gray-50 transition-colors duration-300 focus:outline-none faq-toggle" type="button" aria-expanded="false" aria-controls="faq3Content">
                        <div class="flex items-center">
                            <span class="text-[#D9C5B2] mr-3"><i class="fas fa-leaf"></i></span>
                            <span class="font-medium text-[#96744F]">Do you offer special dietary options?</span>
                        </div>
                        <i class="fa fa-angle-down faq-toggle-icon text-[#96744F]"></i>
                    </button>
                    <div id="faq3Content" class="faq-content px-6 py-0 bg-white border-t border-gray-200">
                        <div class="py-4">
                            <p>Yes, we offer gluten-free, sugar-free, and vegan options for selected items. Please call us in advance to inquire about availability or to place custom orders with specific dietary requirements.</p>
                        </div>
                    </div>
                </div>
                
                <div class="border border-gray-200 rounded-lg overflow-hidden faq-item">
                    <button class="flex justify-between items-center w-full py-4 px-6 text-left bg-white hover:bg-gray-50 transition-colors duration-300 focus:outline-none faq-toggle" type="button" aria-expanded="false" aria-controls="faq4Content">
                        <div class="flex items-center">
                            <span class="text-[#D9C5B2] mr-3"><i class="fas fa-map-marker-alt"></i></span>
                            <span class="font-medium text-[#96744F]">Can I request Filipino regional specialties?</span>
                        </div>
                        <i class="fa fa-angle-down faq-toggle-icon text-[#96744F]"></i>
                    </button>
                    <div id="faq4Content" class="faq-content px-6 py-0 bg-white border-t border-gray-200">
                        <div class="py-4">
                            <p>Absolutely! We specialize in pastries from different regions of the Philippines. If you have a specific regional specialty in mind, please place your order at least 3-5 days in advance so we can prepare it authentically.</p>
                        </div>
                    </div>
                </div>
                
                <div class="border border-gray-200 rounded-lg overflow-hidden faq-item">
                    <button class="flex justify-between items-center w-full py-4 px-6 text-left bg-white hover:bg-gray-50 transition-colors duration-300 focus:outline-none faq-toggle" type="button" aria-expanded="false" aria-controls="faq5Content">
                        <div class="flex items-center">
                            <span class="text-[#D9C5B2] mr-3"><i class="fas fa-gift"></i></span>
                            <span class="font-medium text-[#96744F]">Do you offer gift packaging?</span>
                        </div>
                        <i class="fa fa-angle-down faq-toggle-icon text-[#96744F]"></i>
                    </button>
                    <div id="faq5Content" class="faq-content px-6 py-0 bg-white border-t border-gray-200">
                        <div class="py-4">
                            <p>Yes, we offer elegant gift packaging options for all our products. From simple ribbon wrapping to custom gift boxes, we can make your pastry gifts look as special as they taste. Additional charges may apply depending on the packaging requested.</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Toast Notification -->
    <div class="fixed top-5 right-5 bg-white rounded shadow-lg z-50 hidden" role="alert" aria-live="assertive" aria-atomic="true" id="messageToast">
        <div class="flex items-center justify-between border-b border-gray-200 px-4 py-2">
            <strong class="text-gray-800">Pastry Palace</strong>
            <button type="button" class="text-gray-400 hover:text-gray-600 focus:outline-none" data-bs-dismiss="toast" aria-label="Close">
                <span aria-hidden="true">&times;</span>
            </button>
        </div>
        <div class="px-4 py-3" id="toastMessage">
            Your message has been sent successfully!
        </div>
    </div>

    <!-- Font Awesome -->
    <script src="https://kit.fontawesome.com/a076d05399.js" crossorigin="anonymous"></script>

    <!-- Updated Font Awesome CDN (more reliable) -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css" integrity="sha512-1ycn6IcaQQ40/MKBW2W4Rhis/DbILU74C1vSrLJxCq57o941Ym01SwNsOMqvEBFlcgUa6xLiPY/NS5R+E6ztJQ==" crossorigin="anonymous" referrerpolicy="no-referrer" />
    
    <!-- Animation and Interaction Script -->
    <script>
        // Animation on scroll
        document.addEventListener('DOMContentLoaded', function() {
            // Intersection Observer for scroll animations
            const observerOptions = {
                threshold: 0.1,
                rootMargin: '0px 0px -100px 0px'
            };
            
            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('animate-fade-in');
                        
                        // Add additional animation classes based on data attributes
                        if (entry.target.dataset.animation === 'slide-up') {
                            entry.target.classList.add('animate-slide-up');
                        } else if (entry.target.dataset.animation === 'slide-left') {
                            entry.target.classList.add('animate-slide-left');
                        } else if (entry.target.dataset.animation === 'slide-right') {
                            entry.target.classList.add('animate-slide-right');
                        } else if (entry.target.dataset.animation === 'zoom-in') {
                            entry.target.classList.add('animate-zoom-in');
                        }
                        
                        // Unobserve after animation is triggered
                        observer.unobserve(entry.target);
                    }
                });
            }, observerOptions);
            
            // Observe all elements with animate-on-scroll class
            document.querySelectorAll('.animate-on-scroll').forEach(element => {
                // Set random animation type if not specified
                if (!element.dataset.animation) {
                    const animations = ['fade-in', 'slide-up', 'slide-left', 'slide-right', 'zoom-in'];
                    const randomAnimation = animations[Math.floor(Math.random() * animations.length)];
                    element.dataset.animation = randomAnimation;
                }
                
                // Set random delay if not specified
                if (!element.style.animationDelay) {
                    const randomDelay = Math.floor(Math.random() * 5) * 100;
                    element.style.animationDelay = `${randomDelay}ms`;
                }
                
                observer.observe(element);
            });
            
            // Initialize FAQ toggles
            initFAQs();
        });
        
        // Enhanced FAQ toggle functionality with smooth animation
        function initFAQs() {
            document.querySelectorAll('.faq-toggle').forEach(toggle => {
                toggle.addEventListener('click', function() {
                    const contentId = this.getAttribute('aria-controls');
                    const content = document.getElementById(contentId);
                    const isExpanded = this.getAttribute('aria-expanded') === 'true';
                    
                    // Toggle aria-expanded attribute
                    this.setAttribute('aria-expanded', !isExpanded);
                    
                    // Get all FAQ toggles and contents
                    const allToggles = document.querySelectorAll('.faq-toggle');
                    const allContents = document.querySelectorAll('.faq-content');
                    
                    // Close all other FAQs
                    allToggles.forEach(otherToggle => {
                        if (otherToggle !== this) {
                            otherToggle.setAttribute('aria-expanded', 'false');
                        }
                    });
                    
                    allContents.forEach(otherContent => {
                        if (otherContent.id !== contentId) {
                            otherContent.style.maxHeight = null;
                        }
                    });
                    
                    // Toggle the content height
                    if (!isExpanded) {
                        // Open this FAQ
                        content.style.maxHeight = content.scrollHeight + 'px';
                    } else {
                        // Close this FAQ
                        content.style.maxHeight = null;
                    }
                });
            });
        }
        
        // Smooth scroll for anchor links
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', function(e) {
                e.preventDefault();
                
                const targetId = this.getAttribute('href');
                const targetElement = document.querySelector(targetId);
                
                if (targetElement) {
                    window.scrollTo({
                        top: targetElement.offsetTop - 100,
                        behavior: 'smooth'
                    });
                }
            });
        });
        
        // Custom checkbox functionality
        document.addEventListener('DOMContentLoaded', function() {
            const checkboxes = document.querySelectorAll('input[type="checkbox"]');
            checkboxes.forEach(checkbox => {
                checkbox.addEventListener('change', function() {
                    const customCheckbox = this.nextElementSibling;
                    if (this.checked) {
                        customCheckbox.classList.add('bg-[#96744F]');
                        customCheckbox.classList.add('border-[#96744F]');
                    } else {
                        customCheckbox.classList.remove('bg-[#96744F]');
                        customCheckbox.classList.remove('border-[#96744F]');
                    }
                });
            });
        });

        // Show toast notification function
        function showToast(message) {
            const toast = document.getElementById('messageToast');
            const toastMessage = document.getElementById('toastMessage');
            
            if (toast && toastMessage) {
                toastMessage.textContent = message;
                toast.classList.remove('hidden');
                
                // Auto-hide after 5 seconds
                setTimeout(() => {
                    toast.classList.add('hidden');
                }, 5000);
                
                // Close button functionality
                const closeButton = toast.querySelector('[data-bs-dismiss="toast"]');
                if (closeButton) {
                    closeButton.addEventListener('click', () => {
                        toast.classList.add('hidden');
                    });
                }
            }
        }
    </script>
</asp:Content>
