using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web.UI;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Web;

namespace OnlinePastryShop.Pages
{
    public partial class Default : System.Web.UI.Page
    {
        protected global::System.Web.UI.WebControls.Repeater rptFeaturedProducts;
        protected global::System.Web.UI.WebControls.Repeater rptCategories;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("========== Page_Load Start ==========");
                
                if (!IsPostBack)
                {
                    System.Diagnostics.Debug.WriteLine("Initial page load - loading top selling products");
                    LoadTopSellingProducts();
                    
                    // Load category counts
                    LoadCategoryCounts();
                }
                
                System.Diagnostics.Debug.WriteLine("========== Page_Load End ==========");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in Page_Load: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
        
        protected override void OnPreRender(EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("========== OnPreRender Start ==========");
                
                base.OnPreRender(e);
                
                // Data binding for the repeater
                if (rptFeaturedProducts != null)
                {
                    System.Diagnostics.Debug.WriteLine("Binding data to rptFeaturedProducts");
                    DataTable products = GetTopSellingProducts();
                    
                    if (products != null && products.Rows.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Binding {products.Rows.Count} products to repeater");
                        rptFeaturedProducts.DataSource = products;
                        rptFeaturedProducts.DataBind();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("WARNING: No products available for binding");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: rptFeaturedProducts control is null");
                }
                
                // Bind category data to the categories repeater
                if (rptCategories != null)
                {
                    System.Diagnostics.Debug.WriteLine("Binding data to rptCategories");
                    DataTable categories = GetCategoryData();
                    
                    if (categories != null && categories.Rows.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Binding {categories.Rows.Count} categories to repeater");
                        rptCategories.DataSource = categories;
                        rptCategories.DataBind();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("WARNING: No categories available for binding");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: rptCategories control is null");
                }
                
                System.Diagnostics.Debug.WriteLine("========== OnPreRender End ==========");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in OnPreRender: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void LoadTopSellingProducts()
        {
            using (OracleConnection conn = new OracleConnection(GetConnectionString()))
            {
                try
                {
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine("Database connection opened for LoadTopSellingProducts");
                    
                    // Modified SQL query to handle BLOB data correctly by separating the aggregation query
                    // from the image retrieval
                    string sql = @"
                        SELECT 
                            p.PRODUCTID,
                            p.NAME AS ProductName,
                            p.DESCRIPTION,
                            p.PRICE,
                            p.IMAGE
                        FROM 
                            AARON_IPT.PRODUCTS p
                        INNER JOIN (
                            SELECT 
                                p.PRODUCTID,
                                NVL(SUM(od.QUANTITY), 0) AS QuantitySold
                            FROM 
                                AARON_IPT.PRODUCTS p
                            LEFT JOIN 
                                AARON_IPT.ORDERDETAILS od ON p.PRODUCTID = od.PRODUCTID
                            LEFT JOIN 
                                AARON_IPT.ORDERS o ON od.ORDERID = o.ORDERID
                            WHERE 
                                (o.ORDERDATE BETWEEN :startDate AND :endDate OR o.ORDERDATE IS NULL)
                                AND (o.STATUS = 'Approved' OR o.STATUS IS NULL)
                                AND p.ISACTIVE = 1
                            GROUP BY 
                                p.PRODUCTID
                            ORDER BY 
                                NVL(SUM(od.QUANTITY), 0) DESC
                        ) sales ON p.PRODUCTID = sales.PRODUCTID
                        WHERE ROWNUM <= :limit
                        ORDER BY sales.QuantitySold DESC";
                    
                    OracleCommand cmd = new OracleCommand(sql, conn);
                    
                    // Add parameters
                    cmd.Parameters.Add(new OracleParameter("startDate", OracleDbType.Date)).Value = DateTime.Now.AddDays(-90);
                    cmd.Parameters.Add(new OracleParameter("endDate", OracleDbType.Date)).Value = DateTime.Now;
                    cmd.Parameters.Add(new OracleParameter("limit", OracleDbType.Int32)).Value = 4;
                    
                    System.Diagnostics.Debug.WriteLine("Executing top selling products query with fixed BLOB handling");
                    
                    // Create DataAdapter and fill DataTable
                    OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    
                    // Store in ViewState for binding to UI
                    if (dt.Rows.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found {dt.Rows.Count} top selling products");
                        // Log the column names to help with debugging
                        string columns = string.Join(", ", GetColumnNames(dt));
                        System.Diagnostics.Debug.WriteLine($"DataTable columns: {columns}");
                        
                        ViewState["TopSellingProducts"] = dt;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No top selling products found, loading any active products");
                        LoadAnyActiveProducts();
                    }
                }
                catch (Exception ex)
                {
                    // Log the error
                    System.Diagnostics.Debug.WriteLine($"Error loading top selling products: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    
                    // Fallback: Load any active products
                    LoadAnyActiveProducts();
                }
            }
        }
        
        private void LoadAnyActiveProducts()
        {
            using (OracleConnection conn = new OracleConnection(GetConnectionString()))
            {
                try
                {
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine("Database connection opened for LoadAnyActiveProducts");
                    
                    // Fixed Oracle 11g syntax for limiting rows with ROWNUM
                    string sql = @"
                        SELECT 
                            p.PRODUCTID,
                            p.NAME AS ProductName,
                            p.DESCRIPTION,
                            p.PRICE,
                            p.IMAGE
                        FROM 
                            AARON_IPT.PRODUCTS p
                        WHERE 
                            p.ISACTIVE = 1
                            AND ROWNUM <= 4
                        ORDER BY 
                            p.ISLATEST DESC, p.DATEMODIFIED DESC";
                    
                    OracleCommand cmd = new OracleCommand(sql, conn);
                    System.Diagnostics.Debug.WriteLine("Executing fallback products query");
                    
                    // Create DataAdapter and fill DataTable
                    OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    
                    // Store in ViewState for binding to UI
                    if (dt.Rows.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Loaded {dt.Rows.Count} active products as fallback");
                        // Log the column names to help with debugging
                        string columns = string.Join(", ", GetColumnNames(dt));
                        System.Diagnostics.Debug.WriteLine($"DataTable columns: {columns}");
                        
                        ViewState["TopSellingProducts"] = dt;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No active products found");
                    }
                }
                catch (Exception ex)
                {
                    // Log the error
                    System.Diagnostics.Debug.WriteLine($"Error loading fallback products: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }
        
        protected DataTable GetTopSellingProducts()
        {
            DataTable dt = ViewState["TopSellingProducts"] as DataTable;
            if (dt == null)
            {
                System.Diagnostics.Debug.WriteLine("WARNING: TopSellingProducts ViewState is null");
                return new DataTable(); // Return empty table instead of null
            }
            return dt;
        }
        
        // Helper method to get column names from DataTable for debugging
        private string[] GetColumnNames(DataTable dt)
        {
            string[] columnNames = new string[dt.Columns.Count];
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                columnNames[i] = dt.Columns[i].ColumnName;
            }
            return columnNames;
        }

        protected string GetProductImage(object imageData, string productName)
        {
            try
            {
                if (imageData != null && imageData != DBNull.Value)
                {
                    byte[] bytes = (byte[])imageData;
                    if (bytes.Length > 0)
                    {
                        string base64String = Convert.ToBase64String(bytes);
                        return $"<img src='data:image/jpeg;base64,{base64String}' alt='{HttpUtility.HtmlAttributeEncode(productName)}' class='w-full h-full object-cover' />";
                    }
                }
                
                // Return generic placeholder image without product name
                return $"<img src='../Images/product-placeholder.svg' alt='Product image coming soon' class='w-full h-full object-cover' />";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetProductImage: {ex.Message}");
                return $"<img src='../Images/product-placeholder.svg' alt='Error loading image' class='w-full h-full object-cover' />";
            }
        }

        protected int GetCategoryProductCount(string categoryName)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    string sql = @"
                        SELECT COUNT(*) 
                        FROM AARON_IPT.PRODUCTS p
                        JOIN AARON_IPT.PRODUCTCATEGORIES pc ON p.PRODUCTID = pc.PRODUCTID
                        JOIN AARON_IPT.CATEGORIES c ON pc.CATEGORYID = c.CATEGORYID
                        WHERE c.NAME = :categoryName
                        AND p.ISACTIVE = 1";
                    
                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("categoryName", OracleDbType.Varchar2)).Value = categoryName;
                        object result = cmd.ExecuteScalar();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting category count: {ex.Message}");
                // Return default values in case of error
                switch (categoryName)
                {
                    case "Cakes": return 12;
                    case "Breads": return 8;
                    case "Pastries": return 15;
                    case "Donuts": return 9;
                    case "Cupcakes": return 10;
                    case "Macaroons": return 8;
                    default: return 0;
                }
            }
        }

        private string GetConnectionString()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["OracleConnection"]?.ConnectionString;

                if (string.IsNullOrEmpty(connectionString))
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: Oracle connection string is missing or empty in the configuration file. Using fallback connection string.");
                    return "User Id=Aaron_IPT;Password=qwen123;Data Source=localhost:1521/xe;";
                }

                return connectionString;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR retrieving connection string: {ex.Message}");
                // Return fallback connection string instead of throwing exception
                return "User Id=Aaron_IPT;Password=qwen123;Data Source=localhost:1521/xe;";
            }
        }

        private void LoadCategoryCounts()
        {
            try
            {
                // Create a DataTable to store category information
                DataTable dt = new DataTable();
                dt.Columns.Add("CategoryName", typeof(string));
                dt.Columns.Add("ItemCount", typeof(int));
                dt.Columns.Add("ImagePath", typeof(string));
                
                // Add rows for each category
                dt.Rows.Add("Cakes", GetCategoryProductCount("Cakes"), "../Images/cake.jpg");
                dt.Rows.Add("Breads", GetCategoryProductCount("Breads"), "../Images/bread.png");
                dt.Rows.Add("Pastries", GetCategoryProductCount("Pastries"), "../Images/pastriess.png");
                dt.Rows.Add("Donuts", GetCategoryProductCount("Donuts"), "../Images/donuts.jpg");
                dt.Rows.Add("Cupcakes", GetCategoryProductCount("Cupcakes"), "../Images/cupcakes.png");
                dt.Rows.Add("Macaroons", GetCategoryProductCount("Macaroons"), "../Images/macaroons.jpg");
                
                // Store in ViewState
                ViewState["CategoryCounts"] = dt;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading category counts: {ex.Message}");
            }
        }
        
        protected DataTable GetCategoryData()
        {
            DataTable dt = ViewState["CategoryCounts"] as DataTable;
            if (dt == null)
            {
                LoadCategoryCounts();
                dt = ViewState["CategoryCounts"] as DataTable;
            }
            return dt ?? new DataTable();
        }
    }
}