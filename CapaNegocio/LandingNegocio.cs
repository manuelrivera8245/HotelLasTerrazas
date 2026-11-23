using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class TipoLanding
    {
        public string Nombre { get; set; }
        public int Capacidad { get; set; }
        public string Descripcion { get; set; }
        public decimal? Desde { get; set; }
    }

    public class LandingNegocio
    {
        // Tipos activos con su tarifa más baja ("desde"), de todas las temporadas
        public List<TipoLanding> Tipos()
        {
            using (var db = new HotelContext())
            {
                var datos = db.TipoHabitacion
                    .Where(t => t.Estado)
                    .OrderBy(t => t.IdTipoHabitacion)
                    .Select(t => new
                    {
                        t.NombreTipo,
                        t.Capacidad,
                        t.Descripcion,
                        Desde = t.Tarifa.Min(x => (decimal?)x.PrecioNoche)
                    })
                    .ToList();

                return datos.Select(d => new TipoLanding
                {
                    Nombre = d.NombreTipo,
                    Capacidad = d.Capacidad,
                    Descripcion = d.Descripcion,
                    Desde = d.Desde
                }).ToList();
            }
        }
    }
}
