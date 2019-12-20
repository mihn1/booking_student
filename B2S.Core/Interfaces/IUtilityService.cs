using B2S.Core.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace B2S.Core.Interfaces
{
    public interface IUtilityService
    {
        Task SendEmailBySendGridAsync(string apiKey, string fromEmail, string fromFullName, string subject, string message, string email);

        Task<bool> IsAccountActivatedAsync(string email, UserManager<User> userManager);

        Task SendEmailByGmailAsync(string fromEmail,
            string fromFullName,
            string subject,
            string messageBody,
            string toEmail,
            string toFullName,
            string smtpUser,
            string smtpPassword,
            string smtpHost,
            int smtpPort,
            bool smtpSSL);

        Task SendEmailByMailKitAsync(string fromEmail,
            string fromFullName,
            string subject,
            string messageBody,
            string toEmail,
            string toFullName,
            string smtpUser,
            string smtpPassword,
            string smtpHost,
            int smtpPort,
            bool smtpSSL);

        Task SendEmailByMailKitWithDocAsync(string fromEmail,
            string fromFullName,
            string subject,
            string messageBody,
            string toEmail,
            string toFullName,
            string smtpUser,
            string smtpPassword,
            string smtpHost,
            int smtpPort,
            bool smtpSSL,
            string documentName,
            byte[] documentBytes);

        Task SendEmailByMailKitWithPropertyDocsAsync(string fromEmail,
            string fromFullName,
            string subject,
            string messageBody,
            string toEmail,
            string toFullName,
            string smtpUser,
            string smtpPassword,
            string smtpHost,
            int smtpPort,
            bool smtpSSL,
            List<PropertyDocument> documents);



    Task<string> UploadFile(List<IFormFile> files, IHostingEnvironment env);

        Task UpdateRoles(User appUser, User currentUserLogin);

        Task UpdateUserRoles(User user, string userProfileId);

        Task CreateDefaultSuperAdmin();

        Task InitApp();

        Task SetupUserVendor();
        Task SetupUserAgent();
        Task SetUpVendors();
        Task SetUpAgents();
        Task SetupInvoiceSettings();
        Task SetupPropertyData();
        Task SetupExpense();
        Task CreateNewRoles();
    }
}
