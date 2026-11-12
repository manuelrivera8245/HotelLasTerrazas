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
    [AutorizarRol("Recepcionista")]
    public class RecepcionController : Controller
    {
        private readonly RecepcionNegocio negocio = new RecepcionNegocio();

        [HttpGet]
        public ActionResult Index(string estado, DateTime? fecha)
        {
            return View(new ReservasRecepcionViewModel
            {
                Estado = estado,
                Fecha = fecha,
                Estados = negocio.Estados(),
                Filas = negocio.Listar(estado, fecha)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Confirmar(int id, string estado, string fecha)
        {
            return Resultado(negocio.Confirmar(id), "Reserva confirmada.", estado, fecha);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckIn(int id, string estado, string fecha)
        {
            return Resultado(negocio.CheckIn(id), "Check-in registrado. Las habitaciones pasaron a Ocupada.", estado, fecha);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOut(int id, string estado, string fecha)
        {
            return Resultado(negocio.CheckOut(id), "Check-out registrado. Las habitaciones pasaron a Limpieza.", estado, fecha);
        }

        [HttpGet]
        public ActionResult Cancelar(int id, string estado, string fecha)
        {
            var f = negocio.ObtenerFila(id);
            if (f == null) return HttpNotFound();

            if (!f.PuedeCancelar)
            {
                TempData["Aviso"] = "Solo se pueden cancelar reservas Pendientes o Confirmadas.";
                return Volver(estado, fecha);
            }

            return VistaCancelar(new CancelarRecepcionViewModel
            {
                IdReserva = id,
                FiltroEstado = estado,
                FiltroFecha = fecha
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancelar(CancelarRecepcionViewModel m)
        {
            if (!ModelState.IsValid) return VistaCancelar(m);

            string error = negocio.Cancelar(m.IdReserva, m.Motivo);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                return VistaCancelar(m);
            }

            TempData["Ok"] = "Reserva cancelada.";
            return Volver(m.FiltroEstado, m.FiltroFecha);
        }

        [HttpGet]
        public ActionResult Habitaciones()
        {
            return View(negocio.Habitaciones());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarcarLista(int id)
        {
            string error = negocio.MarcarLista(id);
            if (error != null) TempData["Aviso"] = error;
            else TempData["Ok"] = "Habitación disponible otra vez.";
            return RedirectToAction("Habitaciones");
        }


        private ActionResult VistaCancelar(CancelarRecepcionViewModel m)
        {
            var f = negocio.ObtenerFila(m.IdReserva);
            if (f == null) return HttpNotFound();

            m.Cliente = f.Cliente;
            m.Fechas = f.Entrada.ToString("dd/MM/yyyy") + " – " + f.Salida.ToString("dd/MM/yyyy");
            m.Habitaciones = f.Habitaciones;
            return View("Cancelar", m);
        }

        private ActionResult Resultado(string error, string ok, string estado, string fecha)
        {
            if (error != null) TempData["Aviso"] = error;
            else TempData["Ok"] = ok;
            return Volver(estado, fecha);
        }

        private ActionResult Volver(string estado, string fecha)
        {
            return RedirectToAction("Index", new
            {
                estado = string.IsNullOrEmpty(estado) ? null : estado,
                fecha = string.IsNullOrEmpty(fecha) ? null : fecha
            });
        }
    }
}