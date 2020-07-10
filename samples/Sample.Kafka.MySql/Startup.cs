using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartSql;

namespace Sample.Kafka.MySql
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
                .AddRepositoryFromAssembly(options => { options.AssemblyString = "Sample.Kafka.MySql"; })
                .AddCapRepository();

            services.AddCap(options =>
            {
                options.UseSmartSql(smartSqlOptions => { smartSqlOptions.InitializeTable = false; });
                options.UseKafka("127.0.0.1:9092");
                options.UseDashboard();
            });

            var assembly = Assembly.Load("Sample.Kafka.MySql");
            var allTypes = assembly.GetTypes();
            foreach (var type in allTypes.Where(t =>
                !string.IsNullOrEmpty(t.FullName) && t.FullName.Contains("Sample.Kafka.MySql.Service")))
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
