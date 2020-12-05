using System.Linq;
using BookLib.Entities;
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
            });

            //services.AddControllers(options =>
            //{
            //    options.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
            //});
            services.AddScoped<IAuthorRepository, AuthorRepository>();
            services.AddScoped<IBookRepository, BookRepository>();

            services.AddDbContext<LibraryDbContext>(config =>
            {
                //从appsettings.json里面读节点“DefaultConnection”
                config.UseMySQL(Configuration.GetConnectionString("DefaultConnection"));
            });
        }

        private static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
        {
            var builder = new ServiceCollection()
                .AddLogging()
                .AddMvc()
                .AddNewtonsoftJson()
                .Services.BuildServiceProvider();

            return builder
                .GetRequiredService<IOptions<MvcOptions>>()
                .Value
                .InputFormatters
                .OfType<NewtonsoftJsonPatchInputFormatter>()
                .First();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

           
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
