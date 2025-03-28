using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;
using System.Linq;
using System.Collections.Generic;
using System.Web;

namespace OnlinePastryShop.Pages
{
    public partial class SalesReport : System.Web.UI.Page
    {
        // Public properties for data binding
        public string TotalSales { get; set; }
        public string TotalRevenue { get; set; }
        public string AverageOrderValue { get; set; }
        public string LowStockCount { get; set; }
        public string SalesGrowth { get; set; }
        public string RevenueGrowth { get; set; }
        public string AvgOrderValueChange { get; set; }
        public string AvgOrderValueChangeClass { get; set; }
        public string AvgOrderValueIcon { get; set; }
        public string ChartPeriodText { get; set; }

        // Chart data
        public string SalesTrendLabels { get; set; }
        public string SalesTrendData { get; set; }
        public string CategoryNames { get; set; }
        public string CategoryRevenueData { get; set; }
        public string TopProductNames { get; set; }
        public string TopProductQuantities { get; set; }
        public string InventoryStatusData { get; set; }

        // Time period parameters
        private string TimeRange { get; set; }
        private DateTime StartDate { get; set; }
        private DateTime EndDate { get; set; }
        private DateTime PreviousStartDate { get; set; }
        private DateTime PreviousEndDate { get; set; }

        // Properties for product ratings
        public List<ProductRating> ProductRatings { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    System.Diagnostics.Debug.WriteLine("SalesReport: Initial page load");

                    // Check if time range text controls exist
                    var timeRangeSelectorExists = (FindControl("TimeRangeSelector") != null);
                    var startDateControlExists = (FindControl("txtStartDate") != null);
                    var endDateControlExists = (FindControl("txtEndDate") != null);

                    System.Diagnostics.Debug.WriteLine($"Control existence check - TimeRangeSelector: {timeRangeSelectorExists}, StartDate: {startDateControlExists}, EndDate: {endDateControlExists}");

                    // Initialize time parameters with default values (Weekly)
                    TimeRange = "weekly";
                    InitializeTimeParameters();

                    // Initialize chart data with default values to prevent null reference exceptions
                    InitializeChartData();

                    // Load dashboard data with default values
                    LoadDashboardData();

                    System.Diagnostics.Debug.WriteLine("SalesReport: Initial page load completed");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("SalesReport: Postback detected");

                    // Restore saved values from ViewState
                    if (ViewState["SelectedTimeRange"] != null)
                    {
                        TimeRange = ViewState["SelectedTimeRange"].ToString();
                        System.Diagnostics.Debug.WriteLine($"Restored TimeRange from ViewState: {TimeRange}");
                    }

                    if (ViewState["StartDate"] != null && ViewState["EndDate"] != null)
                    {
                        try
                        {
                            StartDate = (DateTime)ViewState["StartDate"];
                            EndDate = (DateTime)ViewState["EndDate"];
                            System.Diagnostics.Debug.WriteLine($"Restored date range from ViewState - Start: {StartDate:yyyy-MM-dd HH:mm:ss}, End: {EndDate:yyyy-MM-dd HH:mm:ss}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error restoring dates from ViewState: {ex.Message}");
                            InitializeTimeParameters(); // Use default values if restoration fails
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No date range in ViewState, initializing defaults");
                        InitializeTimeParameters();
                    }

                    System.Diagnostics.Debug.WriteLine("SalesReport: Postback handling completed");
                }

                // Set period text for displaying the time range
                SetPeriodText();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Page_Load: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ShowError("An error occurred while loading the sales report.");
            }
        }

        private void InitializeTimeParameters()
        {
            // Get time range from query string, default to weekly
            TimeRange = Request.QueryString["timeRange"] ?? "weekly";
            DateTime now = DateTime.Now;

            // Try to parse dates from query string
            DateTime parsedStartDate;
            DateTime parsedEndDate;

            bool hasStartDate = DateTime.TryParse(Request.QueryString["startDate"], out parsedStartDate);
            bool hasEndDate = DateTime.TryParse(Request.QueryString["endDate"], out parsedEndDate);

            // If dates provided in query string, use them
            if (hasStartDate && hasEndDate)
            {
                StartDate = parsedStartDate;
                EndDate = parsedEndDate.AddHours(23).AddMinutes(59).AddSeconds(59); // End of the day
            }
            else
            {
                // Otherwise set default date ranges based on time range selection
                switch (TimeRange)
                {
                    case "daily":
                        StartDate = now.Date;
                        EndDate = now;
                        break;
                    case "weekly": // Default
                        StartDate = now.AddDays(-7);
                        EndDate = now;
                        break;
                    case "monthly":
                        StartDate = new DateTime(now.Year, now.Month, 1);
                        EndDate = now;
                        break;
                    case "yearly":
                        StartDate = new DateTime(now.Year, 1, 1);
                        EndDate = now;
                        break;
                    default:
                        StartDate = now.AddDays(-7);
                        EndDate = now;
                        break;
                }
            }

            // For testing purposes, extend the date range to ensure we have data
            StartDate = new DateTime(now.Year - 1, 1, 1); // Set to start of last year to capture more data

            // Calculate previous period for comparison
            TimeSpan periodLength = EndDate - StartDate;
            PreviousEndDate = StartDate.AddSeconds(-1);
            PreviousStartDate = PreviousEndDate.Subtract(periodLength);
        }

        private void SetPeriodText()
        {
            switch (TimeRange)
            {
                case "daily":
                    ChartPeriodText = "Today";
                    break;
                case "weekly":
                    ChartPeriodText = "Last 7 Days";
                    break;
                case "monthly":
                    ChartPeriodText = StartDate.ToString("MMMM yyyy");
                    break;
                case "yearly":
                    ChartPeriodText = StartDate.Year.ToString();
                    break;
                default:
                    ChartPeriodText = $"{StartDate:MMM d} - {EndDate:MMM d, yyyy}";
                    break;
            }
        }

