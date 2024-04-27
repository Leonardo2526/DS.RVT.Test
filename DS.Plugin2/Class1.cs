using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitApp.Test;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DS.Plugin3
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
           ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;

            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiApp.ActiveUIDocument.Document;

            AppDomain domain = AppDomain.CreateDomain("Domain2", null);
            GetAllAssemblies(domain);


            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            var filePath3 = Path.GetDirectoryName(path);
            //var s = System.Reflection.Assembly.GetEntryAssembly().Location;
            //MessageBox.Show(filePath3);1`


            //GC.Collect(); // collects all unused memory
            //GC.WaitForPendingFinalizers(); // wait until GC has finished its work
            //GC.Collect();


            var asm = Assembly.LoadFrom(filePath3 + @"\DS.RevitApp.Test.dll");
            var types = asm.GetTypes();
            var type = types.FirstOrDefault(obj => obj.Name == "MessageCaller");
            //var t = asm.GetType("MessageCaller");
            //Type type = typeof(MessageCaller);
            if (type is not null)
            {
                // получаем метод Square
                MethodInfo? mesCall = type.GetMethod("Call", BindingFlags.Public | BindingFlags.Static);

                // вызываем метод, передаем ему значения для параметров и получаем результат
                object? result = mesCall?.Invoke(null, new object[] { });
            }

            //MessageCaller.Call();

            GetAllAssemblies(AppDomain.CurrentDomain);

            return Autodesk.Revit.UI.Result.Succeeded;
        }

        public void GetAllAssemblies(AppDomain domain)
        {
            var sb = new StringBuilder();
            sb.Append($"Name: {domain.FriendlyName}\n");

            Debug.WriteLine($"Name: {domain.FriendlyName}\n");
            sb.Append($"Base Directory: {domain.BaseDirectory}\n");

            Assembly[] assemblies = domain.GetAssemblies();
            foreach (Assembly asm in assemblies)
                sb.Append(asm.GetName().Name + "\n");

            Debug.WriteLine(sb.ToString());
            //MessageBox.Show(sb.ToString());
        }
    }
}