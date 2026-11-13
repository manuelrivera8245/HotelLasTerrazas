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
        private readonly ReservaClienteNegocio clienteNegocio = new ReservaClienteNegocio();

        private readonly PagoNegocio pagos = new PagoNegocio();
        private readonly HabitacionNegocio habNegocio = new HabitacionNegocio();

        private int IdCliente
        {
            get
            {
                var s = Session["Usuario"] as SesionUsuario;
                return (s != null && s.IdCliente.HasValue) ? s.IdCliente.Value : 0;
            }
        }

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
                return VolverABuscar(entrada, salida, idTipo);
            }

            ViewBag.IdTipo = idTipo;
            return View(c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Confirmar(DateTime entrada, DateTime salida, int[] habitaciones, int? idTipo)
        {
            if (IdCliente == 0)
            {
                TempData["Aviso"] = "Tu usuario no tiene un perfil de cliente asociado.";
                return RedirectToAction("Buscar");
            }

            RegistroResultado r = clienteNegocio.Registrar(IdCliente, entrada, salida, habitaciones);
            if (r.Error != null)
            {
                TempData["Aviso"] = r.Error;
                return VolverABuscar(entrada, salida, idTipo);
            }

            return RedirectToAction("Confirmacion", new { id = r.IdReserva });
        }

        [HttpGet]
        public ActionResult Confirmacion(int id)
        {
            var d = clienteNegocio.Obtener(id, IdCliente);
            if (d == null) return HttpNotFound();
            return View(d);
        }

        [HttpGet]
        public ActionResult MisReservas()
        {
            return View(clienteNegocio.MisReservas(IdCliente));
        }

        [HttpGet]
        public ActionResult Constancia(int id)
        {
            var d = pagos.ObtenerPorReserva(id, IdCliente);
            if (d == null) return HttpNotFound();
            return View(d);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancelar(int id)
        {
            string error = clienteNegocio.Cancelar(id, IdCliente);
            if (error != null) TempData["Aviso"] = error;
            else TempData["Ok"] = "Reserva cancelada.";
            return RedirectToAction("MisReservas");
        }

        private ActionResult VolverABuscar(DateTime entrada, DateTime salida, int? idTipo)
        {
            return RedirectToAction("Buscar", new
            {
                entrada = entrada.ToString("yyyy-MM-dd"),
                salida = salida.ToString("yyyy-MM-dd"),
                idTipo = idTipo
            });
        }
    }
}