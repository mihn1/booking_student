using B2S.Core.Entities;
using B2S.Core.Interfaces;
using B2S.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B2S.InvoiceGenerator.Services
{
    public class InvoiceService
    {
        private readonly INLogger _logger;
        private readonly IRepository<Booking> _bookingRepo;
        private readonly IRepository<InvoiceSetting> _invSettingRepo;
        private readonly IRepository<Invoice> _invoiceRepo;
        private readonly IRepository<InvoiceItem> _invoiceItemRepo;
        private readonly AppDbContext _appDBContext;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Agent> _agentRepo;
        private readonly IRepository<Payment> _paymentRepo;
        private readonly User user;

        public InvoiceService(INLogger logger, AppDbContext appDBContext)
        {
            _logger = logger;
            _appDBContext = appDBContext;
            _bookingRepo = new Repository<Booking>(appDBContext);
            _agentRepo = new Repository<Agent>(appDBContext);
            _userRepo = new Repository<User>(appDBContext);
            _paymentRepo = new Repository<Payment>(appDBContext);
            _invoiceRepo = new Repository<Invoice>(appDBContext);
            _invoiceItemRepo = new Repository<InvoiceItem>(appDBContext);
            _invSettingRepo = new Repository<InvoiceSetting>(appDBContext);
            
            //Get user
            user = _userRepo.FindEntity(u => u.Email == "super@admin.com");
            
        }

        public void GenerateAllInvoices()
        {
            var paymentDates = GetAllPaymentDates();

            foreach (var date in paymentDates)
            {
                GenerateInvoices(date, "ELC");
            }
        }

        public void GenerateInvoices(DateTime invoiceDate, string agentName)
        {
            var agent = _agentRepo.FindEntity(a => a.BusinessName == agentName);
            var invoiceSettings = _invSettingRepo.FindEntity(a=>a.InvoiceSettingId == agent.InvoiceSettingId);
            decimal subTotal = 0, taxAmount = 0, invoiceAmount = 0;

            //Get all payment
            var payments = _paymentRepo.FindAllEntity(a => a.PaymentDate  == invoiceDate && a.IsCreateInvoice == false && a.AgentId == agent.AgentId);

            if (payments.Count() < 0)
                return;

            Invoice invoice = new Invoice
            {
                AgentId = agent.AgentId,
                CreatedAt = DateTime.Today,
                CreatedBy = user.Id,

                DueDate = invoiceDate.AddDays(invoiceSettings.PaymentTerm),
                InvoiceDate = invoiceDate,
                PaymentStatus = Core.Enums.PaymentStatus.Pending,
                InvoiceNumber = GetInvoiceNumber(invoiceSettings),
                SubTotal = subTotal,
                TaxAmount = taxAmount,
                InvoiceAmount = invoiceAmount
            };

            invoice.InvoiceItems =  new List<InvoiceItem>();
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

            taxAmount = (subTotal * invoiceSettings.TaxPercentage) / 100;
            invoiceAmount = subTotal + taxAmount;
            invoice.TaxAmount = taxAmount;
            invoice.SubTotal = subTotal;
            invoice.InvoiceAmount = invoiceAmount;
            invoice.BookingId = payments.FirstOrDefault().BookingId;

            _invoiceRepo.AddEntity(invoice);
         
        }

        private void UpdatePayment(Payment payment, string invoiceId)
        {
            payment.PaymentStatus = Core.Enums.PaymentStatus.Sent;
            payment.IsCreateInvoice = true;

        }
        private string GetInvoiceNumber(InvoiceSetting invoiceSetting)
        {
            string invoiceNumber = string.Empty;

            int currentInvoice = Convert.ToInt32(invoiceSetting.CurrentInvoiceNo);
            invoiceNumber = (currentInvoice + 1).ToString();
            invoiceSetting.CurrentInvoiceNo = invoiceNumber;
            _invSettingRepo.UpdateEntity(invoiceSetting);
            return invoiceNumber;
        }
        public void GeneratePayments()
        {
            //Get all bookings
            var bookings = GetAllBookings();
            List<Payment> payments = new List<Payment>();

            foreach (var booking in bookings)
            {
                
                if (booking.Agent.InvoiceSetting != null)
                {
                    if (booking.Agent.InvoiceSetting.RecurringType == Core.Enums.RecurringInvoiceType.Weeks)
                    {
                        int paymentDays = booking.Agent.InvoiceSetting.RecurringPeriod * 7;
                        decimal quantity = booking.Agent.InvoiceSetting.RecurringPeriod;
                        decimal periods = Convert.ToDecimal((booking.BookingTo - booking.BookingFrom).TotalDays / 7);
                        //decimal price = booking.Bed.Room.RoomType.Price;

                        if (periods <= 2)
                        {
                            quantity = periods;
                            var amount = booking.Price * quantity;
                            string description = string.Format("Payment from {0} to {1}",booking.BookingFrom, booking.BookingTo);
                            var payment = CreatePayments(booking, booking.BookingFrom, booking.Price, quantity, amount, description);
                            payments.Add(payment);
                        }
                        else
                        {
                            decimal tmp = Convert.ToDecimal((booking.BookingTo - booking.BookingFrom).TotalDays / (7 * booking.Agent.InvoiceSetting.RecurringPeriod));
                            int billingTimes = Convert.ToInt32(tmp);
                            decimal remainder = billingTimes - Math.Truncate(tmp);
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
                                    string description = string.Format("Payment from {0} to {1}", booking.BookingFrom, booking.BookingTo);
                                    var payment = CreatePayments(booking, paymentDate, booking.Price, quantity, amount, description);
                                }
                                else
                                {
                                    
                                    string description = string.Empty;
                                    var amount = booking.Price * quantity;
                                    if (i == 1)
                                    {
                                        lastPaymentDate = booking.BookingFrom;
                                        description = string.Format("Payment from {0} to {1}", booking.BookingFrom, booking.BookingFrom.AddDays(paymentDays));
                                    }
                                    else
                                    {
                                        paymentDate = lastPaymentDate.AddDays(paymentDays);
                                        lastPaymentDate = paymentDate;
                                        description = string.Format("Payment from {0} to {1}", booking.BookingFrom, booking.BookingFrom.AddDays(paymentDays));
                                    }
                                    
                                    var payment = CreatePayments(booking, paymentDate, booking.Price, quantity, amount, description);
                                    payments.Add(payment);
                                }
                               
                            }
                        }
                        
                    }
                    
                }
            }
            _paymentRepo.SaveAll(payments);
        }

        private Payment CreatePayments(Booking booking, DateTime paymentDate, decimal price, decimal quantity, decimal amount, string description)
        {
            Payment payment = new Payment
            {
                Payee = string.Format("{0} {1} - {2} rent", booking.FirstName, booking.LastName, booking.Bed.Room.RoomType.RoomTypeName),
                AgentId = booking.AgentId,
                PaymentDate = paymentDate,
                CreatedAt = DateTime.Today,
                IsCreateInvoice = false,
                CreatedBy = user.Id,
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
        private List<Booking> GetAllBookings()
        {
            var bookings = _appDBContext.Booking
                       .Include(b => b.Bed)
                       .ThenInclude(r => r.Room)
                       .ThenInclude(rt => rt.RoomType)
                       .Include(a => a.Agent)
                       .ThenInclude(i => i.InvoiceSetting)
                       .Where(b => b.Status == Core.Enums.BookingStatus.Confirmed && !string.IsNullOrEmpty(b.AgentId))
                       .ToList();
            return bookings;
        }
        private List<DateTime> GetAllPaymentDates()
        {
            var paymentDates = (from p in _appDBContext.Payment
                            select p.PaymentDate).Distinct().ToList();

            return paymentDates;
        }
    }
}
