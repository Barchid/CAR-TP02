using FluentFTP;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.IO;
using WebApi.Controllers;
using WebApi.Ftp;
using WebApi.Model;
using Xunit;

namespace Tests
{
    public class DirectoriesControllerTest
    {
        private readonly DirectoriesController _controller;
        private readonly Mock<IClient> _mockClient;

        public DirectoriesControllerTest()
        {
            _mockClient = new Mock<IClient>();
            _controller = new DirectoriesController(_mockClient.Object);
        }

        [Fact]
        public void TestListPathEmpty()
        {
            IActionResult response = _controller.List(null);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public void TestListNotFound()
        {
            _mockClient.Setup(client => client.ListDirectory("/boubou")).Returns<IClient, IEnumerable<FtpListItem>>(null);
            IActionResult response = _controller.List("/boubou");
            NotFoundObjectResult notFound = Assert.IsType<NotFoundObjectResult>(response);
            Assert.Equal(404, notFound.StatusCode);
        }

        [Fact]
        public void TestListSuccessful()
        {
            FtpListItem item = new FtpListItem();
            IEnumerable<FtpListItem> list = new FtpListItem[] { item };

            _mockClient.Setup(client => client.ListDirectory("/boubou")).Returns(list);

            IActionResult response = _controller.List("/boubou");
            JsonResult okResp = Assert.IsType<JsonResult>(response);
        }

        [Fact]
        public void TestDownloadPathEmpty()
        {
            IActionResult response = _controller.Download(null);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public void TestDownloadFailed()
        {
            _mockClient.Setup(client => client.DownloadDirectory("/bibite")).Returns<MemoryStream>(null);
            IActionResult response = _controller.Download("/bibite");
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(400, badRequest.StatusCode);
            Assert.Equal("The directory could not be downloaded. Please check if the specified path is valid.", badRequest.Value);
        }

        [Fact]
        public void TestCreatePathEmpty()
        {
            IActionResult response = _controller.Create(null);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public void TestCreateFailed()
        {
            _mockClient.Setup(client => client.AddDirectory("tchoin")).Returns(false);
            IActionResult response = _controller.Create("tchoin");
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(400, badRequest.StatusCode);
            Assert.Equal("Cannot create a directory of path tchoin", badRequest.Value);
        }

        [Fact]
        public void TestCreateSuccess()
        {
            _mockClient.Setup(client => client.AddDirectory("tchoin")).Returns(true);
            IActionResult response = _controller.Create("tchoin");
            OkObjectResult okResp = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(200, okResp.StatusCode);
            Assert.Equal("Directory created at path tchoin", okResp.Value);
        }

        [Fact]
        public void TestMoveBodyEmpty()
        {
            IActionResult response = _controller.Move(null);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public void TestMoveFailed()
        {
            MoveInput move = new MoveInput
            {
                OldPath = "Skouskou",
                TargetPath = "Banlieue sale"
            };

            _mockClient.Setup(client => client.Move("Skouskou", "Banlieue sale")).Returns(false);
            IActionResult response = _controller.Move(move);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(400, badRequest.StatusCode);
            Assert.Equal($"Directory could not be moved from path {move.OldPath} to path {move.TargetPath}", badRequest.Value);
        }

        [Fact]
        public void TestMoveSuccess()
        {
            MoveInput move = new MoveInput
            {
                OldPath = "Skouskou",
                TargetPath = "Banlieue sale"
            };

            _mockClient.Setup(client => client.Move("Skouskou", "Banlieue sale")).Returns(true);
            IActionResult response = _controller.Move(move);
            OkObjectResult okResp = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(200, okResp.StatusCode);
            Assert.Equal($"Directory moved from path {move.OldPath} to path {move.TargetPath}.", okResp.Value);
        }

        [Fact]
        public void TestDeletePathEmpty()
        {
            IActionResult response = _controller.Delete(null);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public void TestDeleteFailed()
        {
            _mockClient.Setup(client => client.RemoveDirectory("tchoin")).Returns(false);
            IActionResult response = _controller.Delete("tchoin");
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(400, badRequest.StatusCode);
            Assert.Equal("Directory could not be removed from path tchoin", badRequest.Value);
        }

        [Fact]
        public void TestDeleteSuccessful()
        {
            _mockClient.Setup(client => client.RemoveDirectory("tchoin")).Returns(true);
            IActionResult response = _controller.Delete("tchoin");
            OkObjectResult okResp = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(200, okResp.StatusCode);
            Assert.Equal("Directory removed from path tchoin.", okResp.Value);
        }

        [Fact]
        public void TestUploadEmpty()
        {
            IActionResult response = _controller.Upload(null, null);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public void TestUploadFailed()
        {
            FormFile formFile = new FormFile(null, 1, 1, "lol", "lol");

            _mockClient.Setup(client => client.UploadDirectory("tchoin", formFile)).Returns<IEnumerable<FtpListItem>>(null);
            IActionResult response = _controller.Upload("tchoin", formFile);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(400, badRequest.StatusCode);
            Assert.Equal("The archive cannot be uploaded in the specified path.", badRequest.Value);
        }

        [Fact]
        public void TestUploadSuccessful()
        {
            FormFile formFile = new FormFile(null, 1, 1, "lol", "lol");
            FtpListItem item = new FtpListItem();
            IEnumerable<FtpListItem> list = new FtpListItem[] { item };

            _mockClient.Setup(client => client.UploadDirectory("tchoin", formFile)).Returns(list);
            IActionResult response = _controller.Upload("tchoin", formFile);

            JsonResult jsonRes = Assert.IsType<JsonResult>(response);
        }
    }
}
