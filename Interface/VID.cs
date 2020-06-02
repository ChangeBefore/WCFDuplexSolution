using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interface
{
    /// <summary>
    /// VID查询及返回类
    /// </summary>
    public class VID
    {
        public string ID { get; set; }
        public string Value { get; set; }
        public List<Group> Groups { get; set; }
    }
}
