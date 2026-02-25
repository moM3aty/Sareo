document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form");
    const userNameInput = document.getElementById("userName");
    const passwordInput = document.getElementById("password");
    const confirmPasswordInput = document.getElementById("confirmPassword");

    const patterns = {
        userName: /^[\u0621-\u064Aa-zA-Z\s]{3,}$/, 
        password: /^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&]).{8,}$/ 
    };

    form.addEventListener("submit", function (e) {
        e.preventDefault();
        let isValid = true;

        clearErrors();

        // Validate userName
        if (userNameInput.value.trim() === "") {
            showError(userNameInput, "هذا الحقل مطلوب");
            isValid = false;
        } else if (!patterns.userName.test(userNameInput.value.trim())) {
            showError(userNameInput, "يجب إدخال اسم صحيح (3 أحرف على الأقل)");
            isValid = false;
        }

        // Validate password
        if (passwordInput.value.trim() === "") {
            showError(passwordInput, "هذا الحقل مطلوب");
            isValid = false;
        } else if (!patterns.password.test(passwordInput.value.trim())) {
            showError(passwordInput, "يجب أن يحتوي الرقم السري على 8 أحرف على الأقل، وحرف ورقم ورمز خاص واحد على الأقل");
            isValid = false;
        }

        // Validate confirm password
        if (confirmPasswordInput.value.trim() === "") {
            showError(confirmPasswordInput, "هذا الحقل مطلوب");
            isValid = false;
        } else if (confirmPasswordInput.value.trim() !== passwordInput.value.trim()) {
            showError(confirmPasswordInput, "تأكيد الرقم السري غير مطابق");
            isValid = false;
        }

        if (isValid) {
            form.submit();
        }
    });

    [userNameInput, passwordInput, confirmPasswordInput].forEach(input => {
        input.addEventListener("input", function () {
            clearError(input);

            if (input.value.trim() === "") {
                showError(input, "هذا الحقل مطلوب");
            } else if (input.id === "userName" && !patterns.userName.test(input.value.trim())) {
                showError(input, "يجب إدخال اسم صحيح (3 أحرف على الأقل)");
            } else if (input.id === "password" && !patterns.password.test(input.value.trim())) {
                showError(input, "يجب أن يحتوي الرقم السري على 8 أحرف على الأقل، وحرف ورقم ورمز خاص واحد على الأقل");
            } else if (input.id === "confirmPassword") {
                if (input.value.trim() !== passwordInput.value.trim()) {
                    showError(input, "تأكيد الرقم السري غير مطابق");
                }
            }
        });
    });

    document.querySelectorAll(".toggle-password").forEach(icon => {
        icon.addEventListener("click", function () {
            togglePassword(this);
        });
    });

    function showError(input, message) {
        const parent = input.closest(".co-12") || input.closest(".password-field");
        const errorMsg = parent.querySelector(".error-msg");
        if (errorMsg) {
            errorMsg.textContent = message;
        }
    }

    function clearError(input) {
        const parent = input.closest(".co-12") || input.closest(".password-field");
        const errorMsg = parent.querySelector(".error-msg");
        if (errorMsg) {
            errorMsg.textContent = "";
        }
    }

    function clearErrors() {
        document.querySelectorAll(".error-msg").forEach(msg => msg.textContent = "");
    }
});

function togglePassword(icon) {
    const inputId = icon.getAttribute("data-input");
    const input = document.getElementById(inputId);

    if (input.type === "password") {
        input.type = "text";
    } else {
        input.type = "password";
    }

    icon.classList.toggle("fa-eye");
    icon.classList.toggle("fa-eye-slash");

    input.style.direction = 'rtl';
    input.style.textAlign = 'right';
}
