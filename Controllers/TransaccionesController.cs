using AutoMapper;
using ClosedXML.Excel;
using ManejoPresupuestos.Models;
using ManejoPresupuestos.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace ManejoPresupuestos.Controllers
{
    
    public class TransaccionesController:Controller
    {
        private readonly IServicioReportes servicioReportes;
        private IRepositorioTransacciones RepositorioTransacciones { get; }
        private IServicioUsuarios ServicioUsuarios { get; }
        private IRepositorioCuentas RepositorioCuentas { get; }
        private IRepositorioCategoria RepositorioCategorias { get; }
        private IMapper Mapper { get; }

        public TransaccionesController(IRepositorioTransacciones repositorioTransacciones,
            IServicioUsuarios servicioUsuarios,IRepositorioCuentas repositorioCuentas,
            IRepositorioCategoria repositorioCategorias,IMapper mapper,IServicioReportes _servicioReportes)
        {
            RepositorioTransacciones = repositorioTransacciones;
            ServicioUsuarios = servicioUsuarios;
            RepositorioCuentas = repositorioCuentas;
            RepositorioCategorias = repositorioCategorias;
            Mapper = mapper;
            servicioReportes = _servicioReportes;
        }

        public async Task<IActionResult> Index(int mes, int año)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladas(usuarioId, mes, año, ViewBag);
            return View(modelo);
        }
        public async Task<IActionResult> Semanal(int mes,int año)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            IEnumerable<ResultadoObtenerPorSemana> transaccionesPorSemana= await servicioReportes.ObtenerReporteSemanal(usuarioId,mes,año, ViewBag);
            var agrupado = transaccionesPorSemana.GroupBy(x => x.Semana).Select(x => new ResultadoObtenerPorSemana()
            {
                Semana = x.Key,
                Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
                Gastos= x.Where(x => x.TipoOperacionId == TipoOperacion.Egreso).Select(x => x.Monto).FirstOrDefault()
            }).ToList();
            if(año==0 || mes == 0)
            {
                var hoy = DateTime.Today;
                año=hoy.Year;
                mes=hoy.Month;
            }
            var fechaReferencia = new DateTime(año, mes, 1);
            var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);
            var diasSegmentados = diasDelMes.Chunk(7).ToList();
            for (int i = 0; i < diasSegmentados.Count; i++)
            {
                var semana = i + 1;
                var fechaInicio = new DateTime(año, mes, diasSegmentados[i].First());
                var fechaFin=new DateTime(año,mes, diasSegmentados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);
                if(grupoSemana is null)
                {
                    agrupado.Add(new ResultadoObtenerPorSemana()
                    {
                        Semana = semana,
                        FechaInicio=fechaInicio,
                        FechaFin=fechaFin
                    });
                }
                else
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }
            }
            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();
            var modelo = new ReporteSemanalViewModel();
            modelo.TransaccionesPorSemana=agrupado;
            modelo.FechaReferencia = fechaReferencia;
            return View(modelo);
        }
        public async Task<IActionResult> Mensual(int año)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            if (año == 0)
            {
                año = DateTime.Today.Year;
            }
            var transaccionesPorMes=await RepositorioTransacciones.ObtenerPorMes(usuarioId,año);
            var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x => x.Mes).Select(x => new ResultadoObtenerPorMes()
            {
                Mes = x.Key,
                Ingreso = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
                Gasto=x.Where(x => x.TipoOperacionId == TipoOperacion.Egreso).Select(x => x.Monto).FirstOrDefault()
            }).ToList();
            for (int i = 1; i <= 12; i++)
            {
                var transacciones = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == i);
                var fechaReferencia = new DateTime(año, i, 1);
                if(transacciones is null)
                {
                    transaccionesAgrupadas.Add(new ResultadoObtenerPorMes()
                    {
                        Mes=i,
                        FechaReferencia=fechaReferencia
                    });
                }
                else
                {
                    transacciones.FechaReferencia = fechaReferencia;
                }
            }
            transaccionesAgrupadas=transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();
            var modelo = new ReporteMensualViewModel()
            {
                Año = año,
                TransaccionesPorMes = transaccionesAgrupadas
            };
            return View(modelo);
        }
        public IActionResult ExcelReporte()
        {

            return View();
        }
        public async Task<FileResult> ExportarExcelPorMes(int mes,int año)
        {
            var fechaInicio = new DateTime(año, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
            var usuario = ServicioUsuarios.ObtenerUsuarioId();
            var transacciones = await RepositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuario,
                FechaFin = fechaFin,
                FechaInicio = fechaInicio
            });
            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("MMM yyyy")}.xlsx";
            return GenerarExcel(nombreArchivo, transacciones);
        }
        public async Task<FileResult> ExportarExcelPorAño(int año)
        {
            var fechaInicio = new DateTime(año, 1, 1);
            var fechaFin=fechaInicio.AddYears(1).AddDays(-1);
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var transacciones = await RepositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                FechaFin = fechaFin,
                FechaInicio = fechaInicio,
                UsuarioId = usuarioId
            });
            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("yyyy")}.xlsx";
            return GenerarExcel(nombreArchivo,transacciones);
        }
        public async Task<FileResult> ExportarExcelTodo()
        {
            var fechaInicio = DateTime.Today.AddYears(-100);
            var fechaFin = DateTime.Today.AddYears(1000);
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var transacciones = await RepositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaFin = fechaFin,
                FechaInicio = fechaInicio
            });
            var nombreArchivo = $"Manejo Presupuesto - {DateTime.Today.ToString("dd-MM-yyyy")}.xlsx";
            return GenerarExcel(nombreArchivo, transacciones);
        }

        private FileResult GenerarExcel(string nombre,IEnumerable<Transaccion> transacciones)
        {
            DataTable dataTable= new DataTable("Transacciones");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Fecha"),
                new DataColumn("Cuenta"),
                new DataColumn("Categoria"),
                new DataColumn("Nota"),
                new DataColumn("Monto"),
                new DataColumn("Ingreso/Gasto")
            });
            foreach (var transaccion in transacciones)
            {
                dataTable.Rows.Add(transaccion.FechaTransaccion,
                    transaccion.Cuenta, transaccion.Categoria,
                    transaccion.Nota, transaccion.Monto,
                    transaccion.TipoOperacionId);
            }
            using (XLWorkbook wb=new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);
                using (MemoryStream stream=new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "appliaction/vnd.openxmlformats-officedocument.spreadsheetml.sheet",nombre);
                }
            }
        }

        public IActionResult Calendario()
        {
            return View();
        }
        public async Task<JsonResult> ObtenerTransaccionesCalendario(DateTime start,DateTime end)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var transacciones = await RepositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaFin = end,
                FechaInicio = start
            });
            var eventosCalendario = transacciones.Select(transaccion => new EventoCalendario()
            {
                Title = transaccion.Monto.ToString(),
                Start = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                End = transaccion.FechaTransaccion.ToString("yyy-MM-dd"),
                Color = transaccion.TipoOperacionId == TipoOperacion.Egreso ? "Red" : null
            });
            return Json(eventosCalendario);
        }
        public async Task<JsonResult> ObtenerTransaccionesPorFecha(DateTime fecha)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var transacciones = await RepositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaFin = fecha,
                FechaInicio = fecha
            });
            return Json(transacciones);
        }
        public async Task<IActionResult> Crear()
        {
            var usuarioId =  ServicioUsuarios.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId,modelo.TipoOperacionId);
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionViewModel modelo)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }
            var cuenta = await RepositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var categoria = await RepositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);
            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            modelo.UsuarioId = usuarioId;
            if(modelo.TipoOperacionId == TipoOperacion.Egreso)
            {
                modelo.Monto *= -1;
            }

            await RepositorioTransacciones.Crear(modelo);
            return RedirectToAction("Index");

        }
        public async Task<IActionResult> Editar(int id, string urlRetorno=null)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var transaccion = await RepositorioTransacciones.ObtenerPorId(id, usuarioId);
            if(transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var modelo = Mapper.Map<TransaccionActualizarViewModel>(transaccion);
            modelo.MontoAnterior = modelo.Monto;
            if(modelo.TipoOperacionId == TipoOperacion.Egreso)
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }
            modelo.CuentaAnteriorId = transaccion.CuentaId;
            modelo.Categorias = await ObtenerCategorias(usuarioId,transaccion.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.UrlRetorno=urlRetorno;
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizarViewModel modelo)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }
            var cuenta = await RepositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);
            if(cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var categoria = await RepositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);

            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var transaccion = Mapper.Map<Transaccion>(modelo);
            if (modelo.TipoOperacionId == TipoOperacion.Egreso)
            {
                transaccion.Monto *= -1;
            }

            await RepositorioTransacciones.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorId);
            if (!string.IsNullOrEmpty(modelo.UrlRetorno))
            {
                return LocalRedirect(modelo.UrlRetorno);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id, string urlRetorno = null)
        {
            var usuarioId = ServicioUsuarios.ObtenerUsuarioId();
            var transaccion = await RepositorioTransacciones.ObtenerPorId(id, usuarioId);
            if(transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await RepositorioTransacciones.Borrar(id);
            if (!string.IsNullOrEmpty(urlRetorno))
            {
                return LocalRedirect(urlRetorno);
            }      
            return RedirectToAction("Index");

        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await RepositorioCuentas.Buscar(usuarioId);
            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await RepositorioCategorias.Obtener(usuarioId,tipoOperacion);
            var resultado= categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString())).ToList();
            var opcionPorDefecto = new SelectListItem("-- Seleccione una categoria --", "0", true);
            resultado.Insert(0,opcionPorDefecto);
            return resultado;
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
