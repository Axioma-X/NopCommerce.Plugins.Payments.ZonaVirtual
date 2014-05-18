using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services;
using System.Diagnostics;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace Nop.Plugin.Payments.ZonaVirtual
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name = "ServiceSoap", Namespace = "http://www.zonapagos.com/ws_verificar_pagos")]

    public class ZonaVirtualHelper : System.Web.Services.Protocols.SoapHttpClientProtocol
    {
        private System.Threading.SendOrPostCallback ds_verificar_pagoOperationCompleted;

        private System.Threading.SendOrPostCallback ds_verificar_pago_v2OperationCompleted;

        private System.Threading.SendOrPostCallback verificar_pago_v3OperationCompleted;

        private bool useDefaultCredentialsSetExplicitly;

        private readonly ZonaVirtualPaymentSettings _ZonaVirtualPaymentSettings;
        public ZonaVirtualHelper() {
            this.Url = _ZonaVirtualPaymentSettings.RutaTienda;
            if ((this.IsLocalFileSystemWebService(this.Url) == true))
            {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else
            {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }

        public new string Url
        {
            get
            {
                return base.Url;
            }
            set
            {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true)
                            && (this.useDefaultCredentialsSetExplicitly == false))
                            && (this.IsLocalFileSystemWebService(value) == false)))
                {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }

        public new bool UseDefaultCredentials
        {
            get
            {
                return base.UseDefaultCredentials;
            }
            set
            {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }

        public event ds_verificar_pagoCompletedEventHandler ds_verificar_pagoCompleted;
        public event ds_verificar_pago_v2CompletedEventHandler ds_verificar_pago_v2Completed;
        public event verificar_pago_v3CompletedEventHandler verificar_pago_v3Completed;

        public void ds_verificar_pagoAsync(string id_pago, int id_tienda, string id_clave)
        {
            this.ds_verificar_pagoAsync(id_pago, id_tienda, id_clave, null);
        }

        public void ds_verificar_pagoAsync(string id_pago, int id_tienda, string id_clave, object userState)
        {
            if ((this.ds_verificar_pagoOperationCompleted == null))
            {
                this.ds_verificar_pagoOperationCompleted = new System.Threading.SendOrPostCallback(this.Onds_verificar_pagoOperationCompleted);
            }
            this.InvokeAsync("ds_verificar_pago", new object[] {
                        id_pago,
                        id_tienda,
                        id_clave}, this.ds_verificar_pagoOperationCompleted, userState);
        }

        private void Onds_verificar_pagoOperationCompleted(object arg)
        {
            if ((this.ds_verificar_pagoCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ds_verificar_pagoCompleted(this, new ds_verificar_pagoCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.zonapagos.com/ws_verificar_pagos/ds_verificar_pago_v2", RequestNamespace = "http://www.zonapagos.com/ws_verificar_pagos", ResponseNamespace = "http://www.zonapagos.com/ws_verificar_pagos", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public Respuesta ds_verificar_pago_v2(string id_pago, int id_tienda, string id_clave)
        {
            object[] results = this.Invoke("ds_verificar_pago_v2", new object[] {
                        id_pago,
                        id_tienda,
                        id_clave});
            return ((Respuesta)(results[0]));
        }

        public void ds_verificar_pago_v2Async(string id_pago, int id_tienda, string id_clave)
        {
            this.ds_verificar_pago_v2Async(id_pago, id_tienda, id_clave, null);
        }

        public void ds_verificar_pago_v2Async(string id_pago, int id_tienda, string id_clave, object userState)
        {
            if ((this.ds_verificar_pago_v2OperationCompleted == null))
            {
                this.ds_verificar_pago_v2OperationCompleted = new System.Threading.SendOrPostCallback(this.Onds_verificar_pago_v2OperationCompleted);
            }
            this.InvokeAsync("ds_verificar_pago_v2", new object[] {
                        id_pago,
                        id_tienda,
                        id_clave}, this.ds_verificar_pago_v2OperationCompleted, userState);
        }

        private void Onds_verificar_pago_v2OperationCompleted(object arg)
        {
            if ((this.ds_verificar_pago_v2Completed != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ds_verificar_pago_v2Completed(this, new ds_verificar_pago_v2CompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.zonapagos.com/ws_verificar_pagos/verificar_pago_v3", RequestNamespace = "http://www.zonapagos.com/ws_verificar_pagos", ResponseNamespace = "http://www.zonapagos.com/ws_verificar_pagos", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int verificar_pago_v3(string str_id_pago, int int_id_tienda, string str_id_clave, ref pagos_v3[] res_pagos_v3, ref int int_error, ref string str_error)
        {
            object[] results = this.Invoke("verificar_pago_v3", new object[] {
                        str_id_pago,
                        int_id_tienda,
                        str_id_clave,
                        res_pagos_v3,
                        int_error,
                        str_error});
            res_pagos_v3 = ((pagos_v3[])(results[1]));
            int_error = ((int)(results[2]));
            str_error = ((string)(results[3]));
            return ((int)(results[0]));
        }

        public void verificar_pago_v3Async(string str_id_pago, int int_id_tienda, string str_id_clave, pagos_v3[] res_pagos_v3, int int_error, string str_error)
        {
            this.verificar_pago_v3Async(str_id_pago, int_id_tienda, str_id_clave, res_pagos_v3, int_error, str_error, null);
        }

        public void verificar_pago_v3Async(string str_id_pago, int int_id_tienda, string str_id_clave, pagos_v3[] res_pagos_v3, int int_error, string str_error, object userState)
        {
            if ((this.verificar_pago_v3OperationCompleted == null))
            {
                this.verificar_pago_v3OperationCompleted = new System.Threading.SendOrPostCallback(this.Onverificar_pago_v3OperationCompleted);
            }
            this.InvokeAsync("verificar_pago_v3", new object[] {
                        str_id_pago,
                        int_id_tienda,
                        str_id_clave,
                        res_pagos_v3,
                        int_error,
                        str_error}, this.verificar_pago_v3OperationCompleted, userState);
        }

        private void Onverificar_pago_v3OperationCompleted(object arg)
        {
            if ((this.verificar_pago_v3Completed != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.verificar_pago_v3Completed(this, new verificar_pago_v3CompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        public new void CancelAsync(object userState)
        {
            base.CancelAsync(userState);
        }

        private bool IsLocalFileSystemWebService(string url)
        {
            if (((url == null)
                        || (url == string.Empty)))
            {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024)
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0)))
            {
                return true;
            }
            return false;
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.18034")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.zonapagos.com/ws_verificar_pagos")]
    public partial class Respuesta
    {

        private int codigoField;

        private string descripcionField;

        private System.Data.DataSet ds_verificar_pagoField;

        /// <remarks/>
        public int codigo
        {
            get
            {
                return this.codigoField;
            }
            set
            {
                this.codigoField = value;
            }
        }

        /// <remarks/>
        public string descripcion
        {
            get
            {
                return this.descripcionField;
            }
            set
            {
                this.descripcionField = value;
            }
        }

        /// <remarks/>
        public System.Data.DataSet ds_verificar_pago
        {
            get
            {
                return this.ds_verificar_pagoField;
            }
            set
            {
                this.ds_verificar_pagoField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.18034")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.zonapagos.com/ws_verificar_pagos")]
    public partial class pagos_v3
    {

        private string str_id_pagoField;

        private int int_estado_pagoField;

        private int int_id_forma_pagoField;

        private double dbl_valor_pagadoField;

        private string str_ticketIDField;

        private string str_id_claveField;

        private string str_id_clienteField;

        private string str_franquiciaField;

        private int int_cod_aprobacionField;

        private int int_codigo_servicoField;

        private int int_codigo_bancoField;

        private string str_nombre_bancoField;

        private string str_codigo_transaccionField;

        private int int_ciclo_transaccionField;

        private string str_campo1Field;

        private string str_campo2Field;

        private string str_campo3Field;

        private System.DateTime dat_fechaField;

        /// <remarks/>
        public string str_id_pago
        {
            get
            {
                return this.str_id_pagoField;
            }
            set
            {
                this.str_id_pagoField = value;
            }
        }

        /// <remarks/>
        public int int_estado_pago
        {
            get
            {
                return this.int_estado_pagoField;
            }
            set
            {
                this.int_estado_pagoField = value;
            }
        }

        /// <remarks/>
        public int int_id_forma_pago
        {
            get
            {
                return this.int_id_forma_pagoField;
            }
            set
            {
                this.int_id_forma_pagoField = value;
            }
        }

        /// <remarks/>
        public double dbl_valor_pagado
        {
            get
            {
                return this.dbl_valor_pagadoField;
            }
            set
            {
                this.dbl_valor_pagadoField = value;
            }
        }

        /// <remarks/>
        public string str_ticketID
        {
            get
            {
                return this.str_ticketIDField;
            }
            set
            {
                this.str_ticketIDField = value;
            }
        }

        /// <remarks/>
        public string str_id_clave
        {
            get
            {
                return this.str_id_claveField;
            }
            set
            {
                this.str_id_claveField = value;
            }
        }

        /// <remarks/>
        public string str_id_cliente
        {
            get
            {
                return this.str_id_clienteField;
            }
            set
            {
                this.str_id_clienteField = value;
            }
        }

        /// <remarks/>
        public string str_franquicia
        {
            get
            {
                return this.str_franquiciaField;
            }
            set
            {
                this.str_franquiciaField = value;
            }
        }

        /// <remarks/>
        public int int_cod_aprobacion
        {
            get
            {
                return this.int_cod_aprobacionField;
            }
            set
            {
                this.int_cod_aprobacionField = value;
            }
        }

        /// <remarks/>
        public int int_codigo_servico
        {
            get
            {
                return this.int_codigo_servicoField;
            }
            set
            {
                this.int_codigo_servicoField = value;
            }
        }

        /// <remarks/>
        public int int_codigo_banco
        {
            get
            {
                return this.int_codigo_bancoField;
            }
            set
            {
                this.int_codigo_bancoField = value;
            }
        }

        /// <remarks/>
        public string str_nombre_banco
        {
            get
            {
                return this.str_nombre_bancoField;
            }
            set
            {
                this.str_nombre_bancoField = value;
            }
        }

        /// <remarks/>
        public string str_codigo_transaccion
        {
            get
            {
                return this.str_codigo_transaccionField;
            }
            set
            {
                this.str_codigo_transaccionField = value;
            }
        }

        /// <remarks/>
        public int int_ciclo_transaccion
        {
            get
            {
                return this.int_ciclo_transaccionField;
            }
            set
            {
                this.int_ciclo_transaccionField = value;
            }
        }

        /// <remarks/>
        public string str_campo1
        {
            get
            {
                return this.str_campo1Field;
            }
            set
            {
                this.str_campo1Field = value;
            }
        }

        /// <remarks/>
        public string str_campo2
        {
            get
            {
                return this.str_campo2Field;
            }
            set
            {
                this.str_campo2Field = value;
            }
        }

        /// <remarks/>
        public string str_campo3
        {
            get
            {
                return this.str_campo3Field;
            }
            set
            {
                this.str_campo3Field = value;
            }
        }

        /// <remarks/>
        public System.DateTime dat_fecha
        {
            get
            {
                return this.dat_fechaField;
            }
            set
            {
                this.dat_fechaField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    public delegate void ds_verificar_pagoCompletedEventHandler(object sender, ds_verificar_pagoCompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ds_verificar_pagoCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal ds_verificar_pagoCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public Respuesta Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((Respuesta)(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    public delegate void ds_verificar_pago_v2CompletedEventHandler(object sender, ds_verificar_pago_v2CompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ds_verificar_pago_v2CompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal ds_verificar_pago_v2CompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public Respuesta Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((Respuesta)(this.results[0]));
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    public delegate void verificar_pago_v3CompletedEventHandler(object sender, verificar_pago_v3CompletedEventArgs e);

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class verificar_pago_v3CompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
    {

        private object[] results;

        internal verificar_pago_v3CompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState)
        {
            this.results = results;
        }

        /// <remarks/>
        public int Result
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((int)(this.results[0]));
            }
        }

        /// <remarks/>
        public pagos_v3[] res_pagos_v3
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((pagos_v3[])(this.results[1]));
            }
        }

        /// <remarks/>
        public int int_error
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((int)(this.results[2]));
            }
        }

        /// <remarks/>
        public string str_error
        {
            get
            {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[3]));
            }
        }
    }
}
