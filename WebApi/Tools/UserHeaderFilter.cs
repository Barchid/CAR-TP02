using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Swagger;

namespace WebApi.Tools
{
    /// <summary>
    /// General filter UI in the SWAGGER documentation.
    /// </summary>
    public class UserHeaderFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<IParameter>();

            operation.Parameters.Add(new NonBodyParameter
            {
                Name = "USER",
                In = "header",
                Type = "string",
                Required = false // set to false if this is optional
            });
        }
    }
}
