// Sidebar toggle for mobile devices (authenticated app)
function toggleSidebar() {
    var sidebar = document.getElementById('sidebar');
    var overlay = document.getElementById('sidebarOverlay');
    if (sidebar && overlay) {
        sidebar.classList.toggle('open');
        overlay.classList.toggle('active');
    }
}

// Landing page mobile nav toggle
function toggleMobileNav() {
    var links = document.getElementById('navLinks');
    var actions = document.getElementById('navActions');
    if (links) links.classList.toggle('open');
    if (actions) actions.classList.toggle('open');
}

// Smooth scroll for landing page anchor links
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.ln-nav-links a[href^="#"]').forEach(function (anchor) {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            var target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
                // Close mobile nav if open
                var links = document.getElementById('navLinks');
                var actions = document.getElementById('navActions');
                if (links) links.classList.remove('open');
                if (actions) actions.classList.remove('open');
            }
        });
    });
});

