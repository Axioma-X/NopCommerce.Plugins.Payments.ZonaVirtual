# NopCommerce Plugins Payments ZonaVirtual #

Plugin para **nopComerce 3.30** disponible para escoger como metodo de pago de la compra.

## VERSIONES ##
1.0 Versión inicial

1.1 Versión con manejo de iframe para el Pago

1.2 Versión con manejo Descuentos y ajustes al precion por el metodo de envío


## IMPORTANTE ##
Desarrollado con .Net Framework 4.5.1, sobre Visual Studio 2013, y Basado en la Arquitectura de plugin de Nop.Commerce 3.30
Proveedor del servicio de pago: [ZonaVirtual S.A](http://www.zonavirtual.com/ "Zona Virtual")


## CONTENIDO ##
1. "*Nop.Plugin.Payments.ZonaVirtual*" Folder que contiene el código fuente del proyecto.
2. "*Payments.ZonaVirtual*" Folder que contiene el compilado del Plugin listo para usar.


## FUNCIONAMIENTO ##
Consiste en usar como metodo de pago los servicios provistos por Zona Virtual, realizando la configuración completa del metodo de pago, de manera que NopCommerce 3.30 pueda ejecutar una compra usando todas las tarjetas que ofrece Zona Virtual.


**Nota:**

1. No almacena información de tarjetas de credito o Débito.
2. Transporte vía Get y Post de la información.
3. Se realiza un redirect a la pagina de Zona Virtual para efectuar el pago, la cual regresa a la tienda para confirmar la compra.