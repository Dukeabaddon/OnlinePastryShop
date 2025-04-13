using System;
using System.Web;
using System.Web.UI;
using System.Text;

namespace OnlinePastryShop.App_Code
{
    /// <summary>
    /// Represents the different types of notifications
    /// </summary>
    public enum NotificationType
    {
        Success,
        Error,
        Warning,
        Info
    }
    
    /// <summary>
    /// Utility class for displaying toast notifications
    /// </summary>
    public static class ToastNotification
    {
        /// <summary>
        /// Session key for storing notifications
        /// </summary>
        private const string NOTIFICATION_KEY = "ToastNotification";
        
        /// <summary>
        /// Shows a notification message
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <param name="type">The type of notification</param>
        public static void Show(string message, NotificationType type = NotificationType.Info)
        {
            // Create notification object
            var notification = new
            {
                Message = message,
                Type = type.ToString().ToLower()
            };
            
            // Store in session
            HttpContext.Current.Session[NOTIFICATION_KEY] = notification;
        }
        
        /// <summary>
        /// Shows a success notification
        /// </summary>
        /// <param name="message">The message to show</param>
        public static void ShowSuccess(string message)
        {
            Show(message, NotificationType.Success);
        }
        
        /// <summary>
        /// Shows an error notification
        /// </summary>
        /// <param name="message">The message to show</param>
        public static void ShowError(string message)
        {
            Show(message, NotificationType.Error);
        }
        
        /// <summary>
        /// Shows a warning notification
        /// </summary>
        /// <param name="message">The message to show</param>
        public static void ShowWarning(string message)
        {
            Show(message, NotificationType.Warning);
        }
        
        /// <summary>
        /// Shows an info notification
        /// </summary>
        /// <param name="message">The message to show</param>
        public static void ShowInfo(string message)
        {
            Show(message, NotificationType.Info);
        }
        
        /// <summary>
        /// Gets the script to display any pending notifications
        /// </summary>
        /// <returns>JavaScript to display the notification</returns>
        public static string GetNotificationScript()
        {
            // Check if there's a notification to display
            if (HttpContext.Current.Session[NOTIFICATION_KEY] == null)
                return string.Empty;
            
            // Get notification details
            dynamic notification = HttpContext.Current.Session[NOTIFICATION_KEY];
            string message = notification.Message;
            string type = notification.Type;
            
            // Clear the notification from session
            HttpContext.Current.Session.Remove(NOTIFICATION_KEY);
            
            // Build toast notification script
            StringBuilder script = new StringBuilder();
            script.AppendLine("<script>");
            script.AppendLine("document.addEventListener('DOMContentLoaded', function() {");
            script.AppendLine("    function showToast(message, type) {");
            script.AppendLine("        // Create toast container if it doesn't exist");
            script.AppendLine("        let toastContainer = document.getElementById('toast-container');");
            script.AppendLine("        if (!toastContainer) {");
            script.AppendLine("            toastContainer = document.createElement('div');");
            script.AppendLine("            toastContainer.id = 'toast-container';");
            script.AppendLine("            toastContainer.style.position = 'fixed';");
            script.AppendLine("            toastContainer.style.top = '1rem';");
            script.AppendLine("            toastContainer.style.right = '1rem';");
            script.AppendLine("            toastContainer.style.zIndex = '9999';");
            script.AppendLine("            document.body.appendChild(toastContainer);");
            script.AppendLine("        }");
            script.AppendLine("");
            script.AppendLine("        // Create toast element");
            script.AppendLine("        const toast = document.createElement('div');");
            script.AppendLine("        toast.style.minWidth = '250px';");
            script.AppendLine("        toast.style.margin = '0.5rem 0';");
            script.AppendLine("        toast.style.padding = '1rem';");
            script.AppendLine("        toast.style.borderRadius = '0.25rem';");
            script.AppendLine("        toast.style.boxShadow = '0 0.5rem 1rem rgba(0, 0, 0, 0.15)';");
            script.AppendLine("        toast.style.opacity = '0';");
            script.AppendLine("        toast.style.transition = 'opacity 0.3s ease-in-out';");
            script.AppendLine("");
            script.AppendLine("        // Set toast style based on type");
            script.AppendLine("        switch (type) {");
            script.AppendLine("            case 'success':");
            script.AppendLine("                toast.style.backgroundColor = '#96744F';"); // Brand brown for success
            script.AppendLine("                toast.style.color = 'white';");
            script.AppendLine("                break;");
            script.AppendLine("            case 'error':");
            script.AppendLine("                toast.style.backgroundColor = '#dc3545';");
            script.AppendLine("                toast.style.color = 'white';");
            script.AppendLine("                break;");
            script.AppendLine("            case 'warning':");
            script.AppendLine("                toast.style.backgroundColor = '#ffc107';");
            script.AppendLine("                toast.style.color = '#212529';");
            script.AppendLine("                break;");
            script.AppendLine("            case 'info':");
            script.AppendLine("            default:");
            script.AppendLine("                toast.style.backgroundColor = '#0dcaf0';");
            script.AppendLine("                toast.style.color = '#212529';");
            script.AppendLine("                break;");
            script.AppendLine("        }");
            script.AppendLine("");
            script.AppendLine("        // Set toast content");
            script.AppendLine("        toast.textContent = message;");
            script.AppendLine("");
            script.AppendLine("        // Add toast to container");
            script.AppendLine("        toastContainer.appendChild(toast);");
            script.AppendLine("");
            script.AppendLine("        // Trigger reflow and show toast");
            script.AppendLine("        toast.offsetHeight;");
            script.AppendLine("        toast.style.opacity = '1';");
            script.AppendLine("");
            script.AppendLine("        // Auto-dismiss after 3 seconds");
            script.AppendLine("        setTimeout(function () {");
            script.AppendLine("            toast.style.opacity = '0';");
            script.AppendLine("            setTimeout(function () {");
            script.AppendLine("                toastContainer.removeChild(toast);");
            script.AppendLine("            }, 300);");
            script.AppendLine("        }, 3000);");
            script.AppendLine("    }");
            script.AppendLine("");
            script.AppendFormat("    showToast('{0}', '{1}');", message.Replace("'", "\\'"), type);
            script.AppendLine("});");
            script.AppendLine("</script>");
            
            return script.ToString();
        }
        
        /// <summary>
        /// Registers the notification script with a page
        /// </summary>
        /// <param name="page">The page to register the script with</param>
        public static void RegisterNotificationScript(Page page)
        {
            string script = GetNotificationScript();
            if (!string.IsNullOrEmpty(script))
            {
                page.ClientScript.RegisterStartupScript(typeof(Page), "ToastNotification", script);
            }
        }
    }
} 