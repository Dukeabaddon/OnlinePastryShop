using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using OnlinePastryShop.App_Code.Models;
using OnlinePastryShop.App_Code.Utilities;

namespace OnlinePastryShop.App_Code.Repositories
{
    /// <summary>
    /// Repository for user-related data operations
    /// </summary>
    public class UserRepository
    {
        private readonly DatabaseUtility _dbUtility = new DatabaseUtility();

        /// <summary>
        /// Gets a user by username
        /// </summary>
        /// <param name="username">The username to search for</param>
        /// <returns>UserInfo if found; otherwise, null</returns>
        public UserInfo GetUserByUsername(string username)
        {
            string sql = "SELECT * FROM Users WHERE Username = @Username";
            SqlParameter[] parameters = { new SqlParameter("@Username", username) };

            DataTable dataTable = _dbUtility.ExecuteQuery(sql, parameters);
            
            if (dataTable.Rows.Count == 0)
                return null;

            DataRow row = dataTable.Rows[0];
            return MapRowToUserInfo(row);
        }

        /// <summary>
        /// Gets a user by email
        /// </summary>
        /// <param name="email">The email to search for</param>
        /// <returns>UserInfo if found; otherwise, null</returns>
        public UserInfo GetUserByEmail(string email)
        {
            string sql = "SELECT * FROM Users WHERE Email = @Email";
            SqlParameter[] parameters = { new SqlParameter("@Email", email) };

            DataTable dataTable = _dbUtility.ExecuteQuery(sql, parameters);
            
            if (dataTable.Rows.Count == 0)
                return null;

            DataRow row = dataTable.Rows[0];
            return MapRowToUserInfo(row);
        }

        /// <summary>
        /// Gets a user by Google ID
        /// </summary>
        /// <param name="googleId">The Google ID to search for</param>
        /// <returns>UserInfo if found; otherwise, null</returns>
        public UserInfo GetUserByGoogleId(string googleId)
        {
            string sql = "SELECT * FROM Users WHERE GoogleId = @GoogleId";
            SqlParameter[] parameters = { new SqlParameter("@GoogleId", googleId) };

            DataTable dataTable = _dbUtility.ExecuteQuery(sql, parameters);
            
            if (dataTable.Rows.Count == 0)
                return null;

            DataRow row = dataTable.Rows[0];
            return MapRowToUserInfo(row);
        }

        /// <summary>
        /// Gets a user by ID
        /// </summary>
        /// <param name="userId">The user ID to search for</param>
        /// <returns>UserInfo if found; otherwise, null</returns>
        public UserInfo GetUserById(int userId)
        {
            string sql = "SELECT * FROM Users WHERE UserId = @UserId";
            SqlParameter[] parameters = { new SqlParameter("@UserId", userId) };

            DataTable dataTable = _dbUtility.ExecuteQuery(sql, parameters);
            
            if (dataTable.Rows.Count == 0)
                return null;

            DataRow row = dataTable.Rows[0];
            return MapRowToUserInfo(row);
        }

