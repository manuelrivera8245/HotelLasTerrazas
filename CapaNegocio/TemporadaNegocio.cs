using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaDatos;

namespace CapaNegocio
{
    public class TemporadaFila
    {
        public int IdTemporada { get; set; }
        public string NombreTemporada { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int Dias { get; set; }
        public int TarifasCargadas { get; set; }
        public int TiposActivos { get; set; }
    }

    public class HuecoFechas
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
    }

    public class TemporadaNegocio
    {
        public List<TemporadaFila> Listar()
        {
            using (var db = new HotelContext())
            {
                int tipos = db.TipoHabitacion.Count(t => t.Estado);

                var datos = db.Temporada
                    .OrderBy(t => t.FechaInicio)
                    .Select(t => new
                    {
                        t.IdTemporada,
                        t.NombreTemporada,
                        t.FechaInicio,
                        t.FechaFin,
                        Tarifas = t.Tarifa.Count(x => x.TipoHabitacion.Estado)
                    })
                    .ToList();

                return datos.Select(d => new TemporadaFila
                {
                    IdTemporada = d.IdTemporada,
                    NombreTemporada = d.NombreTemporada,
                    FechaInicio = d.FechaInicio,
                    FechaFin = d.FechaFin,
                    Dias = (d.FechaFin - d.FechaInicio).Days + 1,
                    TarifasCargadas = d.Tarifas,
                    TiposActivos = tipos
                }).ToList();
            }
        }

        public List<HuecoFechas> Huecos()
        {
            using (var db = new HotelContext())
            {
                var lista = db.Temporada
                    .OrderBy(t => t.FechaInicio)
                    .Select(t => new { t.FechaInicio, t.FechaFin })
                    .ToList();

                var huecos = new List<HuecoFechas>();
                if (lista.Count == 0) return huecos;

                DateTime libre = lista[0].FechaFin.AddDays(1);
                for (int i = 1; i < lista.Count; i++)
                {
                    if (lista[i].FechaInicio > libre)
                        huecos.Add(new HuecoFechas { Desde = libre, Hasta = lista[i].FechaInicio.AddDays(-1) });

                    DateTime siguiente = lista[i].FechaFin.AddDays(1);
                    if (siguiente > libre) libre = siguiente;
                }
                return huecos;
            }
        }

        public Temporada Obtener(int id)
        {
            using (var db = new HotelContext())
            {
                return db.Temporada.AsNoTracking()
                    .FirstOrDefault(t => t.IdTemporada == id);
            }
        }

        public string Guardar(int id, string nombre, DateTime inicio, DateTime fin)
        {
            nombre = (nombre ?? "").Trim();
            inicio = inicio.Date;
            fin = fin.Date;

            if (nombre.Length == 0) return "El nombre es obligatorio.";
            if (nombre.Length > 40) return "El nombre admite máximo 40 caracteres.";
            if (fin < inicio) return "La fecha de fin no puede ser anterior a la de inicio.";

            using (var db = new HotelContext())
            {
                var choque = db.Temporada.FirstOrDefault(t =>
                    t.IdTemporada != id && t.FechaInicio <= fin && t.FechaFin >= inicio);
                if (choque != null)
                    return string.Format("Las fechas se cruzan con la temporada «{0}» ({1:dd/MM/yyyy} al {2:dd/MM/yyyy}).",
                        choque.NombreTemporada, choque.FechaInicio, choque.FechaFin);

                Temporada temp;
                if (id == 0)
                {
                    temp = new Temporada();
                    db.Temporada.Add(temp);
                }
                else
                {
                    temp = db.Temporada.Find(id);
                    if (temp == null) return "La temporada no existe.";
                }

                temp.NombreTemporada = nombre;
                temp.FechaInicio = inicio;
                temp.FechaFin = fin;
                db.SaveChanges();
                return null;
            }
        }

        public string Eliminar(int id)
        {
            using (var db = new HotelContext())
            {
                var temp = db.Temporada.Find(id);
                if (temp == null) return "La temporada no existe.";

                var tarifas = db.Tarifa.Where(x => x.IdTemporada == id).ToList();
                db.Tarifa.RemoveRange(tarifas);
                db.Temporada.Remove(temp);
                db.SaveChanges();
                return null;
            }
        }
    }
}
