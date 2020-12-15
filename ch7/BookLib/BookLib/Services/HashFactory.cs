using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace BookLib.Services
{
    public interface IHashFactory
    {
        string GetHash(object entity);
    }

    public class HashFactory : IHashFactory
    {
        public string GetHash(object entity)
        {
            //JsonConvert的设置项，这里主要是用ReferenceLoopHandling = ReferenceLoopHandling.Ignore 忽略循环引用，因为Book和Author之间有主外键关系，直接序列化Book会json循环引用错误
            JsonSerializerSettings setting = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.None
            };
            string result = string.Empty;
            //序列化对象
            var json = JsonConvert.SerializeObject(entity, setting);
            //把序列化的字符串转为字节
            var bytes = Encoding.UTF8.GetBytes(json);

            using(var hasher = MD5.Create())
            {
                //利用MD5创建序列化字符串字节的哈希字节数组
                var hash = hasher.ComputeHash(bytes);
                //将哈希字节叔祖转化成哈希字符串
                result = BitConverter.ToString(hash);
                //去除dash
                result = result.Replace("-", "");
            }

            return result;
        }
    }
}
