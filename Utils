using System;
using System.Web;
using System.Web.Security;
using System.Configuration;
using System.Security.Claims;
using System.Linq;
using Newtonsoft.Json;
using OnlinePastryShop.App_Code.Models;

namespace OnlinePastryShop.App_Code.Utils
{
    /// <summary>
    /// Manages user authentication and session-related functionality
    /// </summary>
    public static class AuthManager
    {
        // Constants for authentication settings
        private const int MAX_FAILED_ATTEMPTS = 5;
        private const int DEFAULT_LOCKOUT_MINUTES = 30;
        private const int REMEMBER_ME_DAYS = 30;

        // Constants for session management
        private const int SESSION_TIMEOUT_MINUTES = 30;
        private const int SESSION_WARNING_SECONDS = 60;

        /// <summary>
        /// Logs in a user and stores their information in session
        /// </summary>
        /// <param name="userInfo">The user information to store</param>
        /// <param name="rememberMe">Whether to remember the user's login</param>
        public static void Login(UserInfo userInfo, bool rememberMe = false)
        {
            if (userInfo == null)
                throw new ArgumentNullException(nameof(userInfo));

            // Store user information in session
            HttpContext.Current.Session[SessionKeys.UserInfo] = userInfo;
            HttpContext.Current.Session[SessionKeys.IsLoggedIn] = true;
            HttpContext.Current.Session[SessionKeys.LastActivity] = DateTime.Now;

            // Reset failed login attempts if any
            ResetFailedLoginAttempts(userInfo.Username);

            // Create authentication ticket
            if (rememberMe)
            {
                // Set persistent authentication cookie for "Remember Me"
                CreateAuthCookie(userInfo, true);
            }
            else
            {
                // Set session-only authentication cookie
                FormsAuthentication.SetAuthCookie(userInfo.Username, false);
            }
        }

        /// <summary>
        /// Logs in a user with Google OAuth
        /// </summary>
        /// <param name="googleUserId">The Google user ID</param>
        /// <param name="email">The user's email</param>
        /// <param name="firstName">The user's first name</param>
        /// <param name="lastName">The user's last name</param>
        /// <param name="rememberMe">Whether to remember the user's login</param>
        /// <returns>The logged in user's info</returns>
        public static UserInfo GoogleLogin(string googleUserId, string email, string firstName, string lastName, bool rememberMe = false)
        {
            // Check if user exists by Google ID
            var userRepository = new OnlinePastryShop.App_Code.Repositories.UserRepository();
            UserInfo userInfo = userRepository.GetUserByGoogleId(googleUserId);

            if (userInfo == null)
            {
                // Check if email already exists
                userInfo = userRepository.GetUserByEmail(email);
                
                if (userInfo != null)
                {
                    // Update existing user with Google ID
                    userInfo.IsGoogleAuth = true;
                    userInfo.GoogleId = googleUserId;
                    userRepository.UpdateUser(userInfo);
                }
                else
                {
                    // Create new user with Google info
                    userInfo = new UserInfo
                    {
                        Username = email,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        Role = "Customer",
                        IsGoogleAuth = true,
                        GoogleId = googleUserId
                    };

                    // Generate a random password for the user (they'll use Google to login)
                    string randomPassword = Guid.NewGuid().ToString();
                    int userId = userRepository.CreateUserWithGoogleAuth(userInfo, randomPassword, googleUserId);
                    
                    if (userId <= 0)
                        return null;

                    userInfo.UserId = userId;
                }
            }

            // Login the user
            Login(userInfo, rememberMe);
            return userInfo;
        }

        /// <summary>
        /// Logs out the current user
        /// </summary>
        public static void Logout()
        {
            // Clear user-related session variables
            HttpContext.Current.Session[SessionKeys.UserInfo] = null;
            HttpContext.Current.Session[SessionKeys.IsLoggedIn] = false;

            // Sign out from forms authentication
            FormsAuthentication.SignOut();
        }

