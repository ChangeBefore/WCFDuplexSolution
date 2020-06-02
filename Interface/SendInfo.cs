using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interface
{
    public class SendInfo
    {
        /// <summary>
        /// 发送者名字
        /// </summary>
        public string SendName { get; set; }
        /// <summary>
        /// 接受者名字
        /// </summary>
        public string RevName { get; set; }
        /// <summary>
        ///  发送内容
        /// </summary>
        public MessageBase SendContent { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime { get; set; }

    }
}
