using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class DailyRecord
    {
        public string UserName { get; set; }
        public DateTime Date { get; set; }
        public string Shift { get; set; }
        public string Activity { get; set; }
        public DateTime TimeIn { get; set; }
        public DateTime TimeOut { get; set; }
        public int Breaktime { get; set; }
        public int MealAllowance { get; set; }
        public int TranspoAllowance { get; set; }
    }
}
