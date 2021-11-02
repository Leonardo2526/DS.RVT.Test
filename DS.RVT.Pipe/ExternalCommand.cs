using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace DS.RVT.PipeTest
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
           ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application application = uiapp.Application;

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;

            DSPipe pipe = new DSPipe(uiapp, uidoc, doc);
            //pipe.CreatePipeSystem();
            //pipe.DeleteElement();
            pipe.SplitElement();


            TaskDialog.Show("Revit", "Pipe created!");
            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}