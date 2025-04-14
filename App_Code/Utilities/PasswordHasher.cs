using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Utility class for hashing passwords securely
/// </summary>
public static class PasswordHasher
{
    /// <summary>
    /// Computes a hash for the given password
    /// </summary>
    /// <param name="password">The password to hash</param>
    /// <returns>A hashed representation of the password</returns>
    public static string ComputeHash(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException("password");

        using (SHA256 sha256 = SHA256.Create())
        {
            // Convert the input string to a byte array and compute the hash
            byte[] data = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Create a new StringBuilder to collect the bytes
            // and create a string
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string
            return sBuilder.ToString();
        }
    }

    /// <summary>
    /// Verifies that a password matches a hash
    /// </summary>
    /// <param name="password">The password to verify</param>
    /// <param name="hash">The hash to check against</param>
    /// <returns>True if the password matches the hash, false otherwise</returns>
    public static bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;

        string passwordHash = ComputeHash(password);
        return string.Equals(passwordHash, hash, StringComparison.OrdinalIgnoreCase);
    }
} 