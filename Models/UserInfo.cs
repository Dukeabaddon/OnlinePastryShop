using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlinePastryShop.App_Code.Models
{
    /// <summary>
    /// Represents user information for authentication and profile management
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Gets or sets the email address
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        public string FirstName { get; set; }
        
        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        public string LastName { get; set; }
        
        /// <summary>
        /// Gets or sets the phone number
        /// </summary>
        public string PhoneNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the user role (e.g., Admin, Customer)
        /// </summary>
        public string Role { get; set; }
        
        /// <summary>
        /// Gets or sets whether the user authenticated through Google
        /// </summary>
        public bool IsGoogleAuth { get; set; }
        
        /// <summary>
        /// Gets or sets the Google ID for OAuth users
        /// </summary>
        public string GoogleId { get; set; }
        
        /// <summary>
        /// Gets or sets the number of failed login attempts
        /// </summary>
        public int FailedLoginAttempts { get; set; }
        
        /// <summary>
        /// Gets or sets the datetime until which the account is locked
        /// </summary>
        public DateTime? LockoutUntil { get; set; }
        
        /// <summary>
        /// Gets or sets the last login date
        /// </summary>
        public DateTime? LastLoginDate { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the email has been verified.
        /// </summary>
        public bool EmailVerified { get; set; }
        
        /// <summary>
        /// Gets or sets the login time for the current session
        /// </summary>
        public DateTime LoginTime { get; set; }
        
        /// <summary>
        /// Gets the full name (first name + last name)
        /// </summary>
        public string FullName
        {
            get { return $"{FirstName} {LastName}".Trim(); }
        }
        
        /// <summary>
        /// Gets user initials for display in the UI
        /// </summary>
        public string Initials
        {
            get
            {
                string initials = "";
                
                if (!string.IsNullOrEmpty(FirstName) && FirstName.Length > 0)
                    initials += FirstName[0];
                
                if (!string.IsNullOrEmpty(LastName) && LastName.Length > 0)
                    initials += LastName[0];
                
                return initials.ToUpper();
            }
        }
        
        /// <summary>
        /// Gets whether the user is an administrator
        /// </summary>
        public bool IsAdmin
        {
            get { return Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true; }
        }
        
        /// <summary>
        /// Gets whether the account is currently locked
        /// </summary>
        public bool IsLocked
        {
            get { return LockoutUntil.HasValue && LockoutUntil.Value > DateTime.Now; }
        }
    }
} 