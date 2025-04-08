using System;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace OnlinePastryShop.Pages
{
    public partial class GetProductImage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string productIdString = Request.QueryString["id"];
                
                if (!string.IsNullOrEmpty(productIdString) && int.TryParse(productIdString, out int productId))
                {
                    ServeProductImage(productId);
                }
                else
                {
                    // Redirect to default image
                    Response.Redirect("~/Images/no-image.png");
                }
            }
        }

        private void ServeProductImage(int productId)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    
                    string sql = "SELECT IMAGE FROM PRODUCTS WHERE PRODUCTID = :ProductId";
                    
                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("ProductId", productId));
                        
                        object result = cmd.ExecuteScalar();
                        
                        if (result != null && result != DBNull.Value)
                        {
                            byte[] imageData = (byte[])result;
                            Response.ContentType = "image/jpeg"; // Assuming JPEG format, adjust if needed
                            Response.BinaryWrite(imageData);
                        }
                        else
                        {
                            // Return fallback image
                            Response.Redirect("~/Images/no-image.png");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving product image: {ex.Message}");
                Response.Redirect("~/Images/no-image.png");
            }
        }

        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString;
        }
    }
} 