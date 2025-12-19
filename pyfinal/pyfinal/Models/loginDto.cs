namespace pyfinal.Models
{
    public class LoginDto
    {
        // Solo pedimos lo necesario para entrar
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}