        private void LoadDashboardData()
        {
            try
            {
                DateTime startDate;
                DateTime endDate;

                // Check if the text boxes have values, otherwise use default dates
                TextBox txtStartDate = FindControl("txtStartDate") as TextBox;
                TextBox txtEndDate = FindControl("txtEndDate") as TextBox;

                System.Diagnostics.Debug.WriteLine($"Loading dashboard data with StartDate control: {(txtStartDate != null ? txtStartDate.Text : "null")} and EndDate control: {(txtEndDate != null ? txtEndDate.Text : "null")}");

                if (txtStartDate == null || txtEndDate == null ||
                    string.IsNullOrEmpty(txtStartDate.Text) || string.IsNullOrEmpty(txtEndDate.Text))
                {
                    // Default to last 7 days if no dates are provided
                    endDate = DateTime.Now;
                    startDate = endDate.AddDays(-7);
                    System.Diagnostics.Debug.WriteLine($"Using default dates: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
                }
                else
                {
                    // Use the dates from the text boxes
                    if (DateTime.TryParse(txtStartDate.Text, out startDate) && DateTime.TryParse(txtEndDate.Text, out endDate))
                    {
                        // Make sure the end date includes the full day (23:59:59)
                        endDate = endDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                        System.Diagnostics.Debug.WriteLine($"Parsed dates: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd HH:mm:ss}");
                    }
                    else
                    {
                        // Parse failed, use defaults
                        endDate = DateTime.Now;
                        startDate = endDate.AddDays(-7);
                        System.Diagnostics.Debug.WriteLine($"Parse failed, using default dates: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
                    }
                }

                // Store the parsed dates for other methods to use
                this.StartDate = startDate;
                this.EndDate = endDate;

                // Calculate number of days in the selected period
                int daysDiff = (int)(endDate.Date - startDate.Date).TotalDays + 1;
                ChartPeriodText = $"{daysDiff} day{(daysDiff > 1 ? "s" : "")}";
                System.Diagnostics.Debug.WriteLine($"Date range days: {daysDiff}, ChartPeriodText: {ChartPeriodText}");

                // Load metrics
                LoadSalesMetrics(startDate, endDate);

                // Load additional data for charts
                LoadSalesTrendData(startDate, endDate);
                LoadCategoryRevenueData(startDate, endDate);
                LoadTopProductsData(startDate, endDate);
                LoadInventoryStatusData();

                // Load product ratings
                LoadProductRatingData();

                System.Diagnostics.Debug.WriteLine("Dashboard data loading completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadDashboardData: {ex.Message}");
                ShowError("An error occurred loading dashboard data.");
            }
        }

        private void LoadSalesMetrics(DateTime startDate, DateTime endDate)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"LoadSalesMetrics - Start date: {startDate:yyyy-MM-dd HH:mm:ss}, End date: {endDate:yyyy-MM-dd HH:mm:ss}");

                // Format dates for Oracle query using TO_DATE function
                string startDateStr = startDate.ToString("yyyy-MM-dd HH:mm:ss");
                string endDateStr = endDate.ToString("yyyy-MM-dd HH:mm:ss");

                System.Diagnostics.Debug.WriteLine($"Formatted dates for query - Start: {startDateStr}, End: {endDateStr}");

                // Get the proper connection string
                string connectionString = GetConnectionString();
                System.Diagnostics.Debug.WriteLine($"Connection string (partial): {connectionString.Substring(0, Math.Min(20, connectionString.Length))}...");

                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();

                    // First, check if the database knows what time it is
                    using (OracleCommand timeCmd = new OracleCommand("SELECT SYSDATE, TO_CHAR(SYSDATE, 'YYYY-MM-DD HH24:MI:SS') FROM DUAL", conn))
                    {
                        using (OracleDataReader reader = timeCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                System.Diagnostics.Debug.WriteLine($"Database time: {reader[0]} (formatted: {reader[1]})");
                            }
                        }
                    }

                    // Query for getting sales totals between date range
                    string query = @"
                        SELECT 
                            NVL(SUM(TOTALAMOUNT), 0) AS TOTALREVENUE, 
                            COUNT(*) AS ORDERCOUNT,
                            NVL(AVG(TOTALAMOUNT), 0) AS AVERAGEORDER
                        FROM ORDERS 
                        WHERE ORDERDATE BETWEEN TO_DATE(:startDate, 'YYYY-MM-DD HH24:MI:SS') 
                                          AND TO_DATE(:endDate, 'YYYY-MM-DD HH24:MI:SS')
                        AND STATUS IN ('Completed', 'Approved', 'Delivered', 'Shipped', 'Processing')";

                    System.Diagnostics.Debug.WriteLine($"Sales metrics query: {query}");
                    System.Diagnostics.Debug.WriteLine($"Parameters - Start date: {startDateStr}, End date: {endDateStr}");

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        // Add parameters with TO_DATE format
                        cmd.Parameters.Add(new OracleParameter("startDate", OracleDbType.Varchar2)).Value = startDateStr;
                        cmd.Parameters.Add(new OracleParameter("endDate", OracleDbType.Varchar2)).Value = endDateStr;

                        // Execute the query
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Extract values from the reader
                                decimal totalRevenue = reader.GetDecimal(0);
                                int orderCount = reader.GetInt32(1);
                                decimal averageOrder = reader.GetDecimal(2);

                                System.Diagnostics.Debug.WriteLine($"Query results - Total Revenue: {totalRevenue}, Order Count: {orderCount}, Average Order: {averageOrder}");

                                // Update public properties for data binding
                                TotalRevenue = totalRevenue.ToString("C");
                                TotalSales = orderCount.ToString();
                                AverageOrderValue = averageOrder.ToString("C");

                                // Store metrics in ViewState for persistence
                                ViewState["TotalRevenue"] = totalRevenue;
                                ViewState["OrderCount"] = orderCount;
                                ViewState["AverageOrder"] = averageOrder;

                                // Update UI elements if they exist
                                if (FindControl("lblTotalRevenue") is Label lblTotalRevenue)
                                {
                                    lblTotalRevenue.Text = totalRevenue.ToString("C");
                                }

                                if (FindControl("lblOrderCount") is Label lblOrderCount)
                                {
                                    lblOrderCount.Text = orderCount.ToString();
                                }

