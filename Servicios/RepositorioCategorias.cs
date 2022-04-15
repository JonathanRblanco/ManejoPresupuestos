using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Servicios
{
    public interface IRepositorioCategoria
    {
        Task Actualizar(Categoria categoria);
        Task Borrar(int id);
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId);
        Task<Categoria> ObtenerPorId(int id, int usuarioId);
    }
    public class RepositorioCategorias : IRepositorioCategoria
    {
        private readonly string connectionString;
        public RepositorioCategorias(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("INSERT INTO Categorias (Nombre,TipoOperacionId,UsuarioId) VALUES (@Nombre,@TipoOperacionId,@UsuarioId) SELECT SCOPE_IDENTITY()", categoria);

            categoria.Id = id;
        }
        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>("SELECT * FROM Categorias WHERE UsuarioId = @UsuarioId", new { UsuarioId = usuarioId }); 
        }
        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId,TipoOperacion tipoOperacionId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>(@"SELECT * FROM Categorias 
            WHERE UsuarioId = @UsuarioId AND TipoOperacionId = @tipoOperacionId",
            new { usuarioId,tipoOperacionId});
        }

        public async Task<Categoria> ObtenerPorId(int id,int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Categoria>(@$"SELECT * FROM Categorias WHERE Id=@id AND UsuarioId = @usuarioId", new { id, usuarioId });
        }
        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("UPDATE Categorias SET Nombre = @Nombre, TipoOperacionId = @TipoOperacionId WHERE Id = @Id", categoria);
        }
        public async Task Borrar(int id) 
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE Categorias WHERE Id = @Id", new { Id = id });
        }
    } 
}
