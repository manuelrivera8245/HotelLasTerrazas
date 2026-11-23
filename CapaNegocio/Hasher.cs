using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace CapaNegocio
{
    public static class Hasher
    {
        private const int Iteraciones = 10000;
        private const int TamSalt = 16;
        private const int TamHash = 32;

        public static string Crear(string clave)
        {
            byte[] salt = new byte[TamSalt];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            byte[] hash = Derivar(clave, salt, Iteraciones);
            return "PBKDF2$" + Iteraciones + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
        }

        public static bool Verificar(string clave, string guardado)
        {
            if (string.IsNullOrEmpty(clave) || string.IsNullOrEmpty(guardado)) return false;
            string[] p = guardado.Split('$');
            if (p.Length != 4 || p[0] != "PBKDF2") return false;
            int iter;
            if (!int.TryParse(p[1], out iter)) return false;

            byte[] salt = Convert.FromBase64String(p[2]);
            byte[] esperado = Convert.FromBase64String(p[3]);
            byte[] actual = Derivar(clave, salt, iter);

            if (esperado.Length != actual.Length) return false;
            int dif = 0;
            for (int i = 0; i < esperado.Length; i++) dif |= esperado[i] ^ actual[i];
            return dif == 0;
        }

        private static byte[] Derivar(string clave, byte[] salt, int iter)
        {
            using (var kdf = new Rfc2898DeriveBytes(clave, salt, iter))
            {
                return kdf.GetBytes(TamHash);
            }
        }
    }
}