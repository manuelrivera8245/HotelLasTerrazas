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
        private readonly RegistroNegocio registro = new RegistroNegocio();

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

        [HttpGet]
        public ActionResult Registro()
        {
            if (Session["Usuario"] != null) return RedirectToAction("Index", "Home");
            return View(ConTipos(new RegistroViewModel { TipoDocumento = "DNI" }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registro(RegistroViewModel m)
        {
            if (!ModelState.IsValid) return View(ConTipos(m));

            var r = registro.RegistrarCliente(m.Nombres, m.Apellidos, m.TipoDocumento,
                m.NumeroDocumento, m.Telefono, m.Correo, m.Clave);
            if (!r.Ok)
            {
                ModelState.AddModelError(r.Campo ?? "", r.Error);
                return View(ConTipos(m));
            }

            SesionUsuario sesion = negocio.ValidarLogin(m.Correo.Trim().ToLowerInvariant(), m.Clave);
            if (sesion == null)
            {
                TempData["Ok"] = "Cuenta creada. Ya puedes iniciar sesión.";
                return RedirectToAction("Login");
            }

            Session.Clear();
            Session["Usuario"] = sesion;
            TempData["Ok"] = "Cuenta creada. ¡Bienvenido!";
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Inicio");
        }

        private static RegistroViewModel ConTipos(RegistroViewModel m)
        {
            m.Tipos = new List<SelectListItem>
            {
                new SelectListItem { Value = "DNI", Text = "DNI" },
                new SelectListItem { Value = "CE", Text = "Carné de extranjería" },
                new SelectListItem { Value = "Pasaporte", Text = "Pasaporte" }
            };
            return m;
        }
    }
}