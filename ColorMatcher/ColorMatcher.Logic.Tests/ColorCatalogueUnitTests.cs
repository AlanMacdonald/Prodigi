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
    public class ColorCatalogueUnitTests
    {

        [TestMethod]
        public void WhenConstructed_CatalogueByArgbIsSet()
        {
            var catalogue = new Dictionary<string, ColorWrapper>();
            catalogue.Add("colorName1", new ColorWrapper(Color.FromArgb(255, 1, 1, 1)));
            catalogue.Add("colorName2", new ColorWrapper(Color.FromArgb(255, 2, 2, 2)));
            catalogue.Add("colorName3", new ColorWrapper(Color.FromArgb(255, 3, 3, 3)));

            var colorCatalogue = new ColorCatalogue(catalogue);

            Assert.AreEqual(3, colorCatalogue.CatalogueByArgb.Count);
            
            foreach(var colorWrapper in catalogue)
            {
                

                Assert.AreEqual(true, colorCatalogue.CatalogueByArgb.ContainsKey(colorWrapper.Value.Color.ToArgb()), "Expected to find color in reverse dictionary by Argb");

                var item = colorCatalogue.CatalogueByArgb[colorWrapper.Value.Color.ToArgb()];

                Assert.AreEqual(colorWrapper.Key, item.ColorNameFromCatalogue);
                Assert.AreEqual(colorWrapper.Value.Color, item.Color);
            }
        }

        [TestMethod]
        public void WhenConstructed_CatalogueByRgbSumIsSet()
        {
            var catalogue = new Dictionary<string, ColorWrapper>();
            catalogue.Add("colorName1", new ColorWrapper(Color.FromArgb(255, 1, 1, 1)));
            catalogue.Add("colorName2", new ColorWrapper(Color.FromArgb(255, 2, 2, 2)));
            catalogue.Add("colorName3", new ColorWrapper(Color.FromArgb(255, 3, 3, 3)));

            var colorCatalogue = new ColorCatalogue(catalogue);

            Assert.AreEqual(3, colorCatalogue.CatalogueByRgbSum.Count);

            foreach (var colorWrapper in catalogue)
            {
                var rgbSum = colorWrapper.Value.Color.R + colorWrapper.Value.Color.G + colorWrapper.Value.Color.B;
                var reverseLookupItem = colorCatalogue.CatalogueByRgbSum[rgbSum].ToList();

                Assert.AreEqual(1, reverseLookupItem.Count, "Expected to find one color in reverse dictionary by RGB Sum");

                var item = reverseLookupItem.First();

                Assert.AreEqual(colorWrapper.Key, item.ColorNameFromCatalogue);
                Assert.AreEqual(colorWrapper.Value.Color, item.Color);
            }
        }

    }
}
