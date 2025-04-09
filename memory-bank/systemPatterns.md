# System Patterns

## Architecture Overview
The Online Pastry Shop application follows a traditional ASP.NET Web Forms architecture with a multi-layered approach:

1. **Presentation Layer**:
   - ASP.NET Web Forms pages (.aspx files)
   - Master pages for layout consistency
   - Client-side scripting with JavaScript
   - Responsive UI components with CSS Grid and Flexbox

2. **Business Logic Layer**:
   - Code-behind files (.aspx.cs)
   - Utility classes for common operations
   - WebMethods for AJAX data operations

3. **Data Access Layer**:
   - Oracle database integration
   - Stored procedures for data operations
   - Connection management utilities

## Key Design Patterns

### Page Controller Pattern
Each web form (`.aspx`) has a corresponding code-behind file (`.aspx.cs`) that handles:
- User interactions
- Business logic execution
- Data binding to UI elements
- Response generation

### Master Page Pattern
- `AdminMaster.Master` provides consistent layout for admin pages
- `Site.Master` provides consistent layout for customer-facing pages
- Shared components like navigation and footer

### Repository Pattern (Modified)
- Data access occurs through stored procedures 
- Oracle parameters are used for parameterized queries
- Database connections are managed through connection strings

### Badge Counter Pattern
- Notification counters for pending actions (orders, messages, etc.)
- Dynamically updated counts on the admin sidebar

### Dashboard Component Pattern
- Modular dashboard widgets for different metrics
- User-customizable components with persistence
- Real-time data updates through AJAX

### Card UI Pattern
- Product information displayed in consistent card format
- Cards include image, title, description, price, and actions
- Responsive layout adapts to different screen sizes
- Hover effects for enhanced user experience

### Toast Notification Pattern
- Non-intrusive feedback for user actions
- Temporary display with automatic dismissal
- Consistent styling and positioning

### Category Filter Pattern
- Tabbed interface for category selection
- Clear visual feedback for active category
- Dynamic content filtering without page reload

## Database Approach

### Table Design
- Core business entities (USERS, PRODUCTS, ORDERS, etc.)
- Relationship tables for many-to-many relationships
- Support tables for features like wishlists, ratings, etc.

### Stored Procedures
- Parameterized procedures for all CRUD operations
- Complex queries encapsulated in procedures
- Return values and output parameters for complex operations

### Triggers and Sequences
- Automated ID generation using sequences
- Timestamp maintenance with triggers
- Consistency maintained through constraints

## Component Relationships

```mermaid
flowchart TD
    AdminMaster[AdminMaster.Master] --> Dashboard[Dashboard.aspx]
    AdminMaster --> Products[Products.aspx]
    AdminMaster --> Orders[Orders.aspx]
    AdminMaster --> Users[Users.aspx]
    AdminMaster --> Marketing[Marketing Features]
    
    Dashboard --> DashboardPrefs[Dashboard Preferences]
    Dashboard --> KPICards[KPI Cards]
    Dashboard --> Charts[Interactive Charts]
    
    Products --> ProductManagement[Product Management]
    Products --> CategoryManagement[Category Management]
    
    Orders --> OrderProcessing[Order Processing]
    Orders --> OrderFulfillment[Order Fulfillment]
    
    Users --> UserManagement[User Management]
    Users --> CustomerService[Customer Service]
    
    Marketing --> Newsletter[Newsletter]
    Marketing --> Vouchers[Vouchers/Discounts]
    
    SiteMaster[Site.Master] --> HomePage[Default.aspx]
    SiteMaster --> MenuPage[Menu.aspx]
    SiteMaster --> AboutPage[About.aspx]
    SiteMaster --> ContactPage[Contact.aspx]
    
    MenuPage --> CategoryFilters[Category Tabs]
    MenuPage --> ProductGrid[Product Grid]
    MenuPage --> AddToCart[Cart Functions]
    
    ProductGrid --> ProductCard[Product Card Component]
    ProductCard --> ProductImage[Image Display]
    ProductCard --> ProductInfo[Product Information]
    ProductCard --> StockStatus[Stock Indicator]
    ProductCard --> CartButton[Add to Cart Button]
```

## UI Component Patterns

### Product Card
- Image container with fixed aspect ratio
- Consistent information layout (name, description, price)
- Stock status indicator (in-stock or out-of-stock)
- Action button for cart interaction
- Hover effects for visual feedback

### Category Tabs
- Horizontally scrollable on mobile
- Active state highlighting
- Consistent styling with brand colors
- Click event handling for filtering

### Toast Notifications
- Fixed position at bottom right
- Animated entry and exit
- Automatically disappears after delay
- Z-index handling for proper layering

### Loading States
- Centered spinner animation
- Text indication of loading status
- Appropriate hiding/showing based on data state
- Clear error state presentation

## Customization Approach
- User preferences stored in database tables
- Client-side state managed with ViewState and Session
- Server-side user settings loaded on page initialization
- AJAX for dynamic updates without full page reloads 