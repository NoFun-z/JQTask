using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Entities
{
    public class BuyerID
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string UserID { get; set; }
        public int ListBoardID { get; set; }
        public ListBoard ListBoard { get; set; }
    }
}