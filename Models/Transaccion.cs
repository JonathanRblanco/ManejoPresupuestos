using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ManejoPresupuestos.Models
{
    public class Transaccion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        [Display(Name = "Fecha Transacción")]
        [DataType(DataType.Date)]
        public DateTime FechaTransaccion { get; set; } = DateTime.Today;
        public decimal Monto { get; set; }
        [Range(1,int.MaxValue,ErrorMessage ="Debe seleccionar una categoria")]
        [Display(Name = "Categoria")]
        public int CategoriaId { get; set; }
        [StringLength(1000, ErrorMessage = "La nota no puede pasar de {0} caracteres")]
        public string Nota { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una cuenta")]
        [Display(Name = "Cuenta")]
        public int CuentaId { get; set; }
    }
}
