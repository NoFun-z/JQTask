using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class SortTasksByDueDateDTO
    {
        public int ListBoardID { get; set; }
        public int TaskListID { get; set; }
        public string SortTerm { get; set; }
    }
}