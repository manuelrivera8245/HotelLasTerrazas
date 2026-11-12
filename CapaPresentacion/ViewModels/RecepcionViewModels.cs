using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CapaNegocio;

namespace CapaPresentacion.ViewModels
{
    public class ReservasRecepcionViewModel
    {
        public string Estado { get; set; }
        public DateTime? Fecha { get; set; }
        public List<string> Estados { get; set; }
        public List<ReservaRecepcionFila> Filas { get; set; }
    }

    public class CancelarRecepcionViewModel
    {
        public int IdReserva { get; set; }
        public string Cliente { get; set; }
        public string Fechas { get; set; }
        public string Habitaciones { get; set; }

        public string FiltroEstado { get; set; }
        public string FiltroFecha { get; set; }

        [Required(ErrorMessage = "Indica el motivo de la cancelación.")]
        [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
        [Display(Name = "Motivo *")]
        public string Motivo { get; set; }
    }
}