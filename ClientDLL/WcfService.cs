using Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Converters;

namespace ClientDLL
{
    public class WcfService
    {
        string Url;

        CallServer CB;

        SendInfo sendInfo = new SendInfo();

        //public static Object lockObje = new Object();
        ReaderWriterLockSlim LogWriteLock = new ReaderWriterLockSlim();

        public event EventHandler<RecvCallbackArgs> KEEP_ALIVE;
        public event EventHandler<RecvCallbackArgs> EQP_IDENTIFICATION;
        public event EventHandler<RecvCallbackArgs> EQUIP_QUERY_SV;
        public event EventHandler<RecvCallbackArgs> DELETE_EVENT_LINK;
        public event EventHandler<RecvCallbackArgs> EVENT_LINK;
        public event EventHandler<RecvCallbackArgs> QUERY_EVENT_LINK;
        public event EventHandler<RecvCallbackArgs> EVENT_REPORT;
        public event EventHandler<RecvCallbackArgs> EVENT_ENABLE;
        public event EventHandler<RecvCallbackArgs> EVENT_DISABLE;
        public event EventHandler<RecvCallbackArgs> EVENT_DISABLE_ALL;
        public event EventHandler<RecvCallbackArgs> ALARM_REPORT;
        public event EventHandler<RecvCallbackArgs> CLEAR_ALARM_REPORT;
        public event EventHandler<RecvCallbackArgs> EQUIP_EC_GET;
        public event EventHandler<RecvCallbackArgs> EQUIP_EC_SET;
        public event EventHandler<RecvCallbackArgs> ALARM_ENABLE;
        public event EventHandler<RecvCallbackArgs> ALARM_DISABLE;
        public event EventHandler<RecvCallbackArgs> MACH_CMD_INHIBIT_AUTO;
        public event EventHandler<RecvCallbackArgs> MACH_CMD_RELEASE_INHIBIT_AUTO;
        public event EventHandler<RecvCallbackArgs> MACH_CMD_PROCESS_START;
        public event EventHandler<RecvCallbackArgs> MACH_CMD_STOP_PROCESSING;
        public event EventHandler<RecvCallbackArgs> DOWNLOAD_RECIPE_TO_RAM;
        public event EventHandler<RecvCallbackArgs> UPLOAD_RAM_RECIPE;
        public event EventHandler<RecvCallbackArgs> DELETE_RECIPE;
        public event EventHandler<RecvCallbackArgs> QUERY_RECIPE_INFO;
        public event EventHandler<RecvCallbackArgs> PORT_RECIPE_TO_GOLD;
        public event EventHandler<RecvCallbackArgs> BPC_Result;
        public event EventHandler<RecvCallbackArgs> WLC_Result;
        public event EventHandler<RecvCallbackArgs> EQUIP_QUERY_CALIBRATION;
        public event EventHandler<RecvCallbackArgs> STRIP_MAP_REQUEST;
        public event EventHandler<RecvCallbackArgs> STRIP_MAP_UPLOAD;

        DuplexChannelFactory<IPushMessage> channelFactory;
        InstanceContext callback;
        IPushMessage proxy;
        public WcfService(string url)
        {
            Url = url;
            InitCBHandler();

            channelFactory = new DuplexChannelFactory<IPushMessage>(callback, new NetTcpBinding(SecurityMode.None), Url);
            proxy = channelFactory.CreateChannel();
        }

        #region 外部方法

