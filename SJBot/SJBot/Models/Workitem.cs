using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SJBot.Models
{
    public class Workitem
    {
        public string Customer { get; set; }
        public string Object { get; set; }
        public DateTime? Date { get; set; }
        public int? Hours { get; set; }      
        public string Description { get; set; }
        public string Owner { get; set; }

        public Workitem()
        { }

        public Workitem(string _customer, string _object, DateTime _date, int _hours, string _description, string _owner)
        {
            Customer = _customer;
            Object = _object;
            Date = _date;
            Hours = _hours;
            Description = _description;           
            Owner = _owner;
        }
    }
}
