// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Toogle Function

document.querySelectorAll('.sidebar .nav-link').forEach(link => {
    link.addEventListener('click', () => {
        document.getElementById('nav-collapse-toggle').checked = false;
    });
});

// CRUD Function Partial Views - Administration

// Abrir detalles
$(document).on("click", ".details-btn", function () {
    let id = $(this).data("id");
    $.get("/AccountAdministration/Details/" + id, function (html) {
        $("#actionPanel").html(html);
    });
});

// Editar
$(document).on("click", ".edit-btn", function () {
    let id = $(this).data("id");
    $.get("/AccountAdministration/Edit/" + id, function (html) {
        $("#actionPanel").html(html);
    });
});

// Guardar edición
$(document).on("submit", "#editForm", function (e) {
    e.preventDefault();
    $.post($(this).attr("action"), $(this).serialize(), function (tableHtml) {
        $("#usersTable").html(tableHtml);
        $("#actionPanel").html("<p>Usuario actualizado.</p>");
    });
});

// Eliminar
$(document).on("click", ".delete-btn", function () {
    let id = $(this).data("id");
    $.get("/AccountAdministration/Delete/" + id, function (html) {
        $("#actionPanel").html(html);
    });
});

$(document).on("submit", "#deleteForm", function (e) {
    e.preventDefault();
    $.post($(this).attr("action"), $(this).serialize(), function (tableHtml) {
        $("#usersTable").html(tableHtml);
        $("#actionPanel").html("<p>Usuario eliminado.</p>");
    });
});
