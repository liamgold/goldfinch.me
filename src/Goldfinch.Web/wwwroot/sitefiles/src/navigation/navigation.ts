// Get the header element
const header = document.querySelector('header') as HTMLElement;

// Function to toggle the data-scrolled attribute based on the scroll position
function toggleScrolledClass() {
  if (window.scrollY > 0) {
    header?.setAttribute('data-scrolled', 'true');
  } else {
    header?.setAttribute('data-scrolled', 'false');
  }
}

// Call the function on page load in case the page is already scrolled
toggleScrolledClass();

// Add a scroll event listener to update the data-scrolled attribute as the page scrolls
window.addEventListener('scroll', toggleScrolledClass);

// Enhanced header scroll effects
let lastScrollY = window.scrollY;
let ticking = false;

function updateHeaderOnScroll() {
  const currentScrollY = window.scrollY;
  
  if (header) {
    // Add/remove scrolled state
    if (currentScrollY > 50) {
      header.setAttribute('data-scrolled', 'true');
    } else {
      header.setAttribute('data-scrolled', 'false');
    }
  }
  
  lastScrollY = currentScrollY;
  ticking = false;
}

// Throttle scroll events for better performance
function requestTick() {
  if (!ticking) {
    requestAnimationFrame(updateHeaderOnScroll);
    ticking = true;
  }
}

// Update scroll listener to use throttled function
window.removeEventListener('scroll', toggleScrolledClass);
window.addEventListener('scroll', requestTick);

// Add smooth hover effects for navigation links
const navLinks = document.querySelectorAll('.nav-link');

navLinks.forEach((link) => {
  const element = link as HTMLElement;
  element.addEventListener('mouseenter', () => {
    element.style.transform = 'translateY(-1px)';
  });
  
  element.addEventListener('mouseleave', () => {
    element.style.transform = 'translateY(0)';
  });
});
