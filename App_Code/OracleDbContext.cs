using System;
using System.Configuration;

namespace OnlinePastryShop.App_Code
{
    public static class OracleDbContext
    {
        private static string _connectionString;

        /// <summary>
        /// Gets the Oracle connection string from web.config
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    _connectionString = ConfigurationManager.ConnectionStrings["OracleDbContext"].ConnectionString;
                }
                return _connectionString;
            }
        }
    }
} 