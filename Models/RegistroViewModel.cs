using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestos.Models
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage ="El campo {0} es REQUERIDO")]
        [EmailAddress(ErrorMessage ="Debe ingresar un correo electronico valido")]
        public string Email { get; set; }
        [Required(ErrorMessage = "El campo {0} es REQUERIDO")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
