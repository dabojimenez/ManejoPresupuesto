using DocumentFormat.OpenXml.Spreadsheet;
using System.Security.Claims;

namespace ManejoPresupuesto.Servicio
{
    public interface IServicioUsuarios
    {
        int ObtenerUsuarioId();
    }
    public class ServicioUsuarios: IServicioUsuarios
    {
        private readonly HttpContext httpContext;

        //usaremos, el httpcontextaccesor, para usar el usuario que hereda controladorbase, que es propio de asp
        public ServicioUsuarios(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContext = httpContextAccessor.HttpContext;
        }
        public int ObtenerUsuarioId()
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                //claims = es una ifnormacion acerca del usuario, por ejemplo su email, nombre, etc
                //por lo que tenemso un conjunto de claims, del cual podemos extrar ifnromacion
                //Ejemplo:
                /**
                 * 
                 * var claims = User.Claims.ToList();
                 * //Type = se refiere a la naturalesa del claim, en este caso ClaimTypes.NameIdentifier
                 * 
                 * //NameIdentifier = es el id del usuario
                 * //obtenemso un claims que contiene el id del usuario
                 * var usuarioIdReal = claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
                 * //ya podemso obtener el valor real del id del usuario
                 * var id = usuarioIdReal.Value;
                */


                var idClaim = httpContext.User.Claims.Where(
                    x => x.Type == ClaimTypes.NameIdentifier)
                    .FirstOrDefault();
                var id = int.Parse(idClaim.Value);
                return id;
            }
            else 
            { 
                throw new ApplicationException("El usuario no está autenticado"); 
            }
        }
    }
}
