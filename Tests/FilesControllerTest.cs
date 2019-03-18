using Microsoft.AspNetCore.Mvc;
using Moq;
using System.IO;
using WebApi.Controllers;
using WebApi.Ftp;
using WebApi.Model;
using Xunit;

namespace Tests
{
    public class FilesControllerTest
    {
        private readonly FilesController _controller;
        private readonly Mock<IClient> _mockClient;

        public FilesControllerTest()
        {
            _mockClient = new Mock<IClient>();
            _controller = new FilesController(_mockClient.Object);
        }

        [Fact]
        public void DownloadEmptyPath()
        {
            IActionResult response = _controller.Download(null);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public void DownloadFailed()
        {
            _mockClient.Setup(client => client.DownloadFile("/")).Returns<MemoryStream>(null);

            IActionResult response = _controller.Download("/");
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public void UploadEmptyPath()
        {
            IActionResult response = _controller.Upload(null, null);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public void UploadFailed()
        {
            _mockClient.Setup(client => client.UploadFile("/yolo", null)).Returns(false);
            IActionResult response = _controller.Upload("/yolo", null);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public void UploadSuccessful()
        {
            _mockClient.Setup(client => client.UploadFile("/yolo", null)).Returns(true);
            IActionResult response = _controller.Upload("/yolo", null);
            OkObjectResult okResponse = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(200, okResponse.StatusCode);
            Assert.Equal("/yolo", okResponse.Value);
        }

        [Fact]
        public void RemoveEmptyPath()
        {
            IActionResult response = _controller.Remove(null);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public void RemoveFailed()
        {
            _mockClient.Setup(client => client.DeleteFile("/couille")).Returns(false);
            IActionResult response = _controller.Remove("/couille");
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public void RemoveSuccessful()
        {
            _mockClient.Setup(client => client.DeleteFile("/couille")).Returns(true);
            IActionResult response = _controller.Remove("/couille");
            OkObjectResult okResp = Assert.IsType<OkObjectResult>(response);
            Assert.Equal(200, okResp.StatusCode);
        }

        [Fact]
        public void MoveEmptyPath()
        {
            IActionResult response = _controller.Move(null);
            BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public void MoveFailed()
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
            Assert.Equal($"File could not be moved from path {move.OldPath} to path {move.TargetPath}", badRequest.Value);
        }

        [Fact]
        public void MoveSuccessful()
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
            Assert.Equal($"File moved from path {move.OldPath} to path {move.TargetPath}.", okResp.Value);
        }
    }
}
