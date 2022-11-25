using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCommand.PostCommandTest.Models
{
    internal class TestClass
    {
        private readonly UIApplication _uiApp;

        public TestClass(UIApplication uiApp)
        {
            _uiApp = uiApp;
        }


        public void Run()
        {
            OpenDoc();
            RunCommand();
        }
        private void OpenDoc()
        {
            string dirName = "e:\\YandexDisk\\Олимпроект\\Тесты\\MEPAC\\Тестовые модели\\90град\\Трубы\\0_Труба_(0,y,0)_[]_[]_Труба_(x,0,0)_[]_[]_90.rvt";
            _uiApp.OpenAndActivateDocument(dirName);
        }


        private void RunCommand()
        {
            string name = "8b231223-c983-49c8-b3a4-ab5c3ad81cda";

            RevitCommandId id_addin = RevitCommandId.LookupCommandId(name);
            _uiApp.PostCommand(id_addin);
            Debug.WriteLine("Command posted.");
        }
    }
}
