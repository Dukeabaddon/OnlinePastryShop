using System;
using System.Data;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace OnlinePastryShop.App_Code
{
    /// <summary>
    /// Utility class for database operations (Root version)
    /// </summary>
    public class DatabaseUtility
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the DatabaseUtility class
        /// </summary>
        public DatabaseUtility()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString;
        }

        /// <summary>
        /// Executes a query and returns the results as a DataTable
        /// </summary>
        /// <param name="query">The SQL query to execute</param>
        /// <param name="parameters">Optional parameters for the query</param>
        /// <returns>A DataTable containing the query results</returns>
        public DataTable ExecuteQuery(string query, OracleParameter[] parameters = null)
        {
            DataTable dataTable = new DataTable();

            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    try
                    {
                        connection.Open();
                        using (OracleDataAdapter adapter = new OracleDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the exception
                        System.Diagnostics.Debug.WriteLine("Database error: " + ex.Message);
                        throw;
                    }
                }
            }

            return dataTable;
        }

        /// <summary>
        /// Executes a non-query command (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="query">The SQL command to execute</param>
        /// <param name="parameters">Optional parameters for the command</param>
        /// <returns>The number of affected rows</returns>
        public int ExecuteNonQuery(string query, OracleParameter[] parameters = null)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    try
                    {
                        connection.Open();
                        return command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        // Log the exception
                        System.Diagnostics.Debug.WriteLine("Database error: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Executes a scalar query and returns the first column of the first row
        /// </summary>
        /// <param name="query">The SQL query to execute</param>
        /// <param name="parameters">Optional parameters for the query</param>
        /// <returns>The first column of the first row in the result set</returns>
        public object ExecuteScalar(string query, OracleParameter[] parameters = null)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    try
                    {
                        connection.Open();
                        return command.ExecuteScalar();
                    }
                    catch (Exception ex)
                    {
                        // Log the exception
                        System.Diagnostics.Debug.WriteLine("Database error: " + ex.Message);
                        throw;
                    }
                }
            }
        }
    }
} 