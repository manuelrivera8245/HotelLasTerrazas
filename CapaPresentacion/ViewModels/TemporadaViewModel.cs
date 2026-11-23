using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CapaPresentacion.ViewModels
{
    public class TemporadaViewModel
    {
        public int IdTemporada { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(40, ErrorMessage = "Máximo 40 caracteres.")]
        [Display(Name = "Nombre *")]
        public string NombreTemporada { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [Display(Name = "Fecha de inicio *")]
        public DateTime? FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        [Display(Name = "Fecha de fin *")]
        public DateTime? FechaFin { get; set; }
    }
}