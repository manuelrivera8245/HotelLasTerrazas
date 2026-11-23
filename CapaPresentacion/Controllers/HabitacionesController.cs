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
    public class HabitacionesController : Controller
    {
        private readonly HabitacionNegocio negocio = new HabitacionNegocio();

        public ActionResult Index()
        {
            return View(negocio.Listar());
        }

        [HttpGet]
        public ActionResult Crear()
        {
            var m = new HabitacionViewModel { Piso = 1, IdEstadoHabitacion = EstadoHabitacionId.Disponible };
            CargarListas(m);
            m.IdTipoHabitacion = m.Tipos.Any() ? int.Parse(m.Tipos.First().Value) : 0;
            return View("Formulario", m);
        }

        [HttpGet]
        public ActionResult Editar(int id)
        {
            var h = negocio.Obtener(id);
            if (h == null) return HttpNotFound();

            var m = new HabitacionViewModel
            {
                IdHabitacion = h.IdHabitacion,
                NumeroHabitacion = h.NumeroHabitacion,
                Piso = h.Piso,
                IdTipoHabitacion = h.IdTipoHabitacion,
                IdEstadoHabitacion = h.IdEstadoHabitacion
            };
            CargarListas(m);
            return View("Formulario", m);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guardar(HabitacionViewModel m)
        {
            if (!ModelState.IsValid)
            {
                CargarListas(m);
                return View("Formulario", m);
            }

            string error = negocio.Guardar(m.IdHabitacion, m.NumeroHabitacion, m.Piso,
                                           m.IdTipoHabitacion, m.IdEstadoHabitacion);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                CargarListas(m);
                return View("Formulario", m);
            }

            TempData["Ok"] = m.IdHabitacion == 0 ? "Habitación creada." : "Cambios guardados.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id)
        {
            string error = negocio.Eliminar(id);
            if (error != null) TempData["Aviso"] = error;
            else TempData["Ok"] = "Habitación eliminada.";
            return RedirectToAction("Index");
        }

        private void CargarListas(HabitacionViewModel m)
        {
            m.Tipos = negocio.TiposParaCombo(m.IdTipoHabitacion)
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Texto });
            m.Estados = negocio.EstadosParaCombo()
                .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Texto });
        }
    }
}
