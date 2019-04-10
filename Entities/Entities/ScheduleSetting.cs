using System;

namespace Entities.Entities
{
    public class ScheduleSetting
    {
        public string TimeZone { get; set; }
        public string Shift { get; set; }
        public string StartTime { get; set; }
        public string TeamManager { get; set; }
        public string Activity { get; set; }
        public bool AutoSubmit { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
