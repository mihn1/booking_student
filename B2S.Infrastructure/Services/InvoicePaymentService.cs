using B2S.Core.Entities;
using B2S.Core.Interfaces;
using B2S.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2S.Infrastructure.Services
{
    public class InvoicePaymentService : IInvoicePaymentService
    {
        private readonly AppDbContext _appDBContext;
        public InvoicePaymentService(AppDbContext appDBContext)
        {
            _appDBContext = appDBContext;
        }
        public async Task GenerateAllPayments()
        {
            //Get all bookings that has Status = CheckIn to process payment. This only apply for agent.
            //One Booking can has multiple payments. The payment based on Booking Period Recurring Invoice Type, Recurring Period
            var bookings = await GetAllBookings();

            foreach (var booking in bookings)
            {
                var payments = CreatePayments(booking);
                if (payments.Count > 0)
                {
                    _appDBContext.Payment.AddRange(payments);
                    await _appDBContext.SaveChangesAsync();
                }
            }
           

        }

        public async Task GeneratePaymentsByBookingIds(List<string> bookingIds)
        {
            foreach(var bookingId in bookingIds)
            {
                var booking = await _appDBContext.Booking
                    .Include(b => b.Bed.Room.Building.Property.Vendor)
                    .Include(b => b.Bed.Room.RoomType)
                    .Include(b => b.Agent.InvoiceSetting)
                    .Include(b => b.Customer)
                    .FirstOrDefaultAsync(u => u.BookingId == bookingId);
                if (booking != null && booking?.Status != Core.Enums.BookingStatus.Paid)
                {
                    var payments = CreatePayments(booking);
                    if (payments.Count > 0)
                    {
                        _appDBContext.Payment.AddRange(payments);
                    }
                    booking.Status = Core.Enums.BookingStatus.Paid;
                    _appDBContext.Update(booking);
                }
            }
            await _appDBContext.SaveChangesAsync();
        }

        public async Task GenerateInvoices(DateTime invoiceDate, string agentId, string vendorId = null)
        {
            var agent = await _appDBContext.Agent.FindAsync(agentId);
            if (agent == null) return;
            var invoiceSettings = await _appDBContext.InvoiceSetting.FirstOrDefaultAsync(a => a.InvoiceSettingId == agent.InvoiceSettingId);
            if (invoiceSettings == null) return;

            decimal subTotal = 0, taxAmount = 0, invoiceAmount = 0;

            List<Payment> payments = new List<Payment>();

            //Get all payment
            var paymentQueries = _appDBContext.Payment
                .Include(p => p.Booking.Bed.Room.Building.Property)
                .Where(a => a.PaymentDate == invoiceDate && a.IsCreateInvoice == false && a.AgentId == agentId);

            if (!string.IsNullOrEmpty(vendorId))
            {
                paymentQueries = paymentQueries.Where(p => p.Booking.Bed.Room.Building.Property.VendorId == vendorId);
            }

            payments = await paymentQueries.ToListAsync();

            if (payments.Count() == 0)
                return;

            Invoice invoice = new Invoice
            {
                AgentId = agent.AgentId,
                CreatedAt = DateTime.Today,
                DueDate = invoiceDate.AddDays(invoiceSettings.PaymentTerm),
                InvoiceDate = invoiceDate,
                PaymentStatus = Core.Enums.PaymentStatus.Pending,
                InvoiceNumber = await GetInvoiceNumber(invoiceSettings),
                SubTotal = subTotal,
                TaxAmount = taxAmount,
                InvoiceAmount = invoiceAmount
            };

            invoice.InvoiceItems = new List<InvoiceItem>();
            foreach (var payment in payments)
            {
                InvoiceItem invoiceItem = new InvoiceItem
                {
                    InvoiceId = invoice.InvoiceId,
                    Amount = payment.Amount,
                    Price = payment.Price,
                    Quantity = payment.Quantity,
                    Item = payment.Payee,
                    Description = payment.Description
                };
                subTotal += payment.Amount;
                invoice.InvoiceItems.Add(invoiceItem);
                UpdatePayment(payment, invoice.InvoiceId);
            }
            _appDBContext.Payment.UpdateRange(payments);

            taxAmount = (subTotal * invoiceSettings.TaxPercentage) / 100;
            invoiceAmount = subTotal + taxAmount;
            invoice.TaxAmount = taxAmount;
            invoice.SubTotal = subTotal;
            invoice.InvoiceAmount = invoiceAmount;
            invoice.BookingId = payments.FirstOrDefault()?.BookingId;

            _appDBContext.Invoice.Add(invoice);
            await _appDBContext.SaveChangesAsync();

        }

        private async Task <List<Booking>> GetAllBookings()
        {
            var bookings = await _appDBContext.Booking
                       .Include(b => b.Bed)
                       .ThenInclude(r => r.Room)
                       .ThenInclude(rt => rt.RoomType)
                       .Include(a => a.Agent)
                       .ThenInclude(i => i.InvoiceSetting)
                       .Where(b => b.Status == Core.Enums.BookingStatus.CheckIn && !string.IsNullOrEmpty(b.AgentId))
                       .ToListAsync();
            return bookings;
        }

        private List<Payment> CreatePayments(Booking booking)
        {
            List<Payment> payments = new List<Payment>();

            if (booking.Agent?.InvoiceSetting != null)
            {
                if (booking.Agent.InvoiceSetting.RecurringType == Core.Enums.RecurringInvoiceType.Weeks)
                {
                    int paymentDays = booking.Agent.InvoiceSetting.RecurringPeriod * 7;
                    decimal quantity = booking.Agent.InvoiceSetting.RecurringPeriod;
                    decimal periods = decimal.Divide((booking.BookingTo - booking.BookingFrom).Days, 7);

                    if (periods <= 2)
                    {
                        quantity = periods;
                        var amount = booking.Price * quantity;
                        string description = string.Format("Payment from {0} to {1}", booking.BookingFrom.ToShortDateString(), booking.BookingTo.ToShortDateString());
                        var payment = GetPayment(booking, booking.BookingFrom, booking.Price, quantity, amount, description);
                        payments.Add(payment);
                    }
                    else
                    {
                        decimal tmp = decimal.Divide((booking.BookingTo - booking.BookingFrom).Days, (7 * booking.Agent.InvoiceSetting.RecurringPeriod));
                        int billingTimes = (int)decimal.Truncate(tmp) + 1;
                        decimal remainder = (tmp - decimal.Truncate(tmp)) * booking.Agent.InvoiceSetting.RecurringPeriod;
                        //decimal intPart = billingTimes - remainder;
                        DateTime paymentDate = booking.BookingFrom;
                        DateTime lastPaymentDate = booking.BookingFrom;

                        for (int i = 1; i <= billingTimes; i++)
                        {
                            if (i == billingTimes && remainder > 0)
                            {
                                quantity = remainder;
                                var amount = booking.Price * quantity;
                                paymentDate = lastPaymentDate.AddDays(paymentDays);
                                string description = string.Format("Payment from {0} to {1}", paymentDate.ToShortDateString(), booking.BookingTo.ToShortDateString());
                                var payment = GetPayment(booking, paymentDate, booking.Price, quantity, amount, description);
                                payments.Add(payment);
                            }
                            else
                            {

                                string description = string.Empty;
                                var amount = booking.Price * quantity;
                                if (i == 1)
                                {
                                    description = string.Format("Payment from {0} to {1}", booking.BookingFrom.ToShortDateString(), booking.BookingFrom.AddDays(paymentDays).ToShortDateString());
                                }
                                else
                                {
                                    paymentDate = lastPaymentDate.AddDays(paymentDays);
                                    lastPaymentDate = paymentDate;
                                    description = string.Format("Payment from {0} to {1}", paymentDate.ToShortDateString(), paymentDate.AddDays(paymentDays).ToShortDateString());
                                }

                                var payment = GetPayment(booking, paymentDate, booking.Price, quantity, amount, description);
                                payments.Add(payment);
                            }

                        }
                    }

                }

            }

            return payments;
        }
        private Payment GetPayment(Booking booking, DateTime paymentDate, decimal price, decimal quantity, decimal amount, string description)
        {
            Payment payment = new Payment
            {
                Payee = string.Format("{0} {1} - {2} rent", booking.FirstName, booking.LastName, booking.Bed.Room?.RoomType.RoomTypeName),
                AgentId = booking.AgentId,
                PaymentDate = paymentDate,
                CreatedAt = DateTime.Today,
                IsCreateInvoice = false,
                Quantity = quantity,
                Price = price,
                Description = description,
                Amount = amount,
                BookingId = booking.BookingId,
                PaymentMethod = Core.Enums.PaymentMethod.BankTransfer,
                PaymentStatus = Core.Enums.PaymentStatus.Pending,
            };

            return payment;
        }

       
        private async Task <string> GetInvoiceNumber(InvoiceSetting invoiceSetting)
        {
            string invoiceNumber = string.Empty;

            int currentInvoice = Convert.ToInt32(invoiceSetting.CurrentInvoiceNo);
            invoiceNumber = (currentInvoice + 1).ToString();
            invoiceSetting.CurrentInvoiceNo = invoiceNumber;
            _appDBContext.InvoiceSetting.Update(invoiceSetting);
            await _appDBContext.SaveChangesAsync();

            return invoiceNumber;
        }

        private void UpdatePayment(Payment payment, string invoiceId)
        {
            payment.PaymentStatus = Core.Enums.PaymentStatus.Sent;
            payment.IsCreateInvoice = true;

        }
    }
}
