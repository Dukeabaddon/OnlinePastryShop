using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Oracle.ManagedDataAccess.Client;
using System.Text;

namespace OnlinePastryShop.Pages
{
    public partial class ProductDetails : System.Web.UI.Page
    {
        // Protected properties for product data
        protected int ProductId { get; set; }
        protected decimal AverageRating { get; set; }
        protected int RatingCount { get; set; }
        protected bool IsUserLoggedIn { get; set; }
        protected string ProductName { get; set; }
        protected string ProductDescription { get; set; }
        protected decimal ProductPrice { get; set; }
        protected int StockQuantity { get; set; }
        protected byte[] ProductImage { get; set; }

        // Control declarations
        protected Literal litProductName;
        protected Literal litDescription;
        protected Literal litPrice;
        protected Literal litCategory;
        protected Literal litRating;
        protected Image imgProduct;
        protected HtmlGenericControl divStockStatus;
        protected Button btnAddToCart;
        protected HiddenField hiddenQuantity;
        protected Literal litBreadcrumbProduct;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Check if user is logged in
                IsUserLoggedIn = HttpContext.Current.User.Identity.IsAuthenticated;

                // Get product ID from query string
                if (!int.TryParse(Request.QueryString["id"], out int productId))
                {
                    // Invalid or missing product ID
                    ShowErrorMessage("Invalid product ID");
                    return;
                }

                ProductId = productId;

