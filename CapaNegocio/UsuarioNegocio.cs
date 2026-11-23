using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using CapaDatos;

namespace CapaNegocio
{
    public class UsuarioNegocio
    {
        public SesionUsuario ValidarLogin(string correo, string clave)
        {
            using (var db = new HotelContext())
            {
                var u = db.Usuario.Include(x => x.Rol)
                    .FirstOrDefault(x => x.NombreUsuario == correo && x.Estado);

                if (u == null || !Hasher.Verificar(clave, u.ClaveHash)) return null;

                var s = new SesionUsuario
                {
                    IdUsuario = u.IdUsuario,
                    NombreUsuario = u.NombreUsuario,
                    Rol = u.Rol.NombreRol,
                    NombreMostrar = u.NombreUsuario
                };

                var c = db.Cliente.FirstOrDefault(x => x.IdUsuario == u.IdUsuario);
                if (c != null)
                {
                    s.IdCliente = c.IdCliente;
                    s.NombreMostrar = c.Nombres + " " + c.Apellidos;
                }
                else
                {
                    var e = db.Empleado.FirstOrDefault(x => x.IdUsuario == u.IdUsuario);
                    if (e != null) s.NombreMostrar = e.Nombres + " " + e.Apellidos;
                }
                return s;
            }
        }

        // Solo para desarrollo: cambia los textos de ejemplo del script (HASH_..._SIMULADO)
        // por hashes reales. Se quita antes de la entrega final.
        public void PrepararUsuariosDemo()
        {
            var claves = new Dictionary<string, string>
            {
                { "admin@hotellasterrazas.com", "Admin123*" },
                { "recepcion@hotellasterrazas.com", "Recep123*" },
                { "jperez@gmail.com", "Cliente123*" }
            };

            using (var db = new HotelContext())
            {
                foreach (var par in claves)
                {
                    var u = db.Usuario.FirstOrDefault(x => x.NombreUsuario == par.Key);
                    if (u != null && u.ClaveHash.StartsWith("HASH_"))
                        u.ClaveHash = Hasher.Crear(par.Value);
                }
                db.SaveChanges();
            }
        }
    }
}