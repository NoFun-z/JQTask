namespace API.DTOs
{
    public class UserDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public List<ListBoardDTO> ListBoards { get; set; }
    }
}