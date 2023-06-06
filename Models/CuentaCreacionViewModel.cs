using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Models
{
    public class CuentaCreacionViewModel: Cuenta
    {
        //SelectListItem, clase especial de asp.net core, que nos permite crear selects de una manera muy sencilla
        public IEnumerable<SelectListItem> TiposCuentas { get; set; }
    }
}
