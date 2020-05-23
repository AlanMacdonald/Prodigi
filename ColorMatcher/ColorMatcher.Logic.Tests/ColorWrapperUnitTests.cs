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
    public class ColorWrapperUnitTests
    {
        ColorWrapper colorWrapper;

        [TestInitialize]
        public void Initialize()
        {
            colorWrapper = new ColorWrapper(Color.FromArgb(255, 100, 100, 100));
        }

        [TestMethod]
        public void WhenRedPropertyChanges_ColorChanges()
        {
            colorWrapper.Red = 200;
            Assert.AreEqual(Color.FromArgb(255, 200, 100, 100), colorWrapper.Color);
        }

        [TestMethod]
        public void WhenGreenPropertyChanges_ColorChanges()
        {
            colorWrapper.Green = 200;
            Assert.AreEqual(Color.FromArgb(255, 100, 200, 100), colorWrapper.Color);
        }

        [TestMethod]
        public void WhenBluePropertyChanges_ColorChanges()
        {
            colorWrapper.Blue = 200;
            Assert.AreEqual(Color.FromArgb(255, 100, 100, 200), colorWrapper.Color);
        }
    }
}
