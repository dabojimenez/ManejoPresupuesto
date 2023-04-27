using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Validaciones
{
    public class PrimeraLetraMayusculaAttribute : ValidationAttribute
    {
        //En el value, obtenemos el valor que tiene el campo en el que tiene el atributo
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value == null || string.IsNullOrEmpty(value.ToString()))
            {
                //si no es nullo o vacio, retornaremos lo siguiente, ya que es exitoso
                return ValidationResult.Success;
            }
            //obtenemos la priemra letra del caracter
            var primeraLetra = value.ToString()[0].ToString();
            //Validamos que si la priemra letra es diferente a la priemra letra en mayyusculas, retornara un error
            if (primeraLetra != primeraLetra.ToUpper())
            {
                return new ValidationResult("La primera letra debe ser mayuscula");
            }
            //retornara exitoso si la priemra letra si es mayuscula
            return ValidationResult.Success;
            //return new ValidationResult("La primera letra debe ser mayuscula");
        }
    }
}
