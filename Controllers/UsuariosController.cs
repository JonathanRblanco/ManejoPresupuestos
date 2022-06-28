using ManejoPresupuestos.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuestos.Controllers
{
    public class UsuariosController : Controller
    {
        // GET: UsuariosController
        private readonly UserManager<Usuario> UserManager;
        private readonly SignInManager<Usuario> signInManager;

        public UsuariosController(UserManager<Usuario> userManager,SignInManager<Usuario> _signInManager)
        {
            UserManager = userManager;
            signInManager = _signInManager;
        }
        [AllowAnonymous]
        public ActionResult Registro()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Registro(RegistroViewModel registro)
        {
            if (!ModelState.IsValid)
            {
                return View(registro);
            }
            var usuario=new Usuario() { Email = registro.Email };
            var resultado=await UserManager.CreateAsync(usuario,registro.Password);
            if (resultado.Succeeded)
            {
                await signInManager.SignInAsync(usuario, isPersistent: true);
                return RedirectToAction("Index","Transacciones");
            }
            else
            {
                foreach (var item in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Description);
                }
                return View(registro);
            }
        }
        [HttpPost]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Transacciones");
        }
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var resultado=await signInManager.PasswordSignInAsync(model.Email,model.Password,model.RememberMe,lockoutOnFailure:false);
            if (resultado.Succeeded)
            {
                return RedirectToAction("Index", "Transacciones");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Nombre de usuario o password incorrecto");
                return View(model);
            }
        }
    }
}
