using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Net.Http;

namespace DS.RevitCmd.Template1
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

            var value = "AliceJ";
            var path = "addq";
            //TestHtmlClass.Test6(new HttpClient(), path, value);
            //HtmlClass.RunJson(new HttpClient(), "addq", value);
            HtmlClass.RunQuery(new HttpClient(), path, value);
            //PersonClient.Test5(new HttpClient(), "addj");

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}