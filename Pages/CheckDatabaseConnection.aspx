using System;
using System.Web;
using System.Web.Script.Serialization;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;

namespace OnlinePastryShop.Pages
{
    public partial class CheckDatabaseConnection : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool isConnected = false;
            string message = "Database connection failed";
            
            try
            {
                using (OracleConnection connection = new OracleConnection(GetConnectionString()))
                {
                    // Try to open a connection with a short timeout
                    connection.Open();
                    
                    // If we got here, connection is successful
                    isConnected = true;
                    message = "Database connection successful";
                }
            }
            catch (Exception ex)
            {
                // Connection failed
                message = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Database connection check failed: {ex.Message}");
            }
            
            // Create a response object
            var response = new { 
                isConnected = isConnected, 
                message = message,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            // Serialize to JSON
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string jsonResponse = serializer.Serialize(response);
            
            // Write the JSON response
            Response.Clear();
            Response.ContentType = "application/json";
            Response.Write(jsonResponse);
            Response.End();
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