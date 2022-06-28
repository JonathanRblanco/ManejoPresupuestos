using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestos.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El campo {0} es REQUERIDO")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo electronico valido")]
        public string Email { get; set; }
        [Required(ErrorMessage = "El campo {0} es REQUERIDO")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
