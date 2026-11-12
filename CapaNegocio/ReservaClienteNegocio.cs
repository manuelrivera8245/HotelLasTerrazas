using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using CapaDatos;

namespace CapaNegocio
{
    public class ResultadoSp
    {
        public int IdReservaGenerada { get; set; }
        public decimal MontoTotal { get; set; }
    }

    public class RegistroResultado
    {
        public int IdReserva { get; set; }
        public string Error { get; set; }
    }

    public class ReservaFila
    {
        public int IdReserva { get; set; }
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public int Noches { get; set; }
        public string Habitaciones { get; set; }
        public string Estado { get; set; }
        public decimal Total { get; set; }
        public bool Pagada { get; set; }
        public bool PuedeCancelar { get; set; }
    }

    public class ReservaDetalleCliente
    {
        public int IdReserva { get; set; }
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public int Noches { get; set; }
        public string Estado { get; set; }
        public decimal Total { get; set; }
        public List<LineaCotizacion> Lineas { get; set; }

        public ReservaDetalleCliente()
        {
            Lineas = new List<LineaCotizacion>();
        }
    }

    public class ReservaClienteNegocio
    {
        private readonly ReservaNegocio cotizador = new ReservaNegocio();

        public RegistroResultado Registrar(int idCliente, DateTime entrada, DateTime salida, int[] ids)
        {
            Cotizacion cot = cotizador.Cotizar(entrada, salida, ids);
            if (cot.Error != null) return new RegistroResultado { Error = cot.Error };

            string lista = string.Join(",", cot.Lineas.Select(l => l.IdHabitacion));

            try
            {
                using (var db = new HotelContext())
                {
                    var res = db.Database.SqlQuery<ResultadoSp>(
                        "EXEC sp_RegistrarReserva @IdCliente, @FechaEntrada, @FechaSalida, @Habitaciones",
                        new SqlParameter("@IdCliente", idCliente),
                        new SqlParameter("@FechaEntrada", SqlDbType.Date) { Value = cot.Entrada },
                        new SqlParameter("@FechaSalida", SqlDbType.Date) { Value = cot.Salida },
                        new SqlParameter("@Habitaciones", SqlDbType.VarChar, 200) { Value = lista }
                    ).ToList();

                    if (res.Count == 0)
                        return new RegistroResultado { Error = "No se pudo registrar la reserva. Inténtalo de nuevo." };

                    return new RegistroResultado { IdReserva = res[0].IdReservaGenerada };
                }
            }
            catch (Exception ex)
            {
                return new RegistroResultado { Error = "No se pudo registrar la reserva: " + MensajeInterno(ex) };
            }
        }

        public List<ReservaFila> MisReservas(int idCliente)
        {
            using (var db = new HotelContext())
            {
                var datos = db.Reserva
                    .Where(r => r.IdCliente == idCliente)
                    .OrderByDescending(r => r.FechaReserva)
                    .Select(r => new
                    {
                        r.IdReserva,
                        r.FechaEntrada,
                        r.FechaSalida,
                        r.MontoTotal,
                        r.IdEstadoReserva,
                        Estado = r.EstadoReserva.NombreEstado,
                        Habs = r.DetalleReserva.Select(d => d.Habitacion.NumeroHabitacion),
                        Pagada = r.Pago.Any()
                    })
                    .ToList();

                return datos.Select(d => new ReservaFila
                {
                    IdReserva = d.IdReserva,
                    Entrada = d.FechaEntrada,
                    Salida = d.FechaSalida,
                    Noches = (d.FechaSalida - d.FechaEntrada).Days,
                    Habitaciones = string.Join(", ", d.Habs.OrderBy(x => x)),
                    Estado = d.Estado,
                    Total = d.MontoTotal,
                    Pagada = d.Pagada,
                    PuedeCancelar = d.IdEstadoReserva == EstadoReservaId.Pendiente
                                 || d.IdEstadoReserva == EstadoReservaId.Confirmada
                }).ToList();
            }
        }

        public ReservaDetalleCliente Obtener(int idReserva, int idCliente)
        {
            using (var db = new HotelContext())
            {
                var r = db.Reserva
                    .Where(x => x.IdReserva == idReserva && x.IdCliente == idCliente)
                    .Select(x => new
                    {
                        x.IdReserva,
                        x.FechaEntrada,
                        x.FechaSalida,
                        x.MontoTotal,
                        Estado = x.EstadoReserva.NombreEstado,
                        Lineas = x.DetalleReserva.Select(d => new
                        {
                            d.IdHabitacion,
                            d.Habitacion.NumeroHabitacion,
                            d.Habitacion.TipoHabitacion.NombreTipo,
                            d.PrecioNocheAplicado
                        })
                    })
                    .FirstOrDefault();

                if (r == null) return null;

                int noches = (r.FechaSalida - r.FechaEntrada).Days;
                var det = new ReservaDetalleCliente
                {
                    IdReserva = r.IdReserva,
                    Entrada = r.FechaEntrada,
                    Salida = r.FechaSalida,
                    Noches = noches,
                    Estado = r.Estado,
                    Total = r.MontoTotal
                };

                foreach (var l in r.Lineas.OrderBy(x => x.NumeroHabitacion))
                {
                    det.Lineas.Add(new LineaCotizacion
                    {
                        IdHabitacion = l.IdHabitacion,
                        NumeroHabitacion = l.NumeroHabitacion,
                        NombreTipo = l.NombreTipo,
                        PrecioNoche = l.PrecioNocheAplicado,
                        Subtotal = l.PrecioNocheAplicado * noches
                    });
                }
                return det;
            }
        }

        public string Cancelar(int idReserva, int idCliente)
        {
            using (var db = new HotelContext())
            {
                var r = db.Reserva.AsNoTracking()
                    .FirstOrDefault(x => x.IdReserva == idReserva && x.IdCliente == idCliente);
                if (r == null) return "La reserva no existe.";

                if (r.IdEstadoReserva != EstadoReservaId.Pendiente && r.IdEstadoReserva != EstadoReservaId.Confirmada)
                    return "Solo se pueden cancelar reservas Pendientes o Confirmadas.";

                try
                {
                    db.Database.ExecuteSqlCommand(
                        "EXEC sp_CancelarReserva @IdReserva, @Motivo",
                        new SqlParameter("@IdReserva", idReserva),
                        new SqlParameter("@Motivo", "Cancelada por el cliente"));
                    return null;
                }
                catch (Exception ex)
                {
                    return MensajeInterno(ex);
                }
            }
        }

        private static string MensajeInterno(Exception ex)
        {
            while (ex.InnerException != null) ex = ex.InnerException;
            return ex.Message;
        }
    }
}
