namespace TaskManagement.Models
{
    public class TaskItemViewModel
    {
        public TaskItem TaskItem { get; set; }
        public List<Comment> Comments { get; set; }
        //public string Author { get; set; }
        public string Message { get; set; }
        public int TaskItemID { get; set; }
    }
}
