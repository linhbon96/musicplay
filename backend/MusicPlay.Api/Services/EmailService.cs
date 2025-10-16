using System.Net;
using Microsoft.Extensions.Options;
using MusicPlay.Api.Configuration;
using MusicPlay.Api.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MusicPlay.Api.Services;

public class EmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;
    private readonly Lazy<SendGridClient?> _client;

    public EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _client = new Lazy<SendGridClient?>(() =>
        {
            if (string.IsNullOrWhiteSpace(_options.SendGrid.ApiKey))
            {
                _logger.LogWarning("SendGrid API key is not configured; confirmation emails will not be sent.");
                return null;
            }

            return new SendGridClient(_options.SendGrid.ApiKey);
        });
    }

    public string BuildConfirmationLink(ApplicationUser user)
    {
        var baseUrl = string.IsNullOrWhiteSpace(_options.FrontendBaseUrl)
            ? "http://localhost:5173"
            : _options.FrontendBaseUrl.TrimEnd('/');

        var token = WebUtility.UrlEncode(user.EmailConfirmationToken ?? string.Empty);
        var email = WebUtility.UrlEncode(user.Email);
        return $"{baseUrl}/confirm-email?token={token}&email={email}";
    }

    public async Task SendEmailConfirmationAsync(ApplicationUser user, string confirmationLink, CancellationToken cancellationToken)
    {
        var client = _client.Value;
        if (client is null)
        {
            return;
        }

        var message = new SendGridMessage
        {
            Subject = "Xác nhận email MusicPlay",
            HtmlContent = $@"<p>Chào {WebUtility.HtmlEncode(user.DisplayName ?? user.Username)},</p>" +
                          "<p>Cảm ơn bạn đã tham gia MusicPlay. Nhấn vào nút dưới đây để xác nhận địa chỉ email và bắt đầu chia sẻ âm nhạc của bạn.</p>" +
                          $"<p><a href='{confirmationLink}' style='background:#8a4fff;color:#fff;padding:12px 20px;border-radius:6px;text-decoration:none;'>Xác nhận email</a></p>" +
                          "<p>Nếu bạn không tạo tài khoản MusicPlay, vui lòng bỏ qua email này.</p>"
        };

        message.SetFrom(new EmailAddress(_options.SendGrid.SenderEmail, _options.SendGrid.SenderName));
        message.AddTo(new EmailAddress(user.Email, user.DisplayName ?? user.Username));

        var response = await client.SendEmailAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Body.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to send confirmation email to {Email}. Status: {Status}. Body: {Body}", user.Email, response.StatusCode, body);
        }
    }
}
