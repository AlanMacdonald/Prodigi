using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ColorMatcher.Logic.Tests
{
    [TestClass]
    public class DownloaderUnitTests
    {
        IHttpClientFactory httpClientFactory;
        HttpClient httpClient;
        MockHttpMessageHandler httpMessageHandler;
        Downloader downloader;

        [TestInitialize]
        public void Initialize()
        {
            httpClientFactory = Substitute.For<IHttpClientFactory>();
            var logger = Substitute.For<ILogger<Downloader>>();
            httpMessageHandler = new MockHttpMessageHandler();
            httpClient = new HttpClient(httpMessageHandler);
            
            httpClientFactory.CreateClient().Returns(httpClient);
            downloader = new Downloader(httpClientFactory, logger);
        }

        [TestMethod]
        public async Task WhenImageDoesNotExist_ReturnsError()
        {
            var uri = "http://mock/image.png";
            httpMessageHandler.StatusCode = HttpStatusCode.NotFound;
            httpMessageHandler.StringResponse = "Mock Response";

            (bool Success, Image Image, string ErrorMsg) expected = (Success: false,
                Image: null,
                ErrorMsg: $"Failed to acquire image from provided url {uri}.{Environment.NewLine}{(int)httpMessageHandler.StatusCode} Not Found");
            Assert.AreEqual(expected, await downloader.GetImageFromUri(uri));
        }

        [TestMethod]
        public async Task WhenImageDoesExist_ReturnsSuccess()
        {
            var uri = "http://mock/image.png";
            httpMessageHandler.StatusCode = HttpStatusCode.OK;
            var imagePath = Path.Combine("SampleImages", "test-sample-black.png");
            var image = (Bitmap)Image.FromFile(imagePath);

            using (var fileStream = File.OpenRead(imagePath))
            {
                httpMessageHandler.StreamResponse = fileStream;

                (bool Success, Bitmap Image, string ErrorMsg) expected = (Success: true,
                    Image: image,
                    ErrorMsg: null);

                var actual = await downloader.GetImageFromUri(uri);
                Assert.AreEqual(expected.Success, actual.Success);

                //For image equality we'll just do some rudimentary checks
                //as pixel to pixel comparisons are not suitable for unit testing
                //due to speed on large images
                Assert.AreEqual(expected.Image.Size, actual.Image.Size, "Image sizes are expected to match");
                Assert.AreEqual(expected.Image.Flags, actual.Image.Flags, "Images flags are expected to match");

                var random = new Random();
                for (int i = 0; i < 10; i++)
                {
                    var randomX = random.Next(0, expected.Image.Width - 1);
                    var randomY = random.Next(0, expected.Image.Height - 1);
                    Assert.AreEqual(expected.Image.GetPixel(randomX, randomY),
                                    actual.Image.GetPixel(randomX, randomY),
                                    "Randomly selected pixels should match");
                }
                    
            }   
        }
    }
}
