using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.ZonaVirtual.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Tax;
using System.Collections;

namespace Nop.Plugin.Payments.ZonaVirtual
{
    /// <summary>
    /// ZonaVirtual payment processor
    /// </summary>
    public class ZonaVirtualPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly ZonaVirtualPaymentSettings _ZonaVirtualPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ITaxService _taxService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly HttpContextBase _httpContext;

        private WebRequest theRequest;
        private HttpWebResponse theResponse;
        private ArrayList theQueryData;
       
        #endregion

        #region Ctor

        public ZonaVirtualPaymentProcessor(ZonaVirtualPaymentSettings ZonaVirtualPaymentSettings,
            ISettingService settingService, ICurrencyService currencyService,
            CurrencySettings currencySettings, IWebHelper webHelper,
            ICheckoutAttributeParser checkoutAttributeParser, ITaxService taxService, 
            IOrderTotalCalculationService orderTotalCalculationService, HttpContextBase httpContext)
        {
            this._ZonaVirtualPaymentSettings = ZonaVirtualPaymentSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._taxService = taxService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._httpContext = httpContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets Paypal URL
        /// </summary>
        /// <returns></returns>
        private string GetZonaVirtualUrl()
        {
            return  _ZonaVirtualPaymentSettings.RutaTienda;
            //_ZonaVirtualPaymentSettings.UseSandbox ? "https://www.sandbox.paypal.com/us/cgi-bin/webscr" :
            //    "https://www.paypal.com/us/cgi-bin/webscr";
        }
        /// <summary>
        /// Gets PDT details
        /// </summary>
        /// <param name="tx">TX</param>
        /// <param name="values">Values</param>
        /// <param name="response">Response</param>
        /// <returns>Result</returns>
        public bool GetPDTDetails(string tx, out Dictionary<string, string> values, out string response)
        {
            var req = (HttpWebRequest)WebRequest.Create(GetZonaVirtualUrl());
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            string formContent = string.Format("cmd=_notify-synch&at={0}&tx={1}", _ZonaVirtualPaymentSettings.PdtToken, tx);
            req.ContentLength = formContent.Length;

            using (var sw = new StreamWriter(req.GetRequestStream(), Encoding.ASCII))
                sw.Write(formContent);

            response = null;
            using (var sr = new StreamReader(req.GetResponse().GetResponseStream()))
                response = HttpUtility.UrlDecode(sr.ReadToEnd());

            values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool firstLine = true, success = false;
            foreach (string l in response.Split('\n'))
            {
                string line = l.Trim();
                if (firstLine)
                {
                    success = line.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase);
                    firstLine = false;
                }
                else
                {
                    int equalPox = line.IndexOf('=');
                    if (equalPox >= 0)
                        values.Add(line.Substring(0, equalPox), line.Substring(equalPox + 1));
                }
            }

            return success;
        }

        /// <summary>
        /// Verifies IPN
        /// </summary>
        /// <param name="formString">Form string</param>
        /// <param name="values">Values</param>
        /// <returns>Result</returns>
        public bool VerifyIPN(string formString, out Dictionary<string, string> values)
        {
            var req = (HttpWebRequest)WebRequest.Create(GetZonaVirtualUrl());
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            //
            string formContent = string.Format("{0}&cmd=_notify-validate", formString);
            req.ContentLength = formContent.Length;

            using (var sw = new StreamWriter(req.GetRequestStream(), Encoding.ASCII))
            {
                sw.Write(formContent);
            }

            string response = null;
            using (var sr = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                response = HttpUtility.UrlDecode(sr.ReadToEnd());
            }
            bool success = response.Trim().Equals("VERIFIED", StringComparison.OrdinalIgnoreCase);

            values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string l in formString.Split('&'))
            {
                string line = l.Trim();
                int equalPox = line.IndexOf('=');
                if (equalPox >= 0)
                    values.Add(line.Substring(0, equalPox), line.Substring(equalPox + 1));
            }

            return success;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            // Datos necesarios para Zona Virtual

            string Id_pago = HttpContext.Current.Session["order_id_temp"].ToString();

