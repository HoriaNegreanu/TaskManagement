using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.Models
{
    public class EmployeeHour
    {
        public int ID { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal WorkedHours { get; set; }
        public string UserID { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public int TaskItemID { get; set; }
        public virtual TaskItem? TaskItem { get; set; }

        [DataType(DataType.Date)]
        public DateTime CompletedDate { get; set; }
    }
}
