using FluentFTP;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        /// <summary>
        /// Connects to the FTP server by using the user and password placed in the FTP context.
        /// </summary>
        /// <returns>the ftp client that is connected to the FTP server</returns>
        private FtpClient ConnectToFtp()
        {
            FtpClient ftpClient = new FtpClient(_host, _port, new NetworkCredential(_ftpContext.User, _ftpContext.Pass));
            ftpClient.Connect();
            return ftpClient;
        }

        public IEnumerable<FtpListItem> UploadDirectory(string remotePath, IFormFile archive)
        {
            FtpClient ftpClient = ConnectToFtp();
            if (ftpClient.DirectoryExists(remotePath))
            {
                ftpClient.Disconnect();
                return null;
            }

            string tmpPath = Path.Combine(_enviroment.WebRootPath, Guid.NewGuid().ToString());

            using (var stream = new FileStream(tmpPath, FileMode.Create))
            {
                archive.CopyTo(stream);
            }

            ZipFile.ExtractToDirectory(tmpPath, $"{tmpPath}directory");
            File.Delete(tmpPath);
            DirectoryInfo directoryInfo = new DirectoryInfo($"{tmpPath}directory");
            UploadTree(remotePath, directoryInfo, ftpClient);

            IEnumerable<FtpListItem> result = ftpClient.GetListing(remotePath);
            ftpClient.Disconnect();
            Directory.Delete($"{tmpPath}directory", true);
            return result;
        }

        private void UploadTree(string parentRemotePath, DirectoryInfo parent, FtpClient ftpClient)
        {
            foreach (FileInfo file in parent.EnumerateFiles())
            {
                string remotePath = $"{parentRemotePath}/{file.Name}";
                ftpClient.UploadFile(file.FullName, remotePath, FtpExists.Overwrite, true);
            }

            foreach (DirectoryInfo directory in parent.EnumerateDirectories())
            {
                string remotePath = $"{parentRemotePath}/{directory.Name}";
                ftpClient.CreateDirectory(remotePath);
                UploadTree(remotePath, directory, ftpClient);
            }
        }

        public MemoryStream DownloadDirectory(string remotePath)
        {
            FtpClient ftpClient = ConnectToFtp();
            if (!ftpClient.DirectoryExists(remotePath))
            {
                ftpClient.Disconnect();
                return null;
            }
            string directoryName = Path.Combine(_enviroment.WebRootPath, GetFileName(remotePath));
            Directory.CreateDirectory(directoryName);
            DownloadTree(ftpClient, remotePath, directoryName);

            string zipPath = $"{directoryName}.zip";
            ZipFile.CreateFromDirectory(directoryName, zipPath);

            Directory.Delete(directoryName, true);

            MemoryStream memory = new MemoryStream();
            using (FileStream stream = new FileStream(zipPath, FileMode.Open))
            {
                stream.CopyTo(memory);
            }
            memory.Position = 0;
            File.Delete(zipPath);

            ftpClient.Disconnect();
            return memory;
        }

        /// <summary>
        /// Downloads the file tree in the directory of remote FTP server specified by path.
        /// The tree will be added in a local directory specified in the parent path
        /// </summary>
        /// <param name="ftpClient">The client used to communicate with the FTP server</param>
        /// <param name="path">The path of the remote FTP directory</param>
        /// <param name="parent">The parent directory where the tree will be downloaded.</param>
        private void DownloadTree(FtpClient ftpClient, string path, string parent)
        {
            IEnumerable<FtpListItem> items = ftpClient.GetListing(path);
            foreach (FtpListItem item in items)
            {
                if (item.Type == FtpFileSystemObjectType.Directory)
                {
                    string directoryName = Path.Combine(parent, item.Name);
                    Directory.CreateDirectory(directoryName);
                    DownloadTree(ftpClient, item.FullName, directoryName);
                }
                else
                {
                    string fileName = Path.Combine(parent, item.Name);
                    ftpClient.DownloadFile(fileName, item.FullName, FtpLocalExists.Overwrite);
                }
            }
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
