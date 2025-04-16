# Technical Context

## Technology Stack
- ASP.NET Web Forms (.NET Framework)
- C# for server-side logic
- SQL Server for database
- JavaScript for client-side interactions
- Bootstrap for responsive UI components

## Development Environment
- Visual Studio 2022 (primary IDE)
- Visual Studio Code (supplementary editor)
- SQL Server Management Studio for database management

## Project Structure
- `Pages/` - Contains all ASPX pages and their code-behind files
- `Styles/` - CSS stylesheets
- `Scripts/` - JavaScript files
- `App_Data/` - Database files and app data
- `Images/` - Product and UI images
- `MasterPage.master` - Main template for consistent layout

## Authentication Implementation
- Form-based authentication with custom user store
- SHA256 password hashing
- Session-based auth state management
- Role-based access control (Admin/Customer)

## Database Schema
- Users (UserID, Username, PasswordHash, Email, Role)
- Products (ProductID, Name, Description, Price, CategoryID, ImagePath)
- Orders (OrderID, UserID, OrderDate, TotalAmount, Status)
- OrderDetails (OrderDetailID, OrderID, ProductID, Quantity, UnitPrice)
- Categories (CategoryID, Name, Description)

## Connection String Management
- Stored in Web.config
- Retrieved via ConfigurationManager.ConnectionStrings