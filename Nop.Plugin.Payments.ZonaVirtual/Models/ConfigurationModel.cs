using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.ZonaVirtual.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Nombre Tienda")]
        public string NombreTienda { get; set; }
        [NopResourceDisplayName("Ruta Tienda")]
        public string RutaTienda { get; set; }
         [NopResourceDisplayName("ID Tienda")]
        public int ID_Tienda { get; set; }
         [NopResourceDisplayName("ID Clave")]
        public string ID_Clave { get; set; }
       
    }
}