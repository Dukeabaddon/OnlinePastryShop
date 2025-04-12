using System;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;
using OnlinePastryShop.App_Code.Utils;

namespace OnlinePastryShop.Pages
{
    public partial class GetSessionTimeout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Disable caching for this page
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            
            // Set response content type to JSON
            Response.ContentType = "application/json";
            
            // Check if user is logged in
            bool isLoggedIn = AuthManager.IsLoggedIn();
            
            if (isLoggedIn)
            {
                // Get the remaining session time
                int secondsRemaining = AuthManager.GetSecondsUntilTimeout();
                int warningTime = AuthManager.GetSessionTimeoutWarning();
                
                // Return response with session info
                var result = new
                {
                    success = true,
                    secondsRemaining = secondsRemaining,
                    warningTime = warningTime,
                    isAuthenticated = true
                };
                
                Response.Write(JsonConvert.SerializeObject(result));
            }
            else
            {
                // Return response indicating session has expired
                var result = new
                {
                    success = false,
                    secondsRemaining = 0,
                    warningTime = 0,
                    isAuthenticated = false,
                    redirectUrl = AuthManager.GetTimedOutRedirectUrl()
                };
                
                Response.Write(JsonConvert.SerializeObject(result));
            }
            
            Response.End();
        }
    }
} 