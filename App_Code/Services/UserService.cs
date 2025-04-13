using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using OnlinePastryShop.App_Code.Models;
using OnlinePastryShop.App_Code.Utilities;

/// <summary>
/// Handles all user-related operations including authentication, retrieval, and management
/// </summary>
public class UserService
{
    private string connectionString = ConfigurationManager.ConnectionStrings["PastryConnectionString"].ConnectionString;

    /// <summary>
    /// Validates a user's login credentials
    /// </summary>
    /// <param name="email">The user's email</param>
    /// <param name="password">The user's password</param>
    /// <returns>A UserInfo object if validation succeeds, null otherwise</returns>
    public UserInfo ValidateUser(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            return null;

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("SELECT UserID, Username, Email, FirstName, LastName, [Role], PasswordHash FROM Users WHERE Email = @Email", conn);
            cmd.Parameters.AddWithValue("@Email", email);

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                string storedHash = reader["PasswordHash"].ToString();
                string inputHash = PasswordHasher.ComputeHash(password);

                if (storedHash == inputHash)
                {
                    // Create and return a UserInfo object
                    UserInfo user = new UserInfo
                    {
                        UserId = Convert.ToInt32(reader["UserID"]),
                        Username = reader["Username"].ToString(),
                        Email = reader["Email"].ToString(),
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        Role = reader["Role"].ToString()
                    };

                    return user;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Retrieves a user by their ID
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <returns>A UserInfo object if found, null otherwise</returns>
    public UserInfo GetUserById(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("SELECT UserID, Username, Email, FirstName, LastName, [Role] FROM Users WHERE UserID = @UserID", conn);
            cmd.Parameters.AddWithValue("@UserID", userId);

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                UserInfo user = new UserInfo
                {
                    UserId = Convert.ToInt32(reader["UserID"]),
                    Username = reader["Username"].ToString(),
                    Email = reader["Email"].ToString(),
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    Role = reader["Role"].ToString()
                };

                return user;
            }
        }

        return null;
    }

    /// <summary>
    /// Retrieves a user by their email
    /// </summary>
    /// <param name="email">The user's email</param>
    /// <returns>A UserInfo object if found, null otherwise</returns>
    public UserInfo GetUserByEmail(string email)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            SqlCommand cmd = new SqlCommand("SELECT UserID, Username, Email, FirstName, LastName, [Role] FROM Users WHERE Email = @Email", conn);
            cmd.Parameters.AddWithValue("@Email", email);

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                UserInfo user = new UserInfo
                {
                    UserId = Convert.ToInt32(reader["UserID"]),
                    Username = reader["Username"].ToString(),
                    Email = reader["Email"].ToString(),
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    Role = reader["Role"].ToString()
                };

                return user;
            }
        }

        return null;
    }
} 