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
    [AutorizarRol("Administrador")]
    public class TemporadasController : Controller
    {
        private readonly TemporadaNegocio negocio = new TemporadaNegocio();

        public ActionResult Index()
        {
            ViewBag.Huecos = negocio.Huecos();
            return View(negocio.Listar());
        }

        [HttpGet]
        public ActionResult Crear()
        {
            return View("Formulario", new TemporadaViewModel());
        }

        [HttpGet]
        public ActionResult Editar(int id)
        {
            var t = negocio.Obtener(id);
            if (t == null) return HttpNotFound();

            return View("Formulario", new TemporadaViewModel
            {
                IdTemporada = t.IdTemporada,
                NombreTemporada = t.NombreTemporada,
                FechaInicio = t.FechaInicio,
                FechaFin = t.FechaFin
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guardar(TemporadaViewModel m)
        {
            if (!ModelState.IsValid) return View("Formulario", m);

            string error = negocio.Guardar(m.IdTemporada, m.NombreTemporada,
                                           m.FechaInicio.Value, m.FechaFin.Value);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                return View("Formulario", m);
            }

            TempData["Ok"] = m.IdTemporada == 0 ? "Temporada creada." : "Cambios guardados.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id)
        {
            string error = negocio.Eliminar(id);
            if (error != null) TempData["Aviso"] = error;
            else TempData["Ok"] = "Temporada eliminada.";
            return RedirectToAction("Index");
        }
    }
}