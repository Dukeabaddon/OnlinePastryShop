using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

class TestConnection
{
    static void Main()
    {
        try
        {
            // Connection string from Web.config
            string connectionString = "User Id=Aaron_IPT;Password=qwen123;Data Source=localhost:1521/xe;";
            
            Console.WriteLine("Attempting to connect to the database...");
            
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connection successful!");
                
                // Try to retrieve users
                using (OracleCommand command = new OracleCommand("SELECT COUNT(*) FROM \"AARON_IPT\".\"USERS\"", connection))
                {
                    int userCount = Convert.ToInt32(command.ExecuteScalar());
                    Console.WriteLine($"User count: {userCount}");
                }
                
                // List all tables in the schema
                using (OracleCommand command = new OracleCommand(
                    "SELECT table_name FROM all_tables WHERE owner = 'AARON_IPT'", connection))
                {
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        Console.WriteLine("Tables in AARON_IPT schema:");
                        while (reader.Read())
                        {
                            Console.WriteLine($"- {reader["TABLE_NAME"]}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
} 