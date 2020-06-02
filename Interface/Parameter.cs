using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interface
{
    /// <summary>
    /// Parameter查询及返回类
    /// </summary>
    public class Parameter
    {
        public string Section { get; set; }

        public string ParameterName { get; set; }        

        public string Min { get; set; }

        public string Max { get; set; }

        public string Value { get; set; }

        public string Unit { get; set; }
    }
}
