# Technical Context

## Technology Stack

### Backend Technologies
- **Framework**: ASP.NET Web Forms (.NET Framework)
- **Language**: C#
- **Database**: Oracle 11g
- **ORM**: Native Oracle Data Provider (Oracle.ManagedDataAccess)

### Frontend Technologies
- **UI Framework**: TailwindCSS v4 (via CDN)
- **JavaScript Libraries**: 
  - Chart.js for dashboard visualizations
  - jQuery for DOM manipulation (when needed)
- **AJAX**: ASP.NET UpdatePanels and ScriptManager

### Development Environment
- **IDE**: Visual Studio 2022
- **Version Control**: Git
- **Database Tools**: Oracle SQL Developer
- **Testing**: Manual testing for UI/UX

## Database Design

### Core Tables
- USERS: User accounts and authentication
- PRODUCTS: Pastry product catalog
- CATEGORIES: Product categorization
- ORDERS: Customer order information
- ORDERDETAILS: Line items within orders
- VOUCHERS: Discount vouchers and promotions
- SHOP_SETTINGS: Global store configuration

### New Feature Tables
- DASHBOARD_PREFERENCES: User dashboard customization
- WISHLIST: User product wishlists
- PRODUCT_RATINGS: Customer reviews and ratings
- NEWSLETTER: Email newsletter subscribers
- BLOGS: Blog content management
- CONTACT_MESSAGES: Customer inquiries

### Database Conventions
- **Naming**: Lowercase for column names (e.g., `user_id`, not `USER_ID`)
- **Primary Keys**: Auto-incrementing numeric IDs via sequences
- **Foreign Keys**: Enforced relationships with constraints
- **Timestamps**: Created/updated timestamps on relevant tables
- **Soft Deletes**: ISACTIVE flags for logical deletion

## API and Integration Points

### Internal APIs
- Stored procedures for complex database operations
- WebMethods for AJAX calls from frontend
- PageMethods for page-specific operations

### External Integrations (Planned)
- Email service for notifications and newsletters
- Payment gateway for checkout processing
- Image storage for product photos

## Development Constraints

### Technical Limitations
- Legacy ASP.NET Web Forms architecture
- Oracle database with specific syntax requirements
- Limited use of modern JavaScript frameworks

### Security Considerations
- Parameterized queries to prevent SQL injection
- Session management for authenticated users
- HTTPS for all traffic
- Form validation on both client and server sides

## Deployment Architecture

### Current Environment
- Windows Server hosting
- Oracle database on dedicated server
- Static assets served from application server

### Future Considerations
- Cloud migration for better scalability
- Content Delivery Network (CDN) for static assets
- Containerization for easier deployment 