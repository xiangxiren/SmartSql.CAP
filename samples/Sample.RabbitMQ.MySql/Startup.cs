using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartSql;

namespace Sample.RabbitMQ.MySql
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
            services.AddControllers();

            services
                .AddSmartSql((provider, builder) => { builder.UseProperties(Configuration); })
                .AddRepositoryFromAssembly(options => { options.AssemblyString = "Sample.RabbitMQ.MySql"; })
                .AddCapRepository();

            services.AddCap(options =>
            {
                options.UseSmartSql(smartSqlOptions => { smartSqlOptions.InitializeTable = false; });
                options.UseRabbitMQ(option =>
                {
                    option.HostName = Configuration["RabbitMQConfig:HostName"];
                    option.Port = Configuration.GetValue<int>("RabbitMQConfig:Port");
                    option.VirtualHost = Configuration["RabbitMQConfig:VirtualHost"];
                    option.UserName = Configuration["RabbitMQConfig:UserName"];
                    option.Password = Configuration["RabbitMQConfig:Password"];
                });
                options.UseDashboard();
            });

            var assembly = Assembly.Load("Sample.RabbitMQ.MySql");
            var allTypes = assembly.GetTypes();
            foreach (var type in allTypes.Where(t =>
                !string.IsNullOrEmpty(t.FullName) && t.FullName.Contains("Sample.RabbitMQ.MySql.Service")))
            {
                services.AddSingleton(type);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
