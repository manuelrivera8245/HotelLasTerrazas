using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class OcupacionFila
    {
        public string Numero { get; set; }
        public int Piso { get; set; }
        public string Tipo { get; set; }
        public int NochesOcupadas { get; set; }
        public decimal Porcentaje { get; set; }
    }

    public class ReporteOcupacion
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public int Dias { get; set; }
        public int Habitaciones { get; set; }
        public int NochesOcupadas { get; set; }
        public int NochesDisponibles { get; set; }
        public decimal Porcentaje { get; set; }
        public string Error { get; set; }
        public List<OcupacionFila> Filas { get; set; }

        public ReporteOcupacion()
        {
            Filas = new List<OcupacionFila>();
        }
    }

    public class IngresoMetodoFila
    {
        public string Metodo { get; set; }
        public int Pagos { get; set; }
        public decimal Monto { get; set; }
        public decimal Porcentaje { get; set; }
    }

    public class IngresoDiaFila
    {
        public DateTime Fecha { get; set; }
        public int Pagos { get; set; }
        public decimal Monto { get; set; }
    }

    public class ReporteIngresos
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public decimal Total { get; set; }
        public int CantPagos { get; set; }
        public decimal Promedio { get; set; }
        public string Error { get; set; }
        public List<IngresoMetodoFila> PorMetodo { get; set; }
        public List<IngresoDiaFila> PorDia { get; set; }

        public ReporteIngresos()
        {
            PorMetodo = new List<IngresoMetodoFila>();
            PorDia = new List<IngresoDiaFila>();
        }
    }

    public class ResumenEstadoFila
    {
        public string Estado { get; set; }
        public int Cantidad { get; set; }
        public decimal Monto { get; set; }
    }

    public class ReservaReporteFila
    {
        public int IdReserva { get; set; }
        public string Cliente { get; set; }
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public int Noches { get; set; }
        public string Habitaciones { get; set; }
        public string Estado { get; set; }
        public decimal Total { get; set; }
        public bool Pagada { get; set; }
    }

    public class ReporteEstados
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public string Estado { get; set; }
        public int TotalReservas { get; set; }
        public string Error { get; set; }
        public List<string> Estados { get; set; }
        public List<ResumenEstadoFila> Resumen { get; set; }
        public List<ReservaReporteFila> Filas { get; set; }

        public ReporteEstados()
        {
            Estados = new List<string>();
            Resumen = new List<ResumenEstadoFila>();
            Filas = new List<ReservaReporteFila>();
        }
    }

    public class ReportesNegocio
    {
        public ReporteOcupacion Ocupacion(DateTime desde, DateTime hasta)
        {
            var r = new ReporteOcupacion { Desde = desde.Date, Hasta = hasta.Date };
            string e = ValidarRango(r.Desde, r.Hasta);
            if (e != null) { r.Error = e; return r; }

            DateTime ini = r.Desde;
            DateTime fin = r.Hasta.AddDays(1);
            r.Dias = (fin - ini).Days;

            int[] estados = { EstadoReservaId.Confirmada, EstadoReservaId.CheckIn, EstadoReservaId.CheckOut };

            using (var db = new HotelContext())
            {
                var estadias = db.DetalleReserva
                    .Where(d => estados.Contains(d.Reserva.IdEstadoReserva)
                             && d.Reserva.FechaEntrada < fin
                             && d.Reserva.FechaSalida > ini)
                    .Select(d => new
                    {
                        d.IdHabitacion,
                        Entrada = d.Reserva.FechaEntrada,
                        Salida = d.Reserva.FechaSalida
                    })
                    .ToList();

                var habs = db.Habitacion
                    .OrderBy(h => h.NumeroHabitacion)
                    .Select(h => new
                    {
                        h.IdHabitacion,
                        h.NumeroHabitacion,
                        h.Piso,
                        Tipo = h.TipoHabitacion.NombreTipo
                    })
                    .ToList();

                foreach (var h in habs)
                {
                    int noches = estadias
                        .Where(x => x.IdHabitacion == h.IdHabitacion)
                        .Sum(x => Solape(x.Entrada, x.Salida, ini, fin));

                    r.Filas.Add(new OcupacionFila
                    {
                        Numero = h.NumeroHabitacion,
                        Piso = h.Piso,
                        Tipo = h.Tipo,
                        NochesOcupadas = noches,
                        Porcentaje = noches * 100m / r.Dias
                    });
                }
            }

            r.Habitaciones = r.Filas.Count;
            r.NochesOcupadas = r.Filas.Sum(f => f.NochesOcupadas);
            r.NochesDisponibles = r.Habitaciones * r.Dias;
            r.Porcentaje = r.NochesDisponibles > 0 ? r.NochesOcupadas * 100m / r.NochesDisponibles : 0m;
            return r;
        }

        public ReporteIngresos Ingresos(DateTime desde, DateTime hasta)
        {
            var r = new ReporteIngresos { Desde = desde.Date, Hasta = hasta.Date };
            string e = ValidarRango(r.Desde, r.Hasta);
            if (e != null) { r.Error = e; return r; }

            DateTime ini = r.Desde;
            DateTime fin = r.Hasta.AddDays(1);

            using (var db = new HotelContext())
            {
                var pagos = db.Pago
                    .Where(p => p.FechaPago >= ini && p.FechaPago < fin)
                    .Select(p => new { p.FechaPago, p.Monto, Metodo = p.MetodoPago.NombreMetodo })
                    .ToList();

                r.CantPagos = pagos.Count;
                r.Total = pagos.Sum(p => p.Monto);
                r.Promedio = r.CantPagos > 0 ? decimal.Round(r.Total / r.CantPagos, 2) : 0m;

                decimal total = r.Total;
                r.PorMetodo = pagos
                    .GroupBy(p => p.Metodo)
                    .Select(g => new IngresoMetodoFila
                    {
                        Metodo = g.Key,
                        Pagos = g.Count(),
                        Monto = g.Sum(x => x.Monto),
                        Porcentaje = total > 0 ? g.Sum(x => x.Monto) * 100m / total : 0m
                    })
                    .OrderByDescending(x => x.Monto)
                    .ToList();

                r.PorDia = pagos
                    .GroupBy(p => p.FechaPago.Date)
                    .Select(g => new IngresoDiaFila { Fecha = g.Key, Pagos = g.Count(), Monto = g.Sum(x => x.Monto) })
                    .OrderBy(x => x.Fecha)
                    .ToList();
            }
            return r;
        }

        public ReporteEstados PorEstado(DateTime desde, DateTime hasta, string estado)
        {
            var r = new ReporteEstados { Desde = desde.Date, Hasta = hasta.Date, Estado = estado };
            string e = ValidarRango(r.Desde, r.Hasta);
            if (e != null) { r.Error = e; return r; }

            DateTime ini = r.Desde;
            DateTime fin = r.Hasta.AddDays(1);

            using (var db = new HotelContext())
            {
                r.Estados = db.EstadoReserva
                    .OrderBy(x => x.IdEstadoReserva)
                    .Select(x => x.NombreEstado)
                    .ToList();

                var datos = db.Reserva
                    .Where(x => x.FechaEntrada >= ini && x.FechaEntrada < fin)
                    .Select(x => new
                    {
                        x.IdReserva,
                        Cliente = x.Cliente.Nombres + " " + x.Cliente.Apellidos,
                        x.FechaEntrada,
                        x.FechaSalida,
                        x.MontoTotal,
                        Estado = x.EstadoReserva.NombreEstado,
                        Habs = x.DetalleReserva.Select(d => d.Habitacion.NumeroHabitacion),
                        Pagada = x.Pago.Any()
                    })
                    .ToList();

                r.TotalReservas = datos.Count;

                foreach (var nombre in r.Estados)
                {
                    var delEstado = datos.Where(d => d.Estado == nombre).ToList();
                    r.Resumen.Add(new ResumenEstadoFila
                    {
                        Estado = nombre,
                        Cantidad = delEstado.Count,
                        Monto = delEstado.Sum(d => d.MontoTotal)
                    });
                }

                var filtradas = string.IsNullOrEmpty(estado)
                    ? datos
                    : datos.Where(d => d.Estado == estado).ToList();

                r.Filas = filtradas
                    .OrderBy(d => d.FechaEntrada)
                    .ThenBy(d => d.IdReserva)
                    .Select(d => new ReservaReporteFila
                    {
                        IdReserva = d.IdReserva,
                        Cliente = d.Cliente,
                        Entrada = d.FechaEntrada,
                        Salida = d.FechaSalida,
                        Noches = (d.FechaSalida - d.FechaEntrada).Days,
                        Habitaciones = string.Join(", ", d.Habs.OrderBy(h => h)),
                        Estado = d.Estado,
                        Total = d.MontoTotal,
                        Pagada = d.Pagada
                    })
                    .ToList();
            }
            return r;
        }


        private static string ValidarRango(DateTime desde, DateTime hasta)
        {
            if (hasta < desde) return "La fecha final no puede ser anterior a la inicial.";
            if ((hasta - desde).Days > 365) return "El rango máximo es de 366 días.";
            return null;
        }

        private static int Solape(DateTime entrada, DateTime salida, DateTime ini, DateTime fin)
        {
            DateTime a = entrada > ini ? entrada : ini;
            DateTime b = salida < fin ? salida : fin;
            int n = (b - a).Days;
            return n > 0 ? n : 0;
        }
    }
}
