using System;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;

namespace OnlinePastryShop.Pages
{
    public partial class Reviews : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindReviews();
            }
        }

        private void BindReviews()
        {
            string statusFilter = ddlFilterStatus.SelectedValue;
            string searchTerm = txtSearch.Text.Trim();
            
            string query = @"
                SELECT 
                    PR.ReviewId, 
                    PR.UserId, 
                    PR.ProductId, 
                    PR.Rating, 
                    PR.ReviewText, 
                    PR.ReviewDate, 
                    PR.IsApproved,
                    U.FirstName || ' ' || U.LastName AS UserName,
                    U.Email,
                    P.Name AS ProductName,
                    P.ImageFileName AS ProductImage
                FROM 
                    ProductRatings PR
                JOIN 
                    Users U ON PR.UserId = U.UserId
                JOIN 
                    Products P ON PR.ProductId = P.ProductId
                WHERE 
                    1=1";

            // Apply filters
            if (statusFilter != "All")
            {
                if (statusFilter == "Pending")
                {
                    query += " AND PR.IsApproved IS NULL";
                }
                else if (statusFilter == "Approved")
                {
                    query += " AND PR.IsApproved = 1";
                }
                else if (statusFilter == "Rejected")
                {
                    query += " AND PR.IsApproved = 0";
                }
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query += @" AND (
                    LOWER(U.FirstName || ' ' || U.LastName) LIKE LOWER('%" + searchTerm + @"%') OR
                    LOWER(P.Name) LIKE LOWER('%" + searchTerm + @"%') OR
                    LOWER(PR.ReviewText) LIKE LOWER('%" + searchTerm + @"%')
                )";
            }

            query += " ORDER BY PR.ReviewDate DESC";

            DataTable dt = ExecuteQuery(query);
            gvReviews.DataSource = dt;
            gvReviews.DataBind();
        }

        protected string GetStarRating(int rating)
        {
            StringBuilder stars = new StringBuilder();
            
            for (int i = 1; i <= 5; i++)
            {
                if (i <= rating)
                {
                    // Full star
                    stars.Append("<i class='fas fa-star text-yellow-400'></i>");
                }
                else
                {
                    // Empty star
                    stars.Append("<i class='far fa-star text-yellow-400'></i>");
                }
            }
            
            return stars.ToString();
        }

        protected string GetStatusText(bool isApproved)
        {
            if (isApproved)
            {
                return "Approved";
            }
            else
            {
                return "Rejected";
            }
        }

        protected string GetStatusCssClass(bool isApproved)
        {
            if (isApproved)
            {
                return "inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-800";
            }
            else
            {
                return "inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-red-100 text-red-800";
            }
        }

        protected void gvReviews_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int reviewId = Convert.ToInt32(e.CommandArgument);
            
            if (e.CommandName == "ApproveReview")
            {
                UpdateReviewStatus(reviewId, true);
            }
            else if (e.CommandName == "RejectReview")
            {
                UpdateReviewStatus(reviewId, false);
            }
            else if (e.CommandName == "DeleteReview")
            {
                DeleteReview(reviewId);
            }
            
            BindReviews();
        }

        private void UpdateReviewStatus(int reviewId, bool isApproved)
        {
            string query = "UPDATE ProductRatings SET IsApproved = :IsApproved WHERE ReviewId = :ReviewId";
            
            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(":IsApproved", OracleDbType.Int32).Value = isApproved ? 1 : 0;
                    command.Parameters.Add(":ReviewId", OracleDbType.Int32).Value = reviewId;
                    
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        // Log the exception
                        System.Diagnostics.Debug.WriteLine($"Error updating review status: {ex.Message}");
                    }
                }
            }
        }

        private void DeleteReview(int reviewId)
        {
            string query = "DELETE FROM ProductRatings WHERE ReviewId = :ReviewId";
            
            using (OracleConnection connection = new OracleConnection(GetConnectionString()))
            {
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(":ReviewId", OracleDbType.Int32).Value = reviewId;
                    
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        // Log the exception
                        System.Diagnostics.Debug.WriteLine($"Error deleting review: {ex.Message}");
                    }
                }
            }
        }

        protected void ddlFilterStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindReviews();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            BindReviews();
        }

        protected void gvReviews_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvReviews.PageIndex = e.NewPageIndex;
            BindReviews();
        }

        protected void gvReviews_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Update the product image path if needed
                Image productImage = (Image)e.Row.FindControl("imgProduct");
                if (productImage != null)
                {
                    string imageFileName = DataBinder.Eval(e.Row.DataItem, "ProductImage").ToString();
                    if (!string.IsNullOrEmpty(imageFileName))
                    {
                        productImage.ImageUrl = $"~/Images/Products/{imageFileName}";
                    }
                    else
                    {
                        productImage.ImageUrl = "~/Images/placeholder.png";
                    }
                }
            }
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