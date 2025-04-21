using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Web.UI.WebControls;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using Oracle.ManagedDataAccess.Types;

namespace OnlinePastryShop.Pages
{
    public partial class SalesReport : System.Web.UI.Page
    {
        #region Properties
        // KPI Properties
        public string TotalRevenue { get; private set; }
        public string TotalSales { get; private set; }
        public string AverageOrderValue { get; private set; }
        public string ItemsPerOrder { get; private set; }

        // Growth Metrics
        public string RevenueGrowth { get; private set; }
        public string SalesGrowth { get; private set; }
        public string AvgOrderValueChange { get; private set; }
        public string ItemsPerOrderChange { get; private set; }

        // Growth Indicator Classes
        public string RevenueGrowthClass { get; private set; }
        public string SalesGrowthClass { get; private set; }
        public string AvgOrderValueChangeClass { get; private set; }
        public string ItemsPerOrderChangeClass { get; private set; }

        // Growth Indicator Icons
        public string RevenueGrowthIcon { get; private set; }
        public string SalesGrowthIcon { get; private set; }
        public string AvgOrderValueIcon { get; private set; }
        public string ItemsPerOrderIcon { get; private set; }

        // Chart Data
        public string ChartPeriodText { get; private set; }
        public string SalesTrendLabels { get; private set; }
        public string SalesTrendData { get; private set; }
        public string CategoryNames { get; private set; }
        public string CategoryRevenueData { get; private set; }
        public string TopProductNames { get; private set; }
        public string TopProductQuantities { get; private set; }
        public string InventoryStatusData { get; private set; }

        // Time Range
        private string TimeRange { get; set; }
        private DateTime StartDate { get; set; }
        private DateTime EndDate { get; set; }
        private DateTime PreviousStartDate { get; set; }
        private DateTime PreviousEndDate { get; set; }

        // Product Ratings
        public List<ProductRatingModel> ProductRatings { get; private set; }
        #endregion

        protected System.Web.UI.HtmlControls.HtmlGenericControl customDateRange;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SalesReport Page_Load started");

                if (!IsPostBack)
                {
                    // On first load of the page
                    System.Diagnostics.Debug.WriteLine("First page load - initializing");

                    // Set default time range and initialize date range controls
                    TimeRange = "Weekly";
                    TimeRangeSelector.SelectedValue = TimeRange;

                    // Calculate date ranges based on selected time period
                    InitializeTimeRange();

                    // Handle custom date range display
                    if (TimeRange == "Custom")
                    {
                        if (customDateRange != null)
                            customDateRange.Style["display"] = "flex";
                    }
                    else
                    {
                        if (customDateRange != null)
                            customDateRange.Style["display"] = "none";
                    }
                }
                else
                {
                    // This is a postback - get current time range
                    TimeRange = TimeRangeSelector.SelectedValue;
                    System.Diagnostics.Debug.WriteLine($"Postback - TimeRange is {TimeRange}");

                    // Recalculate date range
                    CalculateDateRange();
                }

                // Always load dashboard data (both first load and postbacks)
                LoadDashboardData();

                // Initialize chart data properties if needed (as fallback)
                if (string.IsNullOrEmpty(SalesTrendLabels) || SalesTrendLabels == "null")
                {
                    InitializeChartData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Page_Load: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ShowError($"An error occurred while loading the dashboard: {ex.Message}");
            }
        }

        private void InitializeTimeRange()
        {
            // Set default time range to Weekly
            TimeRange = "Weekly";
            TimeRangeSelector.SelectedValue = TimeRange;

            // Calculate date range based on selected time range
            CalculateDateRange();

            // Set date picker values
            txtStartDate.Text = StartDate.ToString("yyyy-MM-dd");
            txtEndDate.Text = EndDate.ToString("yyyy-MM-dd");

            System.Diagnostics.Debug.WriteLine($"Time range initialized: {TimeRange}, Start: {StartDate:yyyy-MM-dd}, End: {EndDate:yyyy-MM-dd}");
        }

        private void CalculateDateRange()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Calculating date range for TimeRange: {TimeRange}");

                // Get current date/time
                DateTime now = DateTime.Now;

                // Calculate start and end dates based on time range
                switch (TimeRange)
                {
                    case "Daily":
                        // For daily view, we show the current day's data by hour
                        StartDate = DateTime.Today; // Start of today
                        EndDate = DateTime.Today.AddDays(1).AddSeconds(-1); // End of today (23:59:59)
                        PreviousStartDate = StartDate.AddDays(-1); // Yesterday
                        PreviousEndDate = StartDate.AddSeconds(-1); // End of yesterday (23:59:59)

                        System.Diagnostics.Debug.WriteLine($"Daily: Today from {StartDate:yyyy-MM-dd HH:mm:ss} to {EndDate:yyyy-MM-dd HH:mm:ss}");
                        System.Diagnostics.Debug.WriteLine($"Daily Previous: Yesterday from {PreviousStartDate:yyyy-MM-dd HH:mm:ss} to {PreviousEndDate:yyyy-MM-dd HH:mm:ss}");
                        break;

                    case "Weekly":
                        // Use a rolling 7-day window instead of calendar week
                        StartDate = now.Date.AddDays(-6); // Last 7 days (including today)
                        EndDate = now.Date.AddDays(1).AddSeconds(-1); // End of today
                        PreviousStartDate = StartDate.AddDays(-7); // Previous 7 days
                        PreviousEndDate = StartDate.AddSeconds(-1); // Day before StartDate

                        System.Diagnostics.Debug.WriteLine($"Weekly: Last 7 days from {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}");
                        System.Diagnostics.Debug.WriteLine($"Weekly Previous: Previous 7 days from {PreviousStartDate:yyyy-MM-dd} to {PreviousEndDate:yyyy-MM-dd}");
                        break;

                    case "Monthly":
                        // Start of current month
                        StartDate = new DateTime(now.Year, now.Month, 1);
                        EndDate = StartDate.AddMonths(1).AddSeconds(-1); // End of current month
                        PreviousStartDate = StartDate.AddMonths(-1); // Start of previous month
                        PreviousEndDate = StartDate.AddSeconds(-1); // End of previous month

                        System.Diagnostics.Debug.WriteLine($"Monthly: This month from {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}");
                        System.Diagnostics.Debug.WriteLine($"Monthly Previous: Last month from {PreviousStartDate:yyyy-MM-dd} to {PreviousEndDate:yyyy-MM-dd}");
                        break;

                    case "Yearly":
                        // Start of current year
                        StartDate = new DateTime(now.Year, 1, 1);
                        EndDate = new DateTime(now.Year, 12, 31, 23, 59, 59); // End of current year
                        PreviousStartDate = new DateTime(now.Year - 1, 1, 1); // Start of previous year
                        PreviousEndDate = new DateTime(now.Year - 1, 12, 31, 23, 59, 59); // End of previous year

                        System.Diagnostics.Debug.WriteLine($"Yearly: This year from {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}");
                        System.Diagnostics.Debug.WriteLine($"Yearly Previous: Last year from {PreviousStartDate:yyyy-MM-dd} to {PreviousEndDate:yyyy-MM-dd}");
                        break;

                    case "Custom":
                        // Check if start and end dates are provided
                        if (txtStartDate.Text != "" && txtEndDate.Text != "")
                        {
                            DateTime customStartDate;
                            DateTime customEndDate;

                            if (DateTime.TryParse(txtStartDate.Text, out customStartDate) &&
                                DateTime.TryParse(txtEndDate.Text, out customEndDate))
                            {
                                StartDate = customStartDate.Date;
                                EndDate = customEndDate.Date.AddDays(1).AddSeconds(-1); // End of selected day

                                // Calculate previous period with same duration
                                TimeSpan duration = EndDate - StartDate;
                                PreviousStartDate = StartDate.AddDays(-duration.TotalDays);
                                PreviousEndDate = StartDate.AddSeconds(-1);

                                System.Diagnostics.Debug.WriteLine($"Custom: From {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}");
                                System.Diagnostics.Debug.WriteLine($"Custom Previous: From {PreviousStartDate:yyyy-MM-dd} to {PreviousEndDate:yyyy-MM-dd}");
                            }
                            else
                            {
                                // Invalid dates, use default (last 7 days)
                                System.Diagnostics.Debug.WriteLine("Invalid custom dates, falling back to last 7 days");
                                StartDate = now.Date.AddDays(-6);
                                EndDate = now.Date.AddDays(1).AddSeconds(-1);
                                PreviousStartDate = StartDate.AddDays(-7);
                                PreviousEndDate = StartDate.AddSeconds(-1);
                            }
                        }
                        else
                        {
                            // Dates not provided, use default (last 7 days)
                            System.Diagnostics.Debug.WriteLine("Custom dates not provided, falling back to last 7 days");
                            StartDate = now.Date.AddDays(-6);
                            EndDate = now.Date.AddDays(1).AddSeconds(-1);
                            PreviousStartDate = StartDate.AddDays(-7);
                            PreviousEndDate = StartDate.AddSeconds(-1);

                            // Set the date controls
                            txtStartDate.Text = StartDate.ToString("yyyy-MM-dd");
                            txtEndDate.Text = EndDate.ToString("yyyy-MM-dd");
                        }
                        break;

                    default:
                        // Default to last 7 days if unknown time range
                        System.Diagnostics.Debug.WriteLine("Unknown time range, falling back to last 7 days");
                        StartDate = now.Date.AddDays(-6);
                        EndDate = now.Date.AddDays(1).AddSeconds(-1);
                        PreviousStartDate = StartDate.AddDays(-7);
                        PreviousEndDate = StartDate.AddSeconds(-1);
                        break;
                }

