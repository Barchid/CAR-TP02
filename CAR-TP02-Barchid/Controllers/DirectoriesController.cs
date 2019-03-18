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
        /// <returns>The response of the request.</returns>
        /// <response code="200">The listing of the specified directory.</response>
        /// <response code="500">Internal server error</response>
        /// <response code="400">When the request is not valid.</response>
        /// <response code="404">When the specified directory is not found in the remote FTP server.</response>
        [HttpGet("list")]
        [ProducesResponseType(typeof(IEnumerable<FtpListItem>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(string), 500)]
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
        /// /// <response code="200">The downloaded zip archive</response>
        /// <response code="500">Internal server error</response>
        /// <response code="400">When the request is not valid.</response>
        [HttpGet("download")]
        [ProducesResponseType(typeof(File), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult Download([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("The path cannot be null.");
            }

            MemoryStream memory = _client.DownloadDirectory(path);
            if (memory != null)
            {
                return File(memory, "application/zip", GetFileName(path) + ".zip");
            }
            else
            {
                return BadRequest("The directory could not be downloaded. Please check if the specified path is valid.");
            }
        }

        /// <summary>
        /// Creates a directory in the specified path.
        /// </summary>
        /// <param name="path">The path of the directory to create.</param>
        /// <returns>OK if the creation has been successful</returns>
        /// <response code="200">A success message</response>
        /// <response code="500">Internal server error</response>
        /// <response code="400">When the request is not valid.</response>
        [HttpPost]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        [ProducesResponseType(typeof(string), 200)]
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
        /// <response code="200">A success message</response>
        /// <response code="500">Internal server error</response>
        /// <response code="400">When the request is not valid.</response>
        [HttpPut()]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 500)]
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
        /// Deletes a file in the remote FTP server.
        /// </summary>
        /// <param name="path">The path of the directory to delete in the remote FTP server.s</param>
        /// <returns>The response</returns>
        /// <response code="200">A success message</response>
        /// <response code="500">Internal server error</response>
        /// <response code="400">When the request is not valid.</response>
        [HttpDelete()]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult Delete([FromQuery(Name = "path")] string path)
        {
            bool result = _client.RemoveDirectory(path);
            if (result)
            {
                return Ok($"Directory removed from path {path}.");
            }
            else
            {
                return BadRequest($"Directory could not be removed from path {path}");
            }
        }

        /// <summary>
        /// Uploads a directory in the specified path on the FTP server.
        /// </summary>
        /// <param name="path">The path where the directory will be uploaded.</param>
        /// <param name="archive">The .zip archive that will be uploaded into the FTP server.</param>
        /// <returns>200 ok if the directory has been uploaded successfully</returns>
        /// <response code="200">The list of items of the created directory</response>
        /// <response code="500">Internal server error</response>
        /// <response code="400">When the request is not valid.</response>
        [HttpPost("Upload")]
        [ProducesResponseType(typeof(IEnumerable<FtpListItem>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult Upload([FromQuery] string path, IFormFile archive)
        {
            if(string.IsNullOrWhiteSpace(path) || archive == null)
            {
                return BadRequest("path & archive required.");
            }

            IEnumerable<FtpListItem> uploaded = _client.UploadDirectory(path, archive);
            if(uploaded == null)
            {
                return BadRequest("The archive cannot be uploaded in the specified path.");
            }

            return Json(uploaded);
        }

        /// <summary>
        /// Gets the filename of a path that will be sent to the FTP server.
        /// </summary>
        /// <param name="path">The path of the FTP server</param>
        /// <returns>The filename extracted from the path</returns>
        private string GetFileName(string path)
        {
            string[] words = path.Split('/');

            return words[words.Length - 1];
        }
    }
}
