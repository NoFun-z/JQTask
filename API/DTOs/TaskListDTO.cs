namespace API.DTOs
{
    public class TaskListDTO
    {
        public int TempID { get; set; }
        public string Title { get; set; }
        public List<TaskDTO> Tasks { get; set; } = new();
    }
}