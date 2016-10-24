using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkTimer.Models
{
    public class TimeRecord
    {
        public DateTime Beginning { get; set; }  // Work beginning
        public DateTime End { get; set; }        // Work end
        public TimeSpan Duration { get; set; }   // Work time
        public string Description { get; set; }  // Work summary
    }
}
