namespace WebApi.Ftp
{
    /// <summary>
    /// Interface that will encapsulate the 
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
        /// <param name="pathToUpload">The path of the file to </param>
        /// <returns></returns>
        bool UploadFile(string remotePath, string pathToUpload);

        /// <summary>
        /// Downloads a file from the remote FTP server
        /// </summary>
        /// <param name="remotePath">The path in the remote FTP server of the file that will be uploaded.</param>
        /// <param name="pathToUpload">The path of the local downloaded file.</param>
        /// <returns>true if the download operation succeeded or else false.</returns>
        bool DownloadFile(string remotePath, string pathToUpload);

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
        string ListDirectory(string remotePath);
    }
}
