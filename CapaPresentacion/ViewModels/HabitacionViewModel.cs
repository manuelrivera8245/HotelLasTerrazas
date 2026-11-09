using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CapaPresentacion.ViewModels
{
    public class HabitacionViewModel
    {
        public int IdHabitacion { get; set; }

        [Required(ErrorMessage = "El número es obligatorio.")]
        [StringLength(10, ErrorMessage = "Máximo 10 caracteres.")]
        [Display(Name = "Número de habitación *")]
        public string NumeroHabitacion { get; set; }

        [Range(1, 20, ErrorMessage = "El piso debe estar entre 1 y 20.")]
        [Display(Name = "Piso *")]
        public int Piso { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Elige un tipo de habitación.")]
        [Display(Name = "Tipo de habitación *")]
        public int IdTipoHabitacion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Elige un estado.")]
        [Display(Name = "Estado *")]
        public int IdEstadoHabitacion { get; set; }

        public IEnumerable<SelectListItem> Tipos { get; set; }
        public IEnumerable<SelectListItem> Estados { get; set; }
    }
}