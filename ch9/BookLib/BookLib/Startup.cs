using System;
using System.IO;
using System.Text;
using AutoMapper;
using BookLib.Conventions;
using BookLib.Entities;
using BookLib.Extensions;
using BookLib.Filter;
using BookLib.Middleware;
using BookLib.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;

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
                config.Conventions.Add(new ApiExplorerGroupPerVersionConvention());
            });

            services.AddDbContext<LibraryDbContext>(config =>
            {
                //从appsettings.json里面读节点“DefaultConnection”
                config.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                    //使用MigrationsAssembly方法为当前DbContext设置其迁移所在的程序集名称，因为DbContext与为其创建迁移并不在统一程序集
                    optionBuilder => optionBuilder.MigrationsAssembly(typeof(Startup).Assembly.GetName().Name));
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
            services.AddApiVersioning(options =>
            {
                //client未指明版本时用默认版本，默认是false。如果设为false，当client未指明版本时，会返回400 bad request
                options.AssumeDefaultVersionWhenUnspecified = true;
                //定义默认版本，v1.0
                options.DefaultApiVersion = new ApiVersion(1, 0);
                //是否在http响应header中包含api-supported-versions和api-deprecated-versions这两项
                options.ReportApiVersions = true;
                //使用ApiVersionReader.Combine支持多种api version的方式
                //注意多个地方使用版本的时候，版本要一致否则会报错
                options.ApiVersionReader = ApiVersionReader.Combine(
                    //修改访问api version的querystring名称，默认是api-version
                    new QueryStringApiVersionReader("ver"),
                    //自定义http消息头访问指定版本的api, 默认是不支持的，需要手工定义
                    new HeaderApiVersionReader("api-version"),
                    //通过媒体类型获取api，在header的accept或者content-type指定, 同时存在时优先使用content-type
                    new MediaTypeApiVersionReader()
                );
                options.Conventions.Controller<API.Controllers.V1.ProjectController>().HasApiVersion(new ApiVersion(1, 0));
                options.Conventions.Controller<API.Controllers.V1.ProjectController>().HasDeprecatedApiVersion(new ApiVersion(1, 0));
                options.Conventions.Controller<API.Controllers.V2.ProjectController>().HasApiVersion(new ApiVersion(2, 0));
            });
            //添加GraphQL的serivce，这是一个自定义的扩展方法
            services.AddGraphQLSchemaAndTypes();
            //添加identity服务。AddIdentity会向容器添加UserManager，RoleManager以及它们依赖的服务，并且会添加Identity用到的Cookie认证
            //所以services.AddIdentity需要添加在services.AddAuthentication之前，避免其认证方式替换JWT Bearer默认方式
            services.AddIdentity<User, Role>()
                .AddEntityFrameworkStores<LibraryDbContext>();//AddEntityFrameworkStores 将EF Core中对IUserStore和IRoleStore添加到容器
            //读取appsettings.json中的Security section的Token section, 注意:前后不能有空格
            var tokenSection = Configuration.GetSection("Security:Token");
            //添加JwtBearer中间件的service到容器中， defaultScheme指定当未指定具体认证方案时的默认方案
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                //从配置中读取token信息
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = tokenSection["Issuer"],
                    ValidAudience = tokenSection["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSection["Key"])),
                    ClockSkew = TimeSpan.Zero
                };
            });
            //添加数据保护服务
            services.AddDataProtection();
            //添加CORS服务, 可以通过option参数配置策略
            services.AddCors(option=>
            {
                //设置允许跨域请求携带的header
                var allowHeaders = new string[] { "Content-Type", "Authorization" };
                //设置跨域请求策略
                option.AddPolicy("AllowAllMethodsPolicy", builder => builder
                    .WithOrigins("http://192.168.2.90:6001")        //设置允许跨域请求的域
                    .WithHeaders(allowHeaders)                      //设置跨域请求的header
                    .AllowAnyMethod());                             //设置跨域请求所有http方法都允许

                option.AddPolicy("AllowAllOriginPolicy", builder => builder.AllowAnyOrigin());

                //当在Configure中没有指明策略名的时候，用此默认策略
                option.AddDefaultPolicy(builder => builder.WithOrigins("http://192.168.2.90:6001"));
            });

            //添加Swagger生成器
            //AddSwaggerGen注册Swagger生成器，OpenApiInfo是GenOptions，指定文档基本信息，比如标题，版本等
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Library API", Version = "v1" });
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "Library API", Version = "v2" });

                //使Swagger中包含xml中的注释内容
                var xmlFile = Path.ChangeExtension(typeof(Startup).Assembly.Location, ".xml");
                c.IncludeXmlComments(xmlFile);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            



            //WithOrigins限定访问源。在此配置CORS策略，同样还有WithMethods和WithHeaders
            //app.UseCors(builder => builder.WithOrigins("http://192.168.2.90:6001"));

            //AllowAllMethodsPolicy策略是在services中配置的
            //app.UseCors("AllowAllMethodsPolicy");

            //使用自定义限流中间件
            app.UseMiddleware<RequestRateLimitingMiddleware>();

            app.UseAuthorization();

            //app.UseHttpsRedirection();

            //在请求管道中添加响应缓存中间件,需要在app.UseMvc()之前使用
            //一旦使用缓存中间件，请求同样结果不会再返回304，因为直接从中间件返回结果，不到controller
            app.UseResponseCaching();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                //默认的访问swagger UI是http://host/swagger
                //加上c.RoutePrefix = string.Empty; 后直接访问http://host即可访问swagger
                c.RoutePrefix = string.Empty;
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library API V1");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "Library API V2");
            });

            app.UseMvc();

            //app.UseRouting();


            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});
        }
    }
}
