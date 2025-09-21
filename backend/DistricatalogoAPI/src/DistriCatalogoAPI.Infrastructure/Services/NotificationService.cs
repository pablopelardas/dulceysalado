using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Application.Interfaces;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IUserNotificationPreferencesRepository _notificationPreferencesRepository;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _baseUrl;
        private readonly string _backofficeUrl;

        public NotificationService(ILogger<NotificationService> logger, IConfiguration configuration, IUserRepository userRepository, IUserNotificationPreferencesRepository notificationPreferencesRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _userRepository = userRepository;
            _notificationPreferencesRepository = notificationPreferencesRepository;
            
            // Configuración SMTP
            _smtpHost = configuration["Email:SmtpHost"] ?? "";
            _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
            _smtpUsername = configuration["Email:SmtpUsername"] ?? "";
            _smtpPassword = configuration["Email:SmtpPassword"] ?? "";
            _fromEmail = configuration["Email:FromEmail"] ?? "";
            _fromName = configuration["Email:FromName"] ?? "Dulce y Salado";
            _baseUrl = configuration["Application:CatalogUrl"] ?? "https://dulceysaladomax.com";
            _backofficeUrl = configuration["Application:BackofficeUrl"] ?? "https://admin.dulceysaladomax.com";
        }

        public async Task NotificarCambioEstadoPedidoAsync(Pedido pedido, string estadoAnterior)
        {
            if (string.IsNullOrEmpty(pedido.Cliente?.Email))
            {
                _logger.LogWarning("Cliente {ClienteId} no tiene email para notificar cambio de estado", pedido.ClienteId);
                return;
            }

            var asunto = $"Actualización de tu pedido #{pedido.Numero} - Dulce y Salado";
            var mensaje = GenerarMensajeCambioEstado(pedido, estadoAnterior);

            await EnviarEmailAsync(pedido.Cliente.Email, asunto, mensaje, true);
            
            _logger.LogInformation("Notificación de cambio de estado enviada para pedido {PedidoId}: {EstadoAnterior} → {EstadoActual}", 
                pedido.Id, estadoAnterior, pedido.Estado.ToString());
        }

        public async Task NotificarCorreccionPedidoAsync(Pedido pedido, string token, string? motivoCorreccion = null)
        {
            if (string.IsNullOrEmpty(pedido.Cliente?.Email))
            {
                _logger.LogWarning("Cliente {ClienteId} no tiene email para notificar corrección", pedido.ClienteId);
                return;
            }

            var asunto = $"Revisión necesaria para tu pedido #{pedido.Numero} - Dulce y Salado";
            var correctionUrl = $"{_baseUrl}/correccion/{token}";
            var mensaje = GenerarMensajeCorreccion(pedido, correctionUrl, motivoCorreccion);

            await EnviarEmailAsync(pedido.Cliente.Email, asunto, mensaje, true);
            
            _logger.LogInformation("Notificación de corrección enviada para pedido {PedidoId} con token {Token}", 
                pedido.Id, token);
        }

        public async Task<bool> EnviarEmailAsync(string destinatario, string asunto, string mensaje, bool esHtml = false)
        {
            if (string.IsNullOrEmpty(_smtpHost) || string.IsNullOrEmpty(_smtpUsername))
            {
                _logger.LogWarning("Configuración SMTP incompleta. Email no enviado a {Destinatario}", destinatario);
                return false;
            }

            try
            {
                using var client = new SmtpClient(_smtpHost, _smtpPort)
                {
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                    EnableSsl = true
                };

                using var mail = new MailMessage(_fromEmail, destinatario)
                {
                    Subject = asunto,
                    Body = mensaje,
                    IsBodyHtml = esHtml
                };

                mail.From = new MailAddress(_fromEmail, _fromName);

                await client.SendMailAsync(mail);
                
                _logger.LogInformation("Email enviado exitosamente a {Destinatario}", destinatario);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email a {Destinatario}", destinatario);
                return false;
            }
        }

        private string GenerarMensajeCambioEstado(Pedido pedido, string estadoAnterior)
        {
            var estadoActual = pedido.Estado.ToString();
            var cliente = pedido.Cliente?.Nombre ?? "Cliente";

            var mensaje = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #1E1E1E; margin: 0; padding: 0; background-color: #F5F5F5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: #FFFFFF; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1); }}
                        .header {{ background-color: #000000; color: #FFFFFF; padding: 30px 20px; text-align: center; }}
                        .logo {{ margin-bottom: 15px; }}
                        .logo img {{ height: 100px; width: auto; }}
                        .content {{ padding: 30px 20px; background-color: #FFFFFF; }}
                        .pedido-info {{ background-color: #F5F5F5; padding: 20px; margin: 20px 0; border-radius: 8px; border-left: 4px solid #E50000; }}
                        .estado {{ font-weight: bold; color: #E50000; }}
                        .footer {{ background-color: #1E1E1E; color: #FFFFFF; padding: 20px; text-align: center; font-size: 14px; }}
                        h1 {{ margin: 0; font-size: 24px; font-weight: 600; }}
                        h2 {{ color: #1E1E1E; font-size: 18px; margin-bottom: 15px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>
                                <img src='https://dulceysaladomax.com/assets/logo-dulceysalado.png' alt='Dulce & Salado' />
                            </div>
                            <h1>Actualización de tu Pedido</h1>
                        </div>
                        <div class='content'>
                            <p style='font-size: 16px; margin-bottom: 25px;'>Hola <strong>{cliente}</strong>,</p>
                            <p>Tu pedido ha sido actualizado:</p>
                            
                            <div class='pedido-info'>
                                <h2>Información del Pedido</h2>
                                <p><strong>Número:</strong> <span class='estado'>#{pedido.Numero}</span></p>
                                <p><strong>Estado anterior:</strong> {estadoAnterior}</p>
                                <p><strong>Estado actual:</strong> <span class='estado'>{estadoActual}</span></p>
                                <p><strong>Total:</strong> <span class='estado'>${pedido.MontoTotal:N2}</span></p>
                            </div>";

            // Mensajes específicos según el estado
            mensaje += estadoActual switch
            {
                "Aceptado" => "<p>✅ <strong>¡Excelente!</strong> Tu pedido ha sido aceptado y estamos preparándolo.</p>",
                "Rechazado" => $"<p>❌ <strong>Lo sentimos.</strong> Tu pedido ha sido rechazado. {(string.IsNullOrEmpty(pedido.MotivoRechazo) ? "" : $"<br><strong>Motivo:</strong> {pedido.MotivoRechazo}")}</p>",
                "Completado" => "<p>🎉 <strong>¡Listo!</strong> Tu pedido está completado. ¡Esperamos que lo disfrutes!</p>",
                "Cancelado" => "<p>🚫 <strong>Cancelado.</strong> Tu pedido ha sido cancelado.</p>",
                _ => "<p>ℹ️ Te mantendremos informado sobre el progreso de tu pedido.</p>"
            };

            mensaje += @"
                            <p style='margin-top: 30px;'>Gracias por elegirnos.</p>
                            <p><strong>Equipo Dulce & Salado</strong></p>
                        </div>
                        <div class='footer'>
                            <p>Sistema de Gestión de Pedidos - Dulce & Salado</p>
                            <p style='font-size: 12px; margin-top: 10px; opacity: 0.8;'>Este es un mensaje automático, no responder a este email</p>
                        </div>
                    </div>
                </body>
                </html>";

            return mensaje;
        }

        private string GenerarMensajeCorreccion(Pedido pedido, string correctionUrl, string? motivoCorreccion)
        {
            var cliente = pedido.Cliente?.Nombre ?? "Cliente";

            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #1E1E1E; margin: 0; padding: 0; background-color: #F5F5F5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: #FFFFFF; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1); }}
                        .header {{ background-color: #000000; color: #FFFFFF; padding: 30px 20px; text-align: center; }}
                        .logo {{ margin-bottom: 15px; }}
                        .logo img {{ height: 100px; width: auto; }}
                        .content {{ padding: 30px 20px; background-color: #FFFFFF; }}
                        .pedido-info {{ background-color: #F5F5F5; padding: 20px; margin: 20px 0; border-radius: 8px; border-left: 4px solid #E50000; }}
                        .btn {{ display: inline-block; padding: 15px 30px; background-color: #E50000; color: #FFFFFF; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0; }}
                        .btn:hover {{ background-color: #CC0000; }}
                        .warning {{ background-color: #fff3cd; color: #856404; padding: 15px; border-radius: 8px; border-left: 4px solid #ffc107; margin: 20px 0; }}
                        .footer {{ background-color: #1E1E1E; color: #FFFFFF; padding: 20px; text-align: center; font-size: 14px; }}
                        h1 {{ margin: 0; font-size: 24px; font-weight: 600; }}
                        h2 {{ color: #1E1E1E; font-size: 18px; margin-bottom: 15px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>
                                <img src='https://dulceysaladomax.com/assets/logo-dulceysalado.png' alt='Dulce & Salado' />
                            </div>
                            <h1>Revisión Necesaria</h1>
                        </div>
                        <div class='content'>
                            <div class='warning'>
                                <p style='margin: 0;'><strong>⚠️ Tu pedido necesita algunos ajustes</strong></p>
                            </div>
                            <p style='font-size: 16px; margin-bottom: 25px;'>Hola <strong>{cliente}</strong>,</p>
                            <p>Tu pedido necesita algunos ajustes debido a disponibilidad de stock:</p>
                            
                            <div class='pedido-info'>
                                <h2>Información del Pedido</h2>
                                <p><strong>Número:</strong> <span style='color: #E50000; font-weight: bold;'>#{pedido.Numero}</span></p>
                                <p><strong>Total original:</strong> <span style='color: #E50000; font-weight: bold;'>${pedido.MontoTotal:N2}</span></p>
                                {(string.IsNullOrEmpty(motivoCorreccion) ? "" : $"<p><strong>Motivo:</strong> {motivoCorreccion}</p>")}
                            </div>
                        
                            <div style='text-align: center; margin: 30px 0;'>
                                <p style='font-size: 18px; margin-bottom: 20px;'><strong>📋 Revisa los cambios y apruébalos:</strong></p>
                                <a href='{correctionUrl}' class='btn' style='display: inline-block; padding: 15px 30px; background-color: #E50000; color: #FFFFFF !important; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0;'>👀 Ver y Aprobar Cambios</a>
                            </div>
                            
                            <div class='warning'>
                                <p style='margin: 0;'><strong>⏰ Importante:</strong> Este enlace es válido por 48 horas.</p>
                            </div>
                        
                            <p style='margin-top: 30px;'>Si tienes alguna duda, no dudes en contactarnos.</p>
                            <p><strong>Equipo Dulce & Salado</strong></p>
                        </div>
                        <div class='footer'>
                            <p>Sistema de Gestión de Pedidos - Dulce & Salado</p>
                            <p style='font-size: 12px; margin-top: 10px; opacity: 0.8;'>Este es un mensaje automático, no responder a este email</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        // NUEVOS MÉTODOS: Notificaciones a usuarios de la empresa
        
        public async Task NotificarNuevoPedidoAsync(Pedido pedido)
        {
            try
            {
                // Obtener usuarios que tienen habilitadas las notificaciones de nuevos pedidos
                var usuariosConNotificacion = await _notificationPreferencesRepository
                    .GetUsersWithEmailForNotificationTypeAsync(pedido.EmpresaId, TipoNotificacion.NuevoPedido);

                if (!usuariosConNotificacion.Any())
                {
                    _logger.LogWarning("No hay usuarios con notificaciones de nuevos pedidos habilitadas en la empresa {EmpresaId}", pedido.EmpresaId);
                    return;
                }

                var asunto = $"🆕 Nuevo pedido recibido #{pedido.Numero}";
                var mensaje = GenerarMensajeNuevoPedido(pedido);

                foreach (var (preferences, userEmail) in usuariosConNotificacion)
                {
                    await EnviarEmailAsync(userEmail, asunto, mensaje, true);
                }
                
                _logger.LogInformation("Notificaciones de nuevo pedido enviadas a {UsuarioCount} usuarios para pedido {PedidoId}", 
                    usuariosConNotificacion.Count(), pedido.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificaciones de nuevo pedido {PedidoId}", pedido.Id);
            }
        }

        public async Task NotificarRespuestaCorreccionAsync(Pedido pedido, bool aprobada, string? comentarioCliente = null)
        {
            try
            {
                // Determinar el tipo de notificación según si fue aprobada o rechazada
                var tipoNotificacion = aprobada ? TipoNotificacion.CorreccionAprobada : TipoNotificacion.CorreccionRechazada;
                
                // Obtener usuarios que tienen habilitadas las notificaciones de este tipo
                var usuariosConNotificacion = await _notificationPreferencesRepository
                    .GetUsersWithEmailForNotificationTypeAsync(pedido.EmpresaId, tipoNotificacion);

                if (!usuariosConNotificacion.Any())
                {
                    _logger.LogWarning("No hay usuarios con notificaciones de {TipoNotificacion} habilitadas en la empresa {EmpresaId}", 
                        tipoNotificacion, pedido.EmpresaId);
                    return;
                }

                var accion = aprobada ? "✅ Aprobó" : "❌ Rechazó";
                var asunto = $"{accion} corrección - Pedido #{pedido.Numero}";
                var mensaje = GenerarMensajeRespuestaCorreccion(pedido, aprobada, comentarioCliente);

                foreach (var (preferences, userEmail) in usuariosConNotificacion)
                {
                    await EnviarEmailAsync(userEmail, asunto, mensaje, true);
                }
                
                _logger.LogInformation("Notificaciones de respuesta corrección enviadas a {UsuarioCount} usuarios para pedido {PedidoId}", 
                    usuariosConNotificacion.Count(), pedido.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificaciones de respuesta corrección {PedidoId}", pedido.Id);
            }
        }

        public async Task NotificarCancelacionPedidoAsync(Pedido pedido, string? motivoCancelacion = null)
        {
            try
            {
                // Obtener usuarios que tienen habilitadas las notificaciones de pedidos cancelados
                var usuariosConNotificacion = await _notificationPreferencesRepository
                    .GetUsersWithEmailForNotificationTypeAsync(pedido.EmpresaId, TipoNotificacion.PedidoCancelado);

                if (!usuariosConNotificacion.Any())
                {
                    _logger.LogWarning("No hay usuarios con notificaciones de pedidos cancelados habilitadas en la empresa {EmpresaId}", pedido.EmpresaId);
                    return;
                }

                var asunto = $"🚫 Pedido cancelado #{pedido.Numero}";
                var mensaje = GenerarMensajeCancelacionPedido(pedido, motivoCancelacion);

                foreach (var (preferences, userEmail) in usuariosConNotificacion)
                {
                    await EnviarEmailAsync(userEmail, asunto, mensaje, true);
                }
                
                _logger.LogInformation("Notificaciones de cancelación enviadas a {UsuarioCount} usuarios para pedido {PedidoId}", 
                    usuariosConNotificacion.Count(), pedido.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificaciones de cancelación {PedidoId}", pedido.Id);
            }
        }

        // Métodos privados para generar los mensajes HTML

        private string GenerarMensajeNuevoPedido(Pedido pedido)
        {
            var cliente = pedido.Cliente?.Nombre ?? "Cliente";
            var fechaPedido = pedido.CreatedAt.ToString("dd/MM/yyyy HH:mm");

            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #1E1E1E; margin: 0; padding: 0; background-color: #F5F5F5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: #FFFFFF; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1); }}
                        .header {{ background-color: #000000; color: #FFFFFF; padding: 30px 20px; text-align: center; }}
                        .logo {{ margin-bottom: 15px; }}
                        .logo img {{ height: 100px; width: auto; }}
                        .content {{ padding: 30px 20px; background-color: #FFFFFF; }}
                        .pedido-info {{ background-color: #F5F5F5; padding: 20px; margin: 20px 0; border-radius: 8px; border-left: 4px solid #E50000; }}
                        .items-list {{ background-color: #4A4A4A; color: #FFFFFF; padding: 20px; margin: 20px 0; border-radius: 8px; }}
                        .item {{ margin: 10px 0; padding: 12px; background-color: #1E1E1E; border-radius: 6px; }}
                        .footer {{ background-color: #1E1E1E; color: #FFFFFF; padding: 20px; text-align: center; font-size: 14px; }}
                        .highlight {{ color: #E50000; font-weight: bold; }}
                        .total {{ font-size: 1.3em; font-weight: bold; color: #E50000; }}
                        .btn {{ display: inline-block; padding: 12px 24px; background-color: #E50000; color: #FFFFFF; text-decoration: none; border-radius: 6px; font-weight: bold; margin-top: 20px; }}
                        .btn:hover {{ background-color: #CC0000; }}
                        h1 {{ margin: 0; font-size: 24px; font-weight: 600; }}
                        h2 {{ color: #1E1E1E; font-size: 18px; margin-bottom: 15px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>
                                <img src='https://dulceysaladomax.com/assets/logo-dulceysalado.png' alt='Dulce & Salado' />
                            </div>
                            <h1>🆕 Nuevo Pedido Recibido</h1>
                        </div>
                        <div class='content'>
                            <p style='font-size: 16px; margin-bottom: 25px;'>Se ha recibido un nuevo pedido que requiere atención:</p>
                            
                            <div class='pedido-info'>
                                <h2>Información del Pedido</h2>
                                <p><strong>Número:</strong> <span class='highlight'>#{pedido.Numero}</span></p>
                                <p><strong>Cliente:</strong> {cliente}</p>
                                <p><strong>Email:</strong> {pedido.Cliente?.Email ?? "No especificado"}</p>
                                <p><strong>Teléfono:</strong> {pedido.Cliente?.Telefono ?? "No especificado"}</p>
                                <p><strong>Fecha:</strong> {fechaPedido}</p>
                                <p><strong>Total:</strong> <span class='total'>${pedido.MontoTotal:N2}</span></p>
                            </div>

                            {(string.IsNullOrEmpty(pedido.Observaciones) ? "" : $@"
                            <div class='pedido-info'>
                                <h2>Observaciones del Cliente</h2>
                                <p style='font-style: italic; color: #4A4A4A;'>{pedido.Observaciones}</p>
                            </div>")}

                            {(string.IsNullOrEmpty(pedido.DireccionEntrega) ? "" : $@"
                            <div class='pedido-info'>
                                <h2>Información de Entrega</h2>
                                <p><strong>Dirección:</strong> {pedido.DireccionEntrega}</p>
                                {(string.IsNullOrEmpty(pedido.FechaEntrega?.ToString()) ? "" : $"<p><strong>Fecha solicitada:</strong> {pedido.FechaEntrega?.ToString("dd/MM/yyyy")}</p>")}
                                {(string.IsNullOrEmpty(pedido.HorarioEntrega) ? "" : $"<p><strong>Horario:</strong> {pedido.HorarioEntrega}</p>")}
                            </div>")}

                            <div class='items-list'>
                                <h2 style='color: #FFFFFF; margin-bottom: 15px;'>Productos ({pedido.Items.Count} items)</h2>
                                {string.Join("", pedido.Items.Select(item => $@"
                                <div class='item'>
                                    <strong style='color: #FFFFFF;'>{item.NombreProducto}</strong><br>
                                    <span style='color: #F5F5F5;'>Código: {item.CodigoProducto}</span><br>
                                    <span style='color: #F5F5F5;'>Cantidad: {item.Cantidad} × ${item.PrecioUnitario:N2} = </span><strong style='color: #E50000;'>${item.Subtotal:N2}</strong>
                                    {(string.IsNullOrEmpty(item.Observaciones) ? "" : $"<br><em style='color: #F5F5F5;'>Obs: {item.Observaciones}</em>")}
                                </div>"))}
                            </div>
                            
                            <div style='text-align: center; margin-top: 30px;'>
                                <a href='{_backofficeUrl}/admin/pedidos/{pedido.Id}' class='btn' style='display: inline-block; padding: 15px 30px; background-color: #E50000; color: #FFFFFF !important; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0;'>Ver Pedido Completo</a>
                            </div>
                        </div>
                        <div class='footer'>
                            <p>Sistema de Gestión de Pedidos - Dulce & Salado</p>
                            <p style='font-size: 12px; margin-top: 10px; opacity: 0.8;'>Este es un mensaje automático, no responder a este email</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerarMensajeRespuestaCorreccion(Pedido pedido, bool aprobada, string? comentarioCliente)
        {
            var cliente = pedido.Cliente?.Nombre ?? "Cliente";
            var accion = aprobada ? "aprobó" : "rechazó";
            var color = aprobada ? "#E50000" : "#E50000";
            var icono = aprobada ? "✅" : "❌";
            var headerBg = aprobada ? "#000000" : "#4A4A4A";

            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #1E1E1E; margin: 0; padding: 0; background-color: #F5F5F5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: #FFFFFF; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1); }}
                        .header {{ background-color: {headerBg}; color: #FFFFFF; padding: 30px 20px; text-align: center; }}
                        .logo {{ margin-bottom: 15px; }}
                        .logo img {{ height: 100px; width: auto; }}
                        .content {{ padding: 30px 20px; background-color: #FFFFFF; }}
                        .pedido-info {{ background-color: #F5F5F5; padding: 20px; margin: 20px 0; border-radius: 8px; border-left: 4px solid {color}; }}
                        .footer {{ background-color: #1E1E1E; color: #FFFFFF; padding: 20px; text-align: center; font-size: 14px; }}
                        .status {{ color: {color}; font-weight: bold; }}
                        .btn {{ display: inline-block; padding: 12px 24px; background-color: #E50000; color: #FFFFFF; text-decoration: none; border-radius: 6px; font-weight: bold; margin-top: 20px; }}
                        .btn:hover {{ background-color: #CC0000; }}
                        h1 {{ margin: 0; font-size: 24px; font-weight: 600; }}
                        h2 {{ color: #1E1E1E; font-size: 18px; margin-bottom: 15px; }}
                        .alert {{ padding: 15px; margin: 20px 0; border-radius: 8px; }}
                        .alert-success {{ background-color: #d4edda; border-left: 4px solid #28a745; color: #155724; }}
                        .alert-warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; color: #856404; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>
                                <img src='https://dulceysaladomax.com/assets/logo-dulceysalado.png' alt='Dulce & Salado' />
                            </div>
                            <h1>{icono} Respuesta de Corrección</h1>
                        </div>
                        <div class='content'>
                            <p style='font-size: 16px; margin-bottom: 25px;'>El cliente <strong>{cliente}</strong> ha <span class='status'>{accion}</span> la corrección del pedido:</p>
                            
                            <div class='pedido-info'>
                                <h2>Información del Pedido</h2>
                                <p><strong>Número:</strong> <span class='status'>#{pedido.Numero}</span></p>
                                <p><strong>Cliente:</strong> {cliente}</p>
                                <p><strong>Email:</strong> {pedido.Cliente?.Email ?? "No especificado"}</p>
                                <p><strong>Estado actual:</strong> <span class='status'>{pedido.Estado}</span></p>
                                <p><strong>Total:</strong> <span class='status'>${pedido.MontoTotal:N2}</span></p>
                            </div>

                            {(string.IsNullOrEmpty(comentarioCliente) ? "" : $@"
                            <div class='pedido-info'>
                                <h2>Comentario del Cliente</h2>
                                <p style='font-style: italic; color: #4A4A4A;'>{comentarioCliente}</p>
                            </div>")}
                            
                            <div class='{(aprobada ? "alert-success" : "alert-warning")} alert'>
                                <p style='margin: 0;'>
                                    {(aprobada ? 
                                        "✅ El pedido ha sido confirmado con los cambios realizados. Puedes proceder con la preparación." : 
                                        "⚠️ El cliente no está conforme con los cambios. Es recomendable contactarlo para resolver la situación.")}
                                </p>
                            </div>
                            
                            <div style='text-align: center; margin-top: 30px;'>
                                <a href='{_backofficeUrl}/admin/pedidos/{pedido.Id}' class='btn' style='display: inline-block; padding: 15px 30px; background-color: #E50000; color: #FFFFFF !important; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0;'>Ver Pedido Completo</a>
                            </div>
                        </div>
                        <div class='footer'>
                            <p>Sistema de Gestión de Pedidos - Dulce & Salado</p>
                            <p style='font-size: 12px; margin-top: 10px; opacity: 0.8;'>Este es un mensaje automático, no responder a este email</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerarMensajeCancelacionPedido(Pedido pedido, string? motivoCancelacion)
        {
            var cliente = pedido.Cliente?.Nombre ?? "Cliente";

            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #1E1E1E; margin: 0; padding: 0; background-color: #F5F5F5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: #FFFFFF; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1); }}
                        .header {{ background-color: #4A4A4A; color: #FFFFFF; padding: 30px 20px; text-align: center; }}
                        .logo {{ margin-bottom: 15px; }}
                        .logo img {{ height: 100px; width: auto; }}
                        .content {{ padding: 30px 20px; background-color: #FFFFFF; }}
                        .pedido-info {{ background-color: #F5F5F5; padding: 20px; margin: 20px 0; border-radius: 8px; border-left: 4px solid #E50000; }}
                        .footer {{ background-color: #1E1E1E; color: #FFFFFF; padding: 20px; text-align: center; font-size: 14px; }}
                        .cancelado {{ color: #E50000; font-weight: bold; }}
                        .btn {{ display: inline-block; padding: 12px 24px; background-color: #E50000; color: #FFFFFF; text-decoration: none; border-radius: 6px; font-weight: bold; margin-top: 20px; }}
                        .btn:hover {{ background-color: #CC0000; }}
                        h1 {{ margin: 0; font-size: 24px; font-weight: 600; }}
                        h2 {{ color: #1E1E1E; font-size: 18px; margin-bottom: 15px; }}
                        .alert {{ padding: 15px; margin: 20px 0; border-radius: 8px; background-color: #fff3cd; border-left: 4px solid #ffc107; color: #856404; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>
                                <img src='https://dulceysaladomax.com/assets/logo-dulceysalado.png' alt='Dulce & Salado' />
                            </div>
                            <h1>🚫 Pedido Cancelado</h1>
                        </div>
                        <div class='content'>
                            <p style='font-size: 16px; margin-bottom: 25px;'>El cliente <strong>{cliente}</strong> ha <span class='cancelado'>cancelado</span> su pedido:</p>
                            
                            <div class='pedido-info'>
                                <h2>Información del Pedido</h2>
                                <p><strong>Número:</strong> <span class='cancelado'>#{pedido.Numero}</span></p>
                                <p><strong>Cliente:</strong> {cliente}</p>
                                <p><strong>Email:</strong> {pedido.Cliente?.Email ?? "No especificado"}</p>
                                <p><strong>Estado:</strong> <span class='cancelado'>Cancelado</span></p>
                                <p><strong>Monto que se deja de facturar:</strong> <span class='cancelado'>${pedido.MontoTotal:N2}</span></p>
                            </div>

                            {(string.IsNullOrEmpty(motivoCancelacion) ? "" : $@"
                            <div class='pedido-info'>
                                <h2>Motivo de Cancelación</h2>
                                <p style='font-style: italic; color: #4A4A4A;'>{motivoCancelacion}</p>
                            </div>")}
                        
                            <div class='alert'>
                                <p style='margin: 0;'>
                                    ℹ️ El pedido ha sido marcado como cancelado en el sistema. 
                                    No es necesario preparar los productos ni realizar entregas para este pedido.
                                </p>
                            </div>
                            
                            <div style='text-align: center; margin-top: 30px;'>
                                <a href='{_backofficeUrl}/admin/pedidos/{pedido.Id}' class='btn' style='display: inline-block; padding: 15px 30px; background-color: #E50000; color: #FFFFFF !important; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0;'>Ver Pedido Cancelado</a>
                            </div>
                        </div>
                        <div class='footer'>
                            <p>Sistema de Gestión de Pedidos - Dulce & Salado</p>
                            <p style='font-size: 12px; margin-top: 10px; opacity: 0.8;'>Este es un mensaje automático, no responder a este email</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        // Métodos para Solicitudes de Reventa
        public async Task NotificarNuevaSolicitudReventaAsync(SolicitudReventa solicitud, Cliente cliente)
        {
            try
            {
                // Obtener usuarios que tienen habilitadas las notificaciones de solicitudes de reventa
                var usuariosConNotificacion = await _notificationPreferencesRepository
                    .GetUsersWithEmailForNotificationTypeAsync(solicitud.EmpresaId, TipoNotificacion.NuevaSolicitudReventa);

                if (!usuariosConNotificacion.Any())
                {
                    _logger.LogWarning("No hay usuarios con notificaciones de solicitudes de reventa habilitadas en la empresa {EmpresaId}", solicitud.EmpresaId);
                    return;
                }

                var asunto = $"📋 Nueva Solicitud de Cuenta de Reventa - {cliente.Nombre ?? cliente.Codigo}";
                var mensaje = GenerarMensajeNuevaSolicitudReventa(solicitud, cliente);

                foreach (var (preferences, userEmail) in usuariosConNotificacion)
                {
                    await EnviarEmailAsync(userEmail, asunto, mensaje, true);
                }
                
                _logger.LogInformation("Notificaciones de nueva solicitud de reventa enviadas a {UsuarioCount} usuarios para solicitud {SolicitudId}", 
                    usuariosConNotificacion.Count(), solicitud.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificaciones de nueva solicitud de reventa {SolicitudId}", solicitud.Id);
            }
        }

        public async Task NotificarRespuestaSolicitudReventaAsync(SolicitudReventa solicitud, Cliente cliente)
        {
            if (string.IsNullOrEmpty(cliente.Email))
            {
                _logger.LogWarning("Cliente {ClienteId} no tiene email para notificar respuesta de solicitud", cliente.Id);
                return;
            }

            var aprobada = solicitud.Estado == EstadoSolicitud.Aprobada;
            var asunto = aprobada 
                ? "✅ Tu solicitud de cuenta de reventa ha sido aprobada - Dulce y Salado"
                : "📋 Respuesta a tu solicitud de cuenta de reventa - Dulce y Salado";
            
            var mensaje = GenerarMensajeRespuestaSolicitudReventa(solicitud, cliente, aprobada);

            await EnviarEmailAsync(cliente.Email, asunto, mensaje, true);
            
            _logger.LogInformation("Notificación de respuesta de solicitud enviada al cliente {ClienteId}: {Estado}", 
                cliente.Id, solicitud.Estado.ToString());
        }

        private string GenerarMensajeNuevaSolicitudReventa(SolicitudReventa solicitud, Cliente cliente)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #1E1E1E; margin: 0; padding: 0; background-color: #F5F5F5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: #FFFFFF; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1); }}
                        .header {{ background-color: #4A4A4A; color: #FFFFFF; padding: 30px 20px; text-align: center; }}
                        .logo {{ margin-bottom: 15px; }}
                        .logo img {{ height: 100px; width: auto; }}
                        .content {{ padding: 30px 20px; background-color: #FFFFFF; }}
                        .solicitud-info {{ background-color: #F5F5F5; padding: 20px; margin: 20px 0; border-radius: 8px; border-left: 4px solid #28a745; }}
                        .footer {{ background-color: #1E1E1E; color: #FFFFFF; padding: 20px; text-align: center; font-size: 14px; }}
                        h1 {{ margin: 0; font-size: 24px; font-weight: 600; }}
                        h2 {{ color: #1E1E1E; font-size: 18px; margin-bottom: 15px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>
                                <img src='https://dulceysaladomax.com/assets/logo-dulceysalado.png' alt='Dulce & Salado' />
                            </div>
                            <h1>📋 Nueva Solicitud de Cuenta de Reventa</h1>
                        </div>
                        <div class='content'>
                            <p style='font-size: 16px; margin-bottom: 25px;'>Se ha recibido una nueva solicitud de cuenta de reventa:</p>
                            
                            <div class='solicitud-info'>
                                <h2>Datos del Cliente</h2>
                                <p><strong>Cliente:</strong> {cliente.Nombre ?? cliente.Codigo}</p>
                                <p><strong>Email:</strong> {cliente.Email ?? "No especificado"}</p>
                                <p><strong>Teléfono:</strong> {cliente.Telefono ?? "No especificado"}</p>
                            </div>

                            <div class='solicitud-info'>
                                <h2>Datos de la Empresa</h2>
                                <p><strong>CUIT:</strong> {solicitud.Cuit ?? "No especificado"}</p>
                                <p><strong>Razón Social:</strong> {solicitud.RazonSocial ?? "No especificado"}</p>
                                <p><strong>Categoría IVA:</strong> {solicitud.CategoriaIva ?? "No especificado"}</p>
                                <p><strong>Dirección:</strong> {solicitud.DireccionComercial ?? "No especificado"}</p>
                                <p><strong>Localidad:</strong> {solicitud.Localidad ?? "No especificado"}</p>
                                <p><strong>Provincia:</strong> {solicitud.Provincia ?? "No especificado"}</p>
                                <p><strong>Código Postal:</strong> {solicitud.CodigoPostal ?? "No especificado"}</p>
                                <p><strong>Teléfono Comercial:</strong> {solicitud.TelefonoComercial ?? "No especificado"}</p>
                                <p><strong>Email Comercial:</strong> {solicitud.EmailComercial ?? "No especificado"}</p>
                            </div>
                            
                            <div style='text-align: center; margin-top: 30px;'>
                                <a href='{_backofficeUrl}/admin/solicitudes-reventa' class='btn' style='display: inline-block; padding: 15px 30px; background-color: #28a745; color: #FFFFFF !important; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0;'>Gestionar Solicitud</a>
                            </div>
                        </div>
                        <div class='footer'>
                            <p>Sistema de Gestión - Dulce & Salado</p>
                            <p style='font-size: 12px; margin-top: 10px; opacity: 0.8;'>Este es un mensaje automático</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private string GenerarMensajeRespuestaSolicitudReventa(SolicitudReventa solicitud, Cliente cliente, bool aprobada)
        {
            var colorEstado = aprobada ? "#28a745" : "#dc3545";
            var textoEstado = aprobada ? "APROBADA" : "RECHAZADA";
            var mensaje = aprobada 
                ? "¡Felicitaciones! Tu solicitud de cuenta de reventa ha sido aprobada. Ya puedes acceder a los precios especiales de reventa."
                : "Lamentablemente tu solicitud no ha sido aprobada en esta ocasión.";

            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #1E1E1E; margin: 0; padding: 0; background-color: #F5F5F5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: #FFFFFF; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1); }}
                        .header {{ background-color: #4A4A4A; color: #FFFFFF; padding: 30px 20px; text-align: center; }}
                        .logo {{ margin-bottom: 15px; }}
                        .logo img {{ height: 100px; width: auto; }}
                        .content {{ padding: 30px 20px; background-color: #FFFFFF; }}
                        .estado-info {{ background-color: #F5F5F5; padding: 20px; margin: 20px 0; border-radius: 8px; border-left: 4px solid {colorEstado}; }}
                        .footer {{ background-color: #1E1E1E; color: #FFFFFF; padding: 20px; text-align: center; font-size: 14px; }}
                        h1 {{ margin: 0; font-size: 24px; font-weight: 600; }}
                        h2 {{ color: #1E1E1E; font-size: 18px; margin-bottom: 15px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>
                                <img src='https://dulceysaladomax.com/assets/logo-dulceysalado.png' alt='Dulce & Salado' />
                            </div>
                            <h1>Respuesta a tu Solicitud de Reventa</h1>
                        </div>
                        <div class='content'>
                            <p style='font-size: 16px; margin-bottom: 25px;'>Hola <strong>{cliente.Nombre ?? "Cliente"}</strong>,</p>
                            
                            <div class='estado-info'>
                                <h2>Estado de tu Solicitud: <span style='color: {colorEstado}'>{textoEstado}</span></h2>
                                <p>{mensaje}</p>
                            </div>

                            {(string.IsNullOrEmpty(solicitud.ComentarioRespuesta) ? "" : $@"
                            <div class='estado-info'>
                                <h2>Comentarios</h2>
                                <p>{solicitud.ComentarioRespuesta}</p>
                            </div>")}
                            
                            {(aprobada ? $@"
                            <div style='text-align: center; margin-top: 30px;'>
                                <p>Ya puedes acceder al catálogo con los nuevos precios:</p>
                                <a href='{_baseUrl}' class='btn' style='display: inline-block; padding: 15px 30px; background-color: #28a745; color: #FFFFFF !important; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0;'>Ir al Catálogo</a>
                            </div>" : @"
                            <div style='text-align: center; margin-top: 30px;'>
                                <p>Si tienes alguna duda o deseas más información, no dudes en contactarnos.</p>
                            </div>")}
                        </div>
                        <div class='footer'>
                            <p>Sistema de Gestión - Dulce & Salado</p>
                            <p style='font-size: 12px; margin-top: 10px; opacity: 0.8;'>Este es un mensaje automático</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}