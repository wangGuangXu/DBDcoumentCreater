using System;
using System.Collections;
using System.Collections.Generic;

namespace Core.JsonSerialize
{
    /// <summary> 操作Json序列化/反序列化的静态对象
    /// </summary>
    public static class JsonConvert
    {
        /// <summary> 将对象转换为Json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeObject(object obj)
        {
            return new MyJsonBuilder().ToJsonString(obj);
        }
        

        /// <summary> 将json字符串转换为指定对象
        /// </summary>
        public static Object DeserializeObject(string json)
        {
            if (json == null || json.Length == 0)
            {
                return null;
            }
            return new JsonParser().ToObject(null, json);
        }

        /// <summary> 将json字符串转换为指定对象
        /// </summary>
        public static T DeserializeObject<T>(string json)
        {
            if (json == null || json.Length == 0)
            {
                return default(T);
            }
            return (T)new JsonParser().ToObject(typeof(T), json);
        }
    }
}
