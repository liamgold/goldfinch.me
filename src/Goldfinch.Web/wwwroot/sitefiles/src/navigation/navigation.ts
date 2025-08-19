// Get the header element
const header = document.querySelector('header') as HTMLElement;

// Tailwind breakpoint constants
const TAILWIND_MD_BREAKPOINT = 768; // Tailwind's md breakpoint

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

// Mobile Menu Functionality
const mobileMenuToggle = document.querySelector('.mobile-menu-toggle') as HTMLButtonElement;
const mobileMenuOverlay = document.querySelector('.nav-links-mobile') as HTMLElement;
const mobileCloseBtn = document.querySelector('.mobile-close-btn') as HTMLButtonElement;
const body = document.body;

function openMobileMenu() {
  mobileMenuToggle?.classList.add('active');
  mobileMenuOverlay?.classList.add('active');
  body.classList.add('mobile-menu-open');
}

function closeMobileMenu() {
  mobileMenuToggle?.classList.remove('active');
  mobileMenuOverlay?.classList.remove('active');
  body.classList.remove('mobile-menu-open');
}

// Toggle mobile menu when burger button is clicked
mobileMenuToggle?.addEventListener('click', () => {
  if (mobileMenuOverlay?.classList.contains('active')) {
    closeMobileMenu();
  } else {
    openMobileMenu();
  }
});

// Close mobile menu when close button is clicked
mobileCloseBtn?.addEventListener('click', closeMobileMenu);

// Close mobile menu when clicking on a navigation link
const mobileNavLinks = document.querySelectorAll('.nav-links-mobile .nav-link');
mobileNavLinks.forEach((link) => {
  link.addEventListener('click', closeMobileMenu);
});

// Close mobile menu when clicking outside the menu
mobileMenuOverlay?.addEventListener('click', (e) => {
  if (e.target === mobileMenuOverlay) {
    closeMobileMenu();
  }
});

// Close mobile menu on escape key press
document.addEventListener('keydown', (e) => {
  if (e.key === 'Escape' && mobileMenuOverlay?.classList.contains('active')) {
    closeMobileMenu();
  }
});

// Handle window resize - close mobile menu if screen becomes large
window.addEventListener('resize', () => {
  if (window.innerWidth >= TAILWIND_MD_BREAKPOINT && mobileMenuOverlay?.classList.contains('active')) {
    closeMobileMenu();
  }
});
