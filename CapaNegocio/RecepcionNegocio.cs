using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using CapaDatos;

namespace CapaNegocio
{
    public class ReservaRecepcionFila
    {
        public int IdReserva { get; set; }
        public string Cliente { get; set; }
        public string Documento { get; set; }
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public int Noches { get; set; }
        public string Habitaciones { get; set; }
        public string Estado { get; set; }
        public decimal Total { get; set; }
        public bool Pagada { get; set; }
        public bool PuedeConfirmar { get; set; }
        public bool PuedeCheckIn { get; set; }
        public bool PuedeCheckOut { get; set; }
        public bool PuedeCancelar { get; set; }
        public string Nota { get; set; }
        public bool PuedeCobrar { get; set; }
        public int? IdPago { get; set; }
    }

    public class HabitacionEstadoFila
    {
        public int IdHabitacion { get; set; }
        public string Numero { get; set; }
        public int Piso { get; set; }
        public string Tipo { get; set; }
        public string Estado { get; set; }
        public string Detalle { get; set; }
        public bool PuedeMarcarLista { get; set; }
    }

    public class RecepcionNegocio
    {
        public List<string> Estados()
        {
            using (var db = new HotelContext())
            {
                return db.EstadoReserva
                    .OrderBy(e => e.IdEstadoReserva)
                    .Select(e => e.NombreEstado)
                    .ToList();
            }
        }

        public List<ReservaRecepcionFila> Listar(string estado, DateTime? fecha)
        {
            using (var db = new HotelContext())
            {
                IQueryable<Reserva> q = db.Reserva;

                if (!string.IsNullOrEmpty(estado))
                    q = q.Where(r => r.EstadoReserva.NombreEstado == estado);

                if (fecha.HasValue)
                {
                    DateTime f = fecha.Value.Date;
                    q = q.Where(r => r.FechaEntrada <= f && r.FechaSalida >= f);
                }

                var ordenada = q
                    .OrderBy(r => (r.IdEstadoReserva == EstadoReservaId.CheckOut
                                || r.IdEstadoReserva == EstadoReservaId.Cancelada) ? 1 : 0)
                    .ThenBy(r => r.FechaEntrada)
                    .ThenBy(r => r.IdReserva);

                return Proyectar(ordenada);
            }
        }

        public ReservaRecepcionFila ObtenerFila(int id)
        {
            using (var db = new HotelContext())
            {
                return Proyectar(db.Reserva.Where(r => r.IdReserva == id)).FirstOrDefault();
            }
        }


        public string Confirmar(int id)
        {
            return EjecutarSp("sp_ConfirmarReserva", id);
        }

        public string CheckIn(int id)
        {
            using (var db = new HotelContext())
            {
                var r = db.Reserva.AsNoTracking().FirstOrDefault(x => x.IdReserva == id);
                if (r == null) return "La reserva no existe.";
                if (r.IdEstadoReserva != EstadoReservaId.Confirmada)
                    return "Solo se puede hacer check-in a reservas Confirmadas.";

                DateTime hoy = DateTime.Today;
                if (hoy < r.FechaEntrada.Date)
                    return "El check-in se habilita desde la fecha de entrada (" + r.FechaEntrada.ToString("dd/MM/yyyy") + ").";
                if (hoy >= r.FechaSalida.Date)
                    return "La fecha de salida de esta reserva ya pasó. Cancélala.";

                var noListas = db.DetalleReserva
                    .Where(d => d.IdReserva == id && d.Habitacion.IdEstadoHabitacion != EstadoHabitacionId.Disponible)
                    .Select(d => new { d.Habitacion.NumeroHabitacion, Estado = d.Habitacion.EstadoHabitacion.NombreEstado })
                    .ToList();

                if (noListas.Count > 0)
                    return "No se puede hacer check-in: "
                         + string.Join("; ", noListas.Select(h => "la habitación " + h.NumeroHabitacion + " está en " + h.Estado))
                         + ".";
            }
            return EjecutarSp("sp_CheckIn", id);
        }

        public string CheckOut(int id)
        {
            using (var db = new HotelContext())
            {
                if (!db.Pago.Any(p => p.IdReserva == id))
                    return "No se puede hacer check-out sin registrar el pago. Cobra la reserva primero.";
            }
            return EjecutarSp("sp_CheckOut", id);
        }

        public string Cancelar(int id, string motivo)
        {
            motivo = (motivo ?? "").Trim();
            if (motivo.Length == 0) return "Indica el motivo de la cancelación.";
            if (motivo.Length > 200) return "El motivo admite máximo 200 caracteres.";

            using (var db0 = new HotelContext())
            {
                if (db0.Pago.Any(p => p.IdReserva == id))
                    return "No se puede cancelar: la reserva ya tiene un pago registrado.";
            }

            try
            {
                using (var db = new HotelContext())
                {
                    db.Database.ExecuteSqlCommand(
                        TransactionalBehavior.DoNotEnsureTransaction,
                        "EXEC sp_CancelarReserva @IdReserva, @Motivo",
                        new SqlParameter("@IdReserva", id),
                        new SqlParameter("@Motivo", SqlDbType.VarChar, 200) { Value = motivo });
                }
                return null;
            }
            catch (Exception ex)
            {
                return MensajeInterno(ex);
            }
        }


