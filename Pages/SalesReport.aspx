<%@ Page Title="Sales Report"
         Language="C#"
         MasterPageFile="~/Pages/AdminMaster.Master"
         AutoEventWireup="true"
         CodeBehind="SalesReport.aspx.cs"
         Inherits="OnlinePastryShop.Pages.SalesReport" %>

<asp:Content ID="Content1" ContentPlaceHolderID="AdminContent" runat="server">

        <!-- Chart Data Initialization -->
        <script type="text/javascript">
            // Global object to hold chart data from server
            var chartData = {};
        </script>

        <div class="container mx-auto p-4">
            <!-- Error Message -->
            <asp:Label ID="lblErrorMessage" runat="server"
                       CssClass="text-red-500 text-center block mb-4"
                       Visible="false"></asp:Label>

            <!-- Success Message -->
            <asp:Label ID="lblSuccessMessage" runat="server"
                       CssClass="text-green-500 text-center block mb-4"
                       Visible="false"></asp:Label>

            <!-- Dashboard Header with Time Selector -->
            <div class="flex flex-col md:flex-row justify-between items-center mb-6">
                <h1 class="text-2xl font-bold text-[#D43B6A] mb-4 md:mb-0">Sales Report Dashboard</h1>

                <!-- Time Range Controls -->
                <div class="flex flex-col sm:flex-row gap-2 items-end w-full md:w-auto">
                    <div class="flex-1 md:flex-none">
                        <label for="TimeRangeSelector" class="block text-sm font-medium text-gray-700 mb-1">Time Range</label>
                        <asp:DropDownList ID="TimeRangeSelector" runat="server"
                                         AutoPostBack="true"
                                         OnSelectedIndexChanged="TimeRangeSelector_SelectedIndexChanged"
                                         CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500">
                            <asp:ListItem Text="Daily" Value="Daily" />
                            <asp:ListItem Text="Weekly" Value="Weekly" Selected="True" />
                            <asp:ListItem Text="Monthly" Value="Monthly" />
                            <asp:ListItem Text="Yearly" Value="Yearly" />
                            <asp:ListItem Text="Custom Range" Value="Custom" />
                        </asp:DropDownList>
                    </div>

                    <!-- Custom Date Range Controls -->
                    <div id="customDateRange" runat="server" class="flex flex-col sm:flex-row gap-2 w-full md:w-auto mt-2 sm:mt-0">
                        <div class="flex-1 md:flex-none">
                            <label for="txtStartDate" class="block text-sm font-medium text-gray-700 mb-1">Start Date</label>
                            <asp:TextBox ID="txtStartDate" runat="server" TextMode="Date"
                                       CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"></asp:TextBox>
                        </div>
                        <div class="flex-1 md:flex-none">
                            <label for="txtEndDate" class="block text-sm font-medium text-gray-700 mb-1">End Date</label>
                            <asp:TextBox ID="txtEndDate" runat="server" TextMode="Date"
                                       CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"></asp:TextBox>
                        </div>
                        <div class="flex-none flex items-end">
                            <asp:Button ID="btnFilterReport" runat="server" Text="Apply Filter"
                                      OnClick="btnFilterReport_Click"
                                      CssClass="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-[#D43B6A] hover:bg-[#c02f5c] focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500" />
                        </div>
                    </div>
                </div>
            </div>

            <!-- Message for no data -->
            <asp:Label ID="lblMessage" runat="server" CssClass="text-gray-500 text-center block my-8" Visible="false">
                No sales data available for the selected time period.
            </asp:Label>

            <!-- KPI Cards Row -->
            <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
                <!-- Total Revenue Card -->
                <div class="bg-white rounded-lg shadow-md p-6 border-l-4 border-[#D43B6A]">
                    <div class="flex justify-between items-start">
                        <div>
                            <p class="text-gray-500 text-sm mb-1">Total Revenue</p>
                            <h3 class="text-2xl font-bold"><%= TotalRevenue %></h3>
                            <div class="flex items-center mt-2">
                                <svg class="<%= RevenueGrowthClass %> h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                    <path fill-rule="evenodd" d="<%= RevenueGrowthIcon %>" clip-rule="evenodd"/>
                                </svg>
                                <span class="<%= RevenueGrowthClass %> ml-1 text-sm"><%= RevenueGrowth %></span>
                                <span class="text-gray-400 text-xs ml-1">vs previous period</span>
                            </div>
                        </div>
                        <div class="p-3 bg-pink-50 rounded-full">
                            <svg class="h-6 w-6 text-[#D43B6A]" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
                            </svg>
                        </div>
                    </div>
                </div>

                <!-- Order Count Card -->
                <div class="bg-white rounded-lg shadow-md p-6 border-l-4 border-blue-500">
                    <div class="flex justify-between items-start">
                        <div>
                            <p class="text-gray-500 text-sm mb-1">Total Orders</p>
                            <h3 class="text-2xl font-bold"><%= TotalSales %></h3>
                            <div class="flex items-center mt-2">
                                <svg class="<%= SalesGrowthClass %> h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                    <path fill-rule="evenodd" d="<%= SalesGrowthIcon %>" clip-rule="evenodd"/>
                                </svg>
                                <span class="<%= SalesGrowthClass %> ml-1 text-sm"><%= SalesGrowth %></span>
                                <span class="text-gray-400 text-xs ml-1">vs previous period</span>
                            </div>
                        </div>
                        <div class="p-3 bg-blue-50 rounded-full">
                            <svg class="h-6 w-6 text-blue-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z"/>
                            </svg>
                        </div>
                    </div>
                </div>

                <!-- Items Per Order Card (replacing Average Order Value) -->
                <div class="bg-white rounded-lg shadow-md p-6 border-l-4 border-purple-500">
                    <div class="flex justify-between items-start">
                        <div>
                            <p class="text-gray-500 text-sm mb-1">Items Per Order</p>
                            <h3 class="text-2xl font-bold"><%= ItemsPerOrder %></h3>
                            <div class="flex items-center mt-2">
                                <svg class="<%= ItemsPerOrderChangeClass %> h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                                    <path fill-rule="evenodd" d="<%= ItemsPerOrderIcon %>" clip-rule="evenodd"/>
                                </svg>
                                <span class="<%= ItemsPerOrderChangeClass %> ml-1 text-sm"><%= ItemsPerOrderChange %></span>
                                <span class="text-gray-400 text-xs ml-1">vs previous period</span>
                            </div>
                        </div>
                        <div class="p-3 bg-purple-50 rounded-full">
                            <svg class="h-6 w-6 text-purple-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 14v6m-3-3h6M6 10h2a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v2a2 2 0 002 2zm10 0h2a2 2 0 002-2V6a2 2 0 00-2-2h-2a2 2 0 00-2 2v2a2 2 0 002 2zM6 20h2a2 2 0 002-2v-2a2 2 0 00-2-2H6a2 2 0 00-2 2v2a2 2 0 002 2z" />
                            </svg>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Main Charts Grid -->
            <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
                <!-- Sales Trend Chart -->
                <div class="bg-white rounded-lg shadow-md p-6">
                    <div class="flex justify-between items-center mb-4">
                        <h3 class="text-lg font-semibold text-gray-800">Sales Trend (<%= ChartPeriodText %>)</h3>
                    </div>
                    <div class="h-72">
                        <canvas id="salesTrendChart"></canvas>
                    </div>
                </div>

                <!-- Revenue by Category -->
                <div class="bg-white rounded-lg shadow-md p-6">
                    <div class="flex justify-between items-center mb-4">
                        <h3 class="text-lg font-semibold text-gray-800">Revenue by Category</h3>
                    </div>
                    <div class="h-72">
                        <canvas id="categoryRevenueChart"></canvas>
                    </div>
                </div>
            </div>

            <!-- Secondary Charts Grid -->
            <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
                <!-- Top Selling Products -->
                <div class="bg-white rounded-lg shadow-md p-6">
                    <div class="flex justify-between items-center mb-4">
                        <h3 class="text-lg font-semibold text-gray-800">Top Selling Products</h3>
                    </div>
                    <div class="h-72">
                        <canvas id="topProductsChart"></canvas>
                    </div>
                </div>

                <!-- Inventory Status -->
                <div class="bg-white rounded-lg shadow-md p-6">
                    <div class="flex justify-between items-center mb-4">
                        <h3 class="text-lg font-semibold text-gray-800">Inventory Status</h3>
                    </div>
                    <div class="h-72">
                        <canvas id="inventoryStatusChart"></canvas>
                    </div>
                </div>
            </div>

            <!-- Product Performance Table -->
            <div class="bg-white rounded-lg shadow-md p-6 mb-8">
                <div class="flex justify-between items-center mb-4">
                    <h3 class="text-lg font-semibold text-gray-800">Product Performance</h3>
                </div>

                <div class="overflow-x-auto">
                    <table class="min-w-full divide-y divide-gray-200">
                        <thead class="bg-gray-50">
                            <tr>
                                <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Product
                                </th>
                                <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Avg. Rating
                                </th>
                                <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Reviews
                                </th>
                                <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Conversion Rate
                                </th>
                            </tr>
                        </thead>
                        <tbody class="bg-white divide-y divide-gray-200">
                            <% if (ProductRatings != null && ProductRatings.Count > 0) { %>
                                <% foreach (var product in ProductRatings) { %>
                                    <tr>
                                        <td class="px-6 py-4 whitespace-nowrap">
                                            <div class="text-sm font-medium text-gray-900"><%= product.ProductName %></div>
                                        </td>
                                        <td class="px-6 py-4 whitespace-nowrap">
                                            <div class="flex items-center">
                                                <div class="text-sm text-gray-900 mr-2"><%= (product.AverageRating > 0 ? product.AverageRating.ToString("0.0") : "0.0") %></div>
                                                <div class="flex text-yellow-400">
                                                    <% for (int i = 1; i <= 5; i++) { %>
                                                        <% if (product.AverageRating > 0 && i <= Math.Floor(product.AverageRating)) { %>
                                                            <!-- Full star -->
                                                            <svg class="h-4 w-4 fill-current" viewBox="0 0 20 20">
                                                                <path d="M10 15l-5.878 3.09 1.123-6.545L.489 6.91l6.572-.955L10 0l2.939 5.955 6.572.955-4.756 4.635 1.123 6.545z"/>
                                                            </svg>
                                                        <% } else if (product.AverageRating > 0 && i <= Math.Ceiling(product.AverageRating) && product.AverageRating % 1 != 0) { %>
                                                            <!-- Half star -->
                                                            <svg class="h-4 w-4 fill-current" viewBox="0 0 20 20">
                                                                <defs>
                                                                    <linearGradient id="half<%= product.ProductName.GetHashCode() %>_<%= i %>">
                                                                        <stop offset="50%" stop-color="currentColor"/>
                                                                        <stop offset="50%" stop-color="#CBD5E0"/>
                                                                    </linearGradient>
                                                                </defs>
                                                                <path fill="url(#half<%= product.ProductName.GetHashCode() %>_<%= i %>)" d="M10 15l-5.878 3.09 1.123-6.545L.489 6.91l6.572-.955L10 0l2.939 5.955 6.572.955-4.756 4.635 1.123 6.545z"/>
                                                            </svg>
                                                        <% } else { %>
                                                            <!-- Empty star -->
                                                            <svg class="h-4 w-4 fill-current text-gray-300" viewBox="0 0 20 20">
                                                                <path d="M10 15l-5.878 3.09 1.123-6.545L.489 6.91l6.572-.955L10 0l2.939 5.955 6.572.955-4.756 4.635 1.123 6.545z"/>
                                                            </svg>
                                                        <% } %>
                                                    <% } %>
                                                </div>
                                            </div>
                                        </td>
                                        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                            <%= product.TotalReviews %>
                                        </td>
                                        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                            <div class="mt-2">
                                                <div class="flex justify-between mb-1">
                                                    <span class="text-xs font-medium text-gray-700">Conversion rate</span>
                                                    <span class="text-xs font-medium text-gray-700"><%= product.ConversionRate %>%</span>
                                                </div>
                                                <div class="w-full bg-gray-200 rounded-full h-2.5">
                                                    <div class="bg-blue-600 h-2.5 rounded-full conversion-bar" data-conversion="<%= product.ConversionRate %>"></div>
                                                </div>
                                            </div>
                                        </td>
                                    </tr>
                                <% } %>
                            <% } else { %>
                                <tr>
                                    <td colspan="4" class="px-6 py-4 text-center text-gray-500">
                                        No product performance data available.
                                    </td>
                                </tr>
                            <% } %>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </form>

    <!-- Include Chart.js -->
    <script src="https://cdn.jsdelivr.net/npm/chart.js@3.7.1/dist/chart.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/luxon@3.0.1/build/global/luxon.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/chartjs-adapter-luxon@1.2.0/dist/chartjs-adapter-luxon.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/jquery@3.6.0/dist/jquery.min.js"></script>

    <!-- Chart Initialization -->
    <script type="text/javascript">
        let salesTrendChart = null;
        let categoryRevenueChart = null;
        let topProductsChart = null;
        let inventoryStatusChart = null;

        // Function to initialize or reinitialize all charts
        function initializeCharts() {
            try {
                console.log('Initializing charts with server data');

                // Destroy existing charts if they exist
                if (salesTrendChart) salesTrendChart.destroy();
                if (categoryRevenueChart) categoryRevenueChart.destroy();
                if (topProductsChart) topProductsChart.destroy();
                if (inventoryStatusChart) inventoryStatusChart.destroy();

                // Set conversion rate bar widths
                try {
                    document.querySelectorAll('.conversion-bar').forEach(function (bar) {
                        const conversion = parseFloat(bar.getAttribute('data-conversion') || 0);
                        const width = Math.min(conversion, 100);
                        console.log('Setting conversion bar width to: ' + width + '%');
                        bar.style.width = width + '%';
                    });
                } catch (conversionError) {
                    console.error('Error setting conversion bars:', conversionError);
                }

                // Access the chart data from global variables set by server
                // Default to empty arrays if data is missing or invalid
                try {
                    var salesTrendLabels = Array.isArray(chartData.salesTrendLabels) ? chartData.salesTrendLabels : [];
                    var salesTrendData = Array.isArray(chartData.salesTrendData) ? chartData.salesTrendData : [];
                    var categoryNames = Array.isArray(chartData.categoryNames) ? chartData.categoryNames : [];
                    var categoryRevenueData = Array.isArray(chartData.categoryRevenueData) ? chartData.categoryRevenueData : [];
                    var topProductNames = Array.isArray(chartData.topProductNames) ? chartData.topProductNames : [];
                    var topProductQuantities = Array.isArray(chartData.topProductQuantities) ? chartData.topProductQuantities : [];
                    var inventoryStatusData = Array.isArray(chartData.inventoryStatusData) ? chartData.inventoryStatusData : [0, 0, 0];
                } catch (dataError) {
                    console.error('Error parsing chart data:', dataError);
                    // Fallback to empty values
                    salesTrendLabels = [];
                    salesTrendData = [];
                    categoryNames = [];
                    categoryRevenueData = [];
                    topProductNames = [];
                    topProductQuantities = [];
                    inventoryStatusData = [0, 0, 0];
                }

                console.log('Chart data loaded:', {
                    salesTrendLabels,
                    salesTrendData,
                    categoryNames,
                    categoryRevenueData
                });

                // Sales Trend Chart
                const salesTrendCtx = document.getElementById('salesTrendChart');
                if (salesTrendCtx) {
                    try {
                        // Check if we have actual data
                        if (salesTrendLabels.length === 0) {
                            // Display a "No data" message instead of an empty chart
                            salesTrendCtx.parentNode.innerHTML = '<div class="flex items-center justify-center h-full"><p class="text-gray-500">No data available for the selected time period</p></div>';
                        } else {
                            salesTrendChart = new Chart(salesTrendCtx, {
                                type: 'line',
                                data: {
                                    labels: salesTrendLabels,
                                    datasets: [{
                                        label: 'Revenue',
                                        data: salesTrendData,
                                        backgroundColor: 'rgba(212, 59, 106, 0.2)',
                                        borderColor: 'rgba(212, 59, 106, 1)',
                                        borderWidth: 2,
                                        tension: 0.3,
                                        fill: true
                                    }]
                                },
                                options: {
                                    responsive: true,
                                    maintainAspectRatio: false,
                                    scales: {
                                        y: {
                                            beginAtZero: true,
                                            ticks: {
                                                callback: function (value) {
                                                    return '₱' + value;
                                                }
                                            }
                                        }
                                    },
                                    plugins: {
                                        tooltip: {
                                            callbacks: {
                                                label: function (context) {
                                                    return 'Revenue: ₱' + context.raw;
                                                }
                                            }
                                        }
                                    }
                                }
                            });
                        }
                    } catch (chartError) {
                        console.error('Error creating sales trend chart:', chartError);
                        salesTrendCtx.parentNode.innerHTML = '<div class="flex items-center justify-center h-full"><p class="text-red-500">Unable to load sales trend chart</p></div>';
                    }
                } else {
                    console.error('Sales trend chart canvas not found');
                }

                // Category Revenue Chart
                const categoryRevenueCtx = document.getElementById('categoryRevenueChart');
                if (categoryRevenueCtx) {
                    try {
                        // Check if we have actual data
                        if (categoryNames.length === 0) {
                            // Display a "No data" message instead of an empty chart
                            categoryRevenueCtx.parentNode.innerHTML = '<div class="flex items-center justify-center h-full"><p class="text-gray-500">No data available for the selected time period</p></div>';
                        } else {
                            categoryRevenueChart = new Chart(categoryRevenueCtx, {
                                type: 'pie',
                                data: {
                                    labels: categoryNames,
                                    datasets: [{
                                        data: categoryRevenueData,
                                        backgroundColor: [
                                            'rgba(212, 59, 106, 0.8)',
                                            'rgba(54, 162, 235, 0.8)',
                                            'rgba(75, 192, 192, 0.8)',
                                            'rgba(255, 206, 86, 0.8)',
                                            'rgba(153, 102, 255, 0.8)',
                                            'rgba(255, 159, 64, 0.8)'
                                        ],
                                        borderWidth: 1
                                    }]
                                },
                                options: {
                                    responsive: true,
                                    maintainAspectRatio: false,
                                    plugins: {
                                        tooltip: {
                                            callbacks: {
                                                label: function (context) {
                                                    const value = context.raw;
                                                    const label = context.label || '';
                                                    const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                                    const percentage = Math.round((value / total) * 100);
                                                    return label + ': ₱' + value + ' (' + percentage + '%)';
                                                }
                                            }
                                        }
                                    }
                                }
                            });
                        }
                    } catch (chartError) {
                        console.error('Error creating category revenue chart:', chartError);
                        categoryRevenueCtx.parentNode.innerHTML = '<div class="flex items-center justify-center h-full"><p class="text-red-500">Unable to load category revenue chart</p></div>';
                    }
                } else {
                    console.error('Category revenue chart canvas not found');
                }

                // Top Products Chart
                const topProductsCtx = document.getElementById('topProductsChart');
                if (topProductsCtx) {
                    try {
                        // Check if we have actual data
                        if (topProductNames.length === 0) {
                            // Display a "No data" message instead of an empty chart
                            topProductsCtx.parentNode.innerHTML = '<div class="flex items-center justify-center h-full"><p class="text-gray-500">No data available for the selected time period</p></div>';
                        } else {
                            topProductsChart = new Chart(topProductsCtx, {
                                type: 'bar',
                                data: {
                                    labels: topProductNames,
                                    datasets: [{
                                        label: 'Units Sold',
                                        data: topProductQuantities,
                                        backgroundColor: 'rgba(54, 162, 235, 0.7)',
                                        borderColor: 'rgba(54, 162, 235, 1)',
                                        borderWidth: 1
                                    }]
                                },
                                options: {
                                    indexAxis: 'y',
                                    responsive: true,
                                    maintainAspectRatio: false,
                                    plugins: {
                                        legend: {
                                            display: false
                                        }
                                    },
                                    scales: {
                                        x: {
                                            beginAtZero: true
                                        }
                                    }
                                }
                            });
                        }
                    } catch (chartError) {
                        console.error('Error creating top products chart:', chartError);
                        topProductsCtx.parentNode.innerHTML = '<div class="flex items-center justify-center h-full"><p class="text-red-500">Unable to load top products chart</p></div>';
                    }
                } else {
                    console.error('Top products chart canvas not found');
                }

                // Inventory Status Chart
                const inventoryStatusCtx = document.getElementById('inventoryStatusChart');
                if (inventoryStatusCtx) {
                    try {
                        // Check if we have actual data
                        if (inventoryStatusData.length === 0) {
                            // Display a "No data" message instead of an empty chart
                            inventoryStatusCtx.parentNode.innerHTML = '<div class="flex items-center justify-center h-full"><p class="text-gray-500">No data available for the selected time period</p></div>';
                        } else {
                            inventoryStatusChart = new Chart(inventoryStatusCtx, {
                                type: 'doughnut',
                                data: {
                                    labels: ['In Stock', 'Low Stock', 'Out of Stock'],
                                    datasets: [{
                                        label: 'Inventory Status',
                                        data: inventoryStatusData,
                                        backgroundColor: [
                                            'rgba(75, 192, 192, 0.7)',
                                            'rgba(255, 206, 86, 0.7)',
                                            'rgba(255, 99, 132, 0.7)'
                                        ],
                                        borderColor: [
                                            'rgba(75, 192, 192, 1)',
                                            'rgba(255, 206, 86, 1)',
                                            'rgba(255, 99, 132, 1)'
                                        ],
                                        borderWidth: 1
                                    }]
                                },
                                options: {
                                    responsive: true,
                                    maintainAspectRatio: false,
                                    plugins: {
                                        legend: {
                                            position: 'right'
                                        }
                                    }
                                }
                            });
                        }
                    } catch (chartError) {
                        console.error('Error creating inventory status chart:', chartError);
                        inventoryStatusCtx.parentNode.innerHTML = '<div class="flex items-center justify-center h-full"><p class="text-red-500">Unable to load inventory status chart</p></div>';
                    }
                } else {
                    console.error('Inventory status chart canvas not found');
                }
            } catch (error) {
                console.error('Error initializing charts:', error);
            }
        }

        // Wait for DOM content to be loaded
        document.addEventListener('DOMContentLoaded', function () {
            // Initialize charts when the page loads
            initializeCharts();
        });
    </script>
</asp:Content>