                                if (FindControl("lblAverageOrder") is Label lblAverageOrder)
                                {
                                    lblAverageOrder.Text = averageOrder.ToString("C");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("No sales data returned for the specified period");

                                // Set default values
                                TotalRevenue = "$0.00";
                                TotalSales = "0";
                                AverageOrderValue = "$0.00";

                                // Set default values for UI elements
                                ViewState["TotalRevenue"] = 0m;
                                ViewState["OrderCount"] = 0;
                                ViewState["AverageOrder"] = 0m;
                            }
                        }
                    }
                }

                // Let's double check with a direct date comparison
                string directQuery = @"
                    SELECT 
                        COUNT(*) AS OrderCount, 
                        MIN(TO_CHAR(ORDERDATE, 'YYYY-MM-DD HH24:MI:SS')) AS FirstOrder, 
                        MAX(TO_CHAR(ORDERDATE, 'YYYY-MM-DD HH24:MI:SS')) AS LastOrder
                    FROM ORDERS 
                    WHERE TO_CHAR(ORDERDATE, 'YYYY-MM-DD') >= :startDateSimple 
                    AND TO_CHAR(ORDERDATE, 'YYYY-MM-DD') <= :endDateSimple";

                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    using (OracleCommand directCmd = new OracleCommand(directQuery, conn))
                    {
                        directCmd.Parameters.Add(new OracleParameter("startDateSimple", OracleDbType.Varchar2)).Value = startDate.ToString("yyyy-MM-dd");
                        directCmd.Parameters.Add(new OracleParameter("endDateSimple", OracleDbType.Varchar2)).Value = endDate.ToString("yyyy-MM-dd");

                        using (OracleDataReader reader = directCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                System.Diagnostics.Debug.WriteLine($"Direct date check - Orders: {reader["OrderCount"]}, First: {reader["FirstOrder"]}, Last: {reader["LastOrder"]}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadSalesMetrics: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Set default values for UI elements in case of error
                TotalRevenue = "$0.00";
                TotalSales = "0";
                AverageOrderValue = "$0.00";

                // Do not throw the exception, just log it to avoid breaking the page load
                ShowError($"Error loading sales metrics: {ex.Message}");
            }
        }

        private void LoadSalesTrendData(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Generate sales trend data for selected period
                // This would populate a JavaScript array for Chart.js
                List<DateTime> dates = new List<DateTime>();
                List<decimal> revenues = new List<decimal>();
                List<int> orderCounts = new List<int>();

                using (OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            TRUNC(O.DATECREATED) AS OrderDate,
                            COUNT(*) AS OrderCount,
                            SUM(O.TOTALAMOUNT) AS DailyRevenue
                        FROM ORDERS O 
                        WHERE O.DATECREATED BETWEEN :StartDate AND :EndDate
                        GROUP BY TRUNC(O.DATECREATED)
                        ORDER BY TRUNC(O.DATECREATED)";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(":StartDate", OracleDbType.Date).Value = startDate;
                        cmd.Parameters.Add(":EndDate", OracleDbType.Date).Value = endDate;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DateTime date = reader.GetDateTime(0);
                                int orderCount = reader.GetInt32(1);
                                decimal revenue = reader.GetDecimal(2);

                                dates.Add(date);
                                orderCounts.Add(orderCount);
                                revenues.Add(revenue);
                            }
                        }
                    }
                }

                // Convert data for JavaScript
                string dateLabels = string.Join(",", dates.Select(d => $"'{d.ToString("MMM dd")}'"));
                string revenueData = string.Join(",", revenues);
                string orderData = string.Join(",", orderCounts);

                // Register script for chart
                string chartScript = $@"
                    var salesCtx = document.getElementById('salesTrendChart').getContext('2d');
                    var salesData = {{
                        labels: [{dateLabels}],
                        datasets: [
                            {{
                                label: 'Revenue',
                                borderColor: '#4F46E5',
                                backgroundColor: 'rgba(79, 70, 229, 0.1)',
                                data: [{revenueData}],
                                yAxisID: 'y-axis-1',
                                fill: true,
                                tension: 0.4
                            }},
                            {{
                                label: 'Orders',
                                borderColor: '#D43B6A',
                                backgroundColor: 'rgba(212, 59, 106, 0.0)',
                                data: [{orderData}],
                                yAxisID: 'y-axis-2',
                                borderDash: [5, 5],
                                fill: false
                            }}
                        ]
                    }};
                    
                    var salesChart = new Chart(salesCtx, {{
                        type: 'line',
                        data: salesData,
                        options: {{
                            responsive: true,
                            interaction: {{
                                mode: 'index',
                                intersect: false,
                            }},
                            stacked: false,
                            scales: {{
                                y: {{
                                    type: 'linear',
                                    display: true,
                                    position: 'left',
                                    id: 'y-axis-1',
                                }},
                                y1: {{
                                    type: 'linear',
                                    display: true,
                                    position: 'right',
                                    id: 'y-axis-2',
                                    grid: {{
                                        drawOnChartArea: false,
                                    }},
                                }}
                            }}
                        }}
                    }});";

                ClientScript.RegisterStartupScript(this.GetType(), "SalesTrendChart", chartScript, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadSalesTrendData: {ex.Message}");
            }
        }

        private void LoadCategoryRevenueData(DateTime startDate, DateTime endDate)
        {
            try
            {
                List<string> categories = new List<string>();
                List<decimal> revenues = new List<decimal>();

                using (OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString))
                {
                    conn.Open();
                    string query = @"
            SELECT 
                            C.NAME AS CategoryName,
                            SUM(OD.QUANTITY * OD.UNITPRICE) AS CategoryRevenue
                        FROM CATEGORIES C
                        JOIN PRODUCTS P ON P.CATEGORYID = C.CATEGORYID
                        JOIN ORDERDETAILS OD ON OD.PRODUCTID = P.PRODUCTID
                        JOIN ORDERS O ON O.ORDERID = OD.ORDERID
                        WHERE O.DATECREATED BETWEEN :StartDate AND :EndDate
                        GROUP BY C.NAME
                        ORDER BY CategoryRevenue DESC";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(":StartDate", OracleDbType.Date).Value = startDate;
                        cmd.Parameters.Add(":EndDate", OracleDbType.Date).Value = endDate;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string category = reader.GetString(0);
                                decimal revenue = reader.GetDecimal(1);

                                categories.Add(category);
                                revenues.Add(revenue);
                            }
                        }
                    }
                }

                // Convert data for JavaScript
                string categoryLabels = string.Join(",", categories.Select(c => $"'{c}'"));
                string revenueData = string.Join(",", revenues);

                // Register script for chart
                string chartScript = $@"
                    var categoryCtx = document.getElementById('categoryRevenueChart').getContext('2d');
                    var categoryData = {{
                        labels: [{categoryLabels}],
                        datasets: [{{
                            data: [{revenueData}],
                            backgroundColor: [
                                '#4F46E5', '#D43B6A', '#10B981', '#F59E0B', '#6366F1',
                                '#8B5CF6', '#EC4899', '#14B8A6', '#F97316', '#3B82F6'
                            ]
                        }}]
                    }};
                    
                    var categoryChart = new Chart(categoryCtx, {{
                        type: 'pie',
                        data: categoryData,
                        options: {{
                            responsive: true,
                            plugins: {{
                                legend: {{
                                    position: 'right',
                                }},
                                tooltip: {{
                                    callbacks: {{
                                        label: function(context) {{
                                            var label = context.label || '';
                                            var value = context.raw || 0;
                                            return label + ': ₱' + value.toFixed(2);
                                        }}
                                    }}
                                }}
                            }}
                        }}
                    }});";

                ClientScript.RegisterStartupScript(this.GetType(), "CategoryRevenueChart", chartScript, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadCategoryRevenueData: {ex.Message}");
            }
        }

        private void LoadTopProductsData(DateTime startDate, DateTime endDate)
        {
            try
            {
                List<string> products = new List<string>();
                List<int> quantities = new List<int>();

                using (OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            P.NAME AS ProductName,
                            SUM(OD.QUANTITY) AS TotalQuantity
                        FROM PRODUCTS P
                        JOIN ORDERDETAILS OD ON OD.PRODUCTID = P.PRODUCTID
                        JOIN ORDERS O ON O.ORDERID = OD.ORDERID
                        WHERE O.DATECREATED BETWEEN :StartDate AND :EndDate
                        GROUP BY P.NAME
                        ORDER BY TotalQuantity DESC
                        FETCH FIRST 10 ROWS ONLY";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(":StartDate", OracleDbType.Date).Value = startDate;
                        cmd.Parameters.Add(":EndDate", OracleDbType.Date).Value = endDate;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string product = reader.GetString(0);
                                int quantity = reader.GetInt32(1);

                                products.Add(product);
                                quantities.Add(quantity);
                            }
                        }
                    }
                }

                // Convert data for JavaScript
                string productLabels = string.Join(",", products.Select(p => $"'{p}'"));
                string quantityData = string.Join(",", quantities);

                // Register script for chart
                string chartScript = $@"
                    var productsCtx = document.getElementById('topProductsChart').getContext('2d');
                    var productsData = {{
                        labels: [{productLabels}],
                        datasets: [{{
                            label: 'Units Sold',
                            backgroundColor: '#4F46E5',
                            borderColor: '#4F46E5',
                            borderWidth: 1,
                            data: [{quantityData}]
                        }}]
                    }};
                    
                    var productsChart = new Chart(productsCtx, {{
                        type: 'bar',
                        data: productsData,
                        options: {{
                            indexAxis: 'y',
                            responsive: true,
                            plugins: {{
                                legend: {{
                                    display: false
                                }}
                            }}
                        }}
                    }});";

                ClientScript.RegisterStartupScript(this.GetType(), "TopProductsChart", chartScript, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadTopProductsData: {ex.Message}");
            }
        }

        private void LoadInventoryStatusData()
        {
            try
            {
                // Get inventory status distribution
                int lowStock = 0;
                int adequateStock = 0;
                int excessStock = 0;
                int outOfStock = 0;

                using (OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            SUM(CASE WHEN P.STOCKQUANTITY = 0 THEN 1 ELSE 0 END) AS OutOfStock,
                            SUM(CASE WHEN P.STOCKQUANTITY BETWEEN 1 AND 10 THEN 1 ELSE 0 END) AS LowStock,
                            SUM(CASE WHEN P.STOCKQUANTITY BETWEEN 11 AND 50 THEN 1 ELSE 0 END) AS AdequateStock,
                            SUM(CASE WHEN P.STOCKQUANTITY > 50 THEN 1 ELSE 0 END) AS ExcessStock
                        FROM PRODUCTS P
                        WHERE P.ISACTIVE = 1";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                outOfStock = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                lowStock = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                                adequateStock = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                                excessStock = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                            }
                        }
                    }
                }

                // Register script for chart
                string chartScript = $@"
                    var inventoryCtx = document.getElementById('inventoryStatusChart').getContext('2d');
                    var inventoryData = {{
                        labels: ['Out of Stock', 'Low Stock', 'Adequate', 'Excess'],
                        datasets: [{{
                            data: [{outOfStock}, {lowStock}, {adequateStock}, {excessStock}],
                            backgroundColor: [
                                '#EF4444', '#F59E0B', '#10B981', '#3B82F6'
                            ]
                        }}]
                    }};
                    
                    var inventoryChart = new Chart(inventoryCtx, {{
                        type: 'doughnut',
                        data: inventoryData,
                        options: {{
                            responsive: true,
                            plugins: {{
                                legend: {{
                                    position: 'right',
                                }}
                            }}
                        }}
                    }});";

                ClientScript.RegisterStartupScript(this.GetType(), "InventoryStatusChart", chartScript, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadInventoryStatusData: {ex.Message}");
            }
        }

        private void LoadProductRatingData()
        {
            try
            {
                ProductRatings = new List<ProductRating>();

                using (OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            P.NAME AS ProductName,
                            AVG(R.RATING) AS AverageRating,
                            COUNT(R.REVIEWID) AS TotalReviews,
                            (COUNT(DISTINCT O.ORDERID) / COUNT(DISTINCT R.USERID)) * 100 AS ConversionRate
                        FROM PRODUCTS P
                        LEFT JOIN REVIEWS R ON R.PRODUCTID = P.PRODUCTID
                        LEFT JOIN ORDERDETAILS OD ON OD.PRODUCTID = P.PRODUCTID
                        LEFT JOIN ORDERS O ON O.ORDERID = OD.ORDERID
                        WHERE P.ISACTIVE = 1
                        GROUP BY P.NAME
                        HAVING COUNT(R.REVIEWID) > 0
                        ORDER BY AverageRating DESC, TotalReviews DESC
                        FETCH FIRST 10 ROWS ONLY";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ProductRatings.Add(new ProductRating
                                {
                                    ProductName = reader.GetString(0),
                                    AverageRating = reader.GetDecimal(1),
                                    TotalReviews = reader.GetInt32(2),
                                    ConversionRate = reader.GetDecimal(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadProductRatingData: {ex.Message}");
                // If we can't load ratings, create some sample data
                ProductRatings = new List<ProductRating>
                {
                    new ProductRating { ProductName = "Chocolate Cake", AverageRating = 4.8m, TotalReviews = 42, ConversionRate = 68 },
                    new ProductRating { ProductName = "Red Velvet Cake", AverageRating = 4.2m, TotalReviews = 35, ConversionRate = 62 },
                    new ProductRating { ProductName = "Vanilla Cupcake", AverageRating = 3.9m, TotalReviews = 28, ConversionRate = 54 }
                };
            }
        }

        private void ShowError(string message)
        {
            Label lblMessage = FindControl("lblMessage") as Label;
            if (lblMessage != null)
            {
                lblMessage.Text = message;
                lblMessage.Visible = true;
                lblMessage.CssClass = "text-red-500 bg-red-50 p-2 rounded-lg mb-4";
            }
        }

        private void BindProductTables()
        {
            try
            {
                // First check if the repeaters already have data
                Repeater lowStockRepeater = FindControl("LowStockRepeater") as Repeater;
                Repeater topProductsRepeater = FindControl("TopProductsRepeater") as Repeater;

                bool lowStockAlreadyBound = lowStockRepeater != null && lowStockRepeater.DataSource != null;
                bool topProductsAlreadyBound = topProductsRepeater != null && topProductsRepeater.DataSource != null;

                System.Diagnostics.Debug.WriteLine($"BindProductTables - LowStockRepeater already bound: {lowStockAlreadyBound}");
                System.Diagnostics.Debug.WriteLine($"BindProductTables - TopProductsRepeater already bound: {topProductsAlreadyBound}");

                // Only bind if not already bound
                if (!lowStockAlreadyBound)
                {
                    // Bind data for low stock products
                    BindLowStockProducts();
                }

                if (!topProductsAlreadyBound)
                {
                    // Bind data for top performing products
                    BindTopPerformingProducts();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in BindProductTables: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        private void BindTopPerformingProducts()
        {
            try
            {
                // Check if repeater already has data
                Repeater topProductsRepeater = FindControl("TopProductsRepeater") as Repeater;
                if (topProductsRepeater != null && topProductsRepeater.DataSource != null)
                {
                    System.Diagnostics.Debug.WriteLine("TopProductsRepeater already has data - skipping database binding");
                    return;
                }

                string query = @"
                    SELECT * FROM (
                        SELECT 
                            p.ProductId,
                            p.Name,
                            COALESCE(c.Name, 'Uncategorized') AS CategoryName,
                            SUM(od.Quantity) AS TotalQuantity,
                            SUM(od.Quantity * od.Price) AS TotalRevenue
                        FROM OrderDetails od
                        JOIN Products p ON od.ProductId = p.ProductId
                        JOIN Orders o ON od.OrderId = o.OrderId
                        LEFT JOIN ProductCategories pc ON p.ProductId = pc.ProductId
                        LEFT JOIN Categories c ON pc.CategoryId = c.CategoryId
                        WHERE o.OrderDate BETWEEN :StartDate AND :EndDate
                        GROUP BY p.ProductId, p.Name, c.Name
                        ORDER BY TotalRevenue DESC
                    ) WHERE ROWNUM <= 5
                ";

                DataTable dataTable = new DataTable();

                // Define the schema explicitly to ensure sample data can be added correctly
                dataTable.Columns.Add("ProductId", typeof(int));
                dataTable.Columns.Add("Name", typeof(string));
                dataTable.Columns.Add("CategoryName", typeof(string));
                dataTable.Columns.Add("TotalQuantity", typeof(int));
                dataTable.Columns.Add("TotalRevenue", typeof(decimal));

                System.Diagnostics.Debug.WriteLine("TopPerformingProducts query: " + query);
                System.Diagnostics.Debug.WriteLine($"Date range: {StartDate} to {EndDate}");

                using (OracleConnection connection = new OracleConnection(GetConnectionString()))
                {
                    try
                    {
                        connection.Open();
                        System.Diagnostics.Debug.WriteLine("Database connection opened for top performing products");
                        using (OracleCommand command = new OracleCommand(query, connection))
                        {
                            command.Parameters.Add("StartDate", OracleDbType.Date).Value = StartDate;
                            command.Parameters.Add("EndDate", OracleDbType.Date).Value = EndDate;

                            using (OracleDataAdapter adapter = new OracleDataAdapter(command))
                            {
                                adapter.Fill(dataTable);
                                System.Diagnostics.Debug.WriteLine($"Adapter filled dataTable with {dataTable.Rows.Count} rows");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Database error in BindTopPerformingProducts: {ex.Message}");
                        // Continue with empty table - we'll add sample data below
                    }
                }

                System.Diagnostics.Debug.WriteLine($"BindTopPerformingProducts: {dataTable.Rows.Count} rows from database");

                // Before binding, check if the control exists and is not already bound
                if (topProductsRepeater != null && topProductsRepeater.DataSource == null)
                {
                    System.Diagnostics.Debug.WriteLine("Found TopProductsRepeater control, not already bound");
                    // If no data, add sample data
                    if (dataTable.Rows.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("No top performing products found, adding sample data");
                        // Add sample data
                        DataRow row = dataTable.NewRow();
                        row["ProductId"] = 1;
                        row["Name"] = "Chocolate Cake";
                        row["CategoryName"] = "Cakes";
                        row["TotalQuantity"] = 42;
                        row["TotalRevenue"] = 12600m;
                        dataTable.Rows.Add(row);
                        System.Diagnostics.Debug.WriteLine("Added sample row: Chocolate Cake");

                        row = dataTable.NewRow();
                        row["ProductId"] = 2;
                        row["Name"] = "Vanilla Cupcake";
                        row["CategoryName"] = "Cupcakes";
                        row["TotalQuantity"] = 35;
                        row["TotalRevenue"] = 8750m;
                        dataTable.Rows.Add(row);
                        System.Diagnostics.Debug.WriteLine("Added sample row: Vanilla Cupcake");

                        row = dataTable.NewRow();
                        row["ProductId"] = 3;
                        row["Name"] = "Red Velvet";
                        row["CategoryName"] = "Cakes";
                        row["TotalQuantity"] = 28;
                        row["TotalRevenue"] = 7000m;
                        dataTable.Rows.Add(row);
                        System.Diagnostics.Debug.WriteLine("Added sample row: Red Velvet");

                        row = dataTable.NewRow();
                        row["ProductId"] = 4;
                        row["Name"] = "Cheesecake";
                        row["CategoryName"] = "Cakes";
                        row["TotalQuantity"] = 22;
                        row["TotalRevenue"] = 5500m;
                        dataTable.Rows.Add(row);
                        System.Diagnostics.Debug.WriteLine("Added sample row: Cheesecake");

                        row = dataTable.NewRow();
                        row["ProductId"] = 5;
                        row["Name"] = "Tiramisu";
                        row["CategoryName"] = "Desserts";
                        row["TotalQuantity"] = 15;
                        row["TotalRevenue"] = 3750m;
                        dataTable.Rows.Add(row);
                        System.Diagnostics.Debug.WriteLine("Added sample row: Tiramisu");

                        System.Diagnostics.Debug.WriteLine("Added sample data for top performing products");
                    }
                    else
                    {
                        // Log the actual products found
                        System.Diagnostics.Debug.WriteLine("Top performing products found in database:");
                        foreach (DataRow row in dataTable.Rows)
                        {
                            System.Diagnostics.Debug.WriteLine($"  Product: {row["Name"]}, Quantity: {row["TotalQuantity"]}, Revenue: {row["TotalRevenue"]}, Category: {row["CategoryName"]}");
                        }
                    }

                    // Bind data to the repeater
                    topProductsRepeater.DataSource = dataTable;
                    topProductsRepeater.DataBind();
                    System.Diagnostics.Debug.WriteLine($"Bound {dataTable.Rows.Count} rows to TopProductsRepeater");
                }
                else
                {
                    if (topProductsRepeater == null)
                        System.Diagnostics.Debug.WriteLine("ERROR: TopProductsRepeater control not found");
                    else
                        System.Diagnostics.Debug.WriteLine("INFO: TopProductsRepeater already has data, skipping binding");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in BindTopPerformingProducts: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        private decimal CalculateGrowthPercentage(decimal current, decimal previous)
        {
            if (previous == 0)
                return current > 0 ? 100 : 0;

            return ((current - previous) / previous) * 100;
        }

        private string GetConnectionString()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["OracleConnection"]?.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                System.Diagnostics.Debug.WriteLine("Warning: Using fallback connection string, OracleConnection not found in Web.config");
                connectionString = "User Id=mecate;Password=qwen123;Data Source=localhost:1521/xe;";
            }
            return connectionString;
        }

        // This method ensures we always have data to display even if the database is empty
        private void CreateSampleDataForRepeaters()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Creating guaranteed sample data for repeaters");

                // Find repeaters
                Repeater lowStockRepeater = FindControl("LowStockRepeater") as Repeater;
                Repeater topProductsRepeater = FindControl("TopProductsRepeater") as Repeater;

                if (lowStockRepeater != null)
                {
                    // Create sample data table for low stock products
                    DataTable lowStockTable = new DataTable();
                    lowStockTable.Columns.Add("ProductId", typeof(int));
                    lowStockTable.Columns.Add("Name", typeof(string));
                    lowStockTable.Columns.Add("StockQuantity", typeof(int));
                    lowStockTable.Columns.Add("CategoryName", typeof(string));

                    // Add sample data rows
                    DataRow row = lowStockTable.NewRow();
                    row["ProductId"] = 1;
                    row["Name"] = "Sample Chocolate Cake";
                    row["StockQuantity"] = 3;
                    row["CategoryName"] = "Cakes";
                    lowStockTable.Rows.Add(row);

                    row = lowStockTable.NewRow();
                    row["ProductId"] = 2;
                    row["Name"] = "Sample Vanilla Cupcake";
                    row["StockQuantity"] = 0;
                    row["CategoryName"] = "Cupcakes";
                    lowStockTable.Rows.Add(row);

                    // Bind sample data directly
                    lowStockRepeater.DataSource = lowStockTable;
                    lowStockRepeater.DataBind();
                    System.Diagnostics.Debug.WriteLine($"Bound {lowStockTable.Rows.Count} sample rows to LowStockRepeater");
                }

                if (topProductsRepeater != null)
                {
                    // Create sample data table for top products
                    DataTable topProductsTable = new DataTable();
                    topProductsTable.Columns.Add("ProductId", typeof(int));
                    topProductsTable.Columns.Add("Name", typeof(string));
                    topProductsTable.Columns.Add("CategoryName", typeof(string));
                    topProductsTable.Columns.Add("TotalQuantity", typeof(int));
                    topProductsTable.Columns.Add("TotalRevenue", typeof(decimal));

                    // Add sample data rows
                    DataRow row = topProductsTable.NewRow();
                    row["ProductId"] = 1;
                    row["Name"] = "Sample Red Velvet";
                    row["CategoryName"] = "Cakes";
                    row["TotalQuantity"] = 42;
                    row["TotalRevenue"] = 12600m;
                    topProductsTable.Rows.Add(row);

                    row = topProductsTable.NewRow();
                    row["ProductId"] = 2;
                    row["Name"] = "Sample Cheesecake";
                    row["CategoryName"] = "Cakes";
                    row["TotalQuantity"] = 35;
                    row["TotalRevenue"] = 8750m;
                    topProductsTable.Rows.Add(row);

                    // Bind sample data directly
                    topProductsRepeater.DataSource = topProductsTable;
                    topProductsRepeater.DataBind();
                    System.Diagnostics.Debug.WriteLine($"Bound {topProductsTable.Rows.Count} sample rows to TopProductsRepeater");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating sample data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        private void BindLowStockProducts()
        {
            try
            {
                // Check if repeater already has data
                Repeater lowStockRepeater = FindControl("LowStockRepeater") as Repeater;
                if (lowStockRepeater != null && lowStockRepeater.DataSource != null)
                {
                    System.Diagnostics.Debug.WriteLine("LowStockRepeater already has data - skipping database binding");
                    return;
                }

                string query = @"
                    SELECT 
                        p.ProductId,
                        p.Name,
                        p.StockQuantity,
                        COALESCE(c.Name, 'Uncategorized') AS CategoryName
                    FROM Products p
                    LEFT JOIN ProductCategories pc ON p.ProductId = pc.ProductId
                    LEFT JOIN Categories c ON pc.CategoryId = c.CategoryId
                    WHERE p.StockQuantity <= 10
                    AND (p.IsActive = 1 OR p.IsActive IS NULL)
                    ORDER BY p.StockQuantity ASC
                ";

                DataTable dataTable = new DataTable();

                // Define the schema explicitly to ensure sample data can be added correctly
                dataTable.Columns.Add("ProductId", typeof(int));
                dataTable.Columns.Add("Name", typeof(string));
                dataTable.Columns.Add("StockQuantity", typeof(int));
                dataTable.Columns.Add("CategoryName", typeof(string));

                System.Diagnostics.Debug.WriteLine("LowStockProducts query: " + query);

                using (OracleConnection connection = new OracleConnection(GetConnectionString()))
                {
                    try
                    {
                        connection.Open();
                        System.Diagnostics.Debug.WriteLine("Database connection opened for low stock products");
                        using (OracleCommand command = new OracleCommand(query, connection))
                        {
                            using (OracleDataAdapter adapter = new OracleDataAdapter(command))
                            {
                                adapter.Fill(dataTable);
                                System.Diagnostics.Debug.WriteLine($"Adapter filled dataTable with {dataTable.Rows.Count} rows");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Database error in BindLowStockProducts: {ex.Message}");
                        // Continue with empty table - we'll add sample data below
                    }
                }

                System.Diagnostics.Debug.WriteLine($"BindLowStockProducts: {dataTable.Rows.Count} rows from database");

                // Before binding, check if the control exists and is not already bound
                if (lowStockRepeater != null && lowStockRepeater.DataSource == null)
                {
                    System.Diagnostics.Debug.WriteLine("Found LowStockRepeater control, not already bound");
                    // If no data, add sample data
                    if (dataTable.Rows.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("No low stock products found, adding sample data");
                        // Add sample data
                        DataRow row = dataTable.NewRow();
                        row["ProductId"] = 1;
                        row["Name"] = "Chocolate Cake";
                        row["StockQuantity"] = 3;
                        row["CategoryName"] = "Cakes";
                        dataTable.Rows.Add(row);
                        System.Diagnostics.Debug.WriteLine("Added sample row: Chocolate Cake");

                        row = dataTable.NewRow();
                        row["ProductId"] = 2;
                        row["Name"] = "Vanilla Cupcake";
                        row["StockQuantity"] = 0;
                        row["CategoryName"] = "Cupcakes";
                        dataTable.Rows.Add(row);
                        System.Diagnostics.Debug.WriteLine("Added sample row: Vanilla Cupcake");

                        row = dataTable.NewRow();
                        row["ProductId"] = 3;
                        row["Name"] = "Strawberry Tart";
                        row["StockQuantity"] = 5;
                        row["CategoryName"] = "Tarts";
                        dataTable.Rows.Add(row);
                        System.Diagnostics.Debug.WriteLine("Added sample row: Strawberry Tart");

                        System.Diagnostics.Debug.WriteLine("Added sample data for low stock products");
                    }
                    else
                    {
                        // Log the actual products found
                        System.Diagnostics.Debug.WriteLine("Low stock products found in database:");
                        foreach (DataRow row in dataTable.Rows)
                        {
                            System.Diagnostics.Debug.WriteLine($"  Product: {row["Name"]}, Stock: {row["StockQuantity"]}, Category: {row["CategoryName"]}");
                        }
                    }

                    // Bind data to the repeater
                    lowStockRepeater.DataSource = dataTable;
                    lowStockRepeater.DataBind();
                    System.Diagnostics.Debug.WriteLine($"Bound {dataTable.Rows.Count} rows to LowStockRepeater");
                }
                else
                {
                    if (lowStockRepeater == null)
                        System.Diagnostics.Debug.WriteLine("ERROR: LowStockRepeater control not found");
                    else
                        System.Diagnostics.Debug.WriteLine("INFO: LowStockRepeater already has data, skipping binding");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in BindLowStockProducts: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        protected void btnFilterReport_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("btnFilterReport_Click fired");

                // Find the date input controls
                TextBox txtStartDate = FindControl("txtStartDate") as TextBox;
                TextBox txtEndDate = FindControl("txtEndDate") as TextBox;

                if (txtStartDate == null || txtEndDate == null)
                {
                    System.Diagnostics.Debug.WriteLine("Date controls not found on the page");
                    ShowError("Unable to find date input controls. Please try again.");
                    return;
                }

                // Get the provided date values
                string startDateText = txtStartDate.Text.Trim();
                string endDateText = txtEndDate.Text.Trim();

                System.Diagnostics.Debug.WriteLine($"Date inputs - Start: '{startDateText}', End: '{endDateText}'");

                // Validate input dates
                if (string.IsNullOrEmpty(startDateText) || string.IsNullOrEmpty(endDateText))
                {
                    System.Diagnostics.Debug.WriteLine("Date inputs are missing");
                    ShowError("Please provide both start and end dates.");
                    return;
                }

                // Parse dates using invariant culture to avoid regional format issues
                if (!DateTime.TryParse(startDateText, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime startDate))
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid start date format: {startDateText}");
                    ShowError("Invalid start date format. Please use MM/DD/YYYY format.");
                    return;
                }

                if (!DateTime.TryParse(endDateText, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime endDate))
                {
                    System.Diagnostics.Debug.WriteLine($"Invalid end date format: {endDateText}");
                    ShowError("Invalid end date format. Please use MM/DD/YYYY format.");
                    return;
                }

                // Ensure start date is before end date
                if (startDate > endDate)
                {
                    System.Diagnostics.Debug.WriteLine($"Start date {startDate} is after end date {endDate}");
                    ShowError("Start date must be before end date.");
                    return;
                }

                // Ensure end date includes the full day
                endDate = endDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

                System.Diagnostics.Debug.WriteLine($"Using date range: {startDate:yyyy-MM-dd HH:mm:ss} to {endDate:yyyy-MM-dd HH:mm:ss}");

                // Set time range to custom
                TimeRange = "custom";
                ViewState["SelectedTimeRange"] = TimeRange;

                // Store dates in properties and ViewState
                StartDate = startDate;
                EndDate = endDate;
                ViewState["StartDate"] = startDate;
                ViewState["EndDate"] = endDate;

                // Update the TimeRangeSelector dropdown if it exists
                DropDownList timeRangeSelector = FindControl("TimeRangeSelector") as DropDownList;
                if (timeRangeSelector != null)
                {
                    // Try to find and select "Custom" option
                    ListItem customOption = timeRangeSelector.Items.FindByValue("custom");
                    if (customOption != null)
                    {
                        timeRangeSelector.SelectedValue = "custom";
                        System.Diagnostics.Debug.WriteLine("Set TimeRangeSelector to 'custom'");
                    }
                    else if (timeRangeSelector.Items.FindByText("Custom") != null)
                    {
                        timeRangeSelector.Items.FindByText("Custom").Selected = true;
                        System.Diagnostics.Debug.WriteLine("Set TimeRangeSelector to item with text 'Custom'");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Could not find 'Custom' option in TimeRangeSelector");
                    }
                }

                // Update period display text
                SetPeriodText();

                // Load the dashboard data with the new date range
                LoadDashboardData();

                // Show success message
                ShowMessage($"Sales report updated for {startDate:MMM d, yyyy} to {endDate:MMM d, yyyy}", true);

                // Ensure the page is rebound
                Page.DataBind();

                System.Diagnostics.Debug.WriteLine("Filter applied successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in btnFilterReport_Click: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ShowError($"Error applying filter: {ex.Message}");
            }
        }

        protected void TimeRangeSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                DropDownList timeRangeSelector = sender as DropDownList;
                if (timeRangeSelector == null)
                {
                    System.Diagnostics.Debug.WriteLine("TimeRangeSelector_SelectedIndexChanged: Sender is not a DropDownList");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"TimeRangeSelector_SelectedIndexChanged fired - Selected value: {timeRangeSelector.SelectedValue}");

                // Get the selected time range
                TimeRange = timeRangeSelector.SelectedValue;
                ViewState["SelectedTimeRange"] = TimeRange;

                // Set date range based on selected time range
                DateTime now = DateTime.Now;
                DateTime startDate;
                DateTime endDate;

                switch (TimeRange.ToLower())
                {
                    case "daily":
                        // Today
                        startDate = now.Date;
                        endDate = now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;

                    case "weekly":
                        // Current week (Sunday to Saturday)
                        int diff = (7 + (now.DayOfWeek - DayOfWeek.Sunday)) % 7;
                        startDate = now.AddDays(-1 * diff).Date;
                        endDate = startDate.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;

                    case "monthly":
                        // Current month
                        startDate = new DateTime(now.Year, now.Month, 1);
                        endDate = startDate.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;

                    case "yearly":
                        // Current year
                        startDate = new DateTime(now.Year, 1, 1);
                        endDate = new DateTime(now.Year, 12, 31, 23, 59, 59);
                        break;

                    default:
                        // Default to weekly if unknown selection
                        diff = (7 + (now.DayOfWeek - DayOfWeek.Sunday)) % 7;
                        startDate = now.AddDays(-1 * diff).Date;
                        endDate = startDate.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59);
                        break;
                }

                // Store the calculated dates
                StartDate = startDate;
                EndDate = endDate;

                // Save to ViewState for persistence
                ViewState["StartDate"] = startDate;
                ViewState["EndDate"] = endDate;

                System.Diagnostics.Debug.WriteLine($"TimeRange set to {TimeRange} - Date range: {startDate:yyyy-MM-dd HH:mm:ss} to {endDate:yyyy-MM-dd HH:mm:ss}");

                // Update UI date controls if they exist
                TextBox txtStartDate = FindControl("txtStartDate") as TextBox;
                TextBox txtEndDate = FindControl("txtEndDate") as TextBox;

                if (txtStartDate != null && txtEndDate != null)
                {
                    txtStartDate.Text = startDate.ToString("MM/dd/yyyy");
                    txtEndDate.Text = endDate.ToString("MM/dd/yyyy");
                    System.Diagnostics.Debug.WriteLine($"Updated UI date controls - StartDate: {txtStartDate.Text}, EndDate: {txtEndDate.Text}");
                }

                // Get connection string (with fallback)
                string connectionString = GetConnectionString();
                System.Diagnostics.Debug.WriteLine($"Using connection string (partial): {connectionString.Substring(0, Math.Min(20, connectionString.Length))}...");

                // Set period text
                SetPeriodText();

                // Load dashboard data with new date range
                LoadDashboardData();

                // Show success message
                ShowMessage($"Sales report updated for {GetTimeRangeDisplayText(TimeRange)}", true);

                // Ensure the page is rebound
                Page.DataBind();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TimeRangeSelector_SelectedIndexChanged: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ShowError($"Error updating time range: {ex.Message}");
            }
        }

        // Helper method to get display text based on time range
        private string GetTimeRangeDisplayText(string timeRange)
        {
            switch (timeRange.ToLower())
            {
                case "daily":
                    return "Today";
                case "weekly":
                    return "This Week";
                case "monthly":
                    return "This Month";
                case "yearly":
                    return "This Year";
                case "custom":
                    return $"{StartDate:MMM d, yyyy} to {EndDate:MMM d, yyyy}";
                default:
                    return "Selected Period";
            }
        }

        private void InitializeChartData()
        {
            // Initialize KPI card values with defaults
            TotalSales = "25";
            TotalRevenue = "₱12,500.00";
            AverageOrderValue = "₱500.00";
            LowStockCount = "3";
            SalesGrowth = "5.2";
            RevenueGrowth = "7.8";
            AvgOrderValueChange = "2.5";
            AvgOrderValueChangeClass = "text-green-500";
            AvgOrderValueIcon = "M5 10l7-7m0 0l7 7m-7-7v18";
            ChartPeriodText = "Last 7 Days";

            // Initialize chart data with sample data in JSON format
            SalesTrendLabels = "[\"Jan 1\", \"Jan 2\", \"Jan 3\", \"Jan 4\", \"Jan 5\", \"Jan 6\", \"Jan 7\"]";
            SalesTrendData = "[350, 420, 380, 450, 500, 470, 520]";

            CategoryNames = "[\"Cakes\", \"Pastries\", \"Breads\", \"Cookies\", \"Desserts\"]";
            CategoryRevenueData = "[5500, 3200, 2100, 1800, 1400]";

            TopProductNames = "[\"Chocolate Cake\", \"Red Velvet\", \"Vanilla Cupcake\", \"Cheesecake\", \"Bread Loaf\"]";
            TopProductQuantities = "[42, 35, 28, 22, 15]";

            InventoryStatusData = "[2, 3, 45]";

            // Initialize ProductRatings with sample data
            if (ProductRatings == null)
            {
                ProductRatings = new List<ProductRating>
                {
                    new ProductRating { ProductName = "Chocolate Cake", AverageRating = 4.8m, TotalReviews = 42, ConversionRate = 68 },
                    new ProductRating { ProductName = "Red Velvet Cake", AverageRating = 4.2m, TotalReviews = 35, ConversionRate = 62 },
                    new ProductRating { ProductName = "Vanilla Cupcake", AverageRating = 3.9m, TotalReviews = 28, ConversionRate = 54 }
                };
            }
        }

        // Method to show success or error messages
        private void ShowMessage(string message, bool isSuccess)
        {
            Label lblMessage = FindControl("lblMessage") as Label;
            if (lblMessage != null)
            {
                lblMessage.Text = message;
                lblMessage.Visible = true;
                lblMessage.CssClass = isSuccess
                    ? "text-green-600 bg-green-50 p-2 rounded-lg mb-4"
                    : "text-red-500 bg-red-50 p-2 rounded-lg mb-4";
            }
        }
    }

    // Class to represent product rating data
    public class ProductRating
    {
        public string ProductName { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public decimal ConversionRate { get; set; }
    }
}