        public List<HabitacionEstadoFila> Habitaciones()
        {
            using (var db = new HotelContext())
            {
                var ocupadas = db.DetalleReserva
                    .Where(d => d.Reserva.IdEstadoReserva == EstadoReservaId.CheckIn)
                    .Select(d => new
                    {
                        d.IdHabitacion,
                        Cliente = d.Reserva.Cliente.Nombres + " " + d.Reserva.Cliente.Apellidos,
                        d.Reserva.FechaSalida
                    })
                    .ToList();

                var lista = db.Habitacion
                    .OrderBy(h => h.NumeroHabitacion)
                    .Select(h => new
                    {
                        h.IdHabitacion,
                        h.NumeroHabitacion,
                        h.Piso,
                        Tipo = h.TipoHabitacion.NombreTipo,
                        h.IdEstadoHabitacion,
                        Estado = h.EstadoHabitacion.NombreEstado
                    })
                    .ToList();

                return lista.Select(h =>
                {
                    var o = ocupadas.FirstOrDefault(x => x.IdHabitacion == h.IdHabitacion);
                    return new HabitacionEstadoFila
                    {
                        IdHabitacion = h.IdHabitacion,
                        Numero = h.NumeroHabitacion,
                        Piso = h.Piso,
                        Tipo = h.Tipo,
                        Estado = h.Estado,
                        Detalle = o == null ? null : o.Cliente + " · sale el " + o.FechaSalida.ToString("dd/MM"),
                        PuedeMarcarLista = h.IdEstadoHabitacion == EstadoHabitacionId.Limpieza
                    };
                }).ToList();
            }
        }

        public string MarcarLista(int idHabitacion)
        {
            using (var db = new HotelContext())
            {
                var h = db.Habitacion.Find(idHabitacion);
                if (h == null) return "La habitación no existe.";
                if (h.IdEstadoHabitacion != EstadoHabitacionId.Limpieza)
                    return "Solo se puede marcar como lista una habitación que está en Limpieza.";

                h.IdEstadoHabitacion = EstadoHabitacionId.Disponible;
                db.SaveChanges();
                return null;
            }
        }


        private static List<ReservaRecepcionFila> Proyectar(IQueryable<Reserva> q)
        {
            var datos = q.Select(r => new
            {
                r.IdReserva,
                Cliente = r.Cliente.Nombres + " " + r.Cliente.Apellidos,
                Documento = r.Cliente.NumeroDocumento,
                r.FechaEntrada,
                r.FechaSalida,
                r.MontoTotal,
                r.IdEstadoReserva,
                Estado = r.EstadoReserva.NombreEstado,
                Habs = r.DetalleReserva.Select(d => d.Habitacion.NumeroHabitacion),
                Pagada = r.Pago.Any(),
                IdPago = r.Pago.Select(p => (int?)p.IdPago).FirstOrDefault()
            }).ToList();

            DateTime hoy = DateTime.Today;

            return datos.Select(d =>
            {
                bool confirmada = d.IdEstadoReserva == EstadoReservaId.Confirmada;

                string nota = null;
                if (confirmada)
                {
                    if (hoy < d.FechaEntrada.Date) nota = "Check-in desde el " + d.FechaEntrada.ToString("dd/MM");
                    else if (hoy >= d.FechaSalida.Date) nota = "Fecha de salida vencida";
                }

                return new ReservaRecepcionFila
                {
                    IdReserva = d.IdReserva,
                    Cliente = d.Cliente,
                    Documento = d.Documento,
                    Entrada = d.FechaEntrada,
                    Salida = d.FechaSalida,
                    Noches = (d.FechaSalida - d.FechaEntrada).Days,
                    Habitaciones = string.Join(", ", d.Habs.OrderBy(x => x)),
                    Estado = d.Estado,
                    Total = d.MontoTotal,
                    Pagada = d.Pagada,
                    IdPago = d.IdPago,
                    PuedeCobrar = !d.Pagada && (confirmada || d.IdEstadoReserva == EstadoReservaId.CheckIn),
                    PuedeConfirmar = d.IdEstadoReserva == EstadoReservaId.Pendiente,
                    PuedeCheckIn = confirmada && hoy >= d.FechaEntrada.Date && hoy < d.FechaSalida.Date,
                    PuedeCheckOut = d.IdEstadoReserva == EstadoReservaId.CheckIn,
                    PuedeCancelar = (d.IdEstadoReserva == EstadoReservaId.Pendiente || confirmada) && !d.Pagada,
                    Nota = nota
                };
            }).ToList();
        }

        private static string EjecutarSp(string procedimiento, int idReserva)
        {
            try
            {
                using (var db = new HotelContext())
                {
                    db.Database.ExecuteSqlCommand(
                        TransactionalBehavior.DoNotEnsureTransaction,
                        "EXEC " + procedimiento + " @IdReserva",
                        new SqlParameter("@IdReserva", idReserva));
                }
                return null;
            }
            catch (Exception ex)
            {
                return MensajeInterno(ex);
            }
        }

        private static string MensajeInterno(Exception ex)
        {
            while (ex.InnerException != null) ex = ex.InnerException;
            return ex.Message;
        }
    }
}
