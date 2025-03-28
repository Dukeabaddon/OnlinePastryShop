<%@ Page Title="" Language="C#" MasterPageFile="~/Pages/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="OnlinePastryShop.Pages.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Add any additional styles or scripts here -->
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Hero Section -->
    <section class="w-full h-screen bg-[url('../Images/landingpage-bg.png')] bg-cover bg-no-repeat bg-bottom">
        <div class="flex mx-32 gap-28 items-center justify-center h-full pb-32">
            <div class="flex flex-col">
                <h3 class="text-6xl text-[#D43B6A]">Indulge in Pure Pastry</h3>
                <h1 class="text-9xl text-[#D43B6A]">Perfection</h1>
                <p class="text-base font-quicksand text-[#D43B6A] break-words text-wrap mt-7">
                    Where every bite is a masterpiece. We're proud to be the best-selling
                    cake destination, offering freshly baked pastries and bespoke cakes crafted to your exact specifications.
                </p>
                <button class="font-semibold text-white w-48 rounded-4xl px-10 py-4 bg-[#D43B6A] mt-16">Order Now</button>
            </div>
            <div>
                <img class="w-3xl" src="../Images/cupcake.png" alt="cupcake">
            </div>
        </div>
    </section>

    <!-- Categories Section -->
    <section class="py-16 px-4">
        <h1 class="text-4xl font-bold text-[#D43B6A] text-center mb-12">EXPLORE OUR PRODUCTS</h1>
        <div class="relative flex items-center justify-center">
            <button onclick="scrollLeftward()" class="absolute left-10 z-10">
                <img src="../Images/left arrow.png" alt="arrow" class="w-8">
            </button>
            <div class="flex items-center mx-24 w-[80vw] overflow-hidden gap-40 snap-x snap-mandatory scrollbar-hide">
                <div class="flex flex-col items-center justify-center snap-center">
                    <div class="w-24 h-24 rounded-full flex items-center justify-center">
                        <img src="../Images/new.png" alt="latest product" class="w-16 h-16 object-contain">
                    </div>
                    <p class="-mt-2 text-gray-700">Latest</p>
                </div>
                <div class="flex flex-col items-center justify-center snap-center">
                    <div class="w-24 h-24 rounded-full flex items-center justify-center">
                        <img src="../Images/cake.png" alt="cake" class="w-16 h-16 object-contain">
                    </div>
                    <p class="-mt-2 text-gray-700">Cakes</p>
                </div>
                <div class="flex flex-col items-center justify-center snap-center">
                    <div class="w-24 h-24 rounded-full flex items-center justify-center">
                        <img src="../Images/capcakes.png" alt="cupcake" class="w-16 h-16 object-contain">
                    </div>
                    <p class="-mt-2 text-gray-700">Cupcakes</p>
                </div>
                <div class="flex flex-col items-center justify-center snap-center">
                    <div class="w-24 h-24 rounded-full flex items-center justify-center">
                        <img src="../Images/macaroons.png" alt="macaroon" class="w-16 h-16 object-contain">
                    </div>
                    <p class="-mt-2 text-gray-700">Macaroons</p>
                </div>
                <div class="flex flex-col items-center justify-center snap-center">
                    <div class="w-24 h-24 rounded-full flex items-center justify-center">
                        <img src="../Images/pastries.png" alt="pastries" class="w-16 h-16 object-contain">
                    </div>
                    <p class="-mt-2 text-gray-700">Pastries</p>
                </div>
                <div class="flex flex-col items-center justify-center snap-center">
                    <div class="w-24 h-24 rounded-full flex items-center justify-center">
                        <img src="../Images/donuts.png" alt="donuts" class="w-16 h-16 object-contain">
                    </div>
                    <p class="-mt-2 text-gray-700">Donuts</p>
                </div>
                <div class="flex flex-col items-center justify-center snap-center">
                    <div class="w-24 h-24 rounded-full flex items-center justify-center">
                        <img src="../Images/breads.png" alt="breads" class="w-16 h-16 object-contain">
                    </div>
                    <p class="-mt-2 text-gray-700">Breads</p>
                </div>
                <div class="flex flex-col items-center justify-center snap-center">
                    <div class="w-24 h-24 rounded-full flex items-center justify-center">
                        <img src="../Images/croissant.png" alt="croissant" class="w-16 h-16 object-contain">
                    </div>
                    <p class="-mt-2 text-gray-700">Croissants</p>
                </div>
            </div>
            <button onclick="scrollRightward()" class="absolute right-10 z-10">
                <img src="../Images/right arrow.png" alt="arrow" class="w-8">
            </button>
        </div>
    </section>

    <!-- Best Seller Section -->
    <section class="relative w-full h-[130vh] bg-[url('../Images/bestseller-bg.png')] bg-cover bg-no-repeat bg-top">
        <div class="absolute inset-0 bg-[url('../Images/bestseller.png')] bg-cover bg-no-repeat"></div>
        <div class="relative z-10 flex flex-col justify-center h-full ml-60 max-w-2xl -mt-14">
            <h2 class="text-6xl font-bold text-[#D43B6A] mb-8">Try Our Best Seller</h2>
            <div class="relative rounded-3xl">
                <div class="absolute inset-0 bg-[url('../Images/bestseller-container.png')] bg-cover bg-no-repeat rounded-3xl"></div>
                <div class="relative z-10 p-8">
                    <h4 class="text-4xl font-semibold text-[#D43B6A] mb-4">Strawberry Shortcake</h4>
                    <p class="text-lg text-gray-700 mb-6">Experience the vibrant flavors of summer with our Strawberry Shortcake. We use only the freshest, locally sourced strawberries, carefully selected for their sweetness and ripeness.</p>
                    <h1 class="text-3xl font-bold text-[#D43B6A]">₱1,099 <span class="text-xl font-normal">(10-inch round)</span></h1>
                </div>
            </div>
        </div>
    </section>

    <!-- Sweet Collection Section -->
    <section class="px-20 py-16 -mt-24 mb-28 z-10">
        <h2 class="text-4xl font-bold text-[#D43B6A] text-center mb-12">Our Sweet Collection</h2>
        <div class="grid grid-cols-4 gap-8">
            <div class="w-80 h-96 flex flex-col shadow-[1px_1px_3px_rgba(107,107,107,0.10)] bg-white rounded-lg border border-[#E5E7EB]">
                <img src="../Images/cupcakes-placeholder.png" alt="cupcake" class="w-full h-72 object-cover rounded-t-lg">
                <div class="flex flex-col p-4">
                    <h4 class="text-black text-xl font-normal leading-5">Cupcakes</h4>
                    <p class="text-[#4B5563] text-base font-normal leading-4 mt-10">₱75</p>
                </div>
            </div>
            <div class="w-80 h-96 flex flex-col shadow-[1px_1px_3px_rgba(107,107,107,0.10)] bg-white rounded-lg border border-[#E5E7EB]">
                <img src="../Images/croissant-placeholder.png" alt="croissant" class="w-full h-72 object-cover rounded-t-lg">
                <div class="flex flex-col p-4">
                    <h4 class="text-black text-xl font-normal leading-5">Croissants</h4>
                    <p class="text-[#4B5563] text-base font-normal leading-4 mt-10">₱75</p>
                </div>
            </div>
            <div class="w-80 h-96 flex flex-col shadow-[1px_1px_3px_rgba(107,107,107,0.10)] bg-white rounded-lg border border-[#E5E7EB]">
                <img src="../Images/macaroons-placeholder.png" alt="macaroons" class="w-full h-72 object-cover rounded-t-lg">
                <div class="flex flex-col p-4">
                    <h4 class="text-black text-xl font-normal leading-5">Macaroons</h4>
                    <p class="text-[#4B5563] text-base font-normal leading-4 mt-10">₱75</p>
                </div>
            </div>
            <div class="w-80 h-96 flex flex-col shadow-[1px_1px_3px_rgba(107,107,107,0.10)] bg-white rounded-lg border border-[#E5E7EB]">
                <img src="../Images/donut-placeholder.png" alt="donuts" class="w-full h-72 object-cover rounded-t-lg">
                <div class="flex flex-col p-4">
                    <h4 class="text-black text-xl font-normal leading-5">Donuts</h4>
                    <p class="text-[#4B5563] text-base font-normal leading-4 mt-10">₱75</p>
                </div>
            </div>
        </div>
    </section>
</asp:Content>