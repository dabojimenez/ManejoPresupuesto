using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class TipoCuenta
    {
        public int Id { get; set; }
        //con {0}, colocamos el nombre del campo o propiedad, en este caso Nombre
        [Required(ErrorMessage = "El campo {0} es Requerido")]
        [StringLength(maximumLength: 50, MinimumLength = 5, ErrorMessage = "Longitud del campo {0} debe estar entre [{2} - {1}]")]
        [Display(Name = "Nombre del tipo Cuenta")]
        public string? Nombre { get; set; }
        public int UsuarioId { get; set; }
        public int Orden { get; set; }

        /*Pruebas de otras validaciones por defecto*/
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [EmailAddress(ErrorMessage = "El campo debe ser un correo electronico valido")]
        public string? Email { get; set; }
        [Range(minimum:18, maximum: 130, ErrorMessage = "Valor entre [{1} - {2}]")]
        public int Edad { get; set; }
        [Url(ErrorMessage = "El campo {0} debe ser una URL valida")]
        public string? URL { get; set; }
        [CreditCard(ErrorMessage = "La tarjeta de credito no es valida")]
        [Display(Name = "Tarjeta de Credito")]
        public string? TarjetaDeCredito { get; set; }
    }
}
