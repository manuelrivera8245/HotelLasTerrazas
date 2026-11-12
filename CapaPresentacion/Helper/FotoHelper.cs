using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Hosting;

namespace CapaPresentacion.Helpers
{
    public static class FotoHelper
    {
        public static string Habitacion(string nombreTipo, int numero = 1)
        {
            string rel = "~/Content/img/habitaciones/" + Slug(nombreTipo) + "-" + numero + ".webp";
            string fisica = HostingEnvironment.MapPath(rel);
            if (fisica == null || !File.Exists(fisica)) return null;
            return VirtualPathUtility.ToAbsolute(rel);
        }

        private static string Slug(string texto)
        {
            string d = (texto ?? "").Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in d)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                sb.Append(c == ' ' ? '-' : c);
            }
            return sb.ToString();
        }
    }
}