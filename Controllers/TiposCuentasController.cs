using Dapper;
using ManejoPresupuestos.Models;
using ManejoPresupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Controllers
{
    public class TiposCuentasController:Controller
    {
        private readonly IRepositorioTiposCuentas _repositorioTiposCuentas;
        private readonly IServicioUsuarios servicioUsuarios;

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas,IServicioUsuarios servicioUsuarios)
        {
            _repositorioTiposCuentas = repositorioTiposCuentas;
            this.servicioUsuarios = servicioUsuarios;
        }
        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await _repositorioTiposCuentas.Obtener(usuarioId);
            return View(tiposCuentas);
        }
        public IActionResult Crear()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }
            tipoCuenta.UsuarioId = servicioUsuarios.ObtenerUsuarioId();

            var yaExisteTipoCuenta = await _repositorioTiposCuentas.Existe(tipoCuenta.Nombre,tipoCuenta.UsuarioId);
            if (yaExisteTipoCuenta)
            {
                ModelState.AddModelError("Nombre", "El tipo de cuenta ya existe");
                return View(tipoCuenta);
            }
            await _repositorioTiposCuentas.Crear(tipoCuenta);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var yaExisteTipoCuenta = await _repositorioTiposCuentas.Existe(nombre, usuarioId);
            if (yaExisteTipoCuenta)
            {
                return Json($"El nombre {nombre} ya existe");
            }
            return Json(true);
        }
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId=servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await _repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado","Home"); ;
            }
            return View(tipoCuenta);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(TipoCuenta tipoCuenta)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuentaExiste = await _repositorioTiposCuentas.ObtenerPorId(tipoCuenta.Id, usuarioId);
            if(tipoCuentaExiste is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await _repositorioTiposCuentas.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await _repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if(tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(tipoCuenta);
        }
        [HttpPost]
        public async Task<IActionResult> BorrarTipoCuenta(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await _repositorioTiposCuentas.ObtenerPorId(id, usuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await _repositorioTiposCuentas.Borrar(id);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await _repositorioTiposCuentas.Obtener(usuarioId);
            var idsTiposCuentas = tiposCuentas.Select(x => x.Id);
            var idsTiposCuentasNoPertenecenAlUsuario = ids.Except(idsTiposCuentas).ToList();

            if (idsTiposCuentasNoPertenecenAlUsuario.Count > 0)
            {
                return Forbid();
            }
            var tiposCuentasOrdenados = ids.Select((valor, indice) => new TipoCuenta() { Id = valor, Orden = indice + 1 }).AsEnumerable();
            await _repositorioTiposCuentas.Ordenar(tiposCuentasOrdenados);
            return Ok();
        }
    }
}
