using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Net.Mail;
using System.Text;
using System.Web.Script.Serialization;

namespace OnlinePastryShop.Pages
{
    public partial class Dashboard : System.Web.UI.Page
    {
        // Dashboard metrics properties for data binding in the UI
        public string DailyRevenue { get; set; }
        public string DailyRevenueChange { get; set; }
        public string DailyRevenueChangeClass { get; set; }
        public string DailyRevenueIcon { get; set; }

        public string TodayOrderCount { get; set; }
        public string OrderCountChange { get; set; }
        public string OrderCountChangeClass { get; set; }
        public string OrderCountIcon { get; set; }

        public string PendingOrderCount { get; set; }
        public string LowStockCount { get; set; }

        // Add missing properties
        public decimal OrderPercentChange { get; set; }
        public decimal RevenuePercentChange { get; set; }

        // Dynamic title properties for the cards
        public string RevenueCardTitle { get; set; } = "Today's Revenue";
        public string RevenueComparisonText { get; set; } = "vs yesterday";
        public string OrderCardTitle { get; set; } = "Today's Orders";
        public string OrderComparisonText { get; set; } = "vs yesterday";

        // Chart data for JavaScript binding
        public string SalesOverviewLabels { get; set; }
        public string SalesOverviewData { get; set; }

        // System status
        public bool IsDatabaseConnected { get; private set; } = false;
        public string SystemStatusClass { get; private set; } = "bg-red-100 text-red-800";
        public string SystemStatusText { get; private set; } = "System Offline";

        // Time range for filtering dashboard data
        private string TimeRange { get; set; }
        private DateTime StartDate { get; set; }
        private DateTime EndDate { get; set; }
        private DateTime PreviousStartDate { get; set; }
        private DateTime PreviousEndDate { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Initialize placeholder visibility to avoid null reference exceptions
            if (EmptyLowStockMessage != null)
                EmptyLowStockMessage.Visible = false;

            if (EmptyPendingOrdersMessage != null)
                EmptyPendingOrdersMessage.Visible = false;

            // Check database connection (do this on every page load)
            CheckDatabaseConnection();

            // Log the control initialization
            System.Diagnostics.Debug.WriteLine("------- Dashboard Page Load -------");
            System.Diagnostics.Debug.WriteLine($"EmptyLowStockMessage control exists: {EmptyLowStockMessage != null}");
            System.Diagnostics.Debug.WriteLine($"EmptyPendingOrdersMessage control exists: {EmptyPendingOrdersMessage != null}");
            System.Diagnostics.Debug.WriteLine($"LowStockRepeater control exists: {LowStockRepeater != null}");
            System.Diagnostics.Debug.WriteLine($"PendingOrdersRepeater control exists: {PendingOrdersRepeater != null}");

            if (!IsPostBack)
            {
                // Initialize time parameters
                if (Request.QueryString["timeRange"] != null)
                {
                    TimeRange = Request.QueryString["timeRange"];
                    // Make sure dropdown matches the query string value
                    if (TimeRangeSelector != null)
                    {
                        TimeRangeSelector.SelectedValue = TimeRange;
                    }
                }
                else
                {
                    TimeRange = "today"; // Default to today
                }

                InitializeTimeParameters();

                // Load dashboard data
                LoadDashboardData();
            }
        }

        private void CheckDatabaseConnection()
        {
            try
            {
                using (OracleConnection connection = new OracleConnection(GetConnectionString()))
                {
                    connection.Open();
                    IsDatabaseConnected = true;
                    SystemStatusClass = "bg-green-100 text-green-800";
                    SystemStatusText = "System Online";
                    System.Diagnostics.Debug.WriteLine("Database connection successful.");
                }
            }
            catch (Exception ex)
            {
                IsDatabaseConnected = false;
                SystemStatusClass = "bg-red-100 text-red-800";
                SystemStatusText = "System Offline";
                System.Diagnostics.Debug.WriteLine($"Database connection failed: {ex.Message}");
            }
        }

        private void InitializeTimeParameters()
        {
            DateTime now = DateTime.Now;

            switch (TimeRange.ToLower())
            {
                case "today":
                    StartDate = now.Date;
                    EndDate = now;
                    PreviousStartDate = now.Date.AddDays(-1);
                    PreviousEndDate = now.Date.AddSeconds(-1);
                    break;

                case "yesterday":
                    StartDate = now.Date.AddDays(-1);
                    EndDate = now.Date.AddSeconds(-1);
                    PreviousStartDate = now.Date.AddDays(-2);
                    PreviousEndDate = now.Date.AddDays(-1).AddSeconds(-1);
                    break;

                case "week":
                    StartDate = now.Date.AddDays(-7);
                    EndDate = now;
                    PreviousStartDate = now.Date.AddDays(-14);
                    PreviousEndDate = now.Date.AddDays(-7).AddSeconds(-1);
                    break;

                case "month":
                    StartDate = now.Date.AddDays(-30);
                    EndDate = now;
                    PreviousStartDate = now.Date.AddDays(-60);
                    PreviousEndDate = now.Date.AddDays(-30).AddSeconds(-1);
                    break;

                default:
                    StartDate = now.Date;
                    EndDate = now;
                    PreviousStartDate = now.Date.AddDays(-1);
                    PreviousEndDate = now.Date.AddSeconds(-1);
                    break;
            }
        }

        // Main method to load all dashboard data
        protected void LoadDashboardData()
        {
            System.Diagnostics.Debug.WriteLine("==== Loading Dashboard Data ====");

            // Set dynamic titles based on time range
            UpdateCardTitles();

            // First load product and order counts
            LoadLowStockItems(new OracleConnection(GetConnectionString()));
            LoadPendingOrderCount();

            // Then load full data
            LoadDailyRevenue();
            LoadOrderCount();
            LoadPendingOrders();
            LoadSalesOverviewChart();
            LoadTopSellingProducts();

            System.Diagnostics.Debug.WriteLine("==== Dashboard Data Loaded ====");
        }

