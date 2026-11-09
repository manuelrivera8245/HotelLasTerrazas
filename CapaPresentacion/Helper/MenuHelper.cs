using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CapaPresentacion.Helpers
{
    public class ItemMenu
    {
        public string Texto { get; set; }
        public string Controlador { get; set; }
        public string Accion { get; set; }
        public bool Listo { get; set; }
    }

    public static class MenuHelper
    {
        private static readonly HashSet<string> Listos = new HashSet<string>
        {
            "Home/Index",
            "TiposHabitacion/Index",
            "Habitaciones/Index"
        };

        public static List<ItemMenu> Para(string rol)
        {
            var m = new List<ItemMenu>();
            switch (rol)
            {
                case "Administrador":
                    m.Add(I("Vista general", "Home", "Index"));
                    m.Add(I("Habitaciones", "Habitaciones", "Index"));
                    m.Add(I("Tipos de habitación", "TiposHabitacion", "Index"));
                    m.Add(I("Temporadas", "Temporadas", "Index"));
                    m.Add(I("Tarifas", "Tarifas", "Index"));
                    m.Add(I("Reportes", "Reportes", "Index"));
                    break;
                case "Recepcionista":
                    m.Add(I("Vista general", "Home", "Index"));
                    m.Add(I("Reservas", "Recepcion", "Index"));
                    m.Add(I("Pagos", "Recepcion", "Pagos"));
                    m.Add(I("Habitaciones", "Recepcion", "Habitaciones"));
                    break;
                case "Cliente":
                    m.Add(I("Inicio", "Home", "Index"));
                    m.Add(I("Nueva reserva", "Reserva", "Buscar"));
                    m.Add(I("Mis reservas", "Reserva", "MisReservas"));
                    break;
            }
            return m;
        }

        private static ItemMenu I(string texto, string controlador, string accion)
        {
            return new ItemMenu
            {
                Texto = texto,
                Controlador = controlador,
                Accion = accion,
                Listo = Listos.Contains(controlador + "/" + accion)
            };
        }
    }
}