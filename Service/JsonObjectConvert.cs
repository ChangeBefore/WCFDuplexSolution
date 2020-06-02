using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Service
{
    public static class JsonObjectConvert
    {
        /// <summary>
        /// JSON.net反序列化
        /// </summary>
        public static T JsonDeserialize<T>(this string jsonString)
        {
            JsonSerializerSettings jsetting = new JsonSerializerSettings();
            //jsetting.Converters.Add(new JavaScriptDateTimeConverter());//指定转化日期的格式
            jsetting.NullValueHandling = NullValueHandling.Ignore;//忽略空值
            jsetting.TypeNameHandling = TypeNameHandling.Auto;
            return JsonConvert.DeserializeObject<T>(jsonString, jsetting);
        }

        /// <summary>
        /// JSON.net序列化
        /// </summary>
        public static string JsonSerialize<T>(this T jsonObject)
        {
            JsonSerializerSettings jsetting = new JsonSerializerSettings();
            //jsetting.Converters.Add(new JavaScriptDateTimeConverter());//指定转化日期的格式
            jsetting.NullValueHandling = NullValueHandling.Ignore;//忽略空值
            jsetting.TypeNameHandling = TypeNameHandling.Auto;

            return JsonConvert.SerializeObject(jsonObject, Formatting.None, jsetting);
        }

        public static string ToFormatJson(string jsonStr)
        {
            MatchEvaluator matchEvaluator = new MatchEvaluator(ConvertJsonDateToDateString);
            Regex reg = new Regex(@"\\/Date(−∗\d+)(−∗\d+)\\/");
            var newjson = reg.Replace(jsonStr, matchEvaluator);
            return newjson;
        }


        private static string ConvertJsonDateToDateString(Match m)
        {

            string result = string.Empty;
            DateTime dt = new DateTime(1970, 1, 1);
            dt = dt.AddMilliseconds(long.Parse(m.Groups[1].Value));
            dt = dt.ToLocalTime();
            result = dt.ToString("yyyy-MM-dd HH:mm:ss");
            return result;
        }

        /// <summary>
        /// JSON.net反序列化
        /// </summary>
        public static T JsonDeserializeNoTypeNameHandling<T>(this string jsonString)
        {
            JsonSerializerSettings jsetting = new JsonSerializerSettings();
            //jsetting.Converters.Add(new JavaScriptDateTimeConverter());//指定转化日期的格式
            jsetting.NullValueHandling = NullValueHandling.Ignore;//忽略空值
            return JsonConvert.DeserializeObject<T>(jsonString, jsetting);
        }

        /// <summary>
        /// JSON.net序列化
        /// </summary>
        public static string JsonSerializeNoTypeNameHandling<T>(this T jsonObject)
        {
            JsonSerializerSettings jsetting = new JsonSerializerSettings();
            //jsetting.Converters.Add(new JavaScriptDateTimeConverter());//指定转化日期的格式
            jsetting.NullValueHandling = NullValueHandling.Ignore;//忽略空值

            return JsonConvert.SerializeObject(jsonObject, Formatting.None, jsetting);
        }


    }
}
