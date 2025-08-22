// user-validation.js
document.addEventListener('DOMContentLoaded', function () {
    initializeUserFormValidation();
    initializeAlertHandling();
    initializePageSpecificFeatures();
});

function initializeUserFormValidation() {
    // Focus vào field đầu tiên
    focusFirstInput();
    // Setup email validation
    setupEmailValidation();
    // Setup password validation
    setupPasswordValidation();
    // Setup form submission validation
    setupFormValidation();
}

function initializeAlertHandling() {
    // Auto hide alerts after 5 seconds
    setTimeout(function () {
        const alerts = document.querySelectorAll('.alert');
        alerts.forEach(function (alert) {
            if (alert.classList.contains('alert-success') || alert.classList.contains('alert-info')) {
                fadeOutAlert(alert);
            }
        });
    }, 5000);
}

function initializePageSpecificFeatures() {
    // Details page functions
    setupDetailsPageFunctions();
    // Delete page extra confirmations
    setupDeletePageConfirmations();
}

function focusFirstInput() {
    const firstInput = document.querySelector('input[type="text"]');
    if (firstInput) {
        firstInput.focus();
    }
}

function setupEmailValidation() {
    const emailInputs = document.querySelectorAll('input[type="email"]');

    emailInputs.forEach(function (emailInput) {
        const errorElement = findErrorElement(emailInput, 'emailError');

        emailInput.addEventListener('blur', function () {
            validateEmail(this, errorElement);
        });

        emailInput.addEventListener('input', function () {
            // Clear error khi user đang gõ
            if (this.classList.contains('is-invalid')) {
                validateEmail(this, errorElement);
            }
        });
    });
}

function validateEmail(emailInput, errorElement) {
    const email = emailInput.value.trim();
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (email && !emailRegex.test(email)) {
        showFieldError(emailInput, errorElement, 'Vui lòng nhập email hợp lệ');
        return false;
    } else {
        hideFieldError(emailInput, errorElement);
        return true;
    }
}

function setupPasswordValidation() {
    const passwordInputs = document.querySelectorAll('input[name="password"]');

    passwordInputs.forEach(function (passwordInput) {
        const errorElement = findErrorElement(passwordInput, 'passwordError');

        passwordInput.addEventListener('input', function () {
            validatePassword(this, errorElement);
        });
    });
}

function validatePassword(passwordInput, errorElement) {
    const password = passwordInput.value;

    if (password.length > 0 && password.length < 6) {
        showFieldError(passwordInput, errorElement, 'Mật khẩu phải có ít nhất 6 ký tự');
        return false;
    } else {
        hideFieldError(passwordInput, errorElement);
        return true;
    }
}

function setupFormValidation() {
    const forms = document.querySelectorAll('form');

    forms.forEach(function (form) {
        // Skip delete forms
        if (form.getAttribute('asp-action') === 'Delete') return;

        form.addEventListener('submit', function (e) {
            if (!validateForm(this)) {
                e.preventDefault();
            }
        });
    });
}

function validateForm(form) {
    let isValid = true;

    // Validate required fields
    const requiredFields = form.querySelectorAll('[required]');
    requiredFields.forEach(function (field) {
        if (!field.value.trim()) {
            field.classList.add('is-invalid');
            isValid = false;
        } else {
            field.classList.remove('is-invalid');
        }
    });

    // Validate email
    const emailInputs = form.querySelectorAll('input[type="email"]');
    emailInputs.forEach(function (emailInput) {
        const errorElement = findErrorElement(emailInput, 'emailError');
        if (!validateEmail(emailInput, errorElement)) {
            isValid = false;
        }
    });

    // Validate password
    const passwordInputs = form.querySelectorAll('input[name="password"]');
    passwordInputs.forEach(function (passwordInput) {
        const errorElement = findErrorElement(passwordInput, 'passwordError');
        if (passwordInput.value && !validatePassword(passwordInput, errorElement)) {
            isValid = false;
        }
    });

    return isValid;
}

function setupDetailsPageFunctions() {
    // View order history function
    window.viewOrderHistory = function (userId) {
        alert('Chức năng xem lịch sử đơn hàng sẽ được triển khai sau');
        // window.location.href = '/Orders?userId=' + userId;
    };

    // Reset password function
    window.resetPassword = function (userId) {
        if (confirm('Bạn có chắc chắn muốn đặt lại mật khẩu cho người dùng này?')) {
            alert('Chức năng đặt lại mật khẩu sẽ được triển khai sau');
        }
    };
}

function setupDeletePageConfirmations() {
    const deleteButtons = document.querySelectorAll('button[type="submit"]');
    const currentUrl = window.location.pathname;

    if (currentUrl.includes('/Delete')) {
        deleteButtons.forEach(function (button) {
            const form = button.closest('form');
            if (form && form.getAttribute('asp-action') === 'Delete') {
                // Auto focus on cancel button for safety
                const cancelButton = document.querySelector('.btn-secondary');
                if (cancelButton) {
                    cancelButton.focus();
                }

                // Check if this is admin user deletion
                const isAdminUser = document.querySelector('.badge-danger') &&
                    document.querySelector('.badge-danger').textContent.includes('Admin');

                if (isAdminUser) {
                    button.addEventListener('click', function (e) {
                        e.preventDefault();
                        if (confirm('ĐÂY LÀ TÀI KHOẢN ADMIN! Bạn có thực sự muốn xóa?')) {
                            if (confirm('Lần xác nhận cuối cùng: XÓA TÀI KHOẢN ADMIN?')) {
                                form.submit();
                            }
                        }
                    });
                }
            }
        });
    }
}

// Helper functions
function findErrorElement(input, defaultId) {
    // Try to find error element by ID
    let errorElement = document.getElementById(defaultId);

    // If not found, try to find by validation span near the input
    if (!errorElement) {
        const validationSpan = input.parentElement.querySelector('.text-danger');
        if (validationSpan) {
            errorElement = validationSpan;
        }
    }

    // If still not found, create one
    if (!errorElement) {
        errorElement = document.createElement('span');
        errorElement.className = 'text-danger';
        errorElement.style.display = 'none';
        input.parentElement.appendChild(errorElement);
    }

    return errorElement;
}

function showFieldError(input, errorElement, message) {
    input.classList.add('is-invalid');
    if (errorElement) {
        errorElement.textContent = message;
        errorElement.style.display = 'block';
    }
}

function hideFieldError(input, errorElement) {
    input.classList.remove('is-invalid');
    if (errorElement) {
        errorElement.style.display = 'none';
    }
}

function fadeOutAlert(alert) {
    alert.style.transition = 'opacity 0.5s ease';
    alert.style.opacity = '0';
    setTimeout(function () {
        if (alert.parentNode) {
            alert.parentNode.removeChild(alert);
        }
    }, 500);
}