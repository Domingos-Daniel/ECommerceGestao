using System.ComponentModel.DataAnnotations;

namespace ECommerceGestao.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Lembrar de mim?")]
        public bool RememberMe { get; set; }
    }    public class RegisterViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [Display(Name = "Nome completo")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O número de BI é obrigatório")]
        [Display(Name = "Bilhete de Identidade")]
        [RegularExpression(@"^\d{9}[A-Z]{2}\d{3}$", ErrorMessage = "Formato de BI inválido. Use o formato: 123456789LA123")]
        public string IdentityNumber { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O número de telefone é obrigatório")]
        [Display(Name = "Telefone")]
        [RegularExpression(@"^9\d{8}$", ErrorMessage = "Formato de telefone inválido. Use o formato: 9XXXXXXXX")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "O endereço é obrigatório")]
        [Display(Name = "Endereço")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "A cidade é obrigatória")]
        [Display(Name = "Cidade")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "A província é obrigatória")]
        [Display(Name = "Província")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "O código postal é obrigatório")]
        [Display(Name = "Código Postal")]
        public string ZipCode { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar senha")]
        [Compare("Password", ErrorMessage = "A senha e a confirmação de senha não correspondem.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
