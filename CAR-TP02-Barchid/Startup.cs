using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;
using WebApi.Ftp;
using WebApi.Tools;
using WebApi.Tools.Extensions;

namespace CAR_TP02_Barchid
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
            services.AddScoped<FtpContext>();
            services.AddScoped<IClient, Client>();

            services.AddMvc();

            services.AddFtpCredentials(options => { });

            services.AddSwaggerGen(context =>
            {
                context.SwaggerDoc("v1", new Info
                {
                    Title = "FTP intermediate Web API",
                    Version = "1.0",
                    Description = "This is an intermediate REST API used to use the FTP service of your choice."
                });

                context.OperationFilter<UserHeaderFilter>();
                context.OperationFilter<PassHeaderFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FTP REST API"));

            app.UseFtpCredentials();

            app.UseMvc();
        }
    }
}
