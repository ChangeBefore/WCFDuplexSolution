using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interface
{
    /// <summary>
    /// Detail查询及返回类
    /// </summary>
    public class Detail
    {
        public List<Parameter> Parameters { get; set; }

        public List<Wire> Wires { get; set; }        
    }
}
