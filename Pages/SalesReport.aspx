<%@ Page Title="Sales Report" 
         Language="C#" 
         MasterPageFile="~/Pages/AdminMaster.Master" 
         AutoEventWireup="true" 
         CodeBehind="SalesReport.aspx.cs" 
         Inherits="OnlinePastryShop.Pages.SalesReport" %>

<asp:Content ID="Content1" ContentPlaceHolderID="AdminContent" runat="server">
    <form runat="server">
    <div class="container mx-auto p-4">
        <!-- Error Message -->
        <asp:Label ID="lblErrorMessage" runat="server" 
                   CssClass="text-red-500 text-center mb-4" 
                   Visible="false"></asp:Label>

        <asp:Label ID="lblMessage" runat="server" Visible="false" CssClass="text-red-500"></asp:Label>
        
        <!-- Dashboard Header with Time Selector -->
        <div class="flex flex-col md:flex-row justify-between items-center mb-6">
            <h1 class="text-2xl font-bold text-[#D43B6A] mb-4 md:mb-0">Pastry Shop Dashboard</h1>
            
            <div class="flex flex-wrap gap-3 items-center">
                <!-- Time Period Selector -->
                <div class="relative">
                    <asp:DropDownList ID="TimeRangeSelector" runat="server" CssClass="pl-3 pr-10 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#D43B6A] focus:border-[#D43B6A]" AutoPostBack="true" OnSelectedIndexChanged="TimeRangeSelector_SelectedIndexChanged">
                        <asp:ListItem Value="daily" Text="Daily"></asp:ListItem>
                        <asp:ListItem Value="weekly" Text="Weekly" Selected="True"></asp:ListItem>
                        <asp:ListItem Value="monthly" Text="Monthly"></asp:ListItem>
                        <asp:ListItem Value="yearly" Text="Yearly"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                
                <!-- Date Range Picker -->
                <div class="flex items-center space-x-4 mb-4">
                    <div>
                        <label for="txtStartDate" class="block text-sm font-medium text-gray-700">Start Date</label>
                        <asp:TextBox ID="txtStartDate" runat="server" CssClass="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500" Type="date"></asp:TextBox>
                    </div>
                    <div>
                        <label for="txtEndDate" class="block text-sm font-medium text-gray-700">End Date</label>
                        <asp:TextBox ID="txtEndDate" runat="server" CssClass="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500" Type="date"></asp:TextBox>
                    </div>
                    <div class="self-end">
                        <asp:Button ID="btnFilterReport" runat="server" Text="Apply Filter" OnClick="btnFilterReport_Click" CssClass="inline-flex justify-center py-2 px-4 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500" />
                    </div>
                </div>
            </div>
        </div>

        <!-- KPI Cards (expanded with more relevant metrics) -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
            <div class="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow duration-300 cursor-pointer">
                <div class="flex items-center justify-between">
                    <div>
                        <p class="text-gray-500 text-sm font-medium mb-1">Period Revenue</p>
                        <h2 class="text-2xl font-bold text-gray-800"><%# TotalRevenue %></h2>
                        <div class="mt-1 inline-flex items-center px-2 py-1 rounded-full text-xs <%# Convert.ToDecimal(RevenueGrowth) >= 0 ? "bg-green-100 text-green-800" : "bg-red-100 text-red-800" %>">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-3 w-3 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="<%# Convert.ToDecimal(RevenueGrowth) >= 0 ? "M5 10l7-7m0 0l7 7m-7-7v18" : "M19 14l-7 7m0 0l-7-7m7 7V3" %>" />
                            </svg>
                            <%# Math.Abs(Convert.ToDecimal(RevenueGrowth)).ToString("0.0") %>% vs previous
                        </div>
                    </div>
                    <div class="p-3 bg-blue-50 rounded-full">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 text-blue-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                    </div>
                </div>
            </div>
            
            <div class="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow duration-300 cursor-pointer">
                <div class="flex items-center justify-between">
                    <div>
                        <p class="text-gray-500 text-sm font-medium mb-1">Total Orders</p>
                        <h2 class="text-2xl font-bold text-gray-800"><%# TotalSales %></h2>
                        <div class="mt-1 inline-flex items-center px-2 py-1 rounded-full text-xs <%# Convert.ToDecimal(SalesGrowth) >= 0 ? "bg-green-100 text-green-800" : "bg-red-100 text-red-800" %>">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-3 w-3 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="<%# Convert.ToDecimal(SalesGrowth) >= 0 ? "M5 10l7-7m0 0l7 7m-7-7v18" : "M19 14l-7 7m0 0l-7-7m7 7V3" %>" />
                            </svg>
                            <%# Math.Abs(Convert.ToDecimal(SalesGrowth)).ToString("0.0") %>% vs previous
                        </div>
                    </div>
                    <div class="p-3 bg-green-50 rounded-full">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
                        </svg>
                    </div>
                </div>
            </div>
            
            <div class="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow duration-300 cursor-pointer">
                <div class="flex items-center justify-between">
                    <div>
                        <p class="text-gray-500 text-sm font-medium mb-1">Avg Order Value</p>
                        <h2 class="text-2xl font-bold text-gray-800"><%# AverageOrderValue %></h2>
                        <div class="mt-1 inline-flex items-center px-2 py-1 rounded-full text-xs <%# Convert.ToDecimal(AvgOrderValueChange) >= 0 ? "bg-green-100 text-green-800" : "bg-red-100 text-red-800" %>">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-3 w-3 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="<%# AvgOrderValueIcon %>" />
                            </svg>
                            <%# Math.Abs(Convert.ToDecimal(AvgOrderValueChange)).ToString("0.0") %>% vs previous
                        </div>
                    </div>
                    <div class="p-3 bg-purple-50 rounded-full">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 text-purple-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 14h.01M12 14h.01M15 11h.01M12 11h.01M9 11h.01M7 21h10a2 2 0 002-2V5a2 2 0 00-2-2H7a2 2 0 00-2 2v14a2 2 0 002 2z" />
                        </svg>
                    </div>
                </div>
            </div>
        </div>

        <!-- Main Charts Section - 2-column Grid -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
            <!-- Sales Trend Chart (Line) -->
            <div class="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow duration-300">
                <h3 class="text-lg font-semibold text-gray-800 mb-4 flex justify-between items-center">
                    <span>Sales Trend</span>
                    <span class="text-sm text-gray-500"><%# ChartPeriodText %></span>
                </h3>
                <canvas id="salesTrendChart" height="250"></canvas>
            </div>

            <!-- Revenue by Category (Pie) -->
            <div class="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow duration-300">
                <h3 class="text-lg font-semibold text-gray-800 mb-4">Revenue by Category</h3>
                <canvas id="categoryRevenueChart" height="250"></canvas>
            </div>

            <!-- Top Selling Products (Bar) -->
            <div class="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow duration-300">
                <h3 class="text-lg font-semibold text-gray-800 mb-4">Top Selling Products</h3>
                <canvas id="topProductsChart" height="250"></canvas>
            </div>

            <!-- Stock Status Overview (Doughnut) -->
            <div class="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow duration-300">
                <h3 class="text-lg font-semibold text-gray-800 mb-4">Inventory Status</h3>
                <canvas id="inventoryStatusChart" height="250"></canvas>
            </div>
        </div>

        <!-- Product Feedback Section (New) -->
        <div class="bg-white rounded-lg shadow-md p-6 mb-8 hover:shadow-lg transition-shadow duration-300">
            <h3 class="text-lg font-semibold text-gray-800 mb-4 flex items-center">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2 text-yellow-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.197-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118l-3.976-2.888c-.784-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.674z" />
                </svg>
                Product Ratings & Performance
            </h3>
            <div class="overflow-x-auto">
                <table class="min-w-full divide-y divide-gray-200">
                    <thead class="bg-gray-50">
                        <tr>
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Product</th>
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Average Rating</th>
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Total Reviews</th>
                            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Sales Conversion</th>
                        </tr>
                    </thead>
                    <tbody class="bg-white divide-y divide-gray-200">
                        <!-- Sample data - this would be replaced with data from a ProductRatings repeater -->
                        <tr class="hover:bg-gray-50 transition-colors duration-200">
                            <td class="px-6 py-4 whitespace-nowrap">
                                <div class="text-sm font-medium text-gray-900">Chocolate Cake</div>
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap">
                                <div class="flex items-center">
                                    <div class="text-sm text-gray-900 mr-2">4.8</div>
                                    <div class="flex text-yellow-400">
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                        </svg>
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                        </svg>
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                        </svg>
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                        </svg>
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                        </svg>
                                    </div>
                                </div>
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap">
                                <div class="text-sm text-gray-900">42</div>
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap">
                                <div class="text-sm text-gray-900">68%</div>
                                <div class="w-full bg-gray-200 rounded-full h-2.5 mt-1">
                                    <div class="bg-green-500 h-2.5 rounded-full" style="width: 68%"></div>
                                </div>
                            </td>
                        </tr>
                        <tr class="hover:bg-gray-50 transition-colors duration-200">
                            <td class="px-6 py-4 whitespace-nowrap">
                                <div class="text-sm font-medium text-gray-900">Red Velvet Cake</div>
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap">
                                <div class="flex items-center">
                                    <div class="text-sm text-gray-900 mr-2">4.2</div>
                                    <div class="flex text-yellow-400">
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                        </svg>
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                        </svg>
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                        </svg>
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                        </svg>
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 text-gray-300" viewBox="0 0 20 20" fill="currentColor">
                                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                        </svg>
                                    </div>
                                </div>
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap">
                                <div class="text-sm text-gray-900">35</div>
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap">
                                <div class="text-sm text-gray-900">62%</div>
                                <div class="w-full bg-gray-200 rounded-full h-2.5 mt-1">
                                    <div class="bg-green-500 h-2.5 rounded-full" style="width: 62%"></div>
                                </div>
                            </td>
                        </tr>
                        <tr class="hover:bg-gray-50 transition-colors duration-200">
                            <td class="px-6 py-4 whitespace-nowrap">
                                <div class="text-sm font-medium text-gray-900">Vanilla Cupcake</div>
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap">
                                <div class="flex items-center">
                                    <div class="text-sm text-gray-900 mr-2">3.9</div>
                                    <div class="flex text-yellow-400">
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                        </svg>
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                        </svg>
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                        </svg>
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                        </svg>
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 text-gray-300" viewBox="0 0 20 20" fill="currentColor">
                                            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                        </svg>
                                    </div>
                                </div>
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap">
                                <div class="text-sm text-gray-900">28</div>
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap">
                                <div class="text-sm text-gray-900">54%</div>
                                <div class="w-full bg-gray-200 rounded-full h-2.5 mt-1">
                                    <div class="bg-yellow-500 h-2.5 rounded-full" style="width: 54%"></div>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
    </form>

    <!-- Chart.js Integration -->
    <script src="https://cdn.jsdelivr.net/npm/chart.js@3.7.1/dist/chart.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/luxon@3.0.1/build/global/luxon.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/chartjs-adapter-luxon@1.2.0/dist/chartjs-adapter-luxon.min.js"></script>
   <script>
       // Set default dates (last week)
       document.addEventListener('DOMContentLoaded', function () {
           const now = new Date();
           const lastWeek = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);

           // Format dates for input fields: YYYY-MM-DD
           const txtEndDate = document.getElementById('<%= txtEndDate.ClientID %>');
            const txtStartDate = document.getElementById('<%= txtStartDate.ClientID %>');
            
            if (txtEndDate) txtEndDate.value = now.toISOString().split('T')[0];
            if (txtStartDate) txtStartDate.value = lastWeek.toISOString().split('T')[0];
            
            // Initialize charts
            initCharts();
        });
        
        function initCharts() {
            console.log("Initializing charts...");
            
            // Helper function to safely parse JSON or return an empty array/object
            function safeJSONParse(jsonStr, defaultValue = []) {
                try {
                    if (!jsonStr || jsonStr === '') return defaultValue;
                    return JSON.parse(jsonStr);
                } catch (e) {
                    console.error("JSON parse error:", e, "for string:", jsonStr);
                    return defaultValue;
                }
            }
            
            // Sales Trend Chart (Line chart)
            const salesTrendCtx = document.getElementById('salesTrendChart');
            if (salesTrendCtx) {
                console.log("Initializing Sales Trend Chart");
                try {
                    const salesLabels = safeJSONParse('<%= SalesTrendLabels %>', []);
                    const salesData = safeJSONParse('<%= SalesTrendData %>', []);
                    
                    console.log("Sales Trend Data:", { labels: salesLabels, data: salesData });
                    
                    const salesTrendChart = new Chart(salesTrendCtx.getContext('2d'), {
                        type: 'line',
           data: {
                            labels: salesLabels,
            datasets: [{
                                label: 'Sales',
                                data: salesData,
                                borderColor: '#D43B6A',
                                backgroundColor: 'rgba(212, 59, 106, 0.1)',
                                borderWidth: 2,
                                tension: 0.3,
                                fill: true,
                                pointBackgroundColor: '#D43B6A',
                                pointBorderColor: '#fff',
                                pointBorderWidth: 2,
                                pointRadius: 4,
                                pointHoverRadius: 6
            }]
        },
        options: {
            responsive: true,
            plugins: {
                                legend: {
                                    display: false,
                                },
                tooltip: {
                                    mode: 'index',
                                    intersect: false,
                    callbacks: {
                                        label: function(context) {
                                            return `Sales: ${context.raw}`;
                                        }
                                    }
                                }
                            },
                            scales: {
                                x: {
                                    grid: {
                                        display: false
                                    }
                                },
                                y: {
                                    beginAtZero: true,
                                    grid: {
                                        color: 'rgba(0, 0, 0, 0.05)'
                                    }
                }
            }
        }
    });
                } catch (error) {
                    console.error("Error initializing Sales Trend Chart:", error);
                }
            } else {
                console.warn("Sales Trend Chart canvas not found");
            }
            
            // Revenue by Category Chart (Pie chart)
            const categoryRevenueCtx = document.getElementById('categoryRevenueChart');
            if (categoryRevenueCtx) {
                console.log("Initializing Category Revenue Chart");
                try {
                    const categoryNames = safeJSONParse('<%= CategoryNames %>', []);
                    const categoryData = safeJSONParse('<%= CategoryRevenueData %>', []);
                    
                    console.log("Category Revenue Data:", { labels: categoryNames, data: categoryData });
                    
                    const categoryRevenueChart = new Chart(categoryRevenueCtx.getContext('2d'), {
            type: 'pie',
            data: {
                            labels: categoryNames,
            datasets: [{
                                data: categoryData,
                                backgroundColor: [
                                    '#D43B6A', // Pink
                                    '#3B82F6', // Blue
                                    '#10B981', // Green
                                    '#F59E0B', // Yellow
                                    '#8B5CF6', // Purple
                                    '#EC4899', // Pink
                                    '#6366F1'  // Indigo
                                ],
                                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            plugins: {
                                legend: {
                                    position: 'right',
                                    labels: {
                                        boxWidth: 15,
                                        padding: 15,
                                        font: {
                                            size: 12
                                        }
                                    }
                                },
                tooltip: {
                    callbacks: {
                                        label: function(context) {
                                            const label = context.label || '';
                                            const value = context.raw || 0;
                                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                            const percentage = Math.round((value / total) * 100);
                                            return `${label}: ₱${value} (${percentage}%)`;
                                        }
                                    }
                                }
                            }
                        }
                    });
                } catch (error) {
                    console.error("Error initializing Category Revenue Chart:", error);
                }
            } else {
                console.warn("Category Revenue Chart canvas not found");
            }
            
            // Top Products Chart (Bar chart)
            const topProductsCtx = document.getElementById('topProductsChart');
            if (topProductsCtx) {
                console.log("Initializing Top Products Chart");
                try {
                    const productNames = safeJSONParse('<%= TopProductNames %>', []);
                    const productData = safeJSONParse('<%= TopProductQuantities %>', []);
                    
                    console.log("Top Products Data:", { labels: productNames, data: productData });
                    
                    const topProductsChart = new Chart(topProductsCtx.getContext('2d'), {
                        type: 'bar',
                        data: {
                            labels: productNames,
                            datasets: [{
                                label: 'Units Sold',
                                data: productData,
                                backgroundColor: 'rgba(212, 59, 106, 0.7)',
                                borderColor: 'rgba(212, 59, 106, 1)',
                                borderWidth: 1
                            }]
                        },
                        options: {
                            indexAxis: 'y',
                            responsive: true,
                            plugins: {
                                legend: {
                                    display: false
                                },
                                tooltip: {
                                    callbacks: {
                                        label: function(context) {
                                            return `Units Sold: ${context.raw}`;
                                        }
                                    }
                                }
                            },
                            scales: {
                                x: {
                                    beginAtZero: true,
                                    grid: {
                                        display: false
                                    }
                                },
                                y: {
                                    grid: {
                                        display: false
                                    }
                                }
                            }
                        }
                    });
                } catch (error) {
                    console.error("Error initializing Top Products Chart:", error);
                }
            } else {
                console.warn("Top Products Chart canvas not found");
            }
            
            // Inventory Status Chart (Doughnut chart)
            const inventoryCtx = document.getElementById('inventoryStatusChart');
            if (inventoryCtx) {
                console.log("Initializing Inventory Status Chart");
                try {
                    const inventoryData = safeJSONParse('<%= InventoryStatusData %>', [0, 0, 0]);
                    
                    console.log("Inventory Status Data:", inventoryData);
                    
                    const inventoryChart = new Chart(inventoryCtx.getContext('2d'), {
                        type: 'doughnut',
                        data: {
                            labels: ['Out of Stock', 'Low Stock', 'In Stock'],
                            datasets: [{
                                data: inventoryData,
                                backgroundColor: [
                                    'rgba(239, 68, 68, 0.7)', // Red (out of stock)
                                    'rgba(245, 158, 11, 0.7)', // Yellow (low stock)
                                    'rgba(16, 185, 129, 0.7)'  // Green (in stock)
                                ],
                                borderColor: [
                                    'rgba(239, 68, 68, 1)',
                                    'rgba(245, 158, 11, 1)',
                                    'rgba(16, 185, 129, 1)'
                                ],
                                borderWidth: 1
                            }]
                        },
                        options: {
                            responsive: true,
                            plugins: {
                legend: {
                                    position: 'right',
                    labels: {
                                        boxWidth: 15,
                                        padding: 15,
                        font: {
                                            size: 12
                                        }
                                    }
                                },
                                tooltip: {
                                    callbacks: {
                                        label: function(context) {
                                            const label = context.label || '';
                                            const value = context.raw || 0;
                                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                            const percentage = total > 0 ? Math.round((value / total) * 100) : 0;
                                            return `${label}: ${value} products (${percentage}%)`;
                        }
                    }
                }
            }
        }
    });
                } catch (error) {
                    console.error("Error initializing Inventory Status Chart:", error);
                }
            } else {
                console.warn("Inventory Status Chart canvas not found");
            }
            
            // Handle date filter changes
            const btnFilterReport = document.getElementById('<%= btnFilterReport.ClientID %>');
            if (btnFilterReport) {
                btnFilterReport.addEventListener('click', function(e) {
                    // We'll let the server-side event handler deal with this
                    console.log("Filter button clicked");
                });
            }
            
            // Time range selector change handler
            const timeRangeSelector = document.getElementById('<%= TimeRangeSelector.ClientID %>');
            if (timeRangeSelector) {
                console.log("TimeRangeSelector connected:", timeRangeSelector);
                timeRangeSelector.addEventListener('change', function() {
                    console.log("Time range changed to: " + this.value);
                    // Form will auto-postback due to AutoPostBack="true"
                });
            } else {
                console.warn("TimeRangeSelector not found");
            }
            
            // Display debug info in console
            console.log("Chart initialization completed");
            console.log("Data binding values:");
            console.log("Total Revenue:", '<%= TotalRevenue %>');
            console.log("Total Sales:", '<%= TotalSales %>');
            console.log("Average Order Value:", '<%= AverageOrderValue %>');
            console.log("Sales Growth:", '<%= SalesGrowth %>%');
            console.log("Revenue Growth:", '<%= RevenueGrowth %>%');
            console.log("AVG Order Value Change:", '<%= AvgOrderValueChange %>%');
       }
   </script>
</asp:Content>