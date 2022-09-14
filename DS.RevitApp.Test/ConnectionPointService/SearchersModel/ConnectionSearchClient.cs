using Autodesk.Revit.DB;
using DS.RevitApp.Test.ConnectionPointService.PointModel;
using DS.RevitApp.Test.PathFinders;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.SystemTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.ConnectionPointService.SearchersModel
{
    internal class ConnectionSearchClient : AbstractConnectionSearcher
    {
        private readonly Element _baseElement;

        private List<IConnectionPoint> _points1;
        private List<IConnectionPoint> _points2;
        private readonly int _baseIndex;

        public ConnectionSearchClient(IPathFinder pathFinder, ITransfromBuilder transfromBuilder, MEPSystemModel mEPSystemModel) :
            base(pathFinder, transfromBuilder, mEPSystemModel)
        {
            _baseElement = _mEPSystemModel.Root.BaseElement;
            _baseIndex = _mEPSystemModel.Root.Elements.FindIndex(obj => obj.Id == _baseElement.Id);
        }

        public override (IConnectionPoint Point1, IConnectionPoint Point2) GetConnectionPoints()
        {
            var (con1, con2) = ConnectorUtils.GetMainConnectors(_baseElement);
            var builer = new CheckPointsBuilder(_mEPSystemModel, _baseElement);
            _points1 = builer.Build(con1);
            _points2 = builer.Build(con2);


            var searcher1 = new SearcherByNodes(_pathFinder, _transfromBuilder, _mEPSystemModel, _points1, _points2);
            //var searcher2 = new SeracherByMEPCurves(_pathFinder, _transfromBuilder, _mEPSystemModel);
            //searcher1.SetSuccessor(searcher2);

            var result = searcher1.GetConnectionPoints();
            Path = searcher1.Path;
            FamInstTransforms = searcher1.FamInstTransforms;

            return result;
        }

        //private List<IConnectionPoint> GetCheckedPoints(Element baseElement, Connector connector)
        //{
        //    var points = new List<IConnectionPoint>();

        //    var connectedElem = ConnectorUtils.GetConnectedByConnector(connector, baseElement);
        //    if (connectedElem is null)
        //        return points;

        //    int connectedElemInd = _mEPSystemModel.Root.Elements.FindIndex(obj => obj.Id == connectedElem.Id);
        //    int dInd = _baseIndex - connectedElemInd;
        //    List<FamilyInstance> fittings = null;
        //    if (dInd > 0)
        //    {
        //        var elems = _mEPSystemModel.Root.GetElements(_mEPSystemModel.Root.Elements.First(), connectedElem);
        //        var fIds = elems.Select(obj => obj.Id).Intersect(_mEPSystemModel.Root.Fittings.Select(obj => obj.Id))?.ToList();
        //        fittings = _mEPSystemModel.Root.Fittings.Where(obj => fIds.Contains(obj.Id))?.Reverse().ToList();
        //    }
        //    else
        //    {
        //        var elems = _mEPSystemModel.Root.GetElements(connectedElem, _mEPSystemModel.Root.Elements.Last());
        //        var fIds = elems.Select(obj => obj.Id).Intersect(_mEPSystemModel.Root.Fittings.Select(obj => obj.Id))?.ToList();
        //        fittings = _mEPSystemModel.Root.Fittings.Where(obj => fIds.Contains(obj.Id))?.ToList();
        //    }


        //    if (fittings is not null && fittings.Any())
        //    {
        //        foreach (var fam in fittings)
        //        {
        //            XYZ lp = fam.GetLocationPoint();
        //            var point = new ConnectionPoint(lp, fam);
        //            points.Add(point);
        //        }
        //    }


        //    return points;
        //}



        //private List<Element> GetElementsSpanWithoutChilds(List<Element> elements)
        //{
        //    var childIds = _mEPSystemModel.Root.ChildrenNodes?.Select(obj => obj.Element.Id).ToList();
        //    if (childIds is null | !childIds.Any())
        //    {
        //        return elements;
        //    }

        //    var span = new List<Element>();
        //    span.AddRange(elements);
        //    span.Reverse();
        //    foreach (var elem in span)
        //    {
        //        if (childIds.Contains(elem.Id))
        //        {
        //            var firstChild = _mEPSystemModel.Root.ChildrenNodes.Where(obj => obj.Element.Id == elem.Id).FirstOrDefault();
        //            return _mEPSystemModel.Root.GetElements(elements.First(), firstChild.Element);
        //        }
        //    }

        //    return null;
        //}

    }
}
