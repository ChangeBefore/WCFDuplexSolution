using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ServiceModel.Description;
using System.ServiceModel;

namespace Service
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(PushMessageServer)))
            {
                host.Open();
                Console.WriteLine("WCF心跳包实现开始监听");
                PushMessageServer.StartListenClients();
                int i = 0;
                while (true)
                {
                    System.Threading.Thread.Sleep(2000);
                    i++;
                }
                Console.Read();
                host.Abort();
                host.Close();

                
            }
        }
    }
}