        /// <summary>
        /// Authenticates a user by username and password
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <returns>UserInfo if authentication successful; otherwise, null</returns>
        public UserInfo AuthenticateUser(string username, string password)
        {
            UserInfo user = GetUserByUsername(username);
            
            if (user == null)
                return null;
                
            // Check if account is locked
            if (user.IsLocked)
                return null;

            string hashedPassword = HashPassword(password);
            
            string sql = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password";
            SqlParameter[] parameters = 
            {
                new SqlParameter("@Username", username),
                new SqlParameter("@Password", hashedPassword)
            };

            int count = Convert.ToInt32(_dbUtility.ExecuteScalar(sql, parameters));
            
            if (count > 0)
            {
                // Update last login date
                UpdateLastLoginDate(user.UserId);
                return user;
            }
            
            return null;
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="userInfo">The user information</param>
        /// <param name="password">The password</param>
        /// <returns>The new user ID if successful; otherwise, -1</returns>
        public int CreateUser(UserInfo userInfo, string password)
        {
            // Check if username or email already exists
            if (GetUserByUsername(userInfo.Username) != null)
                return -1; // Username already exists

            if (GetUserByEmail(userInfo.Email) != null)
                return -1; // Email already exists

            string hashedPassword = HashPassword(password);
            
            string sql = @"
                INSERT INTO Users (Username, Password, Email, FirstName, LastName, PhoneNumber, Role, FailedLoginAttempts)
                VALUES (@Username, @Password, @Email, @FirstName, @LastName, @PhoneNumber, @Role, 0);
                SELECT SCOPE_IDENTITY();";
            
            SqlParameter[] parameters =
            {
                new SqlParameter("@Username", userInfo.Username),
                new SqlParameter("@Password", hashedPassword),
                new SqlParameter("@Email", userInfo.Email),
                new SqlParameter("@FirstName", userInfo.FirstName),
                new SqlParameter("@LastName", userInfo.LastName),
                new SqlParameter("@PhoneNumber", userInfo.PhoneNumber),
                new SqlParameter("@Role", userInfo.Role ?? "Customer")
            };

            object result = _dbUtility.ExecuteScalar(sql, parameters);
            
            return result != null ? Convert.ToInt32(result) : -1;
        }

        /// <summary>
        /// Creates a new user with Google authentication
        /// </summary>
        /// <param name="userInfo">The user information</param>
        /// <param name="password">A backup password</param>
        /// <param name="googleId">The Google ID</param>
        /// <returns>The new user ID if successful; otherwise, -1</returns>
        public int CreateUserWithGoogleAuth(UserInfo userInfo, string password, string googleId)
        {
            // Check if username or email already exists
            if (GetUserByUsername(userInfo.Username) != null)
                return -1; // Username already exists

            if (GetUserByEmail(userInfo.Email) != null)
                return -1; // Email already exists

            if (GetUserByGoogleId(googleId) != null)
                return -1; // Google ID already exists

            string hashedPassword = HashPassword(password);
            
            string sql = @"
                INSERT INTO Users (Username, Password, Email, FirstName, LastName, PhoneNumber, Role, 
                                   IsGoogleAuth, GoogleId, FailedLoginAttempts, LastLoginDate)
                VALUES (@Username, @Password, @Email, @FirstName, @LastName, @PhoneNumber, @Role, 
                        @IsGoogleAuth, @GoogleId, 0, @LastLoginDate);
                SELECT SCOPE_IDENTITY();";
            
            SqlParameter[] parameters =
            {
                new SqlParameter("@Username", userInfo.Username),
                new SqlParameter("@Password", hashedPassword),
                new SqlParameter("@Email", userInfo.Email),
                new SqlParameter("@FirstName", userInfo.FirstName),
                new SqlParameter("@LastName", userInfo.LastName),
                new SqlParameter("@PhoneNumber", userInfo.PhoneNumber),
                new SqlParameter("@Role", userInfo.Role ?? "Customer"),
                new SqlParameter("@IsGoogleAuth", true),
                new SqlParameter("@GoogleId", googleId),
                new SqlParameter("@LastLoginDate", DateTime.Now)
            };

            object result = _dbUtility.ExecuteScalar(sql, parameters);
            
            return result != null ? Convert.ToInt32(result) : -1;
        }

        /// <summary>
        /// Updates an existing user
        /// </summary>
        /// <param name="userInfo">The updated user information</param>
        /// <returns>True if update successful; otherwise, false</returns>
        public bool UpdateUser(UserInfo userInfo)
        {
            string sql = @"
                UPDATE Users
                SET Username = @Username,
                    Email = @Email,
                    FirstName = @FirstName,
                    LastName = @LastName,
                    PhoneNumber = @PhoneNumber,
                    Role = @Role,
                    IsGoogleAuth = @IsGoogleAuth,
                    GoogleId = @GoogleId
                WHERE UserId = @UserId";
            
            SqlParameter[] parameters =
            {
                new SqlParameter("@UserId", userInfo.UserId),
                new SqlParameter("@Username", userInfo.Username),
                new SqlParameter("@Email", userInfo.Email),
                new SqlParameter("@FirstName", userInfo.FirstName),
                new SqlParameter("@LastName", userInfo.LastName),
                new SqlParameter("@PhoneNumber", userInfo.PhoneNumber),
                new SqlParameter("@Role", userInfo.Role),
                new SqlParameter("@IsGoogleAuth", userInfo.IsGoogleAuth),
                new SqlParameter("@GoogleId", userInfo.GoogleId)
            };

            int rowsAffected = _dbUtility.ExecuteNonQuery(sql, parameters);
            
            return rowsAffected > 0;
        }

        /// <summary>
        /// Changes the password for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="newPassword">The new password</param>
        /// <returns>True if password change successful; otherwise, false</returns>
        public bool ChangePassword(int userId, string newPassword)
        {
            string hashedPassword = HashPassword(newPassword);

            string sql = @"
                UPDATE Users
                SET Password = @Password
                WHERE UserId = @UserId";
            
            SqlParameter[] parameters =
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@Password", hashedPassword)
            };

            int rowsAffected = _dbUtility.ExecuteNonQuery(sql, parameters);
            
            return rowsAffected > 0;
        }

        /// <summary>
        /// Deletes a user by ID
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>True if deletion successful; otherwise, false</returns>
        public bool DeleteUser(int userId)
        {
            string sql = "DELETE FROM Users WHERE UserId = @UserId";
            SqlParameter[] parameters = { new SqlParameter("@UserId", userId) };

            int rowsAffected = _dbUtility.ExecuteNonQuery(sql, parameters);
            
            return rowsAffected > 0;
        }

        /// <summary>
        /// Updates the last login date for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>True if update successful; otherwise, false</returns>
        public bool UpdateLastLoginDate(int userId)
        {
            string sql = @"
                UPDATE Users
                SET LastLoginDate = @LastLoginDate
                WHERE UserId = @UserId";
            
            SqlParameter[] parameters =
            {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@LastLoginDate", DateTime.Now)
            };

            int rowsAffected = _dbUtility.ExecuteNonQuery(sql, parameters);
            
            return rowsAffected > 0;
        }

