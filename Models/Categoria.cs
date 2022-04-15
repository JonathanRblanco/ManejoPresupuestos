using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestos.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50,ErrorMessage = "No puede ser mayor a {1} caracteres")]
        public string Nombre { get; set; }
        [Display(Name = "Tipo Categoría")]
        public TipoOperacion TipoOperacionId { get; set; }
        public int UsuarioId { get; set; }
    }
}
