using Microsoft.AspNetCore.Mvc.Rendering;

namespace TaskManagement.Models
{
    public class TaskItemFiltersViewModel
    {
        public List<TaskItem>? TaskItems { get; set; }
        public SelectList? Priorities { get; set; }
        public SelectList? Projects { get; set; }
        public SelectList? Users { get; set; }
        public SelectList? Statuses { get; set; }
        public string? TaskPriority { get; set; }
        public int? TaskProject { get; set; }
        public string? SearchTitle { get; set; }
        public string? TaskAssignedTo {get; set; }
        public string? TaskStatus {get; set; }
    }
}
