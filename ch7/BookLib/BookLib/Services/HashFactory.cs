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
            string result = string.Empty;
            //序列化对象
            var json = JsonConvert.SerializeObject(entity);
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
