using System;
using Microsoft.AspNetCore.Builder;

namespace DotNetCoreFeatures.Middleware
{
    //扩展IApplicationBuilder, 使用UseHttpMethodCheckMiddleware更加方便
    public static class CustomMiddlewareExtentions
    {
        public static IApplicationBuilder UseHttpMethodCheckMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HttpMethodCheckMiddleware>();
        }
    }
}
