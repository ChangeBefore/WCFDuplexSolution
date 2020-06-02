using System;
using System.IO;
using System.Net;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Web.Services.Description;
using Microsoft.CSharp;
using System.Text;
using System.Runtime.InteropServices;

namespace Service
{
    public class WebServiceHelper
    {
        #region InvokeWebService
        /// <summary>
        /// 动态调用web服务
        /// </summary>
        /// <param name="url">WSDL服务地址</param>
        /// <param name="methodname">方法名</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public static object InvokeWebService(string url, string methodname, object[] args)
        {
            //return WebServiceHelper.InvokeWebService(url, null, methodname, args);
            return InvokeWebServiceNew(url, null, methodname, args);
        }
        /// <summary>
        /// 动态调用web服务
        /// </summary>
        /// <param name="url">WSDL服务地址</param>
        /// <param name="classname">类名</param>
        /// <param name="methodname">方法名</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        public static object InvokeWebService(string url, string classname, string methodname, object[] args)
        {
            string @namespace = "EnterpriseServerBase.WebService.DynamicWebCalling";
            if ((classname == null) || (classname == ""))
            {
                classname = WebServiceHelper.GetWsClassName(url);
            }
            try
            {
                //获取WSDL
                WebClient wc = new WebClient();
                Stream stream = wc.OpenRead(url + "?WSDL");
                ServiceDescription sd = ServiceDescription.Read(stream);
                ServiceDescriptionImporter sdi = new ServiceDescriptionImporter();
                sdi.AddServiceDescription(sd, "", "");
                CodeNamespace cn = new CodeNamespace(@namespace);
                //生成客户端代理类代码
                CodeCompileUnit ccu = new CodeCompileUnit();
                ccu.Namespaces.Add(cn);
                sdi.Import(cn, ccu);
                CSharpCodeProvider icc = new CSharpCodeProvider();
                //设定编译参数
                CompilerParameters cplist = new CompilerParameters();
                cplist.GenerateExecutable = false;
                cplist.GenerateInMemory = true;
                cplist.ReferencedAssemblies.Add("System.dll");
                cplist.ReferencedAssemblies.Add("System.XML.dll");
                cplist.ReferencedAssemblies.Add("System.Web.Services.dll");
                cplist.ReferencedAssemblies.Add("System.Data.dll");
                //编译代理类
                CompilerResults cr = icc.CompileAssemblyFromDom(cplist, ccu);
                if (true == cr.Errors.HasErrors)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (System.CodeDom.Compiler.CompilerError ce in cr.Errors)
                    {
                        sb.Append(ce.ToString());
                        sb.Append(System.Environment.NewLine);
                    }
                    throw new Exception(sb.ToString());
                }
                //生成代理实例，并调用方法
                System.Reflection.Assembly assembly = cr.CompiledAssembly;
                //Type t = assembly.GetType(@namespace + "." + classname, true, true);
                Type t = assembly.GetTypes()[0];
                object obj = Activator.CreateInstance(t);
                System.Reflection.MethodInfo mi = t.GetMethod(methodname);
                var aaa = mi.Invoke(obj, args);
                return aaa;
                /*
                PropertyInfo propertyInfo = type.GetProperty(propertyname);
                return propertyInfo.GetValue(obj, null);
                */
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("调用URL：{0}， 方法：{1}， 参数：{2}， 没有回传值！{3}, {4}", url, methodname, args[0].ToString(), ex.InnerException.Message, new Exception(ex.InnerException.StackTrace)));
            }
        }
        private static string GetWsClassName(string wsUrl)
        {
            string[] parts = wsUrl.Split('/');
            string[] pps = parts[parts.Length - 1].Split('.');
            return pps[0];
        }
        #endregion


        private static object InvokeWebServiceNew(string url, string classname, string methodname, object[] args)
        {
            try
            {
                string @namespace = "SyncTools.WebService.DynamicWebLoad";
                if (classname == null || classname == "")
                {
                    classname = GetWsClassName(url);
                }
                classname = @namespace + "." + classname;
                // Construct and initialize settings for a second AppDomain.
                AppDomainSetup appDomainSetup = new AppDomainSetup
                {
                    ApplicationBase = System.Environment.CurrentDirectory,
                    DisallowBindingRedirects = false,
                    DisallowCodeDownload = false,
                    ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile
                };
                // Create the second AppDomain.
                // AppDomain appDomain = AppDomain.CreateDomain("SyncTools.WebService.DynamicWebLoad", null, appDomainSetup);
                AppDomain appDomain = AppDomain.CreateDomain("SyncTools.WebService.DynamicWebLoad");

                Type t = typeof(ProxyObject);
                ProxyObject obj = (ProxyObject)appDomain.CreateInstanceAndUnwrap(t.Assembly.FullName, t.FullName);
                obj.LoadAssembly(url, @namespace);
                obj.Invoke(classname, methodname, args);
                var result = obj.Result;
                AppDomain.Unload(appDomain);
                appDomain = null;
                obj = null;
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("调用URL：{0}， 方法：{1}， 参数：{2}， 没有回传值！{3}, {4}", url, methodname, args[0].ToString(), ex.InnerException.Message, new Exception(ex.InnerException.StackTrace)));
            }
            finally
            {
                GC.Collect();
            }
        }
    }
    internal class ProxyObject : MarshalByRefObject
    {
        #region 内存回收
        //[DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        [DllImportAttribute("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
        /// <summary>
        /// 释放内存
        /// </summary>
        public static void ClearMemory()
        {
            SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
        }
        #endregion

        System.Reflection.Assembly assembly = null;
        private Object result = null;
        public Object Result
        {
            get { return this.result; }
        }

        public void LoadAssembly(string url, string @namespace)
        {
            // assembly = Assembly.LoadFile(@"TestDLL.dll");
            //获取服务描述语言(WSDL)
            using (WebClient wc = new WebClient())
            {
                using (Stream stream = wc.OpenRead(url + "?WSDL"))
                {
                    ServiceDescription sd = ServiceDescription.Read(stream);
                    ServiceDescriptionImporter sdi = new ServiceDescriptionImporter();
                    sdi.AddServiceDescription(sd, "", "");
                    CodeNamespace cn = new CodeNamespace(@namespace);
                    //生成客户端代理类代码
                    CodeCompileUnit ccu = new CodeCompileUnit();
                    ccu.Namespaces.Add(cn);
                    sdi.Import(cn, ccu);
                    CSharpCodeProvider csc = new CSharpCodeProvider();
                    ICodeCompiler icc = csc.CreateCompiler();
                    //设定编译器的参数
                    CompilerParameters cplist = new CompilerParameters();
                    cplist.GenerateExecutable = false;
                    cplist.GenerateInMemory = false;
                    cplist.ReferencedAssemblies.Add("System.dll");
                    cplist.ReferencedAssemblies.Add("System.XML.dll");
                    cplist.ReferencedAssemblies.Add("System.Web.Services.dll");
                    cplist.ReferencedAssemblies.Add("System.Data.dll");
                    //编译代理类
                    CompilerResults cr = icc.CompileAssemblyFromDom(cplist, ccu);
                    if (true == cr.Errors.HasErrors)
                    {
                        System.Text.StringBuilder sb = new StringBuilder();
                        foreach (CompilerError ce in cr.Errors)
                        {
                            sb.Append(ce.ToString());
                            sb.Append(System.Environment.NewLine);
                        }
                        throw new Exception(sb.ToString());
                    }
                    //生成代理实例,并调用方法
                    assembly = cr.CompiledAssembly;
                }
            }
        }
        public bool Invoke(string className, string methodName, params Object[] args)
        {
            if (assembly == null)
                return false;
            //Type tp = assembly.GetType(className, true, true);
            Type tp = assembly.GetTypes()[0];
            if (tp == null)
                return false;
            System.Reflection.MethodInfo method = tp.GetMethod(methodName);
            if (method == null)
                return false;
            Object obj = Activator.CreateInstance(tp);
            result = method.Invoke(obj, args);
            return true;
        }

    }
}
