using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CapaNegocio;

namespace CapaPresentacion.Filters
{
    public class AutorizarRolAttribute : AuthorizeAttribute
    {
        private readonly string[] roles;

        public AutorizarRolAttribute(params string[] roles)
        {
            this.roles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var s = httpContext.Session["Usuario"] as SesionUsuario;
            if (s == null) return false;
            return roles.Length == 0 || roles.Contains(s.Rol);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext ctx)
        {
            if (ctx.HttpContext.Session["Usuario"] == null)
            {
                ctx.Result = new RedirectToRouteResult(new RouteValueDictionary(
                    new { controller = "Cuenta", action = "Login", returnUrl = ctx.HttpContext.Request.RawUrl }));
            }
            else
            {
                ctx.Controller.TempData["Aviso"] = "No tienes permiso para entrar a esa sección.";
                ctx.Result = new RedirectToRouteResult(new RouteValueDictionary(
                    new { controller = "Home", action = "Index" }));
            }
        }
    }
}