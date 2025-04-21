# Technical Context

## Technologies Used

### Frontend
- **ASP.NET Web Forms**: Core framework for web pages
- **HTML5/CSS3**: Markup and styling
- **Tailwind CSS**: Utility-first CSS framework for responsive design
- **JavaScript**: Client-side interactivity
- **SVG Icons**: For UI elements and visual components

### Backend
- **C#**: Primary programming language
- **ASP.NET Framework**: Web application framework
- **ADO.NET**: Data access technology
- **SQL Server**: Database management system

### Development Tools
- **Visual Studio**: Primary IDE
- **Git**: Version control
- **Azure DevOps**: CI/CD and issue tracking
- **SQL Server Management Studio**: Database management

## Technical Constraints

### Browser Compatibility
- Support for modern browsers (Chrome, Firefox, Safari, Edge)
- Graceful degradation for older browsers
- Mobile browser optimization

### Performance Requirements
- Page load time under 3 seconds
- Responsive to user interactions within 300ms
- Database queries optimized for quick results

### Security Considerations
- HTTPS implementation
- Secure handling of customer data
- PCI compliance for payment processing
- Protection against common web vulnerabilities (XSS, CSRF, SQL Injection)

## Deployment Environment
- **Web Server**: IIS on Windows Server
- **Database**: SQL Server 2019
- **Hosting**: Azure App Service (or similar cloud provider)
- **Content Delivery**: Azure CDN for static assets

## Dependencies

### External Services
- Payment gateway integration (to be determined)
- Email service provider for notifications
- Google Maps API for store location
- Social media sharing integrations

### Internal Dependencies
- Authentication system
- Product inventory management
- Order processing workflow
- Customer account management

## Development Setup
- Local development using Visual Studio
- Local SQL Server instance for database
- Source control with Git
- Deployment using Azure DevOps pipelines

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
  - Vanilla JavaScript for modern UI components
- **AJAX**: ASP.NET UpdatePanels, ScriptManager, and WebMethods
- **CSS Techniques**:
  - CSS Grid for responsive layouts
  - Flexbox for component alignment
  - CSS animations for interactive elements
  - Media queries for device adaptations

### Development Environment
- **IDE**: Visual Studio 2022
- **Version Control**: Git
- **Database Tools**: Oracle SQL Developer
- **Testing**: Manual testing for UI/UX

## UI Component System

### Core Components
- **Product Cards**: Consistent display of product information
- **Category Tabs**: Filterable navigation system
- **Toast Notifications**: User feedback system
- **Loading States**: Visual indicators for data fetching
- **Responsive Grid**: Adapts to various screen sizes
- **Stock Indicators**: Visual representation of inventory status

### Component Design Principles
- **Consistency**: Common styling across all UI elements
- **Responsiveness**: Adapts to all device sizes
- **Accessibility**: Clear contrast and readable text
- **Interaction Feedback**: Visual cues for user actions
- **Error Handling**: Clear messaging for error states

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