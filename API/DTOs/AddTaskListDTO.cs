using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class AddTaskListDTO
    {
        public int ListBoardID { get; set; }
        public string Title { get; set; }
    }
}