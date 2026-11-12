using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CapaNegocio;

namespace CapaPresentacion.ViewModels
{
    public class BuscarViewModel
    {
        public DateTime? Entrada { get; set; }
        public DateTime? Salida { get; set; }
        public int? IdTipo { get; set; }
        public List<ItemCombo> Tipos { get; set; }
        public ResultadoBusqueda Resultado { get; set; }
    }
}