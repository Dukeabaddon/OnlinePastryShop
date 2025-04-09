<%@ Page Title="About Us" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="OnlinePastryShop.Pages.About" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Hero section zoom effect */
        .hero-zoom-bg {
            transition: transform 10s ease-out;
            transform: scale(1);
        }
        
        .hero-zoom-active .hero-zoom-bg {
            transform: scale(1.15);
        }
        
        /* Custom styles */
        .section-heading {
            font-family: 'Playfair Display', serif;
            color: #96744F;
        }
        
        .section-badge {
            font-size: 12px;
            letter-spacing: 2px;
            color: #96744F;
            opacity: 0.8;
        }
        
        .value-card {
            transition: all 0.5s ease;
        }
        
        .value-card:hover {
            transform: translateY(-10px);
            box-shadow: 0 10px 20px rgba(0,0,0,0.1);
        }
        
        /* Team section styles */
        .team-section {
            background-color: #fff;
            padding: 4rem 0;
        }
        
        .team-container {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 3rem;
            max-width: 1200px;
            margin: 0 auto;
            padding: 0 1rem;
        }
        
        .team-member {
            display: flex;
            flex-direction: column;
            align-items: center;
            text-align: center;
        }
        
        .team-photo-wrapper {
            position: relative;
            width: 180px;
            height: 180px;
            margin-bottom: 2rem;
        }
        
        .team-photo {
            width: 100%;
            height: 100%;
            border-radius: 50%;
            object-fit: cover;
            border: 5px solid white;
            box-shadow: 0 5px 15px rgba(0,0,0,0.1);
        }
        
        .team-badge {
            position: absolute;
            bottom: -1rem;
            left: 50%;
            transform: translateX(-50%);
            background-color: #ffbf00;
            color: #4a4a4a;
            font-weight: 500;
            padding: 0.5rem 1rem;
            border-radius: 100px;
            white-space: nowrap;
            font-size: 0.875rem;
        }
        
        .team-name {
            font-family: 'Playfair Display', serif;
            color: #96744F;
            font-size: 1.5rem;
            margin-bottom: 0.25rem;
        }
        
        .team-title {
            color: #96744F;
            font-weight: 500;
            margin-bottom: 1rem;
        }
        
        .team-description {
            color: #6b7280;
            line-height: 1.6;
            max-width: 85%;
            margin: 0 auto;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Hero Section -->
    <section class="relative overflow-hidden" style="height: 500px;">
        <!-- Background Image with Zoom Effect -->
        <div class="hero-zoom-bg absolute inset-0 bg-cover bg-center" style="background-image: url('../Images/hero-section.jpg');"></div>
        
        <!-- Overlay -->
        <div class="absolute inset-0 bg-black opacity-60"></div>
        
        <!-- Content -->
        <div class="relative z-10 h-full flex flex-col justify-center items-center text-center px-4">
            <h1 class="text-4xl md:text-6xl text-white font-bold mb-4" style="font-family: 'Playfair Display', serif;">Our Filipino Heritage</h1>
            <p class="text-xl text-white max-w-3xl">A journey of passion, tradition, and Filipino baking excellence for more than 25 years</p>
            
            <!-- Stats -->
            <div class="flex flex-wrap justify-center gap-12 mt-12">
                <div class="text-center">
                    <div class="text-4xl font-bold text-white">8,000+</div>
                    <div class="text-white opacity-80">Happy Customers</div>
                </div>
                <div class="text-center">
                    <div class="text-4xl font-bold text-white">25</div>
                    <div class="text-white opacity-80">Years of Experience</div>
                </div>
                <div class="text-center">
                    <div class="text-4xl font-bold text-white">45</div>
                    <div class="text-white opacity-80">Award-winning Recipes</div>
                </div>
            </div>
        </div>
    </section>
    
    <!-- Heritage Section -->
    <section class="py-20 px-4">
        <div class="max-w-7xl mx-auto">
            <div class="text-center mb-12">
                <div class="uppercase section-badge tracking-wider mb-2">OUR STORY</div>
                <h2 class="text-4xl font-bold section-heading mb-0">A Taste of Philippine Heritage</h2>
            </div>
            
            <div class="flex flex-col md:flex-row items-center gap-8 md:gap-16">
                <!-- Image -->
                <div class="w-full md:w-5/12 rounded-lg overflow-hidden shadow-xl">
                    <img src="../Images/hero-section.jpg" alt="Bakery interior with traditional Filipino pastries" class="w-full h-full object-cover" style="min-height: 400px;">
                </div>
                
                <!-- Content -->
                <div class="w-full md:w-7/12">
                    <p class="text-gray-700 mb-6">
                        Pastry Palace began as a small family bakery in Manila back in 1997. Our founder Maria Santos inherited the art of pastry making from her grandmother who carried the flavors of Spanish-Filipino fusion baking techniques.
                    </p>
                    
                    <p class="text-gray-700 mb-6">
                        What began as a neighborhood panaderya has grown into a beloved establishment, where we create pastries and desserts with a distinctly Filipino twist. Each creation combines traditional techniques with delicious Filipino flavors like ube, pandan, and calamansi.
                    </p>
                    
                    <p class="text-gray-700">
                        Through economic challenges and changing times, our commitment to quality and authenticity has remained unwavering. Today Pastry Palace continues to serve as a cornerstone of Filipino baking excellence.
                    </p>
                </div>
            </div>
        </div>
    </section>
    
    <!-- Core Values Section -->
    <section class="py-16 px-4 bg-[#FDF7ED]">
        <div class="max-w-7xl mx-auto">
            <div class="text-center mb-12">
                <div class="uppercase section-badge tracking-wider mb-2">OUR CORE VALUES</div>
                <h2 class="text-4xl font-bold section-heading mb-0">The Heart of Our Bakery</h2>
                <p class="mt-4 text-gray-600 max-w-3xl mx-auto">
                    These principles have guided our bakery from our humble beginnings in Manila to our multiple locations across the Philippines today.
                </p>
            </div>
            
            <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
                <!-- Value 1 -->
                <div class="bg-white rounded-lg p-8 shadow-md value-card">
                    <div class="flex justify-center mb-6">
                        <div class="w-16 h-16 bg-[#FDF7ED] rounded-full flex items-center justify-center">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 text-[#96744F]" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                            </svg>
                        </div>
                    </div>
                    <h3 class="text-xl font-bold text-center section-heading mb-3">Passion for Quality</h3>
                    <p class="text-gray-600 text-center">
                        We use only premium local Filipino ingredients, from coconut cream to native chocolate, ensuring every bite transports you to the flavors of home.
                    </p>
                </div>
                
                <!-- Value 2 -->
                <div class="bg-white rounded-lg p-8 shadow-md value-card">
                    <div class="flex justify-center mb-6">
                        <div class="w-16 h-16 bg-[#FDF7ED] rounded-full flex items-center justify-center">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 text-[#96744F]" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                            </svg>
                        </div>
                    </div>
                    <h3 class="text-xl font-bold text-center section-heading mb-3">Family Heritage</h3>
                    <p class="text-gray-600 text-center">
                        We preserve traditional Filipino baking methods, while also embracing innovation to create pastries that bridge the past with the present.
                    </p>
                </div>
                
                <!-- Value 3 -->
                <div class="bg-white rounded-lg p-8 shadow-md value-card">
                    <div class="flex justify-center mb-6">
                        <div class="w-16 h-16 bg-[#FDF7ED] rounded-full flex items-center justify-center">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 text-[#96744F]" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
                            </svg>
                        </div>
                    </div>
                    <h3 class="text-xl font-bold text-center section-heading mb-3">Community First</h3>
                    <p class="text-gray-600 text-center">
                        We actively support local farmers, participate in community events, and provide baking workshops to preserve Filipino culinary traditions.
                    </p>
                </div>
            </div>
        </div>
    </section>
    
    <!-- Team Section -->
    <section class="team-section py-20">
        <div class="max-w-7xl mx-auto px-4">
            <div class="text-center mb-16">
                <h2 class="text-4xl font-bold section-heading mb-4">The Masters Behind Our Creations</h2>
                <p class="text-gray-600 max-w-3xl mx-auto">
                    Meet the talented people who combine Filipino tradition with modern techniques to create our delicious pastries.
                </p>
            </div>
            
            <div class="team-container">
                <!-- Team Member 1 -->
                <div class="team-member">
                    <div class="team-photo-wrapper">
                        <img src="https://images.unsplash.com/photo-1607631568561-e5b6ff274815?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=80" alt="Maria Santos" class="team-photo">
                        <div class="team-badge">Founder</div>
                    </div>
                    <h3 class="team-name">Maria Santos</h3>
                    <p class="team-title">Head Pastry Chef</p>
                    <p class="team-description">
                        A culinary arts graduate from the University of the Philippines who specialized in traditional Filipino desserts.
                    </p>
                </div>
                
                <!-- Team Member 2 -->
                <div class="team-member">
                    <div class="team-photo-wrapper">
                        <img src="https://images.unsplash.com/photo-1577219491135-ce391730fb2c?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=80" alt="David Reyes" class="team-photo">
                        <div class="team-badge">Executive Chef</div>
                    </div>
                    <h3 class="team-name">David Reyes</h3>
                    <p class="team-title">Executive Baker</p>
                    <p class="team-description">
                        Trained in France but specializes in incorporating local ingredients from Bicol and Pampanga regions.
                    </p>
                </div>
                
                <!-- Team Member 3 -->
                <div class="team-member">
                    <div class="team-photo-wrapper">
                        <img src="https://images.unsplash.com/photo-1580489944761-15a19d654956?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=80" alt="Sarah Mendoza" class="team-photo">
                        <div class="team-badge">Creative Lead</div>
                    </div>
                    <h3 class="team-name">Sarah Mendoza</h3>
                    <p class="team-title">Head Decorator</p>
                    <p class="team-description">
                        Known for incorporating traditional Filipino art patterns into her cake decorations and pastry designs.
                    </p>
                </div>
            </div>
        </div>
    </section>

    <!-- CTA Section -->
    <section class="py-20 px-4 bg-cover bg-center relative" style="background-image: url('../Images/hero-section.jpg');">
        <!-- Overlay -->
        <div class="absolute inset-0 bg-[#96744F] opacity-90"></div>
        
        <!-- Content -->
        <div class="relative z-10 max-w-4xl mx-auto text-center">
            <h2 class="text-4xl font-bold text-white mb-6" style="font-family: 'Playfair Display', serif;">Experience Filipino Pastry Excellence</h2>
            <p class="text-white text-lg mb-8 max-w-3xl mx-auto">
                From traditional pandesal to innovative ube-cheese ensaymada, taste why we're the Philippines' favorite bakery for over two decades.
            </p>
            <div class="flex flex-wrap gap-4 justify-center">
                <a href="/Menu" class="px-8 py-3 bg-yellow-400 text-gray-900 font-semibold rounded-lg hover:bg-yellow-300 transition duration-300">Explore Our Menu</a>
                <a href="#" class="px-8 py-3 bg-transparent border-2 border-white text-white font-semibold rounded-lg hover:bg-white hover:text-[#96744F] transition duration-300">Contact Us</a>
            </div>
        </div>
    </section>
    
    <!-- Flagship Store Section -->
    <section class="py-16 px-4 bg-[#FDF7ED]">
        <div class="max-w-7xl mx-auto">
            <div class="text-center mb-12">
                <div class="uppercase section-badge tracking-wider mb-2">OUR LOCATIONS</div>
                <h2 class="text-4xl font-bold section-heading mb-0">Visit Our Flagship Store</h2>
                <p class="mt-4 text-gray-600 max-w-3xl mx-auto">
                    We're centrally located in Metro Manila, serving our delicious Filipino pastries to local customers.
                </p>
            </div>
            
            <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
                <!-- Map -->
                <div class="h-80 lg:h-auto lg:min-h-[400px] rounded-lg overflow-hidden">
                    <img src="../Images/hero-section.jpg" alt="Store location" class="w-full h-full object-cover">
                </div>
                
                <!-- Store Info -->
                <div class="bg-white rounded-lg p-8 shadow-md">
                    <div class="mb-6">
                        <h3 class="font-bold text-xl section-heading">Service Area Notice</h3>
                        <p class="text-gray-600 mt-2">
                            We are currently only accepting orders within Metro Manila. We're working hard to expand our delivery services outside the metro soon. Thank you for your patience.
                        </p>
                    </div>
                    
                    <div class="mb-6">
                        <h3 class="font-bold text-xl section-heading flex items-center">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                            </svg>
                            Manila Flagship
                        </h3>
                        <p class="text-gray-600 mt-2">
                            123 Sampaloc Street, Intramuros<br>
                            Manila, Metro Manila<br>
                            Philippines<br>
                            <span class="text-sm text-gray-500 mt-1">San Lorenzo Shopping Center</span>
                        </p>
                    </div>
                    
                    <div class="mb-6">
                        <h3 class="font-bold text-xl section-heading flex items-center">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                            Opening Hours
                        </h3>
                        <p class="text-gray-600 mt-2">
                            Monday - Friday: 7:00 AM - 8:00 PM<br>
                            Saturday: 8:00 AM - 5:00 PM<br>
                            Sunday: 8:00 AM - 4:00 PM<br>
                            Holiday Hours: <a href="#" class="text-[#96744F] underline">Special Schedule</a>
                        </p>
                    </div>
                    
                    <div class="text-center mt-8">
                        <a href="#" class="inline-block px-6 py-3 bg-[#96744F] text-white rounded-lg hover:bg-[#7d6142] transition duration-300 ease-in-out">Contact Us</a>
                    </div>
                </div>
            </div>
        </div>
    </section>
    
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsContent" runat="server">
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            // Start hero zoom animation immediately
            document.querySelector('.hero-zoom-bg').classList.add('hero-zoom-active');
        });
    </script>
</asp:Content>
