namespace DistriCatalogoAPI.Domain.Constants
{
    public static class FeatureCodes
    {
        // Pedidos
        public const string PEDIDO_WHATSAPP = "pedido_whatsapp";
        public const string PEDIDO_DATOS_ADICIONALES = "pedido_datos_adicionales";
        public const string PEDIDO_CAMPOS_REQUERIDOS = "pedido_campos_requeridos";
        
        // Autenticación
        public const string CLIENTE_AUTENTICACION = "cliente_autenticacion";
        public const string CLIENTE_REGISTRO_PUBLICO = "cliente_registro_publico";
        
        // Catálogo
        public const string CATALOGO_PRECIOS_OCULTOS = "catalogo_precios_ocultos";
        public const string CATALOGO_STOCK_VISIBLE = "catalogo_stock_visible";
        public const string CATALOGO_DESCUENTO_MAXIMO = "catalogo_descuento_maximo";
        
        // Notificaciones
        public const string NOTIFICACION_EMAIL = "notificacion_email";
        public const string NOTIFICACION_SMS = "notificacion_sms";
        public const string NOTIFICACION_WEBHOOK = "notificacion_webhook";
        
        // Categorías
        public static class Categorias
        {
            public const string PEDIDOS = "pedidos";
            public const string SEGURIDAD = "seguridad";
            public const string CATALOGO = "catalogo";
            public const string NOTIFICACIONES = "notificaciones";
        }
    }
}