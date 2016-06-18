using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Runtime.Serialization;

namespace Scheduler
{
    [DataContractAttribute]
    class JakiTydzien
    {
        public string tydzien { get; set; }
        public int expires { get; set; }
        public string details { get; set; }
    }
}