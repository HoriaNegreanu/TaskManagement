﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskManagement.Data;
using TaskManagement.Models;
using TaskManagement.Models.Mail;
using TaskManagement.Services;

namespace TaskManagement.Controllers
{
    [Authorize]
    public class TaskItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public readonly IOptions<MailSettings> _mailSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const int ITEMS_PER_PAGE = 10;

        public TaskItemsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IOptions<MailSettings> mailSettings, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _mailSettings = mailSettings;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: TaskItems
        public async Task<IActionResult> Index(string searchTitle, string taskPriority, int? taskProject, string taskAssignedTo, string taskStatus, int currentPage)
        {
            var taskItems = await _context.TaskItem.Where(t => t.Status != Status.Closed.ToString()).Include(t => t.Project).ToListAsync();

            //Filters
            //Search by title filter
            if (!String.IsNullOrEmpty(searchTitle))
            {
                taskItems = taskItems.Where(t => t.Title.Contains(searchTitle, System.StringComparison.CurrentCultureIgnoreCase)).ToList();
            }

            //Search by assigned to
            var users = CreateUsersSelectList();
            if (!String.IsNullOrEmpty(taskAssignedTo))
            {
                taskItems = taskItems.Where(t => t.AssignedTo.Contains(taskAssignedTo, System.StringComparison.CurrentCultureIgnoreCase)).ToList();
            }

            //Search by project
            var projects = CreateProjectIDSelectList();
            if (taskProject != null)
            {
                taskItems = taskItems.Where(x => x.ProjectID == taskProject).ToList();
            }

            //Search by priority
            var prioties = new SelectList(Enum.GetValues(typeof(Priority)));
            if (!string.IsNullOrEmpty(taskPriority))
            {
                taskItems = taskItems.Where(x => x.Priority == taskPriority).ToList();
            }

            //Search by status
            var statuses = CreateStatusSelectList();
            if (!String.IsNullOrEmpty(taskStatus))
            {
                taskItems = taskItems.Where(t => t.Status.Contains(taskStatus, System.StringComparison.CurrentCultureIgnoreCase)).ToList();
            }

            //Pagination
            var count = taskItems.Count();
            var totalPages = (int)Math.Ceiling(count / (double)ITEMS_PER_PAGE);
            if(currentPage > totalPages)
                currentPage = totalPages;
            else if(currentPage < 1)
                currentPage = 1;
            taskItems = taskItems.Skip((currentPage - 1) * ITEMS_PER_PAGE).Take(ITEMS_PER_PAGE).ToList();

            //Create model with data
            var taskItemFilterVM = new TaskItemFiltersViewModel()
            {
                Priorities = prioties,
                TaskItems = taskItems,
                Projects = projects,
                Users = users,
                Statuses = statuses,
                CurrentPage = currentPage,
                TotalPages = totalPages
            };

            return View("Index", taskItemFilterVM);
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

            //gets comments associated with task
            var listComments = await _context.Comment.Where(c => c.TaskItemID == id).ToListAsync();
            var result = new TaskItemViewModel();
            result.TaskItem = taskItem;
            //order descending by date
            result.Comments = listComments.OrderByDescending(i => i.CreatedDate).ToList();

            //gets milestones associated with task
            var listStages = await _context.TaskStage.Where(c => c.TaskItemID == id).ToListAsync();
            foreach (var stage in listStages)
            {
                stage.TaskItem = null;
            }
            result.TaskStages = listStages;

            //gets files associated with task
            var fileuploadViewModel = await LoadAllFiles(id);
            result.FilesOnFileSystem = fileuploadViewModel.FilesOnFileSystem;

            return View(result);
        }

        // GET: TaskItems/Create
        [Authorize(Roles = "Administrator, QA")]
        public IActionResult Create()
        {
            //Gets data for select lists
            ViewData["Status"] = CreateStatusSelectList();
            ViewData["AssignedTo"] = CreateUsersSelectList();
            ViewData["ProjectID"] = CreateProjectIDSelectList();
            return View();
        }

        // POST: TaskItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrator, QA")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Title,CreatedBy,AssignedTo,CreatedDate,ActivatedDate,WorkedHours,Priority,Status,ProjectID,Description")] TaskItem taskItem)
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

                //send Email
                //get destination email
                var users = await _userManager.Users.ToListAsync<ApplicationUser>();
                var destinationUser = users.First(u => u.FullName == taskItem.AssignedTo);
                var mailAddress = await _userManager.GetEmailAsync(destinationUser);

                //send email when creating task
                if(mailAddress != null)
                {
                    var mailService = new MailService(_mailSettings, _httpContextAccessor);
                    mailService.SendEmailTaskAvailable(mailAddress, taskItem);
                } 

