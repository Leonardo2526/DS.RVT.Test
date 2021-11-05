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


          

            Cell cell = new Cell(app, uiapp, doc, uidoc);
            cell.GetCells();

            
            List<XYZ> ICLocations = cell.FindCollisions();

            //uidoc.RefreshActiveView();

            XYZ startPoint = new XYZ(0, 0, 0);
            XYZ endPoint = new XYZ(1000, 800, 0);

            WaveAlgorythm waveAlgorythm = new WaveAlgorythm(app, uiapp, doc, uidoc, ICLocations, 
                startPoint, endPoint, cell.AreaSize, cell.AreaSize, cell.CellSize, cell);
            waveAlgorythm.FindPath();


            TaskDialog.Show("Revit", "Done!");
            return Autodesk.Revit.UI.Result.Succeeded;
        } 
    }
}