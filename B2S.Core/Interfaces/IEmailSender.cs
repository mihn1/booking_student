using B2S.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace B2S.Core.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);

        Task SendEmailWithDocAsync(string email, string subject, string message, string documentName, byte[] documentBytes);

        Task SendEmailWithPropertyDocsAsync(string email, string subject, string message, List<PropertyDocument> documents);
    }
}
