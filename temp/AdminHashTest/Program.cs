using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        string password = "admin123";
        string hash = HashPassword(password);
        
        string storedHash = "a319cad4db59838d112f5a8acc0fafb49bbdf9fe73c070680130bf44fe705abe";
        
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Generated hash: {hash}");
        Console.WriteLine($"Stored hash: {storedHash}");
        Console.WriteLine($"Match: {hash.Equals(storedHash, StringComparison.OrdinalIgnoreCase)}");
    }

    static string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }
} 