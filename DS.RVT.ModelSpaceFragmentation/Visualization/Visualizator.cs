using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using DS.RVT.ModelSpaceFragmentation.Path;

namespace DS.RVT.ModelSpaceFragmentation.Visualization
{
    class Visualizator
    {
        readonly Document Doc;

        public Visualizator(Document doc)
        {
            Doc = doc;
        }

        public void ShowPoints(IPointsVisualization pointsVisualization)
        {
            pointsVisualization.Show(Doc);
        }
    }
}
