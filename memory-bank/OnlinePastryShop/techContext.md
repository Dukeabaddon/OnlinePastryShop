# Technical Context

## Technology Stack
- ASP.NET Web Forms (.NET Framework)
- C# for server-side logic
- Oracle Database for data storage (Oracle.ManagedDataAccess client)
- JavaScript for client-side interactions
- Tailwind CSS for responsive UI components

## Development Environment
- Visual Studio 2022 (primary IDE)
- Visual Studio Code (supplementary editor)
- Oracle Database (local or remote instance)
- Web browser developer tools for front-end debugging

## Project Structure
- `Pages/` - Contains all ASPX pages and their code-behind files
  - `*.aspx` - Web Form pages
  - `*.aspx.cs` - Code-behind logic
  - `*.aspx.designer.cs` - Auto-generated control definitions
  - `Site.Master` - Main template for customer pages
  - `AdminMaster.Master` - Template for admin dashboard pages
- `Styles/` - CSS stylesheets
- `Scripts/` - JavaScript files
- `App_Data/` - Database files and app data
- `Images/` - Product and UI images
- `Web.config` - Application configuration including connection strings

## Authentication Implementation
- Form-based authentication with custom user store
- SHA256 password hashing (currently without salt)
- Session-based auth state management
- Role-based access control (Admin/Customer)
- Session variables for maintaining authentication state:
  - UserID: Primary identifier for the authenticated user
  - FirstName/LastName: Personal information
  - UserRole: Role-based access control identifier
  - UserInitials: UI display element

## Session Management
- Session variables created on successful login
- Session validation on protected pages
- Session clearing on logout (Session.Clear() and Session.Abandon())
- Different logout flows based on user role:
  - Customer: Redirect to Login.aspx
  - Admin: Redirect to Default.aspx

## Database Schema
- Users (UserID, Username, FirstName, LastName, PasswordHash, Email, Role, IsActive)
- Products (ProductID, Name, Description, Price, CategoryID, StockQuantity, ImagePath, IsActive)
- Orders (OrderID, UserID, OrderDate, TotalAmount, Status, IsActive)
- OrderDetails (OrderDetailID, OrderID, ProductID, Quantity, Price)
- Categories (CategoryID, Name, Description, IsActive)

## Connection String Management
- Stored in Web.config
- Retrieved via ConfigurationManager.ConnectionStrings
- Fallback to hardcoded connection string if configuration fails

## Security Implementation
- Password hashing with SHA256
- Parameterized queries to prevent SQL injection
- Server-side validation of user inputs
- Role-based access restrictions
- Session validation on protected pages
- Exception handling around security-critical operations