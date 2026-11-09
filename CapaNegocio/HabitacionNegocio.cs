using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class HabitacionFila
    {
        public int IdHabitacion { get; set; }
        public string NumeroHabitacion { get; set; }
        public int Piso { get; set; }
        public string NombreTipo { get; set; }
        public int Capacidad { get; set; }
        public string Estado { get; set; }
    }

    public class HabitacionNegocio
    {
        public List<HabitacionFila> Listar()
        {
            using (var db = new HotelContext())
            {
                return db.Habitacion
                    .OrderBy(h => h.NumeroHabitacion)
                    .Select(h => new HabitacionFila
                    {
                        IdHabitacion = h.IdHabitacion,
                        NumeroHabitacion = h.NumeroHabitacion,
                        Piso = h.Piso,
                        NombreTipo = h.TipoHabitacion.NombreTipo,
                        Capacidad = h.TipoHabitacion.Capacidad,
                        Estado = h.EstadoHabitacion.NombreEstado
                    })
                    .ToList();
            }
        }

        public Habitacion Obtener(int id)
        {
            using (var db = new HotelContext())
            {
                return db.Habitacion.AsNoTracking()
                    .FirstOrDefault(h => h.IdHabitacion == id);
            }
        }

        public List<ItemCombo> TiposParaCombo(int idActual)
        {
            using (var db = new HotelContext())
            {
                return db.TipoHabitacion
                    .Where(t => t.Estado || t.IdTipoHabitacion == idActual)
                    .OrderBy(t => t.NombreTipo)
                    .Select(t => new ItemCombo { Id = t.IdTipoHabitacion, Texto = t.NombreTipo })
                    .ToList();
            }
        }

        public List<ItemCombo> EstadosParaCombo()
        {
            using (var db = new HotelContext())
            {
                return db.EstadoHabitacion
                    .OrderBy(e => e.IdEstadoHabitacion)
                    .Select(e => new ItemCombo { Id = e.IdEstadoHabitacion, Texto = e.NombreEstado })
                    .ToList();
            }
        }

        public string Guardar(int id, string numero, int piso, int idTipo, int idEstado)
        {
            numero = (numero ?? "").Trim();
            if (numero.Length == 0) return "El número de habitación es obligatorio.";
            if (numero.Length > 10) return "El número admite máximo 10 caracteres.";
            if (piso < 1 || piso > 20) return "El piso debe estar entre 1 y 20.";

            using (var db = new HotelContext())
            {
                if (db.Habitacion.Any(h => h.NumeroHabitacion == numero && h.IdHabitacion != id))
                    return "Ya existe una habitación con ese número.";

                var tipo = db.TipoHabitacion.Find(idTipo);
                if (tipo == null) return "El tipo de habitación no existe.";
                if (!db.EstadoHabitacion.Any(e => e.IdEstadoHabitacion == idEstado))
                    return "El estado seleccionado no existe.";

                Habitacion hab;
                if (id == 0)
                {
                    if (!tipo.Estado) return "El tipo de habitación elegido está inactivo.";
                    hab = new Habitacion();
                    db.Habitacion.Add(hab);
                }
                else
                {
                    hab = db.Habitacion.Find(id);
                    if (hab == null) return "La habitación no existe.";

                    if (hab.IdTipoHabitacion != idTipo)
                    {
                        if (!tipo.Estado) return "El tipo de habitación elegido está inactivo.";

                        bool conReservas = db.DetalleReserva.Any(d => d.IdHabitacion == id &&
                            (d.Reserva.IdEstadoReserva == EstadoReservaId.Pendiente ||
                             d.Reserva.IdEstadoReserva == EstadoReservaId.Confirmada ||
                             d.Reserva.IdEstadoReserva == EstadoReservaId.CheckIn));
                        if (conReservas)
                            return "No se puede cambiar el tipo: la habitación tiene reservas activas.";
                    }
                }

                hab.NumeroHabitacion = numero;
                hab.Piso = piso;
                hab.IdTipoHabitacion = idTipo;
                hab.IdEstadoHabitacion = idEstado;
                db.SaveChanges();
                return null;
            }
        }

        public string Eliminar(int id)
        {
            using (var db = new HotelContext())
            {
                var hab = db.Habitacion.Find(id);
                if (hab == null) return "La habitación no existe.";

                if (db.DetalleReserva.Any(d => d.IdHabitacion == id))
                    return "No se puede eliminar: la habitación tiene reservas registradas. Puedes ponerla en Mantenimiento.";

                db.Habitacion.Remove(hab);
                db.SaveChanges();
                return null;
            }
        }
    }
}