# System Patterns

## Architecture Overview
The Pastry Palace website is built using ASP.NET Web Forms with a responsive front-end design. It follows a traditional web application architecture:

- **Presentation Layer**: ASP.NET Web Forms (.aspx pages)
- **Business Logic Layer**: C# code-behind files (.aspx.cs)
- **Data Access Layer**: ADO.NET for database operations
- **Database**: SQL Server database for product, user, and order data

## Design Patterns

### Master Page Pattern
- Site.Master provides consistent layout and navigation
- AdminMaster.Master for admin dashboard pages
- Content placeholders for page-specific content
- Shared components like header, footer, and navigation

### Responsive Design Pattern
- Mobile-first approach using Tailwind CSS
- Flexbox and Grid layouts for responsive components
- Media queries for different device sizes

### Component-Based Structure
- Reusable UI components (navigation, product cards, etc.)
- Consistent styling across components
- Modular JavaScript for interactive elements

### Form Handling Pattern
- Server-side validation for form submissions
- Client-side validation for immediate feedback
- Error handling and user notifications

## Key Technical Decisions

### Front-End Framework
- Tailwind CSS for styling
- Minimal custom CSS for specific components
- Custom animations for enhanced user experience

### State Management
- ASP.NET session state for shopping cart
- Authentication cookies for user sessions
- Local storage for user preferences

### Performance Optimizations
- Lazy loading images
- CSS/JS minification
- Database query optimization
- Caching for frequent database operations

### Security Measures
- Input validation and sanitization
- SQL injection prevention
- Cross-Site Scripting (XSS) protection
- Secure authentication workflow

## Component Relationships
- Master page provides the shell for all content pages
- Navigation links connect different sections of the website
- Product catalog connects to individual product details
- Shopping cart integrates with checkout process
- User authentication connects to account management

## Key Design Patterns

### Page Controller Pattern
Each web form (`.aspx`) has a corresponding code-behind file (`.aspx.cs`) that handles:
- User interactions
- Business logic execution
- Data binding to UI elements
- Response generation

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