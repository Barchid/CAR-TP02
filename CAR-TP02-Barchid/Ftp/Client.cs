using FluentFTP;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net;
using WebApi.Tools;

namespace WebApi.Ftp
{
    /// <summary>
    /// FTP client that will handle the FTP interaction between the REST server and the remote FTP server.
    /// </summary>
    public class Client : IClient
    {
        private readonly FtpContext _ftpContext;

        private readonly int _port = 21;
        private readonly string _host;

        public Client(FtpContext ftpContext, IConfiguration configuration)
        {
            _ftpContext = ftpContext;
            _port = int.Parse(configuration["Port"]);
            _host = configuration["Host"];
        }

        public bool AddDirectory(string remotePath)
        {
            FtpClient ftpClient = ConnectToFtp();
            ftpClient.CreateDirectory(remotePath, true);

            bool result = ftpClient.DirectoryExists(remotePath);
            ftpClient.Disconnect();

            return result;
        }

        public bool DeleteFile(string remotePath)
        {
            FtpClient ftpClient = ConnectToFtp();

            if (!ftpClient.FileExists(remotePath))
            {
                ftpClient.Disconnect();
                return false;
            }

            ftpClient.DeleteFile(remotePath);

            if (ftpClient.FileExists(remotePath))
            {
                ftpClient.Disconnect();
                return false;
            }

            ftpClient.Disconnect();
            return true;
        }

        public string ListDirectory(string remotePath)
        {
            throw new System.NotImplementedException();
        }

        public bool RemoveDirectory(string remotePath)
        {
            FtpClient ftpClient = ConnectToFtp();

            if (!ftpClient.DirectoryExists(remotePath))
            {
                ftpClient.Disconnect();
                return false;
            }

            ftpClient.DeleteDirectory(remotePath);

            bool isDeleted = ftpClient.DirectoryExists(remotePath);
            ftpClient.Disconnect();
            return isDeleted;
        }

        public bool Move(string remotePath, string newPath)
        {
            FtpClient ftpClient = ConnectToFtp();
            bool isMoved = false;

            if (ftpClient.FileExists(remotePath))
            {
                isMoved = ftpClient.MoveFile(remotePath, newPath);
            }
            else if (ftpClient.DirectoryExists(remotePath))
            {
                isMoved = ftpClient.MoveDirectory(remotePath, newPath);
            }

            ftpClient.Disconnect();
            return isMoved;
        }

        public bool UploadFile(string remotePath, string pathToUpload)
        {
            FtpClient ftpClient = ConnectToFtp();
            ftpClient.Disconnect();
            return true;
        }

        private FtpClient ConnectToFtp()
        {
            FtpClient ftpClient = new FtpClient(_host, _port, new NetworkCredential(_ftpContext.User, _ftpContext.Pass));
            ftpClient.Connect();
            return ftpClient;
        }

        public bool DownloadFile(string remotePath, string pathToUpload)
        {
            throw new System.NotImplementedException();
        }
    }
}