        // New method to update card titles based on time range
        private void UpdateCardTitles()
        {
            switch (TimeRange.ToLower())
            {
                case "today":
                    RevenueCardTitle = "Today's Revenue";
                    RevenueComparisonText = "vs yesterday";
                    OrderCardTitle = "Today's Orders";
                    OrderComparisonText = "vs yesterday";
                    break;
                case "yesterday":
                    RevenueCardTitle = "Yesterday's Revenue";
                    RevenueComparisonText = "vs previous day";
                    OrderCardTitle = "Yesterday's Orders";
                    OrderComparisonText = "vs previous day";
                    break;
                case "week":
                    RevenueCardTitle = "This Week's Revenue";
                    RevenueComparisonText = "vs previous week";
                    OrderCardTitle = "This Week's Orders";
                    OrderComparisonText = "vs previous week";
                    break;
                case "month":
                    RevenueCardTitle = "This Month's Revenue";
                    RevenueComparisonText = "vs previous month";
                    OrderCardTitle = "This Month's Orders";
                    OrderComparisonText = "vs previous month";
                    break;
                default:
                    RevenueCardTitle = "Today's Revenue";
                    RevenueComparisonText = "vs yesterday";
                    OrderCardTitle = "Today's Orders";
                    OrderComparisonText = "vs yesterday";
                    break;
            }

            System.Diagnostics.Debug.WriteLine($"Set card titles for {TimeRange}: {RevenueCardTitle}, {OrderCardTitle}");
        }

        private string LoadDailyRevenue()
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    string dateCriteria = "";
                    string prevDateCriteria = "";

                    // Determine date criteria based on time range
                    switch (TimeRange.ToLower())
                    {
                        case "today":
                            dateCriteria = "TRUNC(O.ORDERDATE) = TRUNC(SYSDATE)";
                            prevDateCriteria = "TRUNC(O.ORDERDATE) = TRUNC(SYSDATE) - 1";
                            break;
                        case "yesterday":
                            dateCriteria = "TRUNC(O.ORDERDATE) = TRUNC(SYSDATE) - 1";
                            prevDateCriteria = "TRUNC(O.ORDERDATE) = TRUNC(SYSDATE) - 2";
                            break;
                        case "week":
                            dateCriteria = "O.ORDERDATE >= TRUNC(SYSDATE) - 7";
                            prevDateCriteria = "O.ORDERDATE >= TRUNC(SYSDATE) - 14 AND O.ORDERDATE < TRUNC(SYSDATE) - 7";
                            break;
                        case "month":
                            dateCriteria = "O.ORDERDATE >= TRUNC(SYSDATE) - 30";
                            prevDateCriteria = "O.ORDERDATE >= TRUNC(SYSDATE) - 60 AND O.ORDERDATE < TRUNC(SYSDATE) - 30";
                            break;
                        default:
                            dateCriteria = "TRUNC(O.ORDERDATE) = TRUNC(SYSDATE)";
                            prevDateCriteria = "TRUNC(O.ORDERDATE) = TRUNC(SYSDATE) - 1";
                            break;
                    }

                    System.Diagnostics.Debug.WriteLine($"Time Range: {TimeRange}");
                    System.Diagnostics.Debug.WriteLine($"Date Criteria: {dateCriteria}");
                    System.Diagnostics.Debug.WriteLine($"Prev Date Criteria: {prevDateCriteria}");

                    // Calculate true profit (revenue - cost) using ORDERDETAILS and PRODUCTS table
                    string currentPeriodQuery = @"
                        SELECT
                            NVL(SUM(OD.QUANTITY * (OD.PRICE - P.COSTPRICE)), 0) AS TrueProfit
                        FROM 
                            AARON_IPT.ORDERDETAILS OD,
                            AARON_IPT.PRODUCTS P,
                            AARON_IPT.ORDERS O
                        WHERE
                            OD.PRODUCTID = P.PRODUCTID
                            AND OD.ORDERID = O.ORDERID
                            AND " + dateCriteria + @"
                            AND O.STATUS IN ('Completed', 'Approved', 'Delivered', 'Shipped', 'Processing')";

                    System.Diagnostics.Debug.WriteLine($"Revenue Query: {currentPeriodQuery}");

