using System;
using System.Web;
using System.Web.SessionState;

/// <summary>
/// Static utility class for managing user sessions in the application
/// </summary>
public static class SessionManager
{
    /// <summary>
    /// Checks if a user is currently logged in
    /// </summary>
    /// <returns>True if a user is logged in, false otherwise</returns>
    public static bool IsUserLoggedIn()
    {
        return HttpContext.Current.Session["UserID"] != null && 
               HttpContext.Current.Session["Username"] != null;
    }
    
    /// <summary>
    /// Gets the current user's ID
    /// </summary>
    /// <returns>The user ID or null if not logged in</returns>
    public static int? GetUserId()
    {
        if (HttpContext.Current.Session["UserID"] != null)
        {
            return Convert.ToInt32(HttpContext.Current.Session["UserID"]);
        }
        return null;
    }
    
    /// <summary>
    /// Gets the current user's role
    /// </summary>
    /// <returns>The user role or an empty string if not logged in</returns>
    public static string GetUserRole()
    {
        return HttpContext.Current.Session["UserRole"]?.ToString() ?? string.Empty;
    }
    
    /// <summary>
    /// Gets the current user's first name
    /// </summary>
    /// <returns>The user's first name or an empty string if not logged in</returns>
    public static string GetFirstName()
    {
        return HttpContext.Current.Session["FirstName"]?.ToString() ?? string.Empty;
    }
    
    /// <summary>
    /// Gets the current user's last name
    /// </summary>
    /// <returns>The user's last name or an empty string if not logged in</returns>
    public static string GetLastName()
    {
        return HttpContext.Current.Session["LastName"]?.ToString() ?? string.Empty;
    }
    
    /// <summary>
    /// Gets the current user's username
    /// </summary>
    /// <returns>The username or an empty string if not logged in</returns>
    public static string GetUsername()
    {
        return HttpContext.Current.Session["Username"]?.ToString() ?? string.Empty;
    }
    
    /// <summary>
    /// Gets the current user's initials (first letter of first name + first letter of last name)
    /// </summary>
    /// <returns>User initials or a default if not available</returns>
    public static string GetUserInitials()
    {
        string initials = string.Empty;
        string firstName = GetFirstName();
        string lastName = GetLastName();
        
        if (!string.IsNullOrEmpty(firstName) && firstName.Length > 0)
        {
            initials += firstName[0];
        }
        if (!string.IsNullOrEmpty(lastName) && lastName.Length > 0)
        {
            initials += lastName[0];
        }
        
        // If we couldn't get initials from first/last name, try username
        if (string.IsNullOrEmpty(initials))
        {
            string username = GetUsername();
            if (!string.IsNullOrEmpty(username) && username.Length > 0)
            {
                initials = username[0].ToString().ToUpper();
            }
        }
        
        // Ensure we have at least one character
        if (string.IsNullOrEmpty(initials))
        {
            initials = "U";
        }
        
        return initials.ToUpper();
    }
    
    /// <summary>
    /// Stores user information in the session after successful login
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="username">Username</param>
    /// <param name="firstName">First name</param>
    /// <param name="lastName">Last name</param>
    /// <param name="role">User role</param>
    public static void LoginUser(int userId, string username, string firstName, string lastName, string role)
    {
        HttpSessionState session = HttpContext.Current.Session;
        
        session["UserID"] = userId;
        session["Username"] = username;
        session["FirstName"] = firstName;
        session["LastName"] = lastName;
        session["UserRole"] = role;
    }
    
    /// <summary>
    /// Logs out the current user by clearing all session variables
    /// </summary>
    public static void Logout()
    {
        HttpContext context = HttpContext.Current;
        
        try
        {
            // Set cache control headers to prevent caching
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            context.Response.Cache.SetExpires(DateTime.MinValue);
            
            // Add additional cache control headers
            context.Response.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate");
            context.Response.AddHeader("Pragma", "no-cache");
            context.Response.AddHeader("Expires", "0");
            
            // Clear all session variables
            context.Session.Clear();
            context.Session.Abandon();
            
            // Clear session cookie
            if (context.Request.Cookies["ASP.NET_SessionId"] != null)
            {
                HttpCookie sessionCookie = new HttpCookie("ASP.NET_SessionId");
                sessionCookie.Expires = DateTime.Now.AddDays(-1d);
                sessionCookie.Value = string.Empty;
                context.Response.Cookies.Add(sessionCookie);
            }
            
            // Clear authentication cookie if exists
            if (context.Request.Cookies["AuthCookie"] != null)
            {
                HttpCookie authCookie = new HttpCookie("AuthCookie");
                authCookie.Expires = DateTime.Now.AddDays(-1d);
                authCookie.Value = string.Empty;
                context.Response.Cookies.Add(authCookie);
            }
            
            // Force session end by creating and abandoning a new session
            context.Session.RemoveAll();
        }
        catch (Exception ex)
        {
            // Log exception in production
            // For now, ensure session is cleared even if an error occurs
            if (context.Session != null)
            {
                context.Session.Clear();
                context.Session.Abandon();
            }
        }
    }
    
    /// <summary>
    /// Redirects to login page if not logged in, or to specified URL if not Admin
    /// </summary>
    /// <param name="requiredRole">Required role to access the page</param>
    /// <param name="redirectUrl">URL to redirect to if not in required role</param>
    public static void EnsureUserAccess(string requiredRole = null, string redirectUrl = "~/Pages/Login.aspx")
    {
        if (!IsUserLoggedIn())
        {
            HttpContext.Current.Response.Redirect(redirectUrl);
            return;
        }
        
        if (!string.IsNullOrEmpty(requiredRole) && GetUserRole() != requiredRole)
        {
            if (redirectUrl == "~/Pages/Login.aspx")
            {
                redirectUrl = "~/Pages/Default.aspx";
            }
            HttpContext.Current.Response.Redirect(redirectUrl);
        }
    }
} 