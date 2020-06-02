using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Interface;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Threading;
using System.Xml;
using System.Net;
using System.Collections;
using System.IO;

namespace Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class PushMessageServer : IPushMessage
    {
        public void Login(string UID)
        {
            SysLog("用户:" + UID + "上线", "ContentInfo");
            lock (DataClass.lockObje)
            {
                if (!DataClass.dit_callback.ContainsKey(UID))
                    DataClass.dit_callback.Add(UID, new WcfCleint(OperationContext.Current.GetCallbackChannel<ICallback>(), DateTime.Now));
                else
                    DataClass.dit_callback[UID] = new WcfCleint(OperationContext.Current.GetCallbackChannel<ICallback>(), DateTime.Now);
            }
        }

        public void Update(string UID)
        {
            SysLog("用户:" + UID + "心跳更新！", "ContentInfo");
            ICallback callback = OperationContext.Current.GetCallbackChannel<ICallback>();            
            lock (DataClass.lockObje)
            {
                if (DataClass.dit_callback.ContainsKey(UID))
                {
                    var wcfClient = DataClass.dit_callback[UID];
                    wcfClient.NowdateTime = DateTime.Now;
                }
                else
                {
                    DataClass.dit_callback.Add(UID, new WcfCleint(OperationContext.Current.GetCallbackChannel<ICallback>(), DateTime.Now));
                }
                //if (DataClass.dit_callback.ContainsKey(UID))
                //{
                //    DataClass.dit_callback.Remove(UID);
                //}
                //DataClass.dit_callback.Add(UID, new WcfCleint(OperationContext.Current.GetCallbackChannel<ICallback>(), DateTime.Now));
            }
        }

        public void Leave(string UID)
        {
            SysLog("用户:" + UID + "退出！", "ContentInfo");
            lock (DataClass.lockObje)
            {
                DataClass.dit_callback.Remove(UID);
            }
        }

        /// <summary>
        /// net端发送信息
        /// </summary>
        /// <param name="sendInfo">信息详情</param>
        public void netSendMessage(SendInfo sendInfo)
        {
            string sendName = sendInfo.SendName;

            WcfCleint cb1 = null;
            try
            {
                string recInfo = string.Format("Recieve Content：{0}", JsonConvert.SerializeObject(sendInfo, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }));
                SysLog(recInfo, "ContentInfo");

                lock (DataClass.lockObje)
                {
                    if (!DataClass.dit_callback.ContainsKey(sendName))
                    {
                        throw new Exception(string.Format("netSendMessage_{0}没有注册！", sendName));
                    }
                    cb1 = DataClass.dit_callback[sendName];

                    if (sendInfo.SendContent == null) return;
                    if (sendInfo.SendContent.IsRecv)
                    {
                        // CIM请求EM信息， 由EM回复信息
                        if (!DataClass.EMReplyCIMList.ContainsKey(sendInfo.SendContent.TransactionId))
                        {
                            DataClass.EMReplyCIMList.Add(sendInfo.SendContent.TransactionId, new CimReplyInfo() { replyInfo = JsonConvert.SerializeObject(sendInfo.SendContent, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }), replyTime = DateTime.Now });
                        }
                    }
                }

                if (!sendInfo.SendContent.IsRecv)
                {
                    // EM请求CIM信息
                    MessageBase recvMessage = GetMessageResultFromJava(sendInfo.SendContent);

                    string tmpStr = sendInfo.RevName;
                    sendInfo.RevName = sendInfo.SendName;
                    sendInfo.SendName = tmpStr;
                    sendInfo.SendContent = recvMessage;
                    sendInfo.SendTime = DateTime.Now;

                    if (sendInfo.SendContent != null)
                    {
                        recInfo = string.Format("Send Content：{0}", JsonConvert.SerializeObject(sendInfo, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }));
                        SysLog(recInfo, "ContentInfo");

                        //WcfCleint cb1 = DataClass.dit_callback[sendName];
                        cb1.callbackHandler.ShowMessage(sendInfo.SendName, sendInfo.RevName, sendInfo.SendContent, sendInfo.SendTime);
                    }
                }
            }
            catch (Exception ex)
            {
                SysLog(ex.Message, "Error");
                sendInfo.SendContent.ErrorMessage = ex.Message;

                cb1 = new WcfCleint(OperationContext.Current.GetCallbackChannel<ICallback>(), DateTime.Now);
                cb1.callbackHandler.ShowMessage(sendInfo.SendName, sendInfo.RevName, sendInfo.SendContent, sendInfo.SendTime);
            }
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="DateStr"></param>
        /// <param name="recInfo"></param>
        /// <param name="LogType"></param>
        public static void SysLog(string recInfo, string LogType)
        {
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //根据Key读取<add>元素的Value
            string IsLog = config.AppSettings.Settings["Log"].Value;
            if (IsLog != "1") return;

            string DateStr = DateTime.Now.Date.ToString("yyyy-MM-dd");
            DataClass.LogWriteLock.EnterWriteLock();

            try
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(string.Format("{0}\\Log\\{1}_{2}.txt", AppDomain.CurrentDomain.BaseDirectory, DateStr, LogType), true))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ：" + recInfo);
                }
            }
            catch (Exception ex)
            {
                //throw ;
            }
            finally
            {
                DataClass.LogWriteLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// java端发送信息
        /// </summary>
        /// <param name="sendInfo">信息详情</param>
        public void javaSendMessage(SendInfo sendInfo)
        {
            try
            {
                string recInfo = string.Format("Recieve Content：{0}", JsonConvert.SerializeObject(sendInfo, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }));
                SysLog(recInfo, "ContentInfo");

                lock (DataClass.lockObje)
                {
                    if (!DataClass.dit_callback.ContainsKey(sendInfo.RevName))
                    {
                        throw new Exception(string.Format("javaSendMessage_{0}没有注册！", sendInfo.RevName));
                    }

                    if (sendInfo.SendContent != null)
                    {
                        recInfo = string.Format("Send Content：{0}", JsonConvert.SerializeObject(sendInfo, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }));
                        SysLog(recInfo, "ContentInfo");

                        WcfCleint cb1 = DataClass.dit_callback[sendInfo.RevName];
                        cb1.callbackHandler.ShowMessage(sendInfo.SendName, sendInfo.RevName, sendInfo.SendContent, sendInfo.SendTime);
                    }
                }
            }
            catch (Exception ex)
            {
                SysLog(ex.Message, "Error");
                sendInfo.SendContent.ErrorMessage = ex.Message;

                WcfCleint cb1 = new WcfCleint(OperationContext.Current.GetCallbackChannel<ICallback>(), DateTime.Now);
                cb1.callbackHandler.ShowMessage(sendInfo.SendName, sendInfo.RevName, sendInfo.SendContent, sendInfo.SendTime);
            }
        }

        /// <summary>
        /// 将EM传送信息发给JAVA得到接收信息
        /// </summary>
        /// <param name="messageBase"></param>
        /// <returns></returns>
        public MessageBase GetMessageResultFromJava(MessageBase messageSend)
        {
            MessageBase messageResult = null;

            try
            {
                string jsonStr = JsonObjectConvert.JsonSerialize(messageSend);
                
                //获取Configuration对象
                Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                //根据Key读取<add>元素的Value
                string url = config.AppSettings.Settings["JavaServicePath"].Value;
                string methodName = config.AppSettings.Settings["MethodName"].Value;

                string[] args = new string[1];
                args[0] = jsonStr;
                object resultObj = WebServiceHelper.InvokeWebService(url, methodName, args);
                if (resultObj == null)
                    throw new Exception(string.Format("调用URL：{0}， 方法：{1}， 参数：{2}， 没有回传值！", url, methodName, jsonStr));

                string resultStr = resultObj.ToString();

                messageResult = JsonObjectConvert.JsonDeserialize<MessageBase>(resultStr);
                messageResult.IsRecv = true; // 回复给EM
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return messageResult;
        }

        public static void StartListenClients()
        {
            System.Timers.Timer timer1 = new System.Timers.Timer();
            timer1.Interval = 300000; // 5分钟
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(time_EventArgs);
            timer1.Start();
        }

        static void time_EventArgs(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (DataClass.lockObje)
            {
                foreach (string key in new List<string>(DataClass.dit_callback.Keys))
                {
                    if (DataClass.dit_callback[key].NowdateTime.AddMinutes(30) < DateTime.Now)
                    {
                        DataClass.dit_callback.Remove(key);
                        SysLog("脱机用户" + key, "ContentInfo");
                    }                    
                }

                foreach (string key in new List<string>(DataClass.EMReplyCIMList.Keys))
                {
                    if (DataClass.EMReplyCIMList[key].replyTime.AddMinutes(10) < DateTime.Now)
                    {
                        DataClass.EMReplyCIMList.Remove(key);
                        SysLog("删除EM返回的超时数据" + key, "ContentInfo");
                    }
                }
            }
        }

        /// <summary>
        /// 获取从EM接收的信息
        /// </summary>
        /// <param name="TransactionId"></param>
        /// <returns></returns>
        public string GetEMReplyInfoById(string TransactionId, string SendName, string RecvName)
        {
            string resultStr = null;

            if (!DataClass.dit_callback.ContainsKey(RecvName))
            {
                throw new Exception(string.Format("GetEMReplyInfoById_{0}没有注册, 请确认！", RecvName));
            }

            lock (DataClass.lockObje)
            {
                if (DataClass.EMReplyCIMList.ContainsKey(TransactionId))
                {
                    resultStr = DataClass.EMReplyCIMList[TransactionId].replyInfo;
                    DataClass.EMReplyCIMList.Remove(TransactionId);
                }
            }
            return resultStr;
        }

        public bool CheckRegister(string SendName)
        {
            bool flag = false;
            lock (DataClass.lockObje)
            {
                if (DataClass.dit_callback.ContainsKey(SendName))
                    flag = true;
            }

            return flag;
        }
    }
}
