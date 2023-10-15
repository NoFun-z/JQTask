using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class ListBoardController : BaseApiController
    {
        private readonly JContext _context;
        private readonly UserManager<User> _userManager;

        public ListBoardController(JContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet(Name = "GetListBoards")]
        public async Task<ActionResult<List<ListBoardDTO>>> GetListBoards()
        {
            var listBoards = await RetrieveListBoards(GetBuyerId());

            if (listBoards == null) return NotFound();

            return listBoards.Select(l => l.MapListBoardToDTO()).ToList();
        }

        [HttpPost]
        public async Task<ActionResult<ListBoard>> CreateListBoardAsync([FromBody] string Name)
        {
            //Find existing listboard with matching name
            var existedListBoards = await RetrieveListBoards(GetBuyerId());
            if (existedListBoards.Any(el => el.Name == Name))
            {
                return BadRequest(new ProblemDetails { Title = "This ListBoard Name already existed" });
            }

            var buyerId = User.Identity?.Name;
            if (string.IsNullOrEmpty(buyerId))
            {
                buyerId = Guid.NewGuid().ToString();
                var cookieOptions = new CookieOptions { IsEssential = true, Expires = DateTime.Now.AddDays(30) };
                Response.Cookies.Append("buyerId", buyerId, cookieOptions);
            }

            var user = await _userManager.FindByNameAsync(buyerId);

            BuyerID buyerID = new BuyerID { UserName = buyerId, UserID = user.Email };

            var listBoardToAdd = new ListBoard
            {
                Name = Name,
                BuyerIDs = new List<BuyerID>(),
                Tasklists = new List<TaskList>()
            };

            listBoardToAdd.BuyerIDs.Add(buyerID);

            _context.ListBoards.Add(listBoardToAdd);

            var result = await _context.SaveChangesAsync() > 0;
            var listBoard = listBoardToAdd.MapListBoardToDTO();

            if (result)
            {
                return CreatedAtRoute("GetListBoards", new { Id = listBoard.ID }, listBoard);
            };

            return BadRequest(new ProblemDetails { Title = "Problem creating new listboard" });
        }

        [Authorize]
        [HttpPost("AddTaskList")]
        public async Task<ActionResult<ListBoardDTO>> AddTaskListToListBoard(AddTaskListDTO addTaskListDTO)
        {
            var listBoards = await RetrieveListBoards(GetBuyerId());
            ListBoard listBoard = listBoards.Find(l => l.ID == addTaskListDTO.ListBoardID);
            if (listBoard == null) return NotFound();

            var existedTaskList = listBoard.Tasklists;
            if (existedTaskList.Any(tl => tl.Title == addTaskListDTO.Title))
            {
                return BadRequest(new ProblemDetails { Title = "This TaskList title already existed" });
            }

            listBoard.AddItem(addTaskListDTO.Title);

            var result = await _context.SaveChangesAsync() > 0;
            if (result) return CreatedAtRoute("GetListBoards", listBoard.MapListBoardToDTO());

            return BadRequest(new ProblemDetails { Title = "Problem saving tasklist to listboard" });
        }

        [Authorize]
        [HttpPost("AddUsers")]
        public async Task<ActionResult<ListBoardDTO>> AddUsers(AddUserDTO addUserDTO)
        {
            //if (BuyerID == null) return BadRequest(new ProblemDetails { Title = "Invalid Input, member email cannot be null" });

            var listBoards = await RetrieveListBoards(GetBuyerId());
            ListBoard listBoard = listBoards.Find(l => l.ID == addUserDTO.ListBoardID);
            if (listBoard == null) return NotFound();

            var user = await _userManager.FindByEmailAsync(addUserDTO.BuyerID);
            if (user == null) return BadRequest(new ProblemDetails
            { Title = "Cannot find any user with the specified email" });

            var verifyListBoardWithNewUser = listBoard.BuyerIDs.Any(buyerID => buyerID.UserID.Contains(addUserDTO.BuyerID));

            if (verifyListBoardWithNewUser) return BadRequest(new ProblemDetails { Title = "Member already in the listboard" });

            if (user != null && !verifyListBoardWithNewUser)
            {
                listBoard.AddUser(user.UserName, user.Email);
                var result = await _context.SaveChangesAsync() > 0;
                if (result) return CreatedAtRoute("GetListBoards", listBoard.MapListBoardToDTO());
            }

            return BadRequest(new ProblemDetails { Title = "Problem saving tasklist to listboard" });
        }

        [Authorize]
        [HttpDelete("RemoveUser")]
        public async Task<ActionResult> RemoveUser(AddUserDTO removeUserDTO)
        {
            var listBoards = await RetrieveListBoards(GetBuyerId());
            ListBoard listBoard = listBoards.Find(l => l.ID == removeUserDTO.ListBoardID);
            if (listBoard == null) return NotFound();

            listBoard.RemoveUser(removeUserDTO.BuyerID);

            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok();

            return BadRequest(new ProblemDetails { Title = "Problem removing tasklist from listboard" });
        }

        [Authorize]
        [HttpPost("AddTask")]
        public async Task<ActionResult<ListBoardDTO>> AddTaskToTaskList(AddTaskDTO addTaskDTO)
        {
            var listBoards = await RetrieveListBoards(GetBuyerId());
            ListBoard listBoard = listBoards.Find(l => l.ID == addTaskDTO.ListBoardID);
            if (listBoard == null) return NotFound();

            var tasklist = listBoard.Tasklists.Find(t => t.TempID == addTaskDTO.TaskListID);
            if (tasklist == null) return NotFound();

            tasklist.AddItem(addTaskDTO.Description, addTaskDTO.DueDate);

            var result = await _context.SaveChangesAsync() > 0;
            if (result) return CreatedAtRoute("GetListBoards", listBoard.MapListBoardToDTO());

            return BadRequest(new ProblemDetails { Title = "Problem saving tasklist to listboard" });
        }

        [Authorize]
        [HttpPut("EditTask")]
        public async Task<ActionResult<ListBoardDTO>> EditListBoardTask(UpdateTaskDTO updateTaskDTO)
        {
            var listBoards = await RetrieveListBoards(GetBuyerId());
            ListBoard listBoard = listBoards.Find(l => l.ID == updateTaskDTO.ListBoardID);
            if (listBoard == null) return NotFound();

            var tasklist = listBoard.Tasklists.Find(t => t.TempID == updateTaskDTO.TaskListID);
            if (tasklist == null) return NotFound();

            var task = tasklist.Tasks.Find(t => t.TempID == updateTaskDTO.TaskID);
            if (task == null) return NotFound();

            if (updateTaskDTO.Description != null && updateTaskDTO.DueDate != null)
            {
                task.Description = updateTaskDTO.Description;
                task.DueDate = DateTime.Parse(updateTaskDTO.DueDate);
            }
            else
            {
                return BadRequest(new ProblemDetails { Title = "Invalid values, description and duedate cannot be null" });
            }

            var result = await _context.SaveChangesAsync() > 0;
            if (result) return CreatedAtRoute("GetListBoards", listBoard.MapListBoardToDTO());

            return BadRequest(new ProblemDetails { Title = "Problem Editting task" });
        }


        [Authorize]
        [HttpPut("SortTasksByDueDate")]
        public async Task<ActionResult<ListBoardDTO>> SortListBoardTasksByDueDate(SortTasksByDueDateDTO sortTasksByDueDateDTO)
        {
            var listBoards = await RetrieveListBoards(GetBuyerId());
            ListBoard listBoard = listBoards.Find(l => l.ID == sortTasksByDueDateDTO.ListBoardID);
            if (listBoard == null) return NotFound();

            var tasklist = listBoard.Tasklists.Find(t => t.TempID == sortTasksByDueDateDTO.TaskListID);
            if (tasklist == null) return NotFound();

            if (sortTasksByDueDateDTO.SortTerm == "SortByDueDate Asc")
            {
                tasklist.Tasks = tasklist.Tasks.OrderBy(d => d.DueDate).ToList();
            }
            else if (sortTasksByDueDateDTO.SortTerm == "SortByDueDate Desc")
            {
                tasklist.Tasks = tasklist.Tasks.OrderByDescending(d => d.DueDate).ToList();
            }

            // Update the order of items based on the new order list
            for (int i = 0; i < tasklist.Tasks.Count; i++)
            {
                int itemId = tasklist.Tasks[i].ID;
                Entities.Task item = tasklist.Tasks.FirstOrDefault(x => x.ID == itemId);
                if (item != null)
                {
                    item.Order = i + 1;
                }
            }

            var result = await _context.SaveChangesAsync() > 0;
            if (result) return CreatedAtRoute("GetListBoards", listBoard.MapListBoardToDTO());

            return BadRequest(new ProblemDetails { Title = "Problem sorting tasks from listboard's tasklist" });
        }


        [Authorize]
        [HttpPut("SortTasks")]
        public async Task<ActionResult<ListBoardDTO>> ReorderListBoardsTasks(ReOrderTaskDTO reOrderTaskDTO)
        {
            var listBoards = await RetrieveListBoards(GetBuyerId());
            ListBoard listBoard = listBoards.Find(l => l.ID == reOrderTaskDTO.ListBoardID);
            if (listBoard == null) return NotFound();

            var tasklist = listBoard.Tasklists.Find(t => t.TempID == reOrderTaskDTO.TaskListID);
            if (tasklist == null) return NotFound();

            // Update the order of items based on the new order list
            for (int i = 0; i < reOrderTaskDTO.NewOrder.Length; i++)
            {
                int itemId = reOrderTaskDTO.NewOrder[i];
                Entities.Task item = tasklist.Tasks.FirstOrDefault(x => x.TempID == itemId);
                if (item != null)
                {
                    item.Order = i + 1;
                }
            }

            var result = await _context.SaveChangesAsync() > 0;
            if (result) return CreatedAtRoute("GetListBoards", listBoard.MapListBoardToDTO());

            return BadRequest(new ProblemDetails { Title = "Problem sorting tasks from listboard's tasklist" });
        }


        [Authorize]
        [HttpDelete("RemoveTaskList")]
        public async Task<ActionResult> RemoveTaskList(RemoveTaskListDTO removeTaskListDTO)
        {
            var listBoards = await RetrieveListBoards(GetBuyerId());
            ListBoard listBoard = listBoards.Find(l => l.ID == removeTaskListDTO.ListBoardID);
            if (listBoard == null) return NotFound();

            listBoard.RemoveItem(removeTaskListDTO.TaskListID);

            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok();

            return BadRequest(new ProblemDetails { Title = "Problem removing tasklist from listboard" });
        }

        [Authorize]
        [HttpDelete("RemoveTask")]
        public async Task<ActionResult> RemoveTask(UpdateTaskDTO deleteTaskDTO)
        {
            var listBoards = await RetrieveListBoards(GetBuyerId());
            ListBoard listBoard = listBoards.Find(l => l.ID == deleteTaskDTO.ListBoardID);
            if (listBoard == null) return NotFound();

            var tasklist = listBoard.Tasklists.Find(t => t.TempID == deleteTaskDTO.TaskListID);
            if (tasklist == null) return NotFound();

            tasklist.RemoveItem(deleteTaskDTO.TaskID);

            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok();

            return BadRequest(new ProblemDetails { Title = "Problem removing task from tasklist" });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteListBoard(int id)
        {
            var listBoard = await _context.ListBoards.FindAsync(id);

            if (listBoard == null)
            {
                return NotFound();
            }

            _context.ListBoards.Remove(listBoard);

            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                return Ok();
            }

            return BadRequest(new ProblemDetails { Title = "Problem deleting listboard" });
        }


        private async Task<List<ListBoard>> RetrieveListBoards(string buyerId)
        {
            if (string.IsNullOrEmpty(buyerId))
            {
                Response.Cookies.Delete("buyerId");
                return null;
            }

            var user = await _userManager.FindByNameAsync(buyerId);

            var listboards = await _context.ListBoards
                .Include(x => x.BuyerIDs)
                .Include(x => x.Tasklists)
                .ThenInclude(x => x.Tasks)
                .Where(c => c.BuyerIDs.Any(bid => bid.UserID == user.Email))
                .ToListAsync();

            foreach (var lb in listboards)
            {
                foreach (var tl in lb.Tasklists)
                {
                    tl.Tasks = tl.Tasks.OrderBy(o => o.Order).ToList();
                }
            }

            return listboards;
        }

        private string GetBuyerId()
        {
            return User.Identity?.Name ?? Request.Cookies["buyerId"];
        }
    }
}