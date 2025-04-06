<%@ WebHandler Language="C#" Class="OnlinePastryShop.GetProductImage" %>

using System;
using System.Web;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;

namespace OnlinePastryShop
{
    public class GetProductImage : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                // Get product ID from query parameter
                string idParam = context.Request.QueryString["id"];
                if (string.IsNullOrEmpty(idParam))
                {
                    context.Response.StatusCode = 400;
                    context.Response.End();
                    return;
                }

                int productId;
                if (!int.TryParse(idParam, out productId) || productId <= 0)
                {
                    context.Response.StatusCode = 400;
                    context.Response.End();
                    return;
                }

                // Get image data from database
                byte[] imageData = GetProductImageData(productId);
                if (imageData == null || imageData.Length == 0)
                {
                    // If no image, return 404
                    context.Response.StatusCode = 404;
                    context.Response.End();
                    return;
                }

                // Set response content type to JPEG image (most products will likely be JPEG)
                context.Response.ContentType = "image/jpeg";
                
                // Add cache control headers (cache for 1 day)
                context.Response.Cache.SetCacheability(HttpCacheability.Public);
                context.Response.Cache.SetExpires(DateTime.Now.AddDays(1));
                context.Response.Cache.SetMaxAge(new TimeSpan(1, 0, 0, 0));
                context.Response.Cache.SetValidUntilExpires(true);
                
                // Set ETag based on product ID and last modified time
                context.Response.Cache.SetETag($"\"{productId}\"");

                // Write image data to response
                context.Response.BinaryWrite(imageData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetProductImage: {ex.Message}");
                context.Response.StatusCode = 500;
                context.Response.Write("Error retrieving image");
            }
            finally
            {
                context.Response.End();
            }
        }

        private byte[] GetProductImageData(int productId)
        {
            using (OracleConnection conn = new OracleConnection(GetConnectionString()))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand(@"SELECT IMAGE FROM ""AARON_IPT"".""PRODUCTS"" WHERE PRODUCTID = :ProductId AND ISACTIVE = 1", conn))
                    {
                        cmd.Parameters.Add("ProductId", OracleDbType.Int32).Value = productId;
                        
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return (byte[])result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Database error in GetProductImageData: {ex.Message}");
                }
                
                return null;
            }
        }
        
        private string GetConnectionString()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["OracleConnection"]?.ConnectionString;
            
            if (string.IsNullOrEmpty(connectionString))
            {
                System.Diagnostics.Debug.WriteLine("WARNING: Oracle connection string is missing or empty in the configuration file. Using fallback connection string.");
                return "User Id=mecate;Password=qwen123;Data Source=localhost:1521/xe;";
            }
            
            return connectionString;
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
} 