        private void GetWcfServiceVoid(string name, Action func)
        {
            try
            {
                func();
            }
            catch (Exception ex)
            {
                proxy = channelFactory.CreateChannel();
                func();
            }
        }
        private void GetWcfMsgVoid(SendInfo sendInfo, Action func)
        {
            try
            {
                func();
            }
            catch (Exception ex)
            {
                proxy = channelFactory.CreateChannel();
                func();
            }
        }
        private bool GetWcfServiceBool(Func<bool> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                proxy = channelFactory.CreateChannel();
                return func();
            }
        }
        private string GetWcfServiceString(string TransactionId, string SendName, string RecvName, Func<string> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                proxy = channelFactory.CreateChannel();
                return func();
            }
        }

        #region EM注册
        /// <summary>
        /// EM注册
        /// </summary>
        /// <returns></returns>
        public void EM_Register(string emName)
        {
            GetWcfServiceVoid(emName, () =>
            {
                proxy.Login(emName);
            });
        }
        #endregion

        #region EM重连
        /// <summary>
        /// EM重连
        /// </summary>
        public void EM_Update(string emName)
        {
            GetWcfServiceVoid(emName, () =>
            {
                proxy.Update(emName);
            });
        }
        #endregion

        #region EM退出
        /// <summary>
        /// EM退出
        /// </summary>
        public void EM_Leave(string emName)
        {
            GetWcfServiceVoid(emName, () =>
            {
                proxy.Leave(emName);
            });
        }
        #endregion

        #region EM发送消息到CIM
        /// <summary>
        /// EM发送消息到CIM
        /// </summary>
        /// <param name="sendContent"></param>
        public void SendMessageToCIM(MessageBase sendContent, string emName, string cimName)
        {
            
            try
            {
                if (string.IsNullOrEmpty(cimName) || string.IsNullOrEmpty(emName))
                    throw new Exception("发送方和接收方不能为空， 请确认！");

                bool flag = GetWcfServiceBool(() =>
                {
                    return proxy.CheckRegister(emName);
                });

                if (!flag)
                    throw new Exception(string.Format("{0}没有注册", emName));

                sendInfo.SendName = emName;
                sendInfo.RevName = cimName;
                sendInfo.SendContent = sendContent;
                sendInfo.SendTime = DateTime.Now;

                string msg = string.Format("Name:[{0}], TransationID:[{1}]", sendInfo.SendContent.Name, sendInfo.SendContent.TransactionId);
                WriteLog(msg, "INFO");
                //WriteLog(JsonConvert.SerializeObject(sendInfo, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" }), "INFO");
                proxy.netSendMessage(sendInfo);

                //GetWcfMsgVoid(sendInfo, () =>
                //{
                //    proxy.netSendMessage(sendInfo);
                //});
            }
            catch (Exception ex)
            {
                string msg = string.Format("Name:[{0}], TransationID:[{1}]", sendContent.Name, sendContent.TransactionId);
                WriteLog("[" + msg + "]SendMessageToCIM发送信息失败：" + ex.Message, "ERROR");
                throw new Exception("SendMessageToCIM发送信息失败：" + ex.Message);
            }
        }
        #endregion
        private void WriteLog(string msg, string LogType)
        {
            string DateStr = DateTime.Now.Date.ToString("yyyy-MM-dd");
            LogWriteLock.EnterWriteLock();
            try
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(string.Format("{0}\\{1}_{2}.txt", AppDomain.CurrentDomain.BaseDirectory, DateStr, LogType), true))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ：" + msg);
                }
            }
            catch (Exception ex)
            {
                //throw ;
            }
            finally
            {
                LogWriteLock.ExitWriteLock();
            }
        }

        #region EM回复消息到CIM
        /// <summary>
        /// EM回复消息到CIM
        /// </summary>
        /// <param name="sendContent"></param>
        public void ReplyMessageToCIM(MessageBase sendContent, string emName, string cimName)
        {
            try
            {
                if (string.IsNullOrEmpty(cimName) || string.IsNullOrEmpty(emName))
                    throw new Exception("发送方和接收方不能为空， 请确认！");

                bool flag = GetWcfServiceBool(() =>
                {
                    return proxy.CheckRegister(emName);
                });

                if (!flag)
                    throw new Exception(string.Format("{0}没有注册", emName));

                sendContent.IsRecv = true;
                sendInfo.SendName = emName;
                sendInfo.RevName = cimName;
                sendInfo.SendContent = sendContent;
                sendInfo.SendTime = DateTime.Now;

                string msg = string.Format("Reply_Name:[{0}], TransationID:[{1}]", sendInfo.SendContent.Name, sendInfo.SendContent.TransactionId);
                WriteLog(msg, "INFO");
                proxy.netSendMessage(sendInfo);

                //GetWcfMsgVoid(sendInfo, () =>
                //{
                //    proxy.netSendMessage(sendInfo);
                //});
            }
            catch (Exception ex)
            {
                string msg = string.Format("Reply_Name:[{0}], TransationID:[{1}]", sendContent.Name, sendContent.TransactionId);
                WriteLog("[" + msg + "]ReplyMessageToCIM发送信息失败：" + ex.Message, "ERROR");
                throw new Exception("ReplyMessageToCIM发送信息失败：" + ex.Message);
            }
        }
        #endregion

        #region CIM注册
        /// <summary>
        /// CIM注册
        /// </summary>
        /// <returns></returns>
        public void CIM_Register(string cimName)
        {
            GetWcfServiceVoid(cimName, () =>
            {
                proxy.Login(cimName);
            });
        }
        #endregion

        #region CIM重连
        /// <summary>
        /// CIM重连
        /// </summary>
        public void CIM_Update(string cimName)
        {
            GetWcfServiceVoid(cimName, () =>
            {
                proxy.Update(cimName);
            });            
        }
        #endregion

        #region CIM重连
        /// <summary>
        /// CIM重连
        /// </summary>
        public void CIM_Leave(string cimName)
        {
            GetWcfServiceVoid(cimName, () =>
            {
                proxy.Leave(cimName);
            });            
        }
        #endregion

        #region CIM发送消息到EM
        /// <summary>
        /// CIM发送消息到EM
        /// </summary>
        /// <param name="sendContent"></param>
        public void SendMessageToEM(MessageBase sendContent, string cimName, string emName)
        {
            try
            {
                if (string.IsNullOrEmpty(cimName) || string.IsNullOrEmpty(emName))
                    throw new Exception("发送方和接收方不能为空， 请确认！");

                //bool flag = GetWcfServiceBool(() =>
                //{
                //    return proxy.CheckRegister(cimName);
                //});

                //if (!flag)
                //    throw new Exception(string.Format("{0}没有注册", cimName));

                sendInfo.SendName = cimName;
                sendInfo.RevName = emName;
                sendInfo.SendContent = sendContent;
                sendInfo.SendTime = DateTime.Now;

                GetWcfMsgVoid(sendInfo, () =>
                {
                    string msg = string.Format("CIM_Name:[{0}], TransationID:[{1}]", sendInfo.SendContent.Name, sendInfo.SendContent.TransactionId);
                    WriteLog(msg, "INFO");
                    proxy.javaSendMessage(sendInfo);
                });
            }
            catch (Exception ex)
            {
                string msg = string.Format("CIM_Name:[{0}], TransationID:[{1}]", sendContent.Name, sendContent.TransactionId);
                WriteLog("[" + msg + "]SendMessageToEM发送信息失败：" + ex.Message, "ERROR");
                throw new Exception("SendMessageToEM发送信息失败：" + ex.Message);
            }
        }
        #endregion

        #region 获取EM信息
        /// <summary>
        /// 获取EM信息
        /// </summary>
        /// <param name="TransactionId"></param>
        /// <returns></returns>
        public string GetEMReplyInfo(string TransactionId, string SendName, string RecvName)
        {
            try
            {
                return GetWcfServiceString(TransactionId, SendName, RecvName, () =>
                {
                    return proxy.GetEMReplyInfoById(TransactionId, SendName, RecvName);
                });
            }
            catch (Exception ex)
            {
                WriteLog("[" + TransactionId + "]GetEMReplyInfo获取EM信息失败：" + ex.Message, "ERROR");
                throw new Exception("GetEMReplyInfo获取EM信息失败：" + ex.Message);
            }
        }
        #endregion

        #endregion

        #region 内部方法
        /// <summary>
        /// 接收消息后处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_reciveInfo(object sender, CallbackRecEventArgs e)
        {
            MessageBase messageBase = e.SendContent;
            if (messageBase == null) return;

            try
            {
                switch (messageBase.Name.Trim())
                {
                    case "KEEP_ALIVE":
                        if (KEEP_ALIVE != null)
                            KEEP_ALIVE(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "EQP_IDENTIFICATION":
                        if (EQP_IDENTIFICATION != null)
                            EQP_IDENTIFICATION(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "EQUIP_QUERY_SV":
                        if (EQUIP_QUERY_SV != null)
                            EQUIP_QUERY_SV(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "DELETE_EVENT_LINK":
                        if (DELETE_EVENT_LINK != null)
                            DELETE_EVENT_LINK(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "EVENT_LINK":
                        if (EVENT_LINK != null)
                            EVENT_LINK(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "QUERY_EVENT_LINK":
                        if (QUERY_EVENT_LINK != null)
                            QUERY_EVENT_LINK(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "EVENT_REPORT":
                        if (EVENT_REPORT != null)
                            EVENT_REPORT(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "EVENT_ENABLE":
                        if (EVENT_ENABLE != null)
                            EVENT_ENABLE(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "EVENT_DISABLE":
                        if (EVENT_DISABLE != null)
                            EVENT_DISABLE(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "EVENT_DISABLE_ALL":
                        if (EVENT_DISABLE_ALL != null)
                            EVENT_DISABLE_ALL(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "ALARM_REPORT":
                        if (ALARM_REPORT != null)
                            ALARM_REPORT(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "CLEAR_ALARM_REPORT":
                        if (CLEAR_ALARM_REPORT != null)
                            CLEAR_ALARM_REPORT(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "EQUIP_EC_GET":
                        if (EQUIP_EC_GET != null)
                            EQUIP_EC_GET(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "EQUIP_EC_SET":
                        if (EQUIP_EC_SET != null)
                            EQUIP_EC_SET(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "ALARM_ENABLE":
                        if (ALARM_ENABLE != null)
                            ALARM_ENABLE(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "ALARM_DISABLE":
                        if (ALARM_DISABLE != null)
                            ALARM_DISABLE(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "MACH_CMD":
                        switch (messageBase.CommandName.Trim())
                        {
                            case "INHIBIT_AUTO":
                                if (MACH_CMD_INHIBIT_AUTO != null)
                                    MACH_CMD_INHIBIT_AUTO(sender, new RecvCallbackArgs(messageBase));
                                break;
                            case "RELEASE_INHIBIT_AUTO":
                                if (MACH_CMD_RELEASE_INHIBIT_AUTO != null)
                                    MACH_CMD_RELEASE_INHIBIT_AUTO(sender, new RecvCallbackArgs(messageBase));
                                break;
                            case "PROCESS_START":
                                if (MACH_CMD_PROCESS_START != null)
                                    MACH_CMD_PROCESS_START(sender, new RecvCallbackArgs(messageBase));
                                break;
                            case "STOP_PROCESSING":
                                if (MACH_CMD_STOP_PROCESSING != null)
                                    MACH_CMD_STOP_PROCESSING(sender, new RecvCallbackArgs(messageBase));
                                break;
                        }
                        break;
                    case "DOWNLOAD_RECIPE_TO_RAM":
                        if (DOWNLOAD_RECIPE_TO_RAM != null)
                            DOWNLOAD_RECIPE_TO_RAM(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "UPLOAD_RAM_RECIPE":
                        if (UPLOAD_RAM_RECIPE != null)
                            UPLOAD_RAM_RECIPE(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "DELETE_RECIPE":
                        if (DELETE_RECIPE != null)
                            DELETE_RECIPE(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "QUERY_RECIPE_INFO":
                        if (QUERY_RECIPE_INFO != null)
                            QUERY_RECIPE_INFO(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "PORT_RECIPE_TO_GOLD":
                        if (PORT_RECIPE_TO_GOLD != null)
                            PORT_RECIPE_TO_GOLD(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "BPC_Result":
                        if (BPC_Result != null)
                            BPC_Result(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "WLC_Result":
                        if (WLC_Result != null)
                            WLC_Result(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "EQUIP_QUERY_CALIBRATION":
                        if (EQUIP_QUERY_CALIBRATION != null)
                            EQUIP_QUERY_CALIBRATION(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "STRIP_MAP_REQUEST":
                        if (STRIP_MAP_REQUEST != null)
                            STRIP_MAP_REQUEST(sender, new RecvCallbackArgs(messageBase));
                        break;
                    case "STRIP_MAP_UPLOAD":
                        if (STRIP_MAP_UPLOAD != null)
                            STRIP_MAP_UPLOAD(sender, new RecvCallbackArgs(messageBase));
                        break;

                }
            }
            catch (Exception ex)
            {
                //throw;
                Console.WriteLine(ex.Message);
            }
        }

        private void InitCBHandler()
        {
            CB = new CallServer();
            CB.ChatEvent -= cb_reciveInfo;
            CB.ChatEvent += cb_reciveInfo;
            callback = new InstanceContext(CB);
        }
        #endregion





    }

}
