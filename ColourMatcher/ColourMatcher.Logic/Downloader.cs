using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ColorMatcher.Logic
{
    public interface IDownloader
    {
        Task<(bool Success, Bitmap Image, string ErrorMsg)> GetImageFromUri(string uri);
    }

    public class Downloader : IDownloader
    {
        IHttpClientFactory httpClientFactory;
        ILogger<Downloader> logger;

        public Downloader(IHttpClientFactory httpClientFactory, ILogger<Downloader> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        public async Task<(bool Success, Bitmap Image, string ErrorMsg)> GetImageFromUri(string uri)
        {
            var response = await httpClientFactory.CreateClient().GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                var readStream = await response.Content.ReadAsStreamAsync();
                var image = Image.FromStream(readStream);

                if (image is Bitmap)
                {
                    return (Success: true, Image: image as Bitmap, ErrorMsg: null);
                }
                else
                {
                    return (Success: false, Image: null, ErrorMsg: $"Unsupported Image format {image.RawFormat} used");
                }
            }
            else
            {
                string message = $"Failed to acquire image from provided url {uri}.{Environment.NewLine}{(int)response.StatusCode} {response.ReasonPhrase}";

                logger.LogError(message);

                return (Success: false, Image: null, ErrorMsg: message);

            }
        }
    }
}
