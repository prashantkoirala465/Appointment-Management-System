// ============================================================
// site.js — Client-side JavaScript for the entire application
// Handles: sidebar toggle, mobile nav, scroll animations, table row clicks
// ============================================================

// Sidebar toggle for mobile devices (used in the authenticated layout)
// Called when the user taps the hamburger menu button in the mobile header
function toggleSidebar() {
    var sidebar = document.getElementById('sidebar');     // the <aside> sidebar element
    var overlay = document.getElementById('sidebarOverlay'); // dark backdrop behind sidebar on mobile
    if (sidebar && overlay) {
        sidebar.classList.toggle('open');   // slide the sidebar in/out
        overlay.classList.toggle('active'); // show/hide the dark overlay
    }
}

// Landing page mobile nav toggle (used on the public homepage)
// Toggles the hamburger button between ☰ and ✕, and shows/hides nav links
function toggleMobileNav() {
    var links = document.getElementById('navLinks');      // the <ul> of nav links
    var actions = document.getElementById('navActions');  // the login/signup buttons
    var toggle = document.getElementById('navToggle');    // the hamburger button itself
    if (links) links.classList.toggle('open');            // show/hide links on mobile
    if (actions) actions.classList.toggle('open');        // show/hide action buttons
    if (toggle) toggle.classList.toggle('active');        // animate hamburger → X
}

// Wait for the DOM to fully load before setting up event listeners
document.addEventListener('DOMContentLoaded', function () {

    // --- SMOOTH SCROLLING for landing page anchor links ---
    // When someone clicks a nav link like "#features", scroll smoothly to that section
    document.querySelectorAll('.ln-nav-links a[href^="#"]').forEach(function (anchor) {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();  // stop the default jump-to-anchor behavior
            var target = document.querySelector(this.getAttribute('href')); // find the target section
            if (target) {
                target.scrollIntoView({ behavior: 'smooth', block: 'start' }); // smooth scroll to it
                // close the mobile nav after clicking a link
                var links = document.getElementById('navLinks');
                var actions = document.getElementById('navActions');
                var toggle = document.getElementById('navToggle');
                if (links) links.classList.remove('open');
                if (actions) actions.classList.remove('open');
                if (toggle) toggle.classList.remove('active');
            }
        });
    });

    // --- SCROLL REVEAL ANIMATIONS ---
    // Elements with classes .reveal, .reveal-left, .reveal-right, .reveal-scale
    // start hidden (via CSS) and get a "visible" class when they scroll into view
    var revealElements = document.querySelectorAll('.reveal, .reveal-left, .reveal-right, .reveal-scale');
    if (revealElements.length > 0 && 'IntersectionObserver' in window) {
        // IntersectionObserver watches elements and fires when they enter the viewport
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('visible');  // trigger the CSS animation
                    observer.unobserve(entry.target);      // stop watching (animate only once)
                }
            });
        }, {
            threshold: 0.1,                    // trigger when 10% of the element is visible
            rootMargin: '0px 0px -40px 0px'    // start 40px before the element reaches the bottom edge
        });

        // start observing each reveal element
        revealElements.forEach(function (el) {
            observer.observe(el);
        });
    } else {
        // Fallback for browsers that don't support IntersectionObserver:
        // just show everything immediately without animation
        revealElements.forEach(function (el) {
            el.classList.add('visible');
        });
    }

    // --- CLICKABLE TABLE ROWS ---
    // Makes entire table rows clickable (navigates to the Details page)
    // This is a UX enhancement — users can click anywhere on the row, not just the link
    document.querySelectorAll('.table-custom tbody tr').forEach(function (row) {
        var detailLink = row.querySelector('.action-links a[href*="Details"]'); // find the Details link in this row
        if (detailLink) {
            row.style.cursor = 'pointer';  // show pointer cursor to indicate it's clickable
            row.addEventListener('click', function (e) {
                // if they clicked on the action buttons area, don't navigate
                // (let the individual Edit/Delete links work normally)
                if (e.target.closest('.action-links')) return;
                detailLink.click();  // simulate clicking the Details link
            });
        }
    });
});
