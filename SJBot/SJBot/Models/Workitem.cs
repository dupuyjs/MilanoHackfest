using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SJBot.Models
{
    public class Workitem
    {
        public string  Description { get; set; }
        public int? Hours { get; set; }
        public int? Customerid { get; set; }
        public string  Owner { get; set; }
        public string Object { get; set; }

        public Workitem()
        { }

        public Workitem(string _description, int _hours, int _customerid, string _owner)
        {
            Description = _description;
            Hours = _hours;
            Customerid = _customerid;
            Owner = _owner;
        }
    }
}
