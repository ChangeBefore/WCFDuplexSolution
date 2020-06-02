using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Interface
{
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void ShowMessage(string sendName, string recName, MessageBase sendContent, DateTime sendTime);
    }
}
