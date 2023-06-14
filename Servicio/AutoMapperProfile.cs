using AutoMapper;
using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Servicio
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Cuenta, CuentaCreacionViewModel>();
        }
    }
}
