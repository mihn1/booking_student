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
using B2S.Core.Common;
using Microsoft.Extensions.Options;
using B2S.Core.Utilities;

namespace B2S.Web.Controllers
{
    public class AgentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PaginationOptions _paginationOptions;

        public AgentController(AppDbContext context, IOptions<PaginationOptions> paginationOptions)
        {
            _context = context;
            _paginationOptions = paginationOptions.Value;
        }

        // GET: Agent
        public async Task<IActionResult> Index(string search, int? page)
        {
            ViewData["CurrentSearch"] = search;
            var agents = _context.Agent.Select(c => c);
            
            if (!string.IsNullOrEmpty(search))
            {
                agents = agents.Where(l => string.Format("{0} {1}", l.BusinessName, l.ContactName).ToUpper().Contains(search.ToUpper()) || l.Email.ToUpper().Contains(search.ToUpper()) || l.Phone.Contains(search));
            }

            int pageSize = _paginationOptions.PageSize;

            return View(await PaginatedList<Agent>.CreateAsync(agents, page ?? 1, pageSize));
        }

        // GET: Agent/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var agent = await _context.Agent
                .Include(i => i.InvoiceSetting)
                .FirstOrDefaultAsync(m => m.AgentId == id);
            if (agent == null)
            {
                return NotFound();
            }

            return View(agent);
        }

        // GET: Agent/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Agent/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AgentId,BusinessName,LegalName,BusinessNumber,InvoiceSettingId,ContactName,Phone,Fax,Email,Address,City,Province,Postcode,Country,IsActive,CreatedAt,CreatedBy,ModifiedAt,ModifiedBy,DeletedAt,DeletedBy,IsDeleted")] Agent agent)
        {
            if (ModelState.IsValid)
            {
                agent.CreatedBy = User.Identity.GetUserId();
                _context.Add(agent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(agent);
        }

        // GET: Agent/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var agent = await _context.Agent.FindAsync(id);
            if (agent == null)
            {
                return NotFound();
            }

            ViewData["InvoiceSettingId"] = new SelectList(_context.InvoiceSetting, "InvoiceSettingId", "Name", agent.InvoiceSettingId);
            return View(agent);
        }

        // POST: Agent/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("AgentId,BusinessName,LegalName,BusinessNumber,InvoiceSettingId,ContactName,Phone,Fax,Email,Address,City,Province,Postcode,Country,IsActive,CreatedAt,CreatedBy,ModifiedAt,ModifiedBy,DeletedAt,DeletedBy,IsDeleted")] Agent agent)
        {
            if (id != agent.AgentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    agent.ModifiedAt = DateTime.UtcNow;
                    agent.ModifiedBy = User.Identity.GetUserId();
                    _context.Update(agent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AgentExists(agent.AgentId))
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

            ViewData["InvoiceSettingId"] = new SelectList(_context.InvoiceSetting, "InvoiceSettingId", "Name", agent.InvoiceSettingId);
            return View(agent);
        }

        // GET: Agent/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var agent = await _context.Agent
                .Include(i => i.InvoiceSetting)
                .FirstOrDefaultAsync(m => m.AgentId == id);
            if (agent == null)
            {
                return NotFound();
            }

            return View(agent);
        }

        // POST: Agent/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var agent = await _context.Agent.FindAsync(id);
            _context.Agent.Remove(agent);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AgentExists(string id)
        {
            return _context.Agent.Any(e => e.AgentId == id);
        }
    }
}
