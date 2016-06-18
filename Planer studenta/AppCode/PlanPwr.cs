using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net;
using System.Windows;

namespace Planer_studenta
{
    public class PlanPwrWrapper
    {
        public string year { get; set; }
        public int? semester { get; set; }
        public List<PlanPwrEntry> entries { get; set; }
    }

    public class PlanPwrEntry
    {
        public string course_code { get; set; }
        public string course_name { get; set; }
        public string course_type { get; set; }

        public string group_code { get; set; }

        public int week { get; set; }
        public int week_day { get; set; }
        public int start_hour { get; set; }
        public int start_min { get; set; }
        public int end_hour { get; set; }
        public int end_min { get; set; }

        public string building { get; set; }
        public string room { get; set; }

        public string lecturer { get; set; }
    }
}
