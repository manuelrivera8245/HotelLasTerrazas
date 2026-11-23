using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using CapaDatos;

namespace CapaNegocio
{
    public class PanelHabitaciones
    {
        public int Total { get; set; }
        public int Disponibles { get; set; }
        public int Ocupadas { get; set; }
        public int Limpieza { get; set; }
        public int Mantenimiento { get; set; }
    }

    public class MiniReserva
    {
        public int IdReserva { get; set; }
        public string Cliente { get; set; }
        public string Habitaciones { get; set; }
        public string Estado { get; set; }
        public bool Pagada { get; set; }
    }

    public class PuntoIngreso
    {
        public string Etiqueta { get; set; }
        public decimal Monto { get; set; }
    }

    public class PuntoEstado
    {
        public string Estado { get; set; }
        public int Cantidad { get; set; }
    }

    public class ReservaReciente
    {
        public int IdReserva { get; set; }
        public string Cliente { get; set; }
        public string Habitaciones { get; set; }
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public string Estado { get; set; }
        public decimal Monto { get; set; }
        public bool Pagada { get; set; }
    }

    public class PanelAdmin
    {
        public PanelHabitaciones Habitaciones { get; set; }
        public int LlegadasHoy { get; set; }
        public int Pendientes { get; set; }
        public decimal IngresosMes { get; set; }
        public int PagosMes { get; set; }
        public decimal OcupacionHoy { get; set; }
        public int TarifasFaltan { get; set; }
        public int HuecosTemporada { get; set; }

        // Nuevos (solo lectura, para el panel)
        public decimal IngresosMesAnterior { get; set; }
        public List<PuntoIngreso> IngresosSemana { get; set; }
        public List<PuntoEstado> ReservasPorEstado { get; set; }
        public List<ReservaReciente> Recientes { get; set; }

        public PanelAdmin()
        {
            IngresosSemana = new List<PuntoIngreso>();
            ReservasPorEstado = new List<PuntoEstado>();
            Recientes = new List<ReservaReciente>();
        }
    }

    public class PanelRecepcion
    {
        public PanelHabitaciones Habitaciones { get; set; }
        public List<MiniReserva> Llegadas { get; set; }
        public List<MiniReserva> Salidas { get; set; }
        public int Pendientes { get; set; }
        public int PorCobrar { get; set; }

        public PanelRecepcion()
        {
            Llegadas = new List<MiniReserva>();
            Salidas = new List<MiniReserva>();
        }
    }

    public class PanelCliente
    {
        public int Activas { get; set; }
        public List<ReservaFila> Proximas { get; set; }

        public PanelCliente()
        {
            Proximas = new List<ReservaFila>();
        }
    }

    public class PanelNegocio
    {
        public PanelAdmin Admin()
        {
            DateTime hoy = DateTime.Today;
            DateTime iniMes = new DateTime(hoy.Year, hoy.Month, 1);
            DateTime finMes = iniMes.AddMonths(1);
            var p = new PanelAdmin();

            using (var db = new HotelContext())
            {
                p.Habitaciones = Habs(db);

                p.LlegadasHoy = db.Reserva.Count(r => r.FechaEntrada == hoy
                    && (r.IdEstadoReserva == EstadoReservaId.Pendiente || r.IdEstadoReserva == EstadoReservaId.Confirmada));

                p.Pendientes = db.Reserva.Count(r => r.IdEstadoReserva == EstadoReservaId.Pendiente);

                var pagos = db.Pago.Where(x => x.FechaPago >= iniMes && x.FechaPago < finMes);
                p.IngresosMes = pagos.Select(x => (decimal?)x.Monto).Sum() ?? 0m;
                p.PagosMes = pagos.Count();

                // ---- Nuevos datos del panel (solo lectura) ----
                DateTime iniMesAnt = iniMes.AddMonths(-1);
                p.IngresosMesAnterior = db.Pago
                    .Where(x => x.FechaPago >= iniMesAnt && x.FechaPago < iniMes)
                    .Select(x => (decimal?)x.Monto).Sum() ?? 0m;

                // Ingresos de los últimos 7 días (se agrupa en memoria)
                DateTime desde = hoy.AddDays(-6);
                DateTime hasta = hoy.AddDays(1);
                var pagosSemana = db.Pago
                    .Where(x => x.FechaPago >= desde && x.FechaPago < hasta)
                    .Select(x => new { x.FechaPago, x.Monto })
                    .ToList();
                var es = new CultureInfo("es-PE");
                for (int i = 0; i < 7; i++)
                {
                    DateTime d = desde.AddDays(i);
                    p.IngresosSemana.Add(new PuntoIngreso
                    {
                        Etiqueta = d.ToString("ddd d", es),
                        Monto = pagosSemana.Where(x => x.FechaPago.Date == d).Sum(x => x.Monto)
                    });
                }

                // Reservas por estado
                var estados = db.Reserva
                    .GroupBy(r => r.EstadoReserva.NombreEstado)
                    .Select(g => new { Estado = g.Key, N = g.Count() })
                    .ToList();
                string[] orden = { "Pendiente", "Confirmada", "CheckIn", "CheckOut", "Cancelada" };
                p.ReservasPorEstado = estados
                    .OrderBy(x => Array.IndexOf(orden, x.Estado))
                    .Select(x => new PuntoEstado { Estado = x.Estado, Cantidad = x.N })
                    .ToList();

                // Últimas 6 reservas
                var rec = db.Reserva
                    .OrderByDescending(r => r.IdReserva)
                    .Take(6)
                    .Select(r => new
                    {
                        r.IdReserva,
                        Cliente = r.Cliente.Nombres + " " + r.Cliente.Apellidos,
                        r.FechaEntrada,
                        r.FechaSalida,
                        Estado = r.EstadoReserva.NombreEstado,
                        r.MontoTotal,
                        Habs = r.DetalleReserva.Select(x => x.Habitacion.NumeroHabitacion),
                        Pagada = r.Pago.Any()
                    }).ToList();

                p.Recientes = rec.Select(x => new ReservaReciente
                {
                    IdReserva = x.IdReserva,
                    Cliente = x.Cliente,
                    Habitaciones = string.Join(", ", x.Habs.OrderBy(h => h)),
                    Entrada = x.FechaEntrada,
                    Salida = x.FechaSalida,
                    Estado = x.Estado,
                    Monto = x.MontoTotal,
                    Pagada = x.Pagada
                }).ToList();
            }

            p.OcupacionHoy = new ReportesNegocio().Ocupacion(hoy, hoy).Porcentaje;
            p.TarifasFaltan = new TarifaNegocio().Matriz().Faltan;
            p.HuecosTemporada = new TemporadaNegocio().Huecos().Count;
            return p;
        }

