using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace DS.RVT.WaveAlgorythm
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
           ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;

            //Input and get all input data
            Data data = new Data();
            data.SetValues();

            Cell cell = new Cell(app, uiapp, doc, uidoc, data);
            //cell.GetCells();
            cell.GetCurves();

            //uidoc.RefreshActiveView();

            //List<XYZ> ICLocations = cell.FindCollisions();

           //uidoc.RefreshActiveView();
         

            //WaveAlgorythm waveAlgorythm = new WaveAlgorythm(app, uiapp, doc, uidoc, ICLocations, data, cell);
            //waveAlgorythm.FindPath();
             

            TaskDialog.Show("Revit", "Done!");
            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}