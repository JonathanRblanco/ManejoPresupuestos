using System.Security.Claims;

namespace ManejoPresupuestos.Servicios
{

    public interface IServicioUsuarios
    {
        int ObtenerUsuarioId();
    }
    public class ServicioUsuarios:IServicioUsuarios
    {
        private readonly HttpContext httpContextAccessor;

        public ServicioUsuarios(IHttpContextAccessor _httpContextAccessor)
        {
            httpContextAccessor = _httpContextAccessor.HttpContext;
        }
        public int ObtenerUsuarioId()
        {
            if (httpContextAccessor.User.Identity.IsAuthenticated)
            {
                var idClaim = int.Parse(httpContextAccessor.User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value);
                return idClaim;
            }
            else
            {
                throw new ApplicationException("El usuario no esta autenticado");
            }
        }
        
    }
}
