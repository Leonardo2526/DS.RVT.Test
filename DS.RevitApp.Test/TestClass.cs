using NUnit.Framework;
using RTF.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
    [TestFixture]
    public class TestClass
    {
        [Test, TestModel(@"E:\YandexDisk\Олимпроект\Тесты\MEPAC\Тестовые модели\Прочее\Template.rvt")]
        public void SomeTest()
        {
            Assert.AreEqual(1, 1);
        }
    }
}