                // Load product details
                try
                {
                    LoadProductDetails();
                }
                catch (Exception ex)
                {
                    ShowErrorMessage("Error loading product details: " + ex.Message);
                }
            }
        }

        private void ShowErrorMessage(string message)
        {
            // If we had an error panel, we'd show it here
            // For now, we'll use JavaScript alert
            ScriptManager.RegisterStartupScript(this, GetType(), "alert", 
                $"alert('{message}'); window.location.href = '/Pages/Menu.aspx';", true);
        }

        private void LoadProductDetails()
        {
            using (var connection = new OracleConnection(GetConnectionString()))
            {
                connection.Open();

                // First, get product details
                string productQuery = @"
                    SELECT 
                        p.NAME, 
                        p.DESCRIPTION, 
                        p.PRICE, 
                        p.STOCKQUANTITY, 
                        p.IMAGE,
                        c.NAME AS CATEGORYNAME
                    FROM 
                        PRODUCTS p
                    LEFT JOIN 
                        PRODUCTCATEGORIES pc ON p.PRODUCTID = pc.PRODUCTID
                    LEFT JOIN 
                        CATEGORIES c ON pc.CATEGORYID = c.CATEGORYID
                    WHERE 
                        p.PRODUCTID = :ProductId
                        AND p.ISACTIVE = 1";

                using (var cmd = new OracleCommand(productQuery, connection))
                {
                    cmd.Parameters.Add(new OracleParameter("ProductId", ProductId));
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Store product details in properties
                            ProductName = reader["NAME"].ToString();
                            ProductDescription = reader["DESCRIPTION"].ToString();
                            ProductPrice = Convert.ToDecimal(reader["PRICE"]);
                            StockQuantity = Convert.ToInt32(reader["STOCKQUANTITY"]);
                            
                            if (reader["IMAGE"] != DBNull.Value)
                            {
                                ProductImage = (byte[])reader["IMAGE"];
                            }

                            // Get category name if available
                            string categoryName = reader["CATEGORYNAME"] != DBNull.Value 
                                ? reader["CATEGORYNAME"].ToString() 
                                : string.Empty;

                            // Set page title
                            Page.Title = ProductName + " - Pastry Palace";
                            
                            // Now, get ratings for this product
                            LoadProductRatings();
                            
                            // Bind data to the UI controls
                            BindProductData(categoryName);
                        }
                        else
                        {
                            // Product not found or inactive
                            ShowErrorMessage("Product not found");
                        }
                    }
                }
            }
        }

        private void LoadProductRatings()
        {
            using (var connection = new OracleConnection(GetConnectionString()))
            {
                connection.Open();

                string ratingsQuery = @"
                    SELECT 
                        AVG(RATING) AS AVERAGE_RATING,
                        COUNT(RATINGID) AS RATING_COUNT
                    FROM 
                        PRODUCTRATINGS
                    WHERE 
                        PRODUCTID = :ProductId
                        AND ISAPPROVED = 1";

                using (var cmd = new OracleCommand(ratingsQuery, connection))
                {
                    cmd.Parameters.Add(new OracleParameter("ProductId", ProductId));
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Handle NULL value for average (no ratings yet)
                            if (reader["AVERAGE_RATING"] != DBNull.Value)
                            {
                                AverageRating = Convert.ToDecimal(reader["AVERAGE_RATING"]);
                            }
                            else
                            {
                                AverageRating = 0;
                            }

                            RatingCount = Convert.ToInt32(reader["RATING_COUNT"]);
                        }
                        else
                        {
                            // No ratings found, set defaults
                            AverageRating = 0;
                            RatingCount = 0;
                        }
                    }
                }
            }
        }

        private void BindProductData(string categoryName = "")
        {
            // Set product category
            if (litCategory != null)
                litCategory.Text = categoryName;
                
            // Set product name
            if (litProductName != null)
            {
                litProductName.Text = ProductName;
                // Also set the breadcrumb product name
                if (litBreadcrumbProduct != null)
                    litBreadcrumbProduct.Text = ProductName;
            }

            // Set product price (formatted with Philippine Peso ₱ instead of $)
            if (litPrice != null)
                litPrice.Text = string.Format("₱{0:0.00}", ProductPrice);

            // Set product description
            if (litDescription != null)
                litDescription.Text = ProductDescription;

            // Set product image
            if (imgProduct != null && ProductImage != null)
            {
                string base64String = Convert.ToBase64String(ProductImage);
                imgProduct.ImageUrl = "data:image/jpeg;base64," + base64String;
            }
            else if (imgProduct != null)
            {
                // Use placeholder image if no product image is available
                imgProduct.ImageUrl = "../Images/product-placeholder.svg";
            }

            // Handle stock status and Add to Cart button
            if (divStockStatus != null)
            {
                if (StockQuantity <= 0)
                {
                    divStockStatus.InnerHtml = "<span class='text-red-600 font-semibold'>Out of Stock</span>";
                    if (btnAddToCart != null)
                    {
                        btnAddToCart.Enabled = false;
                        btnAddToCart.CssClass = btnAddToCart.CssClass.Replace("bg-[#96744F] hover:bg-[#7a5f3e]", "bg-gray-400");
                        btnAddToCart.Text = "Out of Stock";
                    }
                }
                else if (StockQuantity <= 10)
                {
                    // Low stock warning
                    divStockStatus.InnerHtml = $"<span class='text-red-600 font-medium'>Low Stock: Only {StockQuantity} left!</span>";
                }
                else
                {
                    divStockStatus.InnerHtml = "<span class='text-green-600 font-medium'>In Stock</span>";
                }
            }

            // Handle ratings display
            if (litRating != null)
            {
                // Generate stars HTML
                string stars = GenerateStarsHtml(AverageRating);
                string ratingText = RatingCount > 0 
                    ? $"{stars} ({RatingCount} review{(RatingCount != 1 ? "s" : "")})"
                    : "No reviews yet";
                
                litRating.Text = ratingText;
            }

            // Handle guest users for Add to Cart button
            if (!IsUserLoggedIn && btnAddToCart != null && btnAddToCart.Enabled)
            {
                // Option B: Set JavaScript to redirect to login
                btnAddToCart.OnClientClick = "window.location.href='/Pages/Login.aspx'; return false;";
                btnAddToCart.ToolTip = "Please log in to add items to your cart";
            }

            // Load related products
            LoadRelatedProducts(ProductId, categoryName);
        }

        private string GenerateStarsHtml(decimal rating)
        {
            StringBuilder stars = new StringBuilder();
            
            // Convert rating to half stars (e.g., 3.5 => 7 half stars)
            int halfStars = (int)Math.Round(rating * 2);
            
            // Output full and half stars
            for (int i = 0; i < 5; i++)
            {
                if (halfStars >= 2)
                {
                    // Full star
                    stars.Append("<i class='fas fa-star text-yellow-500'></i>");
                    halfStars -= 2;
                }
                else if (halfStars == 1)
                {
                    // Half star
                    stars.Append("<i class='fas fa-star-half-alt text-yellow-500'></i>");
                    halfStars -= 1;
                }
                else
                {
                    // Empty star
                    stars.Append("<i class='far fa-star text-yellow-500'></i>");
                }
            }
            
            return stars.ToString();
        }

        protected void btnAddToCart_Click(object sender, EventArgs e)
        {
            if (!IsUserLoggedIn)
            {
                // Redirect to login if somehow a non-logged in user submits the form
                Response.Redirect("/Pages/Login.aspx");
                return;
            }

            if (StockQuantity <= 0)
            {
                // Don't process if out of stock
                return;
            }

            // Get selected quantity from hidden field
            int quantity = 1;
            if (!string.IsNullOrEmpty(hiddenQuantity.Value) && int.TryParse(hiddenQuantity.Value, out int parsedQuantity))
            {
                quantity = parsedQuantity;
            }

            // TODO: Implement actual cart functionality
            // For now, just show success message
            ScriptManager.RegisterStartupScript(this, GetType(), "cartSuccess", 
                "showCartSuccessAnimation();", true);
        }

        private void LoadRelatedProducts(int currentProductId, string category)
        {
            try
            {
                DataTable relatedProducts = GetRelatedProducts(currentProductId, category);
                
                if (relatedProducts != null && relatedProducts.Rows.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    
                    // Create JavaScript that will directly append DOM elements
                    sb.Append(@"
                    function loadRelatedProducts() {
                        console.log('Loading related products...');
                        const container = document.getElementById('relatedProductsContainer');
                        
                        if (!container) {
                            console.error('Related products container not found');
                            return;
                        }
                        
                        // Clear loading indicator
                        container.innerHTML = '';
                        
                        // Log related products found
                        console.log('Found " + relatedProducts.Rows.Count + @" related products');
                    
                    ");
                    
                    // Build JavaScript for each product
                    for (int i = 0; i < relatedProducts.Rows.Count; i++)
                    {
                        DataRow row = relatedProducts.Rows[i];
                        string productId = row["ProductID"].ToString();
                        string name = row["Name"].ToString();
                        string price = row["Price"].ToString();
                        string imageUrl = row["ImageUrl"].ToString();
                        string productUrl = $"ProductDetails.aspx?id={productId}";
                        
                        sb.Append(@"
                        // Create card for product " + productId + @"
                        const card" + i + @" = document.createElement('div');
                        card" + i + @".className = 'product-card w-full sm:w-1/2 md:w-1/3 lg:w-1/4 p-3';
                        card" + i + @".innerHTML = `
                            <div class='bg-white rounded-lg shadow-md overflow-hidden h-full flex flex-col border border-gray-100 hover:shadow-lg transition-shadow duration-300'>
                                <div class='block relative h-48 overflow-hidden'>
                                    <img src='" + imageUrl + @"' alt='" + name + @"' class='object-cover object-center w-full h-full block'>
                                </div>
                                <div class='p-5 flex-grow flex flex-col'>
                                    <h3 class='text-gray-900 text-lg font-medium mb-1'>" + name + @"</h3>
                                    <div class='text-[#96744F] font-bold mb-2'>₱" + price + @"</div>
                                    <p class='text-gray-600 text-sm mb-4 line-clamp-2'>Delicious fresh pastry made with premium ingredients.</p>
                                    <a href='" + productUrl + @"' class='mt-auto text-white bg-[#96744F] hover:bg-[#7a5f3e] px-4 py-2 rounded-md transition-colors duration-300 text-center font-medium w-full'>View Details</a>
                                </div>
                            </div>
                        `;
                        container.appendChild(card" + i + @");
                        ");
                    }
                    
                    // Close the function and call it
                    sb.Append(@"
                        console.log('Related products loaded successfully');
                    }
                    
                    // Execute immediately if document is already loaded
                    if (document.readyState === 'complete' || document.readyState === 'interactive') {
                        console.log('Document already loaded, running immediately');
                        loadRelatedProducts();
                    } else {
                        // Otherwise wait for DOM to be ready
                        console.log('Waiting for document to be ready');
                        document.addEventListener('DOMContentLoaded', loadRelatedProducts);
                    }

                    // Add heading and container styles
                    const relatedHeading = document.querySelector('.mb-16 h2');
                    if (relatedHeading) {
                        relatedHeading.className = 'text-2xl font-semibold text-black mb-6 pb-2 border-b border-gray-200';
                    }
                    
                    const relatedContainer = document.querySelector('.related-products');
                    if (relatedContainer) {
                        relatedContainer.className = 'related-products bg-gray-50 p-4 rounded-lg';
                    }
                    
                    const productCarousel = document.getElementById('relatedProductsContainer');
                    if (productCarousel) {
                        productCarousel.className = 'product-carousel flex flex-wrap';
                    }
                    ");
                    
                    // Register the script block
                    ClientScript.RegisterClientScriptBlock(this.GetType(), "LoadRelatedProducts", sb.ToString(), true);
                    System.Diagnostics.Debug.WriteLine("Related products script registered");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No related products found");
                    
                    // Hide loading message and display no products message
                    ClientScript.RegisterClientScriptBlock(this.GetType(), "NoRelatedProducts", @"
                    document.addEventListener('DOMContentLoaded', function() {
                        const container = document.getElementById('relatedProductsContainer');
                        if (container) {
                            container.innerHTML = '<p class=""w-full text-center py-4 text-gray-600"">No related products available.</p>';
                        }
                    });", true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading related products: " + ex.Message);
                
                // Show error message
                ClientScript.RegisterClientScriptBlock(this.GetType(), "RelatedProductsError", @"
                document.addEventListener('DOMContentLoaded', function() {
                    const container = document.getElementById('relatedProductsContainer');
                    if (container) {
                        container.innerHTML = '<p class=""w-full text-center py-4 text-red-600"">Error loading related products.</p>';
                        console.error('Error loading related products');
                    }
                });", true);
            }
        }

        private DataTable GetRelatedProducts(int productId, string category)
        {
            System.Diagnostics.Debug.WriteLine($"=== BEGIN GetRelatedProducts for ProductId: {productId}, Category: {category} ===");
            DataTable dt = new DataTable();
            dt.Columns.Add("ProductID", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Price", typeof(string));
            dt.Columns.Add("ImageUrl", typeof(string));
            
            try
            {
                using (var connection = new OracleConnection(GetConnectionString()))
                {
                    connection.Open();
                    System.Diagnostics.Debug.WriteLine("Database connection opened");
                    
                    // First approach: Get products in the same category
                    string relatedProductsQuery = @"
                        SELECT * FROM (
                            SELECT 
                                p.PRODUCTID, 
                                p.NAME, 
                                p.PRICE, 
                                p.STOCKQUANTITY, 
                                p.IMAGE
                            FROM 
                                PRODUCTS p
                            JOIN 
                                PRODUCTCATEGORIES pc1 ON p.PRODUCTID = pc1.PRODUCTID
                            WHERE 
                                pc1.CATEGORYID IN (
                                    SELECT CATEGORYID FROM PRODUCTCATEGORIES WHERE PRODUCTID = :ProductId
                                )
                                AND p.PRODUCTID != :ProductId
                                AND p.ISACTIVE = 1
                            ORDER BY 
                                DBMS_RANDOM.VALUE
                        ) WHERE ROWNUM <= 4";

                    System.Diagnostics.Debug.WriteLine($"Executing query: {relatedProductsQuery}");
                    
                    using (var cmd = new OracleCommand(relatedProductsQuery, connection))
                    {
                        cmd.Parameters.Add("ProductId", Oracle.ManagedDataAccess.Client.OracleDbType.Int32).Value = productId;
                        System.Diagnostics.Debug.WriteLine($"Parameter ProductId: {productId}");
                        
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int relatedProductId = Convert.ToInt32(reader["PRODUCTID"]);
                                string name = reader["NAME"].ToString();
                                decimal price = Convert.ToDecimal(reader["PRICE"]);
                                
                                System.Diagnostics.Debug.WriteLine($"Found related product: ID={relatedProductId}, Name={name}");
                                
                                // Generate image source
                                string imageUrl = "../Images/product-placeholder.svg";
                                if (reader["IMAGE"] != DBNull.Value)
                                {
                                    byte[] imageData = (byte[])reader["IMAGE"];
                                    if (imageData != null && imageData.Length > 0)
                                    {
                                        string base64String = Convert.ToBase64String(imageData);
                                        imageUrl = "data:image/jpeg;base64," + base64String;
                                    }
                                }

                                DataRow newRow = dt.NewRow();
                                newRow["ProductID"] = relatedProductId;
                                newRow["Name"] = name;
                                newRow["Price"] = price.ToString("0.00");
                                newRow["ImageUrl"] = imageUrl;
                                dt.Rows.Add(newRow);
                            }
                        }
                    }
                    
                    // If no related products found, try random products
                    if (dt.Rows.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("No related products found, getting random products");
                        
                        string randomProductsQuery = @"
                            SELECT * FROM (
                                SELECT 
                                    p.PRODUCTID, 
                                    p.NAME, 
                                    p.PRICE, 
                                    p.STOCKQUANTITY, 
                                    p.IMAGE
                                FROM 
                                    PRODUCTS p
                                WHERE 
                                    p.PRODUCTID != :ProductId
                                    AND p.ISACTIVE = 1
                                ORDER BY 
                                    DBMS_RANDOM.VALUE
                            ) WHERE ROWNUM <= 4";
                        
                        using (var cmd = new OracleCommand(randomProductsQuery, connection))
                        {
                            cmd.Parameters.Add("ProductId", Oracle.ManagedDataAccess.Client.OracleDbType.Int32).Value = productId;
                            
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int relatedProductId = Convert.ToInt32(reader["PRODUCTID"]);
                                    string name = reader["NAME"].ToString();
                                    decimal price = Convert.ToDecimal(reader["PRICE"]);
                                    
                                    System.Diagnostics.Debug.WriteLine($"Found random product: ID={relatedProductId}, Name={name}");
                                    
                                    // Generate image source
                                    string imageUrl = "../Images/product-placeholder.svg";
                                    if (reader["IMAGE"] != DBNull.Value)
                                    {
                                        byte[] imageData = (byte[])reader["IMAGE"];
                                        if (imageData != null && imageData.Length > 0)
                                        {
                                            string base64String = Convert.ToBase64String(imageData);
                                            imageUrl = "data:image/jpeg;base64," + base64String;
                                        }
                                    }

                                    DataRow newRow = dt.NewRow();
                                    newRow["ProductID"] = relatedProductId;
                                    newRow["Name"] = name;
                                    newRow["Price"] = price.ToString("0.00");
                                    newRow["ImageUrl"] = imageUrl;
                                    dt.Rows.Add(newRow);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetRelatedProducts: {ex.Message}");
            }
            
            System.Diagnostics.Debug.WriteLine($"Returning {dt.Rows.Count} related products");
            return dt;
        }

        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString;
        }
    }
}