using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Ftp;
using System.Collections;
using FluentFTP;
using WebApi.Model;
using System.IO.Compression;
using System.IO;

namespace WebApi.Controllers
{
    /// <summary>
    /// Controller class used to manage all the operations related to a directory in the
    /// FTP server.
    /// </summary>
    [Produces("application/json")]
    [Route("api/Directories")]
    public class DirectoriesController : Controller
    {
        private readonly IClient _client;

        public DirectoriesController(IClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Lists the information of the specified directory
        /// </summary>
        /// <param name="path">the path of the directory to list</param>
        /// <returns>The list</returns>
        [HttpGet("list")]
        public IActionResult List([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("The path cannot be null.");
            }
            IEnumerable<FtpListItem> list = _client.ListDirectory(path);
            if (list == null)
            {
                return NotFound(new
                {
                    Message = "The specified folder is not found in the FTP server."
                });
            }
            else
            {
                return Json(list);
            }
        }

        /// <summary>
        /// Downloads recursively the directory specified in parameter.
        /// </summary>
        /// <param name="path">The path of the directory to download</param>
        /// <returns>A ZIP file with the content of the specified directory</returns>
        [HttpGet("download")]
        public IActionResult Download([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("The path cannot be null.");
            }

            return Ok();
        }

        /// <summary>
        /// Creates a directory in the specified path.
        /// </summary>
        /// <param name="path">The path of the directory to create.</param>
        /// <returns>OK if the creation has been successful</returns>
        [HttpPost]
        public IActionResult Create([FromQuery(Name = "path")]string path)
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

        /// <summary>
        /// Moves a directory to another path.
        /// </summary>
        /// <param name="move">The new and old paths of the directory to move</param>
        /// <returns>200 ok if the move operation is succeeded or else false.</returns>
        [HttpPut()]
        public IActionResult Move([FromBody] MoveInput move)
        {
            bool result = _client.Move(move.OldPath, move.TargetPath);
            if (result)
            {
                return Ok($"Directory moved from path ${move.OldPath} to path ${move.TargetPath}.");
            }
            else
            {
                return BadRequest($"Directory could not be moved from path ${move.OldPath} to path ${move.TargetPath}");
            }
        }

        /// <summary>
        /// Deletes a file in the remote server
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpDelete()]
        public IActionResult Delete([FromQuery(Name = "path")] string path)
        {
            bool result = _client.RemoveDirectory(path);
            if (result)
            {
                return Ok($"Directory removed from path ${path}.");
            }
            else
            {
                return BadRequest($"Directory could not be removed from path ${path}");
            }
        }

        /// <summary>
        /// Uploads a directory
        /// </summary>
        /// <param name="path">The path where the directory will be uploaded.</param>
        /// <param name="archive">The .zip archive that will be uploaded into the FTP server.</param>
        /// <returns>200 ok if the directory has been uploaded successfully</returns>
        [HttpPost("Upload")]
        public IActionResult Upload([FromQuery] string path, IFormFile archive)
        {
            if(string.IsNullOrWhiteSpace(path) || archive == null)
            {
                return BadRequest("path & archive required.");
            }

            // TODO
            return Ok();
        }
    }
}
