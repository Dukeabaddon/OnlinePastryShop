# Active Context

## Current Focus
We are currently updating the address and contact information across the website to reflect the new flagship store location. This includes:

1. Updating the address in the footer of Site.Master
2. Updating the address in the About.aspx page in the Flagship Store section
3. Modifying the store hours to indicate closure on Sundays with the message "Closed on Sundays to give time for rest, family, and faith."
4. Removing the "Special Schedule" link for holiday hours
5. Updating all contact information to be consistent across the website

## Recent Changes
- Added responsive design improvements to mobile layout
- Implemented new product categories on the Menu page
- Updated images for team members on the About page
- Fixed newsletter subscription form validation

## Next Steps
After completing the address and hours update:
1. Update the contact phone number on the Contact page
2. Add a notice about the new location on the homepage
3. Create a map integration with the correct location pinpoint
4. Update delivery area boundaries based on the new location

## Active Decisions
- The new store address is: "49 Quirino Hwy, Novaliches, Quezon City, Metro Manila, Philippines"
- We will remove the "San Lorenzo Shopping Center" reference as it's no longer applicable
- We will update the phone number to follow Philippines format but with random numbers
- All delivery service area information will still be limited to Metro Manila
- Sunday hours will be replaced with a closure notice emphasizing family and faith

## Considerations
- Ensure consistency of address format across all pages
- Make sure all location references are updated (header, footer, about page, contact page)
- Verify that the phone number format follows the Philippines standard (+63 format)
- Consider updating the map imagery to show the new location

## Current Focus

We are currently focused on enhancing the user experience and completing core pages:

- ✅ **About Page Implementation**: The About page has been successfully implemented with sections for Filipino heritage, core values, team profiles, and store information. All animations have been removed as requested.
- ✅ **Contact Page Implementation**: The Contact page is fully implemented with all necessary sections - hero section, contact methods (call, visit, email), bakery information, contact form, map section, FAQs, and newsletter signup. The page includes server-side validation for the contact form and handling for form submission. The form includes proper ASP.NET server controls (TextBox, Button, etc.) that are connected to the code-behind. The page features responsive design and matches the overall site aesthetic.
- ✅ **User Authentication MVP**: Successfully implemented the complete user authentication system with role-based access control, session management, and UI updates for logged-in users.
- 🔄 **Menu Page Optimization**: Continuing work on the Menu page to optimize product display and filtering by categories.
- 🔄 **Image Handling Enhancement**: Improving how product images are displayed and served.
- 🔄 **CSS Standardization**: Converting internal CSS to Tailwind CSS in both Menu and Contact pages to maintain consistent styling approach throughout the application.

## Recent Changes

1. **About Page Completion**: 
   - Implemented the About page with all required sections
   - Added team members with circular profile images and badges
   - Included Filipino heritage story and core values
   - Removed all animations as per requirement, keeping only the hero section zoom effect

2. **Contact Page Completion**:
   - ✅ Completed the Contact page implementation with all required sections
   - ✅ Implemented contact method cards (Call Us, Visit Us, Email Us)
   - ✅ Added bakery information section with image
   - ✅ Integrated contact form with validation
   - ✅ Added map section showing bakery location
   - ✅ Implemented FAQ accordion section
   - ✅ Added newsletter subscription section
   - ✅ Ensured mobile responsiveness across all sections

3. **Authentication System Implementation**:
   - ✅ Implemented user login with database validation
   - ✅ Added session management to store user information
   - ✅ Created role-based redirects (Admin → Dashboard, Customer → Homepage)
   - ✅ Implemented session verification to prevent unauthorized dashboard access
   - ✅ Updated UI to show user initials for logged-in users
   - ✅ Added dropdown menu with profile, orders, and logout options
   - ✅ Implemented logout functionality to clear sessions

4. **Menu Page Fixes**:
   - Fixed SQL query issues for product loading
   - Enhanced image handling for products

5. **CSS Standardization**:
   - 🔄 Converting internal CSS in Menu.aspx to Tailwind CSS classes
   - 🔄 Converting internal CSS in Contact.aspx to Tailwind CSS classes
   - 🔄 Removing all animations from Menu.aspx as requested
   - 🔄 Ensuring responsive design is maintained during conversion

## Next Steps

### Short-term Goals
- Implement proper password hashing for enhanced security
- Develop the user profile page functionality
- Add "Remember Me" option to the login form
- Create account recovery functionality
- Update the address and contact information across all pages
- Continue ensuring mobile responsiveness across all pages
- Finish optimizing the Menu page functionality

### Medium-term Goals
- Implement the shopping cart functionality
- Connect cart to the backend
- Set up the order processing system
- Integrate payment gateway
- Implement email notifications

## Current Decisions and Considerations

### Design Decisions
- Using the brand colors (#96744F) consistently across all pages
- Employing circular profile images for team members with descriptive badges
- Implemented a clean, modern design for the Contact page with card-based layouts
- Using circular user initials for logged-in users with a dropdown menu
- Ensuring form fields are properly labeled and validated
- Using Tailwind CSS exclusively for styling, avoiding internal CSS

### Technical Decisions
- Using ASP.NET Repeaters for data binding to display dynamic content
- Using session variables to manage user state across pages
- Implementing role-based access control for security
- Using vanilla JavaScript for client-side functionality to minimize dependencies
- Planning to enhance password security with proper hashing in the next phase

## Open Questions
- What password hashing algorithm should we implement?
- How should we structure the user profile page?
- What features should be included in the account recovery system?
- Should we implement automatic session timeout after inactivity? 