// CRUD Function Partial Views - Administration

// Abrir detalles
$(document).on("click", ".details-btn", function () {
    let id = $(this).data("id");
    $.get("/Administration/Details/" + id, function (html) {
        $("#actionPanel").html(html);
    });
});

// Editar
$(document).on("click", ".edit-btn", function () {
    let id = $(this).data("id");
    $.get("/Administration/Edit/" + id, function (html) {
        $("#actionPanel").html(html);
    });
});

// Guardar edición
$(document).on("submit", "#editForm", function (e) {
    e.preventDefault();

    var form = $(this);

    $.ajax({
        url: form.attr("action"),
        type: form.attr("method"),
        data: form.serialize(),
        success: function (res) {
            if (res.success) {
                $("#usersTable").html(res.html);
                $("#actionPanel").html("<p class='text-success'>✅ User Updated.</p>");
            } else {
                $("#actionPanel").html(res.html);
            }
        }
    });
});

// Eliminar
$(document).on("click", ".delete-btn", function () {
    let id = $(this).data("id");
    $.get("/Administration/Delete/" + id, function (html) {
        $("#actionPanel").html(html);
    });
});

$(document).on("submit", "#deleteForm", function (e) {
    e.preventDefault();
    var form = $(this);

    $.ajax({
        url: form.attr("action"),
        type: form.attr("method"),
        data: form.serialize(),
        success: function (res) {
            if (res.success) {
                $("#usersTable").html(res.html);
                $("#actionPanel").html("<p class='text-success'>✅ User Deleted.</p>");
            } else {
                $("#actionPanel").html("<p class='text-danger'>❌ Error deleting user.</p>");
            }
        }
    });
});

// Crear
$(document).on("click", ".create-btn", function () {
    let id = $(this).data("id");
    $.get("/Administration/Create/" + id, function (html) {
        $("#actionPanel").html(html);
    });
});

// Guardar Usuario
$(document).on("submit", "#createForm", function (e) {
    e.preventDefault();

    var form = $(this);

    $.ajax({
        url: form.attr("action"),
        type: form.attr("method"),
        data: form.serialize(),
        success: function (res) {
            if (res.success) {
                $("#usersTable").html(res.html);
                $("#actionPanel").html("<p class='text-success'>✅ User Registered.</p>");
            } else {
                $("#actionPanel").html(res.html);
            }
        }
    });
});

/* ==== Search Text ==== */

let searchInput = document.getElementById('searchInput');

if (searchInput) {
    let timeout = null;

    searchInput.addEventListener('keyup', function () {
        clearTimeout(timeout);

        let searchTerm = this.value;
        let searchUrl = this.getAttribute('data-url');

        timeout = setTimeout(function () {
            fetch(`${searchUrl}?searchTerm=${encodeURIComponent(searchTerm)}`, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            })
                .then(response => response.text())
                .then(data => {
                    document.getElementById('userTableContainer').innerHTML = data;
                });
        }, 300);
    });
}