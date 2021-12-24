using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using DS.RVT.ModelSpaceFragmentation.Path;


namespace DS.RVT.ModelSpaceFragmentation
{
    class PathFinder
    {
        public List<XYZ> PathCoords { get; set;}
        public List<XYZ> GetPath(XYZ startPoint, XYZ endPoint, List<XYZ> unpassablePoints)
        {
            InputData data = new InputData(startPoint, endPoint, unpassablePoints);
            data.ConvertToPlane();

            WaveAlgorythm waveAlgorythm = new WaveAlgorythm(data);
            PathCoords = waveAlgorythm.FindPath();

            return PathCoords;
        }

    }
}
