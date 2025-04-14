using System;
using System.IO;

namespace OnlinePastryShop.Pages
{
    public partial class Error : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Get the exception from session if it exists
                if (Session["LastError"] != null)
                {
                    Exception ex = (Exception)Session["LastError"];
                    lblErrorDetails.Text = $"Error: {ex.Message}<br/>Stack Trace: {ex.StackTrace}";
                }
                else
                {
                    // If no exception in session, try to get the last error from the log file
                    try
                    {
                        string errorLogPath = Server.MapPath("~/error.txt");
                        if (File.Exists(errorLogPath))
                        {
                            string[] lastErrorLines = File.ReadAllLines(errorLogPath);
                            if (lastErrorLines.Length > 0)
                            {
                                // Get just the most recent error entry (last 10 lines max)
                                int startIndex = Math.Max(0, lastErrorLines.Length - 10);
                                int count = Math.Min(10, lastErrorLines.Length);
                                string[] recentError = new string[count];
                                Array.Copy(lastErrorLines, startIndex, recentError, 0, count);
                                
                                lblErrorDetails.Text = string.Join("<br/>", recentError);
                            }
                            else
                            {
                                lblErrorDetails.Text = "No error details available.";
                            }
                        }
                        else
                        {
                            lblErrorDetails.Text = "Error log not found.";
                        }
                    }
                    catch
                    {
                        lblErrorDetails.Text = "Unable to retrieve error details.";
                    }
                }
            }
        }

        protected void btnShowDetails_Click(object sender, EventArgs e)
        {
            // Toggle visibility of error details
            lblErrorDetails.Visible = !lblErrorDetails.Visible;
            
            // Update button text
            btnShowDetails.Text = lblErrorDetails.Visible ? "Hide Technical Details" : "Show Technical Details";
        }
    }
} 