using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.Models
{
    public class Comment
    {
        public int ID { get; set; }

        [StringLength(500)]
        [Required]
        public string Message { get; set; }

        [StringLength(50)]
        [Required]
        public string Author { get; set; }

        public int TaskItemID { get; set; }

        public TaskItem? TaskItem { get; set; }
    }
}
