using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;

namespace TaskManagement.Controllers
{
    [Authorize]
    public class TaskStagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskStagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TaskStages
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TaskStage.Include(t => t.TaskItem);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TaskStages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TaskStage == null)
            {
                return NotFound();
            }

            var taskStage = await _context.TaskStage
                .Include(t => t.TaskItem)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (taskStage == null)
            {
                return NotFound();
            }

            return View(taskStage);
        }

        // GET: TaskStages/Create
        [Authorize(Roles = "Administrator, QA")]
        public IActionResult Create()
        {
            ViewData["TaskItemID"] = new SelectList(_context.TaskItem, "ID", "Title");
            return View();
        }

        // POST: TaskStages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrator, QA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Title,Message,TaskItemID,Status")] TaskStage taskStage)
        {
            if (ModelState.IsValid)
            {
                taskStage.Status = StageStatus.Proposed.ToString();
                _context.Add(taskStage);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "TaskItems", new { id = taskStage.TaskItemID });
            }
            ViewData["TaskItemID"] = new SelectList(_context.TaskItem, "ID", "Title", taskStage.TaskItemID);
            return RedirectToAction("Details", "TaskItems", new { id = taskStage.TaskItemID });
        }

        // GET: TaskStages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TaskStage == null)
            {
                return NotFound();
            }

            var taskStage = await _context.TaskStage.FindAsync(id);
            if (taskStage == null)
            {
                return NotFound();
            }
            ViewData["TaskItemID"] = new SelectList(_context.TaskItem, "ID", "Title", taskStage.TaskItemID);
            return View(taskStage);
        }

        // POST: TaskStages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,Message,TaskItemID,Status")] TaskStage taskStage)
        {
            if (id != taskStage.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskStage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskStageExists(taskStage.ID))
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
            ViewData["TaskItemID"] = new SelectList(_context.TaskItem, "ID", "Title", taskStage.TaskItemID);
            return View(taskStage);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (_context.TaskStage == null)
            {
                return Problem("Entity set 'ApplicationDbContext.TaskStage'  is null.");
            }
            var taskStage = await _context.TaskStage.FindAsync(id);
            var taskID = taskStage.TaskItemID;
            if (taskStage != null)
            {
                _context.TaskStage.Remove(taskStage);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "TaskItems", new { id = taskID });
        }

        private bool TaskStageExists(int id)
        {
          return (_context.TaskStage?.Any(e => e.ID == id)).GetValueOrDefault();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetCompleted(int? id)
        {
            var taskStage = await _context.TaskStage.FindAsync(id);
            if(taskStage == null)
            {
                return NotFound();
            }
            taskStage.Status = StageStatus.Completed.ToString();
            _context.Update(taskStage);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "TaskItems", new { id = taskStage.TaskItemID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetProposed(int? id)
        {
            var taskStage = await _context.TaskStage.FindAsync(id);
            if (taskStage == null)
            {
                return NotFound();
            }
            taskStage.Status = StageStatus.Proposed.ToString();
            _context.Update(taskStage);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "TaskItems", new { id = taskStage.TaskItemID });
        }
    }
}
