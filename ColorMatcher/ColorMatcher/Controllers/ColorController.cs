using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ColorMatcher.Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Net;

namespace ColorMatcher.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Route("/colour")] //Support British English in case customer hits this.
    public class ColorController : ControllerBase
    {

        private readonly ILogger<ColorController> logger;
        private ColorMatcher.Logic.IColorMatcher colorMatcher;
        private IDownloader downloader;
        IOptions<ImageOptions> imageOptions;

        public ColorController(ILogger<ColorController> logger, IDownloader downloader, ColorMatcher.Logic.IColorMatcher colorMatcher, IOptions<ImageOptions> imageOptions)
        {
            this.logger = logger;
            this.downloader = downloader;
            this.colorMatcher = colorMatcher;
            this.imageOptions = imageOptions;
        }

        [HttpGet("{encodedImageUri}")]
        public async Task<string> Get(string encodedImageUri)
        {
            if (string.IsNullOrEmpty(encodedImageUri))
                return "Image uri must be provided to color match against.";

            try
            {
                var decodedImageUri = System.Web.HttpUtility.UrlDecode(encodedImageUri);

                var imageResponse = await downloader.GetImageFromUri(decodedImageUri);

                if (imageResponse.Success)
                {
                    var match = colorMatcher.MatchPixel(imageResponse.Image, this.imageOptions.Value.ColorMatchPixelX, this.imageOptions.Value.ColorMatchPixelY, this.imageOptions.Value.RgbFuzziness);

                    if (match.MatchFound)
                    {
                        return match.ColorNameFromCatalogue;
                    }
                    else
                    {
                        return "No matching color found in predefined color catalogue";
                    }
                }
                else
                {
                    return imageResponse.ErrorMsg;
                }
            }
            catch (System.Net.Http.HttpRequestException httpEx)
            {
                logger.LogError(httpEx, $"Call to {nameof(ColorController)} with uri {encodedImageUri}");
                Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                return $"{httpEx.Message} Please check the image uri {encodedImageUri} you provided is valid and encoded properly."; ;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Call to {nameof(ColorController)} with uri {encodedImageUri}");
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return $"Internal Server Error occurred. Please check the image uri {encodedImageUri} you provided is valid and encoded properly.";
            }
        }
    }
}
