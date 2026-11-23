using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CapaPresentacion.ViewModels
{
    public class TipoHabitacionViewModel
    {
        public int IdTipoHabitacion { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(40, ErrorMessage = "Máximo 40 caracteres.")]
        [Display(Name = "Nombre *")]
        public string NombreTipo { get; set; }

        [Range(1, 10, ErrorMessage = "La capacidad debe estar entre 1 y 10.")]
        [Display(Name = "Capacidad (huéspedes) *")]
        public int Capacidad { get; set; }

        [StringLength(250, ErrorMessage = "Máximo 250 caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        public bool Estado { get; set; }
    }
}