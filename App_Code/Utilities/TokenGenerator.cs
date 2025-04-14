using System;
using System.Security.Cryptography;
using System.Text;

namespace OnlinePastryShop.App_Code.Utilities
{
    /// <summary>
    /// Utility class for generating secure tokens and password hashes
    /// </summary>
    public class TokenGenerator
    {
        /// <summary>
        /// Computes a SHA256 hash of the given input string
        /// </summary>
        /// <param name="input">The string to hash</param>
        /// <returns>The hashed string</returns>
        public static string ComputeHash(string input)
        {
            // Create a SHA256 object
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Generates a random string token of specified length
        /// </summary>
        /// <param name="length">Length of the token</param>
        /// <returns>A random token string</returns>
        public static string GenerateToken(int length = 32)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            byte[] data = new byte[length];

            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }

            StringBuilder result = new StringBuilder(length);
            foreach (byte b in data)
            {
                result.Append(chars[b % chars.Length]);
            }

            return result.ToString();
        }

        /// <summary>
        /// Generates a reset token and its expiration date
        /// </summary>
        /// <param name="token">Output token</param>
        /// <param name="expirationDate">Output expiration date</param>
        /// <param name="hours">Hours until expiration</param>
        public static void GenerateResetToken(out string token, out DateTime expirationDate, int hours = 24)
        {
            token = GenerateToken();
            expirationDate = DateTime.Now.AddHours(hours);
        }
    }
} 