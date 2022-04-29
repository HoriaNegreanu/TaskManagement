using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;

namespace TaskManagement.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public EmployeesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList<ApplicationUser>();
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
            var taskItems = await _context.TaskItem.Where(t => t.AssignedTo == employee.FullName).Include(t => t.Project).ToListAsync();
            var employeeHours = await _context.EmployeeHour.Where(e => e.UserID == employee.Id).Include(e => e.User).Include(e => e.TaskItem).ToListAsync();
            var model = new EmployeesViewModel();
            model.Month = month;
            model.Year = year;
            model.UserId = employee.Id;
            model.FirstName = employee.FirstName;
            model.LastName = employee.LastName;
            model.Tasks = taskItems;

            if (month != null)
            {
                model.EmployeeHours = employeeHours.FindAll(t => t.CompletedDate.Month == month).FindAll(t => t.CompletedDate.Year == year).ToList();
                var totalWorkHoursMonth = model.EmployeeHours.Where(t => t.CompletedDate.Month == month).Where(t => t.CompletedDate.Year == year).Sum(t => t.WorkedHours);
                ViewBag.TotalWorkHoursMonth = totalWorkHoursMonth;
            }
            else
                model.EmployeeHours = null;

            var totalHours = model.Tasks.Sum(x => x.WorkedHours);
            model.TotalHours = totalHours;

            return View(model);
        }

        public async Task<IActionResult> GetTasksForMonth(string userId, int month, int year)
        {
            return RedirectToAction("Details", "Employees", new { UserId = userId, Month = month, Year = year });
        }
    }
}
