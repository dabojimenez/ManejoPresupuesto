using ManejoPresupuesto.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace ManejoPresupuesto.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<Usuario> userManager;
        private readonly SignInManager<Usuario> signInManager;

        //UserManager, clase por la cual podremso crear al usuario
        public UsuariosController(UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registro(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = new Usuario()
            {
                Email = model.Email
            };
            //aqui creamos al usuario, enviandole el usuario y el password
            var resultado = await userManager.CreateAsync(usuario, password: model.Password);

            //verificamso el resultado
            if (resultado.Succeeded)
            {
                //pasamos el usuario y la persistencia (isPersistent), que es basicamente que asi este cerrado el navegador
                //el usuario va seguir autenticado en la aplicacion web
                await signInManager.SignInAsync(usuario, isPersistent: true);
                return RedirectToAction("Index", "Transacciones");
            }
            else
            {
                //mostrams los errores al usuario
                foreach (var error in resultado.Errors)
                {
                    //estos son errores al nivel del modelo
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            //deslogueamos a usuario
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            //rederigimos al index, del controlador transacciones
            //return RedirectToAction("Index", "Transacciones");
            return RedirectToAction("Login");
        }

        [HttpGet]
        public  IActionResult Login()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //lockoutOnFailure, es para que cuando el usuario coloca varias veces mal su password, no se le permitira ingresar
            //a la cuenta, pero le enviamso en falso
            var resultado = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.Recuerdame,
                lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return RedirectToAction("Index", "Transacciones");
            }
            else
            {
                //si no es satisfactorio, agregamos una repsuesta, de error
                ModelState.AddModelError(string.Empty, "Nombre de usuario o password incorrecto");
                return View(model);
            }
        }
    }
}
