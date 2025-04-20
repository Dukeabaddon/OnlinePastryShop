# System Patterns

## Architecture Overview
The Online Pastry Shop application follows a standard ASP.NET Web Forms architecture with a code-behind pattern that separates the UI elements from business logic.

## Key Design Patterns

### Presentation Layer
- **Code-Behind Pattern**: Each ASPX page has a corresponding .aspx.cs file containing the server-side logic
- **Master Page Pattern**: Using Site.Master for customer pages and AdminMaster.Master for admin pages
- **Repeater Pattern**: Using ASP.NET Repeaters to display product catalogs and order lists
- **Conditional Rendering**: UI elements shown or hidden based on session state
- **Client-Side Interaction**: JavaScript for dynamic UI components like dropdowns

### Business Logic Layer
- **Service Classes**: Separate classes handling specific business functions
- **Data Transfer Objects (DTOs)**: Objects used to pass data between layers
- **Validation Logic**: Form validation and user credential checking

### Data Access Layer
- **Connection String Abstraction**: Centralized retrieval from Web.config
- **Command Pattern**: Using OracleCommand objects to execute queries
- **Data Reader Pattern**: Using OracleDataReader to retrieve data from the database
- **Parameter Binding**: Using OracleParameter to prevent SQL injection

## Security Implementation
- **Password Hashing**: SHA256 implementation for password security
- **Role-based Authorization**: Admin and Customer roles with different access levels
- **Session Validation**: Checking session variables to enforce authenticated access
- **Session Termination**: Proper session clearing and abandonment on logout
- **Exception Handling**: Try-catch blocks around security-critical operations
- **Conditional Access**: Different master pages and access patterns based on role

## Authentication Flow
```
Login.aspx
├── User enters credentials
├── ValidateUser() retrieves user from database
├── HashPassword() creates SHA256 hash
├── Compare hashed password with database
├── On success: Create session variables
│   ├── Session["UserID"] = userId
│   ├── Session["FirstName"] = firstName
│   ├── Session["LastName"] = lastName
│   ├── Session["UserRole"] = role
│   └── Session["UserInitials"] = initials
└── Redirect based on role
    ├── Admin → Dashboard.aspx
    └── Customer → Default.aspx
```

## Session Validation Flow
```
Protected Page (e.g., Dashboard.aspx)
├── Page_Load checks Session["UserID"]
│   ├── If null: Redirect to Login.aspx
│   └── If exists: Continue
├── Page_Load checks Session["UserRole"]
│   ├── If not "Admin": Redirect to Default.aspx
│   └── If "Admin": Allow access
└── Load page content for authenticated user
```

## Logout Flow
```
Site.Master (Customer)
├── User clicks logout
├── lnkLogout_Click handler
│   ├── Session.Clear()
│   ├── Session.Abandon()
│   └── Redirect to Login.aspx
    
AdminMaster.Master (Admin)
├── User clicks logout
├── lnkAdminLogout_Click handler
│   ├── Try: Clear session
│   │   ├── Session.Clear()
│   │   └── Session.Abandon()
│   ├── Catch: Log error
│   └── Finally: Redirect to Default.aspx
```

## UI State Management
- **Session Variables**: Store user authentication state
- **Client-Side State**: JavaScript for UI interactions
- **ViewState**: ASP.NET's built-in state management
- **Conditional Rendering**:
  ```
  <% if (Session["UserID"] != null) { %>
      <!-- Logged in UI elements -->
  <% } else { %>
      <!-- Not logged in UI elements -->
  <% } %>
  ```

## Error Handling
- Try-catch blocks for database operations
- Exception logging with System.Diagnostics.Debug
- User-friendly error messages displayed in UI
- Exception handling around session management
- Finally blocks to ensure critical operations complete

## State Management
- **Session State**: Storing user authentication information
- **View State**: Maintaining page state between postbacks
- **Application State**: Storing application-wide settings
- **Query String**: Passing simple parameters between pages

## Responsive Design
- Bootstrap-based responsive grid system
- Mobile-first approach for UI components
- Tailwind CSS for styling components