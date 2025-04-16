using System;
using System.Web;

/// <summary>
/// Handler class for processing logout requests
/// </summary>
public class LogoutHandler : IHttpHandler
{
    /// <summary>
    /// Processes the logout request by clearing the user session and redirecting to the login page
    /// </summary>
    /// <param name="context">The HTTP context</param>
    public void ProcessRequest(HttpContext context)
    {
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
            
            // Clear all session variables using SessionManager
            SessionManager.Logout();
            
            // Get the redirect URL or default to login page
            string redirectUrl = context.Request.QueryString["returnUrl"] ?? "~/Pages/Login.aspx";
            
            // Add cache-busting parameter to prevent browser caching
            if (redirectUrl.Contains("?"))
            {
                redirectUrl += "&nocache=" + DateTime.Now.Ticks;
            }
            else
            {
                redirectUrl += "?nocache=" + DateTime.Now.Ticks;
            }
            
            // Redirect to specified page
            context.Response.Redirect(redirectUrl, true);
        }
        catch (Exception ex)
        {
            // Log the exception (in a production application)
            
            // Ensure the session is cleared even if an error occurs
            if (context.Session != null)
            {
                context.Session.Clear();
                context.Session.Abandon();
            }
            
            // Add cache-busting parameter
            string redirectUrl = "~/Pages/Login.aspx?nocache=" + DateTime.Now.Ticks;
            
            // Redirect to login page
            context.Response.Redirect(redirectUrl, true);
        }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is reusable
    /// </summary>
    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
} 