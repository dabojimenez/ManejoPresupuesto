using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class Transaccion
    {
        public int Id { get; set; }
        public int UsurioId { get; set; }
        [Display(Name = "Fecha Transaccion")]
        //indicamos que es solo de tipo fecha sin la hora, con DataType.Date
        [DataType(DataType.Date)]
        //solo mostraremso, la fecha ocn horas y minutos, para eso comentamos (DateTime.Now;)
        // (yyyy-MM-dd hh:MM tt) ===== g   ====> (DateTime.Parse(DateTime.Now.ToString("g"));)
        public DateTime FechaTransaccion { get; set; } = DateTime.Today;
        public decimal Monto { get; set; }
        [Display(Name = "Categoria")]
        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una Categoria")]
        public int CategoriaId { get; set; }
        [StringLength(maximumLength: 1000, ErrorMessage = "La Nota, no puede pasar de {1} caracteres")]
        public string Nota { get; set; }
        [Range(1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una Categoria")]
        [Display(Name = "Cuenta")]
        public int CuentaId { get; set; }
    }
}
