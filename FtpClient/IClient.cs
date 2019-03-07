namespace Ftp
{
    /// <summary>
    /// Interface that will encapsulate the 
    /// </summary>
    interface IClient
    {
        bool Rename(string remotePath, string newName);

        bool DeleteFile(string remotePath);

        bool UploadFile(string remotePath, string pathToUpload);

        bool AddDirectory(string remotePath);

        bool RemoveDirectory(string remotePath);

        string ListDirectory(string remotePath);
    }
}
