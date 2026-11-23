using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class TemporadaCol
    {
        public int IdTemporada { get; set; }
        public string NombreTemporada { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }

    public class MatrizTarifas
    {
        public List<ItemCombo> Tipos { get; set; }
        public List<TemporadaCol> Temporadas { get; set; }
        public Dictionary<string, decimal> Precios { get; set; }
        public int Faltan { get; set; }

        public MatrizTarifas()
        {
            Tipos = new List<ItemCombo>();
            Temporadas = new List<TemporadaCol>();
            Precios = new Dictionary<string, decimal>();
        }

        public decimal? Precio(int idTipo, int idTemporada)
        {
            decimal p;
            return Precios.TryGetValue(idTipo + "-" + idTemporada, out p) ? (decimal?)p : null;
        }
    }

    public class FilaPrecio
    {
        public int IdTipoHabitacion { get; set; }
        public string NombreTipo { get; set; }
        public decimal? Precio { get; set; }
    }

    public class TarifaNegocio
    {
        public MatrizTarifas Matriz()
        {
            using (var db = new HotelContext())
            {
                var m = new MatrizTarifas();

                m.Tipos = db.TipoHabitacion
                    .Where(t => t.Estado)
                    .OrderBy(t => t.IdTipoHabitacion)
                    .Select(t => new ItemCombo { Id = t.IdTipoHabitacion, Texto = t.NombreTipo })
                    .ToList();

                m.Temporadas = db.Temporada
                    .OrderBy(t => t.FechaInicio)
                    .Select(t => new TemporadaCol
                    {
                        IdTemporada = t.IdTemporada,
                        NombreTemporada = t.NombreTemporada,
                        FechaInicio = t.FechaInicio,
                        FechaFin = t.FechaFin
                    })
                    .ToList();

                var ids = m.Tipos.Select(t => t.Id).ToList();
                var tarifas = db.Tarifa
                    .Where(x => ids.Contains(x.IdTipoHabitacion))
                    .Select(x => new { x.IdTipoHabitacion, x.IdTemporada, x.PrecioNoche })
                    .ToList();

                foreach (var t in tarifas)
                    m.Precios[t.IdTipoHabitacion + "-" + t.IdTemporada] = t.PrecioNoche;

                m.Faltan = m.Tipos.Count * m.Temporadas.Count - m.Precios.Count;
                return m;
            }
        }

        public List<FilaPrecio> PreciosDeTemporada(int idTemporada)
        {
            using (var db = new HotelContext())
            {
                var existentes = db.Tarifa.Where(x => x.IdTemporada == idTemporada).ToList();

                return db.TipoHabitacion
                    .Where(t => t.Estado)
                    .OrderBy(t => t.IdTipoHabitacion)
                    .ToList()
                    .Select(t =>
                    {
                        var x = existentes.FirstOrDefault(e => e.IdTipoHabitacion == t.IdTipoHabitacion);
                        return new FilaPrecio
                        {
                            IdTipoHabitacion = t.IdTipoHabitacion,
                            NombreTipo = t.NombreTipo,
                            Precio = x == null ? (decimal?)null : x.PrecioNoche
                        };
                    })
                    .ToList();
            }
        }

        public string GuardarPrecios(int idTemporada, Dictionary<int, decimal?> precios)
        {
            using (var db = new HotelContext())
            {
                if (!db.Temporada.Any(t => t.IdTemporada == idTemporada))
                    return "La temporada no existe.";

                var tipos = db.TipoHabitacion.Where(t => t.Estado).ToList();
                var existentes = db.Tarifa.Where(x => x.IdTemporada == idTemporada).ToList();

                foreach (var par in precios)
                {
                    var tipo = tipos.FirstOrDefault(t => t.IdTipoHabitacion == par.Key);
                    if (tipo == null || !par.Value.HasValue) continue;

                    decimal p = decimal.Round(par.Value.Value, 2);
                    if (p <= 0 || p > 99999.99m)
                        return string.Format("El precio de {0} debe ser mayor que 0 y no superar 99,999.99.", tipo.NombreTipo);
                }

                foreach (var par in precios)
                {
                    var tipo = tipos.FirstOrDefault(t => t.IdTipoHabitacion == par.Key);
                    if (tipo == null) continue;

                    var actual = existentes.FirstOrDefault(e => e.IdTipoHabitacion == par.Key);
                    if (par.Value.HasValue)
                    {
                        decimal p = decimal.Round(par.Value.Value, 2);
                        if (actual == null)
                            db.Tarifa.Add(new Tarifa { IdTipoHabitacion = par.Key, IdTemporada = idTemporada, PrecioNoche = p });
                        else
                            actual.PrecioNoche = p;
                    }
                    else if (actual != null)
                    {
                        db.Tarifa.Remove(actual);
                    }
                }

                db.SaveChanges();
                return null;
            }
        }
    }
}
