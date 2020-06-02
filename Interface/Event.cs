using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interface
{
    /// <summary>
    /// EventID
    /// </summary>
    public class Event
    {
        public string ID { get; set; }

        public List<VID> VIDS { get; set; }
    }
}
