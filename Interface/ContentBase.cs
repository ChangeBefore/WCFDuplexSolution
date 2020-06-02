using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interface
{
    /// <summary>
    /// ContentBase查询及返回类
    /// </summary>
    public class ContentBase
    {
        public string Status { get; set; }
        public string StripId { get; set; }
        public string MapString { get; set; }
        public string Message { get; set; }
        public List<VID> VIDS { get; set; }
        public List<Event> Events { get; set; }
        public List<Alarm> Alarms { get; set; }
        public List<EC> ECs { get; set; }
        public List<Recipe> Recipes { get; set; }
        public List<Detail> Details { get; set; }
        /// <summary>
        /// 数据校准
        /// </summary>
        public List<Calibration> Calibrations { get; set; }
    }
}
