using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using OnlinePastryShop.App_Code.Utilities;

public partial class Login : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // If user is already logged in, redirect to home page
        if (Session["UserID"] != null)
        {
            Response.Redirect("~/Default.aspx");
        }

        // Check if we have a return URL
        if (!IsPostBack && Request.QueryString["ReturnUrl"] != null)
        {
            lblErrorMessage.Text = "You must log in to access that page.";
            lblErrorMessage.Visible = true;
        }
    }

    protected void btnLogin_Click(object sender, EventArgs e)
    {
        string email = txtEmail.Text.Trim();
        string password = txtPassword.Text;

        // Validate input
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("Email and password are required.");
            return;
        }

        try
        {
            // Get connection string from web.config
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT UserID, Email, Password, FirstName, LastName, Username, Role FROM Users WHERE Email = @Email";
                
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Get the stored hash from the database
                            string storedHash = reader["Password"].ToString();
                            
                            // Verify the password
                            bool isValid = PasswordHasher.VerifyPassword(password, storedHash);
                            
                            if (isValid)
                            {
                                // Create user info object
                                UserInfo userInfo = new UserInfo
                                {
                                    UserId = Convert.ToInt32(reader["UserID"]),
                                    Email = reader["Email"].ToString(),
                                    FirstName = reader["FirstName"].ToString(),
                                    LastName = reader["LastName"].ToString(),
                                    Username = reader["Username"].ToString(),
                                    Role = reader["Role"].ToString()
                                };
                                
                                // Store user info in session
                                Session["UserID"] = userInfo.UserId;
                                Session["UserInfo"] = userInfo;
                                
                                // Set authentication cookie if Remember Me is checked
                                if (chkRememberMe.Checked)
                                {
                                    HttpCookie authCookie = new HttpCookie("AuthCookie");
                                    authCookie.Values["UserID"] = userInfo.UserId.ToString();
                                    authCookie.Expires = DateTime.Now.AddDays(30); // Cookie expires in 30 days
                                    Response.Cookies.Add(authCookie);
                                }
                                
                                // Redirect based on role
                                if (userInfo.IsAdmin)
                                {
                                    Response.Redirect("~/Admin/Dashboard.aspx");
                                }
                                else
                                {
                                    Response.Redirect("~/Default.aspx");
                                }
                            }
                            else
                            {
                                ShowError("Invalid email or password.");
                            }
                        }
                        else
                        {
                            ShowError("Invalid email or password.");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ShowError("An error occurred: " + ex.Message);
        }
    }

    private void ShowError(string message)
    {
        pnlError.Visible = true;
        litError.Text = message;
    }
} 