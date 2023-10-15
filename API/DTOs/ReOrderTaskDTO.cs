namespace API.DTOs
{
    public class ReOrderTaskDTO
    {
        public int ListBoardID { get; set; }
        public int TaskListID { get; set; }
        public int[] NewOrder { get; set; }
    }
}