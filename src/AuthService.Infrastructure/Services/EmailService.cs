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
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordRecoveryEmailAsync(string email, string token)
        {
            var subject = "Recuperación de Contraseña";
            var body = $@"
                <h2>Recuperación de Contraseña</h2>
                <p>Has solicitado restablecer tu contraseña. Utiliza el siguiente token para completar el proceso:</p>
                <p><strong>{token}</strong></p>
                <p>Si no has solicitado este cambio, por favor ignora este correo.</p>
                <p>Este token expirará en 15 minutos por razones de seguridad.</p>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string email, string name)
        {
            var subject = "Bienvenido a nuestro servicio";
            var body = $@"
                <h2>Bienvenido, {name}!</h2>
                <p>Gracias por registrarte en nuestro servicio. Tu cuenta ha sido creada exitosamente.</p>
                <p>Si tienes alguna pregunta, no dudes en contactarnos.</p>";

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string email, string subject, string body)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];

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
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {email}");
                throw new ApplicationException("Error sending email", ex);
            }
        }
    }
}