using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using B2S.Core.Entities;
using B2S.Infrastructure.Data;
using B2S.Core.Common;
using Microsoft.Extensions.Options;
using B2S.Core.Utilities;
using B2S.Core.Enums;
using Microsoft.AspNet.Identity;

namespace B2S.Web.Controllers
{
    public class DamageReportController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PaginationOptions _paginationOptions;

        public DamageReportController(AppDbContext context, IOptions<PaginationOptions> paginationOptions)
        {
            _context = context;
            _paginationOptions = paginationOptions.Value;
        }

        // GET: DamageReport
        public async Task<IActionResult> Index(string search, DamageStatus status, string propertyId, int? page)
        {
            ViewData["CurrentSearch"] = search;
            var statuses = from DamageStatus sta in Enum.GetValues(typeof(DamageStatus))
                                  select new { value = sta, text = sta.ToString() };
            ViewData["CurrentStatus"] = (int)status;
            ViewData["CurrentPropertyId"] = propertyId;
            //ViewData["CurrentDate"] = incidentDate;

            ViewData["PropertyId"] = new SelectList(_context.Property, "PropertyId", "PropertyName", propertyId);
            //ViewData["VendorId"] = new SelectList(_context.Property, "PropertyId", "PropertyName", propertyId);
            ViewData["Statuses"] = new SelectList(statuses, "value", "text", status);

            var reports = _context.DamageReport.Select(c => c);

            if (!string.IsNullOrEmpty(search))
            {
                reports = reports.Where(r => r.CustomerName.ToUpper().Contains(search.ToUpper()));
            }
            if (status != 0)
            {
                reports = reports.Where(r => r.DamageStatus == status);
            }
            if (!string.IsNullOrEmpty(propertyId))
            {
                reports = reports.Where(r => r.PropertyId == propertyId);
            }
            

            int pageSize = _paginationOptions.PageSize;

            return View(await PaginatedList<DamageReport>.CreateAsync(reports, page ?? 1, pageSize));
        }

        // GET: DamageReport/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var damageReport = await _context.DamageReport
                .Include(d => d.Property.Vendor)
                //.Include(d => d.Vendor)
                .FirstOrDefaultAsync(m => m.ReportId == id);
            if (damageReport == null)
            {
                return NotFound();
            }

            return View(damageReport);
        }

        // GET: DamageReport/Create
        public IActionResult Create(string bookingId)
        {
            DamageReport damageReport = new DamageReport
            {
                BookingId = bookingId,
                ActualRepairCost = 0,
                EstimateRepairCost = 0
            };
            ViewData["PropertyId"] = new SelectList(_context.Property, "PropertyId", "PropertyName");
            //ViewData["VendorId"] = new SelectList(_context.Vendor, "VendorId", "VendorId");
            return View(damageReport);
        }

        // POST: DamageReport/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DamageReport damageReport)
        {
            if (ModelState.IsValid)
            {
                damageReport.CreatedBy = User.Identity.GetUserId();
                _context.Add(damageReport);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PropertyId"] = new SelectList(_context.Property, "PropertyId", "PropertyName", damageReport.PropertyId);
            //ViewData["VendorId"] = new SelectList(_context.Vendor, "VendorId", "VendorId", damageReport.VendorId);
            return View(damageReport);
        }

        // GET: DamageReport/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var damageReport = await _context.DamageReport.FindAsync(id);
            if (damageReport == null)
            {
                return NotFound();
            }
            ViewData["PropertyId"] = new SelectList(_context.Property, "PropertyId", "PropertyName", damageReport.PropertyId);
            //ViewData["VendorId"] = new SelectList(_context.Vendor, "VendorId", "VendorId", damageReport.VendorId);
            return View(damageReport);
        }

        // POST: DamageReport/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, DamageReport damageReport)
        {
            if (id != damageReport.ReportId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    damageReport.ModifiedAt = DateTime.UtcNow;
                    damageReport.ModifiedBy = User.Identity.GetUserId();
                    _context.Update(damageReport);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DamageReportExists(damageReport.ReportId))
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
            ViewData["PropertyId"] = new SelectList(_context.Property, "PropertyId", "PropertyName", damageReport.PropertyId);
            //ViewData["VendorId"] = new SelectList(_context.Vendor, "VendorId", "VendorId", damageReport.VendorId);
            return View(damageReport);
        }

        // GET: DamageReport/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var damageReport = await _context.DamageReport
                .Include(d => d.Property.Vendor)
                //.Include(d => d.Vendor)
                .FirstOrDefaultAsync(m => m.ReportId == id);
            if (damageReport == null)
            {
                return NotFound();
            }

            return View(damageReport);
        }

        // POST: DamageReport/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var damageReport = await _context.DamageReport.FindAsync(id);
            _context.DamageReport.Remove(damageReport);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DamageReportExists(string id)
        {
            return _context.DamageReport.Any(e => e.ReportId == id);
        }
    }
}
