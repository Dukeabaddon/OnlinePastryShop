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
                    // Define week as last 7 days from now
                    StartDate = now.Date.AddDays(-7);
                    EndDate = now;
                    PreviousStartDate = now.Date.AddDays(-14);
                    PreviousEndDate = now.Date.AddDays(-7).AddSeconds(-1);
                    break;

                case "month":
                    // Define month as last 30 days from now
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

            System.Diagnostics.Debug.WriteLine($"Time parameters initialized for '{TimeRange}':");
            System.Diagnostics.Debug.WriteLine($"  Current period: {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd HH:mm:ss}");
            System.Diagnostics.Debug.WriteLine($"  Previous period: {PreviousStartDate:yyyy-MM-dd} to {PreviousEndDate:yyyy-MM-dd HH:mm:ss}");
        }

        // Main method to load all dashboard data
        protected void LoadDashboardData()
        {
            System.Diagnostics.Debug.WriteLine("==== Loading Dashboard Data ====");

            // Set dynamic titles based on time range
            UpdateCardTitles();

            // First load product and order counts
            LoadLowStockProducts();
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
                System.Diagnostics.Debug.WriteLine("Using REVENUE_SUMMARY view approach");

                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    // Get all revenue values in a single query
                    string query = "SELECT TIME_RANGE, REVENUE FROM REVENUE_SUMMARY";
                    System.Diagnostics.Debug.WriteLine($"Executing query: {query}");

                    Dictionary<string, decimal> revenues = new Dictionary<string, decimal>();

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string timeRange = reader["TIME_RANGE"].ToString();
                                decimal revenue = Convert.ToDecimal(reader["REVENUE"]);
                                revenues[timeRange] = revenue;
                                System.Diagnostics.Debug.WriteLine($"Retrieved {timeRange} revenue: {revenue}");
                            }
                        }
                    }

                    // Set the revenue for the selected time period
                    decimal currentRevenue = 0;
                    decimal previousRevenue = 0;
                    string timeRangeLabel = "";

                    switch (TimeRange.ToLower())
                    {
                        case "yesterday":
                            currentRevenue = revenues.ContainsKey("yesterday") ? revenues["yesterday"] : 0;
                            previousRevenue = revenues.ContainsKey("yesterday-prev") ? revenues["yesterday-prev"] : 0;
                            timeRangeLabel = "Yesterday";
                            break;
                        case "week":
                            currentRevenue = revenues.ContainsKey("week") ? revenues["week"] : 0;
                            previousRevenue = revenues.ContainsKey("week-prev") ? revenues["week-prev"] : 0;
                            timeRangeLabel = "This Week";
                            break;
                        case "month":
                            currentRevenue = revenues.ContainsKey("month") ? revenues["month"] : 0;
                            previousRevenue = revenues.ContainsKey("month-prev") ? revenues["month-prev"] : 0;
                            timeRangeLabel = "This Month";
                            break;
                        default: // today
                            currentRevenue = revenues.ContainsKey("today") ? revenues["today"] : 0;
                            previousRevenue = revenues.ContainsKey("yesterday") ? revenues["yesterday"] : 0;
                            timeRangeLabel = "Today";
                            break;
                    }

                    // Set the revenue value and calculate percentage change
                    DailyRevenue = currentRevenue.ToString("N2");

                    // Calculate percentage change
                    decimal revenuePercentChange = 0;
                    if (previousRevenue > 0)
                    {
                        revenuePercentChange = Math.Round(((currentRevenue - previousRevenue) / previousRevenue) * 100, 1);
                    }
                    else
                    {
                        revenuePercentChange = currentRevenue > 0 ? 100 : 0;
                    }

                    // Format the change display
                    DailyRevenueChange = $"{(revenuePercentChange >= 0 ? "+" : "")}{revenuePercentChange}%";

                    // Set the UI indicator classes
                    if (revenuePercentChange > 0)
                    {
                        DailyRevenueChangeClass = "text-green-500";
                        DailyRevenueIcon = "M5 10l7-7m0 0l7 7m-7-7v18";
                    }
                    else if (revenuePercentChange < 0)
                    {
                        DailyRevenueChangeClass = "text-red-500";
                        DailyRevenueIcon = "M19 14l-7 7m0 0l-7-7m7 7V3";
                    }
                    else
                    {
                        DailyRevenueChangeClass = "text-gray-500";
                        DailyRevenueIcon = "M5 12h14";
                    }

                    // Set the revenue card title
                    RevenueCardTitle = $"{timeRangeLabel}'s Revenue";
                    RevenueComparisonText = timeRangeLabel.ToLower() == "today" ? "vs yesterday" :
                                           timeRangeLabel.ToLower() == "yesterday" ? "vs 2 days ago" :
                                           timeRangeLabel.ToLower() == "this week" ? "vs last week" :
                                           "vs last month";

                    // Add debug information
                    System.Diagnostics.Debug.WriteLine($"Time Range: {TimeRange}, Label: {timeRangeLabel}");
                    System.Diagnostics.Debug.WriteLine($"Current Revenue: {currentRevenue}");
                    System.Diagnostics.Debug.WriteLine($"Previous Revenue: {previousRevenue}");
                    System.Diagnostics.Debug.WriteLine($"Revenue Change: {DailyRevenueChange}");

                    // Add a debug panel to the page if debug=1 is in the query string
                    if (Request.QueryString["debug"] == "1")
                    {
                        LiteralControl debugPanel = new LiteralControl();
                        debugPanel.Text = $@"
                            <div class='bg-blue-100 p-4 mb-4 rounded-lg'>
                                <h3 class='font-bold'>Debug Information</h3>
                                <p>Today's Revenue: ₱{revenues.ContainsKey("today") ? revenues["today"].ToString("N2") : "0.00"}</p>
                                <p>Yesterday's Revenue: ₱{revenues.ContainsKey("yesterday") ? revenues["yesterday"].ToString("N2") : "0.00"}</p>
                                <p>This Week's Revenue: ₱{revenues.ContainsKey("week") ? revenues["week"].ToString("N2") : "0.00"}</p>
                                <p>This Month's Revenue: ₱{revenues.ContainsKey("month") ? revenues["month"].ToString("N2") : "0.00"}</p>
                                <p>Selected Time Range: {TimeRange}</p>
                                <p>Displayed Revenue: ₱{DailyRevenue}</p>
                            </div>";
                        Page.Form.Controls.AddAt(0, debugPanel);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadDailyRevenue: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Set default values in case of error
                DailyRevenue = "0.00";
                DailyRevenueChange = "0%";
                DailyRevenueChangeClass = "text-gray-500";
                DailyRevenueIcon = "M5 12h14";
                RevenueCardTitle = "Today's Revenue";
                RevenueComparisonText = "vs yesterday";
            }
                        object result = cmd.ExecuteScalar();
                        currentRevenue = (result != null && result != DBNull.Value) ? Convert.ToDecimal(result) : 0;
                        System.Diagnostics.Debug.WriteLine($"Current period revenue: {currentRevenue}");
                    }

                    // Execute the previous period query for comparison
                    string prevQuery = $@"
                        SELECT
                            NVL(SUM(OD.PRICE * OD.QUANTITY - NVL(OD.COSTPRICE, 0) * OD.QUANTITY), 0) AS TotalRevenue
                        FROM
                            ORDERS O
                        JOIN
                            ORDERDETAILS OD ON O.ORDERID = OD.ORDERID
                        WHERE
                            {prevDateFilter}
                        AND
                            O.STATUS IN ('Completed', 'Approved', 'Delivered', 'Shipped', 'Processing')";

                    System.Diagnostics.Debug.WriteLine($"Previous Period Query: {prevQuery}");

                    using (OracleCommand cmd = new OracleCommand(prevQuery, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        previousRevenue = (result != null && result != DBNull.Value) ? Convert.ToDecimal(result) : 0;
                        System.Diagnostics.Debug.WriteLine($"Previous period revenue: {previousRevenue}");
                    }
                }

                // Set the revenue value
                DailyRevenue = currentRevenue.ToString("N2");

                // Calculate percentage change
                decimal revenuePercentChange = 0;
                if (previousRevenue > 0)
                {
                    revenuePercentChange = Math.Round(((currentRevenue - previousRevenue) / previousRevenue) * 100, 1);
                }
                else
                {
                    revenuePercentChange = currentRevenue > 0 ? 100 : 0;
                }

                // Format the change display
                DailyRevenueChange = $"{(revenuePercentChange >= 0 ? "+" : "")}{revenuePercentChange}%";

                // Set the UI indicator classes
                if (revenuePercentChange > 0)
                {
                    DailyRevenueChangeClass = "text-green-500";
                    DailyRevenueIcon = "M5 10l7-7m0 0l7 7m-7-7v18";
                }
                else if (revenuePercentChange < 0)
                {
                    DailyRevenueChangeClass = "text-red-500";
                    DailyRevenueIcon = "M19 14l-7 7m0 0l-7-7m7 7V3";
                }
                else
                {
                    DailyRevenueChangeClass = "text-gray-500";
                    DailyRevenueIcon = "M5 12h14";
                }

                // Set the revenue card title
                RevenueCardTitle = $"{timeRangeLabel}'s Revenue";
                RevenueComparisonText = timeRangeLabel.ToLower() == "today" ? "vs yesterday" :
                                       timeRangeLabel.ToLower() == "yesterday" ? "vs 2 days ago" :
                                       timeRangeLabel.ToLower() == "this week" ? "vs last week" :
                                       "vs last month";

                // Log the final results
                System.Diagnostics.Debug.WriteLine($"Final revenue for {timeRangeLabel}: {DailyRevenue}");
                System.Diagnostics.Debug.WriteLine($"Revenue change: {DailyRevenueChange}");
                System.Diagnostics.Debug.WriteLine($"Revenue card title: {RevenueCardTitle}");
                System.Diagnostics.Debug.WriteLine($"Revenue comparison text: {RevenueComparisonText}");
                if (previousRevenue > 0)
                {
                    revenuePercentChange = Math.Round(((currentRevenue - previousRevenue) / previousRevenue) * 100, 1);
                }
                else
                {
                    revenuePercentChange = currentRevenue > 0 ? 100 : 0;
                }

                // Format the change display
                DailyRevenueChange = $"{(revenuePercentChange >= 0 ? "+" : "")}{revenuePercentChange}%";

                // Set the UI indicator classes
                if (revenuePercentChange > 0)
                {
                    DailyRevenueChangeClass = "text-green-500";
                    DailyRevenueIcon = "M5 10l7-7m0 0l7 7m-7-7v18";
                }
                else if (revenuePercentChange < 0)
                {
                    DailyRevenueChangeClass = "text-red-500";
                    DailyRevenueIcon = "M19 14l-7 7m0 0l-7-7m7 7V3";
                }
                else
                {
                    DailyRevenueChangeClass = "text-gray-500";
                    DailyRevenueIcon = "M5 12h14";
                }

                // Set the revenue card title
                RevenueCardTitle = $"{timeRangeLabel}'s Revenue";
                RevenueComparisonText = timeRangeLabel.ToLower() == "today" ? "vs yesterday" :
                                       timeRangeLabel.ToLower() == "yesterday" ? "vs 2 days ago" :
                                       timeRangeLabel.ToLower() == "this week" ? "vs last week" :
                                       "vs last month";

                // Log the final results
                System.Diagnostics.Debug.WriteLine($"Final revenue for {timeRangeLabel}: {DailyRevenue}");
                System.Diagnostics.Debug.WriteLine($"Revenue change: {DailyRevenueChange}");
                System.Diagnostics.Debug.WriteLine($"Revenue card title: {RevenueCardTitle}");
                System.Diagnostics.Debug.WriteLine($"Revenue comparison text: {RevenueComparisonText}");

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadDailyRevenue: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Set default values in case of error
                DailyRevenue = "0.00";
                DailyRevenueChange = "0%";
                DailyRevenueChangeClass = "text-gray-500";
                DailyRevenueIcon = "M5 12h14";
                RevenueCardTitle = "Today's Revenue";
                RevenueComparisonText = "vs yesterday";
            }
        }

                    System.Diagnostics.Debug.WriteLine($"Revenue Query: {currentPeriodQuery}");

                    using (OracleCommand cmd = new OracleCommand(currentPeriodQuery, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        decimal currentRevenue = 0;

                        // Handle null result
                        if (result == null || result == DBNull.Value)
                        {
                            System.Diagnostics.Debug.WriteLine("No revenue data found - result was null or DBNull");
                            Console.WriteLine($"NO REVENUE DATA FOUND FOR {timeRangeLabel.ToUpper()} - RESULT WAS NULL");
                            currentRevenue = 0;

                            // For yesterday specifically, let's run an additional check
                            if (TimeRange.ToLower() == "yesterday")
                            {
                                string checkYesterdayQuery = "SELECT TO_CHAR(TRUNC(SYSDATE) - 1, 'YYYY-MM-DD') FROM DUAL";
                                using (OracleCommand checkCmd = new OracleCommand(checkYesterdayQuery, conn))
                                {
                                    string yesterdayDate = checkCmd.ExecuteScalar().ToString();
                                    System.Diagnostics.Debug.WriteLine($"Yesterday's date: {yesterdayDate}");

                                    // Try a direct query based on the exact date string
                                    string directQuery = $@"
                                        SELECT
                                            NVL(SUM(OD.PRICE * OD.QUANTITY), 0) AS TotalRevenue
                                        FROM
                                            ORDERS O
                                        JOIN
                                            ORDERDETAILS OD ON O.ORDERID = OD.ORDERID
                                        WHERE
                                            TO_CHAR(TRUNC(O.ORDERDATE), 'YYYY-MM-DD') = '{yesterdayDate}'
                                        AND
                                            O.STATUS IN ('Completed', 'Approved', 'Delivered', 'Shipped', 'Processing')";

                                    using (OracleCommand directCmd = new OracleCommand(directQuery, conn))
                                    {
                                        object directResult = directCmd.ExecuteScalar();
                                        if (directResult != null && directResult != DBNull.Value)
                                        {
                                            currentRevenue = Convert.ToDecimal(directResult);
                                            DailyRevenue = currentRevenue.ToString("N2");
                                            System.Diagnostics.Debug.WriteLine($"Direct query found revenue for yesterday: {currentRevenue}");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                currentRevenue = Convert.ToDecimal(result);
                                System.Diagnostics.Debug.WriteLine($"Successfully converted result to decimal: {currentRevenue}");
                                Console.WriteLine($"REVENUE FOR {timeRangeLabel.ToUpper()}: {currentRevenue:N2}");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error converting result to decimal: {ex.Message}");
                                currentRevenue = 0;
                            }
                        }

                        DailyRevenue = currentRevenue.ToString("N2");
                        System.Diagnostics.Debug.WriteLine($"Current period revenue: {DailyRevenue}");

                        // If today's revenue is 0, do more detailed debugging to understand why
                        if (TimeRange.ToLower() == "today" && currentRevenue == 0)
                        {
                            // Let's examine the order table to see if there are any orders for today
                            string ordersQuery = @"
                                SELECT
                                    ORDERID,
                                    TO_CHAR(ORDERDATE, 'YYYY-MM-DD HH24:MI:SS') AS ORDERDATE_STR,
                                    STATUS,
                                    TOTALAMOUNT
                                FROM
                                    ORDERS
                                WHERE
                                    TRUNC(ORDERDATE) = TRUNC(SYSDATE)
                                ORDER BY
                                    ORDERID";

                            using (OracleCommand ordersCmd = new OracleCommand(ordersQuery, conn))
                            {
                                using (OracleDataReader reader = ordersCmd.ExecuteReader())
                                {
                                    System.Diagnostics.Debug.WriteLine("Detailed order information for today:");
                                    bool hasOrders = false;
                                    decimal manualTotal = 0;

                                    while (reader.Read())
                                    {
                                        hasOrders = true;
                                        string status = reader["STATUS"].ToString();
                                        decimal amount = Convert.ToDecimal(reader["TOTALAMOUNT"]);

                                        System.Diagnostics.Debug.WriteLine($"  Order #{reader["ORDERID"]}, Date: {reader["ORDERDATE_STR"]}, Status: {status}, Amount: {amount}");

                                        // Only include orders with status that should be counted
                                        if (status == "Completed" || status == "Approved" || status == "Delivered" ||
                                            status == "Shipped" || status == "Processing")
                                        {
                                            manualTotal += amount;
                                        }
                                    }

                                    if (!hasOrders)
                                    {
                                        System.Diagnostics.Debug.WriteLine("  No orders found for today.");
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine($"  Manually calculated revenue for today: {manualTotal}");

                                        // If we have a manual calculation and it's not zero, use it
                                        if (manualTotal > 0 && currentRevenue == 0)
                                        {
                                            DailyRevenue = manualTotal.ToString("N2");
                                            currentRevenue = manualTotal;
                                            System.Diagnostics.Debug.WriteLine($"Using manually calculated revenue: {DailyRevenue}");
                                        }
                                    }
                                }
                            }

                            // Let's also check the ORDERDETAILS table in case that's where we need to calculate from
                            string detailsQuery = @"
                                SELECT
                                    OD.ORDERID,
                                    SUM(OD.QUANTITY * OD.PRICE - OD.COSTPRICE * OD.QUANTITY) AS LINE_TOTAL
                                FROM
                                    ORDERDETAILS OD
                                JOIN
                                    ORDERS O ON OD.ORDERID = O.ORDERID
                                WHERE
                                    TRUNC(O.ORDERDATE) = TRUNC(SYSDATE)
                                AND
                                    O.STATUS IN ('Completed', 'Approved', 'Delivered', 'Shipped', 'Processing')
                                GROUP BY
                                    OD.ORDERID";

                            using (OracleCommand detailsCmd = new OracleCommand(detailsQuery, conn))
                            {
                                using (OracleDataReader reader = detailsCmd.ExecuteReader())
                                {
                                    System.Diagnostics.Debug.WriteLine("Order details calculation for today:");
                                    bool hasDetails = false;
                                    decimal detailsTotal = 0;

                                    while (reader.Read())
                                    {
                                        hasDetails = true;
                                        decimal lineTotal = Convert.ToDecimal(reader["LINE_TOTAL"]);
                                        detailsTotal += lineTotal;
                                        System.Diagnostics.Debug.WriteLine($"  Order #{reader["ORDERID"]}, Line Total: {lineTotal}");
                                    }

                                    if (!hasDetails)
                                    {
                                        System.Diagnostics.Debug.WriteLine("  No order details found for today.");
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine($"  Total from ORDERDETAILS: {detailsTotal}");

                                        // If we have a details calculation and current is still zero, use it
                                        if (detailsTotal > 0 && currentRevenue == 0)
                                        {
                                            DailyRevenue = detailsTotal.ToString("N2");
                                            currentRevenue = detailsTotal;
                                            System.Diagnostics.Debug.WriteLine($"Using ORDERDETAILS calculated revenue: {DailyRevenue}");
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Now calculate previous period revenue for comparison
                    string previousPeriodQuery = @"
                        SELECT
                            NVL(SUM(OD.PRICE * OD.QUANTITY - OD.COSTPRICE * OD.QUANTITY), 0) AS TotalRevenue
                        FROM
                            ORDERS O
                        JOIN
                            ORDERDETAILS OD ON O.ORDERID = OD.ORDERID
                        WHERE
                            " + prevDateCriteria + @"
                        AND
                            O.STATUS IN ('Completed', 'Approved', 'Delivered', 'Shipped', 'Processing')";

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
                        // Use between to clearly define current week (last 7 days)
                        dateCriteria = "ORDERDATE BETWEEN TRUNC(SYSDATE) - 7 AND SYSDATE";
                        prevDateCriteria = "ORDERDATE BETWEEN TRUNC(SYSDATE) - 14 AND TRUNC(SYSDATE) - 7 - 1/86400";
                        break;
                    case "month":
                        // Use between to clearly define current month (last 30 days)
                        dateCriteria = "ORDERDATE BETWEEN TRUNC(SYSDATE) - 30 AND SYSDATE";
                        prevDateCriteria = "ORDERDATE BETWEEN TRUNC(SYSDATE) - 60 AND TRUNC(SYSDATE) - 30 - 1/86400";
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

                    // Get list of matching orders for current period (only approved orders)
                    string listQuery = $"SELECT ORDERID, ORDERDATE, STATUS FROM ORDERS WHERE {dateCriteria} AND STATUS IN ('Completed', 'Approved', 'Delivered', 'Shipped', 'Processing') ORDER BY ORDERDATE";
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

                    // Same for previous period (only approved orders)
                    string prevListQuery = $"SELECT ORDERID, ORDERDATE, STATUS FROM ORDERS WHERE {prevDateCriteria} AND STATUS IN ('Completed', 'Approved', 'Delivered', 'Shipped', 'Processing') ORDER BY ORDERDATE";
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

                    // Now get the actual counts (only approved orders)
                    string countQuery = $"SELECT COUNT(*) FROM ORDERS WHERE {dateCriteria} AND STATUS IN ('Completed', 'Approved', 'Delivered', 'Shipped', 'Processing')";
                    int currentOrders = 0;

                    using (OracleCommand countCmd = new OracleCommand(countQuery, conn))
                    {
                        object result = countCmd.ExecuteScalar();
                        currentOrders = (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;
                        System.Diagnostics.Debug.WriteLine($"Current period count from COUNT query: {currentOrders}");
                    }

                    string prevCountQuery = $"SELECT COUNT(*) FROM ORDERS WHERE {prevDateCriteria} AND STATUS IN ('Completed', 'Approved', 'Delivered', 'Shipped', 'Processing')";
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
                            (SELECT COUNT(*) FROM ORDERS WHERE STATUS = 'Pending') AS PendingExact,
                            (SELECT COUNT(*) FROM ORDERS WHERE STATUS = 'PENDING') AS PendingUpper,
                            (SELECT COUNT(*) FROM ORDERS WHERE STATUS = 'pending') AS PendingLower,
                            (SELECT COUNT(*) FROM ORDERS WHERE UPPER(STATUS) = 'PENDING') AS PendingUpperFunc
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
                            SELECT ORDERID FROM ORDERS
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

                    // First, check the Orders table structure to confirm fields
                    string tableStructureQuery = @"
                        SELECT column_name, data_type
                        FROM user_tab_columns
                        WHERE table_name = 'ORDERS'
                        ORDER BY column_id";

                    using (OracleCommand structureCmd = new OracleCommand(tableStructureQuery, conn))
                    {
                        using (OracleDataReader reader = structureCmd.ExecuteReader())
                        {
                            System.Diagnostics.Debug.WriteLine("Orders table structure:");
                            while (reader.Read())
                            {
                                System.Diagnostics.Debug.WriteLine($"- {reader["column_name"]}: {reader["data_type"]}");
                            }
                        }
                    }

                    // Get full count of Pending orders to verify we're getting all
                    string countQuery = @"
                        SELECT COUNT(*) FROM ORDERS
                        WHERE STATUS IN ('Pending', 'PENDING', 'pending')";

                    int totalPendingOrders = 0;
                    using (OracleCommand countCmd = new OracleCommand(countQuery, conn))
                    {
                        object countResult = countCmd.ExecuteScalar();
                        totalPendingOrders = countResult != DBNull.Value ? Convert.ToInt32(countResult) : 0;
                        System.Diagnostics.Debug.WriteLine($"Total pending orders count: {totalPendingOrders}");
                    }

                    // Now list all pending order IDs for debugging
                    string debugIdsQuery = @"
                        SELECT ORDERID, USERID, ORDERDATE, STATUS
                        FROM ORDERS
                        WHERE STATUS IN ('Pending', 'PENDING', 'pending')
                        ORDER BY ORDERDATE DESC";

                    using (OracleCommand debugCmd = new OracleCommand(debugIdsQuery, conn))
                    {
                        using (OracleDataReader reader = debugCmd.ExecuteReader())
                        {
                            System.Diagnostics.Debug.WriteLine("All pending orders:");
                            while (reader.Read())
                            {
                                System.Diagnostics.Debug.WriteLine($"- Order ID: {reader["ORDERID"]}, User ID: {reader["USERID"]}, Date: {reader["ORDERDATE"]}, Status: {reader["STATUS"]}");
                            }
                        }
                    }

                    // Use a simple query with Oracle 11g compatible syntax (no ANSI JOIN)
                    // Make sure we're not limiting the results with ROWNUM or similar
                    string query = @"
                        SELECT
                            O.ORDERID AS ""ORDERID"",
                            U.USERNAME AS ""CUSTOMERNAME"",
                            U.EMAIL AS ""EMAIL"",
                            O.ORDERDATE AS ""ORDERDATE"",
                            O.TOTALAMOUNT AS ""TOTALAMOUNT"",
                            O.STATUS AS ""STATUS"",
                            O.PAYMENTMETHOD AS ""PAYMENTMETHOD"",
                            O.SHIPPINGADDRESS AS ""SHIPPINGADDRESS""
                        FROM ORDERS O, USERS U
                        WHERE O.USERID = U.USERID
                        AND O.STATUS IN ('Pending', 'PENDING', 'pending')
                        ORDER BY O.ORDERDATE DESC";

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
                                System.Diagnostics.Debug.WriteLine($"About to bind {dt.Rows.Count} pending orders to repeater");
                                PendingOrdersRepeater.DataSource = dt;
                                PendingOrdersRepeater.DataBind();
                                System.Diagnostics.Debug.WriteLine("DataBind completed");

                                // Show/hide empty message
                                if (dt.Rows.Count == 0)
                                {
                                    System.Diagnostics.Debug.WriteLine("No pending orders found, showing empty message");
                                    PendingOrdersRepeater.Visible = false;
                                    if (EmptyPendingOrdersMessage != null)
                                        EmptyPendingOrdersMessage.Visible = true;
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"Found {dt.Rows.Count} pending orders, showing repeater");
                                    PendingOrdersRepeater.Visible = true;
                                    if (EmptyPendingOrdersMessage != null)
                                        EmptyPendingOrdersMessage.Visible = false;
                                }

                                // Update pending order count
                                PendingOrderCount = dt.Rows.Count.ToString();
                                System.Diagnostics.Debug.WriteLine($"Set PendingOrderCount to {PendingOrderCount}");
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

        private void LoadLowStockProducts()
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();

                    // Simple query for low stock products without any joins
                    string query = @"
                        SELECT
                            PRODUCTID,
                            NAME,
                            STOCKQUANTITY,
                            'N/A' AS CategoryName,
                            PRICE
                        FROM AARON_IPT.PRODUCTS
                        WHERE STOCKQUANTITY <= 10
                        AND ISACTIVE = 1
                        ORDER BY STOCKQUANTITY ASC";

                    System.Diagnostics.Debug.WriteLine($"Low stock products query: {query}");

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        DataTable dt = new DataTable();
                        using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }

                        System.Diagnostics.Debug.WriteLine($"Low stock products found: {dt.Rows.Count}");

                        // For debugging, output the first product if found
                        if (dt.Rows.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine("First low stock product details:");
                            foreach (DataColumn column in dt.Columns)
                            {
                                System.Diagnostics.Debug.WriteLine($"  {column.ColumnName}: {dt.Rows[0][column.ColumnName]}");
                            }
                        }

                        // Bind low stock products to repeater
                        if (LowStockRepeater != null)
                        {
                            LowStockRepeater.DataSource = dt;
                            LowStockRepeater.DataBind();

                            // Show/hide empty message
                            if (dt.Rows.Count == 0)
                            {
                                LowStockRepeater.Visible = false;
                                if (EmptyLowStockMessage != null)
                                    EmptyLowStockMessage.Visible = true;
                            }
                            else
                            {
                                LowStockRepeater.Visible = true;
                                if (EmptyLowStockMessage != null)
                                    EmptyLowStockMessage.Visible = false;
                            }

                            // Update the count property
                            LowStockCount = dt.Rows.Count.ToString();
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("ERROR: LowStockRepeater control is null");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadLowStockProducts: {ex.Message}");
                if (LowStockRepeater != null)
                    LowStockRepeater.Visible = false;
                if (EmptyLowStockMessage != null)
                    EmptyLowStockMessage.Visible = true;
                LowStockCount = "0";
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

                    // Determine which price column to use in OrderDetails
                    string priceColumn = "PRICE"; // Default
                    string schemaQuery = @"
                        SELECT column_name
                        FROM user_tab_columns
                        WHERE table_name = 'ORDERDETAILS'
                        ORDER BY column_id";

                    using (OracleCommand schemaCmd = new OracleCommand(schemaQuery, conn))
                    {
                        using (OracleDataReader reader = schemaCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string columnName = reader["column_name"].ToString();
                                if (columnName == "PRODUCTPRICE")
                                {
                                    priceColumn = "PRODUCTPRICE";
                                    break;
                                }
                            }
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Using price column in sales overview: {priceColumn}");

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
                            // Use between to clearly define current week (last 7 days)
                            dateCriteria = "O.ORDERDATE BETWEEN TRUNC(SYSDATE) - 7 AND SYSDATE";
                            break;
                        case "month":
                            // Use between to clearly define current month (last 30 days)
                            dateCriteria = "O.ORDERDATE BETWEEN TRUNC(SYSDATE) - 30 AND SYSDATE";
                            break;
                        default:
                            dateCriteria = "TRUNC(O.ORDERDATE) = TRUNC(SYSDATE)";
                            break;
                    }

                    switch (timeRange.ToLower())
                    {
                        case "today":
                            // Group by hour for today
                            query = $@"
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
                            // Group by hour for yesterday
                            query = $@"
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
                            // Group by day for week
                            query = $@"
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
                            // Group by week for month
                            query = $@"
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
                            query = $@"
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

                    // Debug query to check product sales
                    string debugQuery = @"
                        SELECT COUNT(*) FROM ORDERDETAILS";

                    using (OracleCommand debugCmd = new OracleCommand(debugQuery, conn))
                    {
                        object result = debugCmd.ExecuteScalar();
                        System.Diagnostics.Debug.WriteLine($"Total order details records: {result}");
                    }

                    // Debug query to check ORDERDETAILS structure first
                    string schemaQuery = @"
                        SELECT column_name, data_type
                        FROM user_tab_columns
                        WHERE table_name = 'ORDERDETAILS'
                        ORDER BY column_id";

                    bool hasPriceColumn = false;
                    bool hasUnitPriceColumn = false;

                    using (OracleCommand schemaCmd = new OracleCommand(schemaQuery, conn))
                    {
                        using (OracleDataReader reader = schemaCmd.ExecuteReader())
                        {
                            System.Diagnostics.Debug.WriteLine("OrderDetails table structure:");
                            while (reader.Read())
                            {
                                string columnName = reader["column_name"].ToString();
                                System.Diagnostics.Debug.WriteLine($"- {columnName}: {reader["data_type"]}");

                                if (columnName == "PRICE")
                                    hasPriceColumn = true;
                                else if (columnName == "UNITPRICE")
                                    hasUnitPriceColumn = true;
                            }
                        }
                    }

                    // Determine which price column to use
                    string priceColumn = hasUnitPriceColumn ? "UNITPRICE" : (hasPriceColumn ? "PRICE" : "PRICE");
                    System.Diagnostics.Debug.WriteLine($"Using price column: {priceColumn}");

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

        private void AddDebugInfoToPage()
        {
            try
            {
                // Create a debug panel
                Panel debugPanel = new Panel();
                debugPanel.CssClass = "bg-gray-100 p-4 rounded-lg shadow mb-6";

                // Add a title
                Literal debugTitle = new Literal();
                debugTitle.Text = "<h2 class='text-lg font-bold mb-2'>Debug Information</h2>";
                debugPanel.Controls.Add(debugTitle);

                // Get revenue information for all periods
                var revenueService = new RevenueService(GetConnectionString());
                var allRevenues = revenueService.GetAllPeriodRevenues();

                // Create the debug info content
                Literal debugInfo = new Literal();
                debugInfo.Text = $@"
                    <div class='grid grid-cols-2 gap-4'>
                        <div>
                            <p><strong>Today's Revenue:</strong> ₱{allRevenues["Today"]:N2}</p>
                            <p><strong>Yesterday's Revenue:</strong> ₱{allRevenues["Yesterday"]:N2}</p>
                            <p><strong>This Week's Revenue:</strong> ₱{allRevenues["Week"]:N2}</p>
                            <p><strong>This Month's Revenue:</strong> ₱{allRevenues["Month"]:N2}</p>
                        </div>
                        <div>
                            <p><strong>Selected Time Range:</strong> {TimeRange}</p>
                            <p><strong>Database Date:</strong> {DateTime.Now:yyyy-MM-dd}</p>
                            <p><strong>Debug Mode:</strong> Enabled</p>
                        </div>
                    </div>
                ";
                debugPanel.Controls.Add(debugInfo);

                // Add the debug panel to the page
                Page.Form.Controls.AddAt(0, debugPanel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding debug info: {ex.Message}");
            }
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

        // Helper method to generate product rank badges
        protected string GetProductRankBadge(int itemIndex)
        {
            string html = "";

            if (itemIndex == 0)
            {
                html = "<div class=\"w-6 h-6 rounded-full bg-amber-100 flex items-center justify-center\">" +
                       "<span class=\"text-sm font-semibold text-amber-700\">1</span>" +
                       "</div>";
            }
            else if (itemIndex == 1)
            {
                html = "<div class=\"w-6 h-6 rounded-full bg-gray-200 flex items-center justify-center\">" +
                       "<span class=\"text-sm font-semibold text-gray-700\">2</span>" +
                       "</div>";
            }
            else if (itemIndex == 2)
            {
                html = "<div class=\"w-6 h-6 rounded-full bg-red-100 flex items-center justify-center\">" +
                       "<span class=\"text-sm font-semibold text-red-700\">3</span>" +
                       "</div>";
            }
            else if (itemIndex == 3)
            {
                html = "<div class=\"w-6 h-6 rounded-full bg-blue-100 flex items-center justify-center\">" +
                       "<span class=\"text-sm font-semibold text-blue-700\">4</span>" +
                       "</div>";
            }
            else
            {
                html = "<div class=\"w-6 h-6 rounded-full bg-gray-100 flex items-center justify-center\">" +
                       "<span class=\"text-sm font-semibold text-gray-600\">" + (itemIndex + 1) + "</span>" +
                       "</div>";
            }

            return html;
        }

        // Add the missing PendingOrdersRepeater_ItemCommand method
        protected void PendingOrdersRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int orderId = Convert.ToInt32(e.CommandArgument);
            System.Diagnostics.Debug.WriteLine($"Processing command {e.CommandName} for order {orderId}");

            if (e.CommandName == "Approve")
            {
                string status = "Approved";
                UpdateOrderStatus(orderId, status);
                System.Diagnostics.Debug.WriteLine($"Order {orderId} has been approved");
            }
            else if (e.CommandName == "Reject")
            {
                string status = "Rejected";
                UpdateOrderStatus(orderId, status);
                System.Diagnostics.Debug.WriteLine($"Order {orderId} has been rejected");
            }

            // Add a timestamp parameter to force a complete page reload and prevent caching
            string redirectUrl = Request.RawUrl;
            if (redirectUrl.Contains("?"))
            {
                redirectUrl += $"&t={DateTime.Now.Ticks}";
            }
            else
            {
                redirectUrl += $"?t={DateTime.Now.Ticks}";
            }

            System.Diagnostics.Debug.WriteLine($"Redirecting to {redirectUrl}");
            Response.Redirect(redirectUrl);
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

                    // Check if the order details have COSTPRICE values
                    string costPriceCheckQuery = $"SELECT COUNT(*) FROM ORDERDETAILS WHERE ORDERID = {orderId} AND COSTPRICE IS NULL";
                    int nullCostPriceCount = 0;
                    using (OracleCommand costPriceCmd = new OracleCommand(costPriceCheckQuery, connection))
                    {
                        nullCostPriceCount = Convert.ToInt32(costPriceCmd.ExecuteScalar());
                        System.Diagnostics.Debug.WriteLine($"Order {orderId} has {nullCostPriceCount} order details with NULL COSTPRICE");
                    }

                    // If there are order details with NULL COSTPRICE, update them from the PRODUCTS table
                    if (nullCostPriceCount > 0 && status == "Approved")
                    {
                        string updateCostPriceQuery = @"
                            UPDATE ORDERDETAILS OD
                            SET OD.COSTPRICE = (
                                SELECT P.COSTPRICE
                                FROM PRODUCTS P
                                WHERE P.PRODUCTID = OD.PRODUCTID
                            )
                            WHERE OD.ORDERID = :OrderId
                            AND OD.COSTPRICE IS NULL";

                        using (OracleCommand updateCostCmd = new OracleCommand(updateCostPriceQuery, connection))
                        {
                            updateCostCmd.Parameters.Add("OrderId", OracleDbType.Int32).Value = orderId;
                            int updatedRows = updateCostCmd.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine($"Updated COSTPRICE for {updatedRows} order details");
                        }
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
    }
}