using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

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
            if (Session["UserID"] != null)
            {
                // Redirect based on role
                if (Session["UserRole"].ToString() == "Admin")
                {
                    Response.Redirect("Dashboard.aspx");
                }
                else
                {
                    Response.Redirect("Default.aspx");
                }
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // Get user input
            string usernameOrEmail = txtLoginEmail.Text.Trim();
            string password = txtLoginPassword.Text;

            // Validate user credentials
            if (ValidateUser(usernameOrEmail, password, out int userId, out string firstName, out string lastName, out string role))
            {
                // Create session variables
                Session["UserID"] = userId;
                Session["FirstName"] = firstName;
                Session["LastName"] = lastName;
                Session["UserRole"] = role;
                Session["UserInitials"] = GetInitials(firstName, lastName);

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
                // Show error message
                pnlLoginError.Visible = true;
                litLoginError.Text = "Invalid username/email or password. Please try again.";
            }
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                // Get form data
                string firstName = txtFirstName.Text.Trim();
                string lastName = txtLastName.Text.Trim();
                string username = txtUsername.Text.Trim();
                string email = txtEmail.Text.Trim();
                string phoneNumber = txtPhoneNumber.Text.Trim();
                string password = txtPassword.Text;
                
                // Validate form data
                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || 
                    string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || 
                    string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(password))
                {
                    ShowError("All fields are required.");
                    return;
                }
                
                // Server-side password validation
                if (password.Length < 8)
                {
                    ShowError("Password must be at least 8 characters long.");
                    return;
                }
                
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    
                    // Check for existing username, email, or phone
                    string checkQuery = @"SELECT 
                                            CASE 
                                                WHEN USERNAME = :Username THEN 'Username' 
                                                WHEN EMAIL = :Email THEN 'Email' 
                                                WHEN PHONENUMBER = :PhoneNumber THEN 'Phone' 
                                                ELSE NULL 
                                            END AS ConflictField
                                        FROM AARON_IPT.USERS
                                        WHERE USERNAME = :Username OR EMAIL = :Email OR PHONENUMBER = :PhoneNumber
                                        AND ROWNUM = 1";
                    
                    using (OracleCommand cmd = new OracleCommand(checkQuery, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("Username", OracleDbType.Varchar2)).Value = username;
                        cmd.Parameters.Add(new OracleParameter("Email", OracleDbType.Varchar2)).Value = email;
                        cmd.Parameters.Add(new OracleParameter("PhoneNumber", OracleDbType.Varchar2)).Value = phoneNumber;
                        
                        object result = cmd.ExecuteScalar();
                        
                        if (result != null && result != DBNull.Value)
                        {
                            string conflictField = result.ToString();
                            
                            if (conflictField == "Username")
                            {
                                ShowError("Username is already taken. Please choose a different username.");
                            }
                            else if (conflictField == "Email")
                            {
                                ShowError("Email is already registered. Please use a different email address.");
                            }
                            else if (conflictField == "Phone")
                            {
                                ShowError("Phone number is already registered. Please use a different phone number.");
                            }
                            else
                            {
                                ShowError("Registration failed. Please try again with different information.");
                            }
                            
                            return;
                        }
                        
                        // If we reach here, the user can be registered
                        string hashedPassword = HashPassword(password);
                        string role = "Customer";
                        int isActive = 1;
                        
                        // Insert the new user
                        string insertQuery = @"INSERT INTO AARON_IPT.USERS 
                                            (USERNAME, EMAIL, FIRSTNAME, LASTNAME, PASSWORDHASH, PHONENUMBER, ROLE, ISACTIVE, DATECREATED) 
                                            VALUES 
                                            (:Username, :Email, :FirstName, :LastName, :PasswordHash, :PhoneNumber, :Role, :IsActive, CURRENT_TIMESTAMP)";
                        
                        using (OracleCommand insertCmd = new OracleCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.Add(new OracleParameter("Username", OracleDbType.Varchar2)).Value = username;
                            insertCmd.Parameters.Add(new OracleParameter("Email", OracleDbType.Varchar2)).Value = email;
                            insertCmd.Parameters.Add(new OracleParameter("FirstName", OracleDbType.Varchar2)).Value = firstName;
                            insertCmd.Parameters.Add(new OracleParameter("LastName", OracleDbType.Varchar2)).Value = lastName;
                            insertCmd.Parameters.Add(new OracleParameter("PasswordHash", OracleDbType.Varchar2)).Value = hashedPassword;
                            insertCmd.Parameters.Add(new OracleParameter("PhoneNumber", OracleDbType.Varchar2)).Value = phoneNumber;
                            insertCmd.Parameters.Add(new OracleParameter("Role", OracleDbType.Varchar2)).Value = role;
                            insertCmd.Parameters.Add(new OracleParameter("IsActive", OracleDbType.Int32)).Value = isActive;
                            
                            int rowsAffected = insertCmd.ExecuteNonQuery();
                            
                            if (rowsAffected > 0)
                            {
                                // Registration successful
                                ShowSuccess("Registration successful! You can now log in with your credentials.");
                                
                                // Clear registration form
                                txtFirstName.Text = string.Empty;
                                txtLastName.Text = string.Empty;
                                txtUsername.Text = string.Empty;
                                txtEmail.Text = string.Empty;
                                txtPhoneNumber.Text = string.Empty;
                                txtPassword.Text = string.Empty;
                                txtConfirmPassword.Text = string.Empty;
                                
                                // Show toast notification and switch to login tab
                                string script = @"
                                    showLoginTab();
                                    
                                    // Create and show toast notification
                                    const toast = document.createElement('div');
                                    toast.className = 'fixed bottom-4 right-4 px-4 py-2 bg-green-500 text-white rounded-lg shadow-lg z-50';
                                    toast.innerHTML = 'Registration successful!';
                                    document.body.appendChild(toast);
                                    
                                    // Fade out and remove after 3 seconds
                                    setTimeout(() => {
                                        toast.style.transition = 'opacity 0.5s';
                                        toast.style.opacity = '0';
                                        setTimeout(() => toast.remove(), 500);
                                    }, 3000);
                                ";
                                
                                ScriptManager.RegisterStartupScript(this, GetType(), "RegistrationSuccess", script, true);
                            }
                            else
                            {
                                ShowError("Registration failed. Please try again later.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registration error: {ex.Message}");
                ShowError("An error occurred during registration. Please try again later.");
            }
        }

        private bool ValidateUser(string usernameOrEmail, string password, out int userId, out string firstName, out string lastName, out string role)
        {
            userId = 0;
            firstName = string.Empty;
            lastName = string.Empty;
            role = string.Empty;

            try
            {
                // Log login attempt
                System.Diagnostics.Debug.WriteLine($"Login attempt for: {usernameOrEmail}");
                
                using (OracleConnection conn = new OracleConnection(GetConnectionString()))
                {
                    conn.Open();
                    string query = @"SELECT USERID, FIRSTNAME, LASTNAME, PASSWORDHASH, ROLE 
                                    FROM AARON_IPT.USERS 
                                    WHERE (USERNAME = :UsernameOrEmail OR EMAIL = :UsernameOrEmail)
                                    AND ISACTIVE = 1";

                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter("UsernameOrEmail", OracleDbType.Varchar2)).Value = usernameOrEmail;
                        
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Get user data
                                userId = Convert.ToInt32(reader["USERID"]);
                                firstName = reader["FIRSTNAME"].ToString();
                                lastName = reader["LASTNAME"].ToString();
                                string hashedPassword = reader["PASSWORDHASH"].ToString();
                                role = reader["ROLE"].ToString();
                                
                                // Log retrieved user data (except password hash)
                                System.Diagnostics.Debug.WriteLine($"User found: ID={userId}, Name={firstName} {lastName}, Role={role}");
                                
                                // Hash the input password
                                string inputHash = HashPassword(password);
                                
                                // Log password hash comparison details
                                System.Diagnostics.Debug.WriteLine($"Password match: {hashedPassword == inputHash}");
                                System.Diagnostics.Debug.WriteLine($"Database hash: {hashedPassword}");
                                System.Diagnostics.Debug.WriteLine($"Input hash: {inputHash}");
                                
                                // Verify password by hashing the input password and comparing
                                return hashedPassword == inputHash;
                            }
                            else
                            {
                                // Log user not found
                                System.Diagnostics.Debug.WriteLine($"No user found for: {usernameOrEmail}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error (in a real application)
                System.Diagnostics.Debug.WriteLine($"Database error during login: {ex.Message}");
            }

            return false;
        }

        private string HashPassword(string password)
        {
            // Regular password hashing with SHA256
            using (SHA256 sha256 = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert the byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private string GetConnectionString()
        {
            // Get connection string from Web.config
            return ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString;
        }

        private string GetInitials(string firstName, string lastName)
        {
            string initials = "";
            
            if (!string.IsNullOrEmpty(firstName) && firstName.Length > 0)
            {
                initials += firstName[0];
            }
            
            if (!string.IsNullOrEmpty(lastName) && lastName.Length > 0)
            {
                initials += lastName[0];
            }
            
            return initials.ToUpper();
        }

        private void ShowError(string message)
        {
            pnlRegisterError.CssClass = "mb-5 p-4 bg-red-50 text-red-500 rounded-md";
            litRegisterError.Text = message;
            pnlRegisterError.Visible = true;
        }

        private void ShowSuccess(string message)
        {
            pnlRegisterError.CssClass = "mb-5 p-4 bg-green-50 text-green-500 rounded-md";
            litRegisterError.Text = message;
            pnlRegisterError.Visible = true;
        }
    }
}