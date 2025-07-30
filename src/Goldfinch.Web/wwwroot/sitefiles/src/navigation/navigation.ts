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

// Get the burger icon button, mobile navigation container, and close button (X) elements
const burgerIcon = document.getElementById('burgerIcon') as HTMLButtonElement;
const mobileNav = document.getElementById('mobileNav') as HTMLDivElement;
const closeNavButton = document.getElementById('closeNav') as HTMLButtonElement;

// Check if the elements are found before adding event listeners
if (burgerIcon && mobileNav && closeNavButton) {
  // Add a click event listener to toggle mobile navigation visibility when the burger icon is clicked
  burgerIcon.addEventListener('click', () => {
    mobileNav.classList.toggle('hidden');
  });

  // Add a click event listener to close the mobile navigation when the close (X) button is clicked
  closeNavButton.addEventListener('click', () => {
    mobileNav.classList.add('hidden');
  });
}
