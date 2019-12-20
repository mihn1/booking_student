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
using B2S.Core.Enums;
using B2S.Core.Utilities;
using B2S.Core.Common;
using Microsoft.Extensions.Options;
using System.IO;

namespace B2S.Web.Controllers
{
    public class ExpenseController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PaginationOptions _paginationOptions;

        public ExpenseController(AppDbContext context, IOptions<PaginationOptions> options)
        {
            _context = context;
            _paginationOptions = options.Value;
        }

        // GET: Expense
        public async Task<IActionResult> Index(string search, string categoryId, int? page)
        {
            ViewData["CurrentSearch"] = search;
            ViewData["CurrentCategoryId"] = categoryId;
            ViewData["CategoryId"] = new SelectList(_context.ExpenseCategory, "CategoryId", "CategoryName", categoryId);

            var user = await _context.User.FindAsync(User.Identity.GetUserId());
            var accountId = (await _context.UserAccount.FirstOrDefaultAsync(u => u.UserId == user.Id))?.AccountId;
            bool isAdmin = user.IsSuperAdmin || user.UserType == UserType.Admin;

            var expenses = _context.Expense.Include(e => e.Category).Select(e => e);

            if (!isAdmin && user.UserType == UserType.Vendor)
            {
                expenses = expenses.Where(i => i.VendorId == accountId);
            }
                       
            if (!string.IsNullOrEmpty(search))
            {
                expenses = expenses.Where(l => l.ExpenseDate.ToShortDateString().Contains(search) || l.Reference.ToUpper().Contains(search.ToUpper()));
            }
            if (!string.IsNullOrEmpty(categoryId))
            {
                expenses = expenses.Where(e => e.CategoryId == categoryId);
            }

            int pageSize = _paginationOptions.PageSize;

            return View(await PaginatedList<Expense>.CreateAsync(expenses, page ?? 1, pageSize));
        }

        // GET: Expense/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense
                .Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.ExpenseId == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // GET: Expense/Create
        public async Task<IActionResult> Create()
        {
            var user = await _context.User.FindAsync(User.Identity.GetUserId());
            if (user.UserType == UserType.Vendor)
            {
                string accountId = (await _context.UserAccount.FirstOrDefaultAsync(u => u.UserId == user.Id))?.AccountId;
                Expense expense = new Expense { VendorId = accountId };
                ViewData["CategoryId"] = new SelectList(_context.ExpenseCategory, "CategoryId", "CategoryName");
                ViewData["IsVendor"] = true;
                return View(expense);
            }
            ViewData["IsVendor"] = false;
            ViewData["VendorId"] = new SelectList(_context.Vendor, "VendorId", "VendorName");
            ViewData["CategoryId"] = new SelectList(_context.ExpenseCategory, "CategoryId", "CategoryName");
            return View();
        }

        // POST: Expense/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ExpenseId,Description,CategoryId,Reference,ExpenseDate,Amount,Receipt,VendorId,CreatedAt,CreatedBy,ModifiedAt,ModifiedBy,DeletedAt,DeletedBy,IsDeleted")] Expense expense)
        {
            if (ModelState.IsValid)
            {
                var formFile = Request.Form.Files["Receipt"];
                if (formFile != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        await formFile.CopyToAsync(stream);
                        expense.Receipt = stream.ToArray();
                        expense.FileName = formFile.FileName;
                    }
                }

                expense.CreatedBy = User.Identity.GetUserId();
                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var user = await _context.User.FindAsync(User.Identity.GetUserId());
            if (user.UserType == UserType.Vendor)
            {
                ViewData["CategoryId"] = new SelectList(_context.ExpenseCategory, "CategoryId", "CategoryName", expense.CategoryId);
                ViewData["IsVendor"] = true;
                return View(expense);
            }
            ViewData["IsVendor"] = false;
            ViewData["VendorId"] = new SelectList(_context.Vendor, "VendorId", "VendorName", expense.VendorId);
            ViewData["CategoryId"] = new SelectList(_context.ExpenseCategory, "CategoryId", "CategoryName", expense.CategoryId);
            return View(expense);
        }

        // GET: Expense/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(User.Identity.GetUserId());
            if (user.UserType == UserType.Vendor)
            {
                ViewData["CategoryId"] = new SelectList(_context.ExpenseCategory, "CategoryId", "CategoryName", expense.CategoryId);
                ViewData["IsVendor"] = true;
                return View(expense);
            }
            ViewData["IsVendor"] = false;
            ViewData["VendorId"] = new SelectList(_context.Vendor, "VendorId", "VendorName", expense.VendorId);
            ViewData["CategoryId"] = new SelectList(_context.ExpenseCategory, "CategoryId", "CategoryName", expense.CategoryId);
            return View(expense);
        }

        // POST: Expense/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ExpenseId,Description,CategoryId,Reference,ExpenseDate,Amount,Receipt,FileName,VendorId,CreatedAt,CreatedBy,ModifiedAt,ModifiedBy,DeletedAt,DeletedBy,IsDeleted")] Expense expense)
        {
            if (id != expense.ExpenseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    expense.ModifiedAt = DateTime.UtcNow;
                    expense.ModifiedBy = User.Identity.GetUserId();
                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.ExpenseId))
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
            var user = await _context.User.FindAsync(User.Identity.GetUserId());
            if (user.UserType == UserType.Vendor)
            {
                ViewData["CategoryId"] = new SelectList(_context.ExpenseCategory, "CategoryId", "CategoryName", expense.CategoryId);
                ViewData["IsVendor"] = true;
                return View(expense);
            }
            ViewData["IsVendor"] = false;
            ViewData["VendorId"] = new SelectList(_context.Vendor, "VendorId", "VendorName", expense.VendorId);
            ViewData["CategoryId"] = new SelectList(_context.ExpenseCategory, "CategoryId", "CategoryName", expense.CategoryId);
            return View(expense);
        }

        // GET: Expense/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense
                .Include(e => e.Category)
                .FirstOrDefaultAsync(m => m.ExpenseId == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // POST: Expense/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var expense = await _context.Expense.FindAsync(id);
            _context.Expense.Remove(expense);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetReceipt(string id)
        {
            var expense = await _context.Expense.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }

            Response.Headers.Add("Content-Disposition", "inline");

            return File(expense.Receipt, "application/octet-stream", expense.FileName);
        }

        private bool ExpenseExists(string id)
        {
            return _context.Expense.Any(e => e.ExpenseId == id);
        }
    }
}
