using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    // Coinciden con los IDs fijos de los catálogos del script SQL
    public static class EstadoReservaId
    {
        public const int Pendiente = 1;
        public const int Confirmada = 2;
        public const int CheckIn = 3;
        public const int CheckOut = 4;
        public const int Cancelada = 5;
    }

    public static class EstadoHabitacionId
    {
        public const int Disponible = 1;
        public const int Ocupada = 2;
        public const int Mantenimiento = 3;
        public const int Limpieza = 4;
    }

    // Para llenar listas desplegables
    public class ItemCombo
    {
        public int Id { get; set; }
        public string Texto { get; set; }
    }
}