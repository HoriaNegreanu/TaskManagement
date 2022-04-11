using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.Models
{
    public class TaskItem
    {
        public int ID { get; set; }

        [StringLength(50)]
        [Required]
        public string? Title { get; set; }

        [Display(Name = "Created by")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Assigned to")]
        public string? AssignedTo { get; set; }

        [Display(Name = "Created on")]
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Active since")]
        public DateTime? ActivatedDate { get; set; }

        [Display(Name = "Hours worked")]
        [Column(TypeName = "decimal(5, 2)")]
        [Range(0.00,1000,ErrorMessage = "Worked Hours must be a positive number")]
        public decimal WorkedHours { get; set; }

        [RegularExpression("Low|Normal|High", ErrorMessage = "Priority can only have values 'Low', 'Normal' or 'High'")]
        [Required]
        public string? Priority { get; set; }

        [StringLength(1000)]
        [Required]
        public string? Description { get; set; }

        [Required]
        [RegularExpression("Proposed|Active|Review", ErrorMessage = "The status of a task can only be 'Proposed', 'Active' or 'Review'")]
        public string? Status { get; set; }
    }

    public enum Priority
    {
        Low,
        Normal,
        High
    }

    public enum Status
    {
        Proposed,
        Active,
        Review
    }
}
