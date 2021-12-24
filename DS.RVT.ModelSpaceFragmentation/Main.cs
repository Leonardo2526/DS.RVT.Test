using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using DS.RVT.ModelSpaceFragmentation.Path;

namespace DS.RVT.ModelSpaceFragmentation
{
    class Main
    {
        readonly Application App;
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;

        public Main(Application app, UIApplication uiapp, UIDocument uidoc, Document doc)
        {
            App = app;
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
        }

        void GetPath(PointsSeparator pointsSeparator)
        {
            InputData data = new InputData(PointsInfo.MinBoundPoint, PointsInfo.MaxBoundPoint, 
                pointsSeparator.UnpassablePoints);
            data.ConvertToPlane();

            WaveAlgorythm waveAlgorythm = new WaveAlgorythm(data);
            List<XYZ> PathCoords = waveAlgorythm.FindPath();
        }



    }
}
