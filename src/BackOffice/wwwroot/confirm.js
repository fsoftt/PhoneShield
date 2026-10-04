'use strict';

// Forms with data-confirm ask before posting. The only script in the back office; without it the forms still work.
document.addEventListener('submit', (event) => {
  const message = event.target.dataset?.confirm;
  if (message && !window.confirm(message)) {
    event.preventDefault();
  }
});
