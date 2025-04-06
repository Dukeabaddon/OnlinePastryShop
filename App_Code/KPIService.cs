using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;

namespace OnlinePastryShop
{
    public class KPIService
    {
        private readonly string _connectionString;

        public KPIService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Gets revenue data for the specified date range
        /// </summary>
        public KPIData GetRevenueData(DateTime startDate, DateTime endDate, DateTime prevStartDate, DateTime prevEndDate)
        {
            KPIData result = new KPIData();

            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                // Get current period revenue - using the unit sold * price calculation
                using (OracleCommand cmd = new OracleCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"
                        SELECT NVL(SUM(OD.QUANTITY * OD.PRICE), 0) AS Revenue
                        FROM ORDERS O
                        JOIN ORDERDETAILS OD ON O.ORDERID = OD.ORDERID
                        WHERE O.ORDERDATE BETWEEN :StartDate AND :EndDate
                        AND O.STATUS IN ('Completed', 'Approved', 'Delivered', 'Shipped')";

                    cmd.Parameters.Add(new OracleParameter("StartDate", OracleDbType.Date) { Value = startDate });
                    cmd.Parameters.Add(new OracleParameter("EndDate", OracleDbType.Date) { Value = endDate });

                    result.CurrentValue = Convert.ToDecimal(cmd.ExecuteScalar());

                    // Get previous period revenue
                    cmd.Parameters.Clear();
                    cmd.CommandText = @"
                        SELECT NVL(SUM(OD.QUANTITY * OD.PRICE), 0) AS Revenue
                        FROM ORDERS O
                        JOIN ORDERDETAILS OD ON O.ORDERID = OD.ORDERID
                        WHERE O.ORDERDATE BETWEEN :PrevStartDate AND :PrevEndDate
                        AND O.STATUS IN ('Completed', 'Approved', 'Delivered', 'Shipped')";

                    cmd.Parameters.Add(new OracleParameter("PrevStartDate", OracleDbType.Date) { Value = prevStartDate });
                    cmd.Parameters.Add(new OracleParameter("PrevEndDate", OracleDbType.Date) { Value = prevEndDate });

                    result.PreviousValue = Convert.ToDecimal(cmd.ExecuteScalar());

                    // Calculate percentage change
                    if (result.PreviousValue > 0)
                    {
                        result.PercentChange = ((result.CurrentValue - result.PreviousValue) / result.PreviousValue) * 100;
                    }
                    else
                    {
                        result.PercentChange = 0;
                    }

                    // Set display properties
                    result.DisplayValue = result.CurrentValue.ToString("N2");
                    result.DisplayChange = Math.Abs(result.PercentChange).ToString("N1");

                    if (result.PercentChange >= 0)
                    {
                        result.ChangeClass = "text-green-600";
                        result.ChangeIcon = "M5 10l7-7m0 0l7 7m-7-7v18";
                    }
                    else
                    {
                        result.ChangeClass = "text-red-600";
                        result.ChangeIcon = "M19 14l-7 7m0 0l-7-7m7 7V3";
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets orders data for the specified date range
        /// </summary>
        public KPIData GetOrdersData(DateTime startDate, DateTime endDate, DateTime prevStartDate, DateTime prevEndDate)
        {
            KPIData result = new KPIData();

            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                conn.Open();

                // Get current period order count
                using (OracleCommand cmd = new OracleCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"
                        SELECT COUNT(DISTINCT O.ORDERID) AS OrderCount
                        FROM ORDERS O
                        WHERE O.ORDERDATE BETWEEN :StartDate AND :EndDate
                        AND O.STATUS IN ('Completed', 'Approved', 'Delivered', 'Shipped', 'Processing')";

                    cmd.Parameters.Add(new OracleParameter("StartDate", OracleDbType.Date) { Value = startDate });
                    cmd.Parameters.Add(new OracleParameter("EndDate", OracleDbType.Date) { Value = endDate });

                    result.CurrentValue = Convert.ToDecimal(cmd.ExecuteScalar());

                    // Get previous period order count
                    cmd.Parameters.Clear();
                    cmd.CommandText = @"
                        SELECT COUNT(DISTINCT O.ORDERID) AS OrderCount
                        FROM ORDERS O
                        WHERE O.ORDERDATE BETWEEN :PrevStartDate AND :PrevEndDate
                        AND O.STATUS IN ('Completed', 'Approved', 'Delivered', 'Shipped', 'Processing')";

                    cmd.Parameters.Add(new OracleParameter("PrevStartDate", OracleDbType.Date) { Value = prevStartDate });
                    cmd.Parameters.Add(new OracleParameter("PrevEndDate", OracleDbType.Date) { Value = prevEndDate });

                    result.PreviousValue = Convert.ToDecimal(cmd.ExecuteScalar());

                    // Calculate percentage change
                    if (result.PreviousValue > 0)
                    {
                        result.PercentChange = ((result.CurrentValue - result.PreviousValue) / result.PreviousValue) * 100;
                    }
                    else
                    {
                        result.PercentChange = 0;
                    }

                    // Set display properties
                    result.DisplayValue = result.CurrentValue.ToString("N0");
                    result.DisplayChange = Math.Abs(result.PercentChange).ToString("N1");

                    if (result.PercentChange >= 0)
                    {
                        result.ChangeClass = "text-green-600";
                        result.ChangeIcon = "M5 10l7-7m0 0l7 7m-7-7v18";
                    }
                    else
                    {
                        result.ChangeClass = "text-red-600";
                        result.ChangeIcon = "M19 14l-7 7m0 0l-7-7m7 7V3";
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the appropriate card title and comparison text based on time range
        /// </summary>
        public static void GetCardLabels(string timeRange, out string revenueCardTitle, out string revenueComparisonText,
                                        out string orderCardTitle, out string orderComparisonText)
        {
            switch (timeRange)
            {
                case "today":
                    revenueCardTitle = "Today's Revenue";
                    revenueComparisonText = "vs yesterday";
                    orderCardTitle = "Today's Orders";
                    orderComparisonText = "vs yesterday";
                    break;

                case "yesterday":
                    revenueCardTitle = "Yesterday's Revenue";
                    revenueComparisonText = "vs previous day";
                    orderCardTitle = "Yesterday's Orders";
                    orderComparisonText = "vs previous day";
                    break;

                case "week":
                    revenueCardTitle = "This Week's Revenue";
                    revenueComparisonText = "vs previous week";
                    orderCardTitle = "This Week's Orders";
                    orderComparisonText = "vs previous week";
                    break;

                case "month":
                    revenueCardTitle = "This Month's Revenue";
                    revenueComparisonText = "vs previous month";
                    orderCardTitle = "This Month's Orders";
                    orderComparisonText = "vs previous month";
                    break;

                default:
                    revenueCardTitle = "Today's Revenue";
                    revenueComparisonText = "vs yesterday";
                    orderCardTitle = "Today's Orders";
                    orderComparisonText = "vs yesterday";
                    break;
            }
        }
    }

    /// <summary>
    /// Data structure for KPI metrics
    /// </summary>
    public class KPIData
    {
        public decimal CurrentValue { get; set; }
        public decimal PreviousValue { get; set; }
        public decimal PercentChange { get; set; }
        public string DisplayValue { get; set; }
        public string DisplayChange { get; set; }
        public string ChangeClass { get; set; }
        public string ChangeIcon { get; set; }
    }
}