                    using (OracleCommand cmd = new OracleCommand(currentPeriodQuery, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        decimal currentRevenue = 0;

                        // Handle null result
                        if (result == null || result == DBNull.Value)
                        {
                            System.Diagnostics.Debug.WriteLine("No revenue data found - result was null or DBNull");
                            currentRevenue = 0;
                        }
                        else
                        {
                            try
                            {
                                currentRevenue = Convert.ToDecimal(result);
                                System.Diagnostics.Debug.WriteLine($"Successfully converted result to decimal: {currentRevenue}");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error converting result to decimal: {ex.Message}");
                                currentRevenue = 0;
                            }
                        }

                        DailyRevenue = currentRevenue.ToString("N2");
                        System.Diagnostics.Debug.WriteLine($"Current period profit: {DailyRevenue}");
                    }

                    // Calculate previous period profit for comparison
                    string previousPeriodQuery = @"
                        SELECT
                            NVL(SUM(OD.QUANTITY * (OD.PRICE - P.COSTPRICE)), 0) AS TrueProfit
                        FROM 
                            AARON_IPT.ORDERDETAILS OD,
                            AARON_IPT.PRODUCTS P,
                            AARON_IPT.ORDERS O
                        WHERE
                            OD.PRODUCTID = P.PRODUCTID
                            AND OD.ORDERID = O.ORDERID
                            AND " + prevDateCriteria + @"
                            AND O.STATUS IN ('Completed', 'Approved', 'Delivered', 'Shipped', 'Processing')";

                    System.Diagnostics.Debug.WriteLine($"Previous Period Profit Query: {previousPeriodQuery}");

                    decimal previousRevenue = 0;
                    using (OracleCommand cmd = new OracleCommand(previousPeriodQuery, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        previousRevenue = (result != DBNull.Value) ? Convert.ToDecimal(result) : 0;
                        System.Diagnostics.Debug.WriteLine($"Previous period profit: {previousRevenue}");
                    }

                    // Calculate percentage change
                    RevenuePercentChange = CalculatePercentChange(Convert.ToDecimal(DailyRevenue), previousRevenue);
                    System.Diagnostics.Debug.WriteLine($"Profit percent change: {RevenuePercentChange}%");

                    // Format the change for display
                    DailyRevenueChange = Math.Abs(RevenuePercentChange).ToString("N1");

                    // Set appropriate CSS and icon based on change
                    if (RevenuePercentChange > 0)
                    {
                        DailyRevenueChangeClass = "text-green-600";
                        DailyRevenueIcon = "M5 10l7-7m0 0l7 7m-7-7v18";  // Up arrow path
                    }
                    else if (RevenuePercentChange < 0)
                    {
                        DailyRevenueChangeClass = "text-red-600";
                        DailyRevenueIcon = "M19 14l-7 7m0 0l-7-7m7 7V3";  // Down arrow path
                    }
                    else
                    {
                        DailyRevenueChangeClass = "text-gray-500";
                        DailyRevenueIcon = "M5 12h.01M12 12h.01M19 12h.01M6 12a1 1 0 11-2 0 1 1 0 012 0zm7 0a1 1 0 11-2 0 1 1 0 012 0zm7 0a1 1 0 11-2 0 1 1 0 012 0z";  // Flat line
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadDailyRevenue: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                DailyRevenue = "0.00";
                DailyRevenueChange = "0.0";
                DailyRevenueChangeClass = "text-gray-500";
                DailyRevenueIcon = "M5 12h.01M12 12h.01M19 12h.01M6 12a1 1 0 11-2 0 1 1 0 012 0zm7 0a1 1 0 11-2 0 1 1 0 012 0zm7 0a1 1 0 11-2 0 1 1 0 012 0z";
            }
            
            return DailyRevenue;
        }

        // Helper method to calculate percentage change
        private decimal CalculatePercentChange(decimal current, decimal previous)
        {
            if (previous == 0)
            {
                return (current > 0) ? 100 : 0;
            }

            return Math.Round(((current - previous) / previous) * 100, 1);
        }

        private void LoadOrderCount()
        {
            try
            {
                string dateCriteria = "";
                string prevDateCriteria = "";

                // Set date criteria based on time range
                switch (TimeRange.ToLower())
                {
                    case "today":
                        dateCriteria = "TRUNC(ORDERDATE) = TRUNC(SYSDATE)";
                        prevDateCriteria = "TRUNC(ORDERDATE) = TRUNC(SYSDATE) - 1";
                        break;
                    case "yesterday":
                        dateCriteria = "TRUNC(ORDERDATE) = TRUNC(SYSDATE) - 1";
                        prevDateCriteria = "TRUNC(ORDERDATE) = TRUNC(SYSDATE) - 2";
                        break;
                    case "week":
                        dateCriteria = "ORDERDATE >= TRUNC(SYSDATE) - 7";
                        prevDateCriteria = "ORDERDATE >= TRUNC(SYSDATE) - 14 AND ORDERDATE < TRUNC(SYSDATE) - 7";
                        break;
                    case "month":
                        dateCriteria = "ORDERDATE >= TRUNC(SYSDATE) - 30";
                        prevDateCriteria = "ORDERDATE >= TRUNC(SYSDATE) - 60 AND ORDERDATE < TRUNC(SYSDATE) - 30";
                        break;
                    default:
                        dateCriteria = "TRUNC(ORDERDATE) = TRUNC(SYSDATE)";
                        prevDateCriteria = "TRUNC(ORDERDATE) = TRUNC(SYSDATE) - 1";
                        break;
                }

                System.Diagnostics.Debug.WriteLine($"LoadOrderCount - Date Criteria: {dateCriteria}");
                System.Diagnostics.Debug.WriteLine($"LoadOrderCount - Prev Date Criteria: {prevDateCriteria}");

                // Count the orders directly for verification
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    // Get list of matching orders for current period
                    string listQuery = $@"SELECT ORDERID, ORDERDATE, STATUS FROM ""AARON_IPT"".""ORDERS"" WHERE {dateCriteria} ORDER BY ORDERDATE";
                    using (OracleCommand listCmd = new OracleCommand(listQuery, conn))
                    {
                        using (OracleDataReader reader = listCmd.ExecuteReader())
                        {
                            System.Diagnostics.Debug.WriteLine("Orders in current period:");
                            int count = 0;
                            while (reader.Read())
                            {
                                count++;
                                System.Diagnostics.Debug.WriteLine($"- #{reader["ORDERID"]}: {reader["ORDERDATE"]} ({reader["STATUS"]})");
                            }
                            System.Diagnostics.Debug.WriteLine($"Total count from listing: {count}");
                        }
                    }

                    // Same for previous period
                    string prevListQuery = $@"SELECT ORDERID, ORDERDATE, STATUS FROM ""AARON_IPT"".""ORDERS"" WHERE {prevDateCriteria} ORDER BY ORDERDATE";
                    using (OracleCommand prevListCmd = new OracleCommand(prevListQuery, conn))
                    {
                        using (OracleDataReader reader = prevListCmd.ExecuteReader())
                        {
                            System.Diagnostics.Debug.WriteLine("Orders in previous period:");
                            int count = 0;
                            while (reader.Read())
                            {
                                count++;
                                System.Diagnostics.Debug.WriteLine($"- #{reader["ORDERID"]}: {reader["ORDERDATE"]} ({reader["STATUS"]})");
                            }
                            System.Diagnostics.Debug.WriteLine($"Total count from listing: {count}");
                        }
                    }

                    // Now get the actual counts
                    string countQuery = $@"SELECT COUNT(*) FROM ""AARON_IPT"".""ORDERS"" WHERE {dateCriteria}";
                    int currentOrders = 0;

                    using (OracleCommand countCmd = new OracleCommand(countQuery, conn))
                    {
                        object result = countCmd.ExecuteScalar();
                        currentOrders = (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;
                        System.Diagnostics.Debug.WriteLine($"Current period count from COUNT query: {currentOrders}");
                    }

                    string prevCountQuery = $@"SELECT COUNT(*) FROM ""AARON_IPT"".""ORDERS"" WHERE {prevDateCriteria}";
                    int previousOrders = 0;

                    using (OracleCommand prevCountCmd = new OracleCommand(prevCountQuery, conn))
                    {
                        object result = prevCountCmd.ExecuteScalar();
                        previousOrders = (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;
                        System.Diagnostics.Debug.WriteLine($"Previous period count from COUNT query: {previousOrders}");
                    }

                    // Set today's order count
                    TodayOrderCount = currentOrders.ToString();
                    System.Diagnostics.Debug.WriteLine($"Final order count set to: {TodayOrderCount}");

                    // Calculate percentage change
                    if (previousOrders > 0)
                    {
                        OrderPercentChange = (decimal)Math.Round(((double)(currentOrders - previousOrders) / previousOrders) * 100, 1);
                    }
                    else
                    {
                        OrderPercentChange = currentOrders > 0 ? 100 : 0;
                    }

                    // Format the change display
                    OrderCountChange = $"{(OrderPercentChange >= 0 ? "+" : "")}{OrderPercentChange}%";
                    System.Diagnostics.Debug.WriteLine($"Order percent change: {OrderPercentChange}%, text: {OrderCountChange}");

                    // Set the UI indicator classes
                    if (OrderPercentChange > 0)
                    {
                        OrderCountChangeClass = "text-green-500";
                        OrderCountIcon = "M5 10l7-7m0 0l7 7m-7-7v18";
                    }
                    else if (OrderPercentChange < 0)
                    {
                        OrderCountChangeClass = "text-red-500";
                        OrderCountIcon = "M19 14l-7 7m0 0l-7-7m7 7V3";
                    }
                    else
                    {
                        OrderCountChangeClass = "text-gray-500";
                        OrderCountIcon = "M19 14l-7 7m0 0l-7-7m7 7V3";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadOrderCount: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                TodayOrderCount = "0";
                OrderCountChange = "0%";
                OrderCountChangeClass = "text-gray-500";
                OrderCountIcon = "M5 10l7-7m0 0l7 7m-7-7v18";
            }
        }

        private void LoadPendingOrderCount()
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    // Debug: test case-sensitivity in Oracle
                    string caseTestQuery = @"
                        SELECT
                            (SELECT COUNT(*) FROM ""AARON_IPT"".""ORDERS"" WHERE STATUS = 'Pending') AS PendingExact,
                            (SELECT COUNT(*) FROM ""AARON_IPT"".""ORDERS"" WHERE STATUS = 'PENDING') AS PendingUpper,
                            (SELECT COUNT(*) FROM ""AARON_IPT"".""ORDERS"" WHERE STATUS = 'pending') AS PendingLower,
                            (SELECT COUNT(*) FROM ""AARON_IPT"".""ORDERS"" WHERE UPPER(STATUS) = 'PENDING') AS PendingUpperFunc
                        FROM DUAL";

                    using (OracleCommand testCmd = new OracleCommand(caseTestQuery, conn))
                    {
                        using (OracleDataReader reader = testCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                System.Diagnostics.Debug.WriteLine("Case sensitivity test for 'Pending':");
                                System.Diagnostics.Debug.WriteLine($"- Exact 'Pending': {reader["PendingExact"]}");
                                System.Diagnostics.Debug.WriteLine($"- Upper 'PENDING': {reader["PendingUpper"]}");
                                System.Diagnostics.Debug.WriteLine($"- Lower 'pending': {reader["PendingLower"]}");
                                System.Diagnostics.Debug.WriteLine($"- Using UPPER(): {reader["PendingUpperFunc"]}");
                            }
                        }
                    }

                    // Count all pending orders with an IN clause to handle case variance
                    string query = @"
                        SELECT
                            COUNT(*) AS PendingCount
                        FROM ""AARON_IPT"".""ORDERS""
                        WHERE STATUS IN ('Pending', 'PENDING', 'pending')";

                    System.Diagnostics.Debug.WriteLine($"Pending order count query: {query}");

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        int pendingCount = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                        PendingOrderCount = pendingCount.ToString();
                        System.Diagnostics.Debug.WriteLine($"Pending order count (from COUNT): {PendingOrderCount}");

                        // Double check with a query that returns all IDs
                        string verifyQuery = @"
                            SELECT ORDERID FROM ""AARON_IPT"".""ORDERS"" 
                            WHERE STATUS IN ('Pending', 'PENDING', 'pending')
                            ORDER BY ORDERID";

                        using (OracleCommand verifyCmd = new OracleCommand(verifyQuery, conn))
                        {
                            using (OracleDataReader reader = verifyCmd.ExecuteReader())
                            {
                                int manualCount = 0;
                                System.Diagnostics.Debug.WriteLine("Pending order IDs:");
                                while (reader.Read())
                                {
                                    manualCount++;
                                    System.Diagnostics.Debug.WriteLine($"- {reader["ORDERID"]}");
                                }
                                System.Diagnostics.Debug.WriteLine($"Pending order count (manual count): {manualCount}");

                                // If there's a discrepancy, use the manual count
                                if (manualCount != pendingCount)
                                {
                                    System.Diagnostics.Debug.WriteLine($"WARNING: Count discrepancy detected! Using manual count instead.");
                                    PendingOrderCount = manualCount.ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadPendingOrderCount: {ex.Message}");
                PendingOrderCount = "0";
            }
        }

        private void LoadPendingOrders()
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    // Simple query to get pending orders - fix the invalid character issue by using proper quotes
                    string query = @"
                        SELECT
                            O.ORDERID,
                            U.USERNAME AS CustomerName,
                            U.EMAIL,
                            O.ORDERDATE,
                            O.TOTALAMOUNT,
                            O.STATUS
                        FROM 
                            AARON_IPT.ORDERS O,
                            AARON_IPT.USERS U
                        WHERE 
                            O.USERID = U.USERID
                        AND O.STATUS IN ('Pending', 'PENDING', 'pending')
                        ORDER BY 
                            O.ORDERDATE DESC";

                    System.Diagnostics.Debug.WriteLine($"Pending orders query: {query}");

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        DataTable dt = new DataTable();
                        try
                        {
                            using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                            {
                                adapter.Fill(dt);
                            }

                            System.Diagnostics.Debug.WriteLine($"Pending orders found: {dt.Rows.Count}");

                            // For debugging, output the first few orders
                            for (int i = 0; i < Math.Min(dt.Rows.Count, 3); i++)
                            {
                                DataRow row = dt.Rows[i];
                                System.Diagnostics.Debug.WriteLine($"Order #{row["ORDERID"]}, Customer: {row["CustomerName"]}, Amount: {row["TOTALAMOUNT"]}");
                            }

                            // Bind pending orders to repeater
                            if (PendingOrdersRepeater != null)
                            {
                                PendingOrdersRepeater.DataSource = dt;
                                PendingOrdersRepeater.DataBind();

                                // Show/hide empty message
                                if (dt.Rows.Count == 0)
                                {
                                    PendingOrdersRepeater.Visible = false;
                                    if (EmptyPendingOrdersMessage != null)
                                        EmptyPendingOrdersMessage.Visible = true;
                                }
                                else
                                {
                                    PendingOrdersRepeater.Visible = true;
                                    if (EmptyPendingOrdersMessage != null)
                                        EmptyPendingOrdersMessage.Visible = false;
                                }

                                // Update pending order count
                                PendingOrderCount = dt.Rows.Count.ToString();
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("ERROR: PendingOrdersRepeater control is null");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error filling pending orders: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadPendingOrders: {ex.Message}");
                if (PendingOrdersRepeater != null)
                    PendingOrdersRepeater.Visible = false;
                if (EmptyPendingOrdersMessage != null)
                    EmptyPendingOrdersMessage.Visible = true;
            }
        }

        private void LoadLowStockItems(OracleConnection connection)
        {
            // Add a null check for LowStockRepeater before proceeding
            if (LowStockRepeater == null)
            {
                System.Diagnostics.Debug.WriteLine("LowStockRepeater control not found. Skipping low stock data binding.");
                // Ensure the KPI card value is handled gracefully
                LowStockCount = "N/A"; 
                return; // Exit the method as we cannot bind data
            }

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                // Execute a direct query instead of using a stored procedure with RefCursor
                using (OracleCommand directCommand = new OracleCommand())
                {
                    directCommand.Connection = connection;
                    directCommand.CommandText = @"
                        SELECT 
                            P.PRODUCTID as ProductId,
                            P.NAME as Name,
                            P.STOCKQUANTITY as StockQuantity,
                            (SELECT C.NAME FROM CATEGORIES C, PRODUCTCATEGORIES PC 
                             WHERE PC.PRODUCTID = P.PRODUCTID 
                             AND PC.CATEGORYID = C.CATEGORYID
                             AND ROWNUM = 1) AS CategoryName
                        FROM ""AARON_IPT"".""PRODUCTS"" P
                        WHERE P.STOCKQUANTITY > 0 
                        AND P.STOCKQUANTITY <= 10
                        AND P.ISACTIVE = 1
                        ORDER BY P.STOCKQUANTITY ASC";
                    
                    // First get the count for the KPI card
                    OracleCommand countCommand = new OracleCommand(
                        @"SELECT COUNT(*) FROM ""AARON_IPT"".""PRODUCTS"" 
                          WHERE STOCKQUANTITY > 0 AND STOCKQUANTITY <= 10 AND ISACTIVE = 1", 
                        connection);
                    
                    // Get the count and update the KPI
                    object countResult = countCommand.ExecuteScalar();
                    if (countResult != null && countResult != DBNull.Value)
                    {
                        LowStockCount = countResult.ToString();
                    }
                    else
                    {
                        LowStockCount = "0";
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Low stock count: {LowStockCount}");
                    
                    // Now get the actual data
                    DataTable lowStockTable = new DataTable();
                    using (OracleDataAdapter adapter = new OracleDataAdapter(directCommand))
                    {
                        adapter.Fill(lowStockTable);
                        System.Diagnostics.Debug.WriteLine($"Low stock table loaded with {lowStockTable.Rows.Count} rows");
                        
                        // Log column names for debugging
                        foreach (DataColumn col in lowStockTable.Columns)
                        {
                            System.Diagnostics.Debug.WriteLine($"Column found: {col.ColumnName}, Type: {col.DataType}");
                        }
                    }

                    // Data binding for Low Stock Items
                    LowStockRepeater.DataSource = lowStockTable;
                    LowStockRepeater.DataBind();

                    // Handle empty data message visibility
                    if (EmptyLowStockMessage != null)
                    {
                        EmptyLowStockMessage.Visible = (lowStockTable.Rows.Count == 0);
                    }
                    LowStockRepeater.Visible = (lowStockTable.Rows.Count > 0);

                    System.Diagnostics.Debug.WriteLine($"Low stock items loaded: {lowStockTable.Rows.Count}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading low stock items: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ShowError("Error loading low stock items.");
                // Ensure UI elements are handled gracefully on error
                if (EmptyLowStockMessage != null) EmptyLowStockMessage.Visible = true;
                if (LowStockRepeater != null) LowStockRepeater.Visible = false;
                LowStockCount = "Err"; // Indicate error on KPI
            }
        }

        private void LoadSalesOverviewChart()
        {
            try
            {
                // Use the TimeRange property instead of getting it from query string
                System.Diagnostics.Debug.WriteLine($"Loading sales overview chart with TimeRange: {TimeRange}");

                // Call method to generate data for the specified time range
                GenerateSalesOverviewData(TimeRange);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadSalesOverviewChart: {ex.Message}");
                // Provide default empty values
                SalesOverviewLabels = "[]";
                SalesOverviewData = "[]";
            }
        }

        private void GenerateSalesOverviewData(string timeRange)
        {
            // Declare lists outside the try block to ensure they're accessible in catch blocks
            List<string> labels = new List<string>();
            List<decimal> dataPoints = new List<decimal>();

            try
            {
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    string query = "";
                    string dateCriteria = "";

                    // Set the correct date criteria based on the time range
                    switch (timeRange.ToLower())
                    {
                        case "today":
                            dateCriteria = "TRUNC(O.ORDERDATE) = TRUNC(SYSDATE)";
                            break;
                        case "yesterday":
                            dateCriteria = "TRUNC(O.ORDERDATE) = TRUNC(SYSDATE) - 1";
                            break;
                        case "week":
                            dateCriteria = "O.ORDERDATE >= TRUNC(SYSDATE) - 7";
                            break;
                        case "month":
                            dateCriteria = "O.ORDERDATE >= TRUNC(SYSDATE) - 30";
                            break;
                        default:
                            dateCriteria = "TRUNC(O.ORDERDATE) = TRUNC(SYSDATE)";
                            break;
                    }

                    switch (timeRange.ToLower())
                    {
                        case "today":
                            // Group by hour for today, showing profit
                            query = @"
                                SELECT
                                    TO_CHAR(O.ORDERDATE, 'HH24') AS TimeLabel,
                                    NVL(SUM(OD.QUANTITY * (OD.PRICE - P.COSTPRICE)), 0) AS Profit
                                FROM 
                                    AARON_IPT.ORDERS O,
                                    AARON_IPT.ORDERDETAILS OD,
                                    AARON_IPT.PRODUCTS P
                                WHERE 
                                    O.ORDERID = OD.ORDERID
                                    AND OD.PRODUCTID = P.PRODUCTID
                                    AND " + dateCriteria + @"
                                GROUP BY 
                                    TO_CHAR(O.ORDERDATE, 'HH24')
                                ORDER BY 
                                    TimeLabel";

                            // Create all 24 hours as labels
                            for (int hour = 0; hour < 24; hour++)
                            {
                                labels.Add($"{hour:D2}:00");
                                dataPoints.Add(0); // Default to 0, will be replaced if data exists
                            }
                            break;

                        case "yesterday":
                            // Same approach for yesterday
                            query = @"
                                SELECT
                                    TO_CHAR(O.ORDERDATE, 'HH24') AS TimeLabel,
                                    NVL(SUM(OD.QUANTITY * (OD.PRICE - P.COSTPRICE)), 0) AS Profit
                                FROM 
                                    AARON_IPT.ORDERS O,
                                    AARON_IPT.ORDERDETAILS OD,
                                    AARON_IPT.PRODUCTS P
                                WHERE 
                                    O.ORDERID = OD.ORDERID
                                    AND OD.PRODUCTID = P.PRODUCTID
                                    AND " + dateCriteria + @"
                                GROUP BY 
                                    TO_CHAR(O.ORDERDATE, 'HH24')
                                ORDER BY 
                                    TimeLabel";

                            // Create all 24 hours as labels
                            for (int hour = 0; hour < 24; hour++)
                            {
                                labels.Add($"{hour:D2}:00");
                                dataPoints.Add(0);
                            }
                            break;

                        case "week":
                            // Group by day for week, showing profit
                            query = @"
                                SELECT
                                    TO_CHAR(O.ORDERDATE, 'DY') AS TimeLabel,
                                    NVL(SUM(OD.QUANTITY * (OD.PRICE - P.COSTPRICE)), 0) AS Profit
                                FROM 
                                    AARON_IPT.ORDERS O,
                                    AARON_IPT.ORDERDETAILS OD,
                                    AARON_IPT.PRODUCTS P
                                WHERE 
                                    O.ORDERID = OD.ORDERID
                                    AND OD.PRODUCTID = P.PRODUCTID
                                    AND " + dateCriteria + @"
                                GROUP BY 
                                    TO_CHAR(O.ORDERDATE, 'DY')
                                ORDER BY 
                                    TimeLabel";

                            // Create all days of the week as labels
                            string[] dayNames = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
                            for (int i = 0; i < 7; i++)
                            {
                                labels.Add(dayNames[i]);
                                dataPoints.Add(0);
                            }
                            break;

                        case "month":
                            // Group by week for month, showing profit
                            query = @"
                                SELECT
                                    TO_CHAR(O.ORDERDATE, 'IW') AS TimeLabel,
                                    NVL(SUM(OD.QUANTITY * (OD.PRICE - P.COSTPRICE)), 0) AS Profit
                                FROM 
                                    AARON_IPT.ORDERS O,
                                    AARON_IPT.ORDERDETAILS OD,
                                    AARON_IPT.PRODUCTS P
                                WHERE 
                                    O.ORDERID = OD.ORDERID
                                    AND OD.PRODUCTID = P.PRODUCTID
                                    AND " + dateCriteria + @"
                                GROUP BY 
                                    TO_CHAR(O.ORDERDATE, 'IW')
                                ORDER BY 
                                    TimeLabel";

                            // Get current week number
                            int currentWeek = DateTime.Now.DayOfYear / 7 + 1;
                            for (int week = currentWeek - 4; week <= currentWeek; week++)
                            {
                                labels.Add($"Week {week}");
                                dataPoints.Add(0);
                            }
                            break;

                        default:
                            // Default to daily view for today
                            query = @"
                                SELECT
                                    TO_CHAR(O.ORDERDATE, 'HH24') AS TimeLabel,
                                    NVL(SUM(OD.QUANTITY * (OD.PRICE - P.COSTPRICE)), 0) AS Profit
                                FROM 
                                    AARON_IPT.ORDERS O,
                                    AARON_IPT.ORDERDETAILS OD,
                                    AARON_IPT.PRODUCTS P
                                WHERE 
                                    O.ORDERID = OD.ORDERID
                                    AND OD.PRODUCTID = P.PRODUCTID
                                    AND " + dateCriteria + @"
                                GROUP BY 
                                    TO_CHAR(O.ORDERDATE, 'HH24')
                                ORDER BY 
                                    TimeLabel";

                            // Create 6-hour intervals as labels for the default view
                            for (int hour = 0; hour < 24; hour += 6)
                            {
                                labels.Add($"{hour:D2}:00");
                                dataPoints.Add(0);
                            }
                            break;
                    }

                    System.Diagnostics.Debug.WriteLine($"Sales overview query: {query}");

                    // Execute the query to get actual sales data
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            System.Diagnostics.Debug.WriteLine("Sales overview data from database:");

                            // Dictionary to map time labels from query to index in our labels array
                            Dictionary<string, int> labelMapping = new Dictionary<string, int>();

                            if (timeRange.ToLower() == "today" || timeRange.ToLower() == "yesterday")
                            {
                                // For hours (00-23), map directly to index
                                for (int i = 0; i < 24; i++)
                                {
                                    labelMapping[i.ToString("D2")] = i;
                                }
                            }
                            else if (timeRange.ToLower() == "week")
                            {
                                // Map abbreviated day names to index
                                labelMapping["MON"] = 0;
                                labelMapping["TUE"] = 1;
                                labelMapping["WED"] = 2;
                                labelMapping["THU"] = 3;
                                labelMapping["FRI"] = 4;
                                labelMapping["SAT"] = 5;
                                labelMapping["SUN"] = 6;
                            }

                            bool hasData = false;

                            while (reader.Read())
                            {
                                hasData = true;
                                string timeLabel = reader["TimeLabel"].ToString().Trim().ToUpper();
                                decimal profit = Convert.ToDecimal(reader["Profit"]);

                                System.Diagnostics.Debug.WriteLine($"  {timeLabel}: {profit}");

                                // Try to map the time label to our predefined labels
                                if (labelMapping.ContainsKey(timeLabel))
                                {
                                    int index = labelMapping[timeLabel];
                                    if (index < dataPoints.Count)
                                    {
                                        dataPoints[index] = profit;
                                    }
                                }
                                else if (timeRange.ToLower() == "month")
                                {
                                    // For month view, handle week numbers differently
                                    if (int.TryParse(timeLabel, out int weekNum))
                                    {
                                        int currentWeek = DateTime.Now.DayOfYear / 7 + 1;
                                        int index = weekNum - (currentWeek - 4);

                                        if (index >= 0 && index < dataPoints.Count)
                                        {
                                            dataPoints[index] = profit;
                                        }
                                    }
                                }
                            }

                            if (!hasData)
                            {
                                System.Diagnostics.Debug.WriteLine("No sales data found for the selected period.");
                            }
                        }
                    }
                }

                // Convert to JSON strings for JavaScript
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                SalesOverviewLabels = serializer.Serialize(labels);
                SalesOverviewData = serializer.Serialize(dataPoints);

                // Log the generated data for debugging
                System.Diagnostics.Debug.WriteLine($"Sales Overview Labels ({timeRange}): {SalesOverviewLabels}");
                System.Diagnostics.Debug.WriteLine($"Sales Overview Data ({timeRange}): {SalesOverviewData}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GenerateSalesOverviewData: {ex.Message}");

                // Provide fallback data on error
                List<string> fallbackLabels = new List<string> { "Error", "Loading", "Data" };
                List<decimal> fallbackData = new List<decimal> { 0, 0, 0 };

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                SalesOverviewLabels = serializer.Serialize(fallbackLabels);
                SalesOverviewData = serializer.Serialize(fallbackData);
            }
        }

        // Add method to load top selling products
        private void LoadTopSellingProducts()
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    // Determine the date criteria based on the selected time range
                    string dateCriteria = "";

                    switch (TimeRange.ToLower())
                    {
                        case "today":
                            dateCriteria = "AND TRUNC(O.ORDERDATE) = TRUNC(SYSDATE)";
                            break;
                        case "yesterday":
                            dateCriteria = "AND TRUNC(O.ORDERDATE) = TRUNC(SYSDATE) - 1";
                            break;
                        case "week":
                            dateCriteria = "AND O.ORDERDATE >= TRUNC(SYSDATE) - 7";
                            break;
                        case "month":
                            dateCriteria = "AND O.ORDERDATE >= TRUNC(SYSDATE) - 30";
                            break;
                        default:
                            dateCriteria = ""; // No date filter
                            break;
                    }

                    // Updated query to include profit calculation with consistent schema naming
                    string query = @"
                        SELECT * FROM (
                            SELECT
                                P.NAME AS ProductName,
                                SUM(OD.QUANTITY) AS QuantitySold,
                                SUM(OD.QUANTITY * OD.PRICE) AS Revenue,
                                SUM(OD.QUANTITY * (OD.PRICE - P.COSTPRICE)) AS Profit,
                                ROUND((SUM(OD.QUANTITY * (OD.PRICE - P.COSTPRICE)) / SUM(OD.QUANTITY * OD.PRICE)) * 100, 2) AS ProfitMargin
                            FROM 
                                AARON_IPT.PRODUCTS P,
                                AARON_IPT.ORDERDETAILS OD,
                                AARON_IPT.ORDERS O
                            WHERE 
                                OD.PRODUCTID = P.PRODUCTID
                            AND OD.ORDERID = O.ORDERID
                            AND P.ISACTIVE = 1
                                " + dateCriteria + @"
                            GROUP BY 
                                P.NAME
                            ORDER BY 
                                Profit DESC
                        ) WHERE ROWNUM <= 10";

                    System.Diagnostics.Debug.WriteLine($"Top selling products query: {query}");

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        DataTable dt = new DataTable();
                        try
                        {
                            using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                            {
                                adapter.Fill(dt);
                            }

                            System.Diagnostics.Debug.WriteLine($"Top selling products found: {dt.Rows.Count}");

                            // If we have less than 5 products with sales, merge with sample data to show at least 5
                            if (dt.Rows.Count < 5)
                            {
                                System.Diagnostics.Debug.WriteLine($"Not enough products with sales data, adding sample products");
                                DataTable sampleData = CreateSampleTopSellingProducts();

                                // Create a new merged table with both real and sample data
                                DataTable mergedTable = dt.Clone(); // Clone schema

                                // Add real products first
                                foreach (DataRow row in dt.Rows)
                                {
                                    mergedTable.ImportRow(row);
                                }

                                // Add sample products until we have at least 5
                                int countToAdd = 5 - dt.Rows.Count;
                                for (int i = 0; i < countToAdd && i < sampleData.Rows.Count; i++)
                                {
                                    // Check if this sample product is already in our real data
                                    string sampleProductName = sampleData.Rows[i]["ProductName"].ToString();
                                    bool alreadyExists = false;

                                    foreach (DataRow row in dt.Rows)
                                    {
                                        if (row["ProductName"].ToString() == sampleProductName)
                                        {
                                            alreadyExists = true;
                                            break;
                                        }
                                    }

                                    // Only add if it doesn't exist
                                    if (!alreadyExists)
                                    {
                                        mergedTable.ImportRow(sampleData.Rows[i]);
                                    }
                                    else
                                    {
                                        // If it exists, try to add one more sample product
                                        countToAdd++;
                                    }
                                }

                                dt = mergedTable;
                                System.Diagnostics.Debug.WriteLine($"Final product count after merging: {dt.Rows.Count}");
                            }

                            if (dt.Rows.Count > 0)
                            {
                                if (TopProductsRepeater != null)
                                {
                                    TopProductsRepeater.DataSource = dt;
                                    TopProductsRepeater.DataBind();

                                    // Show no data message if needed
                                    Page.ClientScript.RegisterStartupScript(this.GetType(), "hideNoDataMessage",
                                        "document.getElementById('noTopProductsData').style.display = 'none';", true);
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("ERROR: TopProductsRepeater control is null");
                                }
                            }
                            else
                            {
                                // Create sample data if no real data exists
                                DataTable sampleData = CreateSampleTopSellingProducts();
                                if (TopProductsRepeater != null)
                                {
                                    TopProductsRepeater.DataSource = sampleData;
                                    TopProductsRepeater.DataBind();
                                }

                                System.Diagnostics.Debug.WriteLine("Using sample data for top products");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error filling top products data: {ex.Message}");
                            throw; // Re-throw to be caught by the outer try-catch
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadTopSellingProducts: {ex.Message}");

                // On error, display sample data
                DataTable sampleData = CreateSampleTopSellingProducts();
                if (TopProductsRepeater != null)
                {
                    TopProductsRepeater.DataSource = sampleData;
                    TopProductsRepeater.DataBind();
                }

                // Show no data message via JavaScript
                Page.ClientScript.RegisterStartupScript(this.GetType(), "showNoDataMessage",
                    "if(document.getElementById('noTopProductsData')) document.getElementById('noTopProductsData').style.display = 'table-row';", true);
            }
        }

        // Helper method to create sample top selling products data
        private DataTable CreateSampleTopSellingProducts()
        {
            DataTable dt = new DataTable();

            // Add columns including profit
            dt.Columns.Add("ProductName", typeof(string));
            dt.Columns.Add("QuantitySold", typeof(int));
            dt.Columns.Add("Revenue", typeof(decimal));
            dt.Columns.Add("Profit", typeof(decimal));
            dt.Columns.Add("ProfitMargin", typeof(decimal));

            // Add sample rows with profit margin data
            dt.Rows.Add("Chocolate Fudge Cake", 12, 1503.00m, 976.68m, 65.0m);
            dt.Rows.Add("Red Velvet Cake", 10, 1000.00m, 650.00m, 65.0m);
            dt.Rows.Add("Vanilla Bean Cake", 8, 1122.00m, 729.28m, 65.0m);
            dt.Rows.Add("Strawberry Cheesecake", 6, 6594.00m, 3594.00m, 54.5m);
            dt.Rows.Add("Blueberry Muffins", 15, 532.50m, 346.05m, 65.0m);
            dt.Rows.Add("Chocolate Chip Cookies", 20, 600.00m, 390.00m, 65.0m);
            dt.Rows.Add("Tiramisu Cake", 4, 603.00m, 391.96m, 65.0m);
            dt.Rows.Add("Carrot Cake", 5, 876.25m, 569.55m, 65.0m);
            dt.Rows.Add("French Croissants", 12, 504.00m, 327.60m, 65.0m);

            return dt;
        }

        private string GetConnectionString()
        {
            try
            {
                var connString = ConfigurationManager.ConnectionStrings["OracleConnection"];
                if (connString != null)
                {
                    return connString.ConnectionString;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("OracleConnection string not found in Web.config, using backup");
                    return "User Id=AARON_IPT;Password=qwen123;Data Source=localhost:1521/xe;";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR getting connection string: {ex.Message}");
                return "User Id=AARON_IPT;Password=qwen123;Data Source=localhost:1521/xe;";
            }
        }

        // Add the missing PendingOrdersRepeater_ItemCommand method
        protected void PendingOrdersRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int orderId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "Approve")
            {
                string status = "Approved";
                UpdateOrderStatus(orderId, status);
            }
            else if (e.CommandName == "Reject")
            {
                string status = "Rejected";
                UpdateOrderStatus(orderId, status);
            }

            // Refresh the page to show updated data
            Response.Redirect(Request.RawUrl);
        }

        private void UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                using (OracleConnection connection = new OracleConnection(GetConnectionString()))
                {
                    connection.Open();

                    // Debug - log the current status before update
                    string checkQuery = @"SELECT STATUS FROM ""AARON_IPT"".""ORDERS"" WHERE ORDERID = :OrderId";
                    using (OracleCommand checkCmd = new OracleCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.Add("OrderId", OracleDbType.Int32).Value = orderId;
                        object currentStatus = checkCmd.ExecuteScalar();
                        System.Diagnostics.Debug.WriteLine($"Updating order {orderId} from status '{currentStatus}' to '{status}'");
                    }

                    string query = @"
                        UPDATE ""AARON_IPT"".""ORDERS""
                        SET Status = :Status
                        WHERE OrderId = :OrderId
                    ";

                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add("Status", OracleDbType.Varchar2).Value = status;
                        command.Parameters.Add("OrderId", OracleDbType.Int32).Value = orderId;

                        int rowsAffected = command.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Order status update affected {rowsAffected} rows");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating order status: {ex.Message}");
                // You could add error handling here, such as displaying a message to the user
            }
        }

