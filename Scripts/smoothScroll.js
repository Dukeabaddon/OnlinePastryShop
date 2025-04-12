/**
 * Smooth Scrolling Implementation
 * This script adds smooth scrolling behavior for both general browser scrolling
 * and internal anchor links, plus adds scroll-triggered animations.
 */

document.addEventListener('DOMContentLoaded', function() {
    console.log('Smooth scrolling initialized');
    
    // 1. Handle all internal anchor links for smooth scrolling
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            e.preventDefault();
            
            const targetId = this.getAttribute('href');
            if (targetId === '#') return; // Skip if it's just "#"
            
            const targetElement = document.querySelector(targetId);
            if (!targetElement) return; // Skip if target element doesn't exist
            
            // Get header height for offset (if you have a fixed header)
            const headerHeight = document.querySelector('nav') ? document.querySelector('nav').offsetHeight : 0;
            
            // Calculate the target position with offset
            const targetPosition = targetElement.getBoundingClientRect().top + window.scrollY - headerHeight - 20; // 20px extra padding
            
            // Perform the smooth scroll
            window.scrollTo({
                top: targetPosition,
                behavior: 'smooth'
            });
            
            // Update URL hash without scrolling (optional)
            history.pushState(null, null, targetId);
        });
    });
    
    // 2. Set up scroll-triggered animations
    const animateOnScroll = function() {
        // Get all elements with animation classes to observe
        const elementsToAnimate = document.querySelectorAll('.animate-on-scroll');
        
        if (elementsToAnimate.length === 0) return; // Skip if no elements to animate
        
        // Create Intersection Observer for animations
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    // Get animation type from data attribute or use default fade-in
                    const animationType = entry.target.dataset.animation || 'fade-in';
                    
                    // Add the animation class
                    entry.target.classList.add(animationType);
                    
                    // Stop observing after animation is applied
                    observer.unobserve(entry.target);
                }
            });
        }, {
            threshold: 0.1, // Trigger when at least 10% of the element is visible
            rootMargin: '0px 0px -50px 0px' // Adjust when animation triggers
        });
        
        // Start observing each element
        elementsToAnimate.forEach(element => {
            observer.observe(element);
        });
    };
    
    // Initialize scroll animations
    animateOnScroll();
});

// Helper function to smooth scroll to any element (can be called from other scripts)
function smoothScrollTo(elementId) {
    const element = document.getElementById(elementId);
    if (!element) return;
    
    const headerHeight = document.querySelector('nav') ? document.querySelector('nav').offsetHeight : 0;
    const targetPosition = element.getBoundingClientRect().top + window.scrollY - headerHeight - 20;
    
    window.scrollTo({
        top: targetPosition,
        behavior: 'smooth'
    });
} 