using System;
using System.Diagnostics;
using System.IO;

namespace OnlinePastryShop
{
    /// <summary>
    /// Simple logging utility class for the application
    /// </summary>
    public static class Log
    {
        private static string LogFilePath = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/application.log");

        static Log()
        {
            // Ensure the App_Data directory exists
            string logDir = Path.GetDirectoryName(LogFilePath);
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
        }

        /// <summary>
        /// Log debug information
        /// </summary>
        public static void Debug(string message)
        {
            LogMessage("DEBUG", message);
            System.Diagnostics.Debug.WriteLine("DEBUG: " + message);
        }

        /// <summary>
        /// Log error information
        /// </summary>
        public static void Error(string message)
        {
            LogMessage("ERROR", message);
            System.Diagnostics.Debug.WriteLine("ERROR: " + message);
        }

        /// <summary>
        /// Log information
        /// </summary>
        public static void Info(string message)
        {
            LogMessage("INFO", message);
            System.Diagnostics.Debug.WriteLine("INFO: " + message);
        }

        /// <summary>
        /// Log warning information
        /// </summary>
        public static void Warning(string message)
        {
            LogMessage("WARNING", message);
            System.Diagnostics.Debug.WriteLine("WARNING: " + message);
        }

        private static void LogMessage(string level, string message)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logMessage = timestamp + " [" + level + "] " + message;

                // Write to file
                using (StreamWriter writer = File.AppendText(LogFilePath))
                {
                    writer.WriteLine(logMessage);
                }
            }
            catch (Exception ex)
            {
                // If we can't log to file, at least write to debug output
                System.Diagnostics.Debug.WriteLine("Failed to write to log file: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Original message: [" + level + "] " + message);
            }
        }
    }
}