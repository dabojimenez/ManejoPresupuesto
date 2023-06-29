using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IReposotorioCuentas reposotorioCuentas;
        private readonly IRepositorioCategorias repositorioCategorias;

        public TransaccionesController(IServicioUsuarios servicioUsuarios,
            IReposotorioCuentas reposotorioCuentas,
            IRepositorioCategorias repositorioCategorias)
        {
            this.servicioUsuarios = servicioUsuarios;
            this.reposotorioCuentas = reposotorioCuentas;
            this.repositorioCategorias = repositorioCategorias;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Crear()
        {
            var usuarioid = servicioUsuarios.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioid);
            modelo.Categorias = await ObtenerCategorias(usuarioid, modelo.TipoOperacionId);
            return View(modelo);
        }
        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await reposotorioCuentas.Buscar(usuarioId);
            //realziamos una proyeccion
            return cuentas.Select(x => new SelectListItem { 
                Text = x.Nombre, 
                Value = x.Id.ToString() 
            });
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioid = servicioUsuarios.ObtenerUsuarioId();
            var categorias = await ObtenerCategorias(usuarioid, tipoOperacion);
            return Ok(categorias);
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioid, TipoOperacion tipoOperacion)
        {
            var categorias = await repositorioCategorias.Obtener(usuarioid, tipoOperacion);
            return categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
    }
}
