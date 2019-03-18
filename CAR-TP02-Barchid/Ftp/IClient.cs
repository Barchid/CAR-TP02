using FluentFTP;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;

namespace WebApi.Ftp
{
    /// <summary>
    /// Interface that defines the interaction between an FTP server and the local application.
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// Renames a file or a directory in the remote FTP server.
        /// </summary>
        /// <param name="remotePath">The path of the file/directory to rename</param>
        /// <param name="newPath">The new name of the file/directory</param>
        /// <returns>true if the file has been renamed successfully or else false</returns>
        bool Move(string remotePath, string newPath);

        /// <summary>
        /// Deletes a file in the remote FTP server.
        /// </summary>
        /// <param name="remotePath">The path of the file to delete in the remote FTP server</param>
        /// <returns></returns>
        bool DeleteFile(string remotePath);

        /// <summary>
        /// Uploads a file to the remote FTP server
        /// </summary>
        /// <param name="remotePath">The path in the remote FTP server where the file will be uploaded.</param>
        /// <param name="file">The file to upload</param>
        /// <returns></returns>
        bool UploadFile(string remotePath, IFormFile file);

        /// <summary>
        /// Downloads a file from the remote FTP server
        /// </summary>
        /// <param name="remotePath">The path in the remote FTP server of the file that will be uploaded.</param>
        /// <returns>The memory stream of the file that has been downloaded or null if the file does not exist</returns>
        MemoryStream DownloadFile(string remotePath);

        /// <summary>
        /// Adds a directory to the remote server.
        /// </summary>
        /// <param name="remotePath">The remote path of the new directory to create.</param>
        /// <returns>true if the new directory is created or else false.</returns>
        bool AddDirectory(string remotePath);

        /// <summary>
        /// Removes a directory from the remote FTP server.
        /// </summary>
        /// <param name="remotePath">The remote path of the directory to remove.</param>
        /// <returns>true if the directory is removed or else false</returns>
        bool RemoveDirectory(string remotePath);

        /// <summary>
        /// Lists the content of the specified directory
        /// </summary>
        /// <param name="remotePath">the path of the directory to list.</param>
        /// <returns>the list of the specified directory</returns>
        IEnumerable<FtpListItem> ListDirectory(string remotePath);

        /// <summary>
        /// Uploads the specified directory to the remote path in the FTP server
        /// </summary>
        /// <param name="remotePath">The path of the directory that will be uploaded</param>
        /// <param name="archive">The directory to upload</param>
        /// <returns>The listing of the uploaded directory or else null if the upload operation is not possible</returns>
        IEnumerable<FtpListItem> UploadDirectory(string remotePath, IFormFile archive);

        /// <summary>
        /// Downloads the directory specified by the remote path in parameter in the FTP server.
        /// </summary>
        /// <param name="remotePath">The path of the directory to download on the FTP server.</param>
        /// <returns>the path of the directory where the files are located</returns>
        MemoryStream DownloadDirectory(string remotePath);
    }
}
