using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WebApi.Ftp;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using WebApi.Model;
using Microsoft.AspNetCore.Http;

namespace WebApi.Controllers
{
    /// <summary>
    /// Controller class used to manage all the operations related to the file management in an FTP server.
    /// </summary>
    [Produces("application/json")]
    [Route("api/Files")]
    public class FilesController : Controller
    {
        private readonly IClient _client;

        public FilesController(IClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Downloads the file specified by the path in query string.
        /// </summary>
        /// <param name="path">The path of the file to download</param>
        /// <returns>200 with the file when the download is successful.</returns>
        [HttpGet("download")]
        public IActionResult Download([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("The path is empty");
            }

            MemoryStream memory = _client.DownloadFile(path);
            if (memory != null)
            {
                return File(memory, GetContentType(GetFileName(path)), GetFileName(path));
            }
            else
            {
                return BadRequest("The file could not be downloaded. Please check if the specified path is valid.");
            }
        }

        /// <summary>
        /// Uploads a file to the specified path in the FTP server.
        /// </summary>
        /// <param name="path">The path in the FTP server where the file will be uploaded.</param>
        /// <param name="file">The file to upload.</param>
        /// <returns></returns>
        [HttpPost()]
        public IActionResult Upload([FromQuery] string path, IFormFile file)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("Path required");
            }

            bool result = _client.UploadFile(path, file);
            if (result)
            {
                return Ok(path);
            }
            else
            {
                return BadRequest("The path could not be uploaded.");
            }
        }

        /// <summary>
        /// Removes the file specified by the path from the FTP server
        /// </summary>
        /// <param name="path">the path of the file to remove from the FTP server.</param>
        /// <returns></returns>
        [HttpDelete()]
        public IActionResult Remove([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest("Path required");
            }

            bool result = _client.DeleteFile(path);
            if (result)
            {
                return Ok("File deleted successfully.");
            }
            else
            {
                return BadRequest("The specified file could not be deleted.");
            }
        }

        /// <summary>
        /// Moves the file specified by the path in query string to another .
        /// </summary>
        /// <param name="move">The new path of the file to move</param>
        /// <returns>200 if the move operation has been correctly processed or else false.</returns>
        [HttpPut]
        public IActionResult Move([FromBody] MoveInput move)
        {
            bool result = _client.Move(move.OldPath, move.TargetPath);
            if (result)
            {
                return Ok($"File moved from path ${move.OldPath} to path ${move.TargetPath}.");
            }
            else
            {
                return BadRequest($"File could not be moved from path ${move.OldPath} to path ${move.TargetPath}");
            }
        }

        /// <summary>
        /// Retrieves the content type of a name.
        /// </summary>
        /// <param name="filename">the filename of the file that we want to create find the content type</param>
        /// <returns>The content type of the file</returns>
        private string GetContentType(string filename)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filename, out string contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
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
