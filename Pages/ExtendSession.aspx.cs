using System;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;
using OnlinePastryShop.App_Code.Utils;

namespace OnlinePastryShop.Pages
{
    public partial class ExtendSession : System.Web.UI.Page
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
                // Extend the session
                AuthManager.RefreshSessionTimeout();
                
                // Return success response
                var result = new
                {
                    success = true,
                    message = "Session extended successfully",
                    secondsRemaining = AuthManager.GetSecondsUntilTimeout()
                };
                
                Response.Write(JsonConvert.SerializeObject(result));
            }
            else
            {
                // Return failure response
                var result = new
                {
                    success = false,
                    message = "Session expired",
                    redirectUrl = AuthManager.GetTimedOutRedirectUrl()
                };
                
                Response.Write(JsonConvert.SerializeObject(result));
            }
            
            Response.End();
        }
    }
} 