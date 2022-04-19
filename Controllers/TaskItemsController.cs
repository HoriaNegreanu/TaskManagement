#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;

namespace TaskManagement.Controllers
{
    public class TaskItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskItemsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: TaskItems
        public async Task<IActionResult> Index()
        {
            return View("Index", await _context.TaskItem.ToListAsync());
        }

        // GET: TaskItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItem
                .FirstOrDefaultAsync(m => m.ID == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            //Creates ViewBag for displaying messages
            ViewBag.Message = TempData["Message"];

            //gets comments associated with task
            var listComments = await _context.Comment.Where(c => c.TaskItemID == id).ToListAsync();
            var result = new TaskItemViewModel();
            result.TaskItem = taskItem;
            result.Comments = listComments.OrderByDescending(i => i.CreatedDate).ToList();

            //gets files associated with task
            var fileuploadViewModel = await LoadAllFiles(id);
            result.FilesOnFileSystem = fileuploadViewModel.FilesOnFileSystem;

            return View(result);
        }

        // GET: TaskItems/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TaskItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Title,CreatedBy,AssignedTo,CreatedDate,ActivatedDate,WorkedHours,Priority,Status,Description")] TaskItem taskItem)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var name = user.FirstName + " " + user.LastName;
                taskItem.CreatedDate = DateTime.Now;
                taskItem.CreatedBy = name;
                taskItem.WorkedHours = 0;
                taskItem.ActivatedDate = null;
                //taskItem.Status = Status.Proposed.ToString();
                if(taskItem.Status == Status.Active.ToString())
                {
                    taskItem.ActivatedDate = DateTime.Now;
                }
                _context.Add(taskItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(taskItem);
        }

        // GET: TaskItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItem.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }
            return View(taskItem);
        }

        // POST: TaskItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,AssignedTo,ActivatedDate,WorkedHours,Priority,Status,Description")] TaskItem taskItem)
        {
            if (id != taskItem.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //make it so fields which can't be edited, like "CreatedBy" or "CreatedDate", will not be null and keep old value
                    TaskItem updateT = await _context.TaskItem.FindAsync(id);
                    var createdBy = updateT.CreatedBy;
                    var createdOn = updateT.CreatedDate;
                    var status = updateT.Status;
                    foreach (var property in typeof(TaskItem).GetProperties())
                    {
                        var propval = property.GetValue(taskItem);
                        property.SetValue(updateT, propval);
                    }
                    updateT.CreatedDate = createdOn;
                    updateT.CreatedBy = createdBy;
                    if(updateT.Status != status && updateT.Status == Status.Active.ToString())
                    {
                        updateT.ActivatedDate = DateTime.Now;
                    }
                    else if(updateT.Status != status && status == Status.Active.ToString())
                    {
                        var timeWorked = (DateTime.Now - updateT.ActivatedDate).Value;
                        updateT.WorkedHours += Math.Abs(Convert.ToDecimal(timeWorked.TotalHours));
                        updateT.ActivatedDate = null;
                    }
                    _context.Update(updateT);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskItemExists(taskItem.ID))
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
            return View(taskItem);
        }

        // GET: TaskItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItem
                .FirstOrDefaultAsync(m => m.ID == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        // POST: TaskItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //delete Task from DB
            var taskItem = await _context.TaskItem.FindAsync(id);
            _context.TaskItem.Remove(taskItem);

            //delete all files associated with task from DB
            var viewModel = await LoadAllFiles(id);
            foreach (var file in viewModel.FilesOnFileSystem)
            {
                _context.FileOnFileSystem.Remove(file);
            }
            //delete task files folder
            var basePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Files\\" + id.ToString() + "\\");
            bool basePathExists = System.IO.Directory.Exists(basePath);
            if (basePathExists) Directory.Delete(basePath, true);

            //save DB changes
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaskItemExists(int id)
        {
            return _context.TaskItem.Any(e => e.ID == id);
        }

        //gets files associated with task
        private async Task<FileUploadViewModel> LoadAllFiles(int? id)
        {
            var viewModel = new FileUploadViewModel();
            var allFiles = await _context.FileOnFileSystem.ToListAsync();
            viewModel.FilesOnFileSystem = new List<FileOnFileSystemModel>();
            foreach(var file in allFiles)
            {
                if(getTaskIdFromPath(file.FilePath) == id)
                {
                    viewModel.FilesOnFileSystem.Add(file);
                }
            }
            return viewModel;
        }

        //gets task id from a path
        private int getTaskIdFromPath(string path)
        {
            var toBeSearched = "Files\\";
            var result = path.Substring(path.IndexOf(toBeSearched) + toBeSearched.Length);
            result = result.Substring(0, result.IndexOf("\\"));
            return Int32.Parse(result);
        }

    }
}
