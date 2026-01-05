using Microsoft.AspNetCore.Identity.UI.Services;

namespace IdeaTrack.Services
{
    /// <summary>
    /// A no-op email sender for development purposes.
    /// Replace with actual implementation for production.
    /// </summary>
    public class NoOpEmailSender : IEmailSender
    {
        private readonly ILogger<NoOpEmailSender> _logger;

        public NoOpEmailSender(ILogger<NoOpEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation("Email would be sent to {Email} with subject: {Subject}", email, subject);
            return Task.CompletedTask;
        }
    }
}
