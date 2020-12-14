using System.Linq;
using AutoMapper;
using BookLib.Entities;
using BookLib.Filter;
using BookLib.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BookLib
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //必须AddNewtonsoftJson 才能使用JsonPatchDocument
            services.AddControllersWithViews()
                    .AddNewtonsoftJson();
            services.AddMvc(config =>
            {
                //此配置是当请求header里面acccept的格式不支持时，返回406
                config.ReturnHttpNotAcceptable = true;
                //将xml格式添加到OutputFormatters中，这样api返回时就可以支持请求header中accept是xml的请求
                config.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                //将JsonExceptionFilter配置进网站
                //加这里是全站有效，加载下面 services.AddScoped<CheckAuthorExistFilterAttribute>();是调用[ServiceFilter(typeof(CheckAuthorExistFilterAttribute))]时有效
                config.Filters.Add<JsonExceptionFilter>();
                //添加全局缓存配置"Default"和"Never"，在[ResponseCache(CacheProfileName= "ProfileName")]这里使用ProfileName替换成配置的名称
                config.CacheProfiles.Add("Default",
                    new CacheProfile()
                    {
                        Duration = 60
                    });
                config.CacheProfiles.Add("Never",
                    new CacheProfile()
                    {
                        Location = ResponseCacheLocation.None,
                        NoStore = true
                    });
                config.EnableEndpointRouting = false;

            });

            services.AddDbContext<LibraryDbContext>(config =>
            {
                //从appsettings.json里面读节点“DefaultConnection”
                config.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
            services.AddAutoMapper(typeof (Startup));
            services.AddScoped<CheckAuthorExistFilterAttribute>();
            services.AddScoped<IHashFactory, HashFactory>();

            //添加缓存中间件服务
            services.AddResponseCaching(options =>
            {
                options.UseCaseSensitivePaths = true;
                options.MaximumBodySize = 1024;
            });

            //添加内存中间件服务，然后在controller中注入IMemoryCache接口使用
            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            //在请求管道中添加响应缓存中间件,需要在app.UseMvc()之前使用
            //一旦使用缓存中间件，请求同样结果不会再返回304，因为直接从中间件返回结果，不到controller
            app.UseResponseCaching();


            app.UseMvc();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
