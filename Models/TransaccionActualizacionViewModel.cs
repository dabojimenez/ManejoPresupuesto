namespace ManejoPresupuesto.Models
{
    //clase, que usaremos para actualizar una transaccion
    public class TransaccionActualizacionViewModel: TransaccionCreacionViewModel
    {
        public int CuentaAnteriorId { get; set; }
        public decimal MontoAnterior { get; set; }
    }
}
