using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BookLib.Models
{
    public abstract class Resource
    {
        [JsonProperty("_links", Order = 100)] //Order属性可以指定所标识属性序列化时的位置，100就会时_link属性放在最后
        public List<Link> Links { get; } = new List<Link>();
    }
}