                // Ensure end date is never in the future
                if (EndDate > now)
                {
                    EndDate = now;
                }

                // Set period text based on calculated dates
                SetPeriodText();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating date range: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Set default dates (last 7 days)
                DateTime now = DateTime.Now;
                StartDate = now.Date.AddDays(-6);
                EndDate = now.Date;
                PreviousStartDate = StartDate.AddDays(-7);
                PreviousEndDate = StartDate.AddDays(-1);
            }
        }

        private void SetPeriodText()
        {
            switch (TimeRange)
            {
                case "Daily":
                    ChartPeriodText = StartDate.ToString("MMMM d, yyyy");
                    break;

                case "Weekly":
                    ChartPeriodText = $"{StartDate.ToString("MMM d")} - {EndDate.ToString("MMM d, yyyy")}";
                    break;

                case "Monthly":
                    ChartPeriodText = StartDate.ToString("MMMM yyyy");
                    break;

                case "Yearly":
                    ChartPeriodText = StartDate.Year.ToString();
                    break;

                case "Custom":
                    ChartPeriodText = $"{StartDate.ToString("MMM d, yyyy")} - {EndDate.ToString("MMM d, yyyy")}";
                    break;

                default:
                    ChartPeriodText = "All Time";
                    break;
            }

            System.Diagnostics.Debug.WriteLine($"Period text set to: {ChartPeriodText}");
        }

        private void LoadDashboardData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Loading dashboard data");

                // Always initialize ProductRatings to prevent NullReferenceException
                ProductRatings = new List<ProductRatingModel>();

                // Load metrics and chart data
                LoadSalesMetrics();

                // Load chart data with error handling for each method
                try
                {
                    LoadSalesTrendData();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading sales trend data: {ex.Message}");
                }

                try
                {
                    LoadCategoryRevenueData();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading category revenue data: {ex.Message}");
                }

                try
                {
                    LoadTopProductsData();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading top products data: {ex.Message}");
                }

                try
                {
                    LoadInventoryStatusData();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading inventory status data: {ex.Message}");
                }

                try
                {
                    LoadProductRatingData();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading product rating data: {ex.Message}");

                    // Initialize with some fallback data if LoadProductRatingData failed
                    if (ProductRatings == null || ProductRatings.Count == 0)
                    {
                        ProductRatings = new List<ProductRatingModel>
                        {
                            new ProductRatingModel
                            {
                                ProductName = "Chocolate Cake",
                                AverageRating = 4.5m,
                                TotalReviews = 0,
                                ConversionRate = 0
                            },
                            new ProductRatingModel
                            {
                                ProductName = "Strawberry Pastry",
                                AverageRating = 4.2m,
                                TotalReviews = 0,
                                ConversionRate = 0
                            },
                            new ProductRatingModel
                            {
                                ProductName = "Blueberry Muffin",
                                AverageRating = 4.0m,
                                TotalReviews = 0,
                                ConversionRate = 0
                            }
                        };
                    }
                }

                // Show success message
                ShowSuccess("Dashboard data updated successfully!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Initialize chart data even in case of error
                InitializeChartData();

                // Show error to user
                ShowError($"Error loading dashboard data: {ex.Message}");
            }

            // Ensure all chart data is valid
            EnsureValidChartData();
        }

        protected void TimeRangeSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"TimeRangeSelector changed to: {TimeRangeSelector.SelectedValue}");

                // Set the time range based on the selection
                TimeRange = TimeRangeSelector.SelectedValue;

                // Show or hide custom date range controls
                if (TimeRange == "Custom")
                {
                    if (customDateRange != null)
                        customDateRange.Style["display"] = "flex";

                    // Set default date values if not already set
                    if (string.IsNullOrEmpty(txtStartDate.Text))
                    {
                        txtStartDate.Text = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd");
                    }

                    if (string.IsNullOrEmpty(txtEndDate.Text))
                    {
                        txtEndDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    }

                    // For custom range, we'll wait for the Apply button click
                    // to calculate date range and load data
                }
                else
                {
                    if (customDateRange != null)
                        customDateRange.Style["display"] = "none";

                    // Calculate date range based on selection
                    CalculateDateRange();

                    // Load dashboard data for the new date range
                    LoadDashboardData();

                    // Ensure chart data is properly initialized
                    EnsureValidChartData();

                    // Log to help with debugging
                    System.Diagnostics.Debug.WriteLine($"Dashboard data loaded for {TimeRange} time range");
                    System.Diagnostics.Debug.WriteLine($"SalesTrendLabels: {SalesTrendLabels}");
                    System.Diagnostics.Debug.WriteLine($"SalesTrendData: {SalesTrendData}");
                }

                // Re-register scripts to update chart data
                RegisterChartScripts();

                // Force chart reinitialization on the client side
                ScriptManager.RegisterStartupScript(this, GetType(), "ReloadCharts",
                    "if (typeof initializeCharts === 'function') { initializeCharts(); }", true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in TimeRangeSelector_SelectedIndexChanged: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ShowError($"Error updating time range: {ex.Message}");

                // Ensure we still have valid chart data even if there's an error
                EnsureValidChartData();
            }
        }

        protected void btnFilterReport_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Custom date filter applied");

                // Validate dates
                DateTime startDate;
                DateTime endDate;

                if (!DateTime.TryParse(txtStartDate.Text, out startDate) ||
                    !DateTime.TryParse(txtEndDate.Text, out endDate))
                {
                    ShowError("Please enter valid dates");
                    return;
                }

                // Ensure end date is not before start date
                if (endDate < startDate)
                {
                    ShowError("End date cannot be before start date");
                    return;
                }

                // Set time range to Custom
                TimeRange = "Custom";
                TimeRangeSelector.SelectedValue = "Custom";

                // Calculate date range based on custom dates
                CalculateDateRange();

                // Load dashboard data for the custom date range
                LoadDashboardData();

                // Ensure chart data is properly initialized
                EnsureValidChartData();

                // Re-register scripts to update chart data
                RegisterChartScripts();

                // Force chart reinitialization on the client side
                ScriptManager.RegisterStartupScript(this, GetType(), "ReloadCharts",
                    "if (typeof initializeCharts === 'function') { initializeCharts(); }", true);

                ShowSuccess("Report updated for the selected date range");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in btnFilterReport_Click: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ShowError($"Error applying custom filter: {ex.Message}");

                // Ensure we still have valid chart data even if there's an error
                EnsureValidChartData();
            }
        }

        private void ShowError(string message)
        {
            lblErrorMessage.Text = message;
            lblErrorMessage.Visible = true;
            lblSuccessMessage.Visible = false;
        }

        private void ShowSuccess(string message)
        {
            lblSuccessMessage.Text = message;
            lblSuccessMessage.Visible = true;
            lblErrorMessage.Visible = false;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            try
            {
                // Register chart initialization scripts
                RegisterChartScripts();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnPreRender: {ex.Message}");
            }
        }

        private void RegisterChartScripts()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Registering chart scripts");

                // Ensure all charts have valid data
                EnsureValidChartData();

                if (ScriptManager.GetCurrent(this.Page) != null)
                {
                    // Create a JavaScript object with chart data
                    string chartDataScript = $@"
                        chartData.salesTrendLabels = {(string.IsNullOrEmpty(SalesTrendLabels) ? "[]" : SalesTrendLabels)};
                        chartData.salesTrendData = {(string.IsNullOrEmpty(SalesTrendData) ? "[]" : SalesTrendData)};
                        chartData.categoryNames = {(string.IsNullOrEmpty(CategoryNames) ? "[]" : CategoryNames)};
                        chartData.categoryRevenueData = {(string.IsNullOrEmpty(CategoryRevenueData) ? "[]" : CategoryRevenueData)};
                        chartData.topProductNames = {(string.IsNullOrEmpty(TopProductNames) ? "[]" : TopProductNames)};
                        chartData.topProductQuantities = {(string.IsNullOrEmpty(TopProductQuantities) ? "[]" : TopProductQuantities)};
                        chartData.inventoryStatusData = {(string.IsNullOrEmpty(InventoryStatusData) ? "[0,0,0]" : InventoryStatusData)};
                    ";

                    // Register the chart data script
                    ScriptManager.RegisterStartupScript(this, GetType(), "ChartDataScript", chartDataScript, true);

                    // Register script for enabling/disabling custom date range
                    if (customDateRange != null)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "ToggleCustomDateRange",
                            $"if ($('#{customDateRange.ClientID}')) {{ " +
                            $"  if ('{TimeRange}' === 'Custom') {{ " +
                            $"    $('#{customDateRange.ClientID}').show(); " +
                            $"  }} else {{ " +
                            $"    $('#{customDateRange.ClientID}').hide(); " +
                            $"  }} " +
                            $"}}", true);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RegisterChartScripts: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void EnsureValidChartData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Ensuring chart data is valid");

                // Initialize ProductRatings if it's null
                if (ProductRatings == null)
                {
                    System.Diagnostics.Debug.WriteLine("Initializing ProductRatings with empty list");
                    ProductRatings = new List<ProductRatingModel>();
                }

                // Ensure we have valid JSON for all chart data
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                // Initialize SalesTrendLabels and SalesTrendData if needed
                if (string.IsNullOrEmpty(SalesTrendLabels) || SalesTrendLabels == "null")
                {
                    // Use empty arrays if no data is available
                    List<string> labels = new List<string>();
                    SalesTrendLabels = serializer.Serialize(labels);
                    System.Diagnostics.Debug.WriteLine($"Initialized SalesTrendLabels as empty array: {SalesTrendLabels}");
                }

                if (string.IsNullOrEmpty(SalesTrendData) || SalesTrendData == "null")
                {
                    // Use empty arrays if no data is available
                    List<decimal> data = new List<decimal>();
                    SalesTrendData = serializer.Serialize(data);
                    System.Diagnostics.Debug.WriteLine($"Initialized SalesTrendData as empty array: {SalesTrendData}");
                }

                // Initialize CategoryNames and CategoryRevenueData if needed
                if (string.IsNullOrEmpty(CategoryNames) || CategoryNames == "null")
                {
                    // Use empty arrays if no data is available
                    List<string> categories = new List<string>();
                    CategoryNames = serializer.Serialize(categories);
                    System.Diagnostics.Debug.WriteLine($"Initialized CategoryNames as empty array: {CategoryNames}");
                }

                if (string.IsNullOrEmpty(CategoryRevenueData) || CategoryRevenueData == "null")
                {
                    // Use empty arrays if no data is available
                    List<decimal> revenues = new List<decimal>();
                    CategoryRevenueData = serializer.Serialize(revenues);
                    System.Diagnostics.Debug.WriteLine($"Initialized CategoryRevenueData as empty array: {CategoryRevenueData}");
                }

                // Initialize TopProductNames and TopProductQuantities if needed
                if (string.IsNullOrEmpty(TopProductNames) || TopProductNames == "null")
                {
                    // Use empty arrays if no data is available
                    List<string> products = new List<string>();
                    TopProductNames = serializer.Serialize(products);
                    System.Diagnostics.Debug.WriteLine($"Initialized TopProductNames as empty array: {TopProductNames}");
                }

                if (string.IsNullOrEmpty(TopProductQuantities) || TopProductQuantities == "null")
                {
                    // Use empty arrays if no data is available
                    List<int> quantities = new List<int>();
                    TopProductQuantities = serializer.Serialize(quantities);
                    System.Diagnostics.Debug.WriteLine($"Initialized TopProductQuantities as empty array: {TopProductQuantities}");
                }

                // Initialize InventoryStatusData if needed
                if (string.IsNullOrEmpty(InventoryStatusData) || InventoryStatusData == "null")
                {
                    // Use empty arrays if no data is available
                    List<int> status = new List<int>();
                    InventoryStatusData = serializer.Serialize(status);
                    System.Diagnostics.Debug.WriteLine($"Initialized InventoryStatusData as empty array: {InventoryStatusData}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ensuring valid chart data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Set absolute fallback values as raw JSON strings representing empty arrays
                SalesTrendLabels = "[]";
                SalesTrendData = "[]";
                CategoryNames = "[]";
                CategoryRevenueData = "[]";
                TopProductNames = "[]";
                TopProductQuantities = "[]";
                InventoryStatusData = "[]";

                // Ensure ProductRatings is at least an empty list
                if (ProductRatings == null)
                {
                    ProductRatings = new List<ProductRatingModel>();
                }
            }
        }

        private void InitializeChartData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Initializing chart data with default values");

                // Initialize KPI values with defaults
                TotalRevenue = "?0.00";
                TotalSales = "0";
                AverageOrderValue = "?0.00";
                ItemsPerOrder = "0.0";

                // Initialize growth metrics with neutral values
                RevenueGrowth = "0%";
                SalesGrowth = "0%";
                AvgOrderValueChange = "0%";
                ItemsPerOrderChange = "0%";

                // Initialize growth indicators to neutral (gray)
                RevenueGrowthClass = "text-gray-500";
                SalesGrowthClass = "text-gray-500";
                AvgOrderValueChangeClass = "text-gray-500";
                ItemsPerOrderChangeClass = "text-gray-500";

                // Use horizontal arrow for neutral indicators
                RevenueGrowthIcon = "M9 5l7 7-7 7";
                SalesGrowthIcon = "M9 5l7 7-7 7";
                AvgOrderValueIcon = "M9 5l7 7-7 7";
                ItemsPerOrderIcon = "M9 5l7 7-7 7";

                // Initialize chart data with sample data
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                // Sales trend data (last 7 days)
                List<string> dateLabels = new List<string>();
                List<decimal> revenueData = new List<decimal>();

                DateTime today = DateTime.Today;
                for (int i = 6; i >= 0; i--)
                {
                    dateLabels.Add(today.AddDays(-i).ToString("MMM dd"));
                    revenueData.Add(0);
                }

                SalesTrendLabels = serializer.Serialize(dateLabels);
                SalesTrendData = serializer.Serialize(revenueData);

                // Category revenue data
                List<string> categories = new List<string> { "Cakes", "Pastries", "Cookies", "Bread", "Drinks" };
                List<decimal> categoryRevenue = new List<decimal> { 0, 0, 0, 0, 0 };

                CategoryNames = serializer.Serialize(categories);
                CategoryRevenueData = serializer.Serialize(categoryRevenue);

                // Top products data
                List<string> products = new List<string> { "Product 1", "Product 2", "Product 3", "Product 4", "Product 5" };
                List<int> quantities = new List<int> { 0, 0, 0, 0, 0 };

                TopProductNames = serializer.Serialize(products);
                TopProductQuantities = serializer.Serialize(quantities);

                // Inventory status data
                List<int> inventoryStatus = new List<int> { 0, 0, 0 }; // Out of stock, Low stock, In stock

                InventoryStatusData = serializer.Serialize(inventoryStatus);

                // Initialize product ratings with sample data
                ProductRatings = new List<ProductRatingModel>
                {
                    new ProductRatingModel { ProductName = "Chocolate Cake", AverageRating = 4.8m, TotalReviews = 42, ConversionRate = 68 },
                    new ProductRatingModel { ProductName = "Red Velvet Cake", AverageRating = 4.2m, TotalReviews = 35, ConversionRate = 62 },
                    new ProductRatingModel { ProductName = "Vanilla Cupcake", AverageRating = 3.9m, TotalReviews = 28, ConversionRate = 54 }
                };

                System.Diagnostics.Debug.WriteLine("Chart data initialized with default values");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in InitializeChartData: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Set minimal defaults for critical properties
                SalesTrendLabels = "[]";
                SalesTrendData = "[]";
                CategoryNames = "[]";
                CategoryRevenueData = "[]";
                TopProductNames = "[]";
                TopProductQuantities = "[]";
                InventoryStatusData = "[0,0,0]";

                TotalRevenue = "?0.00";
                TotalSales = "0";
                AverageOrderValue = "?0.00";
                ItemsPerOrder = "0.0";

                // Initialize growth metrics with neutral values
                RevenueGrowth = "0%";
                SalesGrowth = "0%";
                AvgOrderValueChange = "0%";
                ItemsPerOrderChange = "0%";

                // Initialize growth indicators to neutral (gray)
                RevenueGrowthClass = "text-gray-500";
                SalesGrowthClass = "text-gray-500";
                AvgOrderValueChangeClass = "text-gray-500";
                ItemsPerOrderChangeClass = "text-gray-500";

                // Use horizontal arrow for neutral indicators
                RevenueGrowthIcon = "M9 5l7 7-7 7";
                SalesGrowthIcon = "M9 5l7 7-7 7";
                AvgOrderValueIcon = "M9 5l7 7-7 7";
                ItemsPerOrderIcon = "M9 5l7 7-7 7";
            }
        }

        private void LoadSalesMetrics()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Loading sales metrics for period: {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}");

                // Initialize variables for current and previous period
                decimal currentRevenue = 0;
                int currentOrderCount = 0;
                decimal currentAvgOrder = 0;
                decimal currentItemsPerOrder = 0;
                int currentTotalItems = 0;

                decimal previousRevenue = 0;
                int previousOrderCount = 0;
                decimal previousAvgOrder = 0;
                decimal previousItemsPerOrder = 0;
                int previousTotalItems = 0;

                // Get connection string
                string connectionString = GetConnectionString();

                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();

                    // Format dates for Oracle query
                    string startDateStr = StartDate.ToString("yyyy-MM-dd");
                    string endDateStr = EndDate.ToString("yyyy-MM-dd");

                    // Calculate previous period date range (same duration, immediately before current period)
                    TimeSpan periodDuration = EndDate - StartDate;
                    DateTime previousStartDate = StartDate.AddDays(-periodDuration.Days - 1);
                    DateTime previousEndDate = StartDate.AddDays(-1);

                    string previousStartDateStr = previousStartDate.ToString("yyyy-MM-dd");
                    string previousEndDateStr = previousEndDate.ToString("yyyy-MM-dd");

                    System.Diagnostics.Debug.WriteLine($"Current period: {startDateStr} to {endDateStr}");
                    System.Diagnostics.Debug.WriteLine($"Previous period: {previousStartDateStr} to {previousEndDateStr}");

                    // Query for current period metrics - with additional items per order calculation
                    string currentQuery = @"
                        SELECT 
                            NVL(SUM(o.TOTALAMOUNT), 0) AS TotalRevenue,
                            COUNT(DISTINCT o.ORDERID) AS OrderCount,
                            NVL(SUM(od.QUANTITY), 0) AS TotalItems
                        FROM ORDERS o
                        LEFT JOIN ORDERDETAILS od ON o.ORDERID = od.ORDERID
                        WHERE o.ORDERDATE >= TO_DATE(:StartDate, 'YYYY-MM-DD')
                        AND o.ORDERDATE <= TO_DATE(:EndDate, 'YYYY-MM-DD') + 0.99999
                        AND o.ISACTIVE = 1
                    ";

                    using (OracleCommand cmd = new OracleCommand(currentQuery, connection))
                    {
                        cmd.Parameters.Add(new OracleParameter("StartDate", OracleDbType.Varchar2)).Value = startDateStr;
                        cmd.Parameters.Add(new OracleParameter("EndDate", OracleDbType.Varchar2)).Value = endDateStr;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currentRevenue = reader.GetDecimal(0);
                                currentOrderCount = reader.GetInt32(1);
                                currentTotalItems = reader.GetInt32(2);

                                // Calculate average order value, avoiding division by zero
                                currentAvgOrder = currentOrderCount > 0 ? currentRevenue / currentOrderCount : 0;

                                // Calculate items per order, avoiding division by zero
                                currentItemsPerOrder = currentOrderCount > 0 ? (decimal)currentTotalItems / currentOrderCount : 0;

                                System.Diagnostics.Debug.WriteLine($"Current metrics - Revenue: {currentRevenue}, Orders: {currentOrderCount}, Avg Order: {currentAvgOrder}, Items/Order: {currentItemsPerOrder}");
                            }
                        }
                    }

                    // Query for previous period metrics
                    string previousQuery = @"
                        SELECT 
                            NVL(SUM(o.TOTALAMOUNT), 0) AS TotalRevenue,
                            COUNT(DISTINCT o.ORDERID) AS OrderCount,
                            NVL(SUM(od.QUANTITY), 0) AS TotalItems
                        FROM ORDERS o
                        LEFT JOIN ORDERDETAILS od ON o.ORDERID = od.ORDERID
                        WHERE o.ORDERDATE >= TO_DATE(:StartDate, 'YYYY-MM-DD')
                        AND o.ORDERDATE <= TO_DATE(:EndDate, 'YYYY-MM-DD') + 0.99999
                        AND o.ISACTIVE = 1
                    ";

                    using (OracleCommand cmd = new OracleCommand(previousQuery, connection))
                    {
                        cmd.Parameters.Add(new OracleParameter("StartDate", OracleDbType.Varchar2)).Value = previousStartDateStr;
                        cmd.Parameters.Add(new OracleParameter("EndDate", OracleDbType.Varchar2)).Value = previousEndDateStr;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                previousRevenue = reader.GetDecimal(0);
                                previousOrderCount = reader.GetInt32(1);
                                previousTotalItems = reader.GetInt32(2);

                                // Calculate average order value, avoiding division by zero
                                previousAvgOrder = previousOrderCount > 0 ? previousRevenue / previousOrderCount : 0;

                                // Calculate items per order, avoiding division by zero
                                previousItemsPerOrder = previousOrderCount > 0 ? (decimal)previousTotalItems / previousOrderCount : 0;

                                System.Diagnostics.Debug.WriteLine($"Previous metrics - Revenue: {previousRevenue}, Orders: {previousOrderCount}, Avg Order: {previousAvgOrder}, Items/Order: {previousItemsPerOrder}");
                            }
                        }
                    }
                }

                // Calculate growth percentages
                decimal revenueGrowth = CalculateGrowthPercentage(currentRevenue, previousRevenue);
                decimal orderCountGrowth = CalculateGrowthPercentage(currentOrderCount, previousOrderCount);
                decimal avgOrderGrowth = CalculateGrowthPercentage(currentAvgOrder, previousAvgOrder);
                decimal itemsPerOrderGrowth = CalculateGrowthPercentage(currentItemsPerOrder, previousItemsPerOrder);

                System.Diagnostics.Debug.WriteLine($"Growth - Revenue: {revenueGrowth}%, Orders: {orderCountGrowth}%, Avg Order: {avgOrderGrowth}%, Items/Order: {itemsPerOrderGrowth}%");

                // Set KPI values and growth indicators
                TotalRevenue = FormatCurrency(currentRevenue);
                RevenueGrowth = FormatGrowth(revenueGrowth);

                TotalSales = currentOrderCount.ToString();
                SalesGrowth = FormatGrowth(orderCountGrowth);

                AverageOrderValue = FormatCurrency(currentAvgOrder);
                AvgOrderValueChange = FormatGrowth(avgOrderGrowth);

                // Set Items Per Order value
                ItemsPerOrder = currentItemsPerOrder.ToString("0.0");
                ItemsPerOrderChange = FormatGrowth(itemsPerOrderGrowth);

                // Set CSS classes and SVG paths for growth indicators
                string revenueCssClass, revenueSvgPath;
                string salesCssClass, salesSvgPath;
                string avgOrderCssClass, avgOrderSvgPath;
                string itemsPerOrderCssClass, itemsPerOrderSvgPath;

                SetGrowthIndicators(revenueGrowth, out revenueCssClass, out revenueSvgPath);
                SetGrowthIndicators(orderCountGrowth, out salesCssClass, out salesSvgPath);
                SetGrowthIndicators(avgOrderGrowth, out avgOrderCssClass, out avgOrderSvgPath);
                SetGrowthIndicators(itemsPerOrderGrowth, out itemsPerOrderCssClass, out itemsPerOrderSvgPath);

                RevenueGrowthClass = revenueCssClass;
                RevenueGrowthIcon = revenueSvgPath;

                SalesGrowthClass = salesCssClass;
                SalesGrowthIcon = salesSvgPath;

                AvgOrderValueChangeClass = avgOrderCssClass;
                AvgOrderValueIcon = avgOrderSvgPath;

                ItemsPerOrderChangeClass = itemsPerOrderCssClass;
                ItemsPerOrderIcon = itemsPerOrderSvgPath;

                System.Diagnostics.Debug.WriteLine("Sales metrics loaded successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading sales metrics: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Set default values in case of error
                TotalRevenue = "?0.00";
                RevenueGrowth = "0%";
                RevenueGrowthClass = "text-gray-500";
                RevenueGrowthIcon = "M9 5l7 7-7 7";

                TotalSales = "0";
                SalesGrowth = "0%";
                SalesGrowthClass = "text-gray-500";
                SalesGrowthIcon = "M9 5l7 7-7 7";

                AverageOrderValue = "?0.00";
                AvgOrderValueChange = "0%";
                AvgOrderValueChangeClass = "text-gray-500";
                AvgOrderValueIcon = "M9 5l7 7-7 7";

                ItemsPerOrder = "0.0";
                ItemsPerOrderChange = "0%";
                ItemsPerOrderChangeClass = "text-gray-500";
                ItemsPerOrderIcon = "M9 5l7 7-7 7";
            }
        }

        private decimal CalculateGrowthPercentage(decimal current, decimal previous)
        {
            if (previous == 0)
            {
                return current > 0 ? 100 : 0;
            }

            return Math.Round(((current - previous) / previous) * 100, 2);
        }

        private string FormatGrowth(decimal growthPercent)
        {
            return growthPercent.ToString("0.##") + "%";
        }

        private void SetGrowthIndicators(decimal growthPercent, out string cssClass, out string svgPath)
        {
            // Initialize out parameters
            cssClass = "text-gray-500";
            svgPath = "M9 5l7 7-7 7"; // Horizontal arrow

            if (growthPercent > 0)
            {
                cssClass = "text-green-500";
                svgPath = "M5 10l7-7m0 0l7 7m-7-7v18"; // Up arrow
            }
            else if (growthPercent < 0)
            {
                cssClass = "text-red-500";
                svgPath = "M19 14l-7 7m0 0l-7-7m7 7V3"; // Down arrow
            }
        }

        private string FormatCurrency(decimal amount)
        {
            return $"?{amount:N2}";
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

                System.Diagnostics.Debug.WriteLine("ERROR: OracleConnection string not found in Web.config");
                return "User Id=mecate;Password=qwen123;Data Source=localhost:1521/xe;";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR getting connection string: {ex.Message}");
                return "User Id=mecate;Password=qwen123;Data Source=localhost:1521/xe;";
            }
        }

        private void LoadSalesTrendData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Loading sales trend data");

                // Get connection string
                string connectionString = GetConnectionString();

                // Format dates for Oracle query
                string startDateParam = StartDate.ToString("yyyy-MM-dd");
                string endDateParam = EndDate.ToString("yyyy-MM-dd");

                System.Diagnostics.Debug.WriteLine($"Date range for query: {startDateParam} to {endDateParam}");

                // Lists to store chart data
                List<string> dateLabels = new List<string>();
                List<decimal> revenueData = new List<decimal>();

                // Track if any data was found
                bool hasData = false;

                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    System.Diagnostics.Debug.WriteLine("Database connection opened");

                    string sqlQuery = "";

                    // Create query based on the selected time range
                    switch (TimeRange)
                    {
                        case "Daily":
                            // Group by hour for daily view
                            sqlQuery = @"
                                SELECT 
                                    TO_CHAR(o.ORDERDATE, 'HH24') as time_period,
                                    NVL(SUM(o.TOTALAMOUNT), 0) as revenue
                                FROM 
                                    ORDERS o
                                WHERE 
                                    o.ORDERDATE BETWEEN TO_DATE(:startDate, 'YYYY-MM-DD') 
                                        AND TO_DATE(:endDate, 'YYYY-MM-DD') + 0.99999
                                    AND o.ISACTIVE = 1
                                GROUP BY 
                                    TO_CHAR(o.ORDERDATE, 'HH24')
                                ORDER BY 
                                    time_period";
                            break;

                        case "Weekly":
                            // Group by day for weekly view
                            sqlQuery = @"
                                SELECT 
                                    TO_CHAR(o.ORDERDATE, 'YYYY-MM-DD') as time_period,
                                    NVL(SUM(o.TOTALAMOUNT), 0) as revenue
                                FROM 
                                    ORDERS o
                                WHERE 
                                    o.ORDERDATE BETWEEN TO_DATE(:startDate, 'YYYY-MM-DD') 
                                        AND TO_DATE(:endDate, 'YYYY-MM-DD') + 0.99999
                                    AND o.ISACTIVE = 1
                                GROUP BY 
                                    TO_CHAR(o.ORDERDATE, 'YYYY-MM-DD')
                                ORDER BY 
                                    time_period";
                            break;

                        case "Monthly":
                            // Group by day for monthly view
                            sqlQuery = @"
                                SELECT 
                                    TO_CHAR(o.ORDERDATE, 'YYYY-MM-DD') as time_period,
                                    NVL(SUM(o.TOTALAMOUNT), 0) as revenue
                                FROM 
                                    ORDERS o
                                WHERE 
                                    o.ORDERDATE BETWEEN TO_DATE(:startDate, 'YYYY-MM-DD') 
                                        AND TO_DATE(:endDate, 'YYYY-MM-DD') + 0.99999
                                    AND o.ISACTIVE = 1
                                GROUP BY 
                                    TO_CHAR(o.ORDERDATE, 'YYYY-MM-DD')
                                ORDER BY 
                                    time_period";
                            break;

                        case "Yearly":
                            // Group by month for yearly view
                            sqlQuery = @"
                                SELECT 
                                    TO_CHAR(o.ORDERDATE, 'YYYY-MM') as time_period,
                                    NVL(SUM(o.TOTALAMOUNT), 0) as revenue
                                FROM 
                                    ORDERS o
                                WHERE 
                                    o.ORDERDATE BETWEEN TO_DATE(:startDate, 'YYYY-MM-DD') 
                                        AND TO_DATE(:endDate, 'YYYY-MM-DD') + 0.99999
                                    AND o.ISACTIVE = 1
                                GROUP BY 
                                    TO_CHAR(o.ORDERDATE, 'YYYY-MM')
                                ORDER BY 
                                    time_period";
                            break;

                        case "Custom":
                            // Choose group by based on date range
                            TimeSpan range = EndDate - StartDate;

                            if (range.TotalDays <= 3)
                            {
                                // For short ranges, group by hour
                                sqlQuery = @"
                                    SELECT 
                                        TO_CHAR(o.ORDERDATE, 'YYYY-MM-DD HH24') as time_period,
                                        NVL(SUM(o.TOTALAMOUNT), 0) as revenue
                                    FROM 
                                        ORDERS o
                                    WHERE 
                                        o.ORDERDATE BETWEEN TO_DATE(:startDate, 'YYYY-MM-DD') 
                                            AND TO_DATE(:endDate, 'YYYY-MM-DD') + 0.99999
                                        AND o.ISACTIVE = 1
                                    GROUP BY 
                                        TO_CHAR(o.ORDERDATE, 'YYYY-MM-DD HH24')
                                    ORDER BY 
                                        time_period";
                            }
                            else if (range.TotalDays <= 31)
                            {
                                // For medium ranges, group by day
                                sqlQuery = @"
                                    SELECT 
                                        TO_CHAR(o.ORDERDATE, 'YYYY-MM-DD') as time_period,
                                        NVL(SUM(o.TOTALAMOUNT), 0) as revenue
                                    FROM 
                                        ORDERS o
                                    WHERE 
                                        o.ORDERDATE BETWEEN TO_DATE(:startDate, 'YYYY-MM-DD') 
                                            AND TO_DATE(:endDate, 'YYYY-MM-DD') + 0.99999
                                        AND o.ISACTIVE = 1
                                    GROUP BY 
                                        TO_CHAR(o.ORDERDATE, 'YYYY-MM-DD')
                                    ORDER BY 
                                        time_period";
                            }
                            else
                            {
                                // For longer ranges, group by month
                                sqlQuery = @"
                                    SELECT 
                                        TO_CHAR(o.ORDERDATE, 'YYYY-MM') as time_period,
                                        NVL(SUM(o.TOTALAMOUNT), 0) as revenue
                                    FROM 
                                        ORDERS o
                                    WHERE 
                                        o.ORDERDATE BETWEEN TO_DATE(:startDate, 'YYYY-MM-DD') 
                                            AND TO_DATE(:endDate, 'YYYY-MM-DD') + 0.99999
                                        AND o.ISACTIVE = 1
                                    GROUP BY 
                                        TO_CHAR(o.ORDERDATE, 'YYYY-MM')
                                    ORDER BY 
                                        time_period";
                            }
                            break;

                        default:
                            // Default to daily grouping
                            sqlQuery = @"
                                SELECT 
                                    TO_CHAR(o.ORDERDATE, 'YYYY-MM-DD') as time_period,
                                    NVL(SUM(o.TOTALAMOUNT), 0) as revenue
                                FROM 
                                    ORDERS o
                                WHERE 
                                    o.ORDERDATE BETWEEN TO_DATE(:startDate, 'YYYY-MM-DD') 
                                        AND TO_DATE(:endDate, 'YYYY-MM-DD') + 0.99999
                                    AND o.ISACTIVE = 1
                                GROUP BY 
                                    TO_CHAR(o.ORDERDATE, 'YYYY-MM-DD')
                                ORDER BY 
                                    time_period";
                            break;
                    }

                    System.Diagnostics.Debug.WriteLine($"Executing query: {sqlQuery}");

                    using (OracleCommand command = new OracleCommand(sqlQuery, connection))
                    {
                        command.Parameters.Add(new OracleParameter("startDate", OracleDbType.Varchar2)).Value = startDateParam;
                        command.Parameters.Add(new OracleParameter("endDate", OracleDbType.Varchar2)).Value = endDateParam;

                        // Dictionary to hold the data by time period
                        Dictionary<string, decimal> revenueByTimePeriod = new Dictionary<string, decimal>();

                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            // Read all data from the query
                            while (reader.Read())
                            {
                                hasData = true;
                                string timePeriod = reader["time_period"].ToString();
                                decimal revenue = Convert.ToDecimal(reader["revenue"]);

                                // Only add if revenue > 0
                                if (revenue > 0)
                                {
                                    revenueByTimePeriod[timePeriod] = revenue;
                                    System.Diagnostics.Debug.WriteLine($"Data point: Time={timePeriod}, Revenue={revenue}");
                                }
                            }
                        }

                        // If data was found, build the chart series
                        if (hasData)
                        {
                            // Generate labels and data based on time period
                            if (TimeRange == "Daily")
                            {
                                // For daily view, generate all 24 hours
                                for (int hour = 0; hour < 24; hour++)
                                {
                                    string hourString = hour.ToString("00");
                                    string label = $"{hourString}:00";
                                    dateLabels.Add(label);

                                    if (revenueByTimePeriod.ContainsKey(hourString))
                                    {
                                        revenueData.Add(revenueByTimePeriod[hourString]);
                                    }
                                    else
                                    {
                                        revenueData.Add(0);
                                    }
                                }
                            }
                            else if (TimeRange == "Weekly")
                            {
                                // For weekly view, generate each day in range
                                for (int i = 0; i < 7; i++)
                                {
                                    DateTime date = StartDate.AddDays(i);
                                    string dateString = date.ToString("yyyy-MM-dd");
                                    string label = date.ToString("MMM dd");
                                    dateLabels.Add(label);

                                    if (revenueByTimePeriod.ContainsKey(dateString))
                                    {
                                        revenueData.Add(revenueByTimePeriod[dateString]);
                                    }
                                    else
                                    {
                                        revenueData.Add(0);
                                    }
                                }
                            }
                            else if (TimeRange == "Monthly")
                            {
                                // For monthly view, generate each day
                                int days = (int)(EndDate - StartDate).TotalDays + 1;
                                for (int i = 0; i < days; i++)
                                {
                                    DateTime date = StartDate.AddDays(i);
                                    string dateString = date.ToString("yyyy-MM-dd");
                                    string label = date.ToString("MMM dd");
                                    dateLabels.Add(label);

                                    if (revenueByTimePeriod.ContainsKey(dateString))
                                    {
                                        revenueData.Add(revenueByTimePeriod[dateString]);
                                    }
                                    else
                                    {
                                        revenueData.Add(0);
                                    }
                                }
                            }
                            else if (TimeRange == "Yearly")
                            {
                                // For yearly view, generate each month
                                for (int month = 1; month <= 12; month++)
                                {
                                    DateTime date = new DateTime(StartDate.Year, month, 1);
                                    string monthString = date.ToString("yyyy-MM");
                                    string label = date.ToString("MMM");
                                    dateLabels.Add(label);

                                    if (revenueByTimePeriod.ContainsKey(monthString))
                                    {
                                        revenueData.Add(revenueByTimePeriod[monthString]);
                                    }
                                    else
                                    {
                                        revenueData.Add(0);
                                    }
                                }
                            }
                            else if (TimeRange == "Custom")
                            {
                                // For custom range, determine appropriate grouping
                                TimeSpan range = EndDate - StartDate;

                                if (range.TotalDays <= 3)
                                {
                                    // By hour for short ranges
                                    foreach (var entry in revenueByTimePeriod.OrderBy(e => e.Key))
                                    {
                                        DateTime dt = DateTime.ParseExact(entry.Key, "yyyy-MM-dd HH", 
                                                                         System.Globalization.CultureInfo.InvariantCulture);
                                        dateLabels.Add(dt.ToString("MMM dd HH:00"));
                                        revenueData.Add(entry.Value);
                                    }
                                }
                                else if (range.TotalDays <= 31)
                                {
                                    // By day for medium ranges
                                    for (int i = 0; i <= range.TotalDays; i++)
                                    {
                                        DateTime date = StartDate.AddDays(i);
                                        string dateString = date.ToString("yyyy-MM-dd");
                                        string label = date.ToString("MMM dd");
                                        dateLabels.Add(label);

                                        if (revenueByTimePeriod.ContainsKey(dateString))
                                        {
                                            revenueData.Add(revenueByTimePeriod[dateString]);
                                        }
                                        else
                                        {
                                            revenueData.Add(0);
                                        }
                                    }
                                }
                                else
                                {
                                    // By month for longer ranges
                                    DateTime current = new DateTime(StartDate.Year, StartDate.Month, 1);
                                    while (current <= EndDate)
                                    {
                                        string monthString = current.ToString("yyyy-MM");
                                        string label = current.ToString("MMM yyyy");
                                        dateLabels.Add(label);

                                        if (revenueByTimePeriod.ContainsKey(monthString))
                                        {
                                            revenueData.Add(revenueByTimePeriod[monthString]);
                                        }
                                        else
                                        {
                                            revenueData.Add(0);
                                        }

                                        current = current.AddMonths(1);
                                    }
                                }
                            }
                        }
                    }
                }

                // If no data was found, use empty arrays
                if (!hasData)
                {
                    System.Diagnostics.Debug.WriteLine("No sales trend data found for the selected time period");
                    
                    // Empty arrays indicate no data - will display appropriately in the UI
                    dateLabels = new List<string>();
                    revenueData = new List<decimal>();
                }

                // Serialize the data
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                SalesTrendLabels = serializer.Serialize(dateLabels);
                SalesTrendData = serializer.Serialize(revenueData);

                // Update message visibility
                if (lblMessage != null)
                {
                    lblMessage.Visible = !hasData && !lblMessage.Visible; // Only show if not already visible
                }

                System.Diagnostics.Debug.WriteLine($"Sales trend labels: {SalesTrendLabels}");
                System.Diagnostics.Debug.WriteLine($"Sales trend data: {SalesTrendData}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading sales trend data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Use empty arrays for error states - no fallback data
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                SalesTrendLabels = serializer.Serialize(new List<string>());
                SalesTrendData = serializer.Serialize(new List<decimal>());

                // Show error message
                if (lblMessage != null)
                {
                    lblMessage.Visible = true;
                }
            }
        }

        private void GenerateSampleSalesTrendData(ref List<string> labels, ref List<decimal> data)
        {
            System.Diagnostics.Debug.WriteLine("Generating sample sales trend data");

            // Clear existing data
            labels.Clear();
            data.Clear();

            // Generate sample data based on time range
            Random random = new Random();

            if (TimeRange == "Daily")
            {
                // For daily view, generate hourly data
                for (int hour = 0; hour < 24; hour++)
                {
                    string hourLabel = $"{hour:00}:00";
                    decimal revenue = random.Next(50, 400) + Math.Round((decimal)random.NextDouble(), 2);

                    // Simulate common sales patterns (higher sales in morning and evening)
                    if (hour >= 9 && hour <= 11) revenue *= 1.5m;  // Morning peak
                    if (hour >= 17 && hour <= 20) revenue *= 1.8m; // Evening peak
                    if (hour >= 0 && hour <= 5) revenue *= 0.3m;   // Low overnight

                    labels.Add(hourLabel);
                    data.Add(Math.Round(revenue, 2));
                }
            }
            else if (TimeRange == "Weekly")
            {
                // For weekly view, generate daily data
                string[] days = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
                for (int i = 0; i < 7; i++)
                {
                    DateTime date = DateTime.Now.AddDays(-6 + i);
                    string dayLabel = date.ToString("MMM dd");
                    decimal revenue = random.Next(800, 2500) + Math.Round((decimal)random.NextDouble(), 2);

                    // Simulate higher sales on weekends
                    if (i >= 5) revenue *= 1.4m;

                    labels.Add(dayLabel);
                    data.Add(Math.Round(revenue, 2));
                }
            }
            else if (TimeRange == "Monthly")
            {
                // For monthly view, generate data for each day of the month
                int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                for (int day = 1; day <= daysInMonth; day++)
                {
                    DateTime date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, day);
                    string dayLabel = date.ToString("MMM dd");
                    decimal revenue = random.Next(800, 2500) + Math.Round((decimal)random.NextDouble(), 2);

                    // Simulate higher sales on weekends and mid-month
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        revenue *= 1.4m;

                    if (day >= 10 && day <= 20)
                        revenue *= 1.2m;

                    labels.Add(dayLabel);
                    data.Add(Math.Round(revenue, 2));
                }
            }
            else if (TimeRange == "Yearly")
            {
                // For yearly view, generate monthly data
                for (int month = 1; month <= 12; month++)
                {
                    DateTime date = new DateTime(DateTime.Now.Year, month, 1);
                    string monthLabel = date.ToString("MMM");
                    decimal revenue = random.Next(20000, 40000) + Math.Round((decimal)random.NextDouble(), 2);

                    // Simulate seasonal variations
                    if (month >= 11 || month <= 1) revenue *= 1.5m;  // Holiday season
                    if (month >= 6 && month <= 8) revenue *= 1.3m;   // Summer

                    labels.Add(monthLabel);
                    data.Add(Math.Round(revenue, 2));
                }
            }
            else
            {
                // For custom range, generate appropriate data
                TimeSpan range = EndDate - StartDate;

                if (range.TotalDays <= 7)
                {
                    // Similar to weekly
                    for (int i = 0; i <= range.TotalDays; i++)
                    {
                        DateTime date = StartDate.AddDays(i);
                        string dayLabel = date.ToString("MMM dd");
                        decimal revenue = random.Next(800, 2500) + Math.Round((decimal)random.NextDouble(), 2);

                        // Simulate higher sales on weekends
                        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                            revenue *= 1.4m;

                        labels.Add(dayLabel);
                        data.Add(Math.Round(revenue, 2));
                    }
                }
                else
                {
                    // For longer ranges, use a day interval approach
                    int dayInterval = range.TotalDays > 60 ? 7 : 1;

                    for (int i = 0; i <= range.TotalDays; i += dayInterval)
                    {
                        DateTime date = StartDate.AddDays(i);
                        string dateLabel = dayInterval > 1 ?
                            date.ToString("MMM dd") :
                            date.ToString("MM/dd");

                        decimal revenue = random.Next(1000, 3000) + Math.Round((decimal)random.NextDouble(), 2);

                        labels.Add(dateLabel);
                        data.Add(Math.Round(revenue, 2));
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"Generated {labels.Count} sample data points");
        }

        private void LoadCategoryRevenueData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Loading category revenue data");

                // Get connection string
                string connectionString = GetConnectionString();

                // Format dates for Oracle query
                string startDateParam = StartDate.ToString("yyyy-MM-dd");
                string endDateParam = EndDate.ToString("yyyy-MM-dd");

                System.Diagnostics.Debug.WriteLine($"Date range for query: {startDateParam} to {endDateParam}");

                // Lists to store chart data
                List<string> categoryNames = new List<string>();
                List<decimal> categoryRevenues = new List<decimal>();

                // Track if any data was found
                bool hasData = false;

                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    System.Diagnostics.Debug.WriteLine("Database connection opened");

                    // First, get all active parent categories (regardless of sales)
                    string categoryQuery = @"
                        SELECT 
                            CATEGORYID, NAME 
                        FROM 
                            CATEGORIES 
                        WHERE 
                            ISACTIVE = 1 AND
                            PARENTCATEGORYID IS NULL
                        ORDER BY 
                            NAME";

                    Dictionary<int, string> allCategories = new Dictionary<int, string>();
                    Dictionary<int, decimal> categoryRevenueMap = new Dictionary<int, decimal>();

                    using (OracleCommand catCommand = new OracleCommand(categoryQuery, connection))
                    {
                        using (OracleDataReader catReader = catCommand.ExecuteReader())
                        {
                            while (catReader.Read())
                            {
                                int categoryId = catReader.GetInt32(0);
                                string categoryName = catReader.GetString(1);
                                allCategories.Add(categoryId, categoryName);
                                categoryRevenueMap.Add(categoryId, 0); // Initialize revenue to 0
                            }
                        }
                    }

                    // If we have categories, get their revenue for the selected time period
                    if (allCategories.Count > 0)
                    {
                        // Query to get revenue by category
                        string revenueQuery = @"
                            SELECT * FROM (
                                SELECT 
                                    c.CATEGORYID,
                                    c.NAME AS CategoryName,
                                    NVL(SUM(NVL(od.QUANTITY, 0) * NVL(od.PRICE, 0)), 0) AS Revenue
                                FROM 
                                    CATEGORIES c
                                LEFT JOIN 
                                    PRODUCTCATEGORIES pc ON c.CATEGORYID = pc.CATEGORYID
                                LEFT JOIN 
                                    PRODUCTS p ON pc.PRODUCTID = p.PRODUCTID AND p.ISACTIVE = 1
                                LEFT JOIN 
                                    ORDERDETAILS od ON p.PRODUCTID = od.PRODUCTID
                                LEFT JOIN 
                                    ORDERS o ON od.ORDERID = o.ORDERID AND
                                    o.ORDERDATE BETWEEN TO_DATE(:startDate, 'YYYY-MM-DD')
                                        AND TO_DATE(:endDate, 'YYYY-MM-DD') + 0.99999
                                    AND o.ISACTIVE = 1
                                WHERE 
                                    c.ISACTIVE = 1 AND
                                    c.PARENTCATEGORYID IS NULL
                                GROUP BY 
                                    c.CATEGORYID, c.NAME
                                ORDER BY 
                                    Revenue DESC
                            ) WHERE ROWNUM <= 8";

                        using (OracleCommand revenueCommand = new OracleCommand(revenueQuery, connection))
                        {
                            revenueCommand.Parameters.Add(new OracleParameter("startDate", OracleDbType.Varchar2)).Value = startDateParam;
                            revenueCommand.Parameters.Add(new OracleParameter("endDate", OracleDbType.Varchar2)).Value = endDateParam;

                            using (OracleDataReader revenueReader = revenueCommand.ExecuteReader())
                            {
                                while (revenueReader.Read())
                                {
                                    int categoryId = revenueReader.GetInt32(0);
                                    decimal revenue = revenueReader.IsDBNull(2) ? 0 : revenueReader.GetDecimal(2);

                                    // Update the revenue map
                                    if (categoryRevenueMap.ContainsKey(categoryId))
                                    {
                                        categoryRevenueMap[categoryId] = revenue;
                                    }

                                    // Check if we have any revenue
                                    if (revenue > 0)
                                    {
                                        hasData = true;
                                    }
                                }
                            }
                        }

                        // Convert the category data to sorted lists for the chart
                        // Include all categories from our dictionary - this will include even those with zero revenue
                        var sortedCategories = categoryRevenueMap
                            .OrderByDescending(c => c.Value)
                            .Take(8); // Show top 8 categories

                        foreach (var category in sortedCategories)
                        {
                            int categoryId = category.Key;
                            decimal revenue = category.Value;
                            string categoryName = allCategories[categoryId];

                            categoryNames.Add(categoryName);
                            categoryRevenues.Add(revenue);

                            System.Diagnostics.Debug.WriteLine($"Category data: {categoryName} = {revenue}");
                        }

                        // If we have categories but no data, add at least some categories with zero revenue
                        if (categoryNames.Count == 0 && allCategories.Count > 0)
                        {
                            System.Diagnostics.Debug.WriteLine("No revenue data found, adding categories with zero revenue");
                            
                            // Add up to 8 categories with zero revenue
                            foreach (var category in allCategories.Take(8))
                            {
                                categoryNames.Add(category.Value);
                                categoryRevenues.Add(0);
                                System.Diagnostics.Debug.WriteLine($"Added zero-revenue category: {category.Value}");
                            }
                            
                            // We have data (categories) even if no revenue
                            hasData = true;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No active parent categories found");
                    }
                }

                // If no revenue was found for any category in this time period, show message
                if (!hasData)
                {
                    System.Diagnostics.Debug.WriteLine("No category revenue data found for the selected time period");
                }

                // Convert to JSON - Ensure no null values in the list
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                CategoryNames = serializer.Serialize(categoryNames);
                CategoryRevenueData = serializer.Serialize(categoryRevenues);

                // Display a message if no data found
                if (lblMessage != null)
                {
                    lblMessage.Visible = !hasData && categoryNames.Count == 0;
                }

                System.Diagnostics.Debug.WriteLine($"Category names: {CategoryNames}");
                System.Diagnostics.Debug.WriteLine($"Category revenue data: {CategoryRevenueData}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading category revenue data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Set empty arrays on error - no fallback data
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                CategoryNames = serializer.Serialize(new List<string>());
                CategoryRevenueData = serializer.Serialize(new List<decimal>());

                // Show message that no data is available
                if (lblMessage != null)
                {
                    lblMessage.Visible = true;
                }
            }
        }

        private void LoadTopProductsData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Loading top products data");

                // Get connection string
                string connectionString = GetConnectionString();

                // Format dates for Oracle query
                string startDateParam = StartDate.ToString("yyyy-MM-dd");
                string endDateParam = EndDate.ToString("yyyy-MM-dd");

                System.Diagnostics.Debug.WriteLine($"Date range for query: {startDateParam} to {endDateParam}");

                // Lists to store chart data
                List<string> productNames = new List<string>();
                List<int> productQuantities = new List<int>();

                // Track if any data was found
                bool hasData = false;

                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    System.Diagnostics.Debug.WriteLine("Database connection opened");

                    // Query to get top 5 products by quantity sold for the selected time period
                    string query = @"
                        SELECT * FROM (
                            SELECT 
                                p.NAME AS ProductName,
                                NVL(SUM(NVL(od.QUANTITY, 0)), 0) AS TotalQuantity
                            FROM 
                                PRODUCTS p
                            LEFT JOIN 
                                ORDERDETAILS od ON p.PRODUCTID = od.PRODUCTID
                            LEFT JOIN 
                                ORDERS o ON od.ORDERID = o.ORDERID AND
                                o.ORDERDATE BETWEEN TO_DATE(:startDate, 'YYYY-MM-DD') 
                                    AND TO_DATE(:endDate, 'YYYY-MM-DD') + 0.99999
                                AND o.ISACTIVE = 1
                            WHERE 
                                p.ISACTIVE = 1
                            GROUP BY 
                                p.NAME
                            ORDER BY 
                                TotalQuantity DESC
                        ) WHERE ROWNUM <= 5";

                    System.Diagnostics.Debug.WriteLine($"Executing query: {query}");

                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add(new OracleParameter("startDate", OracleDbType.Varchar2)).Value = startDateParam;
                        command.Parameters.Add(new OracleParameter("endDate", OracleDbType.Varchar2)).Value = endDateParam;

                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string productName = reader.GetString(0);
                                int quantity = reader.GetInt32(1);

                                // Only add products with quantity > 0 for this time period
                                if (quantity > 0)
                                {
                                    productNames.Add(productName);
                                    productQuantities.Add(quantity);
                                    hasData = true;

                                    System.Diagnostics.Debug.WriteLine($"Product data: {productName} = {quantity} units");
                                }
                            }
                        }
                    }
                }

                // If no data was found for the selected time period, use empty arrays
                if (!hasData)
                {
                    System.Diagnostics.Debug.WriteLine("No top products data found for the selected time period");
                    
                    // Empty arrays indicate no data - will display appropriately
                    productNames = new List<string>();
                    productQuantities = new List<int>();
                }

                // Convert to JSON - ensure we have valid data
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                TopProductNames = serializer.Serialize(productNames);
                TopProductQuantities = serializer.Serialize(productQuantities);

                // Update message visibility
                if (lblMessage != null)
                {
                    lblMessage.Visible = !hasData && !lblMessage.Visible; // Only show if not already visible
                }

                System.Diagnostics.Debug.WriteLine($"Top product names: {TopProductNames}");
                System.Diagnostics.Debug.WriteLine($"Top product quantities: {TopProductQuantities}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading top products data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Set empty arrays on error - no fallback data
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                TopProductNames = serializer.Serialize(new List<string>());
                TopProductQuantities = serializer.Serialize(new List<int>());

                // Show message that no data is available
                if (lblMessage != null)
                {
                    lblMessage.Visible = true;
                }
            }
        }

        private void LoadInventoryStatusData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Loading inventory status data");

                // Get connection string
                string connectionString = GetConnectionString();

                // Lists for data
                List<int> inventoryStatus = new List<int>();

                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();

                    // Create query to get inventory counts by status
                    string query = @"
                        SELECT
                            SUM(CASE WHEN STOCKQUANTITY > 20 THEN 1 ELSE 0 END) AS InStock,
                            SUM(CASE WHEN STOCKQUANTITY BETWEEN 1 AND 20 THEN 1 ELSE 0 END) AS LowStock,
                            SUM(CASE WHEN STOCKQUANTITY = 0 THEN 1 ELSE 0 END) AS OutOfStock
                        FROM PRODUCTS
                        WHERE ISACTIVE = 1";

                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Get inventory status counts
                                int inStock = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                int lowStock = reader.IsDBNull(2) ? 0 : reader.GetInt32(1);
                                int outOfStock = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);

                                inventoryStatus.Add(inStock);
                                inventoryStatus.Add(lowStock);
                                inventoryStatus.Add(outOfStock);

                                System.Diagnostics.Debug.WriteLine($"Inventory status: In Stock={inStock}, Low Stock={lowStock}, Out of Stock={outOfStock}");
                            }
                        }
                    }
                }

                // If no data or all zeros, use default values
                if (inventoryStatus.Count == 0 || inventoryStatus.Sum() == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No inventory status data found, using defaults");
                    inventoryStatus = new List<int> { 1, 1, 1 }; // Default placeholder data
                }

                // Convert to JSON
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                InventoryStatusData = serializer.Serialize(inventoryStatus);

                System.Diagnostics.Debug.WriteLine($"Inventory status data: {InventoryStatusData}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading inventory status data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Set default data on error
                InventoryStatusData = "[1, 1, 1]"; // Default data: In Stock, Low Stock, Out of Stock
            }
        }

        private void LoadProductRatingData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Loading product rating data for period: {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}");

                // Get connection string
                string connectionString = GetConnectionString();

                // Format dates for Oracle query
                string startDateParam = StartDate.ToString("yyyy-MM-dd");
                string endDateParam = EndDate.ToString("yyyy-MM-dd");

                // List to store products data
                List<ProductRatingModel> productRatings = new List<ProductRatingModel>();

                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();

                    // Get product performance based on orders in the selected time period
                    string query = @"
                        SELECT * FROM (
                            SELECT 
                                p.NAME AS ProductName,
                                NVL(AVG(5), 0) AS AverageRating,
                                COUNT(DISTINCT o.ORDERID) AS TotalOrders,
                                NVL(SUM(od.QUANTITY), 0) AS TotalQuantity,
                                CASE 
                                    WHEN NVL(p.STOCKQUANTITY, 0) = 0 THEN 0
                                    ELSE NVL(ROUND((NVL(SUM(od.QUANTITY), 0) / p.STOCKQUANTITY) * 100, 2), 0)
                                END AS ConversionRate
                            FROM 
                                PRODUCTS p
                            LEFT JOIN 
                                ORDERDETAILS od ON p.PRODUCTID = od.PRODUCTID
                            LEFT JOIN 
                                ORDERS o ON od.ORDERID = o.ORDERID AND 
                                o.ORDERDATE BETWEEN TO_DATE(:startDate, 'YYYY-MM-DD') AND 
                                TO_DATE(:endDate, 'YYYY-MM-DD') + 0.99999 AND
                                o.ISACTIVE = 1
                            WHERE 
                                p.ISACTIVE = 1
                            GROUP BY 
                                p.NAME, p.STOCKQUANTITY
                            ORDER BY 
                                TotalQuantity DESC
                        ) WHERE ROWNUM <= 5";

                    System.Diagnostics.Debug.WriteLine($"Executing query: {query}");

                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add(new OracleParameter("startDate", OracleDbType.Varchar2)).Value = startDateParam;
                        command.Parameters.Add(new OracleParameter("endDate", OracleDbType.Varchar2)).Value = endDateParam;

                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            bool hasData = false;

                            while (reader.Read())
                            {
                                hasData = true;

                                string productName = reader.GetString(0);
                                decimal averageRating = reader.IsDBNull(2) ? 0 : reader.GetDecimal(1);
                                int totalOrders = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                                int totalQuantity = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                                decimal conversionRate = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);

                                // Cap conversion rate at 100%
                                if (conversionRate > 100)
                                    conversionRate = 100;

                                productRatings.Add(new ProductRatingModel
                                {
                                    ProductName = productName,
                                    AverageRating = averageRating,
                                    TotalReviews = totalOrders,
                                    ConversionRate = conversionRate
                                });

                                System.Diagnostics.Debug.WriteLine($"Product rating data: {productName}, Rating={averageRating}, Orders={totalOrders}, Conversion={conversionRate}%");
                            }

                            // If no data was found, try a fallback query to get at least product names
                            if (!hasData)
                            {
                                System.Diagnostics.Debug.WriteLine("No product rating data found, using fallback query");

                                string fallbackQuery = @"
                                    SELECT * FROM (
                                        SELECT 
                                            NAME,
                                            0 AS AverageRating,
                                            0 AS TotalReviews,
                                            0 AS ConversionRate
                                        FROM 
                                            PRODUCTS 
                                        WHERE 
                                            ISACTIVE = 1
                                        ORDER BY 
                                            NAME
                                    ) WHERE ROWNUM <= 5";

                                using (OracleCommand fallbackCmd = new OracleCommand(fallbackQuery, connection))
                                {
                                    using (OracleDataReader fallbackReader = fallbackCmd.ExecuteReader())
                                    {
                                        while (fallbackReader.Read())
                                        {
                                            string productName = fallbackReader.GetString(0);

                                            productRatings.Add(new ProductRatingModel
                                            {
                                                ProductName = productName,
                                                AverageRating = 0,
                                                TotalReviews = 0,
                                                ConversionRate = 0
                                            });

                                            System.Diagnostics.Debug.WriteLine($"Fallback product: {productName}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // If still no products, add a placeholder
                if (productRatings.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No product data found, adding placeholder");

                    productRatings.Add(new ProductRatingModel
                    {
                        ProductName = "No Products Available",
                        AverageRating = 0,
                        TotalReviews = 0,
                        ConversionRate = 0
                    });
                }

                // Assign the product ratings
                ProductRatings = productRatings;

                System.Diagnostics.Debug.WriteLine($"Loaded {ProductRatings.Count} product ratings");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading product rating data: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Initialize with empty data
                ProductRatings = new List<ProductRatingModel>
                {
                    new ProductRatingModel
                    {
                        ProductName = "Error Loading Data",
                        AverageRating = 0,
                        TotalReviews = 0,
                        ConversionRate = 0
                    }
                };
            }
        }

        public class ProductRatingModel
        {
            public string ProductName { get; set; }
            public decimal AverageRating { get; set; }
            public int TotalReviews { get; set; }
            public decimal ConversionRate { get; set; }
        }
    }
}
