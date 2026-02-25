document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form");
    const inputs = form.querySelectorAll("input");

    const patterns = {
        userName: /^[\u0621-\u064Aa-zA-Z\s]{3,}$/, 
        password: /^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]{8,}$/ 
    };

    form.addEventListener("submit", function (e) {
        e.preventDefault();
        let isValid = true;

        inputs.forEach(input => {
            const parent = input.closest(".co-12");
            const errorMsg = parent.querySelector(".error-msg");
            errorMsg.textContent = "";

            if (input.value.trim() === "") {
                errorMsg.textContent = "هذا الحقل مطلوب";
                isValid = false;
            } else {
                if (input.id === "userName" && !patterns.userName.test(input.value.trim())) {
                    errorMsg.textContent = "يجب إدخال اسم صحيح (3 أحرف على الأقل)";
                    isValid = false;
                }
                if (input.id === "password" && !patterns.password.test(input.value.trim())) {
                    errorMsg.textContent = "يجب أن يحتوي الرقم السري على 8 أحرف على الأقل، وحرف ورقم ورمز خاص واحد على الأقل";
                    isValid = false;
                }
            }
        });

        if (isValid) {
            form.submit();
        }
    });

    inputs.forEach(input => {
        input.addEventListener("input", function () {
            const parent = input.closest(".co-12");
            const errorMsg = parent.querySelector(".error-msg");
            errorMsg.textContent = "";

            if (input.value.trim() === "") {
                errorMsg.textContent = "هذا الحقل مطلوب";
            } else {
                if (input.id === "userName" && !patterns.userName.test(input.value.trim())) {
                    errorMsg.textContent = "يجب إدخال اسم صحيح (3 أحرف على الأقل)";
                }
                if (input.id === "password" && !patterns.password.test(input.value.trim())) {
                    errorMsg.textContent = "يجب أن يحتوي الرقم السري على 8 أحرف على الأقل، وحرف ورقم ورمز خاص واحد على الأقل";
                }
            }
        });
    });

    document.querySelectorAll(".toggle-password").forEach(icon => {
        icon.addEventListener("click", function () {
            togglePassword(this);
        });
    });
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
