namespace API.Entities
{
    public class ListBoard
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<BuyerID> BuyerIDs { get; set; } = new();
        public List<TaskList> Tasklists { get; set; } = new();

        public void AddItem(string title)
        {
            var taskList = new TaskList
            {
                TempID = Tasklists.Count + 1,
                Title = title,
                Tasks = new List<Task>()
            };

            Tasklists.Add(taskList);
        }

        public void AddUser(string BuyerName, string BuyerID)
        {
            var buyerID = new BuyerID
            {
                UserName = BuyerName,
                UserID = BuyerID
            };

            BuyerIDs.Add(buyerID);
        }

        public void RemoveUser(string BuyerID)
        {
            var user = BuyerIDs.FirstOrDefault(user => user.UserID == BuyerID);
            if (user == null) return;
            BuyerIDs.Remove(user);
        }

        public void RemoveItem(int taskListID)
        {
            var tasklist = Tasklists.FirstOrDefault(item => item.TempID == taskListID);
            if (tasklist == null) return;
            Tasklists.Remove(tasklist);
        }
    }
}