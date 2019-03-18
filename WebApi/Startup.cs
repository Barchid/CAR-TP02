using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Reflection;
using WebApi.Ftp;
using WebApi.Tools;

namespace WebApi
{
    /// <summary>
    /// Class used at the start of the program to define how the service will be running and with which configuration.
    /// </summary>
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

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                context.IncludeXmlComments(xmlPath);
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

            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMvc();
        }
    }
}
