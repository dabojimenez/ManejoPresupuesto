using ManejoPresupuesto.Validaciones;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class TipoCuenta //: IValidatableObject
    {
        public int Id { get; set; }
        //con {0}, colocamos el nombre del campo o propiedad, en este caso Nombre
        [Required(ErrorMessage = "El campo {0} es Requerido")]
        //[StringLength(maximumLength: 50, MinimumLength = 5, ErrorMessage = "Longitud del campo {0} debe estar entre [{2} - {1}]")]
        //[Display(Name = "Nombre del tipo Cuenta")]
        [PrimeraLetraMayuscula]
        [Remote(action: "VerificarExisteTipoCuenta", controller: "TiposCuentas")]
        public string Nombre { get; set; }
        public int UsuarioId { get; set; }
        public int Orden { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (Nombre != null && Nombre.Length > 0)
        //    {
        //        string primeraLetra = Nombre[0].ToString();
        //        if (primeraLetra != primeraLetra.ToUpper())
        //        {
        //            //el priumer argumento que se envia, es el texto del error
        //            //el segundo, es un arreglo indicnaod el campo del nombre que se debe aplicar, ya que 
        //            //saldra el mensaje en el campo deseado
        //            yield return new ValidationResult("Primera letra debe ser mayuscula", new[]
        //            {
        //                nameof(Nombre)
        //            });
        //        }
        //    }
        //}

        ///*Pruebas de otras validaciones por defecto*/
        //[Required(ErrorMessage = "El campo {0} es requerido")]
        //[EmailAddress(ErrorMessage = "El campo debe ser un correo electronico valido")]
        //public string? Email { get; set; }
        //[Range(minimum:18, maximum: 130, ErrorMessage = "Valor entre [{1} - {2}]")]
        //public int Edad { get; set; }
        //[Url(ErrorMessage = "El campo {0} debe ser una URL valida")]
        //public string? URL { get; set; }
        //[CreditCard(ErrorMessage = "La tarjeta de credito no es valida")]
        //[Display(Name = "Tarjeta de Credito")]
        //public string? TarjetaDeCredito { get; set; }
    }
}
