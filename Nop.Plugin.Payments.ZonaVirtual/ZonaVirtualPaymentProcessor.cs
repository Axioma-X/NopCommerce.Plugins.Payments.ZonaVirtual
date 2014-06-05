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
            decimal total_con_iva = 0;
            decimal valor_iva = 0;
            string Id_pago = "";
            string descrip_pago = "";
            string Txtemail = "";
            string Id_cliente = "";
            short tipo_id_cliente = 0;
            string nombre_cliente = "";
            string apellido_cliente = "";
            string telefono_cliente = "";
            string txtcampo1 = "";
            string txtcampo2 = "";
            string txtcampo3 = "";

            var builder = new StringBuilder();
     
            if (_ZonaVirtualPaymentSettings.PassProductNamesAndTotals)
            {
              //  builder.AppendFormat("&upload=1");

                //get the items in the cart
                decimal cartTotal = decimal.Zero;
                var cartItems = postProcessPaymentRequest.Order.OrderItems;
                int x = 1;
                foreach (var item in cartItems)
                {
                    var unitPriceExclTax = item.UnitPriceExclTax;
                    var priceExclTax = item.PriceExclTax;
                    //round
                    var unitPriceExclTaxRounded = Math.Round(unitPriceExclTax, 2);              
                    x++;
                    cartTotal += priceExclTax;
                    
                }

                //the checkout attributes that have a dollar value and send them to Paypal as items to be paid for
                var caValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(postProcessPaymentRequest.Order.CheckoutAttributesXml);
                foreach (var val in caValues)
                {
                    var attPrice = _taxService.GetCheckoutAttributePrice(val, false, postProcessPaymentRequest.Order.Customer);
                    //round
                    var attPriceRounded = Math.Round(attPrice, 2);
                    if (attPrice > decimal.Zero) //if it has a price
                    {
                        var ca = val.CheckoutAttribute;
                        if (ca != null)
                        {
                            var attName = ca.Name; //set the name
                   
                            x++;
                            cartTotal += attPrice;
                        }
                    }
                }

                //order totals

                //shipping
                var orderShippingExclTax = postProcessPaymentRequest.Order.OrderShippingExclTax;
                var orderShippingExclTaxRounded = Math.Round(orderShippingExclTax, 2);
                if (orderShippingExclTax > decimal.Zero)
                {            
                    x++;
                    cartTotal += orderShippingExclTax;
                }

                //payment method additional fee
                var paymentMethodAdditionalFeeExclTax = postProcessPaymentRequest.Order.PaymentMethodAdditionalFeeExclTax;
                var paymentMethodAdditionalFeeExclTaxRounded = Math.Round(paymentMethodAdditionalFeeExclTax, 2);
                if (paymentMethodAdditionalFeeExclTax > decimal.Zero)
                {               
                    x++;
                    cartTotal += paymentMethodAdditionalFeeExclTax;
                }

                //tax
                var orderTax = postProcessPaymentRequest.Order.OrderTax;
                var orderTaxRounded = Math.Round(orderTax, 2);
                if (orderTax > decimal.Zero)
                {
               
                    cartTotal += orderTax;
                    x++;
                }

                if (cartTotal > postProcessPaymentRequest.Order.OrderTotal)
                {
                    /* Take the difference between what the order total is and what it should be and use that as the "discount".
                     * The difference equals the amount of the gift card and/or reward points used. 
                     */
                    decimal discountTotal = cartTotal - postProcessPaymentRequest.Order.OrderTotal;
                    discountTotal = Math.Round(discountTotal, 2);
                 }
                total_con_iva = cartTotal;
                valor_iva = postProcessPaymentRequest.Order.OrderTax;
            }
            else
            {
                //pass order total           
                var orderTotal = Math.Round(postProcessPaymentRequest.Order.OrderTotal, 2);
                          
                total_con_iva = postProcessPaymentRequest.Order.OrderTotal;
                valor_iva = postProcessPaymentRequest.Order.OrderTax;
            }

           
            Id_pago = postProcessPaymentRequest.Order.Id.ToString();
           
            string returnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentZonaVirtual/PDTHandler";//?idOrder="+postProcessPaymentRequest.Order.Id;
            string cancelReturnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentZonaVirtual/CancelOrder";
           
            //Instant Payment Notification (server to server message)
            if (_ZonaVirtualPaymentSettings.EnableIpn)
            {
                string ipnUrl;
                if (String.IsNullOrWhiteSpace(_ZonaVirtualPaymentSettings.IpnUrl))
                    ipnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentZonaVirtual/IPNHandler";//?idOrder=" + postProcessPaymentRequest.Order.Id;
                else
                    ipnUrl = _ZonaVirtualPaymentSettings.IpnUrl;
                builder.AppendFormat("&notify_url={0}", ipnUrl);
            }
            descrip_pago = "Compra en la tienda: " + _ZonaVirtualPaymentSettings.NombreTienda;
            Txtemail = postProcessPaymentRequest.Order.BillingAddress.Email;
            Id_cliente = postProcessPaymentRequest.Order.BillingAddress.Id.ToString();
            nombre_cliente = postProcessPaymentRequest.Order.BillingAddress.FirstName;
            apellido_cliente = postProcessPaymentRequest.Order.BillingAddress.LastName;
            telefono_cliente = postProcessPaymentRequest.Order.BillingAddress.PhoneNumber;

          
             // Clear Response Method
             System.Web.HttpContext.Current.Response.Clear();
             // Struct the main page to send Zona Virtual platform
             System.Web.HttpContext.Current.Response.Write("<html><head>");
             System.Web.HttpContext.Current.Response.Write(string.Format("</head><body onload=\"document.{0}.submit()\">", "FormName"));
             System.Web.HttpContext.Current.Response.Write(string.Format("<form name=\"{0}\" method=\"{1}\" action=\"{2}\" >", "FormName", "POST",GetZonaVirtualUrl() + "?estado_pago=enviar_datos"));
             txtcampo1 = " - "; // Not defined by customer
             txtcampo2 = " - ";
             txtcampo3 = " - ";
             System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value={1}>", "Id_pago", Id_pago));
             System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value={1}>", "tipo_id_cliente",tipo_id_cliente ));
             System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value=\"{1}\">", "Id_cliente", Id_cliente ));
             System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value={1}>", "total_con_iva", total_con_iva));
             System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value={1}>", "valor_iva", valor_iva));
             System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value=\"{1}\">", "descrip_pago", descrip_pago ));
             System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value=\"{1}\">", "nombre_cliente", nombre_cliente ));
             System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value=\"{1}\">", "apellido_cliente", apellido_cliente ));
             System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value=\"{1}\">", "Txtemail", Txtemail ));
             System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value=\"{1}\">", "telefono_cliente", telefono_cliente));
             System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value=\"{1}\">", "txtcampo1", txtcampo1 ));
             System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value=\"{1}\">", "txtcampo2", txtcampo2 ));
             System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value=\"{1}\">", "txtcampo3", txtcampo3 ));
            
             System.Web.HttpContext.Current.Response.Write("</form>");
             System.Web.HttpContext.Current.Response.Write("</body></html>");
             // Close the struct and will be sended automatically 
             System.Web.HttpContext.Current.Response.End();
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
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.RedirectionTip", "You will be redirected to PayPal site to complete the order.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.UseSandbox", "Use Sandbox");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.UseSandbox.Hint", "Check to enable Sandbox (testing environment).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.BusinessEmail", "Business Email");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.BusinessEmail.Hint", "Specify your PayPal business email.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.PDTToken", "PDT Identity Token");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.PDTToken.Hint", "Specify PDT identity token");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.PDTValidateOrderTotal", "PDT. Validate order total");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.PDTValidateOrderTotal.Hint", "Check if PDT handler should validate order totals.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.PassProductNamesAndTotals", "Pass product names and order totals to PayPal");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.PassProductNamesAndTotals.Hint", "Check if product names and order totals should be passed to PayPal.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.EnableIpn", "Enable IPN (Instant Payment Notification)");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.EnableIpn.Hint", "Check if IPN is enabled.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.EnableIpn.Hint2", "Leave blank to use the default IPN handler URL.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.IpnUrl", "IPN Handler");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.IpnUrl.Hint", "Specify IPN Handler.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage", "Return to order details page");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.ZonaVirtual.Fields.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage.Hint", "Enable if a customer should be redirected to the order details page when he clicks \"return to store\" link on PayPal site WITHOUT completing a payment");

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
