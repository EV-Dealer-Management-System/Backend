using Microsoft.Extensions.Configuration;
using SWP391Web.Application.IService;
using SWP391Web.Infrastructure.IRepository;
using System.Net;
using System.Net.Mail;

namespace SWP391Web.Application.Service
{
    public class EmailService : IEmailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public EmailService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            var isSuccess = false;
            try
            {
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var userName = _configuration["EmailSettings:UserName"];
                var password = _configuration["EmailSettings:Password"];
                var smtpHost = _configuration["EmailSettings:Host"];
                var smtpPort = int.Parse(_configuration["EmailSettings:Port"]);


                if (fromEmail is null || userName is null || password is null || smtpHost is null)
                {
                    throw new ArgumentNullException("Email configuration is missing");
                }

                var message = new MailMessage(fromEmail, to, subject, body);
                message.IsBodyHtml = true;

                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(userName, password),
                    EnableSsl = true
                };

                await smtpClient.SendMailAsync(message);

                isSuccess = true;
                return isSuccess;
            }
            catch (Exception ex)
            {
                return isSuccess;
            }
        }

        public async Task<bool> SendEmailFromTemplate(string to, string templateName, Dictionary<string, string> placeholders)
        {
            var isSuccess = false;
            try
            {
                var template = await _unitOfWork.EmailTemplateRepository.GetByNameAsync(templateName);
                if (template is null)
                {
                    throw new ArgumentNullException($"Email template: {templateName} not found");
                }
                var subject = template.SubjectLine;
                var body = template.BodyContent;
                foreach (var placeholder in placeholders)
                {
                    body = body.Replace($"{placeholder.Key}", placeholder.Value);
                }
                isSuccess = await SendEmailAsync(to, subject, body);
                return isSuccess;
            }
            catch (Exception ex)
            {
                return isSuccess;
            }
        }

        public async Task<bool> SendResetPassword(string email, string resetLink)
        {
            return await SendEmailFromTemplate(email, "ResetPassword", new Dictionary<string, string>
            {
                { "{ResetLink}", resetLink }
            });
        }

        public async Task<bool> SendVerifyEmail(string to, string verifyLink)
        {
            return await SendEmailFromTemplate(to, "SendVerifyEmail", new Dictionary<string, string>
            {
                { "{Login}", verifyLink }
            });
        }
    }
}
