using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace OnlinePastryShop.Pages
{
    public partial class Login : System.Web.UI.Page
    {
        // Constants for login attempts
        private const int MAX_LOGIN_ATTEMPTS = 5;
        private const int LOCKOUT_DURATION_MINUTES = 5;
        
        // Login form controls
        protected TextBox txtLoginUsername;
        protected TextBox txtLoginPassword;
        protected CheckBox chkRememberMe;
        
        // Registration form controls
        protected TextBox txtRegisterFirstname;
        protected TextBox txtRegisterLastname;
        protected TextBox txtRegisterUsername;
        protected TextBox txtRegisterEmail;
        protected TextBox txtRegisterPhone;
        protected TextBox txtRegisterPassword;
        protected TextBox txtRegisterConfirmPassword;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if user is already logged in
            if (Session["UserID"] != null && Session["Username"] != null)
            {
                // Redirect based on role
                if (Session["UserRole"] != null && Session["UserRole"].ToString() == "Admin")
                {
                    Response.Redirect("Dashboard.aspx");
                }
                else
                {
                    Response.Redirect("Default.aspx");
                }
            }
        }
        
        protected void LoginButton_Click(object sender, EventArgs e)
        {
            string username = txtLoginUsername.Text.Trim();
            string password = txtLoginPassword.Text.Trim();
            bool rememberMe = chkRememberMe.Checked;
            
            // Basic validation
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowClientAlert("Please enter both username/email and password.", "error");
                return;
            }
            
            try
            {
                // Determine if input is email or username
                bool isEmail = username.Contains("@");
                
                // Login logic
                using (OracleConnection connection = new OracleConnection(GetConnectionString()))
                {
                    connection.Open();
                    
                    // Get user data based on username or email
                    string query = isEmail 
                        ? "SELECT USERID, USERNAME, PASSWORDHASH, ROLE, FAILEDLOGINATTEMPTS, ACCOUNTSTATUS, LOCKOUTUNTIL FROM USERS WHERE EMAIL = :Email" 
                        : "SELECT USERID, USERNAME, PASSWORDHASH, ROLE, FAILEDLOGINATTEMPTS, ACCOUNTSTATUS, LOCKOUTUNTIL FROM USERS WHERE USERNAME = :Username";
                    
                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        if (isEmail)
                            command.Parameters.Add(":Email", OracleDbType.Varchar2).Value = username;
                        else
                            command.Parameters.Add(":Username", OracleDbType.Varchar2).Value = username;
                        
                        using (OracleDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = Convert.ToInt32(reader["USERID"]);
                                string storedUsername = reader["USERNAME"].ToString();
                                string storedPassword = reader["PASSWORDHASH"].ToString();
                                string role = reader["ROLE"].ToString();
                                int failedAttempts = Convert.ToInt32(reader["FAILEDLOGINATTEMPTS"]);
                                string accountStatus = reader["ACCOUNTSTATUS"].ToString();
                                object lockoutUntil = reader["LOCKOUTUNTIL"];
                                
                                // Check account status
                                if (accountStatus != "Active")
                                {
                                    ShowClientAlert($"Your account is {accountStatus.ToLower()}. Please contact support.", "error");
                                    return;
                                }
                                
                                // Check if account is locked
                                if (lockoutUntil != DBNull.Value)
                                {
                                    DateTime lockoutTime = Convert.ToDateTime(lockoutUntil);
                                    if (lockoutTime > DateTime.Now)
                                    {
                                        TimeSpan remainingTime = lockoutTime - DateTime.Now;
                                        ShowClientAlert($"Your account is locked. Please try again in {Math.Ceiling(remainingTime.TotalMinutes)} minutes.", "error");
                                        return;
                                    }
                                }
                                
                                // Verify password
                                if (password == storedPassword) // Simple comparison for now - in production, use proper hashing
                                {
                                    // Reset failed attempts
                                    UpdateLoginStatus(connection, userId, true);
                                    
                                    // Create session
                                    Session["UserID"] = userId;
                                    Session["Username"] = storedUsername;
                                    Session["UserRole"] = role;
                                    
                                    // Set authentication cookie if remember me is checked
                                    if (rememberMe)
                                    {
                                        CreateRememberMeCookie(userId, storedUsername);
                                    }
                                    
                                    // Redirect based on role
                                    if (role == "Admin")
                                    {
                                        Response.Redirect("Dashboard.aspx");
                                    }
                                    else
                                    {
                                        Response.Redirect("Default.aspx");
                                    }
                                }
                                else
                                {
                                    // Increment failed attempts
                                    int newFailedAttempts = failedAttempts + 1;
                                    bool isLocked = newFailedAttempts >= MAX_LOGIN_ATTEMPTS;
                                    DateTime? lockoutUntilTime = isLocked ? DateTime.Now.AddMinutes(LOCKOUT_DURATION_MINUTES) : (DateTime?)null;
                                    
                                    UpdateLoginStatus(connection, userId, false, newFailedAttempts, isLocked, lockoutUntilTime);
                                    
                                    if (isLocked)
                                    {
                                        ShowClientAlert($"Too many failed login attempts. Your account has been locked for {LOCKOUT_DURATION_MINUTES} minutes.", "error");
                                    }
                                    else
                                    {
                                        ShowClientAlert("Invalid username or password.", "error");
                                    }
                                }
                            }
                            else
                            {
                                // User not found
                                ShowClientAlert("Invalid username or password.", "error");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowClientAlert("An error occurred during login. Please try again later.", "error");
                // Log the error in production
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
            }
        }
        
        protected void RegisterButton_Click(object sender, EventArgs e)
        {
            string firstname = txtRegisterFirstname.Text.Trim();
            string lastname = txtRegisterLastname.Text.Trim();
            string username = txtRegisterUsername.Text.Trim();
            string email = txtRegisterEmail.Text.Trim();
            string phone = txtRegisterPhone.Text.Trim();
            string password = txtRegisterPassword.Text.Trim();
            string confirmPassword = txtRegisterConfirmPassword.Text.Trim();
            
            // Basic validation
            if (string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname) || 
                string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || 
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ShowClientAlert("Please fill in all required fields.", "error");
                return;
            }
            
            // Password validation
            if (password.Length < 6)
            {
                ShowClientAlert("Password must be at least 6 characters long.", "error");
                return;
            }
            
            // Password match validation
            if (password != confirmPassword)
            {
                ShowClientAlert("Passwords do not match.", "error");
                return;
            }
            
            try
            {
                using (OracleConnection connection = new OracleConnection(GetConnectionString()))
                {
                    connection.Open();
                    
                    // Check if username already exists
                    if (IsUsernameExists(connection, username))
                    {
                        ShowClientAlert("Username already exists. Please choose a different one.", "error");
                        return;
                    }
                    
                    // Check if email already exists
                    if (IsEmailExists(connection, email))
                    {
                        ShowClientAlert("Email already exists. Please use a different one.", "error");
                        return;
                    }
                    
                    // Register the user
                    int userId = RegisterUser(connection, username, email, password, firstname, lastname, phone);
                    
                    if (userId > 0)
                    {
                        // Create session
                        Session["UserID"] = userId;
                        Session["Username"] = username;
                        Session["UserRole"] = "Customer"; // Default role
                        
                        // Show registration success and redirect
                        ShowClientAlert("Registration successful! Redirecting to home page...", "success");
                        
                        // Use JavaScript to redirect after showing the message
                        ClientScript.RegisterStartupScript(GetType(), "redirectScript", 
                            "setTimeout(function() { window.location = 'Default.aspx'; }, 2000);", true);
                    }
                    else
                    {
                        ShowClientAlert("Registration failed. Please try again later.", "error");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowClientAlert("An error occurred during registration. Please try again later.", "error");
                // Log the error in production
                System.Diagnostics.Debug.WriteLine($"Registration error: {ex.Message}");
            }
        }
        
        #region Helper Methods
        
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
                    // Fallback to hardcoded connection string
                    return "User Id=mecate;Password=qwen123;Data Source=localhost:1521/xe;";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting connection string: {ex.Message}");
                // Fallback to hardcoded connection string
                return "User Id=mecate;Password=qwen123;Data Source=localhost:1521/xe;";
            }
        }
        
        private void UpdateLoginStatus(OracleConnection connection, int userId, bool isSuccess, int failedAttempts = 0, bool isLocked = false, DateTime? lockoutUntil = null)
        {
            try
            {
                string query;
                
                if (isSuccess)
                {
                    // Reset failed attempts on successful login
                    query = "UPDATE USERS SET FAILEDLOGINATTEMPTS = 0, LASTLOGIN = SYSDATE, ACCOUNTSTATUS = 'Active', LOCKOUTUNTIL = NULL WHERE USERID = :UserId";
                }
                else
                {
                    // Update failed attempts and possibly lock account
                    if (isLocked)
                    {
                        query = "UPDATE USERS SET FAILEDLOGINATTEMPTS = :FailedAttempts, ACCOUNTSTATUS = 'Locked', LOCKOUTUNTIL = :LockoutUntil WHERE USERID = :UserId";
                    }
                    else
                    {
                        query = "UPDATE USERS SET FAILEDLOGINATTEMPTS = :FailedAttempts WHERE USERID = :UserId";
                    }
                }
                
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(":UserId", OracleDbType.Int32).Value = userId;
                    
                    if (!isSuccess)
                    {
                        command.Parameters.Add(":FailedAttempts", OracleDbType.Int32).Value = failedAttempts;
                        
                        if (isLocked && lockoutUntil.HasValue)
                        {
                            command.Parameters.Add(":LockoutUntil", OracleDbType.Date).Value = lockoutUntil.Value;
                        }
                    }
                    
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // Log the error in production
                System.Diagnostics.Debug.WriteLine($"Error updating login status: {ex.Message}");
            }
        }
        
        private bool IsUsernameExists(OracleConnection connection, string username)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM USERS WHERE USERNAME = :Username";
                
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(":Username", OracleDbType.Varchar2).Value = username;
                    
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                // Log the error in production
                System.Diagnostics.Debug.WriteLine($"Error checking username: {ex.Message}");
                return false;
            }
        }
        
        private bool IsEmailExists(OracleConnection connection, string email)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM USERS WHERE EMAIL = :Email";
                
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(":Email", OracleDbType.Varchar2).Value = email;
                    
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                // Log the error in production
                System.Diagnostics.Debug.WriteLine($"Error checking email: {ex.Message}");
                return false;
            }
        }
        
        private int RegisterUser(OracleConnection connection, string username, string email, string password, string firstname, string lastname, string phone)
        {
            try
            {
                // In a production environment, use proper password hashing
                string query = @"
                    INSERT INTO USERS (
                        USERNAME, EMAIL, PASSWORDHASH, FIRSTNAME, LASTNAME, PHONENUMBER, 
                        ROLE, DATECREATED, DATEMODIFIED, ISACTIVE, ACCOUNTSTATUS, FAILEDLOGINATTEMPTS
                    ) VALUES (
                        :Username, :Email, :PasswordHash, :FirstName, :LastName, :PhoneNumber,
                        'Customer', SYSDATE, SYSDATE, 1, 'Active', 0
                    ) RETURNING USERID INTO :UserId";
                
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(":Username", OracleDbType.Varchar2).Value = username;
                    command.Parameters.Add(":Email", OracleDbType.Varchar2).Value = email;
                    command.Parameters.Add(":PasswordHash", OracleDbType.Varchar2).Value = password; // Use proper hashing in production
                    command.Parameters.Add(":FirstName", OracleDbType.Varchar2).Value = firstname;
                    command.Parameters.Add(":LastName", OracleDbType.Varchar2).Value = lastname;
                    command.Parameters.Add(":PhoneNumber", OracleDbType.Varchar2).Value = string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone;
                    
                    // Output parameter for the new user ID
                    OracleParameter userIdParam = new OracleParameter(":UserId", OracleDbType.Int32);
                    userIdParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(userIdParam);
                    
                    command.ExecuteNonQuery();
                    
                    return Convert.ToInt32(userIdParam.Value.ToString());
                }
            }
            catch (Exception ex)
            {
                // Log the error in production
                System.Diagnostics.Debug.WriteLine($"Error registering user: {ex.Message}");
                return -1;
            }
        }
        
        private void CreateRememberMeCookie(int userId, string username)
        {
            try
            {
                // Create a token (in production, use a secure random token generator)
                string token = GenerateRememberToken();
                
                // Set cookie
                HttpCookie cookie = new HttpCookie("RememberMe");
                cookie.Values["UserID"] = userId.ToString();
                cookie.Values["Username"] = username;
                cookie.Values["Token"] = token;
                cookie.Expires = DateTime.Now.AddDays(30); // 30 days expiration
                cookie.HttpOnly = true; // Not accessible via JavaScript
                Response.Cookies.Add(cookie);
                
                // Store token in database
                using (OracleConnection connection = new OracleConnection(GetConnectionString()))
                {
                    connection.Open();
                    
                    string query = "UPDATE USERS SET REMEMBERTOKEN = :Token, REMEMBERTOKENEXPIRY = :Expiry WHERE USERID = :UserId";
                    
                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add(":Token", OracleDbType.Varchar2).Value = token;
                        command.Parameters.Add(":Expiry", OracleDbType.Date).Value = DateTime.Now.AddDays(30);
                        command.Parameters.Add(":UserId", OracleDbType.Int32).Value = userId;
                        
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error in production
                System.Diagnostics.Debug.WriteLine($"Error creating remember me cookie: {ex.Message}");
            }
        }
        
        private string GenerateRememberToken()
        {
            // Simple token generation - use a more secure method in production
            byte[] tokenData = new byte[32];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(tokenData);
            }
            return Convert.ToBase64String(tokenData);
        }
        
        private void ShowClientAlert(string message, string type)
        {
            // Register a startup script to show the toast notification
            string script = $"setTimeout(function() {{ showToast('{message}', '{type}'); }}, 100);";
            ScriptManager.RegisterStartupScript(this, GetType(), "toastScript", script, true);
        }
        
        #endregion
        
        #region AJAX Methods
        
        [WebMethod]
        public static object CheckUsernameAvailability(string username)
        {
            try
            {
                using (OracleConnection connection = new OracleConnection(GetConnectionStringStatic()))
                {
                    connection.Open();
                    
                    string query = "SELECT COUNT(*) FROM USERS WHERE USERNAME = :Username";
                    
                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add(":Username", OracleDbType.Varchar2).Value = username;
                        
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        
                        return new
                        {
                            Success = true,
                            IsAvailable = count == 0,
                            Message = count == 0 ? "Username is available" : "Username already exists"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new
                {
                    Success = false,
                    IsAvailable = false,
                    Message = "Error checking username availability"
                };
            }
        }
        
        [WebMethod]
        public static object CheckEmailAvailability(string email)
        {
            try
            {
                using (OracleConnection connection = new OracleConnection(GetConnectionStringStatic()))
                {
                    connection.Open();
                    
                    string query = "SELECT COUNT(*) FROM USERS WHERE EMAIL = :Email";
                    
                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add(":Email", OracleDbType.Varchar2).Value = email;
                        
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        
                        return new
                        {
                            Success = true,
                            IsAvailable = count == 0,
                            Message = count == 0 ? "Email is available" : "Email already exists"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new
                {
                    Success = false,
                    IsAvailable = false,
                    Message = "Error checking email availability"
                };
            }
        }
        
        private static string GetConnectionStringStatic()
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
                    // Fallback to hardcoded connection string
                    return "User Id=mecate;Password=qwen123;Data Source=localhost:1521/xe;";
                }
            }
            catch (Exception ex)
            {
                // Fallback to hardcoded connection string
                return "User Id=mecate;Password=qwen123;Data Source=localhost:1521/xe;";
            }
        }
        
        #endregion
    }
}