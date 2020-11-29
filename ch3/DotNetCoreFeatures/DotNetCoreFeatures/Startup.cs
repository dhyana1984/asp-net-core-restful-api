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
        //注意，这里可以注入ILogger
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
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
                //12是自定义eventId, 在console输出中会带着
                logger.LogInformation(12,"this is a testing log");
                await context.Response.WriteAsync("Hello World");
            });

            
            if (env.IsDevelopment())
            {
                //UseDeveloperExceptionPage只有在开发环境有效
                app.UseDeveloperExceptionPage();
            }

            //如果在生产环境，需要用app.UseExceptionHandler
            //这相当于是个全局try-catch机制
            if (env.IsProduction())
            {
                //UseExceptionHandler的参数是个Action<IApplicationBuilder>的委托
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.ContentType = "text/plain;charset=utf-8";
                        await context.Response.WriteAsync("Sorry, something is wrong"); 
                    });
                });
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
