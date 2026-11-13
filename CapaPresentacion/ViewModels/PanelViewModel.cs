using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CapaNegocio;

namespace CapaPresentacion.ViewModels
{
    public class PanelViewModel
    {
        public SesionUsuario Sesion { get; set; }
        public DateTime Hoy { get; set; }
        public PanelAdmin Admin { get; set; }
        public PanelRecepcion Recepcion { get; set; }
        public PanelCliente Cliente { get; set; }
    }
}