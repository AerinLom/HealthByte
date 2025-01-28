window.onload = function () {
    document.getElementById("sidebar").style.width = "0";
};

function createNewList() {
    alert("Redirecting to list creation...");
}

function viewSavedLists() {
    alert("Viewing saved lists...");
}

function viewPopularProducts() {
    alert("Opening popular products...");
}

function login() {
    alert("Redirecting to login page...");
}

function createAccount() {
    alert("Redirecting to create account page...");
}

function manageAccount() {
    alert("Redirecting to manage account page...");
}

function toggleDropdown() {
    const dropdownMenu = document.getElementById("dropdownMenu");
    if (dropdownMenu.style.display === "block") {
        dropdownMenu.style.display = "none";
    } else {
        dropdownMenu.style.display = "block";
    }
}

function closeSidebar() {
    document.getElementById("sidebar").style.width = "0";
    document.querySelector(".toggle-sidebar").style.display = "inline-block";
}

function openSidebar() {
    document.getElementById("sidebar").style.width = "250px";
    document.querySelector(".toggle-sidebar").style.display = "none";
}