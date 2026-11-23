using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure;
using System.Text.RegularExpressions;
using CapaDatos;

namespace CapaNegocio
{
    public class RegistroClienteResultado
    {
        public string Campo { get; set; }
        public string Error { get; set; }
        public bool Ok { get { return Error == null; } }
    }

    public class RegistroNegocio
    {
        private static readonly Regex Nombre = new Regex(@"^[\p{L}][\p{L} '\-]*$", RegexOptions.Compiled);
        private static readonly Regex Telefono = new Regex(@"^[0-9+\-\s()]{7,20}$", RegexOptions.Compiled);
        private static readonly Regex Alfanumerico = new Regex(@"^[A-Za-z0-9]+$", RegexOptions.Compiled);
        private static readonly Regex Ocho = new Regex(@"^[0-9]{8}$", RegexOptions.Compiled);

        public RegistroClienteResultado RegistrarCliente(string nombres, string apellidos, string tipoDoc,
            string numDoc, string telefono, string correo, string clave)
        {
            nombres = Compactar(nombres);
            apellidos = Compactar(apellidos);
            tipoDoc = (tipoDoc ?? "").Trim();
            numDoc = (numDoc ?? "").Trim().ToUpperInvariant();
            telefono = (telefono ?? "").Trim();
            correo = (correo ?? "").Trim().ToLowerInvariant();
            clave = clave ?? "";

            if (nombres.Length < 2 || nombres.Length > 80 || !Nombre.IsMatch(nombres))
                return Falla("Nombres", "Escribe tus nombres usando solo letras (de 2 a 80 caracteres).");
            if (apellidos.Length < 2 || apellidos.Length > 80 || !Nombre.IsMatch(apellidos))
                return Falla("Apellidos", "Escribe tus apellidos usando solo letras (de 2 a 80 caracteres).");

            if (tipoDoc != "DNI" && tipoDoc != "CE" && tipoDoc != "Pasaporte")
                return Falla("TipoDocumento", "Elige un tipo de documento válido.");
            string errDoc = ValidarDocumento(tipoDoc, numDoc);
            if (errDoc != null) return Falla("NumeroDocumento", errDoc);

            if (telefono.Length > 0 && !Telefono.IsMatch(telefono))
                return Falla("Telefono", "El teléfono admite solo números, espacios, + y guiones (de 7 a 20 caracteres).");

            if (correo.Length > 50) return Falla("Correo", "El correo admite máximo 50 caracteres.");
            if (!CorreoValido(correo)) return Falla("Correo", "Escribe un correo válido, por ejemplo nombre@correo.com.");

            if (clave.Length < 8 || clave.Length > 64)
                return Falla("Clave", "La contraseña debe tener entre 8 y 64 caracteres.");
            if (!clave.Any(char.IsLetter) || !clave.Any(char.IsDigit))
                return Falla("Clave", "La contraseña debe combinar letras y números.");

            using (var db = new HotelContext())
            {
                if (db.Usuario.Any(u => u.NombreUsuario == correo))
                    return Falla("Correo", "Ya existe una cuenta con ese correo. Inicia sesión o usa otro correo.");
                if (db.Cliente.Any(c => c.NumeroDocumento == numDoc))
                    return Falla("NumeroDocumento", "Ya hay un cliente registrado con ese documento.");

                var rol = db.Rol.FirstOrDefault(r => r.NombreRol == "Cliente");
                if (rol == null) return Falla("", "No se encontró el rol Cliente en la base de datos.");

                var usuario = new Usuario
                {
                    NombreUsuario = correo,
                    ClaveHash = Hasher.Crear(clave),
                    IdRol = rol.IdRol,
                    Estado = true,
                    FechaCreacion = DateTime.Now
                };
                usuario.Cliente.Add(new Cliente
                {
                    TipoDocumento = tipoDoc,
                    NumeroDocumento = numDoc,
                    Nombres = nombres,
                    Apellidos = apellidos,
                    Telefono = telefono.Length == 0 ? null : telefono,
                    Correo = correo,
                    Direccion = null
                });
                db.Usuario.Add(usuario);

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    return Falla("", "No se pudo crear la cuenta. Es posible que el correo o el documento ya estén registrados.");
                }
            }
            return new RegistroClienteResultado();
        }


        private static RegistroClienteResultado Falla(string campo, string error)
        {
            return new RegistroClienteResultado { Campo = campo, Error = error };
        }

        private static string Compactar(string s)
        {
            return Regex.Replace((s ?? "").Trim(), @"\s+", " ");
        }

        private static string ValidarDocumento(string tipo, string num)
        {
            switch (tipo)
            {
                case "DNI":
                    return Ocho.IsMatch(num) ? null : "El DNI debe tener exactamente 8 dígitos.";
                case "CE":
                    return (Alfanumerico.IsMatch(num) && num.Length >= 9 && num.Length <= 12)
                        ? null : "El carné de extranjería debe tener de 9 a 12 letras o números.";
                default:
                    return (Alfanumerico.IsMatch(num) && num.Length >= 6 && num.Length <= 12)
                        ? null : "El pasaporte debe tener de 6 a 12 letras o números.";
            }
        }

        private static bool CorreoValido(string c)
        {
            try
            {
                var m = new System.Net.Mail.MailAddress(c);
                int arroba = c.IndexOf('@');
                return m.Address == c && arroba > 0 && c.IndexOf('.', arroba) > arroba + 1;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
