using System;
using Newtonsoft.Json;

namespace BookLib.Models
{
    public class Link
    {
        public string Method { get; set; }
        public string Href { get; set; }

        [JsonProperty("rel")] //该attribute能够在序列化时为相应的属性提供自定义属性名
        public string Relation { get; set; }

        public Link(string method, string rel, string href)
        {
            Method = method;
            Href = href;
            Relation = rel;
        }
    }
}
