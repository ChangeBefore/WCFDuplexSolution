using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Interface
{
    [ServiceContract(SessionMode = SessionMode.Required,
        CallbackContract = typeof(ICallback))]
    public interface IPushMessage
    {
        [OperationContract(IsOneWay = true)]
        void Login(string UID);

        [OperationContract(IsOneWay = true)]
        void Update(string UID);

        [OperationContract(IsOneWay = true)]
        void Leave(string UID);

        [OperationContract(IsOneWay = true)]
        void netSendMessage(SendInfo sendInfo);

        [OperationContract(IsOneWay = true)]
        void javaSendMessage(SendInfo sendInfo);

        //[OperationContract]
        //void Login(string UID);

        //[OperationContract]
        //void Update(string UID);

        //[OperationContract]
        //void Leave(string UID);

        //[OperationContract]
        //void netSendMessage(SendInfo sendInfo);

        //[OperationContract]
        //void javaSendMessage(SendInfo sendInfo);

        [OperationContract]
        string GetEMReplyInfoById(string TransactionId, string SendName, string RecvName);

        [OperationContract]
        bool CheckRegister(string SendName);

    }
}
