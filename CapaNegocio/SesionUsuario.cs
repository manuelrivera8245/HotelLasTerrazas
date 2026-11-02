using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    public class SesionUsuario
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string Rol { get; set; }
        public string NombreMostrar { get; set; }
        public int? IdCliente { get; set; }
    }
}
