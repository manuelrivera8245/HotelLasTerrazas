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

        private readonly PagoNegocio pagos = new PagoNegocio();

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

        [HttpGet]
        public ActionResult Cobrar(int id, string estado, string fecha)
        {
            var m = new CobroViewModel { IdReserva = id, FiltroEstado = estado, FiltroFecha = fecha };
            if (!CompletarCobro(m)) return HttpNotFound();

            m.IdMetodoPago = m.Metodos.Any() ? int.Parse(m.Metodos.First().Value) : 0;
            m.IdTipoComprobante = m.Tipos.Any() ? int.Parse(m.Tipos.First().Value) : 0;
            return View(m);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cobrar(CobroViewModel m)
        {
            if (!CompletarCobro(m)) return HttpNotFound();
            if (m.Bloqueo != null || !ModelState.IsValid) return View(m);

            var r = pagos.Registrar(m.IdReserva, m.IdMetodoPago, m.IdTipoComprobante);
            if (r.Error != null)
            {
                ModelState.AddModelError("", r.Error);
                return View(m);
            }

            TempData["Ok"] = "Pago registrado y comprobante emitido.";
            return RedirectToAction("Comprobante", new { id = r.IdPago });
        }

        [HttpGet]
        public ActionResult Comprobante(int id)
        {
            var d = pagos.Obtener(id, null);
            if (d == null) return HttpNotFound();
            return View(d);
        }

        [HttpGet]
        public ActionResult Pagos(DateTime? desde, DateTime? hasta, int? idMetodo)
        {
            return View(new PagosViewModel
            {
                Desde = desde,
                Hasta = hasta,
                IdMetodo = idMetodo,
                Metodos = pagos.Metodos(),
                Historial = pagos.Historial(desde, hasta, idMetodo)
            });
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

        private bool CompletarCobro(CobroViewModel m)
        {
            var info = pagos.ParaCobrar(m.IdReserva);
            if (info == null) return false;

            m.Cliente = info.Cliente;
            m.Documento = info.Documento;
            m.Fechas = info.Entrada.ToString("dd/MM/yyyy") + " – " + info.Salida.ToString("dd/MM/yyyy");
            m.Habitaciones = info.Habitaciones;
            m.EstadoReserva = info.Estado;
            m.Total = info.Total;
            m.Bloqueo = info.Bloqueo;

            m.Metodos = pagos.Metodos().Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Texto });
            m.Tipos = pagos.Tipos().Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Texto });
            return true;
        }
    }
}