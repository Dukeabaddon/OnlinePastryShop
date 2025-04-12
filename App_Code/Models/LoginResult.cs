namespace OnlinePastryShop.App_Code.Models
{
    /// <summary>
    /// Represents the result of a login attempt
    /// </summary>
    public class LoginResult
    {
        /// <summary>
        /// Indicates if the login was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The message describing the result of the login attempt
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The authenticated user information, if login was successful
        /// </summary>
        public UserInfo User { get; set; }

        /// <summary>
        /// Indicates if email verification is required
        /// </summary>
        public bool EmailVerificationRequired { get; set; }

        /// <summary>
        /// Creates a successful login result with the authenticated user
        /// </summary>
        /// <param name="user">The authenticated user</param>
        /// <returns>A LoginResult instance</returns>
        public static LoginResult SuccessResult(UserInfo user)
        {
            return new LoginResult
            {
                Success = true,
                Message = "Login successful",
                User = user,
                EmailVerificationRequired = false
            };
        }

        /// <summary>
        /// Creates a failed login result with the specified error message
        /// </summary>
        /// <param name="message">The error message</param>
        /// <returns>A LoginResult instance</returns>
        public static LoginResult FailureResult(string message)
        {
            return new LoginResult
            {
                Success = false,
                Message = message,
                User = null,
                EmailVerificationRequired = false
            };
        }

        /// <summary>
        /// Creates a login result indicating email verification is required
        /// </summary>
        /// <param name="user">The user that requires email verification</param>
        /// <returns>A LoginResult instance</returns>
        public static LoginResult EmailVerificationResult(UserInfo user)
        {
            return new LoginResult
            {
                Success = false,
                Message = "Email verification required",
                User = user,
                EmailVerificationRequired = true
            };
        }
    }
} 