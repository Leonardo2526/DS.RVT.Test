using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using DS.RVT.ModelSpaceFragmentation.Path;

namespace DS.RVT.ModelSpaceFragmentation.Visualization
{
    class PointsVisualizator
    {
        readonly Document Doc;

        public PointsVisualizator(Document doc)
        {
            Doc = doc;
        }

        public void Show(IPointsVisualization pointsVisualization)
        {
            pointsVisualization.Show(Doc);
        }
    }
}
