using System;
using System.Security.Cryptography;
using System.Text;

class HashTest
{
    static void Main()
    {
        string password = "password123";
        string hash = HashPassword(password);
        
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Hash: {hash}");
        
        // Check if this matches the admin hash in the database
        string dbHash = "eef53b3b59d3f5d46b3f78a51ae7454e1b19cca1ae37d03c25e299c34598b23d";
        Console.WriteLine($"DB Hash: {dbHash}");
        Console.WriteLine($"Matches: {hash == dbHash}");
    }
    
    static string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }
            
            return builder.ToString();
        }
    }
} 