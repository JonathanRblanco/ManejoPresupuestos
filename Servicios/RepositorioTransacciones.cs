using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task Crear(Transaccion transaccion);
    }

    public class RepositorioTransacciones:IRepositorioTransacciones
    {
        private readonly string stringConnection;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            stringConnection = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(stringConnection);
            var id = await connection.QuerySingleAsync<int>("Transacciones_Insertar",
                new { transaccion.UsuarioId, transaccion.FechaTransaccion, transaccion.Monto, transaccion.CategoriaId, transaccion.CuentaId, transaccion.Nota },
                commandType: System.Data.CommandType.StoredProcedure);
            transaccion.Id = id;
        }
    }
}
