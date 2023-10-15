using Microsoft.EntityFrameworkCore;

namespace API.Entities
{
    public class Task
    {
        public int ID { get; set; }
        public int TempID { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public int Order { get; set; }
        public int TaskListID { get; set; }
        public TaskList TaskList { get; set; }

        public Task(string Description)
        {
            this.Description = Description;
            DueDate = DateTime.Today.AddDays(4);
        }
    }
}