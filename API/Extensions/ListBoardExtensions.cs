using API.DTOs;
using API.Entities;

namespace API.Extensions
{
    public static class BasketExtensions
    {
        //Mapping basket to basketDTO
        public static ListBoardDTO MapListBoardToDTO(this ListBoard listBoard)
        {
            return new ListBoardDTO
            {
                ID = listBoard.ID,
                Name = listBoard.Name,
                BuyerIDs = listBoard.BuyerIDs.Select(user => new BuyerIDDTO{
                    UserName = user.UserName,
                    UserID = user.UserID
                }).ToList(),
                Tasklists = listBoard.Tasklists.Select(item => new TaskListDTO
                {
                    TempID = item.TempID,
                    Title = item.Title,
                    Tasks = item.Tasks.Select(task => new TaskDTO
                    {
                        TempID = task.TempID,
                        Description = task.Description,
                        DueDate = task.DueDate,
                        Order = task.Order
                    }).ToList()
                }).ToList()
            };
        }
    }
}