                return RedirectToAction(nameof(Index));
            }
            return View(taskItem);
        }

        // GET: TaskItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            ViewData["Status"] = CreateStatusSelectList();
            ViewData["AssignedTo"] = CreateUsersSelectList();
            ViewData["ProjectID"] = CreateProjectIDSelectList();
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
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,AssignedTo,ActivatedDate,WorkedHours,Priority,Status,ProjectID,Description,CreatedDate,CreatedBy")] TaskItem taskItem)
        {
            if (id != taskItem.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    TaskItem updateT = await _context.TaskItem.FindAsync(id);
                    var status = updateT.Status;
                    var assignedTo = updateT.AssignedTo;
                    foreach (var property in typeof(TaskItem).GetProperties())
                    {
                        var propval = property.GetValue(taskItem);
                        property.SetValue(updateT, propval);
                    }
                    if(updateT.Status != status && updateT.Status == Status.Active.ToString())
                    {
                        updateT.ActivatedDate = DateTime.Now;
                    }
                    else if(updateT.Status != status && status == Status.Active.ToString())
                    {
                        var timeWorked = (DateTime.Now - updateT.ActivatedDate).Value;
                        updateT.WorkedHours += Math.Abs(Convert.ToDecimal(timeWorked.TotalHours));
                        updateT.ActivatedDate = null;

                        var employeeHour = new EmployeeHour();
                        employeeHour.WorkedHours = Math.Abs(Convert.ToDecimal(timeWorked.TotalHours));
                        employeeHour.TaskItemID = id;
                        var user = _userManager.Users.ToList<ApplicationUser>().Find(u => u.FullName == updateT.AssignedTo);
                        employeeHour.UserID = user.Id;
                        employeeHour.CompletedDate = DateTime.Now;
                        _context.Add(employeeHour);
                    }

                    //send email if user assignedTo is changed
                    if (updateT.AssignedTo != assignedTo)
                    {
                        var users = await _userManager.Users.ToListAsync<ApplicationUser>();
                        var destinationUser = users.First(u => u.FullName == updateT.AssignedTo);
                        var mailAddress = await _userManager.GetEmailAsync(destinationUser);

                        //send email
                        if (mailAddress != null)
                        {
                            var mailService = new MailService(_mailSettings, _httpContextAccessor);
                            mailService.SendEmailTaskAssignedToChanged(mailAddress, taskItem);
                        }
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
        [Authorize(Roles = "Administrator, QA")]
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
        [Authorize(Roles = "Administrator, QA")]
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

        public static void DeleteTaskStatic(int id, ApplicationDbContext _context)
        {
            //delete Task from DB
            var taskItem = _context.TaskItem.Find(id);
            _context.TaskItem.Remove(taskItem);

            //delete all files associated with task from DB
            var viewModel = new FileUploadViewModel();
            var allFiles = _context.FileOnFileSystem.ToList();
            viewModel.FilesOnFileSystem = new List<FileOnFileSystemModel>();
            foreach (var file in allFiles)
            {
                if (getTaskIdFromPath(file.FilePath) == id)
                {
                    viewModel.FilesOnFileSystem.Add(file);
                }
            }
            //order descending by date
            viewModel.FilesOnFileSystem = viewModel.FilesOnFileSystem.OrderByDescending(i => i.CreatedOn).ToList();
            foreach (var file in viewModel.FilesOnFileSystem)
            {
                _context.FileOnFileSystem.Remove(file);
            }
            //delete task files folder
            var basePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Files\\" + id.ToString() + "\\");
            bool basePathExists = System.IO.Directory.Exists(basePath);
            if (basePathExists) Directory.Delete(basePath, true);
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
            //order descending by date
            viewModel.FilesOnFileSystem = viewModel.FilesOnFileSystem.OrderByDescending(i => i.CreatedOn).ToList();
            return viewModel;
        }

        //gets task id from a path
        private static int getTaskIdFromPath(string path)
        {
            var toBeSearched = "Files\\";
            var result = path.Substring(path.IndexOf(toBeSearched) + toBeSearched.Length);
            result = result.Substring(0, result.IndexOf("\\"));
            return Int32.Parse(result);
        }

        //creates list with statuses except for "Closed"
        private SelectList CreateStatusSelectList()
        {
            var selectList = new SelectList(Enum.GetValues(typeof(Status))
                        .Cast<Status>()
                        .Where(e => e != Status.Closed));
            return selectList;
        }

        //creates list with user names
        private SelectList CreateUsersSelectList()
        {
            var selectList = new SelectList(_context.Users, "FullName", "FullName");
            return selectList;
        }

        //creates list with project IDs
        private SelectList CreateProjectIDSelectList()
        {
            var selectList = new SelectList(_context.Project, "ID", "Title");
            return selectList;
        }

        //Pagination
        public async Task<IActionResult> PreviousPage(int? currentPage, string searchTitle, string taskPriority, int? taskProject, string taskAssignedTo, string taskStatus)
        {
            return RedirectToAction("Index", new
            {
                currentPage = currentPage - 1,
                searchTitle = searchTitle,
                taskPriority = taskPriority,
                taskProject = taskProject,
                taskAssignedTo = taskAssignedTo,
                taskStatus = taskStatus
            });
        }

        public async Task<IActionResult> NextPage(int? currentPage, string searchTitle, string taskPriority, int? taskProject, string taskAssignedTo, string taskStatus)
        {
            if (currentPage == 0)
                currentPage = 1;
            return RedirectToAction("Index", new
            {
                currentPage = currentPage + 1,
                searchTitle = searchTitle,
                taskPriority = taskPriority,
                taskProject = taskProject,
                taskAssignedTo = taskAssignedTo,
                taskStatus = taskStatus
            });
        }

    }
}
