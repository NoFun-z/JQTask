using Microsoft.EntityFrameworkCore;

namespace API.Entities
{
    public class TaskList
    {
        public int ID { get; set; }
        public int TempID { get; set; }
        public string Title { get; set; }
        public int ListBoardID { get; set; }
        public ListBoard ListBoard { get; set; }
        public List<Task> Tasks { get; set; } = new();

        public void AddItem(string Description, string DueDate)
        {
            var task = new Task(Description);

            if (!string.IsNullOrEmpty(DueDate))
            {
                if (DateTime.TryParse(DueDate, out DateTime dueDate))
                {
                    task.DueDate = dueDate;
                }
                else
                {
                    task.DueDate = DateTime.Today.AddDays(4);
                }
            }
            task.TempID = Tasks.Count + 1;
            task.Order = Tasks.Count + 1;
            Tasks.Add(task);
        }


        public void RemoveItem(int ID)
        {
            var task = Tasks.FirstOrDefault(item => item.TempID == ID);
            if (task == null) return;
            Tasks.Remove(task);
        }
    }
}