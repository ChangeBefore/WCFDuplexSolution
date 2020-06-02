using Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDLL
{
    public class CallServer : ICallback
    {
        public event EventHandler<CallbackRecEventArgs> ChatEvent;
        public void ShowMessage(string sendName, string recName, MessageBase sendContent, DateTime sendTime)
        {
            CallbackRecEventArgs crg = new CallbackRecEventArgs(sendName, recName, sendContent, sendTime);
            ChatEvent(this, crg);
        }
    }
}
