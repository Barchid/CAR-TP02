using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Ftp;

namespace WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Directories")]
    public class DirectoriesController : Controller
    {
        private readonly IClient _client;

        public DirectoriesController(IClient client)
        {
            _client = client;
        }

        // GET: api/Directories
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Directories/5
        [HttpGet()]
        public string Get([FromQuery(Name = "path")] string path)
        {
            return "value";
        }

        // POST: api/Directories
        [HttpPost]
        public IActionResult Post([FromQuery(Name = "path")]string path)
        {
            bool result = _client.AddDirectory(path);
            if (result)
            {
                return Ok("Directory created at path " + path);
            }
            else
            {
                return BadRequest("Cannot create a directory of path " + path);
            }
        }

        // PUT: api/Directories/5
        [HttpPut()]
        public IActionResult Put([FromQuery(Name = "oldPath")] string path, [FromBody]string newPath)
        {
            bool result = _client.Move(path, newPath);
            if (result)
            {
                return Ok($"Directory moved from path ${path} to path ${newPath}.");
            }
            else
            {
                return BadRequest($"Directory could not be moved from path ${path} to path ${newPath}");
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete()]
        public IActionResult Delete([FromQuery(Name = "path")] string path)
        {
            bool result = _client.RemoveDirectory(path);
            if(result)
            {
                return Ok($"Directory removed from path ${path}.");
            } else
            {
                return BadRequest($"Directory could not be removed from path ${path}");
            }
        }
    }
}
