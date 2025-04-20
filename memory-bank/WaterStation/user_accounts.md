# User Accounts

## Authentication Approach
- Password Storage: SHA-256 hashing
- Password Column: VARCHAR2(64) in the USERS table
- Authentication Method: Form-based authentication with database validation

## Admin User
```sql
INSERT INTO USERS (
    username, 
    password, 
    email, 
    first_name, 
    last_name, 
    user_type, 
    address, 
    phone, 
    created_date, 
    status
) VALUES (
    'zen', 
    '5498bc226f9a5fff79e2083034dc5995ae75e7c285078c911553af746e92abcb', -- SHA-256 hash of 'qwen123'
    'admin@waterstation.com', 
    'Gennie', 
    'Begonia', 
    'admin', 
    'Admin Office, Water Station HQ', 
    '555-0100', 
    SYSTIMESTAMP, 
    'active'
);
```

## Customer User
```sql
INSERT INTO USERS (
    username, 
    password, 
    email, 
    first_name, 
    last_name, 
    user_type, 
    address, 
    phone, 
    created_date, 
    status
) VALUES (
    'qwen', 
    '5498bc226f9a5fff79e2083034dc5995ae75e7c285078c911553af746e92abcb', -- SHA-256 hash of 'qwen123'
    'qwen.customer@email.com', 
    'Qwen', 
    'Customer', 
    'customer', 
    '123 Water St', 
    '555-1234', 
    SYSTIMESTAMP, 
    'active'
);
```

## Password Hashing Algorithm
```csharp
public static string HashPassword(string password)
{
    using (SHA256 sha256 = SHA256.Create())
    {
        // Convert the password string to a byte array
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        
        // Compute the hash
        byte[] hashBytes = sha256.ComputeHash(passwordBytes);
        
        // Convert the byte array to a hexadecimal string
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            sb.Append(hashBytes[i].ToString("x2"));
        }
        
        return sb.ToString();
    }
}
```

## User Authentication Logic
```csharp
public static bool ValidateUser(string username, string password)
{
    // Hash the provided password
    string hashedPassword = HashPassword(password);
    
    // Query to check if user exists with matching username and password
    string query = "SELECT COUNT(*) FROM USERS WHERE username = :username AND password = :password AND status = 'active'";
    
    Dictionary<string, object> parameters = new Dictionary<string, object>
    {
        { ":username", username },
        { ":password", hashedPassword }
    };
    
    DataTable result = DbConnection.ExecuteQuery(query, parameters);
    
    // Check if any matching user was found
    if (result.Rows.Count > 0 && Convert.ToInt32(result.Rows[0][0]) > 0)
    {
        // Update last login timestamp
        string updateQuery = "UPDATE USERS SET last_login = SYSTIMESTAMP WHERE username = :username";
        DbConnection.ExecuteNonQuery(updateQuery, new Dictionary<string, object> { { ":username", username } });
        
        return true;
    }
    
    return false;
}
```

## User Authorization
The application will use role-based access control based on the user_type field:
- 'admin': Full access to the admin dashboard and all functions
- 'customer': Access to customer-specific pages and order functionality
- 'staff': (Future implementation) Limited admin access for specific functions