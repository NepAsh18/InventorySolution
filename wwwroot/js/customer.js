document.addEventListener('DOMContentLoaded', function () {
    // Cart dropdown functionality
    const cartDropdown = document.getElementById('cartDropdown');

    // Update cart badge
    function updateCartBadge(count) {
        const badge = cartDropdown.querySelector('.badge');
        badge.textContent = count;
        if (count === 0) {
            badge.classList.add('d-none');
        } else {
            badge.classList.remove('d-none');
        }
    }

    // Example cart items count
    updateCartBadge(3);

    // Cart item quantity adjustment
    document.querySelectorAll('.cart-item .btn').forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            const action = this.querySelector('i').classList.contains('bi-plus') ? 'plus' :
                this.querySelector('i').classList.contains('bi-dash') ? 'minus' : 'remove';

            const quantityElement = this.parentElement.querySelector('span');
            let quantity = parseInt(quantityElement.textContent);

            if (action === 'plus') {
                quantity++;
            } else if (action === 'minus' && quantity > 1) {
                quantity--;
            } else if (action === 'remove') {
                // Remove item from cart
                this.closest('.cart-item').remove();
                updateCartBadge(2); // Example: decrease count
                return;
            }

            quantityElement.textContent = quantity;
        });
    });

    // Search functionality
    const searchInput = document.querySelector('.input-group input');
    const searchButton = document.querySelector('.input-group button');

    searchButton.addEventListener('click', function () {
        if (searchInput.value.trim() !== '') {
            alert(`Searching for: ${searchInput.value}`);
            // Implement actual search functionality here
        }
    });

    searchInput.addEventListener('keypress', function (e) {
        if (e.key === 'Enter' && searchInput.value.trim() !== '') {
            alert(`Searching for: ${searchInput.value}`);
            // Implement actual search functionality here
        }
    });

    // Initialize all Bootstrap tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});