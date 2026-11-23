using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CapaNegocio;

namespace CapaPresentacion.ViewModels
{
    public class CobroViewModel
    {
        public int IdReserva { get; set; }
        public string Cliente { get; set; }
        public string Documento { get; set; }
        public string Fechas { get; set; }
        public string Habitaciones { get; set; }
        public string EstadoReserva { get; set; }
        public decimal Total { get; set; }
        public string Bloqueo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Elige el método de pago.")]
        [Display(Name = "Método de pago *")]
        public int IdMetodoPago { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Elige el tipo de comprobante.")]
        [Display(Name = "Tipo de comprobante *")]
        public int IdTipoComprobante { get; set; }

        public IEnumerable<SelectListItem> Metodos { get; set; }
        public IEnumerable<SelectListItem> Tipos { get; set; }

        public string FiltroEstado { get; set; }
        public string FiltroFecha { get; set; }
    }

    public class PagosViewModel
    {
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
        public int? IdMetodo { get; set; }
        public List<ItemCombo> Metodos { get; set; }
        public HistorialPagos Historial { get; set; }
    }
}