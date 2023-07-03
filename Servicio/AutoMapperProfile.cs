using AutoMapper;
using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Servicio
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Cuenta, CuentaCreacionViewModel>();
            //ReverseMap, para trasnferir de actualizacion a transaccion y viceversa, de las dos formas
            CreateMap<TransaccionActualizacionViewModel, Transaccion>().ReverseMap();
        }
    }
}
