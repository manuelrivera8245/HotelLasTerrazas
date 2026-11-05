using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CapaNegocio;
using CapaPresentacion.ViewModels;

namespace CapaPresentacion.Controllers
{
    public class CuentaController : Controller
    {
        private readonly UsuarioNegocio negocio = new UsuarioNegocio();

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (Session["Usuario"] != null) return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel modelo, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(modelo);
            }

            SesionUsuario sesion = negocio.ValidarLogin(modelo.Correo.Trim(), modelo.Clave);
            if (sesion == null)
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                ViewBag.ReturnUrl = returnUrl;
                return View(modelo);
            }

            Session.Clear();
            Session["Usuario"] = sesion;

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }
    }
}