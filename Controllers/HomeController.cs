using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TaskManagement.Models;

namespace TaskManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public async Task<IActionResult> ViewAllTaskForUser()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var name = user.FirstName + " " + user.LastName;
            return RedirectToAction("Index", "TaskItems", new { TaskAssignedTo = name });
        }

        [Authorize]
        public async Task<IActionResult> ViewUserActivity()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return RedirectToAction("Details", "Employees", new { UserId = user.Id });
        }
    }
}