            ZPagosVerificar.Service Verificar = new ZPagosVerificar.Service();
            var VerificarDemo = new ZPagosVerificarDemo.Service();

            int error = 0;
            string errorStr = "";
            int res = -1;
            if (_ZonaVirtualPaymentSettings.RutaTienda.IndexOf("demo") > 0)
            {
                ZPagosVerificarDemo.pagos_v3[] respuesta = new ZPagosVerificarDemo.pagos_v3[1];
                res = VerificarDemo.verificar_pago_v3(Id_pago, _ZonaVirtualPaymentSettings.ID_Tienda, _ZonaVirtualPaymentSettings.ID_Clave, ref respuesta, ref error, ref errorStr);
                if (respuesta.Length > 0)
                {
                    if (respuesta[0].int_estado_pago == 888)
                    {
                        res = 888;
                        postProcessPaymentRequest.Order.OrderStatus = OrderStatus.Cancelled;
                        postProcessPaymentRequest.Order.PaymentStatus = PaymentStatus.Refunded;

                    }

                    else if (respuesta[0].int_estado_pago == 999)
                    {
                        res = 999;
                        postProcessPaymentRequest.Order.OrderStatus = OrderStatus.Pending;
                        postProcessPaymentRequest.Order.PaymentStatus = PaymentStatus.Pending;

                    }

                    else if (respuesta[0].int_estado_pago == 1)
                    {
                        res = 1;
                        postProcessPaymentRequest.Order.OrderStatus = OrderStatus.Complete;
                        postProcessPaymentRequest.Order.PaymentStatus = PaymentStatus.Authorized;


                    }
                    else
                    {
                        postProcessPaymentRequest.Order.OrderStatus = OrderStatus.Cancelled;
                        postProcessPaymentRequest.Order.PaymentStatus = PaymentStatus.Refunded;

                    }

                }
                else
                {
                    postProcessPaymentRequest.Order.OrderStatus = OrderStatus.Cancelled;
                    postProcessPaymentRequest.Order.PaymentStatus = PaymentStatus.Refunded;

                }

            }
            else
            {
                ZPagosVerificar.pagos_v3[] respuesta = new ZPagosVerificar.pagos_v3[1];
                res = Verificar.verificar_pago_v3(Id_pago, _ZonaVirtualPaymentSettings.ID_Tienda, _ZonaVirtualPaymentSettings.ID_Clave, ref respuesta, ref error, ref errorStr);
                if (respuesta.Length > 0)
                {
                    if (respuesta[0].int_estado_pago == 888)
                    {
                        res = 888;
                        postProcessPaymentRequest.Order.OrderStatus = OrderStatus.Cancelled;
                        postProcessPaymentRequest.Order.PaymentStatus = PaymentStatus.Refunded;

                    }

                    else if (respuesta[0].int_estado_pago == 999)
                    {
                        res = 999;
                        postProcessPaymentRequest.Order.OrderStatus = OrderStatus.Pending;
                        postProcessPaymentRequest.Order.PaymentStatus = PaymentStatus.Pending;

                    }

                    else if (respuesta[0].int_estado_pago == 1)
                    {
                        res = 1;
                        postProcessPaymentRequest.Order.OrderStatus = OrderStatus.Complete;
                        postProcessPaymentRequest.Order.PaymentStatus = PaymentStatus.Authorized;

                    }
                    else
                    {
                        postProcessPaymentRequest.Order.OrderStatus = OrderStatus.Cancelled;
                        postProcessPaymentRequest.Order.PaymentStatus = PaymentStatus.Refunded;

                    }


                }
                else
                {
                    postProcessPaymentRequest.Order.OrderStatus = OrderStatus.Cancelled;
                    postProcessPaymentRequest.Order.PaymentStatus = PaymentStatus.Refunded;

                }
            }

            Nop.Core.Domain.Orders.OrderNote note = new Core.Domain.Orders.OrderNote();
            note.CreatedOnUtc = DateTime.UtcNow;
            note.DisplayToCustomer = false;
            note.Note = "ID de pago en Zona Virtual: " + Id_pago;
            note.OrderId = postProcessPaymentRequest.Order.Id;
            postProcessPaymentRequest.Order.OrderNotes.Add(note);

