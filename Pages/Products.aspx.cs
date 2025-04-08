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
using System.Web;

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
                    using (OracleCommand cmd = new OracleCommand("SELECT COUNT(*) FROM Products WHERE IsActive = 1", conn))
                    {
                        totalProducts = Convert.ToInt32(cmd.ExecuteScalar());
                        System.Diagnostics.Debug.WriteLine($"Total active products: {totalProducts}");
                    }

                    // Get out of stock count
                    using (OracleCommand cmd = new OracleCommand("SELECT COUNT(*) FROM Products WHERE StockQuantity = 0 AND IsActive = 1", conn))
                    {
                        outOfStock = Convert.ToInt32(cmd.ExecuteScalar());
                        System.Diagnostics.Debug.WriteLine($"Out of stock products: {outOfStock}");
                    }

                    // Get low stock count
                    using (OracleCommand cmd = new OracleCommand("SELECT COUNT(*) FROM Products WHERE StockQuantity > 0 AND StockQuantity < 10 AND IsActive = 1", conn))
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
                System.Diagnostics.Debug.WriteLine("======================== BEGIN GetProducts ========================");
                System.Diagnostics.Debug.WriteLine($"Params: search='{search}', categoryId='{categoryId}', sort='{sort}', page={page}, pageSize={pageSize}, includeDeleted={includeDeleted}");

                // Prepare return value in case of early exit
                List<object> emptyList = new List<object>();

                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine("Database connection opened");

                    // Modified query to handle includeDeleted parameter
                    string sql = @"
                        SELECT 
                            p.PRODUCTID, 
                            p.NAME, 
                            p.DESCRIPTION, 
                            p.PRICE, 
                            p.COSTPRICE,
                            p.STOCKQUANTITY, 
                            p.ISLATEST,
                            p.ISACTIVE,
                            c.NAME as CATEGORYNAME, 
                            c.CATEGORYID,
                            CASE WHEN p.IMAGE IS NOT NULL THEN 1 ELSE 0 END as HAS_IMAGE
                        FROM PRODUCTS p
                        LEFT JOIN PRODUCTCATEGORIES pc ON p.PRODUCTID = pc.PRODUCTID 
                        LEFT JOIN CATEGORIES c ON pc.CATEGORYID = c.CATEGORYID";

                    // Only filter by IsActive if we're not including deleted products
                    if (!includeDeleted)
                    {
                        sql += " WHERE p.ISACTIVE = 1";
                    }

                    sql += " ORDER BY p.PRODUCTID DESC";

                    List<object> products = new List<object>();
                    Dictionary<int, object> productMap = new Dictionary<int, object>(); // For deduplication
                    int totalCount = 0;

                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        System.Diagnostics.Debug.WriteLine($"Executing query to get products {(includeDeleted ? "including deleted ones" : "active only")}");

                        try
                        {
                            using (OracleDataReader reader = cmd.ExecuteReader())
                            {
                                System.Diagnostics.Debug.WriteLine("Reader opened successfully");

                                while (reader.Read())
                                {
                                    int productId = Convert.ToInt32(reader["PRODUCTID"]);
                                    bool hasImage = Convert.ToInt32(reader["HAS_IMAGE"]) == 1;
                                    decimal price = Convert.ToDecimal(reader["PRICE"]);
                                    decimal costPrice = reader["COSTPRICE"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["COSTPRICE"]);
                                    int stockQuantity = Convert.ToInt32(reader["STOCKQUANTITY"]);
                                    string name = reader["NAME"].ToString();
                                    string description = reader["DESCRIPTION"] == DBNull.Value ? "" : reader["DESCRIPTION"].ToString();
                                    bool isLatest = Convert.ToInt32(reader["ISLATEST"]) == 1;
                                    bool isActive = Convert.ToInt32(reader["ISACTIVE"]) == 1;
                                    string categoryName = reader["CATEGORYNAME"] == DBNull.Value ? "Uncategorized" : reader["CATEGORYNAME"].ToString();
                                    int catId = reader["CATEGORYID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CATEGORYID"]);

                                    // Skip duplicates (same product with multiple categories)
                                    if (productMap.ContainsKey(productId))
                                    {
                                        continue;
                                    }

                                    // Apply search filter in memory
                                    if (!string.IsNullOrEmpty(search) &&
                                        !name.ToLower().Contains(search.ToLower()))
                                    {
                                        continue;
                                    }

                                    // Check if we're filtering by "latest" products
                                    if (categoryId == "latest" && !isLatest)
                                    {
                                        continue;
                                    }
                                    // Apply regular category filter in memory
                                    else if (!string.IsNullOrEmpty(categoryId) && categoryId != "latest" && categoryId != "0" &&
                                        catId.ToString() != categoryId)
                                    {
                                        continue;
                                    }

                                    var product = new
                                    {
                                        ProductId = productId,
                                        Name = name,
                                        Description = description,
                                        Price = price,
                                        CostPrice = costPrice,
                                        StockQuantity = stockQuantity,
                                        IsLatest = isLatest,
                                        IsActive = isActive,
                                        CategoryName = categoryName,
                                        CategoryId = catId,
                                        HasImage = hasImage
                                    };

                                    productMap[productId] = product;
                                }

                                // Convert dictionary to list
                                products = productMap.Values.ToList();
                                totalCount = products.Count;

                                // Apply sorting in memory
                                if (!string.IsNullOrEmpty(sort))
                                {
                                    switch (sort.ToLower())
                                    {
                                        case "name_asc":
                                            products = products.OrderBy(p => ((dynamic)p).Name).ToList();
                                            break;
                                        case "name_desc":
                                            products = products.OrderByDescending(p => ((dynamic)p).Name).ToList();
                                            break;
                                        case "price_asc":
                                            products = products.OrderBy(p => ((dynamic)p).Price).ToList();
                                            break;
                                        case "price_desc":
                                            products = products.OrderByDescending(p => ((dynamic)p).Price).ToList();
                                            break;
                                        case "stock_asc":
                                            products = products.OrderBy(p => ((dynamic)p).StockQuantity).ToList();
                                            break;
                                        case "stock_desc":
                                            products = products.OrderByDescending(p => ((dynamic)p).StockQuantity).ToList();
                                            break;
                                        default:
                                            products = products.OrderByDescending(p => ((dynamic)p).ProductId).ToList();
                                            break;
                                    }
                                }
                                else
                                {
                                    // Default sort by ID descending
                                    products = products.OrderByDescending(p => ((dynamic)p).ProductId).ToList();
                                }

                                // Apply pagination in memory
                                int offset = (page - 1) * pageSize;
                                products = products.Skip(offset).Take(pageSize).ToList();

                                System.Diagnostics.Debug.WriteLine($"Processed {products.Count} products out of {totalCount} total");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in GetProducts: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                            // Return empty result on error rather than crashing
                            System.Diagnostics.Debug.WriteLine("======================== END GetProducts (ERROR) ========================");
                            return new { Products = emptyList, TotalCount = 0, Error = ex.Message };
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Returning {products.Count} products to client");
                    System.Diagnostics.Debug.WriteLine("======================== END GetProducts ========================");
                    return new { Products = products, TotalCount = totalCount };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPTION in GetProducts: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new { Products = new List<object>(), TotalCount = 0, Error = ex.Message };
            }
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
                        "SELECT CategoryId, Name FROM Categories WHERE IsActive = 1 ORDER BY Name", conn))
                    {
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                categories.Add(new
                                {
                                    CategoryId = reader["CategoryId"].ToString(),
                                    Name = reader["Name"].ToString()
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
                        SELECT p.*, c.CategoryId
                        FROM Products p
                        LEFT JOIN ProductCategories pc ON p.ProductId = pc.ProductId
                        LEFT JOIN Categories c ON pc.CategoryId = c.CategoryId
                        WHERE p.ProductId = :ProductId AND p.IsActive = 1";

                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        cmd.Parameters.Add("ProductId", OracleDbType.Int32).Value = productId;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var product = new
                                {
                                    ProductId = Convert.ToInt32(reader["ProductId"]),
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"]?.ToString(),
                                    Price = Convert.ToDecimal(reader["Price"]),
                                    CostPrice = reader["CostPrice"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["CostPrice"]),
                                    StockQuantity = Convert.ToInt32(reader["StockQuantity"]),
                                    ImageBase64 = reader["Image"] != DBNull.Value ?
                                        Convert.ToBase64String((byte[])reader["Image"]) : null,
                                    IsLatest = Convert.ToBoolean(reader["IsLatest"]),
                                    CategoryId = reader["CategoryId"] != DBNull.Value ?
                                        Convert.ToInt32(reader["CategoryId"]) : 0
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

                            using (OracleCommand checkCmd = new OracleCommand(
                                "SELECT COUNT(*) as Count, MAX(Name) as ExistingName FROM Products WHERE LOWER(Name) = LOWER(:name) AND IsActive = 1", conn))
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
                            using (OracleCommand similarCmd = new OracleCommand(
                                "SELECT ProductId, Name FROM Products WHERE Name LIKE :similar AND IsActive = 1", conn))
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
                                INSERT INTO Products (
                                    Name, Description, Price, CostPrice, StockQuantity, 
                                    Image, IsActive, IsLatest
                                ) VALUES (
                                    :Name, :Description, :Price, :CostPrice, :StockQuantity,
                                    :Image, 1, :IsLatest
                                ) RETURNING ProductId INTO :ProductId";

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

                                using (OracleCommand catCmd = new OracleCommand(
                                    "INSERT INTO ProductCategories (ProductId, CategoryId) VALUES (:pid, :cid)", conn))
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

                            response["status"] = "success";
                            response["message"] = "Product added successfully";
                            response["productId"] = newProductId;
                        }
                        catch (Exception ex)
                        {
                            // Roll back the transaction
                            System.Diagnostics.Debug.WriteLine($"ERROR in transaction, rolling back: {ex.Message}");
                            transaction.Rollback();

                            response["status"] = "error";
                            response["message"] = $"Error adding product: {ex.Message}";
                            response["details"] = ex.ToString();
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
            }

            System.Diagnostics.Debug.WriteLine($"Response: status={response["status"]}, message={response["message"]}");
            System.Diagnostics.Debug.WriteLine("=========== END AddProductSimple ===========");

            return response;
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
                    return new { status = "error", message = "Stock quantity cannot exceed 1000 items." };
                }

                if (costPrice <= 0)
                {
                    return new { status = "error", message = "Cost price must be greater than zero." };
                }

                if (price <= 0)
                {
                    return new { status = "error", message = "Price must be greater than zero." };
                }

                if (costPrice >= price)
                {
                    return new { status = "error", message = "Cost price must be less than selling price." };
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

                // Validate name contains at least one letter and no numbers
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new { status = "error", message = "Product name is required." };
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
                    return new { status = "error", message = "Product name must contain at least one letter." };
                }

                if (containsNumber)
                {
                    return new { status = "error", message = "Product name cannot contain any numbers." };
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
                                SELECT COUNT(*) FROM Products 
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
                                    return new { status = "error", message = "A product with this name already exists." };
                                }
                            }

                            // Use named parameters for basic info update
                            string updateBasicSql = @"
                                UPDATE Products SET 
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
                                        string updateImageSql = "UPDATE Products SET Image = :Image WHERE ProductId = :ProductId";
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
                            return new { status = "success", message = "Product updated successfully." };
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
                return new { status = "error", message = $"Error updating product: {ex.Message}" };
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

                    using (OracleTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // First, delete any category associations
                            using (OracleCommand deleteAssociations = new OracleCommand(
                                "DELETE FROM ProductCategories WHERE ProductId = :ProductId", conn))
                            {
                                deleteAssociations.Transaction = transaction;
                                deleteAssociations.Parameters.Add("ProductId", OracleDbType.Int32).Value = productId;
                                int associationsDeleted = deleteAssociations.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine($"Deleted {associationsDeleted} category associations for product {productId}");
                            }

                            // Then delete the product
                            using (OracleCommand cmd = new OracleCommand(
                                "DELETE FROM Products WHERE ProductId = :ProductId", conn))
                            {
                                cmd.Transaction = transaction;
                                cmd.Parameters.Add("ProductId", OracleDbType.Int32).Value = productId;

                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected == 0)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Product {productId} not found for deletion");
                                    return new { status = "error", message = "Product not found." };
                                }

                                System.Diagnostics.Debug.WriteLine($"Product {productId} PERMANENTLY deleted successfully");

                                // Commit the transaction
                                transaction.Commit();

                                return new
                                {
                                    status = "success",
                                    message = "Product permanently deleted from the database."
                                };
                            }
                        }
                        catch (Exception ex)
                        {
                            // Rollback the transaction if any errors occur
                            transaction.Rollback();
                            System.Diagnostics.Debug.WriteLine($"Transaction rolled back due to error: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting product: {ex.Message}");
                return new { status = "error", message = $"Error deleting product: {ex.Message}" };
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
                    using (OracleCommand cmd = new OracleCommand("SELECT IMAGE, NAME FROM PRODUCTS WHERE PRODUCTID = :ProductId", conn))
                    {
                        cmd.Parameters.Add("ProductId", OracleDbType.Int32).Value = productId;

                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read() && reader["IMAGE"] != DBNull.Value)
                            {
                                byte[] imageBytes = (byte[])reader["IMAGE"];
                                string productName = reader["NAME"].ToString();
                                string base64String = Convert.ToBase64String(imageBytes);
                                string imageTag = $"<img src='data:image/jpeg;base64,{base64String}' alt='{HttpUtility.HtmlAttributeEncode(productName)}' class='w-full h-full object-cover' />";
                                System.Diagnostics.Debug.WriteLine($"Image found for product {productId}, size: {imageBytes.Length} bytes");
                                return new { ImageHtml = imageTag };
                            }
                            System.Diagnostics.Debug.WriteLine($"No image found for product {productId}");
                            return new { ImageHtml = "<img src='../Images/product-placeholder.svg' alt='Product image coming soon' class='w-full h-full object-cover' />" };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting product image: {ex.Message}");
                return new { ImageHtml = "<img src='../Images/product-placeholder.svg' alt='Error loading image' class='w-full h-full object-cover' />" };
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