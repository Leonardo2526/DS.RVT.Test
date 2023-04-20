using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils.Various;
using PathFinderLib;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
    internal class SerilogTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private string _path;

        public SerilogTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;

            var dir = new DirectoryInfo(LogPath);
            if(dir.Exists ) { dir.Delete(true); }
            

            BuildLog();
            Log.Debug("Foo started");

            Run();

            Log.CloseAndFlush();
        }

        public string LogPath
        {
            get
            {
                if(_path == null )
                {
                    string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                    UriBuilder uri = new UriBuilder(codeBase);
                    string path = Uri.UnescapeDataString(uri.Path);
                    _path= System.IO.Path.GetDirectoryName(path) + "//logs";
                }
                return _path;
            }
        }

        private void BuildLog()
        {
            Log.Logger = new LoggerConfiguration()
      .MinimumLevel.Debug()
      .WriteTo.Debug()
      .WriteTo.File(LogPath + "//my_log.log", rollingInterval: RollingInterval.Day)
       .WriteTo.RollingFile(new CompactJsonFormatter(), LogPath + "//app-{Date}.json", Serilog.Events.LogEventLevel.Information)
      .CreateLogger();
        }

        private void Run()
        {
            var elem1= new ElementSelector(_uiDoc).Pick();
            Log.Information("Selected id: {@elemId}", elem1.Id);
            //Log.Information("Selected id: {e}", elem1.Id);
        }
    }
}
