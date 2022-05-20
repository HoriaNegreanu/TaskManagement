
namespace TaskManagement.Models
{
    public class TaskItemViewModel
    {
        public TaskItem TaskItem { get; set; }
        public List<Comment> Comments { get; set; }
        public List<TaskStage> TaskStages { get; set; }
        public string Message { get; set; }
        public int TaskItemID { get; set; }
        public List<FileOnFileSystemModel> FilesOnFileSystem { get; set; }
        public string Title { get; set; }
    }
}
