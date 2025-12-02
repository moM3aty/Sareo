document.addEventListener("DOMContentLoaded", function () {
const scrollToTopBtn = document.getElementById("scrollToTopBtn");
const navbar = document.getElementById("navbar"); 

if (scrollToTopBtn) {
  window.addEventListener("scroll", () => {
    scrollToTopBtn.style.display = window.scrollY > 300 ? "flex" : "none";
    if (navbar) {
      navbar.classList.toggle("scrolled", window.scrollY > 50);
    }
  });

  scrollToTopBtn.addEventListener("click", () => {
    window.scrollTo({ top: 0, behavior: "smooth" });
  });
}
    const navbarCollapse = document.getElementById("navbarNav");

    if (navbarCollapse) {
        const navLinks = navbarCollapse.querySelectorAll(".nav-link:not(.dropdown-toggle)");

        navLinks.forEach(link => {
            link.addEventListener("click", function () {
                if (window.getComputedStyle(navbarCollapse).display !== "none") {
                    new bootstrap.Collapse(navbarCollapse, { toggle: false }).hide();
                }
            });
        });
    }
    const dropdownToggle = document.querySelector(".nav-item.dropdown > .nav-link");

    if (dropdownToggle) {
        dropdownToggle.addEventListener("click", function (e) {
            e.preventDefault();

            const dropdownMenu = this.nextElementSibling;

            dropdownMenu.classList.toggle("show");
        });
    }

    document.addEventListener("click", function (e) {
        const dropdownMenu = document.querySelector(".nav-item.dropdown .dropdown-menu");
        const dropdownLink = document.querySelector(".nav-item.dropdown > .nav-link");

        if (dropdownMenu && !dropdownLink.contains(e.target) && !dropdownMenu.contains(e.target)) {
            dropdownMenu.classList.remove("show");
        }
    });
});