        /// <summary>
        /// Gets the currently logged-in user information
        /// </summary>
        /// <returns>UserInfo object if user is logged in; otherwise, null</returns>
        public static UserInfo GetCurrentUser()
        {
            // First check session
            UserInfo user = HttpContext.Current.Session[SessionKeys.UserInfo] as UserInfo;
            
            if (user != null)
                return user;

            // If not in session, try to get from authentication cookie
            if (HttpContext.Current.User?.Identity?.IsAuthenticated == true)
            {
                string username = HttpContext.Current.User.Identity.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    // Fetch user from database and store in session
                    var userRepository = new OnlinePastryShop.App_Code.Repositories.UserRepository();
                    user = userRepository.GetUserByUsername(username);
                    
                    if (user != null)
                    {
                        // Restore session
                        HttpContext.Current.Session[SessionKeys.UserInfo] = user;
                        HttpContext.Current.Session[SessionKeys.IsLoggedIn] = true;
                        HttpContext.Current.Session[SessionKeys.LastActivity] = DateTime.Now;
                    }
                }
            }
            
            return user;
        }

        /// <summary>
        /// Gets the current user's role
        /// </summary>
        /// <returns>The user's role, or null if not logged in</returns>
        public static string GetUserRole()
        {
            UserInfo user = GetCurrentUser();
            return user?.Role;
        }

