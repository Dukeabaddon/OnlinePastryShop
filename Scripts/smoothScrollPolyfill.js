/**
 * Smooth Scroll Polyfill
 * This polyfill ensures smooth scrolling works on all browsers
 * Based on smoothscroll-polyfill by Dustan Kasten
 */

(function() {
  // Check if smooth scrolling is supported natively
  if ('scrollBehavior' in document.documentElement.style) {
    console.log('Browser supports native smooth scrolling');
    return;
  }
  
  console.log('Adding smooth scrolling polyfill');
  
  // Get requestAnimationFrame method
  const requestAnimFrame = window.requestAnimationFrame || window.webkitRequestAnimationFrame || function(callback) { window.setTimeout(callback, 1000/60); };
  
  // Polyfill the window.scrollTo method
  const originalScrollTo = window.scrollTo;
  
  // Helper for easing animations
  function ease(t) {
    return t<0.5 ? 2*t*t : -1+(4-2*t)*t;  // ease in-out quad
  }
  
  // Smooth scroll implementation
  function smoothScroll(x, y, duration) {
    const startTime = performance.now();
    const startX = window.scrollX || window.pageXOffset;
    const startY = window.scrollY || window.pageYOffset;
    const distanceX = x - startX;
    const distanceY = y - startY;
    
    // Set default duration if not specified
    duration = duration || 300;
    
    function step() {
      const elapsed = performance.now() - startTime;
      const progress = Math.min(elapsed / duration, 1);
      const easedProgress = ease(progress);
      
      window.scrollTo(
        startX + distanceX * easedProgress,
        startY + distanceY * easedProgress
      );
      
      if (progress < 1) {
        requestAnimFrame(step);
      }
    }
    
    step();
  }
  
  // Override the window.scrollTo method
  window.scrollTo = function() {
    if (arguments.length === 1 && typeof arguments[0] === 'object') {
      const options = arguments[0];
      
      if (options.behavior === 'smooth') {
        return smoothScroll(
          options.left !== undefined ? options.left : window.scrollX || window.pageXOffset,
          options.top !== undefined ? options.top : window.scrollY || window.pageYOffset,
          options.duration
        );
      }
    }
    
    // Default to original scrollTo if not smooth scrolling
    return originalScrollTo.apply(window, arguments);
  };
  
  // Also override scrollIntoView
  if (Element.prototype.scrollIntoView) {
    const originalScrollIntoView = Element.prototype.scrollIntoView;
    
    Element.prototype.scrollIntoView = function() {
      if (arguments.length === 1 && typeof arguments[0] === 'object' && arguments[0].behavior === 'smooth') {
        const headerHeight = document.querySelector('nav') ? document.querySelector('nav').offsetHeight : 0;
        const targetPosition = this.getBoundingClientRect().top + window.scrollY - headerHeight - 20;
        
        return smoothScroll(window.scrollX, targetPosition, 300);
      }
      
      // Default to original scrollIntoView
      return originalScrollIntoView.apply(this, arguments);
    };
  }
})(); 