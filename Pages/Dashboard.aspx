<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Pages/AdminMaster.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="OnlinePastryShop.Pages.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="AdminContent" runat="server">
    <form id="dashboardForm" runat="server">
    <div class="container mx-auto p-4">
        <!-- Error Message -->
        <asp:Label ID="lblErrorMessage" runat="server" CssClass="text-red-500 text-center mb-4" Visible="false"></asp:Label>
        
        <!-- Dashboard Header -->
        <div class="flex flex-col md:flex-row justify-between items-center mb-6">
            <h1 class="text-2xl font-bold text-[#D43B6A] mb-4 md:mb-0">Dashboard</h1>
            
            <div class="flex flex-wrap gap-3">
                <!-- Date Filter -->
                <div class="flex items-center">
                    <label for="TimeRangeSelector" class="text-sm text-gray-600 mr-2">Time Range:</label>
                    <asp:DropDownList ID="TimeRangeSelector" runat="server" AutoPostBack="true" OnSelectedIndexChanged="TimeRangeSelector_SelectedIndexChanged"
                        CssClass="text-sm border border-gray-300 rounded px-3 py-1"
                        Width="120px">
                        <asp:ListItem Text="Today" Value="today" Selected="True"></asp:ListItem>
                        <asp:ListItem Text="Yesterday" Value="yesterday"></asp:ListItem>
                        <asp:ListItem Text="This Week" Value="week"></asp:ListItem>
                        <asp:ListItem Text="This Month" Value="month"></asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
        </div>
        
        <!-- Real-time Status Banner -->
        <div class="bg-gradient-to-r from-blue-50 to-purple-50 rounded-lg shadow-md p-4 mb-6 border-l-4 border-[#D43B6A]">
            <div class="flex items-center">
                <div class="mr-3">
                    <div class="w-3 h-3 bg-green-500 rounded-full animate-pulse"></div>
                </div>
                <div>
                    <h2 class="text-lg font-semibold text-gray-800">Last updated: <span id="lastUpdateTime"><%= DateTime.Now.ToString("MMM dd, yyyy - h:mm:ss tt") %></span></h2>
                    <p class="text-sm text-gray-600">Real-time dashboard monitoring all pastry shop activities</p>
                </div>
                <div class="ml-auto">
                    <span class="px-3 py-1 <%= SystemStatusClass %> rounded-full text-xs font-semibold system-status-indicator">
                        <%= SystemStatusText %>
                    </span>
                </div>
            </div>
        </div>
        
        <!-- KPI Cards -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
            <!-- Daily Revenue -->
            <div class="bg-white rounded-lg shadow-md p-6 border-l-4 border-[#D43B6A]">
                <div class="flex items-center justify-between">
                    <div>
                        <p class="text-gray-500 text-sm"><%= RevenueCardTitle %></p>
                        <h2 class="text-3xl font-bold text-gray-800">₱<%= DailyRevenue %></h2>
                        <p class="text-sm mt-1">
                            <span class="<%= DailyRevenueChangeClass %> flex items-center">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="<%= DailyRevenueIcon %>" />
                                </svg>
                                <%= DailyRevenueChange %>%
                            </span>
                            <%= RevenueComparisonText %>
                        </p>
                    </div>
                    <div class="p-3 bg-pink-50 rounded-full">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 text-[#D43B6A]" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-5 0a4 4 0 11-8 0 4 4 0 018 0z" />
                        </svg>
                    </div>
                </div>
            </div>
            
            <!-- Today's Orders -->
            <div class="bg-white rounded-lg shadow-md p-6 border-l-4 border-blue-500">
                <div class="flex items-center justify-between">
                    <div>
                        <p class="text-gray-500 text-sm"><%= OrderCardTitle %></p>
                        <h2 class="text-3xl font-bold text-gray-800"><%= TodayOrderCount %></h2>
                        <p class="text-sm mt-1">
                            <span class="<%= OrderCountChangeClass %> flex items-center">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="<%= OrderCountIcon %>" />
                                </svg>
                                <%= OrderCountChange %>%
                            </span>
                            <%= OrderComparisonText %>
                        </p>
                    </div>
                    <div class="p-3 bg-blue-50 rounded-full">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 text-blue-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
                        </svg>
                    </div>
                </div>
            </div>
            
            <!-- Pending Orders -->
            <div class="bg-white rounded-lg shadow-md p-6 border-l-4 border-amber-500">
                <div class="flex items-center justify-between">
                    <div>
                        <p class="text-gray-500 text-sm">Pending Orders</p>
                        <h2 class="text-3xl font-bold text-gray-800"><%= PendingOrderCount %></h2>
                        <p class="text-sm mt-1 text-amber-600">
                            Requires your attention
                        </p>
                    </div>
                    <div class="p-3 bg-amber-50 rounded-full">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 text-amber-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                    </div>
                </div>
            </div>
            
            <!-- Low Stock Items -->
            <div class="bg-white rounded-lg shadow-md p-6 border-l-4 border-red-500">
                <div class="flex items-center justify-between">
                    <div>
                        <p class="text-gray-500 text-sm">Low Stock Items</p>
                        <h2 class="text-3xl font-bold text-gray-800"><%= LowStockCount %></h2>
                        <p class="text-sm mt-1 text-red-600">
                            Need to restock
                        </p>
                    </div>
                    <div class="p-3 bg-red-50 rounded-full">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                        </svg>
                    </div>
                </div>
            </div>
        </div>
        
        <!-- Main Content Grid -->
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
            <!-- Pending Orders Section (Wider) -->
            <div class="lg:col-span-2 bg-white rounded-lg shadow-md overflow-hidden">
                <div class="p-4 border-b border-gray-200 bg-gray-50 flex justify-between items-center">
                    <h3 class="text-lg font-semibold text-gray-800 flex items-center">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2 text-amber-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                        Pending Orders
                    </h3>
                    <a href="Orders.aspx?filter=pending" class="text-sm text-blue-600 hover:text-blue-800">View All</a>
                </div>
                
                <!-- Orders Table -->
                <div class="overflow-x-auto" style="max-height: 400px;">
                    <table class="min-w-full divide-y divide-gray-200">
                        <thead class="bg-gray-50 sticky top-0">
                            <tr>
                                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Order ID</th>
                                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Customer</th>
                                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date</th>
                                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Amount</th>
                                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Action</th>
                            </tr>
                        </thead>
                        <tbody class="bg-white divide-y divide-gray-200">
                            <asp:Repeater ID="PendingOrdersRepeater" runat="server" OnItemCommand="PendingOrdersRepeater_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td class="px-6 py-4 whitespace-nowrap">
                                            <div class="text-sm font-medium text-gray-900">#<%# Eval("OrderId") %></div>
                                        </td>
                                        <td class="px-6 py-4 whitespace-nowrap">
                                            <div class="text-sm text-gray-900"><%# Eval("CustomerName") %></div>
                                            <div class="text-sm text-gray-500"><%# Eval("Email") %></div>
                                        </td>
                                        <td class="px-6 py-4 whitespace-nowrap">
                                            <div class="text-sm text-gray-900"><%# Convert.ToDateTime(Eval("OrderDate")).ToString("MMM dd, yyyy") %></div>
                                            <div class="text-sm text-gray-500"><%# Convert.ToDateTime(Eval("OrderDate")).ToString("h:mm tt") %></div>
                                        </td>
                                        <td class="px-6 py-4 whitespace-nowrap">
                                            <div class="text-sm font-medium text-gray-900">₱<%# Eval("TotalAmount", "{0:N2}") %></div>
                                        </td>
                                        <td class="px-6 py-4 whitespace-nowrap">
                                            <span class="px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full bg-yellow-100 text-yellow-800">
                                                <%# Eval("Status") %>
                                            </span>
                                        </td>
                                        <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                                            <div class="flex space-x-2">
                                                <asp:LinkButton ID="btnApprove" runat="server" 
                                                    CommandName="Approve" 
                                                    CommandArgument='<%# Eval("OrderId") %>'
                                                    CssClass="text-white bg-green-500 hover:bg-green-600 px-3 py-1 rounded-md">
                                                    Approve
                                                </asp:LinkButton>
                                                <asp:LinkButton ID="btnReject" runat="server" 
                                                    CommandName="Reject" 
                                                    CommandArgument='<%# Eval("OrderId") %>'
                                                    CssClass="text-white bg-red-500 hover:bg-red-600 px-3 py-1 rounded-md">
                                                    Reject
                                                </asp:LinkButton>
                                            </div>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                            <!-- Empty Pending Orders Message -->
                            <asp:PlaceHolder ID="EmptyPendingOrdersMessage" runat="server" Visible="false">
                                <tr>
                                    <td colspan="6" class="px-6 py-4 text-center text-sm text-gray-500">
                                        No pending orders at this time
                                    </td>
                                </tr>
                            </asp:PlaceHolder>
                        </tbody>
                    </table>
                </div>
            </div>
            
            <!-- Top Products Table -->
            <div class="col-span-2 lg:col-span-1 bg-white rounded-lg shadow-md overflow-hidden">
                <div class="p-4 border-b border-gray-200 bg-gray-50">
                    <h3 class="text-lg font-semibold text-gray-800 flex items-center">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2 text-blue-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
                        </svg>
                        Top Selling Products
                    </h3>
                </div>
                <div class="p-4">
                    <div class="max-h-[350px] overflow-y-auto"> 
                        <table class="min-w-full table-fixed">
                            <thead class="bg-gray-50 sticky top-0">
                                <tr>
                                    <th class="w-[40%] px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Product</th>
                                    <th class="w-[20%] px-4 py-2 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">Quantity Sold</th>
                                    <th class="w-[40%] px-4 py-2 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Revenue</th>
                                </tr>
                            </thead>
                            <tbody class="bg-white divide-y divide-gray-200">
                                <asp:Repeater ID="TopProductsRepeater" runat="server">
                                    <ItemTemplate>
                                        <tr class="hover:bg-gray-50">
                                            <td class="px-4 py-3 text-left">
                                                <div class="text-sm font-medium text-gray-900"><%# Eval("ProductName") %></div>
                                            </td>
                                            <td class="px-4 py-3 text-center">
                                                <div class="text-sm text-gray-500"><%# Eval("QuantitySold") %></div>
                                            </td>
                                            <td class="px-4 py-3 text-right">
                                                <div class="text-sm text-gray-900">₱<%# Convert.ToDecimal(Eval("Revenue")).ToString("N2") %></div>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <!-- Empty data placeholder (if no products) -->
                                <tr id="noTopProductsData" style="display:none;">
                                    <td colspan="3" class="px-4 py-6 text-center text-gray-500">
                                        No product sales data available
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
        
        <!-- Additional Insights Grid -->
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
            <!-- Sales Overview Chart -->
            <div class="lg:col-span-2 bg-white rounded-lg shadow-md overflow-hidden">
                <div class="p-4 border-b border-gray-200 bg-gray-50">
                    <h3 class="text-lg font-semibold text-gray-800 flex items-center">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2 text-blue-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                        </svg>
                        Sales Overview
                    </h3>
                </div>
                <!-- Chart container with fixed height to ensure visibility -->
                <div class="p-4" style="height: 300px;">
                    <canvas id="salesOverviewChart"></canvas>
                    <div id="noSalesData" class="hidden flex items-center justify-center h-full">
                        <p class="text-gray-500">No sales data available for the selected period</p>
                    </div>
                </div>
            </div>
            
        <!-- Low Stock Products -->
            <div class="col-span-1 md:col-span-1 bg-white rounded-lg shadow-md overflow-hidden">
                <div class="p-4 border-b border-gray-200 bg-gray-50">
                <h3 class="text-lg font-semibold text-gray-800 flex items-center">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 mr-2 text-amber-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                    </svg>
                    Low Stock Products
                </h3>
            </div>
                <div class="p-4">
                    <div class="overflow-y-auto max-h-[300px]">
                        <asp:Repeater ID="LowStockRepeater" runat="server">
                            <ItemTemplate>
                                <div class="rounded-lg bg-amber-50 mb-3 p-3 border border-amber-200 cursor-pointer" onclick="redirectToProducts()">
                                    <div class="flex justify-between">
                                        <div class="text-sm font-medium text-gray-800"><%# Eval("Name") %></div>
                                        <div class="text-xs font-medium text-amber-600 bg-amber-200 px-2 py-1 rounded-full"><%# Eval("StockQuantity") %> left</div>
                                    </div>
                                    <div class="text-xs text-gray-500 mt-1">Category: <%# Eval("CategoryName") %></div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <!-- Add a placeholder for empty data instead of using EmptyDataTemplate -->
                        <asp:PlaceHolder ID="EmptyLowStockMessage" runat="server" Visible="false">
                            <div class="text-center py-4 text-gray-500">
                                <p>No low stock products.</p>
                            </div>
                        </asp:PlaceHolder>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Chart.js Integration -->
    <script src="https://cdn.jsdelivr.net/npm/chart.js@3.7.1/dist/chart.min.js"></script>
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            console.log("DOM content loaded, initializing charts");
            
            // Debug logging for labels and data
            console.log("Sales Overview Labels:", '<%= SalesOverviewLabels %>');
            console.log("Sales Overview Data:", '<%= SalesOverviewData %>');
            
            // Helper function to check for valid JSON data
            function isValidJsonString(str) {
                if (!str || str.trim() === '' || str === '[]') return false;
                try {
                    JSON.parse(str);
                    return true;
                } catch (e) {
                    console.error("Invalid JSON:", str, e);
                    return false;
                }
            }
            
            // Sales Overview Chart
            const salesOverviewChartElement = document.getElementById('salesOverviewChart');
            const noSalesDataElement = document.getElementById('noSalesData');
            
            if (salesOverviewChartElement) {
                try {
                    // Check if we have valid data first
                    const salesLabelsJson = '<%= SalesOverviewLabels %>';
                    const salesDataJson = '<%= SalesOverviewData %>';
                    
                    if (!isValidJsonString(salesLabelsJson) || !isValidJsonString(salesDataJson)) {
                        throw new Error("Invalid or empty sales data");
                    }
                    
                    const salesOverviewCtx = salesOverviewChartElement.getContext('2d');
                    const salesLabels = JSON.parse(salesLabelsJson);
                    const salesData = JSON.parse(salesDataJson);
                    
                    console.log('Sales overview data:', {
                        labels: salesLabels,
                        data: salesData
                    });
                    
                    // Only create chart if we have actual data
                    if (salesLabels.length > 0 && salesData.length > 0) {
                        new Chart(salesOverviewCtx, {
                            type: 'line',
                            data: {
                                labels: salesLabels,
                                datasets: [{
                                    label: 'Revenue',
                                    data: salesData,
                                    borderColor: 'rgba(59, 130, 246, 1)',
                                    backgroundColor: 'rgba(59, 130, 246, 0.1)',
                                    borderWidth: 2,
                                    tension: 0.3,
                                    fill: true
                                }]
                            },
                            options: {
                                responsive: true,
                                maintainAspectRatio: false,
                                plugins: {
                                    legend: {
                                        display: false
                                    },
                                    tooltip: {
                                        callbacks: {
                                            label: function(context) {
                                                return '₱' + context.raw.toLocaleString();
                                            }
                                        }
                                    }
                                },
                                scales: {
                                    y: {
                                        beginAtZero: true,
                                        grid: {
                                            color: 'rgba(0, 0, 0, 0.05)'
                                        },
                                        ticks: {
                                            callback: function(value) {
                                                return '₱' + value.toLocaleString();
                                            }
                                        }
                                    },
                                    x: {
                                        grid: {
                                            display: false
                                        }
                                    }
                                }
                            }
                        });
                    } else {
                        throw new Error("Empty sales data arrays");
                    }
                } catch (error) {
                    console.error('Error initializing sales overview chart:', error);
                    
                    // Hide the canvas and show the no data message
                    if (salesOverviewChartElement) salesOverviewChartElement.style.display = 'none';
                    if (noSalesDataElement) noSalesDataElement.classList.remove('hidden');
                }
            } else {
                console.warn('salesOverviewChart element not found');
            }
            
            // Update the last updated time every minute
            setInterval(function() {
                const now = new Date();
                document.getElementById('lastUpdateTime').textContent = now.toLocaleString('en-US', {
                    month: 'short',
                    day: '2-digit',
                    year: 'numeric',
                    hour: 'numeric',
                    minute: '2-digit',
                    second: '2-digit',
                    hour12: true
                });
            }, 60000);
            
            // Make the time range selector actually do something
            const timeRangeSelector = document.getElementById('TimeRangeSelector');
            if (timeRangeSelector) {
                timeRangeSelector.addEventListener('change', function() {
                    // Store the selected value in sessionStorage
                    sessionStorage.setItem('selectedTimeRange', this.value);
                    
                    // Redirect to the same page with the new time range parameter
                    window.location.href = window.location.pathname + '?timeRange=' + this.value;
                });
                
                // Set the value from URL parameter or sessionStorage if available
                const urlParams = new URLSearchParams(window.location.search);
                const timeRangeParam = urlParams.get('timeRange');
                if (timeRangeParam) {
                    timeRangeSelector.value = timeRangeParam;
                } else if (sessionStorage.getItem('selectedTimeRange')) {
                    timeRangeSelector.value = sessionStorage.getItem('selectedTimeRange');
                }
            }
            
            // Periodically check connection status (every 30 seconds)
            setInterval(function() {
                fetch('CheckDatabaseConnection.aspx')
                    .then(response => response.json())
                    .then(data => {
                        const statusElement = document.querySelector('.system-status-indicator');
                        if (statusElement) {
                            if (data.isConnected) {
                                statusElement.className = 'px-3 py-1 bg-green-100 text-green-800 rounded-full text-xs font-semibold system-status-indicator';
                                statusElement.textContent = 'System Online';
                            } else {
                                statusElement.className = 'px-3 py-1 bg-red-100 text-red-800 rounded-full text-xs font-semibold system-status-indicator';
                                statusElement.textContent = 'System Offline';
                            }
                        }
                    })
                    .catch(error => {
                        console.error('Error checking connection status:', error);
                        // If we can't reach the server, we're definitely offline
                        const statusElement = document.querySelector('.system-status-indicator');
                        if (statusElement) {
                            statusElement.className = 'px-3 py-1 bg-red-100 text-red-800 rounded-full text-xs font-semibold system-status-indicator';
                            statusElement.textContent = 'System Offline';
                        }
                    });
            }, 30000); // Check every 30 seconds
        });
    </script>

    <!-- Product Detail Modal -->
    <div id="productDetailModal" class="fixed inset-0 bg-gray-900 bg-opacity-50 z-50 flex items-center justify-center hidden">
        <div class="bg-white rounded-lg shadow-xl w-full max-w-4xl max-h-[90vh] overflow-y-auto">
            <div class="p-6">
                <div class="flex justify-between items-center mb-4">
                    <h3 class="text-xl font-semibold text-gray-800" id="modalProductName">Product Details</h3>
                    <button type="button" class="text-gray-400 hover:text-gray-500" onclick="closeProductModal()">
                        <svg class="h-6 w-6" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>
                
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                        <div class="mb-4">
                            <h4 class="text-sm font-medium text-gray-500 mb-1">Product Information</h4>
                            <div class="bg-gray-50 p-4 rounded-lg">
                                <div class="grid grid-cols-2 gap-4">
                                    <div>
                                        <div class="text-xs text-gray-500">Product ID</div>
                                        <div class="font-medium" id="modalProductId">-</div>
                                    </div>
                                    <div>
                                        <div class="text-xs text-gray-500">Category</div>
                                        <div class="font-medium" id="modalCategory">-</div>
                                    </div>
                                    <div>
                                        <div class="text-xs text-gray-500">Price</div>
                                        <div class="font-medium" id="modalPrice">-</div>
                                    </div>
                                    <div>
                                        <div class="text-xs text-gray-500">Stock</div>
                                        <div class="font-medium" id="modalStock">-</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        <div class="mb-4">
                            <h4 class="text-sm font-medium text-gray-500 mb-1">Sales Metrics</h4>
                            <div class="bg-gray-50 p-4 rounded-lg">
                                <div class="grid grid-cols-2 gap-4">
                                    <div>
                                        <div class="text-xs text-gray-500">Units Sold</div>
                                        <div class="font-medium" id="modalUnitsSold">-</div>
                                    </div>
                                    <div>
                                        <div class="text-xs text-gray-500">Total Revenue</div>
                                        <div class="font-medium" id="modalRevenue">-</div>
                                    </div>
                                    <div>
                                        <div class="text-xs text-gray-500">Profit Margin</div>
                                        <div class="font-medium" id="modalProfitMargin">-</div>
                                    </div>
                                    <div>
                                        <div class="text-xs text-gray-500">Growth Rate</div>
                                        <div class="font-medium" id="modalGrowthRate">-</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <div>
                        <div class="mb-4">
                            <h4 class="text-sm font-medium text-gray-500 mb-1">Sales Trend</h4>
                            <div class="bg-gray-50 p-4 rounded-lg h-48">
                                <canvas id="productSalesChart"></canvas>
                            </div>
                        </div>
                        
                        <div>
                            <h4 class="text-sm font-medium text-gray-500 mb-1">Customer Reviews</h4>
                            <div class="bg-gray-50 p-4 rounded-lg max-h-40 overflow-y-auto" id="modalReviews">
                                <div class="flex items-start space-x-3 mb-3">
                                    <div class="text-yellow-400 flex">
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
                                    <div>
                                        <p class="text-sm text-gray-600 font-medium">Jane S.</p>
                                        <p class="text-xs text-gray-500">3 days ago</p>
                                        <p class="text-sm mt-1">Absolutely delicious! Would order again.</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                
                <div class="mt-6 pt-4 border-t border-gray-100 flex justify-end space-x-3">
                    <button type="button" class="px-4 py-2 bg-white border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500" onclick="closeProductModal()">
                        Close
                    </button>
                    <button type="button" class="px-4 py-2 bg-indigo-600 border border-transparent rounded-md text-sm font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500">
                        Edit Product
                    </button>
                </div>
            </div>
        </div>
    </div>

    <!-- JavaScript for Product Detail Modal -->
    <script>
        function showProductDetails(productId) {
            // This would be replaced with an AJAX call in a real implementation
            // For now, we'll use static data for demonstration
            document.getElementById('modalProductName').textContent = 'Product Details';
            document.getElementById('modalProductId').textContent = productId;
            document.getElementById('modalCategory').textContent = 'Cakes';
            document.getElementById('modalPrice').textContent = '₱450.00';
            document.getElementById('modalStock').textContent = '15';
            document.getElementById('modalUnitsSold').textContent = '42';
            document.getElementById('modalRevenue').textContent = '₱18,900.00';
            document.getElementById('modalProfitMargin').textContent = '35%';
            document.getElementById('modalGrowthRate').textContent = '+8.5%';
            
            // Show modal
            document.getElementById('productDetailModal').classList.remove('hidden');
            
            // Initialize chart
            initProductSalesChart();
        }
        
        function closeProductModal() {
            document.getElementById('productDetailModal').classList.add('hidden');
        }
        
        function openEditModal(productId) {
            // Redirect to product edit page
            window.location.href = 'ProductEdit.aspx?id=' + productId;
        }
        
        function initProductSalesChart() {
            const chartCanvas = document.getElementById('productSalesChart');
            if (!chartCanvas) {
                console.warn('productSalesChart canvas not found');
                return;
            }
            
            var ctx = chartCanvas.getContext('2d');
            
            // Sample data
            var salesData = {
                labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
                datasets: [{
                    label: 'Units Sold',
                    borderColor: '#4F46E5',
                    backgroundColor: 'rgba(79, 70, 229, 0.1)',
                    data: [5, 8, 12, 10, 15, 18],
                    fill: true,
                    tension: 0.4
                }]
            };
            
            var salesChart = new Chart(ctx, {
                type: 'line',
                data: salesData,
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            display: false
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
        }
        
        // Add event listeners to product cards for hover effects
        document.addEventListener('DOMContentLoaded', function() {
            const productCards = document.querySelectorAll('.product-card');
            productCards.forEach(card => {
                card.addEventListener('mouseenter', function() {
                    this.classList.add('transform', 'scale-105');
                });
                card.addEventListener('mouseleave', function() {
                    this.classList.remove('transform', 'scale-105');
                });
            });
        });
    </script>

    <!-- Add this script at the end of the file, just before </form> -->
    <script>
        function redirectToProducts() {
            // Navigate to the Products page and set the sort dropdown to "Stock (Low to High)"
            window.location.href = 'Products.aspx';

            // Store the sort preference in localStorage to be read by Products page
            localStorage.setItem('productSort', 'stock_asc');

            // This is a backup approach - many pages use URL parameters
            if (!localStorage) {
                window.location.href = 'Products.aspx?sortOption=stock_asc';
            }
        }
    </script>
    </form>
</asp:Content>
