using System;
namespace BookLib.Helpers
{
    //用来映射使用Dynamic Linq排序时DTO到ENTITY的属性
    public class PropertyMapping
    {
        //IsRevert表示排序方向是否相反
        public bool IsRevert { get; private set; }
        public string TargetProperty { get; private set; }

        public PropertyMapping(string targetProperty, bool revert = false)
        {
            TargetProperty = targetProperty;
            IsRevert = revert;
        }
    }
}