        /// <summary>
        /// Checks if a user is currently logged in
        /// </summary>
        /// <returns>True if a user is logged in; otherwise, false</returns>
        public static bool IsLoggedIn()
        {
            // First check session
            object isLoggedIn = HttpContext.Current.Session[SessionKeys.IsLoggedIn];
            if (isLoggedIn != null && (bool)isLoggedIn)
                return true;

            // If not in session, check authentication cookie
            if (HttpContext.Current.User?.Identity?.IsAuthenticated == true)
            {
                // Restore user in session
                GetCurrentUser();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the current user is an administrator
        /// </summary>
        /// <returns>True if the current user is an admin; otherwise, false</returns>
        public static bool IsAdmin()
        {
            UserInfo user = GetCurrentUser();
            return user != null && user.IsAdmin;
        }

        /// <summary>
        /// Updates the last activity time for the current session
        /// </summary>
        public static void UpdateLastActivity()
        {
            HttpContext.Current.Session[SessionKeys.LastActivity] = DateTime.Now;
        }

        /// <summary>
        /// Sets a success message to be displayed on the next page
        /// </summary>
        /// <param name="message">The success message to display</param>
        public static void SetSuccessMessage(string message)
        {
            HttpContext.Current.Session[SessionKeys.SuccessMessage] = message;
        }

        /// <summary>
        /// Sets an error message to be displayed on the next page
        /// </summary>
        /// <param name="message">The error message to display</param>
        public static void SetErrorMessage(string message)
        {
            HttpContext.Current.Session[SessionKeys.ErrorMessage] = message;
        }

        /// <summary>
        /// Gets and clears the current success message
        /// </summary>
        /// <returns>The current success message, or null if none exists</returns>
        public static string GetAndClearSuccessMessage()
        {
            string message = HttpContext.Current.Session[SessionKeys.SuccessMessage] as string;
            HttpContext.Current.Session[SessionKeys.SuccessMessage] = null;
            return message;
        }

        /// <summary>
        /// Gets and clears the current error message
        /// </summary>
        /// <returns>The current error message, or null if none exists</returns>
        public static string GetAndClearErrorMessage()
        {
            string message = HttpContext.Current.Session[SessionKeys.ErrorMessage] as string;
            HttpContext.Current.Session[SessionKeys.ErrorMessage] = null;
            return message;
        }

        /// <summary>
        /// Handles a failed login attempt for a user
        /// </summary>
        /// <param name="username">The username that failed login</param>
        /// <returns>True if account is now locked; otherwise, false</returns>
        public static bool HandleFailedLoginAttempt(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            var userRepository = new OnlinePastryShop.App_Code.Repositories.UserRepository();
            
            // Increment failed login attempts
            int attempts = userRepository.IncrementFailedLoginAttempts(username);
            
            // Check if account should be locked
            if (attempts >= MAX_FAILED_ATTEMPTS)
            {
                // Lock account for default time
                DateTime lockoutUntil = DateTime.Now.AddMinutes(DEFAULT_LOCKOUT_MINUTES);
                userRepository.LockUserAccount(username, lockoutUntil);
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Checks if a user account is locked
        /// </summary>
        /// <param name="username">The username to check</param>
        /// <returns>True if account is locked; otherwise, false</returns>
        public static bool IsAccountLocked(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            var userRepository = new OnlinePastryShop.App_Code.Repositories.UserRepository();
            return userRepository.IsAccountLocked(username);
        }

        /// <summary>
        /// Gets the remaining lockout time for a user account
        /// </summary>
        /// <param name="username">The username to check</param>
        /// <returns>TimeSpan indicating remaining lockout time, or TimeSpan.Zero if not locked</returns>
        public static TimeSpan GetRemainingLockoutTime(string username)
        {
            if (string.IsNullOrEmpty(username))
                return TimeSpan.Zero;

            var userRepository = new OnlinePastryShop.App_Code.Repositories.UserRepository();
            DateTime? lockoutUntil = userRepository.GetLockoutTime(username);
            
            if (lockoutUntil.HasValue && lockoutUntil.Value > DateTime.Now)
            {
                return lockoutUntil.Value - DateTime.Now;
            }
            
            return TimeSpan.Zero;
        }

        /// <summary>
        /// Resets failed login attempts for a user
        /// </summary>
        /// <param name="username">The username to reset</param>
        public static void ResetFailedLoginAttempts(string username)
        {
            if (string.IsNullOrEmpty(username))
                return;

            var userRepository = new OnlinePastryShop.App_Code.Repositories.UserRepository();
            userRepository.ResetFailedLoginAttempts(username);
        }

        /// <summary>
        /// Creates an authentication cookie for "Remember Me" functionality
        /// </summary>
        /// <param name="userInfo">The user information</param>
        /// <param name="persistent">Whether the cookie should persist</param>
        private static void CreateAuthCookie(UserInfo userInfo, bool persistent)
        {
            // Create authentication ticket
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1,                              // version
                userInfo.Username,              // user name
                DateTime.Now,                   // issue time
                DateTime.Now.AddDays(persistent ? REMEMBER_ME_DAYS : 1), // expiration
                persistent,                     // persistent cookie
                userInfo.Role,                  // user data - role
                FormsAuthentication.FormsCookiePath);  // cookie path

            // Encrypt the ticket
            string encryptedTicket = FormsAuthentication.Encrypt(ticket);

            // Create the cookie
            HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
            {
                HttpOnly = true,
                Secure = HttpContext.Current.Request.IsSecureConnection,
                Path = FormsAuthentication.FormsCookiePath
            };

            if (persistent)
            {
                authCookie.Expires = ticket.Expiration;
            }

            // Add the cookie to the response
            HttpContext.Current.Response.Cookies.Add(authCookie);
        }

        /// <summary>
        /// Gets the Google OAuth client ID from configuration
        /// </summary>
        /// <returns>The Google client ID</returns>
        public static string GetGoogleClientId()
        {
            return ConfigurationManager.AppSettings["GoogleClientId"];
        }

        /// <summary>
        /// Gets the Google OAuth client secret from configuration
        /// </summary>
        /// <returns>The Google client secret</returns>
        public static string GetGoogleClientSecret()
        {
            return ConfigurationManager.AppSettings["GoogleClientSecret"];
        }

        /// <summary>
        /// Gets the Google OAuth redirect URI from configuration
        /// </summary>
        /// <returns>The redirect URI</returns>
        public static string GetGoogleRedirectUri()
        {
            return ConfigurationManager.AppSettings["GoogleRedirectUri"];
        }

        /// <summary>
        /// Sets the session timeout values and initializes the session
        /// </summary>
        public static void SetTimeout()
        {
            HttpContext.Current.Session.Timeout = SESSION_TIMEOUT_MINUTES;
            UpdateLastActivity();
        }

        /// <summary>
        /// Checks if the current session has timed out
        /// </summary>
        /// <returns>True if the session has timed out, false otherwise</returns>
        public static bool CheckSessionTimeout()
        {
            // Get the last activity timestamp
            object lastActivity = HttpContext.Current.Session[SessionKeys.LastActivity];
            if (lastActivity == null)
                return true;

            // Calculate the elapsed time since last activity
            DateTime lastActivityTime = (DateTime)lastActivity;
            TimeSpan elapsed = DateTime.Now - lastActivityTime;

            // Check if the session has timed out
            return elapsed.TotalMinutes >= SESSION_TIMEOUT_MINUTES;
        }

        /// <summary>
        /// Gets the redirect URL for timed-out sessions
        /// </summary>
        /// <returns>The URL to redirect to</returns>
        public static string GetTimedOutRedirectUrl()
        {
            return "~/Pages/Login.aspx?timeout=1";
        }

        /// <summary>
        /// Gets the number of seconds until session timeout
        /// </summary>
        /// <returns>The number of seconds until the session times out</returns>
        public static int GetSecondsUntilTimeout()
        {
            // Get the last activity timestamp
            object lastActivity = HttpContext.Current.Session[SessionKeys.LastActivity];
            if (lastActivity == null)
                return 0;

            // Calculate the elapsed time since last activity
            DateTime lastActivityTime = (DateTime)lastActivity;
            TimeSpan elapsed = DateTime.Now - lastActivityTime;

            // Calculate the remaining time
            int remainingSeconds = (SESSION_TIMEOUT_MINUTES * 60) - (int)elapsed.TotalSeconds;
            return Math.Max(0, remainingSeconds);
        }

        /// <summary>
        /// Gets the session timeout warning time in seconds
        /// </summary>
        /// <returns>The warning time in seconds</returns>
        public static int GetSessionTimeoutWarning()
        {
            return SESSION_WARNING_SECONDS;
        }

        /// <summary>
        /// Refreshes the session timeout by updating the last activity time
        /// </summary>
        public static void RefreshSessionTimeout()
        {
            UpdateLastActivity();
        }

        /// <summary>
        /// Clears all notification messages from the session
        /// </summary>
        public static void ClearAllMessages()
        {
            HttpContext.Current.Session[SessionKeys.SuccessMessage] = null;
            HttpContext.Current.Session[SessionKeys.ErrorMessage] = null;
        }

        /// <summary>
        /// Validates whether the provided string is in a valid email format
        /// </summary>
        /// <param name="email">The email to validate</param>
        /// <returns>True if the email is in a valid format, false otherwise</returns>
        public static bool IsValidEmailFormat(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates whether the provided string is in a valid username format
        /// </summary>
        /// <param name="username">The username to validate</param>
        /// <returns>True if the username is in a valid format, false otherwise</returns>
        public static bool IsValidUsernameFormat(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            // Username should be 3-20 characters and contain only letters, numbers, underscores, and hyphens
            return System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_-]{3,20}$");
        }

        /// <summary>
        /// Validates whether the provided string meets password strength requirements
        /// </summary>
        /// <param name="password">The password to validate</param>
        /// <returns>True if the password meets strength requirements, false otherwise</returns>
        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Password must be at least 8 characters long
            if (password.Length < 8)
                return false;

            // Check for at least one uppercase letter
            if (!password.Any(char.IsUpper))
                return false;

            // Check for at least one lowercase letter
            if (!password.Any(char.IsLower))
                return false;

            // Check for at least one digit
            if (!password.Any(char.IsDigit))
                return false;

            // Check for at least one special character
            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                return false;

            return true;
        }

        /// <summary>
        /// Ensures that the current user is authenticated, redirecting to login if not
        /// </summary>
        /// <returns>True if the user is authenticated, false if redirection occurred</returns>
        public static bool EnsureUserAuthenticated()
        {
            if (!IsLoggedIn())
            {
                RedirectUnauthenticatedUser();
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Redirects an unauthenticated user to the login page
        /// </summary>
        public static void RedirectUnauthenticatedUser()
        {
            // Store the current URL as the return URL
            string returnUrl = HttpContext.Current.Request.Url.PathAndQuery;
            HttpContext.Current.Session[SessionKeys.ReturnUrl] = returnUrl;

            // Redirect to login page
            HttpContext.Current.Response.Redirect("~/Pages/Login.aspx");
        }

        /// <summary>
        /// Checks if the current user has the specified role
        /// </summary>
        /// <param name="role">The role to check</param>
        /// <returns>True if the user has the specified role, false otherwise</returns>
        public static bool IsUserInRole(string role)
        {
            UserInfo user = GetCurrentUser();
            return user != null && user.Role.Equals(role, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates a JSON response for AJAX authentication requests
        /// </summary>
        /// <param name="success">Whether the request was successful</param>
        /// <param name="message">A message to include in the response</param>
        /// <param name="redirectUrl">A URL to redirect to</param>
        /// <returns>A JSON string containing the response data</returns>
        public static string CreateJsonResponse(bool success, string message, string redirectUrl = null)
        {
            var response = new 
            {
                Success = success,
                Message = message,
                RedirectUrl = redirectUrl
            };

            return JsonConvert.SerializeObject(response);
        }

        /// <summary>
        /// Generates a CSRF token for form submission protection
        /// </summary>
        /// <returns>A unique token string</returns>
        public static string GenerateAntiForgeryToken()
        {
            string token = Guid.NewGuid().ToString();
            HttpContext.Current.Session["AntiForgeryToken"] = token;
            return token;
        }

        /// <summary>
        /// Validates that the provided token matches the stored anti-forgery token
        /// </summary>
        /// <param name="token">The token to validate</param>
        /// <returns>True if the token is valid, false otherwise</returns>
        public static bool ValidateAntiForgeryToken(string token)
        {
            string storedToken = HttpContext.Current.Session["AntiForgeryToken"] as string;
            return !string.IsNullOrEmpty(storedToken) && storedToken == token;
        }

        /// <summary>
        /// Gets the user's permissions based on their role
        /// </summary>
        /// <returns>An array of permission strings the user has access to</returns>
        public static string[] GetUserPermissions()
        {
            UserInfo user = GetCurrentUser();
            if (user == null)
                return new string[0];

            // Define permissions based on role
            if (user.IsAdmin)
            {
                // Admin has all permissions
                return new string[] {
                    "view_dashboard",
                    "manage_users",
                    "manage_products",
                    "manage_orders",
                    "manage_categories",
                    "view_reports",
                    "edit_site_settings",
                    "manage_inventory",
                    "process_payments",
                    "view_customer_data"
                };
            }
            else
            {
                // Regular customer permissions
                return new string[] {
                    "view_products",
                    "place_orders",
                    "view_order_history",
                    "edit_profile",
                    "manage_cart",
                    "write_reviews"
                };
            }
        }

        /// <summary>
        /// Checks if the current user has a specific permission
        /// </summary>
        /// <param name="permission">The permission to check for</param>
        /// <returns>True if the user has the specified permission, false otherwise</returns>
        public static bool HasPermission(string permission)
        {
            if (string.IsNullOrEmpty(permission))
                return false;

            string[] userPermissions = GetUserPermissions();
            return Array.Exists(userPermissions, p => p.Equals(permission, StringComparison.OrdinalIgnoreCase));
        }
    }
} 