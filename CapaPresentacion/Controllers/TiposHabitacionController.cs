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
    public class TiposHabitacionController : Controller
    {
        private readonly TipoHabitacionNegocio negocio = new TipoHabitacionNegocio();

        public ActionResult Index()
        {
            return View(negocio.Listar());
        }

        [HttpGet]
        public ActionResult Crear()
        {
            return View("Formulario", new TipoHabitacionViewModel { Capacidad = 1, Estado = true });
        }

        [HttpGet]
        public ActionResult Editar(int id)
        {
            var t = negocio.Obtener(id);
            if (t == null) return HttpNotFound();

            return View("Formulario", new TipoHabitacionViewModel
            {
                IdTipoHabitacion = t.IdTipoHabitacion,
                NombreTipo = t.NombreTipo,
                Capacidad = t.Capacidad,
                Descripcion = t.Descripcion,
                Estado = t.Estado
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guardar(TipoHabitacionViewModel m)
        {
            if (!ModelState.IsValid) return View("Formulario", m);

            string error = negocio.Guardar(m.IdTipoHabitacion, m.NombreTipo, m.Capacidad, m.Descripcion, m.Estado);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                return View("Formulario", m);
            }

            TempData["Ok"] = m.IdTipoHabitacion == 0 ? "Tipo de habitación creado." : "Cambios guardados.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id)
        {
            string error = negocio.Eliminar(id);
            if (error != null) TempData["Aviso"] = error;
            else TempData["Ok"] = "Tipo de habitación eliminado.";
            return RedirectToAction("Index");
        }
    }
}
