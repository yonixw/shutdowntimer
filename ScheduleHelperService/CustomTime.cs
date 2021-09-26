using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleHelperService
{
    public class CustomTime
    {
        public string CronString { get; set; }
        public string ProgramID { get; set; }
        public string AdditionalArgs { get; set; }
    }
}
