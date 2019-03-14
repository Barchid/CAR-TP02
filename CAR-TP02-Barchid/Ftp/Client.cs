using FluentFTP;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        private readonly IHostingEnvironment _enviroment;

        public Client(FtpContext ftpContext, IConfiguration configuration, IHostingEnvironment environment)
        {
            _ftpContext = ftpContext;
            _port = int.Parse(configuration["Port"]);
            _host = configuration["Host"];
            _enviroment = environment;
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

        public IEnumerable<FtpListItem> ListDirectory(string remotePath)
        {
            FtpClient ftpClient = ConnectToFtp();
            IEnumerable<FtpListItem> listing = null;

            if (ftpClient.DirectoryExists(remotePath))
            {
                listing = ftpClient.GetListing(remotePath);
            }

            ftpClient.Disconnect();
            return listing;
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

            bool isDeleted = !ftpClient.DirectoryExists(remotePath);
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

        public bool UploadFile(string remotePath, IFormFile file)
        {
            FtpClient ftpClient = ConnectToFtp();

            if (ftpClient.FileExists(remotePath))
            {
                ftpClient.Disconnect();
                return false;
            }

            string tmpPath = Path.Combine(_enviroment.WebRootPath, Guid.NewGuid().ToString());

            using (var stream = new FileStream(tmpPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            bool result = ftpClient.UploadFile(tmpPath, remotePath, FtpExists.Overwrite, true);

            File.Delete(tmpPath);

            ftpClient.Disconnect();
            return result;
        }

        public MemoryStream DownloadFile(string remotePath)
        {
            FtpClient ftpClient = ConnectToFtp();
            if (!ftpClient.FileExists(remotePath))
            {
                ftpClient.Disconnect();
                return null;
            }

            string tmpPath = Path.Combine(_enviroment.WebRootPath, Guid.NewGuid().ToString());
            bool isDownloadSuccessful = ftpClient.DownloadFile(tmpPath, remotePath);
            MemoryStream memory = null;
            if (isDownloadSuccessful)
            {
                memory = new MemoryStream();
                using (FileStream stream = new FileStream(tmpPath, FileMode.Open))
                {
                    stream.CopyTo(memory);
                }
                memory.Position = 0;

                File.Delete(tmpPath);
            }



            ftpClient.Disconnect();
            return memory;
        }

        private FtpClient ConnectToFtp()
        {
            FtpClient ftpClient = new FtpClient(_host, _port, new NetworkCredential(_ftpContext.User, _ftpContext.Pass));
            ftpClient.Connect();
            return ftpClient;
        }

        public bool UploadDirectory(string remotePath, IFormFile directory)
        {
            throw new NotImplementedException();
        }

        public string DownloadDirectory(string remotePath)
        {
            throw new NotImplementedException();
        }
    }
}