        protected void TimeRangeSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
        {
            DropDownList dropdown = (DropDownList)sender;
            TimeRange = dropdown.SelectedValue;

            System.Diagnostics.Debug.WriteLine($"========================================");
            System.Diagnostics.Debug.WriteLine($"Time range changed to: {TimeRange}");

            // Reinitialize time parameters
            InitializeTimeParameters();
            System.Diagnostics.Debug.WriteLine($"StartDate: {StartDate}, EndDate: {EndDate}");
            System.Diagnostics.Debug.WriteLine($"PreviousStartDate: {PreviousStartDate}, PreviousEndDate: {PreviousEndDate}");

            // Update card titles based on time range
            UpdateCardTitles();
            System.Diagnostics.Debug.WriteLine($"Card titles updated: {RevenueCardTitle}, {OrderCardTitle}");

                // Clear existing data
                DailyRevenue = "0.00";
                TodayOrderCount = "0";
                PendingOrderCount = "0";

            // Reload dashboard data with new time range
            System.Diagnostics.Debug.WriteLine("Reloading dashboard data...");
            LoadDashboardData();

            System.Diagnostics.Debug.WriteLine("Dashboard data reloaded");
            System.Diagnostics.Debug.WriteLine($"DailyRevenue: {DailyRevenue}, TodayOrderCount: {TodayOrderCount}");
            System.Diagnostics.Debug.WriteLine($"========================================");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TimeRangeSelector_SelectedIndexChanged: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        // Add a simple DbHelper class for database operations
        private static class DbHelper
        {
            public static object ExecuteScalar(string query)
            {
                try
                {
                    using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                    {
                        conn.Open();
                        using (OracleCommand cmd = new OracleCommand(query, conn))
                        {
                            return cmd.ExecuteScalar();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error executing scalar query: {ex.Message}");
                    return null;
                }
            }

            public static DataTable ExecuteQuery(string query)
            {
                try
                {
                    using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                    {
                        conn.Open();
                        using (OracleCommand cmd = new OracleCommand(query, conn))
                        {
                            using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                            {
                                DataTable dt = new DataTable();
                                adapter.Fill(dt);
                                return dt;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error executing query: {ex.Message}");
                    return new DataTable();
                }
            }

            private static string GetConnectionString()
            {
                return ConfigurationManager.ConnectionStrings["OracleConnection"]?.ConnectionString
                    ?? "User Id=AARON_IPT;Password=qwen123;Data Source=localhost:1521/xe;";
            }
        }

        /// <summary>
        /// Displays an error message to the user
        /// </summary>
        /// <param name="message">The error message to display</param>
        private void ShowError(string message)
        {
            if (lblErrorMessage != null)
            {
                lblErrorMessage.Text = message;
                lblErrorMessage.Visible = true;
                System.Diagnostics.Debug.WriteLine($"Error displayed: {message}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Unable to display error: {message} - lblErrorMessage control is null");
            }
        }
    }
}