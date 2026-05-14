window.theme = {
    changeTheme: function (theme) {
        localStorage.setItem("colorTheme", theme);
        window.theme.updateTheme();
    },
    updateTheme: function () {
        const themeSelectIcon = document.getElementById("themeSelectIcon");
        const hasIcon = themeSelectIcon != null;
        if (hasIcon) {
            themeSelectIcon.classList.remove("bi-circle-half", "bi-sun-fill", "bi-moon-stars-fill", "bi-lightbulb-off-fill");
        }

        const themeSelectLight = document.getElementById("themeSelectLight");
        const themeSelectDark = document.getElementById("themeSelectDark");
        const themeSelectAuto = document.getElementById("themeSelectAuto");
        const themeSelectOled = document.getElementById("themeSelectOled");

        if (hasIcon) {
            themeSelectLight.classList.remove("active");
            themeSelectDark.classList.remove("active");
            themeSelectAuto.classList.remove("active");
            themeSelectOled.classList.remove("active");
        }

        const theme = localStorage.getItem("colorTheme");

        if (theme == "light") {
            document.querySelector("html").setAttribute("data-bs-theme", "light")
            if (hasIcon) {
                themeSelectIcon.classList.add("bi-sun-fill");
                themeSelectLight.classList.add("active");
            }
        }
        else if (theme == "dark") {
            document.querySelector("html").setAttribute("data-bs-theme", "dark")
            if (hasIcon) {
                themeSelectIcon.classList.add("bi-moon-stars-fill");
                themeSelectDark.classList.add("active");
            }
        }
        else if (theme == "oled") {
            document.querySelector("html").setAttribute("data-bs-theme", "oled")
            if (hasIcon) {
                themeSelectIcon.classList.add("bi-lightbulb-off-fill");
                themeSelectOled.classList.add("active");
            }
        }
        else {
            document.querySelector("html").setAttribute("data-bs-theme",
                window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light")
            if (hasIcon) {
                themeSelectIcon.classList.add("bi-circle-half");
                themeSelectAuto.classList.add("active");
            }
        }
    }
}

window.setFocus = (elementId) => {
    const el = document.getElementById(elementId);
    if (el) {
        el.focus();
    }
};

window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', window.theme.updateTheme);
window.theme.updateTheme();