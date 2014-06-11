using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.ZonaVirtual.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.ZonaVirtual.Fields.StoreName")]
        public string NombreTienda { get; set; }
        [NopResourceDisplayName("Plugins.Payments.ZonaVirtual.Fields.URL")]
        public string RutaTienda { get; set; }
         [NopResourceDisplayName("Plugins.Payments.ZonaVirtual.Fields.IDStore")]
        public int ID_Tienda { get; set; }
         [NopResourceDisplayName("Plugins.Payments.ZonaVirtual.Fields.IDKey")]
        public string ID_Clave { get; set; }
       
    }
}