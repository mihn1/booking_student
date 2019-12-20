using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace B2S.Core.Interfaces
{
    public interface IInvoicePaymentService
    {
        Task GeneratePaymentsByBookingIds(List<string> bookingId);
        Task GenerateAllPayments();
        Task GenerateInvoices(DateTime invoiceDate, string agentId, string vendorId = null);
    }
}
