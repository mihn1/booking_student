using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using B2S.Core.Entities;
using B2S.Infrastructure.Data;
using Microsoft.AspNet.Identity;

namespace B2S.Web.Controllers
{
    public class InvoiceSettingController : Controller
    {
        private readonly AppDbContext _context;

        public InvoiceSettingController(AppDbContext context)
        {
            _context = context;
        }

        // GET: InvoiceSetting
        public async Task<IActionResult> Index()
        {
            return View(await _context.InvoiceSetting.ToListAsync());
        }

        // GET: InvoiceSetting/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoiceSetting = await _context.InvoiceSetting
                .Include(m => m.EmailTemplate)
                .FirstOrDefaultAsync(m => m.InvoiceSettingId == id);
            if (invoiceSetting == null)
            {
                return NotFound();
            }

            return View(invoiceSetting);
        }

        // GET: InvoiceSetting/Create
        public IActionResult Create()
        {
            ViewData["EmailTemplateId"] = new SelectList(_context.EmailTemplate.Where(e => e.IsPublished), "EmailTemplateId", "TemplatName");
            return View();
        }

        // POST: InvoiceSetting/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InvoiceSettingId,Name,Description,BusinessName,BusinessNumber,BusinessLogoUrl,ContactName,Phone,Email,Address,City,Province,Postcode,Country,PaymentTerm,Currency,RecurringPeriod,RecurringType,IsActive,BankName,AccountName,BSBNumber,AccountNumber,TaxCode,TaxPercentage,PaymentMethod,PaymentNote,EmailTemplateId,CreatedAt,CreatedBy,ModifiedAt,ModifiedBy,DeletedAt,DeletedBy,IsDeleted")] InvoiceSetting invoiceSetting)
        {
            if (ModelState.IsValid)
            {
                invoiceSetting.CreatedAt = DateTime.UtcNow;
                invoiceSetting.CreatedBy = User.Identity.GetUserId();
                _context.Add(invoiceSetting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmailTemplateId"] = new SelectList(_context.EmailTemplate.Where(e => e.IsPublished), "EmailTemplateId", "TemplatName", invoiceSetting.EmailTemplateId);
            return View(invoiceSetting);
        }

        // GET: InvoiceSetting/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoiceSetting = await _context.InvoiceSetting.FindAsync(id);
            if (invoiceSetting == null)
            {
                return NotFound();
            }
            ViewData["EmailTemplateId"] = new SelectList(_context.EmailTemplate.Where(e => e.IsPublished), "EmailTemplateId", "TemplatName", invoiceSetting.EmailTemplateId);
            return View(invoiceSetting);
        }

        // POST: InvoiceSetting/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("InvoiceSettingId,Name,Description,BusinessName,BusinessNumber,BusinessLogoUrl,ContactName,Phone,Email,Address,City,Province,Postcode,Country,PaymentTerm,Currency,RecurringPeriod,RecurringType,IsActive,BankName,AccountName,BSBNumber,AccountNumber,TaxCode,TaxPercentage,PaymentMethod,PaymentNote,EmailTemplateId,CreatedAt,CreatedBy,ModifiedAt,ModifiedBy,DeletedAt,DeletedBy,IsDeleted")] InvoiceSetting invoiceSetting)
        {
            if (id != invoiceSetting.InvoiceSettingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    invoiceSetting.ModifiedAt = DateTime.UtcNow;
                    invoiceSetting.ModifiedBy = User.Identity.GetUserId();
                    _context.Update(invoiceSetting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvoiceSettingExists(invoiceSetting.InvoiceSettingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                ViewData["EmailTemplateId"] = new SelectList(_context.EmailTemplate.Where(e => e.IsPublished), "EmailTemplateId", "TemplatName", invoiceSetting.EmailTemplateId);
                return RedirectToAction(nameof(Index));
            }
            return View(invoiceSetting);
        }

        // GET: InvoiceSetting/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoiceSetting = await _context.InvoiceSetting
                .Include(m => m.EmailTemplate)
                .FirstOrDefaultAsync(m => m.InvoiceSettingId == id);
            if (invoiceSetting == null)
            {
                return NotFound();
            }

            return View(invoiceSetting);
        }

        // POST: InvoiceSetting/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var invoiceSetting = await _context.InvoiceSetting.FindAsync(id);
            _context.InvoiceSetting.Remove(invoiceSetting);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetEmailTemplate(string emailTemplateId)
        {
            var emailTemplate = await _context.EmailTemplate.FindAsync(emailTemplateId);          
            return Json(emailTemplate?.BodyHTML);
        }

        private bool InvoiceSettingExists(string id)
        {
            return _context.InvoiceSetting.Any(e => e.InvoiceSettingId == id);
        }
    }
}
