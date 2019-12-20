using B2S.Core.Entities;
using B2S.Infrastructure.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace B2S.Web.Controllers
{
    [Authorize(Roles = "EmailTemplate")]
    public class EmailTemplateController : Controller
    {
        private readonly AppDbContext _context;

        public EmailTemplateController(AppDbContext context)
        {
            _context = context;
        }

        // GET: EmailTemplate
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.EmailTemplate;
            return View(await appDbContext.ToListAsync());
        }

        // GET: EmailTemplate/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emailTemplate = await _context.EmailTemplate
                .SingleOrDefaultAsync(m => m.EmailTemplateId == id);
            if (emailTemplate == null)
            {
                return NotFound();
            }

            return View(emailTemplate);
        }

        // GET: EmailTemplate/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EmailTemplate/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmailTemplateId,TemplatName,Description,Subject,IsPublished,BodyText,BodyHTML,AssignedToId,CreatedAt,CreatedBy,ModifiedAt,ModifiedBy,DeletedAt,DeletedBy,IsDeleted")] EmailTemplate emailTemplate)
        {
            if (ModelState.IsValid)
            {
                emailTemplate.CreatedBy = User.Identity.GetUserId();
                _context.Add(emailTemplate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            return View(emailTemplate);
        }

        // GET: EmailTemplate/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emailTemplate = await _context.EmailTemplate.FindAsync(id);
            if (emailTemplate == null)
            {
                return NotFound();
            }
            return View(emailTemplate);
        }

        // POST: EmailTemplate/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("EmailTemplateId,TemplatName,Description,Subject,IsPublished,BodyText,BodyHTML,AssignedToId,CreatedAt,CreatedBy,ModifiedAt,ModifiedBy,DeletedAt,DeletedBy,IsDeleted")] EmailTemplate emailTemplate)
        {
            if (id != emailTemplate.EmailTemplateId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    emailTemplate.ModifiedBy = User.Identity.GetUserId();
                    emailTemplate.ModifiedAt = DateTime.Today;
                    _context.Update(emailTemplate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmailTemplateExists(emailTemplate.EmailTemplateId))
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
            
            return View(emailTemplate);
        }

        // GET: EmailTemplate/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var emailTemplate = await _context.EmailTemplate
                .FirstOrDefaultAsync(m => m.EmailTemplateId == id);
            if (emailTemplate == null)
            {
                return NotFound();
            }

            return View(emailTemplate);
        }

        // POST: EmailTemplate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var emailTemplate = await _context.EmailTemplate.FindAsync(id);
            _context.EmailTemplate.Remove(emailTemplate);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmailTemplateExists(string id)
        {
            return _context.EmailTemplate.Any(e => e.EmailTemplateId == id);
        }
    }
}
