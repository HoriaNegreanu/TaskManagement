using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Models
{
    public class TaskStage
    {
        public int ID { get; set; }

        [StringLength(50)]
        [Required]
        public string? Title { get; set; }

        [StringLength(100)]
        public string? Message { get; set; }

        public int TaskItemID { get; set; }
        public TaskItem? TaskItem { get; set; }

        [RegularExpression("Proposed|Completed", ErrorMessage = "The status of a stage can only be 'Proposed' or 'Completed'")]
        public string? Status { get; set; }
    }

    public enum StageStatus
    {
        Proposed,
        Completed
    }
}
