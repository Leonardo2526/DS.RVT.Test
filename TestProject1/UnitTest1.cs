using Autodesk.Revit.DB;

namespace TestProject1
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }

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