using System;
using System.IO;
using System.Web;

namespace OnlinePastryShop.Classes
{
    public static class ErrorLogger
    {
        private static readonly string LogDirectory = HttpContext.Current != null 
            ? Path.Combine(HttpContext.Current.Server.MapPath("~/"), "Logs") 
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        static ErrorLogger()
        {
            // Ensure log directory exists
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }

        /// <summary>
        /// Logs an error message to the application log file
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="stackTrace">Stack trace (optional)</param>
        public static void LogError(string message, string stackTrace = null)
        {
            try
            {
                string logFilePath = Path.Combine(LogDirectory, $"ErrorLog_{DateTime.Now:yyyyMMdd}.txt");
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}";
                
                if (!string.IsNullOrEmpty(stackTrace))
                {
                    logMessage += Environment.NewLine + "Stack Trace:" + Environment.NewLine + stackTrace;
                }
                
                logMessage += Environment.NewLine + "----------------------------------------" + Environment.NewLine;

                // Append to log file
                File.AppendAllText(logFilePath, logMessage);
            }
            catch
            {
                // If logging fails, we don't want to throw additional exceptions
            }
        }

        /// <summary>
        /// Logs an exception to the application log file
        /// </summary>
        /// <param name="ex">The exception to log</param>
        public static void LogError(Exception ex)
        {
            LogError(ex.Message, ex.StackTrace);
            
            // Log inner exception if it exists
            if (ex.InnerException != null)
            {
                LogError($"Inner Exception: {ex.InnerException.Message}", ex.InnerException.StackTrace);
            }
        }
    }
} 