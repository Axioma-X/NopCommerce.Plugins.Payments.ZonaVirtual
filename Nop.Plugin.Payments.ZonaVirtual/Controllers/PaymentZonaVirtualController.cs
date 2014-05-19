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
    public class PaymentZonaVirtualController : BaseNopPaymentController
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
            //model.UseSandbox = ZonaVirtualPaymentSettings.UseSandbox;
            model.RutaTienda = ZonaVirtualPaymentSettings.RutaTienda;
            model.ID_Clave = ZonaVirtualPaymentSettings.ID_Clave;
            model.ID_Tienda = ZonaVirtualPaymentSettings.ID_Tienda;
            model.NombreTienda = ZonaVirtualPaymentSettings.NombreTienda;
            //model.BusinessEmail = ZonaVirtualPaymentSettings.BusinessEmail;
            model.PdtToken = ZonaVirtualPaymentSettings.PdtToken;
            model.PdtValidateOrderTotal = ZonaVirtualPaymentSettings.PdtValidateOrderTotal;
            model.AdditionalFee = ZonaVirtualPaymentSettings.AdditionalFee;
            model.AdditionalFeePercentage = ZonaVirtualPaymentSettings.AdditionalFeePercentage;
            model.PassProductNamesAndTotals = ZonaVirtualPaymentSettings.PassProductNamesAndTotals;
            model.EnableIpn = ZonaVirtualPaymentSettings.EnableIpn;
            model.IpnUrl = ZonaVirtualPaymentSettings.IpnUrl;
            model.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage = ZonaVirtualPaymentSettings.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage;

            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                //model.UseSandbox_OverrideForStore = _settingService.SettingExists(ZonaVirtualPaymentSettings, x => x.UseSandbox, storeScope);
                //model.BusinessEmail_OverrideForStore = _settingService.SettingExists(ZonaVirtualPaymentSettings, x => x.BusinessEmail, storeScope);
                model.PdtToken_OverrideForStore = _settingService.SettingExists(ZonaVirtualPaymentSettings, x => x.PdtToken, storeScope);
                model.PdtValidateOrderTotal_OverrideForStore = _settingService.SettingExists(ZonaVirtualPaymentSettings, x => x.PdtValidateOrderTotal, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(ZonaVirtualPaymentSettings, x => x.AdditionalFee, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = _settingService.SettingExists(ZonaVirtualPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.PassProductNamesAndTotals_OverrideForStore = _settingService.SettingExists(ZonaVirtualPaymentSettings, x => x.PassProductNamesAndTotals, storeScope);
                model.EnableIpn_OverrideForStore = _settingService.SettingExists(ZonaVirtualPaymentSettings, x => x.EnableIpn, storeScope);
                model.IpnUrl_OverrideForStore = _settingService.SettingExists(ZonaVirtualPaymentSettings, x => x.IpnUrl, storeScope);
                model.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage_OverrideForStore = _settingService.SettingExists(ZonaVirtualPaymentSettings, x => x.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage, storeScope);
            }

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
            //ZonaVirtualPaymentSettings.UseSandbox = model.UseSandbox;
            ZonaVirtualPaymentSettings.RutaTienda = model.RutaTienda;
            ZonaVirtualPaymentSettings.ID_Clave = model.ID_Clave;
            ZonaVirtualPaymentSettings.ID_Tienda = model.ID_Tienda;
            ZonaVirtualPaymentSettings.NombreTienda = model.NombreTienda;

            //ZonaVirtualPaymentSettings.BusinessEmail = model.BusinessEmail;
            ZonaVirtualPaymentSettings.PdtToken = model.PdtToken;
            ZonaVirtualPaymentSettings.PdtValidateOrderTotal = model.PdtValidateOrderTotal;
            ZonaVirtualPaymentSettings.AdditionalFee = model.AdditionalFee;
            ZonaVirtualPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            ZonaVirtualPaymentSettings.PassProductNamesAndTotals = model.PassProductNamesAndTotals;
            ZonaVirtualPaymentSettings.EnableIpn = model.EnableIpn;
            ZonaVirtualPaymentSettings.IpnUrl = model.IpnUrl;
            ZonaVirtualPaymentSettings.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage = model.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            //if (model.UseSandbox_OverrideForStore || storeScope == 0)
            //    _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.UseSandbox, storeScope, false);
            //else if (storeScope > 0)
            //    _settingService.DeleteSetting(ZonaVirtualPaymentSettings, x => x.UseSandbox, storeScope);
            _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.RutaTienda, storeScope, false);
            _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.NombreTienda, storeScope, false);
            _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.ID_Clave, storeScope, false);
            _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.ID_Tienda, storeScope, false);

            //if (model.BusinessEmail_OverrideForStore || storeScope == 0)
            //    _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.BusinessEmail, storeScope, false);
            //else if (storeScope > 0)
            //    _settingService.DeleteSetting(ZonaVirtualPaymentSettings, x => x.BusinessEmail, storeScope);

            if (model.PdtToken_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.PdtToken, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ZonaVirtualPaymentSettings, x => x.PdtToken, storeScope);

            if (model.PdtValidateOrderTotal_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.PdtValidateOrderTotal, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ZonaVirtualPaymentSettings, x => x.PdtValidateOrderTotal, storeScope);

            if (model.AdditionalFee_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.AdditionalFee, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ZonaVirtualPaymentSettings, x => x.AdditionalFee, storeScope);

            if (model.AdditionalFeePercentage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.AdditionalFeePercentage, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ZonaVirtualPaymentSettings, x => x.AdditionalFeePercentage, storeScope);

            if (model.PassProductNamesAndTotals_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.PassProductNamesAndTotals, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ZonaVirtualPaymentSettings, x => x.PassProductNamesAndTotals, storeScope);

            if (model.EnableIpn_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.EnableIpn, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ZonaVirtualPaymentSettings, x => x.EnableIpn, storeScope);

            if (model.IpnUrl_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.IpnUrl, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ZonaVirtualPaymentSettings, x => x.IpnUrl, storeScope);

            if (model.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(ZonaVirtualPaymentSettings, x => x.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage, storeScope);

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

        [ValidateInput(false)]
        public ActionResult PDTHandler(FormCollection form)
        {
            string tx = _webHelper.QueryString<string>("tx");
            Dictionary<string, string> values;
            string response;

            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.ZonaVirtual") as ZonaVirtualPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("PayPal Standard module cannot be loaded");

            if (processor.GetPDTDetails(tx, out values, out response))
            {
                string orderNumber = string.Empty;
                values.TryGetValue("custom", out orderNumber);
                Guid orderNumberGuid = Guid.Empty;
                try
                {
                    orderNumberGuid = new Guid(orderNumber);
                }
                catch { }
                Order order = _orderService.GetOrderByGuid(orderNumberGuid);
                if (order != null)
                {
                    decimal total = decimal.Zero;
                    try
                    {
                        total = decimal.Parse(values["mc_gross"], new CultureInfo("en-US"));
                    }
                    catch (Exception exc)
                    {
                        _logger.Error("PayPal PDT. Error getting mc_gross", exc);
                    }

                    string payer_status = string.Empty;
                    values.TryGetValue("payer_status", out payer_status);
                    string payment_status = string.Empty;
                    values.TryGetValue("payment_status", out payment_status);
                    string pending_reason = string.Empty;
                    values.TryGetValue("pending_reason", out pending_reason);
                    string mc_currency = string.Empty;
                    values.TryGetValue("mc_currency", out mc_currency);
                    string txn_id = string.Empty;
                    values.TryGetValue("txn_id", out txn_id);
                    string payment_type = string.Empty;
                    values.TryGetValue("payment_type", out payment_type);
                    string payer_id = string.Empty;
                    values.TryGetValue("payer_id", out payer_id);
                    string receiver_id = string.Empty;
                    values.TryGetValue("receiver_id", out receiver_id);
                    string invoice = string.Empty;
                    values.TryGetValue("invoice", out invoice);
                    string payment_fee = string.Empty;
                    values.TryGetValue("payment_fee", out payment_fee);

                    var sb = new StringBuilder();
                    sb.AppendLine("Paypal PDT:");
                    sb.AppendLine("total: " + total);
                    sb.AppendLine("Payer status: " + payer_status);
                    sb.AppendLine("Payment status: " + payment_status);
                    sb.AppendLine("Pending reason: " + pending_reason);
                    sb.AppendLine("mc_currency: " + mc_currency);
                    sb.AppendLine("txn_id: " + txn_id);
                    sb.AppendLine("payment_type: " + payment_type);
                    sb.AppendLine("payer_id: " + payer_id);
                    sb.AppendLine("receiver_id: " + receiver_id);
                    sb.AppendLine("invoice: " + invoice);
                    sb.AppendLine("payment_fee: " + payment_fee);


                    //order note
                    order.OrderNotes.Add(new OrderNote()
                    {
                        Note = sb.ToString(),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);

                    //load settings for a chosen store scope
                    var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
                    var ZonaVirtualPaymentSettings = _settingService.LoadSetting<ZonaVirtualPaymentSettings>(storeScope);

                    //validate order total
                    if (ZonaVirtualPaymentSettings.PdtValidateOrderTotal && !Math.Round(total, 2).Equals(Math.Round(order.OrderTotal, 2)))
                    {
                        string errorStr = string.Format("PayPal PDT. Returned order total {0} doesn't equal order total {1}", total, order.OrderTotal);
                        _logger.Error(errorStr);

                        return RedirectToAction("Index", "Home", new { area = "" });
                    }

                    //mark order as paid
                    if (_orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        order.AuthorizationTransactionId = txn_id;
                        _orderService.UpdateOrder(order);

                        _orderProcessingService.MarkOrderAsPaid(order);
                    }
                }

                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id});
            }
            else
            {
                string orderNumber = string.Empty;
                values.TryGetValue("custom", out orderNumber);
                Guid orderNumberGuid = Guid.Empty;
                try
                {
                    orderNumberGuid = new Guid(orderNumber);
                }
                catch { }
                Order order = _orderService.GetOrderByGuid(orderNumberGuid);
                if (order != null)
                {
                    //order note
                    order.OrderNotes.Add(new OrderNote()
                    {
                        Note = "PayPal PDT failed. " + response,
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        [ValidateInput(false)]
        public ActionResult IPNHandler()
        {
            byte[] param = Request.BinaryRead(Request.ContentLength);
            string strRequest = Encoding.ASCII.GetString(param);
            Dictionary<string, string> values;
            string idOrder = Request.QueryString["idOrder"];
            Respuesta res = _zonaVirtualHelper.ds_verificar_pago_v2(idOrder, _ZonaVirtualPaymentSettings.ID_Tienda, _ZonaVirtualPaymentSettings.ID_Clave);
            strRequest += res.descripcion;
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.ZonaVirtual") as ZonaVirtualPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("PayPal Standard module cannot be loaded");

            if (processor.VerifyIPN(strRequest, out values))
            {
                #region values
                decimal total = decimal.Zero;
                try
                {
                    total = decimal.Parse(values["mc_gross"], new CultureInfo("en-US"));
                }
                catch { }

                string payer_status = string.Empty;
                values.TryGetValue("payer_status", out payer_status);
                string payment_status = string.Empty;
                values.TryGetValue("payment_status", out payment_status);
                string pending_reason = string.Empty;
                values.TryGetValue("pending_reason", out pending_reason);
                string mc_currency = string.Empty;
                values.TryGetValue("mc_currency", out mc_currency);
                string txn_id = string.Empty;
                values.TryGetValue("txn_id", out txn_id);
                string txn_type = string.Empty;
                values.TryGetValue("txn_type", out txn_type);
                string rp_invoice_id = string.Empty;
                values.TryGetValue("rp_invoice_id", out rp_invoice_id);
                string payment_type = string.Empty;
                values.TryGetValue("payment_type", out payment_type);
                string payer_id = string.Empty;
                values.TryGetValue("payer_id", out payer_id);
                string receiver_id = string.Empty;
                values.TryGetValue("receiver_id", out receiver_id);
                string invoice = string.Empty;
                values.TryGetValue("invoice", out invoice);
                string payment_fee = string.Empty;
                values.TryGetValue("payment_fee", out payment_fee);

                #endregion

                var sb = new StringBuilder();
                sb.AppendLine("Paypal IPN:");
                foreach (KeyValuePair<string, string> kvp in values)
                {
                    sb.AppendLine(kvp.Key + ": " + kvp.Value);
                }

                var newPaymentStatus = PaypalHelper.GetPaymentStatus(payment_status, pending_reason);
                sb.AppendLine("New payment status: " + newPaymentStatus);

                switch (txn_type)
                {
                    case "recurring_payment_profile_created":
                        //do nothing here
                        break;
                    case "recurring_payment":
                        #region Recurring payment
                        {
                            Guid orderNumberGuid = Guid.Empty;
                            try
                            {
                                orderNumberGuid = new Guid(rp_invoice_id);
                            }
                            catch
                            {
                            }

                            var initialOrder = _orderService.GetOrderByGuid(orderNumberGuid);
                            if (initialOrder != null)
                            {
                                var recurringPayments = _orderService.SearchRecurringPayments(0, 0, initialOrder.Id, null, 0, int.MaxValue);
                                foreach (var rp in recurringPayments)
                                {
                                    switch (newPaymentStatus)
                                    {
                                        case PaymentStatus.Authorized:
                                        case PaymentStatus.Paid:
                                            {
                                                var recurringPaymentHistory = rp.RecurringPaymentHistory;
                                                if (recurringPaymentHistory.Count == 0)
                                                {
                                                    //first payment
                                                    var rph = new RecurringPaymentHistory()
                                                    {
                                                        RecurringPaymentId = rp.Id,
                                                        OrderId = initialOrder.Id,
                                                        CreatedOnUtc = DateTime.UtcNow
                                                    };
                                                    rp.RecurringPaymentHistory.Add(rph);
                                                    _orderService.UpdateRecurringPayment(rp);
                                                }
                                                else
                                                {
                                                    //next payments
                                                    _orderProcessingService.ProcessNextRecurringPayment(rp);
                                                }
                                            }
                                            break;
                                    }
                                }

                                //this.OrderService.InsertOrderNote(newOrder.OrderId, sb.ToString(), DateTime.UtcNow);
                                _logger.Information("PayPal IPN. Recurring info", new NopException(sb.ToString()));
                            }
                            else
                            {
                                _logger.Error("PayPal IPN. Order is not found", new NopException(sb.ToString()));
                            }
                        }
                        #endregion
                        break;
                    default:
                        #region Standard payment
                        {
                            string orderNumber = string.Empty;
                            values.TryGetValue("custom", out orderNumber);
                            Guid orderNumberGuid = Guid.Empty;
                            try
                            {
                                orderNumberGuid = new Guid(orderNumber);
                            }
                            catch
                            {
                            }

                            var order = _orderService.GetOrderByGuid(orderNumberGuid);
                            if (order != null)
                            {

                                //order note
                                order.OrderNotes.Add(new OrderNote()
                                {
                                    Note = sb.ToString(),
                                    DisplayToCustomer = false,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                                _orderService.UpdateOrder(order);

                                switch (newPaymentStatus)
                                {
                                    case PaymentStatus.Pending:
                                        {
                                        }
                                        break;
                                    case PaymentStatus.Authorized:
                                        {
                                            if (_orderProcessingService.CanMarkOrderAsAuthorized(order))
                                            {
                                                _orderProcessingService.MarkAsAuthorized(order);
                                            }
                                        }
                                        break;
                                    case PaymentStatus.Paid:
                                        {
                                            if (_orderProcessingService.CanMarkOrderAsPaid(order))
                                            {

                                                order.AuthorizationTransactionId = txn_id;
                                                _orderService.UpdateOrder(order);

                                                _orderProcessingService.MarkOrderAsPaid(order);
                                            }
                                        }
                                        break;
                                    case PaymentStatus.Refunded:
                                        {
                                            if (_orderProcessingService.CanRefundOffline(order))
                                            {
                                                _orderProcessingService.RefundOffline(order);
                                            }
                                        }
                                        break;
                                    case PaymentStatus.Voided:
                                        {
                                            if (_orderProcessingService.CanVoidOffline(order))
                                            {
                                                _orderProcessingService.VoidOffline(order);
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                _logger.Error("PayPal IPN. Order is not found", new NopException(sb.ToString()));
                            }
                        }
                        #endregion
                        break;
                }
            }
            else
            {
                _logger.Error("PayPal IPN failed.", new NopException(strRequest));
            }

            //nothing should be rendered to visitor
            return Content("");
        }

        public ActionResult CancelOrder(FormCollection form)
        {
            if (_ZonaVirtualPaymentSettings.ReturnFromPayPalWithoutPaymentRedirectsToOrderDetailsPage)
            {
                var order = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                    customerId: _workContext.CurrentCustomer.Id, pageSize: 1)
                    .FirstOrDefault();
                if (order != null)
                {
                    return RedirectToRoute("OrderDetails", new { orderId = order.Id });
                }
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}