        public PanelRecepcion Recepcion()
        {
            DateTime hoy = DateTime.Today;
            var p = new PanelRecepcion();

            using (var db = new HotelContext())
            {
                p.Habitaciones = Habs(db);

                p.Llegadas = Mini(db.Reserva
                    .Where(r => r.FechaEntrada == hoy
                        && (r.IdEstadoReserva == EstadoReservaId.Pendiente || r.IdEstadoReserva == EstadoReservaId.Confirmada))
                    .OrderBy(r => r.IdReserva));

                p.Salidas = Mini(db.Reserva
                    .Where(r => r.FechaSalida == hoy && r.IdEstadoReserva == EstadoReservaId.CheckIn)
                    .OrderBy(r => r.IdReserva));

                p.Pendientes = db.Reserva.Count(r => r.IdEstadoReserva == EstadoReservaId.Pendiente);

                p.PorCobrar = db.Reserva.Count(r =>
                    (r.IdEstadoReserva == EstadoReservaId.Confirmada || r.IdEstadoReserva == EstadoReservaId.CheckIn)
                    && !r.Pago.Any());
            }
            return p;
        }

        public PanelCliente Cliente(int idCliente)
        {
            DateTime hoy = DateTime.Today;
            var todas = new ReservaClienteNegocio().MisReservas(idCliente);

            var vivas = todas
                .Where(r => (r.Estado == "Pendiente" || r.Estado == "Confirmada" || r.Estado == "CheckIn")
                         && r.Salida >= hoy)
                .OrderBy(r => r.Entrada)
                .ToList();

            return new PanelCliente { Activas = vivas.Count, Proximas = vivas.Take(3).ToList() };
        }


        private static PanelHabitaciones Habs(HotelContext db)
        {
            var g = db.Habitacion
                .GroupBy(h => h.IdEstadoHabitacion)
                .Select(x => new { Id = x.Key, N = x.Count() })
                .ToList();

            var ph = new PanelHabitaciones();
            foreach (var x in g)
            {
                ph.Total += x.N;
                if (x.Id == EstadoHabitacionId.Disponible) ph.Disponibles = x.N;
                else if (x.Id == EstadoHabitacionId.Ocupada) ph.Ocupadas = x.N;
                else if (x.Id == EstadoHabitacionId.Limpieza) ph.Limpieza = x.N;
                else if (x.Id == EstadoHabitacionId.Mantenimiento) ph.Mantenimiento = x.N;
            }
            return ph;
        }

        private static List<MiniReserva> Mini(IQueryable<Reserva> q)
        {
            var d = q.Select(r => new
            {
                r.IdReserva,
                Cliente = r.Cliente.Nombres + " " + r.Cliente.Apellidos,
                Estado = r.EstadoReserva.NombreEstado,
                Habs = r.DetalleReserva.Select(x => x.Habitacion.NumeroHabitacion),
                Pagada = r.Pago.Any()
            }).ToList();

            return d.Select(x => new MiniReserva
            {
                IdReserva = x.IdReserva,
                Cliente = x.Cliente,
                Estado = x.Estado,
                Habitaciones = string.Join(", ", x.Habs.OrderBy(h => h)),
                Pagada = x.Pagada
            }).ToList();
        }
    }
}
