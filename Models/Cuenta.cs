using ManejoPresupuestos.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestos.Models
{
    public class Cuenta
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50, ErrorMessage = "El campo {0} debe tener un maximo de {1} caracteres")]
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; }
        [Display(Name = "Tipo Cuenta")]
        public int tipoCuentaId { get; set; }
        public decimal Balance { get; set; }
        [StringLength(maximumLength:1000)]
        public string Descripcion { get; set; }
        public string TipoCuenta { get; set; }
    }
}
