using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ManejoPresupuestos.Servicios
{
    public interface IRepositorioUsuarios
    {
        Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
        Task<int> CrearUsuario(Usuario usuario);
    }
    public class RepositorioUsuarios:IRepositorioUsuarios
    {
        private readonly string connectionString;
        public RepositorioUsuarios(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<int> CrearUsuario(Usuario usuario)
        {
            using var conection = new SqlConnection(connectionString);
            var UsuarioId = await conection.QuerySingleAsync<int>(@"INSERT INTO Usuarios(Email,EmailNormalizado,PasswordHash)
                                                            VALUES (@Email,@EmailNormalizado,@PasswordHash);
                                                            SELECT SCOPE_IDENTITY();",usuario);
            await conection.ExecuteAsync("CrearDatosUsuarioNuevo",new { UsuarioId },commandType:CommandType.StoredProcedure);
            return UsuarioId;
        }
        public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
        {
            using var conection=new SqlConnection(connectionString);
            return await conection.QuerySingleOrDefaultAsync<Usuario>("SELECT * FROM Usuarios WHERE EmailNormalizado=@emailNormalizado", new { emailNormalizado });
        }
    }
}
