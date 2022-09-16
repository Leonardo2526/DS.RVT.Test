using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using System.Collections.Generic;

namespace DS.RevitApp.Test.PathFindTest.ConnectionPointService
{
    public class ConnectionElementSelector
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;

        public ConnectionElementSelector(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = _uidoc.Document;
        }

        public KeyValuePair<Element, XYZ> Select(string elementName)
        {
            ISelectionFilter selectionFilter = new ElementSelectionFilter();

            Reference reference = _uidoc.Selection.PickObject(ObjectType.Element, selectionFilter, "Select " + elementName);
            Element element = _uidoc.Document.GetElement(reference);

            XYZ point = null;
            if (element is FamilyInstance)
            {
                point = ElementUtils.GetLocationPoint(element);
            }
            else if (element is MEPCurve)
            {
                ISelectionFilter refFilter = new ReferenceSelectionFilter(reference);
                reference = _uidoc.Selection.PickObject(ObjectType.PointOnElement, refFilter, "Select point on " + elementName);

                Line line = MEPCurveUtils.GetLine(element as MEPCurve);
                point = line.Project(reference.GlobalPoint).XYZPoint;
            }

            return new KeyValuePair<Element, XYZ>(element, point);
        }

    }

    public class ElementSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            if (element.IsGeometryElement())
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
    }

    public class ReferenceSelectionFilter : ISelectionFilter
    {
        private Reference reference;

        public ReferenceSelectionFilter(Reference reference)
        {
            this.reference = reference;
        }

        public bool AllowElement(Element element)
        {
            return true;
        }

        public bool AllowReference(Reference refer, XYZ point)
        {
            if (reference.ElementId == refer.ElementId)
            {
                return true;
            }
            return false;
        }
    }
}
