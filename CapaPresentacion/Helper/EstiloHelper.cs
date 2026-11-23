using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.IO;

namespace CapaPresentacion.Helpers
{
    public static class EstiloHelper
    {
        public static string Hotel()
        {
            string rel = "~/Content/hotel.css";
            string url = VirtualPathUtility.ToAbsolute(rel);
            string fisica = HostingEnvironment.MapPath(rel);
            if (fisica == null || !File.Exists(fisica)) return url;
            return url + "?v=" + File.GetLastWriteTimeUtc(fisica).Ticks;
        }
    }
}