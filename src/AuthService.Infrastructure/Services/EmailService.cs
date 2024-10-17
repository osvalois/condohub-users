using AuthService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendPasswordRecoveryEmailAsync(string email, string token)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token cannot be null or empty", nameof(token));

            var resetPasswordUrl = _configuration["PasswordRecovery:ResetPasswordUrl"];
            if (string.IsNullOrEmpty(resetPasswordUrl))
                throw new InvalidOperationException("Reset password URL is not configured.");

            var subject = "Recuperación de Contraseña";
            var body = $@"
                <h2>Recuperación de Contraseña</h2>
                <p>Has solicitado restablecer tu contraseña. Utiliza el siguiente enlace para completar el proceso:</p>
                <p><a href=""{resetPasswordUrl}?token={token}"">Restablecer Contraseña</a></p>
                <p>Si no puedes hacer clic en el enlace, copia y pega esta URL en tu navegador:</p>
                <p>{resetPasswordUrl}?token={token}</p>
                <p>Si no has solicitado este cambio, por favor ignora este correo.</p>
                <p>Este enlace expirará en 15 minutos por razones de seguridad.</p>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string email, string name)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));

            var subject = "Bienvenido a nuestro servicio";
            var body = $@"
                <h2>Bienvenido, {name}!</h2>
                <p>Gracias por registrarte en nuestro servicio. Tu cuenta ha sido creada exitosamente.</p>
                <p>Si tienes alguna pregunta, no dudes en contactarnos.</p>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendAccountLockedEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            var subject = "Alerta de Seguridad: Tu cuenta ha sido bloqueada";
            var body = @"
                <h2>Alerta de Seguridad</h2>
                <p>Tu cuenta ha sido bloqueada debido a múltiples intentos fallidos de inicio de sesión.</p>
                <p>Si no has sido tú quien ha intentado acceder a tu cuenta, por favor contacta con nuestro equipo de soporte inmediatamente.</p>
                <p>Si has sido tú, puedes desbloquear tu cuenta siguiendo el proceso de recuperación de contraseña.</p>";

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string email, string subject, string body)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPortString = _configuration["EmailSettings:SmtpPort"];
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPortString) ||
                    string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword) ||
                    string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderName))
                {
                    throw new InvalidOperationException("Email settings are not properly configured.");
                }

                if (!int.TryParse(smtpPortString, out int smtpPort))
                {
                    throw new InvalidOperationException("Invalid SMTP port configuration.");
                }

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(new MailAddress(email));

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {email}");
                throw new ApplicationException($"Error sending email to {email}", ex);
            }
        }
    }
}