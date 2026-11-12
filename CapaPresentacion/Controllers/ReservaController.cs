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
    [AutorizarRol("Cliente")]
    public class ReservaController : Controller
    {
        private readonly ReservaNegocio negocio = new ReservaNegocio();
        private readonly HabitacionNegocio habNegocio = new HabitacionNegocio();

        [HttpGet]
        public ActionResult Buscar(DateTime? entrada, DateTime? salida, int? idTipo)
        {
            var m = new BuscarViewModel
            {
                Entrada = entrada,
                Salida = salida,
                IdTipo = idTipo,
                Tipos = habNegocio.TiposParaCombo(0)
            };

            if (entrada.HasValue && salida.HasValue)
                m.Resultado = negocio.Buscar(entrada.Value, salida.Value, idTipo);
            else if (entrada.HasValue || salida.HasValue)
                m.Resultado = new ResultadoBusqueda { Error = "Indica las dos fechas: entrada y salida." };

            return View(m);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Resumen(DateTime entrada, DateTime salida, int[] habitaciones, int? idTipo)
        {
            Cotizacion c = negocio.Cotizar(entrada, salida, habitaciones);
            if (c.Error != null)
            {
                TempData["Aviso"] = c.Error;
                return RedirectToAction("Buscar", new
                {
                    entrada = entrada.ToString("yyyy-MM-dd"),
                    salida = salida.ToString("yyyy-MM-dd"),
                    idTipo = idTipo
                });
            }

            ViewBag.IdTipo = idTipo;
            return View(c);
        }
    }
}