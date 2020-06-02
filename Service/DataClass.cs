using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service
{
    public static class DataClass
    {
        public static Dictionary<string, WcfCleint> dit_callback = new Dictionary<string, WcfCleint>();

        // CIM请求EM信息， 保存的List
        //public static Dictionary<string, string> EMReplyCIMList = new Dictionary<string, string>();
        public static Dictionary<string, CimReplyInfo> EMReplyCIMList = new Dictionary<string, CimReplyInfo>();

        public static ReaderWriterLockSlim LogWriteLock = new ReaderWriterLockSlim();

        public static Object lockObje = new Object();

    }

    public class CimReplyInfo
    {
        public string replyInfo { get; set; }

        public DateTime replyTime { get; set; }
    }
}
