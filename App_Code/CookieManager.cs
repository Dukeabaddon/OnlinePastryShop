using System;
using System.Web;
using System.Security.Cryptography;
using System.Text;

namespace OnlinePastryShop.App_Code
{
    /// <summary>
    /// Manages secure cookie operations for "Remember Me" functionality
    /// </summary>
    public static class CookieManager
    {
        // Cookie names
        private const string AUTH_COOKIE_NAME = "PastryPalace_Auth";
        private const string COOKIE_VERSION = "v1";
        
        // Expiration time (30 days)
        private const int COOKIE_EXPIRATION_DAYS = 30;
        
        /// <summary>
        /// Creates a "Remember Me" authentication cookie
        /// </summary>
        /// <param name="userId">User ID to store</param>
        /// <param name="username">Username to store</param>
        public static void CreateAuthCookie(int userId, string username)
        {
            // Create a secure token
            string token = GenerateSecureToken(userId, username);
            
            // Create the cookie
            HttpCookie authCookie = new HttpCookie(AUTH_COOKIE_NAME);
            authCookie.Value = token;
            authCookie.Expires = DateTime.Now.AddDays(COOKIE_EXPIRATION_DAYS);
            authCookie.HttpOnly = true; // Not accessible via JavaScript
            authCookie.Secure = HttpContext.Current.Request.IsSecureConnection; // Require HTTPS in production
            
            // Add the cookie to the response
            HttpContext.Current.Response.Cookies.Add(authCookie);
        }
        
        /// <summary>
        /// Checks if the authentication cookie exists and is valid
        /// </summary>
        /// <param name="userId">Output parameter that will contain the user ID if successful</param>
        /// <param name="username">Output parameter that will contain the username if successful</param>
        /// <returns>True if a valid auth cookie exists, false otherwise</returns>
        public static bool TryGetAuthCookie(out int userId, out string username)
        {
            userId = 0;
            username = null;
            
            // Check if the cookie exists
            HttpCookie authCookie = HttpContext.Current.Request.Cookies[AUTH_COOKIE_NAME];
            if (authCookie == null || string.IsNullOrEmpty(authCookie.Value))
                return false;
            
            // Parse and validate the token
            return ValidateAndExtractToken(authCookie.Value, out userId, out username);
        }
        
        /// <summary>
        /// Removes the authentication cookie
        /// </summary>
        public static void RemoveAuthCookie()
        {
            HttpCookie authCookie = new HttpCookie(AUTH_COOKIE_NAME);
            authCookie.Expires = DateTime.Now.AddDays(-1); // Expire immediately
            authCookie.Value = string.Empty;
            
            HttpContext.Current.Response.Cookies.Add(authCookie);
        }
        
        /// <summary>
        /// Generates a secure token containing encrypted user information
        /// </summary>
        private static string GenerateSecureToken(int userId, string username)
        {
            // Create the token parts
            string timestamp = DateTime.UtcNow.Ticks.ToString();
            string userPart = $"{userId}:{username}";
            string randomPart = GenerateRandomString(16);
            
            // Combine all parts with version
            string tokenData = $"{COOKIE_VERSION}:{timestamp}:{userPart}:{randomPart}";
            
            // Create signature using HMAC
            string signature = CreateSignature(tokenData);
            
            // Combine data and signature
            return $"{tokenData}:{signature}";
        }
        
        /// <summary>
        /// Validates a token and extracts user information
        /// </summary>
        private static bool ValidateAndExtractToken(string token, out int userId, out string username)
        {
            userId = 0;
            username = null;
            
            try
            {
                // Split token into parts
                string[] parts = token.Split(':');
                
                // Ensure we have enough parts
                if (parts.Length < 5)
                    return false;
                
                // Extract version, timestamp, userId, username, randomPart, and signature
                string version = parts[0];
                string timestamp = parts[1];
                string userIdStr = parts[2];
                username = parts[3];
                string randomPart = parts[4];
                string signature = parts[5];
                
                // Verify version
                if (version != COOKIE_VERSION)
                    return false;
                
                // Reconstruct the data part for signature verification
                string tokenData = $"{version}:{timestamp}:{userIdStr}:{username}:{randomPart}";
                
                // Verify signature
                string expectedSignature = CreateSignature(tokenData);
                if (signature != expectedSignature)
                    return false;
                
                // Parse userId
                if (!int.TryParse(userIdStr, out userId))
                    return false;
                
                // Check token age
                long ticks;
                if (!long.TryParse(timestamp, out ticks))
                    return false;
                
                DateTime tokenTime = new DateTime(ticks, DateTimeKind.Utc);
                TimeSpan age = DateTime.UtcNow - tokenTime;
                
                // Ensure token is not too old (max 30 days)
                if (age.TotalDays > COOKIE_EXPIRATION_DAYS)
                    return false;
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Creates a cryptographic signature for the token data
        /// </summary>
        private static string CreateSignature(string data)
        {
            // In production, this key should be stored securely
            // For example, in Azure Key Vault or as an encrypted app setting
            string key = "PastryPalace_SecureKey_20250412";
            
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashBytes);
            }
        }
        
        /// <summary>
        /// Generates a random string for additional security
        /// </summary>
        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder sb = new StringBuilder(length);
            
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);
                
                for (int i = 0; i < length; i++)
                {
                    int index = randomBytes[i] % chars.Length;
                    sb.Append(chars[index]);
                }
            }
            
            return sb.ToString();
        }
    }
} 