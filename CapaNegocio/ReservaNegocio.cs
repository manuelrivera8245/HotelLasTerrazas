using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class HabitacionDisponibleItem
    {
        public int IdHabitacion { get; set; }
        public string NumeroHabitacion { get; set; }
        public int Piso { get; set; }
        public int IdTipoHabitacion { get; set; }
        public string NombreTipo { get; set; }
        public int Capacidad { get; set; }
        public decimal PrecioNoche { get; set; }
    }

    public class ResultadoBusqueda
    {
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public int Noches { get; set; }
        public string NombreTemporada { get; set; }
        public string Error { get; set; }
        public List<HabitacionDisponibleItem> Habitaciones { get; set; }

        public ResultadoBusqueda()
        {
            Habitaciones = new List<HabitacionDisponibleItem>();
        }
    }

    public class LineaCotizacion
    {
        public int IdHabitacion { get; set; }
        public string NumeroHabitacion { get; set; }
        public string NombreTipo { get; set; }
        public decimal PrecioNoche { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class Cotizacion
    {
        public DateTime Entrada { get; set; }
        public DateTime Salida { get; set; }
        public int Noches { get; set; }
        public string NombreTemporada { get; set; }
        public decimal Total { get; set; }
        public string Error { get; set; }
        public List<LineaCotizacion> Lineas { get; set; }

        public Cotizacion()
        {
            Lineas = new List<LineaCotizacion>();
        }
    }

    public class ReservaNegocio
    {
        public ResultadoBusqueda Buscar(DateTime entrada, DateTime salida, int? idTipo)
        {
            var r = new ResultadoBusqueda { Entrada = entrada.Date, Salida = salida.Date };

            string e = ValidarFechas(r.Entrada, r.Salida);
            if (e != null) { r.Error = e; return r; }
            r.Noches = (r.Salida - r.Entrada).Days;

            using (var db = new HotelContext())
            {
                var temp = TemporadaVigente(db, r.Entrada);
                if (temp == null)
                {
                    r.Error = "El hotel aún no tiene tarifas definidas para la fecha de entrada elegida. Prueba con otras fechas.";
                    return r;
                }
                r.NombreTemporada = temp.NombreTemporada;

                var precios = db.Tarifa
                    .Where(x => x.IdTemporada == temp.IdTemporada)
                    .ToDictionary(x => x.IdTipoHabitacion, x => x.PrecioNoche);

                var q = Disponibles(db, r.Entrada, r.Salida);
                if (idTipo.HasValue)
                    q = q.Where(h => h.IdTipoHabitacion == idTipo.Value);

                var lista = q.OrderBy(h => h.NumeroHabitacion)
                    .Select(h => new
                    {
                        h.IdHabitacion,
                        h.NumeroHabitacion,
                        h.Piso,
                        h.IdTipoHabitacion,
                        h.TipoHabitacion.NombreTipo,
                        h.TipoHabitacion.Capacidad
                    })
                    .ToList();

                foreach (var h in lista)
                {
                    decimal precio;
                    if (!precios.TryGetValue(h.IdTipoHabitacion, out precio)) continue;

                    r.Habitaciones.Add(new HabitacionDisponibleItem
                    {
                        IdHabitacion = h.IdHabitacion,
                        NumeroHabitacion = h.NumeroHabitacion,
                        Piso = h.Piso,
                        IdTipoHabitacion = h.IdTipoHabitacion,
                        NombreTipo = h.NombreTipo,
                        Capacidad = h.Capacidad,
                        PrecioNoche = precio
                    });
                }
            }
            return r;
        }

        public Cotizacion Cotizar(DateTime entrada, DateTime salida, int[] ids)
        {
            var c = new Cotizacion { Entrada = entrada.Date, Salida = salida.Date };

            string e = ValidarFechas(c.Entrada, c.Salida);
            if (e != null) { c.Error = e; return c; }
            if (ids == null || ids.Length == 0) { c.Error = "Elige al menos una habitación."; return c; }

            var pedidas = ids.Distinct().ToList();
            c.Noches = (c.Salida - c.Entrada).Days;

            using (var db = new HotelContext())
            {
                var temp = TemporadaVigente(db, c.Entrada);
                if (temp == null)
                {
                    c.Error = "No hay tarifas definidas para la fecha de entrada elegida.";
                    return c;
                }
                c.NombreTemporada = temp.NombreTemporada;

                var precios = db.Tarifa
                    .Where(x => x.IdTemporada == temp.IdTemporada)
                    .ToDictionary(x => x.IdTipoHabitacion, x => x.PrecioNoche);

                var disp = Disponibles(db, c.Entrada, c.Salida)
                    .Where(h => pedidas.Contains(h.IdHabitacion))
                    .Select(h => new { h.IdHabitacion, h.NumeroHabitacion, h.IdTipoHabitacion, h.TipoHabitacion.NombreTipo })
                    .ToList();

                if (disp.Count != pedidas.Count)
                {
                    c.Error = "Alguna de las habitaciones elegidas ya no está disponible en esas fechas. Vuelve a buscar.";
                    return c;
                }

                foreach (var h in disp.OrderBy(x => x.NumeroHabitacion))
                {
                    decimal precio;
                    if (!precios.TryGetValue(h.IdTipoHabitacion, out precio))
                    {
                        c.Error = "La habitación " + h.NumeroHabitacion + " no tiene tarifa para esa fecha.";
                        return c;
                    }
                    c.Lineas.Add(new LineaCotizacion
                    {
                        IdHabitacion = h.IdHabitacion,
                        NumeroHabitacion = h.NumeroHabitacion,
                        NombreTipo = h.NombreTipo,
                        PrecioNoche = precio,
                        Subtotal = precio * c.Noches
                    });
                }
                c.Total = c.Lineas.Sum(l => l.Subtotal);
            }
            return c;
        }


        private static string ValidarFechas(DateTime entrada, DateTime salida)
        {
            if (entrada < DateTime.Today) return "La fecha de entrada no puede ser anterior a hoy.";
            if (salida <= entrada) return "La fecha de salida debe ser posterior a la de entrada.";
            if ((salida - entrada).Days > 30) return "La estadía máxima es de 30 noches.";
            return null;
        }

        private static Temporada TemporadaVigente(HotelContext db, DateTime fecha)
        {
            return db.Temporada.FirstOrDefault(t => t.FechaInicio <= fecha && t.FechaFin >= fecha);
        }

        private static IQueryable<Habitacion> Disponibles(HotelContext db, DateTime entrada, DateTime salida)
        {
            var ocupadas = db.DetalleReserva
                .Where(d => (d.Reserva.IdEstadoReserva == EstadoReservaId.Pendiente
                          || d.Reserva.IdEstadoReserva == EstadoReservaId.Confirmada
                          || d.Reserva.IdEstadoReserva == EstadoReservaId.CheckIn)
                         && entrada < d.Reserva.FechaSalida
                         && salida > d.Reserva.FechaEntrada)
                .Select(d => d.IdHabitacion);

            return db.Habitacion.Where(h =>
                h.IdEstadoHabitacion != EstadoHabitacionId.Mantenimiento
                && h.TipoHabitacion.Estado
                && !ocupadas.Contains(h.IdHabitacion));
        }
    }
}
