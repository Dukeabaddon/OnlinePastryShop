using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Web.Services;
using System.Configuration;
using System.Web.UI;
using System.Web.Script.Services;
using System.Linq;

namespace OnlinePastryShop.Pages
{
    public partial class Products : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Initial page load
            }
        }

        // Get dashboard statistics
        [WebMethod]
        public static object GetDashboardStats()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("======================== BEGIN GetDashboardStats ========================");

                int totalProducts = 0;
                int outOfStock = 0;
                int lowStock = 0;

                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine("Database connection opened");

                    // Get total products
                    using (OracleCommand cmd = new OracleCommand(@"SELECT COUNT(*) FROM ""AARON_IPT"".""PRODUCTS"" WHERE IsActive = 1", conn))
                    {
                        totalProducts = Convert.ToInt32(cmd.ExecuteScalar());
                        System.Diagnostics.Debug.WriteLine($"Total active products: {totalProducts}");
                    }

                    // Get out of stock count
                    using (OracleCommand cmd = new OracleCommand(@"SELECT COUNT(*) FROM ""AARON_IPT"".""PRODUCTS"" WHERE StockQuantity = 0 AND IsActive = 1", conn))
                    {
                        outOfStock = Convert.ToInt32(cmd.ExecuteScalar());
                        System.Diagnostics.Debug.WriteLine($"Out of stock products: {outOfStock}");
                    }

                    // Get low stock count
                    using (OracleCommand cmd = new OracleCommand(@"SELECT COUNT(*) FROM ""AARON_IPT"".""PRODUCTS"" WHERE StockQuantity > 0 AND StockQuantity < 10 AND IsActive = 1", conn))
                    {
                        lowStock = Convert.ToInt32(cmd.ExecuteScalar());
                        System.Diagnostics.Debug.WriteLine($"Low stock products: {lowStock}");
                    }
                }

                var result = new
                {
                    TotalProducts = totalProducts,
                    OutOfStockCount = outOfStock,
                    LowStockCount = lowStock
                };

                System.Diagnostics.Debug.WriteLine($"Dashboard stats: TotalProducts={totalProducts}, OutOfStock={outOfStock}, LowStock={lowStock}");
                System.Diagnostics.Debug.WriteLine("======================== END GetDashboardStats ========================");

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPTION in GetDashboardStats: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new { TotalProducts = 0, OutOfStockCount = 0, LowStockCount = 0 };
            }
        }

        // Get products with filtering and pagination
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetProducts(string search, string categoryId, string sort, int page, int pageSize, bool includeDeleted = false)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetProducts called with search='{search}', categoryId='{categoryId}', sort='{sort}', page={page}, pageSize={pageSize}, includeDeleted={includeDeleted}");
                
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine("Database connection opened");
                    
                    // Build WHERE clause based on parameters
                    string whereClause = !includeDeleted ? " WHERE p.ISACTIVE = 1" : " WHERE 1=1";
                    
                    if (!string.IsNullOrEmpty(search))
                    {
                        whereClause += " AND UPPER(p.NAME) LIKE UPPER(:search)";
                        System.Diagnostics.Debug.WriteLine($"Adding search condition: {whereClause}");
                    }
                    
                    if (!string.IsNullOrEmpty(categoryId) && categoryId != "0")
                    {
                        whereClause += " AND pc.CATEGORYID = :categoryId";
                        System.Diagnostics.Debug.WriteLine($"Adding category condition: {whereClause}");
                    }
                    
                    // First get total count
                    string countSql = $@"
                        SELECT COUNT(DISTINCT p.PRODUCTID) 
                        FROM ""AARON_IPT"".""PRODUCTS"" p
                        LEFT JOIN ""AARON_IPT"".""PRODUCTCATEGORIES"" pc ON p.PRODUCTID = pc.PRODUCTID 
                        LEFT JOIN ""AARON_IPT"".""CATEGORIES"" c ON pc.CATEGORYID = c.CATEGORYID
                        {whereClause}";
                    
                    System.Diagnostics.Debug.WriteLine($"Count SQL: {countSql}");
                    int totalCount = 0;
                    
                    using (OracleCommand countCmd = new OracleCommand(countSql, conn))
                    {
                        if (!string.IsNullOrEmpty(search))
                        {
                            string searchParam = "%" + search + "%";
                            System.Diagnostics.Debug.WriteLine($"Adding search parameter: '{searchParam}'");
                            countCmd.Parameters.Add("search", OracleDbType.Varchar2).Value = searchParam;
                        }
                        
                        if (!string.IsNullOrEmpty(categoryId) && categoryId != "0")
                        {
                            System.Diagnostics.Debug.WriteLine($"Adding categoryId parameter: {categoryId}");
                            countCmd.Parameters.Add("categoryId", OracleDbType.Int32).Value = Convert.ToInt32(categoryId);
                        }
                        
                        object result = countCmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            totalCount = Convert.ToInt32(result);
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"Total count: {totalCount}");
                    }
                    
                    // Calculate pagination
                    int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                    int offset = (page - 1) * pageSize;
                    
                    // Get the ordered products
                    string orderBy = GetOrderByClause(sort);
                    System.Diagnostics.Debug.WriteLine($"Order by: {orderBy}");
                    
                    string sql = $@"
                        SELECT * FROM (
                            SELECT p.PRODUCTID, p.NAME, p.DESCRIPTION, p.PRICE, p.COSTPRICE, p.STOCKQUANTITY, 
                                   c.CATEGORYID, c.NAME AS CATEGORYNAME, p.ISLATEST,
                                   ROW_NUMBER() OVER ({orderBy}) AS RN
                            FROM ""AARON_IPT"".""PRODUCTS"" p
                            LEFT JOIN ""AARON_IPT"".""PRODUCTCATEGORIES"" pc ON p.PRODUCTID = pc.PRODUCTID 
                            LEFT JOIN ""AARON_IPT"".""CATEGORIES"" c ON pc.CATEGORYID = c.CATEGORYID
                            {whereClause}
                        ) WHERE RN BETWEEN :startRow AND :endRow";
                    
                    System.Diagnostics.Debug.WriteLine($"Products SQL: {sql}");
                    List<object> products = new List<object>();
                    
                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        if (!string.IsNullOrEmpty(search))
                        {
                            string searchParam = "%" + search + "%";
                            System.Diagnostics.Debug.WriteLine($"Adding search parameter to main query: '{searchParam}'");
                            cmd.Parameters.Add("search", OracleDbType.Varchar2).Value = searchParam;
                        }
                        
                        if (!string.IsNullOrEmpty(categoryId) && categoryId != "0")
                        {
                            System.Diagnostics.Debug.WriteLine($"Adding categoryId parameter to main query: {categoryId}");
                            cmd.Parameters.Add("categoryId", OracleDbType.Int32).Value = Convert.ToInt32(categoryId);
                        }
                        
                        cmd.Parameters.Add("startRow", OracleDbType.Int32).Value = offset + 1;
                        cmd.Parameters.Add("endRow", OracleDbType.Int32).Value = offset + pageSize;
                        
                        try
                        {
                            using (OracleDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    products.Add(new
                                    {
                                        ProductId = Convert.ToInt32(reader["PRODUCTID"]),
                                        Name = reader["NAME"].ToString(),
                                        Description = reader["DESCRIPTION"]?.ToString(),
                                        Price = Convert.ToDecimal(reader["PRICE"]),
                                        CostPrice = reader["COSTPRICE"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["COSTPRICE"]),
                                        StockQuantity = Convert.ToInt32(reader["STOCKQUANTITY"]),
                                        CategoryId = reader["CATEGORYID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CATEGORYID"]),
                                        CategoryName = reader["CATEGORYNAME"]?.ToString(),
                                        IsLatest = Convert.ToInt32(reader["ISLATEST"]) == 1,
                                        ImageUrl = GetProductImageUrl(Convert.ToInt32(reader["PRODUCTID"]))
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error executing reader: {ex.Message}\nSQL: {sql}");
                            throw;
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"Retrieved {products.Count} products");
                    }
                    
                    return new { TotalCount = totalCount, TotalPages = totalPages, Page = page, Products = products };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetProducts: {ex.Message}\n{ex.StackTrace}");
                throw new Exception($"Error getting products: {ex.Message}");
            }
        }

        private static string GetOrderByClause(string sort)
        {
            switch (sort?.ToLower())
            {
                case "name_asc":
                    return " ORDER BY p.NAME ASC";
                case "name_desc":
                    return " ORDER BY p.NAME DESC";
                case "price_asc":
                    return " ORDER BY p.PRICE ASC";
                case "price_desc":
                    return " ORDER BY p.PRICE DESC";
                case "stock_asc":
                    return " ORDER BY p.STOCKQUANTITY ASC";
                case "stock_desc":
                    return " ORDER BY p.STOCKQUANTITY DESC";
                default:
                    return " ORDER BY p.PRODUCTID DESC"; // Newest first by default
            }
        }

        private static string GetStockStatus(int stockQuantity, bool isActive)
        {
            if (!isActive)
                return "Inactive";
            else if (stockQuantity <= 0)
                return "Out of Stock";
            else if (stockQuantity < 10)
                return "Low Stock";
            else
                return "In Stock";
        }

        // Get categories for dropdowns with named parameters
        [WebMethod]
        public static List<object> GetCategories()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("GetCategories called");

                List<object> categories = new List<object>();
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine("Database connection opened for GetCategories");

                    using (OracleCommand cmd = new OracleCommand(
                        @"SELECT CATEGORYID, NAME FROM ""AARON_IPT"".""CATEGORIES"" WHERE ISACTIVE = 1 ORDER BY NAME", conn))
                    {
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                categories.Add(new
                                {
                                    CategoryId = reader["CATEGORYID"].ToString(),
                                    Name = reader["NAME"].ToString()
                                });
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Retrieved {categories.Count} categories");
                return categories;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting categories: {ex.Message}");
                throw new Exception($"Error getting categories: {ex.Message}");
            }
        }

        // Get product details for editing with named parameters
        [WebMethod]
        public static object GetProductDetails(int productId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetProductDetails called for product {productId}");

                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine("Database connection opened for GetProductDetails");

                    string sql = @"
                        SELECT p.*, c.CATEGORYID
                        FROM ""AARON_IPT"".""PRODUCTS"" p
                        LEFT JOIN ""AARON_IPT"".""PRODUCTCATEGORIES"" pc ON p.PRODUCTID = pc.PRODUCTID
                        LEFT JOIN ""AARON_IPT"".""CATEGORIES"" c ON pc.CATEGORYID = c.CATEGORYID
                        WHERE p.PRODUCTID = :ProductId AND p.ISACTIVE = 1";

                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("ProductId", OracleDbType.Int32).Value = productId;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var product = new
                                {
                                    ProductId = Convert.ToInt32(reader["PRODUCTID"]),
                                    Name = reader["NAME"].ToString(),
                                    Description = reader["DESCRIPTION"]?.ToString(),
                                    Price = Convert.ToDecimal(reader["PRICE"]),
                                    CostPrice = reader["COSTPRICE"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["COSTPRICE"]),
                                    StockQuantity = Convert.ToInt32(reader["STOCKQUANTITY"]),
                                    ImageBase64 = reader["IMAGE"] != DBNull.Value ?
                                        Convert.ToBase64String((byte[])reader["IMAGE"]) : null,
                                    IsLatest = Convert.ToBoolean(reader["ISLATEST"]),
                                    CategoryId = reader["CATEGORYID"] != DBNull.Value ?
                                        Convert.ToInt32(reader["CATEGORYID"]) : 0
                                };

                                System.Diagnostics.Debug.WriteLine($"Product details retrieved successfully for product {productId}");
                                return product;
                            }
                            throw new Exception("Product not found");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting product details: {ex.Message}");
                throw new Exception($"Error getting product details: {ex.Message}");
            }
        }

        // Simple product add method with direct string-based SQL construction
        [WebMethod]
        public static object AddProductSimple(string name, string description, string price,
            string costPrice, string stockQuantity, string imageBase64, string categoryId, bool isLatest)
        {
            System.Diagnostics.Debug.WriteLine("=========== BEGIN AddProductSimple ===========");
            System.Diagnostics.Debug.WriteLine($"Input parameters: name='{name}', price='{price}', costPrice='{costPrice}', stockQty='{stockQuantity}', categoryId='{categoryId}'");

            // Response data structure
            Dictionary<string, object> response = new Dictionary<string, object>();

            try
            {
                // Enhanced validation for product name - must contain at least one letter
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new { status = "error", message = "Product name is required." };
                }

                // Validate that the name contains at least one letter (not just numbers)
                bool containsLetter = false;
                bool containsNumber = false;
                foreach (char c in name)
                {
                    if (char.IsLetter(c))
                    {
                        containsLetter = true;
                    }
                    if (char.IsDigit(c))
                    {
                        containsNumber = true;
                    }
                }

                if (!containsLetter)
                {
                    return new { status = "error", message = "Product name must contain at least one letter." };
                }

                if (containsNumber)
                {
                    return new { status = "error", message = "Product name cannot contain any numbers." };
                }

                // Parse numeric values with defensive code
                decimal priceValue;
                if (!decimal.TryParse(price, out priceValue) || priceValue <= 0)
                {
                    return new { status = "error", message = "Please enter a valid price greater than zero." };
                }

                decimal costPriceValue;
                if (!decimal.TryParse(costPrice, out costPriceValue) || costPriceValue <= 0)
                {
                    return new { status = "error", message = "Please enter a valid cost price greater than zero." };
                }

                if (costPriceValue >= priceValue)
                {
                    return new { status = "error", message = "Cost price must be less than selling price." };
                }

                int stockValue;
                if (!int.TryParse(stockQuantity, out stockValue) || stockValue < 0)
                {
                    return new { status = "error", message = "Please enter a valid stock quantity." };
                }

                if (stockValue > 1000)
                {
                    return new { status = "error", message = "Stock quantity cannot exceed 1000 items." };
                }

                // Validate that a category is selected
                int catIdValue = 0;
                if (string.IsNullOrEmpty(categoryId))
                {
                    return new { status = "error", message = "Please select a category for the product." };
                }

                if (!int.TryParse(categoryId, out catIdValue) || catIdValue <= 0)
                {
                    return new { status = "error", message = "Please select a valid category for the product." };
                }

                if (!string.IsNullOrEmpty(description))
                {
                    // Count words in description
                    string[] words = description.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length > 50)
                    {
                        return new { status = "error", message = "Description cannot exceed 50 words." };
                    }
                }

                // Prepare image data
                byte[] imageBytes = null;
                if (!string.IsNullOrEmpty(imageBase64))
                {
                    try
                    {
                        // Clean the base64 string if it contains data URL prefix
                        if (imageBase64.Contains(","))
                        {
                            imageBase64 = imageBase64.Substring(imageBase64.IndexOf(",") + 1);
                        }

                        imageBytes = Convert.FromBase64String(imageBase64);
                        if (imageBytes.Length == 0)
                        {
                            imageBytes = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Image processing error (continuing without image): {ex.Message}");
                        imageBytes = null;
                    }
                }

                // Get a new database connection
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    // Open the connection with explicit error handling
                    try
                    {
                        conn.Open();
                        System.Diagnostics.Debug.WriteLine("Database connection opened successfully");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"ERROR opening database connection: {ex.Message}");
                        return new { status = "error", message = $"Database connection error: {ex.Message}" };
                    }

                    using (OracleTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Check for duplicate name - with more detailed logging
                            System.Diagnostics.Debug.WriteLine("Checking for duplicate product name...");
                            bool hasDuplicate = false;
                            string existingProductName = "";

                            using (OracleCommand checkCmd = new OracleCommand(@"SELECT COUNT(*) as Count, MAX(Name) as ExistingName FROM ""AARON_IPT"".""PRODUCTS"" WHERE LOWER(Name) = LOWER(:name) AND IsActive = 1", conn))
                            {
                                checkCmd.Transaction = transaction;
                                // Use the correct parameter pattern that works in UpdateProduct
                                checkCmd.Parameters.Add("name", OracleDbType.Varchar2).Value = name;

                                try
                                {
                                    using (OracleDataReader reader = checkCmd.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            int duplicateCount = Convert.ToInt32(reader["Count"]);
                                            hasDuplicate = duplicateCount > 0;

                                            if (hasDuplicate && !reader.IsDBNull(reader.GetOrdinal("ExistingName")))
                                            {
                                                existingProductName = reader["ExistingName"].ToString();
                                            }

                                            System.Diagnostics.Debug.WriteLine($"Duplicate check result: Count={duplicateCount}, ExistingName='{existingProductName}'");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error checking for duplicates: {ex.Message}");
                                    throw new Exception($"Error checking for duplicates: {ex.Message}");
                                }
                            }

                            if (hasDuplicate)
                            {
                                System.Diagnostics.Debug.WriteLine($"Duplicate product name found: '{existingProductName}', returning error");
                                return new { status = "error", message = $"A product with this name already exists: '{existingProductName}'" };
                            }

                            // Additional check to see all existing products with similar names for debugging
                            System.Diagnostics.Debug.WriteLine("Doing additional check for similar product names...");
                            using (OracleCommand similarCmd = new OracleCommand(@"SELECT ProductId, Name FROM ""AARON_IPT"".""PRODUCTS"" WHERE Name LIKE :similar AND IsActive = 1", conn))
                            {
                                similarCmd.Transaction = transaction;
                                similarCmd.Parameters.Add("similar", OracleDbType.Varchar2).Value = $"%{name.Substring(0, Math.Min(3, name.Length))}%";

                                try
                                {
                                    using (OracleDataReader reader = similarCmd.ExecuteReader())
                                    {
                                        int count = 0;
                                        while (reader.Read())
                                        {
                                            count++;
                                            System.Diagnostics.Debug.WriteLine($"Found similar product: ID={reader["ProductId"]}, Name='{reader["Name"]}'");
                                        }
                                        System.Diagnostics.Debug.WriteLine($"Found {count} products with similar names");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error checking for similar names (non-critical): {ex.Message}");
                                    // Don't rethrow - this is just for debugging
                                }
                            }

                            // Insert product using the approach that works in UpdateProduct
                            int newProductId = 0;
                            System.Diagnostics.Debug.WriteLine("Inserting new product...");

                            // Use named parameters with OracleDbType
                            string insertSql = @"
                                INSERT INTO ""AARON_IPT"".""PRODUCTS"" 
                                (NAME, DESCRIPTION, PRICE, COSTPRICE, STOCKQUANTITY, IMAGE, ISLATEST, ISACTIVE, CREATEDATE) 
                                VALUES 
                                (:Name, :Description, :Price, :CostPrice, :StockQuantity, :Image, :IsLatest, 1, SYSDATE) 
                                RETURNING PRODUCTID INTO :ProductId";

                            using (OracleCommand cmd = new OracleCommand(insertSql, conn))
                            {
                                cmd.Transaction = transaction;

                                // Use the same parameter approach that works in UpdateProduct
                                cmd.Parameters.Add("Name", OracleDbType.Varchar2).Value = name.Trim(); // Ensure name is trimmed
                                cmd.Parameters.Add("Description", OracleDbType.Varchar2).Value =
                                    string.IsNullOrEmpty(description) ? DBNull.Value : (object)description;

                                // Convert price to decimal and use OracleDbType.Decimal
                                cmd.Parameters.Add("Price", OracleDbType.Decimal).Value = priceValue;
                                cmd.Parameters.Add("CostPrice", OracleDbType.Decimal).Value = costPriceValue;

                                cmd.Parameters.Add("StockQuantity", OracleDbType.Int32).Value = stockValue;
                                cmd.Parameters.Add("Image", OracleDbType.Blob).Value = imageBytes != null ? (object)imageBytes : DBNull.Value;
                                cmd.Parameters.Add("IsLatest", OracleDbType.Int32).Value = isLatest ? 1 : 0;

                                // Output parameter
                                OracleParameter paramProductId = new OracleParameter("ProductId", OracleDbType.Int32);
                                paramProductId.Direction = ParameterDirection.Output;
                                cmd.Parameters.Add(paramProductId);

                                try
                                {
                                    // Log the SQL and parameters for debugging
                                    System.Diagnostics.Debug.WriteLine($"Executing SQL: {insertSql}");
                                    foreach (OracleParameter param in cmd.Parameters)
                                    {
                                        string paramValue = param.Value == DBNull.Value ? "NULL" :
                                                          param.Value == null ? "NULL" :
                                                          param.ParameterName == "Image" ? "[BINARY DATA]" :
                                                          param.Value.ToString();
                                        System.Diagnostics.Debug.WriteLine($"  Parameter {param.ParameterName} = {paramValue}");
                                    }

                                    cmd.ExecuteNonQuery();

                                    // Get the product ID from output parameter
                                    if (paramProductId.Value != DBNull.Value)
                                    {
                                        newProductId = Convert.ToInt32(paramProductId.Value.ToString());
                                        System.Diagnostics.Debug.WriteLine($"New product created with ID: {newProductId}");
                                    }
                                    else
                                    {
                                        throw new Exception("Failed to get new product ID");
                                    }
                                }
                                catch (OracleException oex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"ORACLE ERROR inserting product: {oex.Message}");
                                    System.Diagnostics.Debug.WriteLine($"Oracle Error Code: {oex.Number}");
                                    System.Diagnostics.Debug.WriteLine($"ERROR stack: {oex.StackTrace}");

                                    // Handle the specific constraint violation error
                                    if (oex.Number == 1) // ORA-00001: unique constraint violated
                                    {
                                        string errorMessage = "A constraint was violated when adding this product.";

                                        // Try to determine which constraint was violated
                                        if (oex.Message.Contains("SYS_C007203"))
                                        {
                                            // Try to get more information about the constraint
                                            try
                                            {
                                                using (OracleCommand constraintCmd = new OracleCommand(
                                                    "SELECT table_name, column_name FROM USER_CONS_COLUMNS WHERE constraint_name = 'SYS_C007203'", conn))
                                                {
                                                    constraintCmd.Transaction = transaction;
                                                    using (OracleDataReader reader = constraintCmd.ExecuteReader())
                                                    {
                                                        if (reader.Read())
                                                        {
                                                            string tableName = reader["TABLE_NAME"].ToString();
                                                            string columnName = reader["COLUMN_NAME"].ToString();
                                                            System.Diagnostics.Debug.WriteLine($"Constraint details - Table: {tableName}, Column: {columnName}");

                                                            if (columnName.ToUpper() == "NAME")
                                                            {
                                                                errorMessage = "A product with this name already exists. Please choose a different name.";
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                System.Diagnostics.Debug.WriteLine($"Error getting constraint details: {ex.Message}");
                                            }
                                        }

                                        return new { status = "error", message = errorMessage };
                                    }

                                    throw;
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"ERROR inserting product: {ex.Message}");
                                    System.Diagnostics.Debug.WriteLine($"ERROR type: {ex.GetType().FullName}");
                                    System.Diagnostics.Debug.WriteLine($"ERROR stack: {ex.StackTrace}");
                                    throw;
                                }
                            }

                            // Associate with category if needed
                            if (catIdValue > 0 && newProductId > 0)
                            {
                                System.Diagnostics.Debug.WriteLine($"Adding category association: Product={newProductId}, Category={catIdValue}");

                                using (OracleCommand catCmd = new OracleCommand(@"INSERT INTO ProductCategories (ProductId, CategoryId) VALUES (:pid, :cid)", conn))
                                {
                                    catCmd.Transaction = transaction;
                                    // Use the working parameter approach
                                    catCmd.Parameters.Add("pid", OracleDbType.Int32).Value = newProductId;
                                    catCmd.Parameters.Add("cid", OracleDbType.Int32).Value = catIdValue;

                                    try
                                    {
                                        catCmd.ExecuteNonQuery();
                                        System.Diagnostics.Debug.WriteLine("Category association added successfully");
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"ERROR adding category: {ex.Message}");
                                        throw;
                                    }
                                }
                            }

                            // Commit the transaction
                            transaction.Commit();
                            System.Diagnostics.Debug.WriteLine("Transaction committed successfully");

                            // The original dictionary response is not needed - we've standardized on the object format
                            System.Diagnostics.Debug.WriteLine("Product added successfully");
                            System.Diagnostics.Debug.WriteLine("=========== END AddProductSimple ===========");

                            response["status"] = "success";
                            response["message"] = "Product added successfully";
                            response["productId"] = newProductId;
                            return response;
                        }
                        catch (Exception ex)
                        {
                            // Roll back the transaction
                            System.Diagnostics.Debug.WriteLine($"ERROR in transaction, rolling back: {ex.Message}");
                            transaction.Rollback();

                            response["status"] = "error";
                            response["message"] = $"Error adding product: {ex.Message}";
                            response["details"] = ex.ToString();
                            return response;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UNHANDLED EXCEPTION: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                response["status"] = "error";
                response["message"] = $"Unexpected error: {ex.Message}";
                response["details"] = ex.ToString();
            return response;
            }
        }

        // Update existing product with the same improved parameter handling approach
        [WebMethod]
        public static object UpdateProduct(int productId, string name, string description,
            decimal price, decimal costPrice, int stockQuantity, string imageBase64, int categoryId, bool isLatest)
        {
            try
            {
                // Validate inputs
                if (stockQuantity > 1000)
                {
                    return new { Success = false, Message = "Stock quantity cannot exceed 1000 items." };
                }

                if (costPrice <= 0)
                {
                    return new { Success = false, Message = "Cost price must be greater than zero." };
                }

                if (price <= 0)
                {
                    return new { Success = false, Message = "Price must be greater than zero." };
                }

                if (costPrice >= price)
                {
                    return new { Success = false, Message = "Cost price must be less than selling price." };
                }

                if (!string.IsNullOrEmpty(description))
                {
                    // Count words in description
                    string[] words = description.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length > 50)
                    {
                        return new { Success = false, Message = "Description cannot exceed 50 words." };
                    }
                }

                // Validate name contains at least one letter and no numbers
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new { Success = false, Message = "Product name is required." };
                }

                // Check for at least one letter character
                bool containsLetter = false;
                bool containsNumber = false;
                foreach (char c in name)
                {
                    if (char.IsLetter(c))
                    {
                        containsLetter = true;
                    }
                    if (char.IsDigit(c))
                    {
                        containsNumber = true;
                    }
                }

                if (!containsLetter)
                {
                    return new { Success = false, Message = "Product name must contain at least one letter." };
                }

                if (containsNumber)
                {
                    return new { Success = false, Message = "Product name cannot contain any numbers." };
                }

                System.Diagnostics.Debug.WriteLine($"UpdateProduct called for product {productId}");
                System.Diagnostics.Debug.WriteLine($"Parameters: name={name}, price={price}, categoryId={categoryId}");

                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine($"Database connection opened for UpdateProduct");

                    using (OracleTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Check for duplicate name excluding current product
                            string checkDuplicateSql = @"
                                SELECT COUNT(*) FROM ""AARON_IPT"".""PRODUCTS"" 
                                WHERE LOWER(Name) = LOWER(:Name) 
                                AND ProductId != :ProductId 
                                AND IsActive = 1";

                            using (OracleCommand checkCmd = new OracleCommand(checkDuplicateSql, conn))
                            {
                                checkCmd.Transaction = transaction;
                                checkCmd.Parameters.Add("Name", OracleDbType.Varchar2).Value = name;
                                checkCmd.Parameters.Add("ProductId", OracleDbType.Int32).Value = productId;

                                if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                                {
                                    return new { Success = false, Message = "A product with this name already exists." };
                                }
                            }

                            // Use named parameters for basic info update
                            string updateBasicSql = @"
                                UPDATE ""AARON_IPT"".""PRODUCTS"" SET 
                                    Name = :Name, 
                                    Description = :Description, 
                                    Price = :Price, 
                                    CostPrice = :CostPrice,
                                    StockQuantity = :StockQuantity, 
                                    IsLatest = :IsLatest
                                WHERE ProductId = :ProductId";

                            using (OracleCommand cmd = new OracleCommand(updateBasicSql, conn))
                            {
                                cmd.Transaction = transaction;

                                // Use named parameters with chained Value assignments
                                cmd.Parameters.Add("Name", OracleDbType.Varchar2).Value = name;
                                cmd.Parameters.Add("Description", OracleDbType.Varchar2).Value =
                                    string.IsNullOrEmpty(description) ? DBNull.Value : (object)description;

                                // Handle price with OracleDbType.Decimal directly - same as in AddProduct
                                cmd.Parameters.Add("Price", OracleDbType.Decimal).Value = price;
                                cmd.Parameters.Add("CostPrice", OracleDbType.Decimal).Value = costPrice;
                                System.Diagnostics.Debug.WriteLine($"Price parameter type: {cmd.Parameters["Price"].OracleDbType}, Value: {cmd.Parameters["Price"].Value}");
                                System.Diagnostics.Debug.WriteLine($"CostPrice parameter type: {cmd.Parameters["CostPrice"].OracleDbType}, Value: {cmd.Parameters["CostPrice"].Value}");

                                cmd.Parameters.Add("StockQuantity", OracleDbType.Int32).Value = stockQuantity;
                                cmd.Parameters.Add("IsLatest", OracleDbType.Int32).Value = isLatest ? 1 : 0;
                                cmd.Parameters.Add("ProductId", OracleDbType.Int32).Value = productId;

                                // Log all parameters before executing
                                foreach (OracleParameter param in cmd.Parameters)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Parameter: {param.ParameterName}, Type: {param.OracleDbType}, Value: {param.Value}");
                                }

                                int rowsAffected = cmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Basic info update affected {rowsAffected} rows");

                                if (rowsAffected == 0)
                                {
                                    throw new Exception("Product not found");
                                }
                            }

                            // Handle image update if needed
                            if (!string.IsNullOrEmpty(imageBase64) && imageBase64 != "keep-existing")
                            {
                                try
                                {
                                    // Clean the base64 string if it contains data URL prefix
                                    if (imageBase64.Contains(","))
                                    {
                                        imageBase64 = imageBase64.Substring(imageBase64.IndexOf(",") + 1);
                                    }

                                    byte[] imageBytes = Convert.FromBase64String(imageBase64);

                                    // Only update if we have valid image data
                                    if (imageBytes.Length > 0)
                                    {
                                        string updateImageSql = @"UPDATE ""AARON_IPT"".""PRODUCTS"" SET Image = :Image WHERE ProductId = :ProductId";
                                        using (OracleCommand imageCmd = new OracleCommand(updateImageSql, conn))
                                        {
                                            imageCmd.Transaction = transaction;

                                            // Use named parameters with chained Value assignments
                                            imageCmd.Parameters.Add("Image", OracleDbType.Blob).Value = imageBytes;
                                            imageCmd.Parameters.Add("ProductId", OracleDbType.Int32).Value = productId;

                                            int imageRowsAffected = imageCmd.ExecuteNonQuery();
                                            System.Diagnostics.Debug.WriteLine($"Image update affected {imageRowsAffected} rows, image size: {imageBytes.Length} bytes");
                                        }
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine("Image bytes length is 0, skipping image update");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error updating image: {ex.Message}");
                                    // Continue without image update rather than failing the entire transaction
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Skipping image update: " +
                                    (imageBase64 == "keep-existing" ? "Using existing image" : "No image data provided"));
                            }

                            // Update category (first remove existing, then add new)
                            string deleteCategories = "DELETE FROM ProductCategories WHERE ProductId = :ProductId";
                            using (OracleCommand deleteCmd = new OracleCommand(deleteCategories, conn))
                            {
                                deleteCmd.Transaction = transaction;
                                deleteCmd.Parameters.Add("ProductId", OracleDbType.Int32).Value = productId;
                                int deletedRows = deleteCmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Deleted {deletedRows} category associations");
                            }

                            // Add new category if one was specified
                            if (categoryId > 0)
                            {
                                string insertCategory = "INSERT INTO ProductCategories (ProductId, CategoryId) VALUES (:ProductId, :CategoryId)";
                                using (OracleCommand insertCmd = new OracleCommand(insertCategory, conn))
                                {
                                    insertCmd.Transaction = transaction;
                                    insertCmd.Parameters.Add("ProductId", OracleDbType.Int32).Value = productId;
                                    insertCmd.Parameters.Add("CategoryId", OracleDbType.Int32).Value = categoryId;
                                    insertCmd.ExecuteNonQuery();
                                    System.Diagnostics.Debug.WriteLine($"Added category association with CategoryId: {categoryId}");
                                }
                            }

                            transaction.Commit();
                            System.Diagnostics.Debug.WriteLine("Product update transaction committed successfully");
                            return new { Success = true, Message = "Product updated successfully." };
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            System.Diagnostics.Debug.WriteLine($"Error in transaction: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating product: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new { Success = false, Message = $"Error updating product: {ex.Message}" };
            }
        }

        // Delete product with named parameters - perform actual deletion
        [WebMethod]
        public static object DeleteProduct(int productId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"DeleteProduct called for product {productId}");
                
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    
                    // Soft delete (update IsActive flag)
                    string sql = @"UPDATE ""AARON_IPT"".""PRODUCTS"" SET ISACTIVE = 0, UPDATEDATE = SYSDATE WHERE PRODUCTID = :ProductId";
                    
                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("ProductId", OracleDbType.Int32).Value = productId;
                        int rowsAffected = cmd.ExecuteNonQuery();
                        
                        if (rowsAffected > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"Product {productId} deleted successfully");
                            return new { success = true, message = "Product successfully deleted" };
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"No product found with ID {productId}");
                            return new { success = false, message = "Product not found" };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting product: {ex.Message}");
                return new { success = false, message = $"Error: {ex.Message}" };
            }
        }

        // Get product image with named parameters
        [WebMethod]
        public static object GetProductImage(int productId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetProductImage called for product {productId}");

                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand(@"SELECT IMAGE FROM ""AARON_IPT"".""PRODUCTS"" WHERE PRODUCTID = :ProductId", conn))
                    {
                        cmd.Parameters.Add("ProductId", OracleDbType.Int32).Value = productId;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read() && reader["IMAGE"] != DBNull.Value)
                            {
                                byte[] imageBytes = (byte[])reader["IMAGE"];
                                System.Diagnostics.Debug.WriteLine($"Image found for product {productId}, size: {imageBytes.Length} bytes");
                                return new { ImageBase64 = Convert.ToBase64String(imageBytes) };
                            }
                            System.Diagnostics.Debug.WriteLine($"No image found for product {productId}");
                        }
                    }
                }
                return new { ImageBase64 = "" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting product image: {ex.Message}");
                return new { Error = ex.Message };
            }
        }

        // Helper method to get connection string with better error handling
        private static string GetConnectionString()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["OracleConnection"]?.ConnectionString;

                if (string.IsNullOrEmpty(connectionString))
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: Oracle connection string is missing or empty in the configuration file. Using fallback connection string.");
                    return "User Id=mecate;Password=qwen123;Data Source=localhost:1521/xe;";
                }

                // Log connection string (excluding password for security)
                string sanitizedConnString = connectionString;
                if (sanitizedConnString.Contains("Password=") || sanitizedConnString.Contains("Password ="))
                {
                    int startIndex = sanitizedConnString.IndexOf("Password=", StringComparison.OrdinalIgnoreCase);
                    if (startIndex >= 0)
                    {
                        int endIndex = sanitizedConnString.IndexOf(";", startIndex);
                        if (endIndex >= 0)
                        {
                            // Replace password=XXX; with password=***;
                            sanitizedConnString = sanitizedConnString.Substring(0, startIndex) +
                                                 "Password=***" +
                                                 sanitizedConnString.Substring(endIndex);
                        }
                        else
                        {
                            // Password is at the end of string
                            sanitizedConnString = sanitizedConnString.Substring(0, startIndex) + "Password=***";
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Using connection string: {sanitizedConnString}");
                return connectionString;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR retrieving connection string: {ex.Message}");
                // Return fallback connection string instead of throwing exception
                return "User Id=mecate;Password=qwen123;Data Source=localhost:1521/xe;";
            }
        }
    
    private static string GetProductImageUrl(int productId)
        {
            return $"GetProductImage.ashx?id={productId}&t={DateTime.Now.Ticks}";
        }

        // Add or update product with named parameters
        [WebMethod]
        public static object AddOrUpdateProduct(int productId, string name, string description, decimal price, decimal costPrice, int stockQuantity, string imageBase64, bool isLatest, int categoryId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AddOrUpdateProduct called: ProductId={productId}, Name={name}, Price={price}, Category={categoryId}");
                
                int newProductId = productId;
                string action = (productId == 0) ? "added" : "updated";
                
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    OracleTransaction transaction = conn.BeginTransaction();
                    
                    try
                    {
                        byte[] imageBytes = null;
                        if (!string.IsNullOrEmpty(imageBase64))
                        {
                            // Remove the data URL prefix if present
                            if (imageBase64.StartsWith("data:image"))
                            {
                                int commaIndex = imageBase64.IndexOf(',');
                                if (commaIndex > 0)
                                {
                                    imageBase64 = imageBase64.Substring(commaIndex + 1);
                                }
                            }
                            imageBytes = Convert.FromBase64String(imageBase64);
                        }
                        
                        if (productId == 0) // Insert new product
                        {
                            string sql = @"
                                INSERT INTO ""AARON_IPT"".""PRODUCTS"" 
                                (NAME, DESCRIPTION, PRICE, COSTPRICE, STOCKQUANTITY, IMAGE, ISLATEST, ISACTIVE, CREATEDATE) 
                                VALUES 
                                (:Name, :Description, :Price, :CostPrice, :StockQuantity, :Image, :IsLatest, 1, SYSDATE) 
                                RETURNING PRODUCTID INTO :ProductId";

                            using (OracleCommand cmd = new OracleCommand(sql, conn))
                            {
                                cmd.Transaction = transaction;
                                cmd.Parameters.Add("Name", OracleDbType.Varchar2).Value = name;
                                cmd.Parameters.Add("Description", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(description) ? DBNull.Value : (object)description;
                                cmd.Parameters.Add("Price", OracleDbType.Decimal).Value = price;
                                cmd.Parameters.Add("CostPrice", OracleDbType.Decimal).Value = costPrice == 0 ? DBNull.Value : (object)costPrice;
                                cmd.Parameters.Add("StockQuantity", OracleDbType.Int32).Value = stockQuantity;
                                cmd.Parameters.Add("Image", OracleDbType.Blob).Value = imageBytes == null ? DBNull.Value : (object)imageBytes;
                                cmd.Parameters.Add("IsLatest", OracleDbType.Int32).Value = isLatest ? 1 : 0;
                                
                                OracleParameter idParam = new OracleParameter("ProductId", OracleDbType.Int32);
                                idParam.Direction = ParameterDirection.Output;
                                cmd.Parameters.Add(idParam);
                                
                                cmd.ExecuteNonQuery();
                                newProductId = Convert.ToInt32(idParam.Value.ToString());
                            }
                            
                            System.Diagnostics.Debug.WriteLine($"New product inserted with ID: {newProductId}");
                        }
                        else // Update existing product
                        {
                            string sql;
                            if (imageBytes != null)
                            {
                                sql = @"
                                    UPDATE ""AARON_IPT"".""PRODUCTS"" 
                                    SET NAME = :Name, 
                                        DESCRIPTION = :Description, 
                                        PRICE = :Price, 
                                        COSTPRICE = :CostPrice, 
                                        STOCKQUANTITY = :StockQuantity, 
                                        IMAGE = :Image, 
                                        ISLATEST = :IsLatest, 
                                        UPDATEDATE = SYSDATE 
                                    WHERE PRODUCTID = :ProductId";
                            }
                            else
                            {
                                sql = @"
                                    UPDATE ""AARON_IPT"".""PRODUCTS"" 
                                    SET NAME = :Name, 
                                        DESCRIPTION = :Description, 
                                        PRICE = :Price, 
                                        COSTPRICE = :CostPrice, 
                                        STOCKQUANTITY = :StockQuantity, 
                                        ISLATEST = :IsLatest, 
                                        UPDATEDATE = SYSDATE 
                                    WHERE PRODUCTID = :ProductId";
                            }

                            using (OracleCommand cmd = new OracleCommand(sql, conn))
                            {
                                cmd.Transaction = transaction;
                                cmd.Parameters.Add("Name", OracleDbType.Varchar2).Value = name;
                                cmd.Parameters.Add("Description", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(description) ? DBNull.Value : (object)description;
                                cmd.Parameters.Add("Price", OracleDbType.Decimal).Value = price;
                                cmd.Parameters.Add("CostPrice", OracleDbType.Decimal).Value = costPrice == 0 ? DBNull.Value : (object)costPrice;
                                cmd.Parameters.Add("StockQuantity", OracleDbType.Int32).Value = stockQuantity;
                                cmd.Parameters.Add("IsLatest", OracleDbType.Int32).Value = isLatest ? 1 : 0;
                                cmd.Parameters.Add("ProductId", OracleDbType.Int32).Value = productId;
                                
                                if (imageBytes != null)
                                {
                                    cmd.Parameters.Add("Image", OracleDbType.Blob).Value = imageBytes;
                                }
                                
                                cmd.ExecuteNonQuery();
                            }
                            
                            System.Diagnostics.Debug.WriteLine($"Product updated with ID: {productId}");
                        }
                        
                        // Handle category assignment
                        if (categoryId > 0)
                        {
                            // First remove existing category assignments
                            string deleteSql = @"DELETE FROM ""AARON_IPT"".""PRODUCTCATEGORIES"" WHERE PRODUCTID = :ProductId";
                            using (OracleCommand deleteCmd = new OracleCommand(deleteSql, conn))
                            {
                                deleteCmd.Transaction = transaction;
                                deleteCmd.Parameters.Add("ProductId", OracleDbType.Int32).Value = newProductId;
                                deleteCmd.ExecuteNonQuery();
                            }
                            
                            // Then add the new category
                            string insertSql = @"INSERT INTO ""AARON_IPT"".""PRODUCTCATEGORIES"" (PRODUCTID, CATEGORYID) VALUES (:ProductId, :CategoryId)";
                            using (OracleCommand insertCmd = new OracleCommand(insertSql, conn))
                            {
                                insertCmd.Transaction = transaction;
                                insertCmd.Parameters.Add("ProductId", OracleDbType.Int32).Value = newProductId;
                                insertCmd.Parameters.Add("CategoryId", OracleDbType.Int32).Value = categoryId;
                                insertCmd.ExecuteNonQuery();
                            }
                            
                            System.Diagnostics.Debug.WriteLine($"Category {categoryId} assigned to product {newProductId}");
                        }
                        
                        transaction.Commit();
                        return new { success = true, message = $"Product successfully {action}", productId = newProductId };
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in transaction: {ex.Message}");
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding/updating product: {ex.Message}");
                return new { success = false, message = $"Error: {ex.Message}" };
            }
        }

        [WebMethod]
        public static object TestDatabaseConnection()
        {
            try
            {
                string connectionString = GetConnectionString();
                System.Diagnostics.Debug.WriteLine($"Testing connection with: {connectionString}");
                
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine("Connection successful");
                    
                    // Check PRODUCTS table
                    using (OracleCommand cmd = new OracleCommand("SELECT COUNT(*) FROM \"AARON_IPT\".\"PRODUCTS\"", conn))
                    {
                        object result = cmd.ExecuteScalar();
                        int productCount = Convert.ToInt32(result);
                        System.Diagnostics.Debug.WriteLine($"Product count: {productCount}");
                        
                        // Test sample query for search
                        string testQuery = @"
                            SELECT COUNT(*) 
                            FROM ""AARON_IPT"".""PRODUCTS"" p
                            LEFT JOIN ""AARON_IPT"".""PRODUCTCATEGORIES"" pc ON p.PRODUCTID = pc.PRODUCTID 
                            LEFT JOIN ""AARON_IPT"".""CATEGORIES"" c ON pc.CATEGORYID = c.CATEGORYID
                            WHERE p.ISACTIVE = 1 AND UPPER(p.NAME) LIKE UPPER('%test%')";
                        
                        using (OracleCommand testCmd = new OracleCommand(testQuery, conn))
                        {
                            int searchCount = Convert.ToInt32(testCmd.ExecuteScalar());
                            System.Diagnostics.Debug.WriteLine($"Test search count: {searchCount}");
                        }

                        // Get list of tables in schema
                        using (OracleCommand tablesCmd = new OracleCommand("SELECT table_name FROM all_tables WHERE owner = 'AARON_IPT'", conn))
                        {
                            System.Diagnostics.Debug.WriteLine("Tables in AARON_IPT schema:");
                            using (OracleDataReader reader = tablesCmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    System.Diagnostics.Debug.WriteLine($"- {reader["TABLE_NAME"]}");
                                }
                            }
                        }
                        
                        return new { 
                            Success = true, 
                            ProductCount = productCount, 
                            Message = $"Connection successful. Found {productCount} products." 
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Connection test error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new { 
                    Success = false, 
                    Message = $"Connection failed: {ex.Message}" 
                };
            }
        }
    }

    public class ProductData
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsLatest { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public string ImageBase64 { get; set; }

    }
}
    
