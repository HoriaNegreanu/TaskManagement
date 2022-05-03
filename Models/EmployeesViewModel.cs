using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Models
{
    public class EmployeesViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public string FullName { get; set; }
        public List<TaskItem> Tasks { get; set; }

        [Display(Name = "Total Worked Hours")]
        public decimal? TotalHours { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public List<EmployeeHour>? EmployeeHours { get; set; }
        public List<EmployeeHour>? EmployeeHoursDistinct { get; set; }
        public string? SearchName { get; set; }
    }
}
