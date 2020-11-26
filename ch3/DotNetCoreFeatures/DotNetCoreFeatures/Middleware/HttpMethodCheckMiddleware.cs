using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace DotNetCoreFeatures.Middleware
{
    //自定义中间件
    public class HttpMethodCheckMiddleware
    {
        private readonly RequestDelegate _next;
        public HttpMethodCheckMiddleware(RequestDelegate requestDelegate, IWebHostEnvironment environment)
        {
            this._next = requestDelegate;
        }

        //需要实现Invoke方法
        public Task Invoke(HttpContext context)
        {
            var requestMethod = context.Request.Method.ToUpper();
            //过滤请求的method，只允许get和head
            if(requestMethod == HttpMethods.Get || requestMethod == HttpMethods.Head)
            {
                return _next(context);
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.Headers.Add("X-AllowHTTPVerb", new[] { "GET, HEAD" });
                context.Response.WriteAsync("only support GET and HEAD http methods");
                Console.WriteLine($"request was rejected because method is {context.Request.Method}");
                return Task.CompletedTask;
            }
        }
    }
}
