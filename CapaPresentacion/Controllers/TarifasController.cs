using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CapaNegocio;
using CapaPresentacion.Filters;
using CapaPresentacion.ViewModels;
using System.Globalization;

namespace CapaPresentacion.Controllers
{
    [AutorizarRol("Administrador")]
    public class TarifasController : Controller
    {
        private readonly TarifaNegocio negocio = new TarifaNegocio();
        private readonly TemporadaNegocio temporadas = new TemporadaNegocio();

        public ActionResult Index()
        {
            return View(negocio.Matriz());
        }

        [HttpGet]
        public ActionResult Editar(int id)
        {
            var m = new TarifasTemporadaViewModel { IdTemporada = id };
            if (!Completar(m)) return HttpNotFound();

            m.Items = negocio.PreciosDeTemporada(id)
                .Select(f => new PrecioTipoItem
                {
                    IdTipoHabitacion = f.IdTipoHabitacion,
                    NombreTipo = f.NombreTipo,
                    Precio = f.Precio.HasValue ? f.Precio.Value.ToString("0.00", CultureInfo.InvariantCulture) : ""
                })
                .ToList();
            return View(m);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guardar(TarifasTemporadaViewModel m)
        {
            if (!Completar(m)) return HttpNotFound();

            var precios = new Dictionary<int, decimal?>();
            for (int i = 0; i < m.Items.Count; i++)
            {
                var it = m.Items[i];
                string txt = (it.Precio ?? "").Trim();
                if (txt.Length == 0)
                {
                    precios[it.IdTipoHabitacion] = null;
                    continue;
                }

                decimal p;
                if (!decimal.TryParse(txt.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out p))
                {
                    ModelState.AddModelError("Items[" + i + "].Precio", "Escribe un número válido.");
                    continue;
                }
                precios[it.IdTipoHabitacion] = p;
            }

            if (!ModelState.IsValid) return View("Editar", m);

            string error = negocio.GuardarPrecios(m.IdTemporada, precios);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                return View("Editar", m);
            }

            TempData["Ok"] = "Precios guardados.";
            return RedirectToAction("Index");
        }

        private bool Completar(TarifasTemporadaViewModel m)
        {
            var t = temporadas.Obtener(m.IdTemporada);
            if (t == null) return false;

            m.NombreTemporada = t.NombreTemporada;
            m.Rango = "Del " + t.FechaInicio.ToString("dd/MM/yyyy") + " al " + t.FechaFin.ToString("dd/MM/yyyy");
            return true;
        }
    }
}
