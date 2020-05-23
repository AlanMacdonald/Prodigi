using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMatcher
{
    public class ImageOptions
    {
        public ImageOptions()
        {

        }

        public int ColorMatchPixelX { get; set; }
        public int ColorMatchPixelY { get; set; }

        public int RgbFuzziness { get; set; }

    }
}
