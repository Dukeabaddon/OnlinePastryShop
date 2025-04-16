<%@ WebHandler Language="C#" Class="LogoutHandler" %>

using System;
using System.Web;

/// <summary>
/// Handles user logout requests by clearing session data and redirecting to login page
/// </summary>
public class LogoutHandler : IHttpHandler 
{
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
            
            // Clear all session variables
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
 
    public bool IsReusable 
    {
        get 
        {
            return false;
        }
    }
} 