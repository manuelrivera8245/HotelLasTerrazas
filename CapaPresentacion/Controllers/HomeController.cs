using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CapaNegocio;
using CapaPresentacion.Filters;

namespace CapaPresentacion.Controllers
{
    [AutorizarRol]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Sesion = (SesionUsuario)Session["Usuario"];
            return View();
        }
    }
}