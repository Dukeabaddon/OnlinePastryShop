using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace OnlinePastryShop
{
    public class RevenueService
    {
        private readonly string _connectionString;

        public RevenueService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public decimal CalculateRevenue(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT
                            NVL(SUM(OD.PRICE * OD.QUANTITY - NVL(OD.COSTPRICE, 0) * OD.QUANTITY), 0) AS TotalRevenue
                        FROM
                            ORDERS O
                        JOIN
                            ORDERDETAILS OD ON O.ORDERID = OD.ORDERID
                        WHERE
                            O.ORDERDATE >= :StartDate AND O.ORDERDATE <= :EndDate
                        AND
                            O.STATUS IN ('Completed', 'Approved', 'Delivered', 'Shipped', 'Processing')";

                    System.Diagnostics.Debug.WriteLine("Executing revenue query for period " + startDate.ToString("yyyy-MM-dd") + " to " + endDate.ToString("yyyy-MM-dd"));
                    System.Diagnostics.Debug.WriteLine("Query: " + query);

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add("StartDate", OracleDbType.Date).Value = startDate;
                        cmd.Parameters.Add("EndDate", OracleDbType.Date).Value = endDate;

                        object result = cmd.ExecuteScalar();
                        decimal revenue = (result != null && result != DBNull.Value) ? Convert.ToDecimal(result) : 0;

                        System.Diagnostics.Debug.WriteLine("Revenue for period " + startDate.ToString("yyyy-MM-dd") + " to " + endDate.ToString("yyyy-MM-dd") + ": " + revenue);
                        return revenue;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error calculating revenue: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack trace: " + ex.StackTrace);
                return 0;
            }
        }

        public List<RevenueItem> GetRevenueDetails(DateTime startDate, DateTime endDate)
        {
            var items = new List<RevenueItem>();

            try
            {
                using (OracleConnection conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT
                            O.ORDERID,
                            O.ORDERDATE,
                            O.STATUS,
                            SUM(OD.PRICE * OD.QUANTITY - NVL(OD.COSTPRICE, 0) * OD.QUANTITY) AS OrderRevenue
                        FROM
                            ORDERS O
                        JOIN
                            ORDERDETAILS OD ON O.ORDERID = OD.ORDERID
                        WHERE
                            O.ORDERDATE >= :StartDate AND O.ORDERDATE <= :EndDate
                        AND
                            O.STATUS IN ('Completed', 'Approved', 'Delivered', 'Shipped', 'Processing')
                        GROUP BY
                            O.ORDERID, O.ORDERDATE, O.STATUS
                        ORDER BY
                            O.ORDERDATE DESC";

                    System.Diagnostics.Debug.WriteLine("Executing revenue details query for period " + startDate.ToString("yyyy-MM-dd") + " to " + endDate.ToString("yyyy-MM-dd"));

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add("StartDate", OracleDbType.Date).Value = startDate;
                        cmd.Parameters.Add("EndDate", OracleDbType.Date).Value = endDate;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new RevenueItem
                                {
                                    OrderId = Convert.ToInt32(reader["ORDERID"]),
                                    OrderDate = Convert.ToDateTime(reader["ORDERDATE"]),
                                    Status = reader["STATUS"].ToString(),
                                    Revenue = Convert.ToDecimal(reader["OrderRevenue"])
                                });
                            }
                        }
                    }

                    System.Diagnostics.Debug.WriteLine("Found " + items.Count + " orders for period " + startDate.ToString("yyyy-MM-dd") + " to " + endDate.ToString("yyyy-MM-dd"));
                    foreach (var item in items)
                    {
                        System.Diagnostics.Debug.WriteLine("- Order " + item.OrderId + ": Date=" + item.OrderDate.ToString("yyyy-MM-dd") + ", Status=" + item.Status + ", Revenue=" + item.Revenue);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting revenue details: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack trace: " + ex.StackTrace);
            }

            return items;
        }

        public Dictionary<string, decimal> GetAllPeriodRevenues()
        {
            var result = new Dictionary<string, decimal>();
            DateTime now = DateTime.Now;
            DateTime today = now.Date;
            DateTime yesterday = today.AddDays(-1);
            DateTime weekStart = today.AddDays(-7);
            DateTime monthStart = today.AddDays(-30);

            result["Today"] = CalculateRevenue(today, now);
            result["Yesterday"] = CalculateRevenue(yesterday, today.AddSeconds(-1));
            result["Week"] = CalculateRevenue(weekStart, now);
            result["Month"] = CalculateRevenue(monthStart, now);

            System.Diagnostics.Debug.WriteLine("=== ALL PERIOD REVENUES ===");
            foreach (var entry in result)
            {
                System.Diagnostics.Debug.WriteLine(entry.Key + ": " + entry.Value);
            }
            System.Diagnostics.Debug.WriteLine("==========================");

            return result;
        }
    }

    public class RevenueItem
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal Revenue { get; set; }
    }
}
