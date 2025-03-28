using System;
using System.Web;

namespace OnlinePastryShop
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            // Code that runs on the first request of a new session
        }

        // Other event handlers can be added here
    }
}