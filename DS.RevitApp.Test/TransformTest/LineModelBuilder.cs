using Autodesk.Revit.DB;
using DS.RevitApp.Test.PathFindTest.ConnectionPointService.PointModel;
using DS.RevitApp.Test.PathFindTest.Solution;
using DS.RevitLib.Utils.MEP.Models;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Models;
using DS.RevitLib.Utils.Solids.Models;
using OLMP.RevitLib.MEPAC.Collisons.Resolvers.MEPBypass.ElementsTransfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;
using DS.RevitLib.Utils.Extensions;

namespace DS.RevitApp.Test.TransformTest
{
    internal class LineModelBuilder
    {
        private readonly List<XYZ> _path;
        private readonly SketchSolutionModel _sketchSolutionModel;
        private readonly double _elbowRadius;
        private readonly List<Element> _elementsToDelete;
        private ConnectionPoint _p1;

        public LineModelBuilder(List<XYZ> path, SketchSolutionModel sketchSolutionModel, double elbowRadius, List<Element> elementsToDelete)
        {
            _path = path;
            _sketchSolutionModel = sketchSolutionModel;
            _elbowRadius = elbowRadius;
            _elementsToDelete = elementsToDelete;
            _p1 = sketchSolutionModel.Point1 as ConnectionPoint;
        }

        public List<Line> Lines { get; private set; } = new List<Line>();
        public List<LineModel> LineModels { get; private set; } = new List<LineModel>();
        public MEPCurveModel StartMEPCurveModel { get; private set; }


        public List<Line> GetLines()
        {
            for (int i = 0; i < _path.Count - 1; i++)
            {
                XYZ dir = (_path[i + 1] - _path[i]).Normalize();

                XYZ p1 = _path[i] + dir.Multiply(_elbowRadius);
                if (!p1.IsBetweenPoints(_path[i], _path[i+1]))
                {
                    throw new InvalidOperationException($"Point {p1} is not between points {_path[i]} and {_path[i+1]}");
                }
                XYZ p2 = _path[i + 1] - dir.Multiply(_elbowRadius);
                if (!p2.IsBetweenPoints(p1, _path[i + 1]))
                {
                    throw new InvalidOperationException($"Point {p2} is not between points {p1} and {_path[i + 1]}");
                }

                var line = Line.CreateBound(p1, p2);
                line.Show(_p1.Element.Document);
                Lines.Add(line);
            }

            return Lines;
        }

        public List<LineModel> Build()
        {
            GetLines();

            var startLineModel = GetStartLineModel(_sketchSolutionModel);
            Basis initBasis = startLineModel.Basis.Clone();
            //initBasis.Show(_doc);
            //_uidoc.RefreshActiveView();

            foreach (var line in Lines)
            {
                var trModel = new BasisLineTransformBuilder(initBasis, line).Build();
                initBasis.Transform(trModel.Transforms);

                var lineModel = new LineModel(line, initBasis);
                LineModels.Add(lineModel);
                initBasis = initBasis.Clone();
            }

            return LineModels;
        }

        private LineModel GetStartLineModel(SketchSolutionModel sketchSolutionModel)
        { 
            MEPCurve startMEPCurve = null;

            var deletedIds = _elementsToDelete.Select(obj => obj.Id);

            if (_p1.Element is MEPCurve)
            {
                startMEPCurve = _p1.Element as MEPCurve;
            }
            else
            {
                while (startMEPCurve is null)
                {
                    var elems = ConnectorUtils.GetConnectedElements(_p1.Element);
                    elems = elems.Where(obj => !deletedIds.Contains(obj.Id)).ToList();
                    var mcurves = elems.OfType<MEPCurve>().ToList();
                    if (mcurves is not null && mcurves.Any())
                    {
                        startMEPCurve = mcurves.First();
                    }
                }
            }

            StartMEPCurveModel = new MEPCurveModel(startMEPCurve, new SolidModel(ElementUtils.GetSolid(startMEPCurve)));
            return new LineModel(startMEPCurve);
        }

    }
}
