using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.ZonaVirtual
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //PDT
            routes.MapRoute("Plugin.Payments.ZonaVirtual.PDTHandler",
                 "Plugins/PaymentZonaVirtual/PDTHandler",
                 new { controller = "PaymentZonaVirtual", action = "PDTHandler" },
                 new[] { "Nop.Plugin.Payments.ZonaVirtual.Controllers" }
            );
            //IPN
            routes.MapRoute("Plugin.Payments.ZonaVirtual.IPNHandler",
                 "Plugins/PaymentZonaVirtual/IPNHandler",
                 new { controller = "PaymentZonaVirtual", action = "IPNHandler" },
                 new[] { "Nop.Plugin.Payments.ZonaVirtual.Controllers" }
            );
            //Cancel
            routes.MapRoute("Plugin.Payments.ZonaVirtual.CancelOrder",
                 "Plugins/PaymentZonaVirtual/CancelOrder",
                 new { controller = "PaymentZonaVirtual", action = "CancelOrder" },
                 new[] { "Nop.Plugin.Payments.ZonaVirtual.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
