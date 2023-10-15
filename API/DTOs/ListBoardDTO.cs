namespace API.DTOs
{
    public class ListBoardDTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<BuyerIDDTO> BuyerIDs { get; set; }
        public List<TaskListDTO> Tasklists { get; set; }
    }
}