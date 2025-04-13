using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace OnlinePastryShop.App_Code.Utils
{
    /// <summary>
    /// Helper class for Google OAuth authentication
    /// </summary>
    public static class GoogleAuthHelper
    {
        // Google OAuth endpoints
        private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
        private const string UserInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
        private const string RevokeTokenEndpoint = "https://oauth2.googleapis.com/revoke";

        // Default scopes for authentication
        private static readonly string[] DefaultScopes = { "openid", "email", "profile" };

        /// <summary>
        /// Generates the URL for the Google authorization page
        /// </summary>
        /// <param name="state">A state value to prevent CSRF attacks</param>
        /// <param name="additionalScopes">Additional OAuth scopes beyond the defaults</param>
        /// <returns>The authorization URL</returns>
        public static string GetAuthorizationUrl(string state, params string[] additionalScopes)
        {
            // Get configuration values
            string clientId = AuthManager.GetGoogleClientId();
            string redirectUri = AuthManager.GetGoogleRedirectUri();
            
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
                throw new InvalidOperationException("Google OAuth is not correctly configured.");

            // Combine default and additional scopes
            var allScopes = new List<string>(DefaultScopes);
            if (additionalScopes != null && additionalScopes.Length > 0)
                allScopes.AddRange(additionalScopes);

            // Build the authorization URL
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(AuthorizationEndpoint);
            urlBuilder.Append("?client_id=").Append(Uri.EscapeDataString(clientId));
            urlBuilder.Append("&redirect_uri=").Append(Uri.EscapeDataString(redirectUri));
            urlBuilder.Append("&response_type=code");
            urlBuilder.Append("&scope=").Append(Uri.EscapeDataString(string.Join(" ", allScopes)));
            urlBuilder.Append("&access_type=offline"); // Get a refresh token
            urlBuilder.Append("&prompt=consent"); // Always show consent screen

            if (!string.IsNullOrEmpty(state))
                urlBuilder.Append("&state=").Append(Uri.EscapeDataString(state));

            return urlBuilder.ToString();
        }

        /// <summary>
        /// Exchanges an authorization code for access and refresh tokens
        /// </summary>
        /// <param name="code">The authorization code from Google</param>
        /// <returns>The token response from Google</returns>
        public static GoogleTokenResponse ExchangeCodeForTokens(string code)
        {
            // Get configuration values
            string clientId = AuthManager.GetGoogleClientId();
            string clientSecret = AuthManager.GetGoogleClientSecret();
            string redirectUri = AuthManager.GetGoogleRedirectUri();
            
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
                throw new InvalidOperationException("Google OAuth is not correctly configured.");

            // Create the token request
            var requestData = new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code"
            };

            // Send the request to Google
            string responseJson = SendPostRequest(TokenEndpoint, requestData);
            
            // Parse the response
            return JsonConvert.DeserializeObject<GoogleTokenResponse>(responseJson);
        }

        /// <summary>
        /// Gets the user's information using the access token
        /// </summary>
        /// <param name="accessToken">The OAuth access token</param>
        /// <returns>The user information response from Google</returns>
        public static GoogleUserInfo GetUserInfo(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentNullException(nameof(accessToken));

            // Send request to Google
            using (var client = new WebClient())
            {
                client.Headers.Add("Authorization", $"Bearer {accessToken}");
                string responseJson = client.DownloadString(UserInfoEndpoint);
                
                // Parse the response
                return JsonConvert.DeserializeObject<GoogleUserInfo>(responseJson);
            }
        }

        /// <summary>
        /// Refreshes an access token using a refresh token
        /// </summary>
        /// <param name="refreshToken">The OAuth refresh token</param>
        /// <returns>The token response from Google</returns>
        public static GoogleTokenResponse RefreshAccessToken(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                throw new ArgumentNullException(nameof(refreshToken));

            // Get configuration values
            string clientId = AuthManager.GetGoogleClientId();
            string clientSecret = AuthManager.GetGoogleClientSecret();
            
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                throw new InvalidOperationException("Google OAuth is not correctly configured.");

            // Create the refresh request
            var requestData = new Dictionary<string, string>
            {
                ["refresh_token"] = refreshToken,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["grant_type"] = "refresh_token"
            };

            // Send the request to Google
            string responseJson = SendPostRequest(TokenEndpoint, requestData);
            
            // Parse the response
            return JsonConvert.DeserializeObject<GoogleTokenResponse>(responseJson);
        }

        /// <summary>
        /// Revokes an access token
        /// </summary>
        /// <param name="token">The token to revoke</param>
        /// <returns>True if revocation was successful; otherwise, false</returns>
        public static bool RevokeToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                // Create the revocation request
                var requestData = new Dictionary<string, string>
                {
                    ["token"] = token
                };

                // Send the request to Google
                SendPostRequest(RevokeTokenEndpoint, requestData);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sends a POST request to a URL with form data
        /// </summary>
        /// <param name="url">The URL to send the request to</param>
        /// <param name="formData">The form data to send</param>
        /// <returns>The response as a string</returns>
        private static string SendPostRequest(string url, Dictionary<string, string> formData)
        {
            // Create the POST data
            var postData = new StringBuilder();
            foreach (var item in formData)
            {
                if (postData.Length > 0)
                    postData.Append('&');
                postData.Append(Uri.EscapeDataString(item.Key));
                postData.Append('=');
                postData.Append(Uri.EscapeDataString(item.Value));
            }

            byte[] postDataBytes = Encoding.UTF8.GetBytes(postData.ToString());

            // Create and configure the web request
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postDataBytes.Length;

            // Send the request
            using (var stream = request.GetRequestStream())
            {
                stream.Write(postDataBytes, 0, postDataBytes.Length);
            }

            // Get the response
            using (var response = (HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }
    }

    /// <summary>
    /// Represents a Google OAuth token response
    /// </summary>
    [Serializable]
    public class GoogleTokenResponse
    {
        /// <summary>
        /// Gets or sets the OAuth access token
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the token expiration time in seconds
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the OAuth refresh token
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Gets or sets the token type (usually "Bearer")
        /// </summary>
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        /// <summary>
        /// Gets or sets the ID token (for OpenID Connect)
        /// </summary>
        [JsonProperty("id_token")]
        public string IdToken { get; set; }

        /// <summary>
        /// Gets the expiration date of the access token
        /// </summary>
        [JsonIgnore]
        public DateTime ExpirationTime => DateTime.UtcNow.AddSeconds(ExpiresIn);
    }

    /// <summary>
    /// Represents Google user information
    /// </summary>
    [Serializable]
    public class GoogleUserInfo
    {
        /// <summary>
        /// Gets or sets the user's ID
        /// </summary>
        [JsonProperty("sub")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the user's name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user's given name (first name)
        /// </summary>
        [JsonProperty("given_name")]
        public string GivenName { get; set; }

        /// <summary>
        /// Gets or sets the user's family name (last name)
        /// </summary>
        [JsonProperty("family_name")]
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets or sets the user's email
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets whether the email is verified
        /// </summary>
        [JsonProperty("email_verified")]
        public bool EmailVerified { get; set; }

        /// <summary>
        /// Gets or sets the user's profile picture URL
        /// </summary>
        [JsonProperty("picture")]
        public string PictureUrl { get; set; }

        /// <summary>
        /// Gets or sets the user's locale
        /// </summary>
        [JsonProperty("locale")]
        public string Locale { get; set; }
    }
} 