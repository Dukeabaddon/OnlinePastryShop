# System Patterns

## Architecture Overview
The Online Pastry Shop application follows a standard ASP.NET Web Forms architecture with a code-behind pattern that separates the UI elements from business logic.

## Key Design Patterns

### Presentation Layer
- **Code-Behind Pattern**: Each ASPX page has a corresponding .aspx.cs file containing the server-side logic
- **Master Page Pattern**: Using MasterPage.master to maintain consistent layout across pages
- **Repeater Pattern**: Using ASP.NET Repeaters to display product catalogs and order lists

### Business Logic Layer
- **Service Classes**: Separate classes handling specific business functions
- **Data Transfer Objects (DTOs)**: Objects used to pass data between layers

### Data Access Layer
- **Connection String Abstraction**: Centralized retrieval from Web.config
- **Command Pattern**: Using SqlCommand objects to execute queries
- **Data Reader Pattern**: Using SqlDataReader to retrieve data from the database

## Security Implementation
- **Password Hashing**: SHA256 implementation for password security
- **Role-based Authorization**: Admin and Customer roles with different access levels
- **Session Validation**: Checking session variables to enforce authenticated access

## Page Flow
```
Login.aspx → [Role Check] → Dashboard.aspx (Admin) or Default.aspx (Customer)
```

## Error Handling
- Try-catch blocks for database operations
- User-friendly error messages displayed in UI
- Logging of critical errors

## State Management
- **Session State**: Storing user authentication information
- **View State**: Maintaining page state between postbacks
- **Application State**: Storing application-wide settings

## Responsive Design
- Bootstrap-based responsive grid system
- Mobile-first approach for UI components