using Autodesk.Revit.DB;
using DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.PathFindTest.Solution
{
    public class SketchSolutionModel : ICollisionSolutionModel
    {
        public SketchSolutionModel(IConnectionPoint point1, IConnectionPoint point2, 
            List<XYZ> path, Dictionary<FamilyInstance, Transform> famInstTransforms)
        {
            Point1 = point1;
            Point2 = point2;
            Path = path;
            FamInstTransforms = famInstTransforms;
        }

        public IConnectionPoint Point1 { get; }
        public IConnectionPoint Point2 { get; }
        public List<XYZ> Path { get; }
        public Dictionary<FamilyInstance, Transform> FamInstTransforms { get; }
    }
}
