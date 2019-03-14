using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace WebApi.Tools.Extensions
{
    public class FtpCredentialsOption
    {
    }

    public class FtpCredentials
    {
        private readonly RequestDelegate _next;
        private readonly FtpCredentialsOption _option;

        public FtpCredentials(RequestDelegate next, IOptions<FtpCredentialsOption> options)
        {
            _next = next;
            _option = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            FtpContext ftpContext = context.RequestServices.GetService<FtpContext>();

            string userInput = context.Request.Headers["USER"].ToString() ?? "anonymous";
            string passInput = context.Request.Headers["PASS"].ToString() ?? "anonymous";

            ftpContext.User = userInput;
            ftpContext.Pass = passInput;

            await _next(context);
        }
    }

    public static class FtpCredentialsExtension
    {
        public static IServiceCollection AddFtpCredentials(this IServiceCollection service, Action<FtpCredentialsOption> options)
        {
            options = options ?? (opts => { });

            service.Configure(options);
            return service;
        }

        public static IApplicationBuilder UseFtpCredentials(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FtpCredentials>();
        }
    }
}
