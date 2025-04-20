# Technical Context

## Technology Stack

### Frontend
- **HTML/CSS/JavaScript**: Standard web technologies for UI
- **ASP.NET WebForms**: Framework for server-rendered UI components
- **jQuery**: JavaScript library (version 3.7.1)
- **Font Awesome**: Icon library (version 6.4.0)
- **Custom CSS**: Extensive custom styling (~3800 lines)

### Backend
- **.NET Framework 4.7.2**: Base runtime framework
- **C#**: Primary programming language
- **ASP.NET**: Web application framework
- **Microsoft.CodeDom.Providers.DotNetCompilerPlatform**: For dynamic compilation

### Database
- **Oracle 11g XE**: Database for storing user data, products, orders, and feedback
- **Connection Details**:
  - Host: localhost
  - Port: 1521
  - Service: xe
  - Username: zen
  - Password: qwen123
- **Tables**:
  - USERS: User accounts and information
  - PRODUCTS: Water product details
  - ORDERS: Customer orders
  - ORDER_DETAILS: Items within each order
  - FEEDBACK: Customer reviews and ratings
  - AUDIT_LOG: System changes for auditing
  - INVENTORY_LOG: Product inventory changes
  - SETTINGS: System configuration settings
- **Database Objects**:
  - Sequences for ID generation
  - Triggers for audit logging
  - Views for reporting

## Development Environment
- **Visual Studio**: Primary IDE (based on project structure)
- **IIS Express**: Local development server
- Default local URL: http://localhost:9090/

## Solution Architecture
- **WebForms Pattern**: ASPX pages with code-behind files
- **Master Page**: Site.Master for layout consistency
- **Page Inheritance**: All pages inherit from the master page
- **Admin Area**: Complete admin dashboard with dedicated master page
- **Data Access Layer**: Custom implementation to communicate with Oracle

## File Structure
- **Root**: Main ASPX pages and Site.Master
- **Admin/**: Administrative dashboard and management pages
- **Assets/**: Static resources
  - **css/**: Stylesheets
  - **js/**: JavaScript files
  - **images/**: Image resources
- **Models/**: Data models
- **DataAccess/**: Oracle database connection and repositories
- **Scripts/**: Library scripts (jQuery)
- **App_Data/**: Data storage (empty)

## Deployment Configuration
- **Web.config**: Main configuration file with Oracle connection string
- **Web.Debug.config**: Debug-specific overrides
- **Web.Release.config**: Production-specific overrides

## Browser Compatibility
CSS and JavaScript indicate support for:
- Modern browsers (Chrome, Firefox, Safari, Edge)
- Mobile responsiveness with media queries
- Fallbacks for older browsers

## Security Considerations
- **validateRequest="true"** in Web.config for XSS protection
- Password hashing implementation
- Role-based access control for admin dashboard

## Integration Points
- Oracle database connection
- Potential for payment gateway integration (not implemented)