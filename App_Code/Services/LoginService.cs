using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;
using OnlinePastryShop.App_Code.Models;
using OnlinePastryShop.App_Code.Utilities;

namespace OnlinePastryShop.App_Code.Services
{
    /// <summary>
    /// Service for handling user authentication
    /// </summary>
    public class LoginService
    {
        private readonly DatabaseUtility _dbUtility;

        /// <summary>
        /// Initializes a new instance of the LoginService class
        /// </summary>
        public LoginService()
        {
            _dbUtility = new DatabaseUtility();
        }

        /// <summary>
        /// Authenticates a user with the provided username and password
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <param name="rememberMe">Whether to set a persistent cookie</param>
        /// <returns>A LoginResult object containing the result of the authentication attempt</returns>
        public LoginResult AuthenticateUser(string username, string password, bool rememberMe)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return LoginResult.FailureResult("Username and password are required");
            }

            try
            {
                // Hash the password for secure comparison
                string hashedPassword = TokenGenerator.ComputeHash(password);
                
                // Get user data from the database
                string query = @"
                    SELECT * FROM Users 
                    WHERE (Username = @Username OR Email = @Username) 
                    AND Password = @Password";
                
                SqlParameter[] parameters = {
                    new SqlParameter("@Username", username),
                    new SqlParameter("@Password", hashedPassword)
                };

                DataTable userDataTable = _dbUtility.ExecuteQuery(query, parameters);

                if (userDataTable.Rows.Count == 0)
                {
                    return LoginResult.FailureResult("Invalid username or password");
                }

                DataRow userData = userDataTable.Rows[0];
                bool isEmailVerified = Convert.ToBoolean(userData["IsEmailVerified"]);

                // Check if email is verified
                if (!isEmailVerified)
                {
                    UserInfo userVerification = MapUserFromDataRow(userData);
                    return LoginResult.EmailVerificationResult(userVerification);
                }

                // Create user info object
                UserInfo user = MapUserFromDataRow(userData);

                // Create authentication ticket
                CreateAuthTicket(user, rememberMe);

                return LoginResult.SuccessResult(user);
            }
            catch (Exception ex)
            {
                // Log the exception
                // TODO: Implement proper logging
                return LoginResult.FailureResult("An error occurred during login: " + ex.Message);
            }
        }

        /// <summary>
        /// Creates the authentication ticket and sets the cookie
        /// </summary>
        /// <param name="user">The authenticated user</param>
        /// <param name="rememberMe">Whether to set a persistent cookie</param>
        private void CreateAuthTicket(UserInfo user, bool rememberMe)
        {
            // Create authentication ticket
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1,                              // version
                user.Username,                  // user name
                DateTime.Now,                   // issue time
                DateTime.Now.AddHours(24),      // expiration (24 hours)
                rememberMe,                     // persistent
                user.Role,                      // user data - role
                FormsAuthentication.FormsCookiePath  // cookie path
            );

            // Encrypt the ticket
            string encryptedTicket = FormsAuthentication.Encrypt(ticket);

            // Create the cookie
            HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            
            if (rememberMe)
            {
                authCookie.Expires = ticket.Expiration;
            }

            // Add the cookie to the response
            HttpContext.Current.Response.Cookies.Add(authCookie);
        }

        /// <summary>
        /// Logs out the current user
        /// </summary>
        public void LogoutUser()
        {
            FormsAuthentication.SignOut();
        }

        /// <summary>
        /// Maps a DataRow to a UserInfo object
        /// </summary>
        /// <param name="row">The DataRow containing user data</param>
        /// <returns>A UserInfo object</returns>
        private UserInfo MapUserFromDataRow(DataRow row)
        {
            return new UserInfo
            {
                UserId = Convert.ToInt32(row["UserId"]),
                Username = row["Username"].ToString(),
                Email = row["Email"].ToString(),
                FirstName = row["FirstName"].ToString(),
                LastName = row["LastName"].ToString(),
                Role = row["Role"].ToString()
            };
        }
    }
} 