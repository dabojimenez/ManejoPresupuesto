namespace ManejoPresupuesto.Models
{
    //clase, que usaremos para actualizar una transaccion
    public class TransaccionActualizacionViewModel: TransaccionCreacionViewModel
    {
        public int CuentaAnteriorId { get; set; }
        public decimal MontoAnterior { get; set; }
        //se agrega la propiedad de url para retonar al usuario a la pagina donde se encontraba
        public string urlRetorno { get; set; }
    }
}
