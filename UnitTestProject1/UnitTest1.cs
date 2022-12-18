using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using System;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [Test]
        public void SomeTest()
        {
            XYZ xyz = new XYZ(1, 2, 3);
            XYZ test = xyz.Add(new XYZ(5, 6, 7));
            Assert.AreEqual(test.X, 6);
            Assert.AreEqual(test.Y, 8);
            Assert.AreEqual(test.Z, 10);
        }
    }
}
