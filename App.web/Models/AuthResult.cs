namespace App.web.Models
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public User User { get; set; }
    }
}
