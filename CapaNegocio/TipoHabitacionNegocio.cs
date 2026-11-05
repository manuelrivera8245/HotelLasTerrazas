using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class TipoHabitacionFila
    {
        public int IdTipoHabitacion { get; set; }
        public string NombreTipo { get; set; }
        public int Capacidad { get; set; }
        public string Descripcion { get; set; }
        public bool Estado { get; set; }
        public int CantHabitaciones { get; set; }
    }

    public class TipoHabitacionNegocio
    {
        public List<TipoHabitacionFila> Listar()
        {
            using (var db = new HotelContext())
            {
                return db.TipoHabitacion
                    .OrderBy(t => t.NombreTipo)
                    .Select(t => new TipoHabitacionFila
                    {
                        IdTipoHabitacion = t.IdTipoHabitacion,
                        NombreTipo = t.NombreTipo,
                        Capacidad = t.Capacidad,
                        Descripcion = t.Descripcion,
                        Estado = t.Estado,
                        CantHabitaciones = t.Habitacion.Count()
                    })
                    .ToList();
            }
        }

        public TipoHabitacion Obtener(int id)
        {
            using (var db = new HotelContext())
            {
                return db.TipoHabitacion.AsNoTracking()
                    .FirstOrDefault(t => t.IdTipoHabitacion == id);
            }
        }

        // id = 0 crea uno nuevo. Devuelve null si salió bien, o el mensaje de error.
        public string Guardar(int id, string nombre, int capacidad, string descripcion, bool estado)
        {
            nombre = (nombre ?? "").Trim();
            if (nombre.Length == 0) return "El nombre es obligatorio.";
            if (capacidad < 1 || capacidad > 10) return "La capacidad debe estar entre 1 y 10.";

            using (var db = new HotelContext())
            {
                bool repetido = db.TipoHabitacion
                    .Any(t => t.NombreTipo == nombre && t.IdTipoHabitacion != id);
                if (repetido) return "Ya existe un tipo de habitación con ese nombre.";

                TipoHabitacion tipo;
                if (id == 0)
                {
                    tipo = new TipoHabitacion();
                    db.TipoHabitacion.Add(tipo);
                }
                else
                {
                    tipo = db.TipoHabitacion.Find(id);
                    if (tipo == null) return "El tipo de habitación no existe.";
                }

                tipo.NombreTipo = nombre;
                tipo.Capacidad = capacidad;
                tipo.Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
                tipo.Estado = estado;
                db.SaveChanges();
                return null;
            }
        }

        public string Eliminar(int id)
        {
            using (var db = new HotelContext())
            {
                var tipo = db.TipoHabitacion.Find(id);
                if (tipo == null) return "El tipo de habitación no existe.";

                if (db.Habitacion.Any(h => h.IdTipoHabitacion == id) ||
                    db.Tarifa.Any(x => x.IdTipoHabitacion == id))
                    return "No se puede eliminar: tiene habitaciones o tarifas asociadas. Puedes desactivarlo desde Editar.";

                db.TipoHabitacion.Remove(tipo);
                db.SaveChanges();
                return null;
            }
        }
    }
}