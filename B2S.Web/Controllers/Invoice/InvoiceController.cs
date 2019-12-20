using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using B2S.Core.Entities;
using B2S.Infrastructure.Data;
using B2S.Core.Utilities;
using B2S.Core.Common;
using Microsoft.Extensions.Options;
using B2S.Core.Enums;
using B2S.Core.Interfaces;
using B2S.Web.ViewModels.Invoice;
using Microsoft.AspNet.Identity;

namespace B2S.Web.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly AppDbContext _context;
        private PaginationOptions _paginationOptions;
        private readonly IEmailSender _emailSender;
        private readonly IInvoicePaymentService _invoicePaymentService;

        public InvoiceController(AppDbContext context, IOptions<PaginationOptions> paginationOptions, IEmailSender emailSender, IInvoicePaymentService invoicePaymentService)
        {
            _context = context;
            _paginationOptions = paginationOptions.Value;
            _emailSender = emailSender;
            _invoicePaymentService = invoicePaymentService;
        }

        // GET: Invoice
        public async Task<IActionResult> Index(string search, PaymentStatus paymentStatus, int? page)
        {
            ViewData["CurrentSearch"] = search;
            ViewData["CurrentPaymentStatus"] = paymentStatus;

            var paymentStatuses = from PaymentStatus status in Enum.GetValues(typeof(PaymentStatus))
                                               select new { value = status, text = status.ToString() };
            ViewData["PaymentStatuses"] = new SelectList(paymentStatuses, "value", "text", paymentStatus);

            var user = await _context.User.FindAsync(User.Identity.GetUserId());
            var accountId = (await _context.UserAccount.FirstOrDefaultAsync(u => u.UserId == user.Id))?.AccountId;
            bool isAdmin = user.IsSuperAdmin || user.UserType == UserType.Admin;

            var invoices = _context.Invoice.Include(c => c.Booking.Bed.Room.Property).Include(c => c.Agent).Include(c => c.Customer).Select(c => c);

            if (!isAdmin && user.UserType == UserType.Vendor)
            {
                invoices = invoices.Where(i => i.Booking.Bed.Room.Property.VendorId == accountId);
            }
            else if (!isAdmin && user.UserType == UserType.Customer)
            {
                invoices = invoices.Where(i => i.CustomerId == accountId);
            }
            else if (!isAdmin && user.UserType == UserType.Agent)
            {
                invoices = invoices.Where(i => i.AgentId == accountId);
            }

            if (!string.IsNullOrEmpty(search))
            {
                invoices = invoices.Where(l => l.InvoiceNumber.ToUpper().Contains(search.ToUpper()) || l.InvoiceDate.ToShortDateString().Contains(search));
            }
            if (paymentStatus != 0)
            {
                invoices = invoices.Where(i => i.PaymentStatus == paymentStatus);
            }

            int pageSize = _paginationOptions.PageSize;

            return View(await PaginatedList<B2S.Core.Entities.Invoice>.CreateAsync(invoices, page ?? 1, pageSize));           
        }

        // GET: Invoice/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoice
                .Include(i => i.Agent)
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(m => m.InvoiceId == id);
            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // GET: Invoice/Create
        public IActionResult Create()
        {          
            ViewData["AgentId"] = new SelectList(_context.Agent, "AgentId", "BusinessName");
            //ViewData["CustomerId"] = new SelectList(_context.Customer.Select(c => new { value = c.CustomerId, text = c.FirstName + " " + c.LastName }), "value", "text");
            return View();
        }

        // POST: Invoice/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InvoiceId,InvoiceNumber,InvoiceDate,DueDate,TaxAmount,DiscountAmount,SubTotal,InvoiceAmount,PaymentMethod,PaymentStatus,AgentId,CustomerId,CreatedAt,CreatedBy,ModifiedAt,ModifiedBy,DeletedAt,DeletedBy,IsDeleted")] Core.Entities.Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                invoice.CreatedBy = User.Identity.GetUserId();
                _context.Add(invoice);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
           
            ViewData["AgentId"] = new SelectList(_context.Agent, "AgentId", "BusinessName", invoice.AgentId);
            //ViewData["CustomerId"] = new SelectList(_context.Customer.Select(c => new { value = c.CustomerId, text = c.FirstName + " " + c.LastName }), "value", "text", invoice.CustomerId);
            return View(invoice);
        }

        // GET: Invoice/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoice.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }
            ViewData["AgentId"] = new SelectList(_context.Agent, "AgentId", "BusinessName", invoice.AgentId);
            ViewData["CustomerId"] = new SelectList(_context.Customer.Select(c => new { value = c.CustomerId, text = c.FirstName + " " + c.LastName}), "value", "text", invoice.CustomerId);
            return View(invoice);
        }

        // POST: Invoice/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("InvoiceId,InvoiceNumber,InvoiceDate,DueDate,TaxAmount,DiscountAmount,SubTotal,InvoiceAmount,PaymentMethod,PaymentStatus,AgentId,CustomerId,CreatedAt,CreatedBy,ModifiedAt,ModifiedBy,DeletedAt,DeletedBy,IsDeleted")] Core.Entities.Invoice invoice)
        {
            if (id != invoice.InvoiceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invoice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvoiceExists(invoice.InvoiceId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AgentId"] = new SelectList(_context.Agent, "AgentId", "BusinessName", invoice.AgentId);
            ViewData["CustomerId"] = new SelectList(_context.Customer.Select(c => new { value = c.CustomerId, text = c.FirstName + " " + c.LastName }), "value", "text", invoice.CustomerId);
            return View(invoice);
        }

        // GET: Invoice/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoice
                .Include(i => i.Agent)
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(m => m.InvoiceId == id);
            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // POST: Invoice/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var invoice = await _context.Invoice.FindAsync(id);
            _context.Invoice.Remove(invoice);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GenerateInvoice(DateTime? currentFrom, DateTime? currentTo, string agentId, int? page)
        {
            currentFrom = currentFrom ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            currentTo = currentTo ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
            ViewData["currentFrom"] = currentFrom;
            ViewData["currentTo"] = currentTo;
            ViewData["currentAgentId"] = agentId;

            var payments = _context.Payment
                .Include(p => p.Booking.Bed.Room.Building.Property)
                .Where(p => p.PaymentDate >= currentFrom && p.PaymentDate <= currentTo && p.IsCreateInvoice == false);

            if (!string.IsNullOrEmpty(agentId))
            {
                payments = payments.Where(p => p.AgentId == agentId);
            }
            var user = await _context.User.FindAsync(User.Identity.GetUserId());
            var accountId = (await _context.UserAccount.FirstOrDefaultAsync(u => u.UserId == user.Id))?.AccountId;
            if (!user.IsSuperAdmin || user.UserType != UserType.Admin)
            {
                if (user.UserType == UserType.Agent)
                {
                    payments = payments.Where(b => b.AgentId == accountId);
                }
                if (user.UserType == UserType.Vendor)
                {
                    payments = payments.Where(b => b.Booking.Bed.Room.Building.Property.VendorId == accountId);
                }
            }

            ViewData["AgentId"] = new SelectList(_context.Agent, "AgentId", "BusinessName", agentId);

            return View(await PaginatedList<Payment>.CreateAsync(payments, page ?? 1, _paginationOptions.PageSize));
        }

        [HttpPost]
        public async Task<IActionResult> GenerateInvoice(DateTime currentFrom, DateTime currentTo, string agentId)
        {
            if (currentTo < currentFrom)
            {
                return Json(new { success = false, message = "Start date must less than end date." });
            }
           
            try
            {
                var payments = _context.Payment
                .Include(p => p.Booking.Bed.Room.Building.Property)
                .Where(p => p.PaymentDate >= currentFrom && p.PaymentDate <= currentTo && p.IsCreateInvoice == false);

                List<string> agentIds = new List<string>();                
                
                var user = await _context.User.FindAsync(User.Identity.GetUserId());
                if (!user.IsSuperAdmin && user.UserType != UserType.Admin)
                {
                    //generate invoice by vendor
                   if (user.UserType == UserType.Vendor)
                    {
                        
                        var vendorId = (await _context.UserAccount.FirstOrDefaultAsync(u => u.UserId == user.Id))?.AccountId;

                        if (!string.IsNullOrEmpty(agentId))
                        {
                            for (int i = 0; i <= (currentTo - currentFrom).Days; i++)
                            {
                                DateTime currentDate = currentFrom.AddDays(i);
                                await _invoicePaymentService.GenerateInvoices(currentDate, agentId, vendorId);
                            }
                        }
                        else
                        {
                            agentIds = await payments.Where(p => p.Booking.Bed.Room.Building.Property.VendorId == vendorId).Select(p => p.AgentId).Distinct().ToListAsync();
                            foreach (var id in agentIds)
                            {
                                for (int i = 0; i <= (currentTo - currentFrom).Days; i++)
                                {
                                    DateTime currentDate = currentFrom.AddDays(i);
                                    await _invoicePaymentService.GenerateInvoices(currentDate, id, vendorId);
                                }
                            }
                        }

                        for (int i = 0; i <= (currentTo - currentFrom).Days; i++)
                        {
                            DateTime currentDate = currentFrom.AddDays(i);
                            await _invoicePaymentService.GenerateInvoices(currentDate, agentId, vendorId);
                        }
                    }
                }
                //generate all invoice by admin
                else
                {
                    if (!string.IsNullOrEmpty(agentId))
                    {
                        for (int i = 0; i <= (currentTo - currentFrom).Days; i++)
                        {
                            DateTime currentDate = currentFrom.AddDays(i);
                            await _invoicePaymentService.GenerateInvoices(currentDate, agentId);
                        }
                    }
                    else
                    {
                        agentIds = await payments.Select(p => p.AgentId).Distinct().ToListAsync();
                        foreach(var id in agentIds)
                        {
                            for (int i = 0; i <= (currentTo - currentFrom).Days; i++)
                            {
                                DateTime currentDate = currentFrom.AddDays(i);
                                await _invoicePaymentService.GenerateInvoices(currentDate, id);
                            }
                        }
                    }
                }

                    
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            return Json(new { success = true, redirectUrl = Url.Action("GenerateInvoice", "Invoice", new { currentFrom, currentTo, agentId }) });
        }

        public async Task<IActionResult> Generate(string id)
        {
            var invoice = await _context.Invoice.Include(i => i.Agent).Include(i => i.Customer).Include(i => i.InvoiceItems).FirstOrDefaultAsync(i => i.InvoiceId == id);
            if (invoice == null)
            {
                return RedirectToAction(nameof(Index));
            }
            string settingId = invoice.Agent?.InvoiceSettingId;
            var setting = await _context.InvoiceSetting.FirstOrDefaultAsync(s => s.IsActive && s.InvoiceSettingId == settingId);
            if (setting == null)
            {
                TempData["ErrorMessage"] = "Error: Set up invoice setting first then send invoice.";
                return RedirectToAction(nameof(Index));
            }
          
            string hostUrl = $"{Request.Scheme}://{Request.Host}";
            InvoiceReport invoiceReport = new InvoiceReport(hostUrl);
            byte[] bytes = invoiceReport.CreateReport(invoice, setting);
            Response.Headers.Add("Content-Disposition", "inline;filename=Invoice#" + invoice.InvoiceNumber);
            return File(bytes, "application/pdf");
        }

        public async Task<IActionResult> Send(string id)
        {

            var invoice = await _context.Invoice.Include(i => i.Agent).Include(i => i.Customer).Include(i => i.InvoiceItems).FirstOrDefaultAsync(i => i.InvoiceId == id);
            if (invoice == null)
            {
                return NotFound();
            }
            string settingId = invoice.Agent?.InvoiceSettingId;
            var setting = await _context.InvoiceSetting.Include(e => e.EmailTemplate).FirstOrDefaultAsync(s => s.IsActive && s.InvoiceSettingId == settingId);
            if (setting == null)
            {
                TempData["ErrorMessage"] = "Error: Set up invoice setting first then send invoice.";
                return RedirectToAction(nameof(Index));
            }
            SendInvoiceViewModel model = new SendInvoiceViewModel
            {
                InvoiceId = id,
                Subject = string.Format(setting.EmailTemplate?.Subject, invoice.InvoiceNumber),
                BodyHTML = string.Format(setting.EmailTemplate?.BodyHTML, invoice.Agent?.ContactName, invoice.InvoiceNumber),
                SendToEmail = invoice.Agent?.Email,
                InvoiceNumber = invoice.InvoiceNumber
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(SendInvoiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = string.Join("; ", ModelState.Values.SelectMany(m => m.Errors).Select(m => m.ErrorMessage)) });
            }

            var invoice = await _context.Invoice.Include(i => i.Agent).Include(i => i.Customer).Include(i => i.InvoiceItems).FirstOrDefaultAsync(i => i.InvoiceId == model.InvoiceId);
            if (invoice == null)
            {
                return Json(new { success = false, message = "Invoice not found." });
            }
            string settingId = invoice.Agent?.InvoiceSettingId;
            var setting = await _context.InvoiceSetting.Include(e => e.EmailTemplate).FirstOrDefaultAsync(s => s.IsActive && s.InvoiceSettingId == settingId);
            if (setting == null)
            {
                return Json(new { success = false, message = "Invoice setting not found." });
            }

            string hostUrl = $"{Request.Scheme}://{Request.Host}";
            InvoiceReport invoiceReport = new InvoiceReport(hostUrl);
            byte[] bytes = invoiceReport.CreateReport(invoice, setting);
            try
            {
                await _emailSender.SendEmailWithDocAsync(model.SendToEmail, model.Subject ?? "", model.BodyHTML, invoice.InvoiceNumber + ".pdf", bytes);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message ?? "Email can not be sent." });
            }

            return Json(new { success = true, redirectUrl = "/Invoice/Index" });
        }

        public async Task<IActionResult> CreateItem(string id, string invoiceId)
        {
            var item = await _context.InvoiceItem.FindAsync(id);
            if (item != null)
            {
                return View(item);
            }

            InvoiceItem newItem = new InvoiceItem
            {
                InvoiceId = invoiceId
            };
            return View(newItem);
        }      

        private bool InvoiceExists(string id)
        {
            return _context.Invoice.Any(e => e.InvoiceId == id);
        }
    }
}
