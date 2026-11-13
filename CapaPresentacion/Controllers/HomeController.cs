using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CapaNegocio;
using CapaPresentacion.Filters;
using CapaPresentacion.ViewModels;

namespace CapaPresentacion.Controllers
{
    [AutorizarRol]
    public class HomeController : Controller
    {
        private readonly PanelNegocio panel = new PanelNegocio();

        public ActionResult Index()
        {
            var s = (SesionUsuario)Session["Usuario"];
            var m = new PanelViewModel { Sesion = s, Hoy = DateTime.Today };

            switch (s.Rol)
            {
                case "Administrador":
                    m.Admin = panel.Admin();
                    break;
                case "Recepcionista":
                    m.Recepcion = panel.Recepcion();
                    break;
                case "Cliente":
                    m.Cliente = panel.Cliente(s.IdCliente ?? 0);
                    break;
            }
            return View(m);
        }
    }
}