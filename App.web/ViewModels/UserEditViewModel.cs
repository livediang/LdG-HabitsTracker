using System.ComponentModel.DataAnnotations;

namespace App.web.ViewModels
{
    public class UserEditViewModel
    {
        public Guid UserId { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public bool IsActive { get; set; }

        // Perfil
        public string FullName { get; set; }

        // Roles seleccionados
        public List<Guid> SelectedRoles { get; set; } = new();
    }
}
