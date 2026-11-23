using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CapaPresentacion.ViewModels
{
    public class PrecioTipoItem
    {
        public int IdTipoHabitacion { get; set; }
        public string NombreTipo { get; set; }
        public string Precio { get; set; }
    }

    public class TarifasTemporadaViewModel
    {
        public int IdTemporada { get; set; }
        public string NombreTemporada { get; set; }
        public string Rango { get; set; }
        public List<PrecioTipoItem> Items { get; set; }

        public TarifasTemporadaViewModel()
        {
            Items = new List<PrecioTipoItem>();
        }
    }
}