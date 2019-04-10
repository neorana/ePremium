using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class DailyTimeEntry
    {
        public string Activity { get; set; }
        public int BreakTime { get; set; }
        public DateTime Date { get; set; }
        public string From { get; set; }
        public int Id { get; set; }
        public string LanId { get; set; }
        public double MealAllowance { get; set; }
        public double ND { get; set; }
        public double OTClaim { get; set; }
        public double OTND { get; set; }
        public double OTRD { get; set; }
        public double OTRDND { get; set; }
        public double OTRE { get; set; }
        public double OTRH { get; set; }
        public double OTRHND { get; set; }
        public double OTSH { get; set; }
        public double OTSHND { get; set; }
        public double RD { get; set; }
        public double RDND { get; set; }
        public double RH { get; set; }
        public double RHND { get; set; }
        public string Remarks { get; set; }
        public double SH { get; set; }
        public double SHND { get; set; }
        public string Shift { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string To { get; set; }
        public double Total { get; set; }
        public double TotalAllowance { get; set; }
        public double TranspoAllowance { get; set; }
        public bool WithOR { get; set; }
    }

    public class TimeSheet
    {
        public List<DailyTimeEntry> GetClaimsForCoverageResult { get; set; }
    }
}
