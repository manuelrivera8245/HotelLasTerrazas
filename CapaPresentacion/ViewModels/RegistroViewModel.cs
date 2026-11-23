using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CapaPresentacion.ViewModels
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "Escribe tus nombres.")]
        [StringLength(80, ErrorMessage = "Máximo 80 caracteres.")]
        [Display(Name = "Nombres *")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Escribe tus apellidos.")]
        [StringLength(80, ErrorMessage = "Máximo 80 caracteres.")]
        [Display(Name = "Apellidos *")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "Elige el tipo de documento.")]
        [Display(Name = "Tipo de documento *")]
        public string TipoDocumento { get; set; }

        [Required(ErrorMessage = "Escribe tu número de documento.")]
        [StringLength(12, ErrorMessage = "Máximo 12 caracteres.")]
        [Display(Name = "N° de documento *")]
        public string NumeroDocumento { get; set; }

        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "Escribe tu correo.")]
        [EmailAddress(ErrorMessage = "Escribe un correo válido.")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
        [Display(Name = "Correo electrónico *")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "Escribe una contraseña.")]
        [StringLength(64, ErrorMessage = "Máximo 64 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña *")]
        public string Clave { get; set; }

        [Required(ErrorMessage = "Confirma tu contraseña.")]
        [System.ComponentModel.DataAnnotations.Compare("Clave", ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña *")]
        public string ConfirmarClave { get; set; }

        public IEnumerable<SelectListItem> Tipos { get; set; }
    }
}