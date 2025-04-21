namespace OnlinePastryShop.App_Code.Utils
{
    /// <summary>
    /// Contains constant values for session keys used throughout the application
    /// </summary>
    public static class SessionKeys
    {
        /// <summary>
        /// Key for storing UserInfo object in session
        /// </summary>
        public const string UserInfo = "UserInfo";

        /// <summary>
        /// Key for storing login status in session
        /// </summary>
        public const string IsLoggedIn = "IsLoggedIn";

        /// <summary>
        /// Key for storing the timestamp of last user activity
        /// </summary>
        public const string LastActivity = "LastActivity";

        /// <summary>
        /// Key for storing shopping cart in session
        /// </summary>
        public const string ShoppingCart = "ShoppingCart";

        /// <summary>
        /// Key for storing success messages to be displayed to the user
        /// </summary>
        public const string SuccessMessage = "SuccessMessage";

        /// <summary>
        /// Key for storing error messages to be displayed to the user
        /// </summary>
        public const string ErrorMessage = "ErrorMessage";

        /// <summary>
        /// Key for storing the user's return URL after login
        /// </summary>
        public const string ReturnUrl = "ReturnUrl";
    }
} 