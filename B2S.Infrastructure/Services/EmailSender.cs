using B2S.Core.Common;
using B2S.Core.Entities;
using B2S.Core.Interfaces;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace B2S.Infrastructure.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        //dependency injection
        private SendGridOptions _sendGridOptions { get; }
        private IUtilityService _utilityService { get; }
        private SmtpOptions _smtpOptions { get; }

        public EmailSender(IOptions<SendGridOptions> sendGridOptions,
            IUtilityService utilityService,
            IOptions<SmtpOptions> smtpOptions)
        {
            _sendGridOptions = sendGridOptions.Value;
            _utilityService = utilityService;
            _smtpOptions = smtpOptions.Value;
        }


        public Task SendEmailAsync(string email, string subject, string message)
        {
            //send email using sendgrid via netcoreService
            //_utilityService.SendEmailBySendGridAsync(_sendGridOptions.SendGridKey,
            //    _sendGridOptions.FromEmail,
            //    _sendGridOptions.FromFullName,
            //    subject,
            //    message,
            //    email).Wait();

            //send email using smtp via dotnetdesk. uncomment to use it

            //_utilityService.SendEmailByGmailAsync(_smtpOptions.fromEmail,
            //    _smtpOptions.fromFullName,
            //    subject,
            //    message,
            //    email,
            //    email,
            //    _smtpOptions.smtpUserName,
            //    _smtpOptions.smtpPassword,
            //    _smtpOptions.smtpHost,
            //    _smtpOptions.smtpPort,
            //    _smtpOptions.smtpSSL).Wait();
            if (_smtpOptions.isActive)
            {
                _utilityService.SendEmailByMailKitAsync(_smtpOptions.fromEmail,
              _smtpOptions.fromFullName,
              subject ?? "",
              message ?? "",
              email,
              "",
              _smtpOptions.smtpUserName,
              _smtpOptions.smtpPassword,
              _smtpOptions.smtpHost,
              _smtpOptions.smtpPort,
              _smtpOptions.smtpSSL);
            }           

            return Task.CompletedTask;
        }

        public Task SendEmailWithDocAsync(string email, string subject, string message, string documentName, byte[] documentBytes)
        {          
            if (_smtpOptions.isActive)
            {
                _utilityService.SendEmailByMailKitWithDocAsync(_smtpOptions.fromEmail,
              _smtpOptions.fromFullName,
              subject,
              message,
              email,
              "",
              _smtpOptions.smtpUserName,
              _smtpOptions.smtpPassword,
              _smtpOptions.smtpHost,
              _smtpOptions.smtpPort,
              _smtpOptions.smtpSSL,
              documentName,
              documentBytes);
            }         

            return Task.CompletedTask;
        }

        public Task SendEmailWithPropertyDocsAsync(string email, string subject, string message, List<PropertyDocument> documents)
        {
            if (_smtpOptions.isActive)
            {
                _utilityService.SendEmailByMailKitWithPropertyDocsAsync(_smtpOptions.fromEmail,
               _smtpOptions.fromFullName,
               subject,
               message,
               email,
               "",
               _smtpOptions.smtpUserName,
               _smtpOptions.smtpPassword,
               _smtpOptions.smtpHost,
               _smtpOptions.smtpPort,
               _smtpOptions.smtpSSL,
                documents);
            }

            return Task.CompletedTask;
        }
    }
}
