using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [EmailAddress(ErrorMessage = "El campo debe ser un correo electronico válido")]
        public string Email { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        //si el usuario cierra el navegador va a seguir logueado en nuestra aplicacio o de lo contrario
        //nuevamente tiene q ingresar al login, para eso es la implementacion de esta propiedad
        public bool Recuerdame { get; set; }
    }
}
