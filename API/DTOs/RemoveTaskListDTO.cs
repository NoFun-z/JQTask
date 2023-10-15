using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class RemoveTaskListDTO
    {
        public int ListBoardID { get; set; }
        public int TaskListID { get; set; }
    }
}