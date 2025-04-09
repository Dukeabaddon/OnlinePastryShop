using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Script.Services;
using System.Web.Services;
using Oracle.ManagedDataAccess.Client;
using System.Web;

namespace OnlinePastryShop.Pages
{
    public partial class Menu : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // This is sufficient as we now have the ScriptManager with EnablePageMethods=true in the ASPX file
            if (!IsPostBack)
            {
                // Additional initialization if needed
            }
        }

        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetProducts()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Getting products for Menu page");
                var products = new List<Dictionary<string, object>>();
                var uniqueProducts = new Dictionary<int, Dictionary<string, object>>();

                using (var connection = new OracleConnection(GetConnectionString()))
                {
                    connection.Open();
                    System.Diagnostics.Debug.WriteLine("Database connection opened");

                    string query = @"
                        SELECT 
                            p.PRODUCTID, 
                            p.NAME, 
                            p.DESCRIPTION, 
                            p.PRICE, 
                            p.STOCKQUANTITY, 
                            p.ISLATEST,
                            p.ISACTIVE,
                            p.IMAGE,
                            c.CATEGORYID,
                            c.NAME AS CATEGORYNAME
                        FROM 
                            PRODUCTS p
                        LEFT JOIN 
                            PRODUCTCATEGORIES pc ON p.PRODUCTID = pc.PRODUCTID
                        LEFT JOIN 
                            CATEGORIES c ON pc.CATEGORYID = c.CATEGORYID
                        WHERE 
                            p.ISACTIVE = 1 
                        ORDER BY 
                            p.NAME ASC";

                    using (var cmd = new OracleCommand(query, connection))
                    {
                        System.Diagnostics.Debug.WriteLine("Executing product query");
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int productId = Convert.ToInt32(reader["PRODUCTID"]);
                                System.Diagnostics.Debug.WriteLine($"Processing product ID: {productId}");

                                // Check if this product is already in the dictionary
                                if (!uniqueProducts.ContainsKey(productId))
                                {
                                    // Process image data
                                    string imageBase64 = null;
                                    bool hasImage = false;
                                    
                                    if (reader["IMAGE"] != DBNull.Value)
                                    {
                                        try
                                        {
                                            byte[] imageData = (byte[])reader["IMAGE"];
                                            if (imageData != null && imageData.Length > 0)
                                            {
                                                System.Diagnostics.Debug.WriteLine($"Processing image for product {productId}. Image size: {imageData.Length} bytes");
                                                imageBase64 = Convert.ToBase64String(imageData);
                                                hasImage = true;
                                                System.Diagnostics.Debug.WriteLine($"Successfully converted image to base64 for product {productId}");
                                            }
                                            else
                                            {
                                                System.Diagnostics.Debug.WriteLine($"Image data is empty for product {productId}");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            System.Diagnostics.Debug.WriteLine($"Error converting image for product {productId}: {ex.Message}");
                                        }
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine($"No image found for product {productId}");
                                    }

                                    // Create dictionary for the product
                                    var product = new Dictionary<string, object>
                                    {
                                        { "ProductId", productId },
                                        { "Name", reader["NAME"].ToString() },
                                        { "Description", reader["DESCRIPTION"].ToString() },
                                        { "Price", Convert.ToDecimal(reader["PRICE"]) },
                                        { "StockQuantity", Convert.ToInt32(reader["STOCKQUANTITY"]) },
                                        { "IsLatest", Convert.ToInt32(reader["ISLATEST"]) == 1 },
                                        { "IsActive", Convert.ToInt32(reader["ISACTIVE"]) == 1 },
                                        { "HasImage", hasImage },
                                        { "ImageBase64", imageBase64 }
                                    };

                                    // Add category information if available
                                    if (reader["CATEGORYID"] != DBNull.Value)
                                    {
                                        product.Add("CategoryId", Convert.ToInt32(reader["CATEGORYID"]));
                                        product.Add("CategoryName", reader["CATEGORYNAME"].ToString());
                                        System.Diagnostics.Debug.WriteLine($"Adding category: {reader["CATEGORYNAME"]} (ID: {reader["CATEGORYID"]})");
                                    }

                                    // Add to the uniqueProducts dictionary
                                    uniqueProducts.Add(productId, product);
                                }
                            }
                        }
                    }
                }

                // Convert the dictionary values to a list
                products = new List<Dictionary<string, object>>(uniqueProducts.Values);
                System.Diagnostics.Debug.WriteLine($"Found {products.Count} products");

                return new
                {
                    Success = true,
                    Products = products
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving products: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                return new
                {
                    Success = false,
                    Message = "Error retrieving products. Please try again later."
                };
            }
        }

        // This method is no longer needed since we're using base64 encoding directly in the response
        // But we'll keep it for now in case it's referenced elsewhere
        public void GetProductImage(int productId)
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
                            // Return fallback image or 404
                            Response.Redirect("~/Images/product-placeholder.svg");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving product image: {ex.Message}");
                Response.Redirect("~/Images/product-placeholder.svg");
            }
            finally
            {
                Response.End();
            }
        }
    }
}