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
    public class PaymentZonaVirtualController : BasePaymentController
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
            model.CodigoServicio = ZonaVirtualPaymentSettings.CodigoServicio;
  

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
            ZonaVirtualPaymentSettings.CodigoServicio = model.CodigoServicio;
    

            /* Do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.RutaTienda, storeScope, false);
            _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.NombreTienda, storeScope, false);
            _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.ID_Clave, storeScope, false);
            _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.ID_Tienda, storeScope, false);
            _settingService.SaveSetting(ZonaVirtualPaymentSettings, x => x.CodigoServicio, storeScope, false);
         
            //now clear settings cache
            _settingService.ClearCache();

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            ProcessPaymentRequest test = new ProcessPaymentRequest();

            ZPagos.ZPagos Pagos = new ZPagos.ZPagos();
            ZPagosDemo.ZPagos PagosDemo = new ZPagosDemo.ZPagos();


            string[] lista_codigos = new string[1] { "" };
            string[] lista_nit_codigos = new string[1] { "" };

            double[] lista_codigos_servicio_multicredito = new double[1] { 0 };
            double[] lista_valores_con_iva = new double[1] { 0 };
            double[] lista_valores_iva = new double[1] { 0 };
            string Respuesta = "";
            string Tienda = _storeContext.CurrentStore.Name;
            var form = this.Request.Form;
            //var order = _orderService.GetOrderByGuid(test.OrderGuid);
            Respuesta = _workContext.CurrentCustomer.BillingAddress.FirstName.ToString();
            var order_id_temp = this.GenerateUniquePayementFromZP(new Random().Next(1, int.MaxValue)); //new Random().Next(1, int.MaxValue);

            var askOrder = this.aksOrder();

            Session.Add("order_id_temp", order_id_temp);
            double Total_con_iva = 0;
            foreach (var item in _workContext.CurrentCustomer.ShoppingCartItems)
            {
                Total_con_iva += (double)item.Product.Price * item.Quantity;
                Respuesta += " " + item.Product.Price + " " + _workContext.CurrentCustomer.BillingAddress.FirstName.ToString() + _workContext.CurrentCustomer.BillingAddress.LastName;
            }; // order.BillingAddress.Email;

            if (_ZonaVirtualPaymentSettings.RutaTienda.IndexOf("demo") > 0)
            {
                Respuesta = PagosDemo.inicio_pagoV2(_ZonaVirtualPaymentSettings.ID_Tienda,
                _ZonaVirtualPaymentSettings.ID_Clave,
                Total_con_iva,
                0,
                order_id_temp.ToString(),
                "Compra en tienda: " + _ZonaVirtualPaymentSettings.NombreTienda,
                _workContext.CurrentCustomer.BillingAddress.Email.ToString(),
                _workContext.CurrentCustomer.Id.ToString(),
                "0",
                _workContext.CurrentCustomer.BillingAddress.FirstName.ToString(),
                _workContext.CurrentCustomer.BillingAddress.LastName.ToString(),
                _workContext.CurrentCustomer.BillingAddress.PhoneNumber.ToString(),
                "Orden ID: " + askOrder.ToString(),
                "_",
                "_",
                _ZonaVirtualPaymentSettings.CodigoServicio.ToString(),
                null, null, null, null, 0);


                var VerificarDemo = new ZPagosVerificarDemo.Service();


                ZPagosVerificarDemo.pagos_v3[] respuesta = new ZPagosVerificarDemo.pagos_v3[1];
                int error = 0;
                string errorStr = "";


                // Verificar compra 
                //var res = VerificarDemo.verificar_pago_v3("11", _ZonaVirtualPaymentSettings.ID_Tienda, _ZonaVirtualPaymentSettings.ID_Clave, ref respuesta, ref error, ref errorStr);

                // Variable de prueba de respuesta
                //Session.Add("Verificacion", res +" Error:" +  error.ToString() +" "+ errorStr + "FORMA DE PAGO: " + respuesta[0].int_id_forma_pago + "Estado: " + respuesta[0].int_estado_pago);
            }
            else
            {
                Respuesta = Pagos.inicio_pagoV2(_ZonaVirtualPaymentSettings.ID_Tienda,
                     _ZonaVirtualPaymentSettings.ID_Clave,
                Total_con_iva,
                0,
                 order_id_temp.ToString(),
                "Compra en tienda: " + _ZonaVirtualPaymentSettings.NombreTienda,
                _workContext.CurrentCustomer.BillingAddress.Email.ToString(),
                _workContext.CurrentCustomer.Id.ToString(),
                "0",
                _workContext.CurrentCustomer.BillingAddress.FirstName.ToString(),
                _workContext.CurrentCustomer.BillingAddress.LastName.ToString(),
                _workContext.CurrentCustomer.BillingAddress.PhoneNumber.ToString(),
                 "Orden ID: " + askOrder.ToString(),
                "_",
                "_",
                _ZonaVirtualPaymentSettings.CodigoServicio.ToString(),
                null, null, null, null, 0);

            }

            string URL = _ZonaVirtualPaymentSettings.RutaTienda + "?estado_pago=iniciar_pago&identificador=" + Respuesta;
            Session.Add("URL", URL);
          
            // var descrip_pago = "Compra en la tienda: " + _ZonaVirtualPaymentSettings.NombreTienda;

            Session.Add("code_buy", Respuesta);

            return View("Nop.Plugin.Payments.ZonaVirtual.Views.PaymentZonaVirtual.PaymentInfo");
        }

        /// <summary>
        /// Consulta si existe una orden con ese id si es asi busca de manera recursiva hasta encontrar un valor disponible
        /// </summary>
        /// <param name="idOrder"></param>
        /// <returns></returns>
        private int aksOrder()
        {
            try
            {


                var query = (from a in _orderService.GetAllOrderItems(null, null, null, null, null, null, null, false) orderby a.Id descending select a).First();
                return query.OrderId + 1;
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        /// Consulta si existe una orden con ese id si es asi busca de manera recursiva hasta encontrar un valor disponible
        /// </summary>
        /// <param name="idOrder"></param>
        /// <returns></returns>
        private int GenerateUniquePayementFromZP(int RandomIndex)
        {


            if (_ZonaVirtualPaymentSettings.RutaTienda.IndexOf("demo") > 0)
            {

                var VerificarDemo = new ZPagosVerificarDemo.Service();
                ZPagosVerificarDemo.pagos_v3[] respuesta = new ZPagosVerificarDemo.pagos_v3[1];
                int error = 0;
                string errorStr = "";
                var res = VerificarDemo.verificar_pago_v3(RandomIndex.ToString(), _ZonaVirtualPaymentSettings.ID_Tienda, _ZonaVirtualPaymentSettings.ID_Clave, ref respuesta, ref error, ref errorStr);
                if (res != 0)
                {

                    return this.GenerateUniquePayementFromZP(new Random().Next(1, int.MaxValue));

                }
            }
            else
            {
                var Verificar = new ZPagosVerificar.Service();
                ZPagosVerificar.pagos_v3[] respuesta = new ZPagosVerificar.pagos_v3[1];
                int error = 0;
                string errorStr = "";
                var res = Verificar.verificar_pago_v3(RandomIndex.ToString(), _ZonaVirtualPaymentSettings.ID_Tienda, _ZonaVirtualPaymentSettings.ID_Clave, ref respuesta, ref error, ref errorStr);
                if (res != 0)
                {

                    return this.GenerateUniquePayementFromZP(new Random().Next(1, int.MaxValue));

                }
            }
            return RandomIndex;
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

                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
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