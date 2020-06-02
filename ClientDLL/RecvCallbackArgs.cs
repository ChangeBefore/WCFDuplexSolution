using Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDLL
{
    public class RecvCallbackArgs : EventArgs
    {
        public RecvCallbackArgs(MessageBase sendContent)
        {
            SendContent = sendContent;
        }
        public MessageBase SendContent { get; set; }
    }
}
