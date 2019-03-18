using FluentFTP;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WebApi.Exceptions;

namespace WebApi.Tools
{
    /// <summary>
    /// Middleware used to catch thrown exceptions and generate an error response (400+ code) that will be send to the client.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context /* other dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode code = HttpStatusCode.InternalServerError; // 500 if unexpected

            bool isBadRequest = exception is FtpCommandException || exception is ArgumentOutOfRangeException ||
                exception is FtpException || exception is InvalidDataException;

            if (isBadRequest)
            {
                code = HttpStatusCode.BadRequest;
            }
            else if (exception is HttpUnauthorizedException)
            {
                code = HttpStatusCode.Unauthorized;
            }

            string result = JsonConvert.SerializeObject(new { error = exception.Message });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }
    }
}
