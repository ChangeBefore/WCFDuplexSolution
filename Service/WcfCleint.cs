using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interface;
using System.ServiceModel;

namespace Service
{
    public class WcfCleint
    {
        public DateTime NowdateTime { get; set; }
        public ICallback callbackHandler { get; set; }

        public WcfCleint(ICallback callback, DateTime nowTime)
        {
            this.callbackHandler = callback;
            this.NowdateTime = nowTime;
        }
    }
}
