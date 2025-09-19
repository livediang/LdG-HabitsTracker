using System.ComponentModel.DataAnnotations;

namespace App.web.ViewModels
{
    public class UserEditViewModel
    {
        public Guid UserId { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        // Perfil
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhotoUrl { get; set; }
        public string TimeZone { get; set; }
        public string Language { get; set; }

        // Roles seleccionados
        public List<Guid> SelectedRoles { get; set; } = new();
    }
}