            System.Web.HttpContext.Current.Response.Clear();
            System.Web.HttpContext.Current.Response.Write(string.Format("</head><body onload=\"document.{0}.submit()\">", "FormName"));
            System.Web.HttpContext.Current.Response.Write(string.Format("<form name=\"{0}\" method=\"{1}\" action=\"{2}\" >", "FormName", "POST", "../orderdetails/" + postProcessPaymentRequest.Order.Id.ToString()));
            System.Web.HttpContext.Current.Response.Write("</form>");
            System.Web.HttpContext.Current.Response.Write("</body></html>");
            System.Web.HttpContext.Current.Response.End();

        }
        public void WebPostRequest(string url)
		{
			theRequest = WebRequest.Create(url);
			theRequest.Method = "POST";
			theQueryData = new ArrayList();
		}

		public void Add(string key, string value)
		{
			theQueryData.Add(String.Format("{0}={1}",key,HttpUtility.UrlEncode(value)));
		}

		public string GetResponse()
		{
			// Set the encoding type
			theRequest.ContentType="application/x-www-form-urlencoded";

			// Build a string containing all the parameters
			string Parameters = String.Join("&",(String[]) theQueryData.ToArray(typeof(string)));
			theRequest.ContentLength = Parameters.Length;

			// We write the parameters into the request
			StreamWriter sw = new StreamWriter(theRequest.GetRequestStream());
  			sw.Write(Parameters);
  			sw.Close();

			// Execute the query
			theResponse =  (HttpWebResponse)theRequest.GetResponse();
  			StreamReader sr = new StreamReader(theResponse.GetResponseStream());
   			return sr.ReadToEnd();
		}

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _ZonaVirtualPaymentSettings.AdditionalFee, _ZonaVirtualPaymentSettings.AdditionalFeePercentage);
            return result;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");
            
            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return false;

            return true;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentZonaVirtual";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.ZonaVirtual.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentZonaVirtual";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.ZonaVirtual.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentZonaVirtualController);
        }

        public override void Install()
        {
            //settings
            var settings = new ZonaVirtualPaymentSettings()
            {
                //UseSandbox = true,
              //  BusinessEmail = "test@test.com",
                PdtToken= "Your PDT token here...",
                PdtValidateOrderTotal = true,
                EnableIpn = true,
            };
            _settingService.SaveSetting(settings);

           
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.StoreName", "Store Name");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.StoreName.Hint", "The name of the Store for Zona Virtual");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.URL", "URL Payment Button");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.URL.Hint", "URL provided for Zona Virtual");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.IDStore", "Unique ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.IDStore.Hint", "Unique ID");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.IDKey", "WebService Password");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.IDKey.Hint", "Password provided for Zona Virtual");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.ServiceCode", "Service Code");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.ServiceCode.Hint", "Service Code provided for Zona Virtual");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Front.Message", "Thanks for shopping with Zona Virtual");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Front.TitleDialog", "Payment process in Zona Virtual");
            base.Install();
        }
        
        
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<ZonaVirtualPaymentSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.RedirectionTip");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.UseSandbox");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.UseSandbox.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.BusinessEmail");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.BusinessEmail.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.PDTToken");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.PDTToken.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.PDTValidateOrderTotal");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.PDTValidateOrderTotal.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.AdditionalFee.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.AdditionalFeePercentage");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.AdditionalFeePercentage.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.PassProductNamesAndTotals");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.PassProductNamesAndTotals.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.EnableIpn");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.EnableIpn.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.EnableIpn.Hint2");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.IpnUrl");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.IpnUrl.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage.Hint");

            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.StoreName");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.StoreName.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.URL");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.URL.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.IDStore");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.IDStore.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.IDKey");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.IDKey.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.ServiceCode");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.ServiceCode.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Front.Message");
            this.DeletePluginLocaleResource("Plugins.Payments.ZonaVirtual.Front.TitleDialog");
            
            base.Uninstall();
        }

        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get
            {
                return false;
            }
        }

        #endregion
    }
}
