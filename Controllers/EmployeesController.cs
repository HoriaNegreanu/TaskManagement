﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;

namespace TaskManagement.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public EmployeesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(string searchName)
        {
            var users = _userManager.Users.ToList<ApplicationUser>();
            //Filters
            //Search by employee name filter
            if (!String.IsNullOrEmpty(searchName))
            {
                users = users.Where(t => t.FullName.Contains(searchName, System.StringComparison.CurrentCultureIgnoreCase)).ToList();
            }

            var employeesViewModel = new List<EmployeesViewModel>();
            foreach (ApplicationUser user in users)
            {
                var thisViewModel = new EmployeesViewModel();
                thisViewModel.UserId = user.Id;
                thisViewModel.FirstName = user.FirstName;
                thisViewModel.LastName = user.LastName;
                employeesViewModel.Add(thisViewModel);
            }
            return View(employeesViewModel);
        }

        public async Task<IActionResult> Details(string userId, int? month, int? year)
        {
            var employee = await _userManager.FindByIdAsync(userId);
            if(employee == null)
            {
                return NotFound();
            }
            ViewBag.EmployeeName = employee.FullName;

            //create model
            if (year == null)
                year = DateTime.Now.Year;
            if(month == null)
                month = DateTime.Now.Month;
            var taskItems = await _context.TaskItem.Where(t => t.AssignedTo == employee.FullName).Include(t => t.Project).ToListAsync();
            var employeeHours = await _context.EmployeeHour.Where(e => e.UserID == employee.Id).Include(e => e.User).Include(e => e.TaskItem).ToListAsync();

            //get task statistics
            var activeTasks = taskItems.Where(t => t.Status == Status.Active.ToString()).Count();
            ViewBag.ActiveTasks = activeTasks;
            decimal totalWorkHoursMonth = 0;
            ViewBag.TotalWorkHoursMonth = totalWorkHoursMonth;
            var assignedTasks = taskItems.Where(t => (t.Status != Status.Closed.ToString() && t.Status != Status.Review.ToString())).Count();
            ViewBag.AssignedTasks = assignedTasks;
            var inReviewTasks = taskItems.Where(t => t.Status == Status.Review.ToString()).Count();
            ViewBag.InReviewTasks = inReviewTasks;

            //remove password hash from being exposed
            //remove description, since a large description may lead to errors
            foreach (var employeeHour in employeeHours)
            {
                employeeHour.User = null;
                employeeHour.TaskItem.Description = null;
            }
            var model = new EmployeesViewModel();
            model.Month = month;
            model.Year = year;
            model.UserId = employee.Id;
            model.FirstName = employee.FirstName;
            model.LastName = employee.LastName;
            model.FullName = employee.FullName;
            model.Tasks = taskItems;
            model.EmployeeHoursDistinct = null;

            if (month != null)
            {
                model.EmployeeHours = employeeHours.FindAll(t => t.CompletedDate.Month == month).FindAll(t => t.CompletedDate.Year == year).ToList();
                totalWorkHoursMonth = model.EmployeeHours.Where(t => t.CompletedDate.Month == month).Where(t => t.CompletedDate.Year == year).Sum(t => t.WorkedHours);
                ViewBag.TotalWorkHoursMonth = totalWorkHoursMonth;
            }
            else
                model.EmployeeHours = null;

            //remove duplicate tasks from employeeHours
            model.EmployeeHoursDistinct = model.EmployeeHours.DistinctBy(t => t.TaskItemID).ToList();

            var totalHours = model.Tasks.Sum(x => x.WorkedHours);
            model.TotalHours = totalHours;

            return View(model);
        }

        public async Task<IActionResult> GetTasksForMonth(string userId, int month, int year)
        {
            return RedirectToAction("Details", "Employees", new { UserId = userId, Month = month, Year = year });
        }

        public async Task<IActionResult> ViewAllTaskForEmployee(string fullName)
        {
            return RedirectToAction("Index", "TaskItems", new { TaskAssignedTo = fullName });
        }
    }
}
