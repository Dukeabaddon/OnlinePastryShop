using System;
using System.Configuration;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace OnlinePastryShop.Pages
{
    public partial class AdminMaster : System.Web.UI.MasterPage
    {
        // Properties for badge counters
        public int PendingOrdersCount { get; private set; }
        public int LowStockCount { get; private set; }
        public int NewOrdersCount { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Load badge counts
                LoadBadgeCounts();
            }
        }

        // Admin logout handler - clear session and redirect to Default.aspx
        protected void lnkAdminLogout_Click(object sender, EventArgs e)
        {
            try
            {
                // Clear session variables
                Session.Clear();
                Session.Abandon();
                
                // Log for debugging
                System.Diagnostics.Debug.WriteLine("Admin logout executed - session cleared");
            }
            catch (Exception ex)
            {
                // Log any errors but continue with redirect
                System.Diagnostics.Debug.WriteLine($"Error during admin logout: {ex.Message}");
            }
            finally
            {
                // Always redirect to Default.aspx, even if an error occurs
                Response.Redirect("~/Pages/Default.aspx");
            }
        }

        private void LoadBadgeCounts()
        {
            // Load pending orders count
            PendingOrdersCount = GetPendingOrdersCount();

            // Load low stock products count
            LowStockCount = GetLowStockCount();

            // Load new orders count (orders created in the past 24 hours)
            NewOrdersCount = GetNewOrdersCount();
        }

        private int GetPendingOrdersCount()
        {
            string query = @"
                SELECT COUNT(OrderId) AS PendingCount
                FROM Orders
                WHERE Status = 'Pending'
                AND IsActive = 1
            ";

            return ExecuteScalarQuery(query);
        }

        private int GetLowStockCount()
        {
            string query = @"
                SELECT COUNT(ProductId) AS LowStockCount
                FROM Products
                WHERE StockQuantity <= 10
                AND IsActive = 1
            ";

            return ExecuteScalarQuery(query);
        }

        private int GetNewOrdersCount()
        {
            string query = @"
                SELECT COUNT(OrderId) AS NewOrdersCount
                FROM Orders
                WHERE OrderDate >= SYSDATE - 1 -- Past 24 hours
                AND IsActive = 1
            ";

            return ExecuteScalarQuery(query);
        }

        private int ExecuteScalarQuery(string query)
        {
            try
            {
                using (OracleConnection connection = new OracleConnection(GetConnectionString()))
                {
                    connection.Open();
                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        object result = command.ExecuteScalar();
                        return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error executing query: {ex.Message}");
                return 0; // Return 0 on error
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
                    // Log the error
                    System.Diagnostics.Debug.WriteLine("ERROR: OracleConnection string not found in Web.config");

                    // Return a hardcoded backup connection string
                    return "User Id=mecate;Password=qwen123;Data Source=localhost:1521/xe;";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR getting connection string: {ex.Message}");

                // Fallback to hardcoded connection string
                return "User Id=mecate;Password=qwen123;Data Source=localhost:1521/xe;";
            }
        }
    }
}