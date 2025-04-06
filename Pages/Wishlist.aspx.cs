using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace OnlinePastryShop.Pages
{
    public partial class Wishlist : System.Web.UI.Page
    {
        // Public properties for UI binding
        public int TotalWishlistItems { get; private set; }
        public int UniqueUsersCount { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadWishlistData();
                LoadPopularWishlistItems();
            }
        }

        private void LoadWishlistData()
        {
            string searchTerm = txtSearch.Text.Trim();
            
            string query = @"
                SELECT 
                    W.WishlistId, 
                    W.UserId, 
                    W.ProductId, 
                    W.DateAdded,
                    U.FirstName || ' ' || U.LastName AS UserName,
                    U.Email,
                    P.Name AS ProductName,
                    P.Price,
                    P.ImageFileName AS ProductImage,
                    CASE WHEN P.StockQuantity > 0 THEN 1 ELSE 0 END AS InStock
                FROM 
                    Wishlist W
                JOIN 
                    Users U ON W.UserId = U.UserId
                JOIN 
                    Products P ON W.ProductId = P.ProductId
                WHERE 
                    1=1";

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query += @" AND (
                    LOWER(U.FirstName || ' ' || U.LastName) LIKE LOWER('%" + searchTerm + @"%') OR
                    LOWER(U.Email) LIKE LOWER('%" + searchTerm + @"%') OR
                    LOWER(P.Name) LIKE LOWER('%" + searchTerm + @"%')
                )";
            }

            query += " ORDER BY W.DateAdded DESC";

            DataTable dt = ExecuteQuery(query);
            gvWishlist.DataSource = dt;
            gvWishlist.DataBind();
            
            // Get summary counts
            TotalWishlistItems = GetTotalWishlistItems();
            UniqueUsersCount = GetUniqueUsersCount();
        }

        private int GetTotalWishlistItems()
        {
            string query = "SELECT COUNT(*) FROM Wishlist";
            return ExecuteScalarQuery<int>(query);
        }

        private int GetUniqueUsersCount()
        {
            string query = "SELECT COUNT(DISTINCT UserId) FROM Wishlist";
            return ExecuteScalarQuery<int>(query);
        }

        private void LoadPopularWishlistItems()
        {
            string query = @"
                SELECT 
                    P.ProductId,
                    P.Name AS ProductName,
                    P.Description,
                    P.Price,
                    P.ImageFileName AS ProductImage,
                    COUNT(*) AS WishlistCount
                FROM 
                    Wishlist W
                JOIN 
                    Products P ON W.ProductId = P.ProductId
                GROUP BY 
                    P.ProductId, P.Name, P.Description, P.Price, P.ImageFileName
                ORDER BY 
                    WishlistCount DESC, P.Name
                FETCH FIRST 6 ROWS ONLY";

            DataTable dt = ExecuteQuery(query);
            rptPopularWishlistItems.DataSource = dt;
            rptPopularWishlistItems.DataBind();
        }

        protected void gvWishlist_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewProduct")
            {
                int productId = Convert.ToInt32(e.CommandArgument);
                Response.Redirect($"Products.aspx?id={productId}");
            }
            else if (e.CommandName == "EmailUser")
            {
                string[] args = e.CommandArgument.ToString().Split('|');
                int userId = Convert.ToInt32(args[0]);
                int productId = Convert.ToInt32(args[1]);
                
                // Implement email functionality or redirect to email composition page
                // For now, just redirect to a placeholder
                Response.Redirect($"EmailUser.aspx?userId={userId}&productId={productId}");
            }
            else if (e.CommandName == "DeleteWishlistItem")
            {
                int wishlistId = Convert.ToInt32(e.CommandArgument);
                DeleteWishlistItem(wishlistId);
                LoadWishlistData();
                LoadPopularWishlistItems();
            }
        }

        private void DeleteWishlistItem(int wishlistId)
        {
            string query = "DELETE FROM Wishlist WHERE WishlistId = :WishlistId";
            
            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(":WishlistId", OracleDbType.Int32).Value = wishlistId;
                    
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        // Log the exception
                        System.Diagnostics.Debug.WriteLine($"Error deleting wishlist item: {ex.Message}");
                    }
                }
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadWishlistData();
        }

        protected void gvWishlist_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvWishlist.PageIndex = e.NewPageIndex;
            LoadWishlistData();
        }

        private DataTable ExecuteQuery(string query)
        {
            DataTable dt = new DataTable();
            
            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        using (OracleDataAdapter adapter = new OracleDataAdapter(command))
                        {
                            adapter.Fill(dt);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the exception
                        System.Diagnostics.Debug.WriteLine($"Error executing query: {ex.Message}");
                    }
                }
            }
            
            return dt;
        }

        private T ExecuteScalarQuery<T>(string query)
        {
            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            return (T)Convert.ChangeType(result, typeof(T));
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the exception
                        System.Diagnostics.Debug.WriteLine($"Error executing scalar query: {ex.Message}");
                    }
                }
            }
            
            return default(T);
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