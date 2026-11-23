using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CapaNegocio;

namespace CapaPresentacion.Controllers
{
    public class InicioController : Controller
    {
        private readonly LandingNegocio negocio = new LandingNegocio();

        public ActionResult Index()
        {
            if (Session["Usuario"] != null) return RedirectToAction("Index", "Home");
            return View(negocio.Tipos());
        }
    }
}