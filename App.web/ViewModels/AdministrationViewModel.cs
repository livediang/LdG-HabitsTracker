using App.web.Models;

namespace App.web.ViewModels
{
    public class AdministrationViewModel
    {
        public List<User> Users { get; set; } = new List<User>();
        public string SearchTerm { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}

