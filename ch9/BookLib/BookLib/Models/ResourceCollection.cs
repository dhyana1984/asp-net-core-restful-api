using System;
using System.Collections.Generic;

namespace BookLib.Models
{
    public class ResourceCollection<T> : Resource where T: Resource
    {
        public List<T> Items { get; }
        public ResourceCollection(List<T> item)
        {
            Items = item;
        }
    }
}
