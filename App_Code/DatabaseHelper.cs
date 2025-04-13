using System;
using System.Data;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace OnlinePastryShop.App_Code
{
    /// <summary>
    /// Helper class for database operations providing singleton access to database utility
    /// </summary>
    public static class DatabaseHelper
    {
        private static readonly Lazy<DatabaseUtility> _instance = new Lazy<DatabaseUtility>(() => new OnlinePastryShop.App_Code.DatabaseUtility());

        /// <summary>
        /// Gets the singleton instance of DatabaseUtility
        /// </summary>
        public static DatabaseUtility Instance => _instance.Value;

        /// <summary>
        /// Executes a query and returns the results as a DataTable
        /// </summary>
        /// <param name="query">The SQL query to execute</param>
        /// <param name="parameters">Optional parameters for the query</param>
        /// <returns>A DataTable containing the query results</returns>
        public static DataTable ExecuteQuery(string query, OracleParameter[] parameters = null)
        {
            return Instance.ExecuteQuery(query, parameters);
        }

        /// <summary>
        /// Executes a non-query command (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="query">The SQL command to execute</param>
        /// <param name="parameters">Optional parameters for the command</param>
        /// <returns>The number of affected rows</returns>
        public static int ExecuteNonQuery(string query, OracleParameter[] parameters = null)
        {
            return Instance.ExecuteNonQuery(query, parameters);
        }

        /// <summary>
        /// Executes a scalar query and returns the first column of the first row
        /// </summary>
        /// <param name="query">The SQL query to execute</param>
        /// <param name="parameters">Optional parameters for the query</param>
        /// <returns>The first column of the first row in the result set</returns>
        public static object ExecuteScalar(string query, OracleParameter[] parameters = null)
        {
            return Instance.ExecuteScalar(query, parameters);
        }
    }
} 