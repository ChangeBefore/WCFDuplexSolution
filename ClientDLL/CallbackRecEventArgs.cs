using Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDLL
{
    public class CallbackRecEventArgs : EventArgs
    {
        public CallbackRecEventArgs(string sendName, string revName, MessageBase sendContent, DateTime Time)
        {
            SendName = sendName;
            RevName = revName;
            SendContent = sendContent;
            SendTime = Time;
        }
        /// <summary>
        /// 发送者姓名
        /// </summary>
        public string SendName { get; set; }
        /// <summary>
        /// 接受者姓名
        /// </summary>
        public string RevName { get; set; }
        /// <summary>
        /// 发送内容
        /// </summary>
        public MessageBase SendContent { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime { get; set; }
    }
}
