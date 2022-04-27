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

        public async Task<IActionResult> Details(string userId, int? month)
        {
            var employee = await _userManager.FindByIdAsync(userId);
            if(employee == null)
            {
                return NotFound();
            }
            ViewBag.EmployeeName = employee.FullName;

            //create model
            var taskItems = await _context.TaskItem.Where(t => t.AssignedTo == employee.FullName).Include(t => t.Project).ToListAsync();
            var model = new EmployeesViewModel();
            model.UserId = employee.Id;
            model.FirstName = employee.FirstName;
            model.LastName = employee.LastName;
            model.Tasks = taskItems;
            if(month != null)
                model.TasksMonth = taskItems.FindAll(t => t.CreatedDate.Month == month).ToList();
            var totalHours = model.Tasks.Sum(x => x.WorkedHours);
            model.TotalHours = totalHours;
            ViewBag.TasksList = model.Tasks.ToArray();

            return View(model);
        }

        public async Task<IActionResult> GetTasksForMonth(string userId, int month)
        {
            return RedirectToAction("Details", "Employees", new { UserId = userId, Month = month });
        }
    }
}
