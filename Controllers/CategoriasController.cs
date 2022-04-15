using ManejoPresupuestos.Models;
using ManejoPresupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuestos.Controllers
{
    public class CategoriasController:Controller
    {
        private readonly IRepositorioCategoria repositorioCategorias;
        private readonly IServicioUsuarios ServicioUsuarios;

        public CategoriasController(IRepositorioCategoria repositorioCategoria,IServicioUsuarios servicioUsuarios)
        {
            repositorioCategorias = repositorioCategoria;
            ServicioUsuarios = servicioUsuarios;
        }

        public IActionResult Crear()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            categoria.UsuarioId = usuarioId;
            await repositorioCategorias.Crear(categoria);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Index()
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var categorias = await repositorioCategorias.Obtener(usuarioId);
            return View(categorias);
        }
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategorias.ObtenerPorId(id,usuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado","Home");
            }
            return View(categoria);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(Categoria categoriaEditar) 
        {
            if (!ModelState.IsValid)
            {
                return View(categoriaEditar);
            }
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategorias.ObtenerPorId(categoriaEditar.Id, usuarioId);
            if (categoriaEditar is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
           
            categoriaEditar.UsuarioId = usuarioId;
            await repositorioCategorias.Actualizar(categoriaEditar);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var categoria = await repositorioCategorias.ObtenerPorId(id, usuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(categoria);
        }
        [HttpPost]
        public async Task<IActionResult> BorrarCategoria(int Id)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var categoriaBorrar = await repositorioCategorias.ObtenerPorId(Id, usuarioId);
            if (categoriaBorrar is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioCategorias.Borrar(Id);
            return RedirectToAction("Index");
        }
    }
}