        /// <summary>
        /// Increments the failed login attempts for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns>The new number of failed attempts</returns>
        public int IncrementFailedLoginAttempts(string username)
        {
            string sql = @"
                UPDATE Users
                SET FailedLoginAttempts = FailedLoginAttempts + 1
                WHERE Username = @Username;
                SELECT FailedLoginAttempts FROM Users WHERE Username = @Username;";

            SqlParameter[] parameters = { new SqlParameter("@Username", username) };

            object result = _dbUtility.ExecuteScalar(sql, parameters);

            return result != null ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// Resets the failed login attempts for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns>True if reset successful; otherwise, false</returns>
        public bool ResetFailedLoginAttempts(string username)
        {
            string sql = @"
                UPDATE Users
                SET FailedLoginAttempts = 0,
                    LockoutUntil = NULL
                WHERE Username = @Username";
            
            SqlParameter[] parameters = { new SqlParameter("@Username", username) };

            int rowsAffected = _dbUtility.ExecuteNonQuery(sql, parameters);
            
            return rowsAffected > 0;
        }

        /// <summary>
        /// Locks a user account until the specified time
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="lockoutUntil">The time until which the account is locked</param>
        /// <returns>True if lock successful; otherwise, false</returns>
        public bool LockUserAccount(string username, DateTime lockoutUntil)
        {
            string sql = @"
                UPDATE Users
                SET LockoutUntil = @LockoutUntil
                WHERE Username = @Username";
            
            SqlParameter[] parameters =
            {
                new SqlParameter("@Username", username),
                new SqlParameter("@LockoutUntil", lockoutUntil)
            };

            int rowsAffected = _dbUtility.ExecuteNonQuery(sql, parameters);
            
            return rowsAffected > 0;
        }

        /// <summary>
        /// Checks if a user account is currently locked
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns>True if account is locked; otherwise, false</returns>
        public bool IsAccountLocked(string username)
        {
            string sql = @"
                SELECT LockoutUntil
                FROM Users
                WHERE Username = @Username";
            
            SqlParameter[] parameters = { new SqlParameter("@Username", username) };

            object result = _dbUtility.ExecuteScalar(sql, parameters);
            
            if (result != null && result != DBNull.Value)
            {
                DateTime lockoutTime = Convert.ToDateTime(result);
                return lockoutTime > DateTime.Now;
            }
            
            return false;
        }

        /// <summary>
        /// Gets the lockout end time for a user account
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns>The lockout end time if locked; otherwise, null</returns>
        public DateTime? GetLockoutTime(string username)
        {
            string sql = @"
                SELECT LockoutUntil
                FROM Users
                WHERE Username = @Username";
            
            SqlParameter[] parameters = { new SqlParameter("@Username", username) };

            object result = _dbUtility.ExecuteScalar(sql, parameters);
            
            if (result != null && result != DBNull.Value)
            {
                return Convert.ToDateTime(result);
            }
            
            return null;
        }

        /// <summary>
        /// Maps a DataRow to a UserInfo object
        /// </summary>
        /// <param name="row">The DataRow containing user data</param>
        /// <returns>A populated UserInfo object</returns>
        private UserInfo MapRowToUserInfo(DataRow row)
        {
            var userInfo = new UserInfo
            {
                UserId = Convert.ToInt32(row["UserId"]),
                Username = row["Username"].ToString(),
                Email = row["Email"].ToString(),
                FirstName = row["FirstName"].ToString(),
                LastName = row["LastName"].ToString(),
                Role = row["Role"].ToString()
            };

            // Handle optional fields that might not exist in older database schemas
            if (row.Table.Columns.Contains("PhoneNumber"))
                userInfo.PhoneNumber = row["PhoneNumber"] != DBNull.Value ? row["PhoneNumber"].ToString() : null;

            if (row.Table.Columns.Contains("IsGoogleAuth"))
                userInfo.IsGoogleAuth = row["IsGoogleAuth"] != DBNull.Value && Convert.ToBoolean(row["IsGoogleAuth"]);

            if (row.Table.Columns.Contains("GoogleId"))
                userInfo.GoogleId = row["GoogleId"] != DBNull.Value ? row["GoogleId"].ToString() : null;

            if (row.Table.Columns.Contains("FailedLoginAttempts"))
                userInfo.FailedLoginAttempts = row["FailedLoginAttempts"] != DBNull.Value ? Convert.ToInt32(row["FailedLoginAttempts"]) : 0;

            if (row.Table.Columns.Contains("LockoutUntil"))
                userInfo.LockoutUntil = row["LockoutUntil"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["LockoutUntil"]) : null;

            if (row.Table.Columns.Contains("LastLoginDate"))
                userInfo.LastLoginDate = row["LastLoginDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["LastLoginDate"]) : null;

            return userInfo;
        }

        /// <summary>
        /// Hashes a password using SHA256
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>The hashed password</returns>
        private string HashPassword(string password)
        {
            return TokenGenerator.ComputeHash(password);
        }
    }
} 