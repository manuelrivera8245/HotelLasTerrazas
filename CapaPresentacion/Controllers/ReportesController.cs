using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CapaNegocio;
using CapaPresentacion.Filters;

namespace CapaPresentacion.Controllers
{
    [AutorizarRol("Administrador")]
    public class ReportesController : Controller
    {
        private readonly ReportesNegocio negocio = new ReportesNegocio();

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Ocupacion(DateTime? desde, DateTime? hasta)
        {
            return View(negocio.Ocupacion(desde ?? IniMes(), hasta ?? FinMes()));
        }

        [HttpGet]
        public ActionResult Ingresos(DateTime? desde, DateTime? hasta)
        {
            return View(negocio.Ingresos(desde ?? IniMes(), hasta ?? FinMes()));
        }

        [HttpGet]
        public ActionResult PorEstado(DateTime? desde, DateTime? hasta, string estado)
        {
            return View(negocio.PorEstado(desde ?? IniMes(), hasta ?? FinMes(), estado));
        }

        private static DateTime IniMes()
        {
            DateTime h = DateTime.Today;
            return new DateTime(h.Year, h.Month, 1);
        }

        private static DateTime FinMes()
        {
            return IniMes().AddMonths(1).AddDays(-1);
        }
    }
}