using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391Web.Application.IService
{
    public interface IEmailService
    {
        Task<bool> SendEmployeeAaccountEmail(string to, string fullName, string password);
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendEmailFromTemplate(string to, string templateName, Dictionary<string, string> placeholders);
        Task<bool> SendVerifyEmail(string to, string verifyLink);
        Task<bool> SendResetPassword(string email, string resetLink);
        Task<bool> SendContractEmailAsync(
        string to,
        string fullName,
        string contractSubject,
        string downloadLink,
        byte[]? pdfBytes = null,
        string? pdfFileName = null,
        string? companyName = null,
        string? supportEmail = null);
    }
}
