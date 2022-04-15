using ManejoPresupuestos.Models;
using ManejoPresupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuestos.Controllers
{
    public class TransaccionesController:Controller
    {
        private IServicioUsuarios ServicioUsuarios { get; }
        private IRepositorioCuentas RepositorioCuentas { get; }
        private IRepositorioCategoria RepositorioCategorias { get; }

        public TransaccionesController(IRepositorioTransacciones repositorioTransacciones,
            IServicioUsuarios servicioUsuarios,IRepositorioCuentas repositorioCuentas,
            IRepositorioCategoria repositorioCategorias)
        {
            ServicioUsuarios = servicioUsuarios;
            RepositorioCuentas = repositorioCuentas;
            RepositorioCategorias = repositorioCategorias;
        }

        public async Task<IActionResult> Crear()
        {
            var usuarioId =  ServicioUsuarios.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId,modelo.TipoOperacionId);
            return View(modelo);
        }
        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await RepositorioCuentas.Buscar(usuarioId);
            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await RepositorioCategorias.Obtener(usuarioId,tipoOperacion);
            return categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(categorias);
        }
    }
}
