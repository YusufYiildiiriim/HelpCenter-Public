using System.Net;
using System.Net.Mail;
using HelpCenter.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Infrastructure.Services.Email;

public class EmailService : IEmailService
{
    private readonly SmtpSettings? _smtp;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _logger = logger;
        _smtp = configuration.GetSection("SmtpSettings").Get<SmtpSettings>();
    }

    public async Task SendNewRequestNotificationAsync(
        string toEmail,
        string agentName,
        string customerName,
        string requestTitle,
        string? locale = null,
        CancellationToken cancellationToken = default)
    {
        if (_smtp == null || string.IsNullOrWhiteSpace(_smtp.Host))
        {
            _logger.LogWarning("SmtpSettings is not configured. Skipping email notification.");
            return;
        }

        var english = HelpCenter.Domain.Common.SupportedLocales.Normalize(locale) == HelpCenter.Domain.Common.SupportedLocales.English;
        var body = $"""
            <html>
            <body style="font-family:Arial,sans-serif;color:#333;">
              <h2 style="color:#1e40af;">{(english ? "New request notification" : "Yeni Talep Bildirimi")}</h2>
              <p>{(english ? "Hello" : "Merhaba")} <strong>{agentName}</strong>,</p>
              <p>{(english ? $"A new request was created by <strong>{customerName}</strong>:" : $"<strong>{customerName}</strong> tarafından yeni bir talep oluşturuldu:")}</p>
              <table style="border-collapse:collapse;margin-top:12px;">
                <tr>
                  <td style="padding:6px 12px;background:#f1f5f9;font-weight:bold;">{(english ? "Request title" : "Talep Başlığı")}</td>
                  <td style="padding:6px 12px;">{requestTitle}</td>
                </tr>
              </table>
              <p style="margin-top:20px;">{(english ? "Sign in to HelpCenter to review this request." : "Lütfen sisteme giriş yaparak talebi inceleyin.")}</p>
            </body>
            </html>
            """;

        using var message = new MailMessage
        {
            From = new MailAddress(_smtp.SenderEmail, _smtp.DisplayName),
            Subject = english ? $"[HelpCenter] New request: {requestTitle}" : $"[HelpCenter] Yeni Talep: {requestTitle}",
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(_smtp.Host, _smtp.Port)
        {
            Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
            EnableSsl = _smtp.EnableSsl
        };

        await client.SendMailAsync(message, cancellationToken);
    }

    public async Task SendExpertAssignedNotificationAsync(
        string toEmail,
        string expertName,
        string agentName,
        string requestTitle,
        string? locale = null,
        CancellationToken cancellationToken = default)
    {
        if (_smtp == null || string.IsNullOrWhiteSpace(_smtp.Host))
        {
            _logger.LogWarning("SmtpSettings is not configured. Skipping email notification.");
            return;
        }

        var english = HelpCenter.Domain.Common.SupportedLocales.Normalize(locale) == HelpCenter.Domain.Common.SupportedLocales.English;
        var body = $"""
            <html>
            <body style="font-family:Arial,sans-serif;color:#333;">
              <h2 style="color:#1e40af;">{(english ? "Request assignment notification" : "Talep Yönlendirme Bildirimi")}</h2>
              <p>{(english ? "Hello" : "Merhaba")} <strong>{expertName}</strong>,</p>
              <p>{(english ? $"A new request was assigned to you by <strong>{agentName}</strong>:" : $"<strong>{agentName}</strong> tarafından size yeni bir talep yönlendirildi:")}</p>
              <table style="border-collapse:collapse;margin-top:12px;">
                <tr>
                  <td style="padding:6px 12px;background:#f1f5f9;font-weight:bold;">{(english ? "Request title" : "Talep Başlığı")}</td>
                  <td style="padding:6px 12px;">{requestTitle}</td>
                </tr>
              </table>
              <p style="margin-top:20px;">{(english ? "Sign in to HelpCenter to review this request." : "Lütfen sisteme giriş yaparak talebi inceleyin.")}</p>
            </body>
            </html>
            """;

        using var message = new MailMessage
        {
            From = new MailAddress(_smtp.SenderEmail, _smtp.DisplayName),
            Subject = english ? $"[HelpCenter] Request assigned to you: {requestTitle}" : $"[HelpCenter] Size Yönlendirilen Talep: {requestTitle}",
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(_smtp.Host, _smtp.Port)
        {
            Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
            EnableSsl = _smtp.EnableSsl
        };

        await client.SendMailAsync(message, cancellationToken);
    }

    public async Task SendPasswordResetCodeAsync(
        string toEmail,
        string userName,
        string code,
        string? locale = null,
        CancellationToken cancellationToken = default)
    {
        if (_smtp == null || string.IsNullOrWhiteSpace(_smtp.Host) || string.IsNullOrWhiteSpace(_smtp.SenderEmail))
        {
            _logger.LogError("SMTP is not configured; password reset email cannot be delivered to {Email}.", toEmail);
            throw new InvalidOperationException("Password reset email delivery is not configured.");
        }

        try
        {
            var english = HelpCenter.Domain.Common.SupportedLocales.Normalize(locale) == HelpCenter.Domain.Common.SupportedLocales.English;
            var body = $"""
                <html>
                <body style="font-family: Arial, sans-serif; background-color: #f8fafc; padding: 20px; color: #1e293b;">
                  <div style="max-width: 480px; margin: 0 auto; background-color: #ffffff; border-radius: 16px; padding: 32px; border: 1px solid #e2e8f0;">
                    <h2 style="color: #4f46e5; margin-top: 0; font-size: 20px;">{(english ? "HelpCenter password reset code" : "HelpCenter Şifre Sıfırlama Kodu")}</h2>
                    <p>{(english ? "Hello" : "Merhaba")} <strong>{userName}</strong>,</p>
                    <p>{(english ? "You requested a password reset. Use this 6-digit verification code:" : "Hesabınızın şifresini sıfırlama talebinde bulundunuz. Aşağıdaki 6 haneli doğrulama kodunu kullanabilirsiniz:")}</p>
                    <div style="text-align: center; margin: 28px 0;">
                      <span style="font-size: 32px; font-weight: 800; tracking: 6px; letter-spacing: 6px; color: #4f46e5; background-color: #eef2ff; padding: 12px 24px; border-radius: 12px; display: inline-block;">
                        {code}
                      </span>
                    </div>
                    <p style="font-size: 13px; color: #64748b;">{(english ? "This code is valid for <strong>15 minutes</strong>. If you did not request it, you can ignore this email." : "Bu kod <strong>15 dakika</strong> boyunca geçerlidir. Eğer bu talebi siz yapmadıysanız lütfen bu e-postayı dikkate almayın.")}</p>
                    <hr style="border: none; border-top: 1px solid #f1f5f9; margin: 24px 0;" />
                    <p style="font-size: 11px; color: #94a3b8; text-align: center;">{(english ? "HelpCenter Security Team" : "HelpCenter Güvenlik Ekibi")}</p>
                  </div>
                </body>
                </html>
                """;

            using var message = new MailMessage
            {
                From = new MailAddress(_smtp.SenderEmail, _smtp.DisplayName),
                Subject = english ? "[HelpCenter] Password reset verification code" : "[HelpCenter] Şifre Sıfırlama Doğrulama Kodu",
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
                EnableSsl = _smtp.EnableSsl
            };

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Password reset OTP email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}.", toEmail);
            throw;
        }
    }

    private sealed class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public string DisplayName { get; set; } = "HelpCenter";
        public bool EnableSsl { get; set; } = true;

        public string SenderEmail =>
            !string.IsNullOrWhiteSpace(From) ? From :
            (!string.IsNullOrWhiteSpace(FromAddress) ? FromAddress : Username);
    }
}
