using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ColorMatcher.Logic.Tests
{
    [TestClass]
    public class ColorMatcherUnitTests
    {
        IColorCatalogue colorCatalogue;
        ColorMatcher colorMatcher;

        [TestInitialize]
        public void Initialize()
        {
            colorCatalogue = Substitute.For<IColorCatalogue>();
            var logger = Substitute.For<ILogger<Downloader>>();
            colorMatcher = new ColorMatcher(colorCatalogue);
        }

        [TestMethod]
        public void MatchPixel_WhenCatalogueDoesNotContainCloseMatchingColor_ReturnsFalse()
        {
            var bitmap = (Bitmap)Image.FromFile(Path.Combine("SampleImages", "test-sample-teal.png"));

            var catalogue = new Dictionary<int, (string ColorNameFromCatalogue, Color Color)>();

            catalogue.Add(100, (ColorNameFromCatalogue: "Dummy1", Color: Color.FromArgb(1, 1, 1)));
            catalogue.Add(200, (ColorNameFromCatalogue: "Dummy2", Color: Color.FromArgb(2, 2, 2)));

            colorCatalogue.CatalogueByArgb.Returns(catalogue);
            colorCatalogue.CatalogueByRgbSum.Returns(catalogue.ToLookup(kvp => kvp.Value.Color.R + kvp.Value.Color.G + kvp.Value.Color.B, kvp => kvp.Value));

            var actual = colorMatcher.MatchPixel(bitmap, 0, 0, 2);

            Assert.AreEqual(false, actual.MatchFound, "Color was not expected to be found in catalogue");
            Assert.AreEqual(null, actual.ColorNameFromCatalogue, "Color name should be null when no match");
            Assert.AreEqual(null, actual.Color, "Color should be null when no match");
        }

        [TestMethod]
        public void MatchPixel_WhenCatalogueDoesContainExactColor_ReturnsMatchingColor()
        {
            var bitmap = (Bitmap)Image.FromFile(Path.Combine("SampleImages", "test-sample-teal.png"));
            var catalogue = new Dictionary<int, (string ColorNameFromCatalogue, Color Color)>();

            catalogue.Add(100, (ColorNameFromCatalogue: "Dummy1", Color: Color.FromArgb(1,1,1)));

            var colorToMatch = Color.FromArgb(255, 0, 98, 110);
            catalogue.Add(colorToMatch.ToArgb(), (ColorNameFromCatalogue: "teal", Color: colorToMatch));

            catalogue.Add(200, (ColorNameFromCatalogue: "Dummy2", Color: Color.FromArgb(2, 2, 2)));

            colorCatalogue.CatalogueByArgb.Returns(catalogue);
            colorCatalogue.CatalogueByRgbSum.Returns(catalogue.ToLookup(kvp => kvp.Value.Color.R + kvp.Value.Color.G + kvp.Value.Color.B, kvp => kvp.Value));

            var actual = colorMatcher.MatchPixel(bitmap, 0, 0, 0);

            Assert.AreEqual(true, actual.MatchFound, "Color was expected to be found in catalogue");
            Assert.AreEqual("teal", actual.ColorNameFromCatalogue, "Color name should be returned with match");
            Assert.AreEqual(colorToMatch, actual.Color, "Color should be returned with match");
        }

        [TestMethod]
        public void MatchPixel_WhenCatalogueDoesContainCloseMatchingColor_ReturnsMatchingColor()
        {
            var bitmap = (Bitmap)Image.FromFile(Path.Combine("SampleImages", "test-sample-teal.png"));
            var catalogue = new Dictionary<int, (string ColorNameFromCatalogue, Color Color)>();

            catalogue.Add(100, (ColorNameFromCatalogue: "Dummy1", Color: Color.FromArgb(1, 1, 1)));

            var rgbFuzziness = 2;
            
            //Set up a color close to the teal in the catalogue
            var colorToMatch = Color.FromArgb(255, 0 + rgbFuzziness, 98 - rgbFuzziness, 110 + rgbFuzziness);
            catalogue.Add(colorToMatch.ToArgb(), (ColorNameFromCatalogue: "tealish", Color: colorToMatch));

            catalogue.Add(200, (ColorNameFromCatalogue: "Dummy2", Color: Color.FromArgb(2, 2, 2)));

            colorCatalogue.CatalogueByArgb.Returns(catalogue);
            colorCatalogue.CatalogueByRgbSum.Returns(catalogue.ToLookup(kvp => kvp.Value.Color.R + kvp.Value.Color.G + kvp.Value.Color.B, kvp => kvp.Value));

            var actual = colorMatcher.MatchPixel(bitmap, 0, 0, rgbFuzziness);

            Assert.AreEqual(true, actual.MatchFound, "Color was expected to be found in catalogue");
            Assert.AreEqual("tealish", actual.ColorNameFromCatalogue, "Color name should be returned with match");
            Assert.AreEqual(colorToMatch, actual.Color, "Color should be returned with match");
        }
    }
}
