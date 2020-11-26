using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreFeatures.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetCoreFeatures
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        //将服务加入依赖注入容器
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //配置中间件
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //使用自定义的中间件, 需要调用IApplicationBuilder的UseMiddleware泛型方法
            //app.UseMiddleware<HttpMethodCheckMiddleware>();

            //使用IApplicationBuilder的扩展方法调用中间件
            app.UseHttpMethodCheckMiddleware();

            // app.Use执行完会执行下一个中间件
            app.Use(async (context, next) =>
            {
                var timer = Stopwatch.StartNew();
                Console.WriteLine($"midware A start: {timer.ElapsedMilliseconds}");
                //next()会执行下一个中间件
                await next();
                Console.WriteLine($"midware A end: {timer.ElapsedMilliseconds}");
            });

            //app.Run执行完就返回Response了
            app.Run(async (context) =>
            {
                Console.WriteLine("midware B");
                await Task.Delay(500);
                await context.Response.WriteAsync("Hello World");
            });

            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
