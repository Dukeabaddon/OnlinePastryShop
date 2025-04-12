using System;
using System.Text.RegularExpressions;

namespace OnlinePastryShop.App_Code.Utils
{
    /// <summary>
    /// Validates password strength based on configurable rules
    /// </summary>
    public static class PasswordValidator
    {
        // Default validation settings
        private const int MinimumLength = 8;
        private const bool RequireUppercase = true;
        private const bool RequireLowercase = true;
        private const bool RequireDigit = true;
        private const bool RequireSpecialChar = true;

        /// <summary>
        /// Validates a password against predefined strength rules
        /// </summary>
        /// <param name="password">The password to validate</param>
        /// <returns>A validation result indicating success or failure with a message</returns>
        public static PasswordValidationResult Validate(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return new PasswordValidationResult 
                { 
                    IsValid = false, 
                    Message = "Password is required.",
                    StrengthScore = 0
                };
            }

            var result = new PasswordValidationResult
            {
                IsValid = true,
                StrengthScore = 0
            };

            // Check minimum length
            if (password.Length < MinimumLength)
            {
                result.IsValid = false;
                result.Message = $"Password must be at least {MinimumLength} characters long.";
                result.StrengthScore += CalculateLengthScore(password.Length);
                return result;
            }
            else
            {
                result.StrengthScore += CalculateLengthScore(password.Length);
            }

            // Check for uppercase letter
            if (RequireUppercase && !Regex.IsMatch(password, "[A-Z]"))
            {
                result.IsValid = false;
                result.Message = "Password must contain at least one uppercase letter.";
            }
            else if (Regex.IsMatch(password, "[A-Z]"))
            {
                result.StrengthScore += 1;
            }

            // Check for lowercase letter
            if (RequireLowercase && !Regex.IsMatch(password, "[a-z]"))
            {
                result.IsValid = false;
                result.Message = "Password must contain at least one lowercase letter.";
            }
            else if (Regex.IsMatch(password, "[a-z]"))
            {
                result.StrengthScore += 1;
            }

            // Check for digit
            if (RequireDigit && !Regex.IsMatch(password, "[0-9]"))
            {
                result.IsValid = false;
                result.Message = "Password must contain at least one digit.";
            }
            else if (Regex.IsMatch(password, "[0-9]"))
            {
                result.StrengthScore += 1;
            }

            // Check for special character
            if (RequireSpecialChar && !Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            {
                result.IsValid = false;
                result.Message = "Password must contain at least one special character.";
            }
            else if (Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            {
                result.StrengthScore += 1;
            }

            // Check for common patterns
            if (ContainsCommonPattern(password))
            {
                result.StrengthScore -= 1;
            }

            // Set overall strength description
            result.StrengthDescription = GetStrengthDescription(result.StrengthScore);

            // If valid but no message set, add a success message
            if (result.IsValid && string.IsNullOrEmpty(result.Message))
            {
                result.Message = "Password meets all requirements.";
            }

            return result;
        }

        /// <summary>
        /// Calculates a score based on password length
        /// </summary>
        /// <param name="length">The password length</param>
        /// <returns>A score between 0 and 2</returns>
        private static int CalculateLengthScore(int length)
        {
            if (length < MinimumLength)
                return 0;
            if (length < 12)
                return 1;
            return 2;
        }

        /// <summary>
        /// Checks if the password contains common patterns that should be avoided
        /// </summary>
        /// <param name="password">The password to check</param>
        /// <returns>True if a common pattern is found; otherwise, false</returns>
        private static bool ContainsCommonPattern(string password)
        {
            // Check for sequential numbers
            if (Regex.IsMatch(password, "123|234|345|456|567|678|789|890"))
                return true;

            // Check for sequential letters
            if (Regex.IsMatch(password, "abc|bcd|cde|def|efg|fgh|ghi|hij|ijk|jkl|klm|lmn|mno|nop|opq|pqr|qrs|rst|stu|tuv|uvw|vwx|wxy|xyz", RegexOptions.IgnoreCase))
                return true;

            // Check for repeated characters
            if (Regex.IsMatch(password, "([a-zA-Z0-9])\\1\\1"))
                return true;

            return false;
        }

        /// <summary>
        /// Gets a description based on the password strength score
        /// </summary>
        /// <param name="score">The password strength score</param>
        /// <returns>A description of the password strength</returns>
        private static string GetStrengthDescription(int score)
        {
            switch (score)
            {
                case 0:
                case 1:
                    return "Weak";
                case 2:
                case 3:
                    return "Medium";
                case 4:
                    return "Strong";
                case 5:
                    return "Very Strong";
                default:
                    return "Unknown";
            }
        }
    }

    /// <summary>
    /// Represents the result of a password validation
    /// </summary>
    public class PasswordValidationResult
    {
        /// <summary>
        /// Gets or sets whether the password is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets a message describing the validation result
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the strength score of the password (0-5)
        /// </summary>
        public int StrengthScore { get; set; }

        /// <summary>
        /// Gets or sets a description of the password strength
        /// </summary>
        public string StrengthDescription { get; set; }
    }
} 