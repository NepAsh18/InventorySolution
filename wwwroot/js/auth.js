document.addEventListener('DOMContentLoaded', function () {
    // Toggle between login and register panels
    const toggleBtn = document.getElementById('togglePanel');
    const showRegister = document.getElementById('showRegister');
    const showLogin = document.getElementById('showLogin');
    const formPanels = document.querySelector('.form-panels');

    if (toggleBtn) {
        toggleBtn.addEventListener('click', function (e) {
            e.preventDefault();
            formPanels.classList.toggle('register-active');
            resetFormErrors();
        });
    }

    if (showRegister) {
        showRegister.addEventListener('click', function (e) {
            e.preventDefault();
            formPanels.classList.add('register-active');
            resetFormErrors();
        });
    }

    if (showLogin) {
        showLogin.addEventListener('click', function (e) {
            e.preventDefault();
            formPanels.classList.remove('register-active');
            resetFormErrors();
        });
    }

    // Form validation helper
    function resetFormErrors() {
        document.querySelectorAll('.text-danger').forEach(el => {
            if (!el.classList.contains('validation-summary-errors')) {
                el.remove();
            }
        });
        document.querySelectorAll('.input-error').forEach(el => {
            el.classList.remove('input-error');
        });
    }

    // Real-time password strength indicator
    const passwordInput = document.querySelector('input[name="Password"]');
    if (passwordInput) {
        passwordInput.addEventListener('input', function () {
            const password = this.value;
            const strengthIndicator = document.getElementById('password-strength');

            if (!strengthIndicator) return;

            // Reset strength indicator
            strengthIndicator.textContent = '';
            strengthIndicator.className = 'password-strength';

            if (password.length === 0) return;

            // Calculate password strength
            let strength = 0;
            if (password.length >= 8) strength += 1;
            if (/[A-Z]/.test(password)) strength += 1;
            if (/[a-z]/.test(password)) strength += 1;
            if (/[0-9]/.test(password)) strength += 1;
            if (/[^A-Za-z0-9]/.test(password)) strength += 1;

            // Update strength indicator
            const strengthText = ['Very Weak', 'Weak', 'Medium', 'Strong', 'Very Strong'][strength - 1] || '';
            const strengthClass = ['very-weak', 'weak', 'medium', 'strong', 'very-strong'][strength - 1] || '';

            strengthIndicator.textContent = strengthText;
            strengthIndicator.classList.add(strengthClass);
        });
    }
});