﻿using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Catrobat.IDEWindowsPhone.Converters;
using Catrobat.IDEWindowsPhone.Misc;
using Catrobat.IDEWindowsPhone.Themes;
using Catrobat.IDEWindowsPhone.ViewModel.Editor.Costumes;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Catrobat.TestsWindowsPhone.Tests.IDE.Converter
{
    [TestClass]
    public class ImageSizeHeightConverterTests
    {
        [TestMethod]
        public void TestConversion()
        {
            var imageDimention = new ImageDimention { Width = 100, Height = 300 };

            var conv = new ImageSizeHeightConverter();
            object output = conv.Convert(ImageSize.Small, null, imageDimention, null);
            Assert.IsNotNull(output);
            Assert.IsTrue(output is int);
            Assert.AreEqual(300, output);
        }

        [TestMethod]
        public void TestBackConversion()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var conv = new ImageSizeHeightConverter();
                object output = conv.ConvertBack(null, null, null, null);
                Assert.AreEqual(null, output);
            });
        }

        [TestMethod]
        public void TestFaultyConversion()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var conv = new ImageSizeHeightConverter();
                object output = conv.Convert((object)"NotValid", null, null, null);
                Assert.AreEqual(0, output);
            });
        }
    }
}
