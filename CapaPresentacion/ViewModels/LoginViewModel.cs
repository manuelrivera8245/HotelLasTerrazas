using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CapaPresentacion.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ingresa tu correo electrónico.")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "Ingresa tu contraseña.")]
        [Display(Name = "Contraseña")]
        public string Clave { get; set; }
    }
}