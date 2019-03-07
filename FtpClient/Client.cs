namespace Ftp
{
    /// <summary>
    /// FTP client that will handle the FTP interaction between the REST server and the remote FTP server.
    /// </summary>
    public class Client : IClient
    {
        public Client()
        {
        }

        private IFtpClient _ftp;

        public bool AddDirectory(string remotePath)
        {
            throw new System.NotImplementedException();
        }

        public bool DeleteFile(string remotePath)
        {
            throw new System.NotImplementedException();
        }

        public string ListDirectory(string remotePath)
        {
            throw new System.NotImplementedException();
        }

        public bool RemoveDirectory(string remotePath)
        {
            throw new System.NotImplementedException();
        }

        public bool Rename(string remotePath, string newName)
        {
            throw new System.NotImplementedException();
        }

        public bool UploadFile(string remotePath, string pathToUpload)
        {
            throw new System.NotImplementedException();
        }
    }
}
