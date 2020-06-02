using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interface
{
    public class MessageBase
    {
        public string Name { get; set; }
        public string TransactionId { get; set; }
        public string ResultType { get; set; }
        public string EMID { get; set; }
        public string EID { get; set; }
        public List<string> EIDS { get; set; }
        public ContentBase Content { get; set; }
        public string CommandName { get; set; }

        private bool isRecv = false;
        /// <summary>
        /// 是否接收
        /// </summary>
        public bool IsRecv
        {
            get
            {
                return isRecv;
            }
            set
            {
                isRecv = value;
            }
        }

        public string ErrorMessage { get; set; }
    }
}
