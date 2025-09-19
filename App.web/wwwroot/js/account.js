/* Modal Function */

document.addEventListener("DOMContentLoaded", () => {
    const modalTitle = document.getElementById("modalTitle");
    const modalBody = document.getElementById("modalBody");
    const btnContinuar = document.getElementById("btnContinuar");

    // Razor fields inyected 
    const message = document.body.dataset.message;
    const messageType = document.body.dataset.messagetype;

    if (message) {
        modalTitle.innerText = messageType || "Message";
        modalBody.innerText = message;

        const modal = new bootstrap.Modal(document.getElementById("messageModal"));
        modal.show();

        btnContinuar.addEventListener("click", () => {
            if (messageType === "success") {
                window.location.href = "/Home/Index";
            }
        });
    }
});