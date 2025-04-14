using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace OnlinePastryShop.App_Code.Utils
{
    /// <summary>
    /// Helper class for database operations
    /// </summary>
    public static class DatabaseHelper
    {
        /// <summary>
        /// Gets the connection string from web.config
        /// </summary>
        /// <returns>Database connection string</returns>
        public static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["PastryShopConnectionString"].ConnectionString;
        }

        /// <summary>
        /// Creates and returns a new SqlConnection object using the configured connection string
        /// </summary>
        /// <returns>A new SqlConnection object</returns>
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(GetConnectionString());
        }

        /// <summary>
        /// Executes a non-query SQL command
        /// </summary>
        /// <param name="commandText">The SQL command text</param>
        /// <param name="parameters">Optional SQL parameters</param>
        /// <returns>Number of rows affected</returns>
        public static int ExecuteNonQuery(string commandText, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = GetConnection())
            {
                using (SqlCommand command = new SqlCommand(commandText, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Executes a SQL command and returns the first column of the first row
        /// </summary>
        /// <param name="commandText">The SQL command text</param>
        /// <param name="parameters">Optional SQL parameters</param>
        /// <returns>The first column of the first row, or null if no rows</returns>
        public static object ExecuteScalar(string commandText, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = GetConnection())
            {
                using (SqlCommand command = new SqlCommand(commandText, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    return command.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Executes a SQL command and returns a DataTable
        /// </summary>
        /// <param name="commandText">The SQL command text</param>
        /// <param name="parameters">Optional SQL parameters</param>
        /// <returns>A DataTable containing the results</returns>
        public static DataTable ExecuteDataTable(string commandText, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = GetConnection())
            {
                using (SqlCommand command = new SqlCommand(commandText, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        /// <summary>
        /// Executes a SQL command and returns a DataSet
        /// </summary>
        /// <param name="commandText">The SQL command text</param>
        /// <param name="parameters">Optional SQL parameters</param>
        /// <returns>A DataSet containing the results</returns>
        public static DataSet ExecuteDataSet(string commandText, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = GetConnection())
            {
                using (SqlCommand command = new SqlCommand(commandText, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataSet dataSet = new DataSet();
                        adapter.Fill(dataSet);
                        return dataSet;
                    }
                }
            }
        }

        /// <summary>
        /// Executes a SQL command and processes each row with the provided action
        /// </summary>
        /// <param name="commandText">The SQL command text</param>
        /// <param name="rowAction">Action to perform on each row</param>
        /// <param name="parameters">Optional SQL parameters</param>
        public static void ExecuteReader(string commandText, Action<SqlDataReader> rowAction, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = GetConnection())
            {
                using (SqlCommand command = new SqlCommand(commandText, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rowAction(reader);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a SqlParameter object
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <returns>A new SqlParameter object</returns>
        public static SqlParameter CreateParameter(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }
    }
} 