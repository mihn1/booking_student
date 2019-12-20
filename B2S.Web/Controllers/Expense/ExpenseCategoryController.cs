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
    public class ExpenseCategoryController : Controller
    {
        private readonly AppDbContext _context;

        public ExpenseCategoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ExpenseCategory
        public async Task<IActionResult> Index()
        {
            return View(await _context.ExpenseCategory.ToListAsync());
        }

        // GET: ExpenseCategory/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expenseCategory = await _context.ExpenseCategory
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (expenseCategory == null)
            {
                return NotFound();
            }

            return View(expenseCategory);
        }

        // GET: ExpenseCategory/Create
        public IActionResult Create()
        {
            ExpenseCategory expenseCategory = new ExpenseCategory
            {
                ColorHex = "#00a65a"
            };

            return View(expenseCategory);
        }

        // POST: ExpenseCategory/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,CategoryName,Description,AccountCode,ColorHex,CreatedAt,CreatedBy,ModifiedAt,ModifiedBy,DeletedAt,DeletedBy,IsDeleted")] ExpenseCategory expenseCategory)
        {
            if (ModelState.IsValid)
            {
                expenseCategory.CreatedBy = User.Identity.GetUserId();
                _context.Add(expenseCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(expenseCategory);
        }

        // GET: ExpenseCategory/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expenseCategory = await _context.ExpenseCategory.FindAsync(id);
            if (expenseCategory == null)
            {
                return NotFound();
            }
            return View(expenseCategory);
        }

        // POST: ExpenseCategory/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CategoryId,CategoryName,Description,AccountCode,ColorHex,CreatedAt,CreatedBy,ModifiedAt,ModifiedBy,DeletedAt,DeletedBy,IsDeleted")] ExpenseCategory expenseCategory)
        {
            if (id != expenseCategory.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    expenseCategory.ModifiedAt = DateTime.UtcNow;
                    expenseCategory.ModifiedBy = User.Identity.GetUserId();
                    _context.Update(expenseCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseCategoryExists(expenseCategory.CategoryId))
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
            return View(expenseCategory);
        }

        // GET: ExpenseCategory/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expenseCategory = await _context.ExpenseCategory
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (expenseCategory == null)
            {
                return NotFound();
            }

            return View(expenseCategory);
        }

        // POST: ExpenseCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var expenseCategory = await _context.ExpenseCategory.FindAsync(id);
            _context.ExpenseCategory.Remove(expenseCategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseCategoryExists(string id)
        {
            return _context.ExpenseCategory.Any(e => e.CategoryId == id);
        }
    }
}
