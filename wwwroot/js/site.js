// Sidebar toggle for mobile devices (authenticated app)
function toggleSidebar() {
    var sidebar = document.getElementById('sidebar');
    var overlay = document.getElementById('sidebarOverlay');
    if (sidebar && overlay) {
        sidebar.classList.toggle('open');
        overlay.classList.toggle('active');
    }
}

// Landing page mobile nav toggle with hamburger ↔ X
function toggleMobileNav() {
    var links = document.getElementById('navLinks');
    var actions = document.getElementById('navActions');
    var toggle = document.getElementById('navToggle');
    if (links) links.classList.toggle('open');
    if (actions) actions.classList.toggle('open');
    if (toggle) toggle.classList.toggle('active');
}

// Intersection Observer for scroll-triggered reveal animations
document.addEventListener('DOMContentLoaded', function () {
    // Smooth scroll for landing page anchor links
    document.querySelectorAll('.ln-nav-links a[href^="#"]').forEach(function (anchor) {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            var target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
                // Close mobile nav if open
                var links = document.getElementById('navLinks');
                var actions = document.getElementById('navActions');
                var toggle = document.getElementById('navToggle');
                if (links) links.classList.remove('open');
                if (actions) actions.classList.remove('open');
                if (toggle) toggle.classList.remove('active');
            }
        });
    });

    // Scroll reveal observer
    var revealElements = document.querySelectorAll('.reveal, .reveal-left, .reveal-right, .reveal-scale');
    if (revealElements.length > 0 && 'IntersectionObserver' in window) {
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('visible');
                    observer.unobserve(entry.target);
                }
            });
        }, {
            threshold: 0.1,
            rootMargin: '0px 0px -40px 0px'
        });

        revealElements.forEach(function (el) {
            observer.observe(el);
        });
    } else {
        // Fallback: show everything immediately
        revealElements.forEach(function (el) {
            el.classList.add('visible');
        });
    }

    // Add hover effect to table rows (cursor pointer for clickable feel)
    document.querySelectorAll('.table-custom tbody tr').forEach(function (row) {
        var detailLink = row.querySelector('.action-links a[href*="Details"]');
        if (detailLink) {
            row.style.cursor = 'pointer';
            row.addEventListener('click', function (e) {
                if (e.target.closest('.action-links')) return;
                detailLink.click();
            });
        }
    });
});
