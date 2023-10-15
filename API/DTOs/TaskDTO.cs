namespace API.DTOs
{
    public class TaskDTO
    {
        public int TempID { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public int Order { get; set; }
    }
}