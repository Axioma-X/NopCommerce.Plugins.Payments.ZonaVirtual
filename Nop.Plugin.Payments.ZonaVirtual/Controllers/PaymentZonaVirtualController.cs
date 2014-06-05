using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.ZonaVirtual.Models;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.ZonaVirtual.Controllers
{
    public class PaymentZonaVirtualController : Nop.Web.Framework.Controllers.BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly PaymentSettings _paymentSettings;
        private readonly ZonaVirtualPaymentSettings _ZonaVirtualPaymentSettings;
        private readonly ZonaVirtualHelper _zonaVirtualHelper;

        public PaymentZonaVirtualController(IWorkContext workContext,
            IStoreService storeService, 
            ISettingService settingService, 
            IPaymentService paymentService, 
            IOrderService orderService, 
            IOrderProcessingService orderProcessingService, 
            IStoreContext storeContext,
            ILogger logger, 
            IWebHelper webHelper,
            PaymentSettings paymentSettings,
            ZonaVirtualPaymentSettings ZonaVirtualPaymentSettings)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._storeContext = storeContext;
            this._logger = logger;
            this._webHelper = webHelper;
            this._paymentSettings = paymentSettings;
            this._ZonaVirtualPaymentSettings = ZonaVirtualPaymentSettings;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var ZonaVirtualPaymentSettings = _settingService.LoadSetting<ZonaVirtualPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
          
            model.RutaTienda = ZonaVirtualPaymentSettings.RutaTienda;
            model.ID_Clave = ZonaVirtualPaymentSettings.ID_Clave;
            model.ID_Tienda = ZonaVirtualPaymentSettings.ID_Tienda;
            model.NombreTienda = ZonaVirtualPaymentSettings.NombreTienda;
           
                       return View("Nop.Plugin.Payments.ZonaVirtual.Views.PaymentZonaVirtual.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var ZonaVirtualPaymentSettings = _settingService.LoadSetting<ZonaVirtualPaymentSettings>(storeScope);

            //save settings
          
            ZonaVirtualPaymentSettings.RutaTienda = model.RutaTienda;
            ZonaVirtualPaymentSettings.ID_Clave = model.ID_Clave;
            ZonaVirtualPaymentSettings.ID_Tienda = model.ID_Tienda;
            ZonaVirtualPaymentSettings.NombreTienda = model.NombreTienda;

           
            /* Do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.RutaTienda, storeScope, false);
            _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.NombreTienda, storeScope, false);
            _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.ID_Clave, storeScope, false);
            _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.ID_Tienda, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("Nop.Plugin.Payments.ZonaVirtual.Views.PaymentZonaVirtual.PaymentInfo");
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }
      
        public ActionResult CancelOrder(FormCollection form)
        {
            

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}