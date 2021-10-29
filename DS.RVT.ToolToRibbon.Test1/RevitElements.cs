using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.ToolToRibbon.Test1
{
    class RevitElements
    {
        readonly UIDocument Uidoc;
        readonly Document Doc;

        public RevitElements(UIDocument uidoc, Document doc)
        {
            Uidoc = uidoc;
            Doc = doc;
        }

        public void MoveElement(Element elementB, XYZ centerPointElementA, double pipeSizeA)
        {
            XYZ centerPointElementB = GetCenterPoint(elementB);

            Pipe pipe = elementB as Pipe;
            double pipeSizeB = pipe.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsDouble();

            //revitElements.CreateModelLine(startPoint, endPoint);           
            double offsetX = 100;
            double offsetXF = UnitUtils.Convert(offsetX / 1000,
                                   DisplayUnitType.DUT_METERS,
                                   DisplayUnitType.DUT_DECIMAL_FEET);
            double fullOffset = (pipeSizeA + pipeSizeB) / 2 +
                (centerPointElementA.X - centerPointElementB.X) + offsetXF;

            //XYZ newPlace = new XYZ(centerPointElementB.X + fullOffset, centerPointElementB.Y, centerPointElementB.Z);
            //revitElements.CreateModelLine(centerPointElementB, newPlace);

            XYZ newVector = new XYZ(fullOffset, 0, 0);
            MoveElementTransaction(elementB, newVector);
        }

        public void MoveElementTransaction(Element ElementB, XYZ newVector)
        {
            using (Transaction transNew = new Transaction(Doc, "MoveElement"))
            {
                try
                {
                    transNew.Start();
                    ElementTransformUtils.MoveElement(Doc, ElementB.Id, newVector);
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }

                transNew.Commit();
            }
        }

        XYZ GetNewVecrot(XYZ startPoint, XYZ endPoint)
        {
            XYZ newVector = new XYZ();
            double Xa;
            double Ya;

            if (startPoint.X == endPoint.X)
            {
                Xa = startPoint.X;

                double A = (startPoint.Y - endPoint.Y) / (startPoint.X - endPoint.X);
                double b = endPoint.Y - (A * endPoint.X);
                Ya = A * Xa + b;
            }


            return newVector;
        }


        public XYZ GetCenterPoint(Element element)
        {
            //get the current location           
            LocationCurve lc = element.Location as LocationCurve;
            Curve c = lc.Curve;
            c.GetEndPoint(0);
            c.GetEndPoint(1);

            XYZ startPoint = c.GetEndPoint(0);
            XYZ endPoint = c.GetEndPoint(1);
            XYZ centerPoint = new XYZ((startPoint.X + endPoint.X) / 2,
                (startPoint.Y + endPoint.Y) / 2,
                (startPoint.Z + endPoint.Z) / 2);

            return centerPoint;
        }


        public void CreateModelLine(XYZ startPoint, XYZ endPoint)
        {            
            Line geomLine = Line.CreateBound(startPoint, endPoint);

            // Create a geometry plane in Revit application
            XYZ p1 = startPoint;
            XYZ p2 = endPoint;
            XYZ p3 = p2 + XYZ.BasisZ;
            Plane geomPlane = Plane.CreateByThreePoints(p1, p2, p3);

            using (Transaction transNew = new Transaction(Doc, "CreateModelLine"))
            {
                try
                {
                    transNew.Start();

                    // Create a sketch plane in current document
                    SketchPlane sketch = SketchPlane.Create(Doc, geomPlane);

                    // Create a ModelLine element using the created geometry line and sketch plane
                    ModelLine line = Doc.Create.NewModelCurve(geomLine, sketch) as ModelLine;
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }

                transNew.Commit();
            }
        }

        public XYZ GetPointInFeets(XYZ entryPoint)
        {
            double XF = UnitUtils.Convert(entryPoint.X / 1000,
                                           DisplayUnitType.DUT_METERS,
                                           DisplayUnitType.DUT_DECIMAL_FEET);
            double YF = UnitUtils.Convert(entryPoint.Y / 1000,
                                            DisplayUnitType.DUT_METERS,
                                            DisplayUnitType.DUT_DECIMAL_FEET);
            double ZF = UnitUtils.Convert(entryPoint.Z / 1000,
                                            DisplayUnitType.DUT_METERS,
                                            DisplayUnitType.DUT_DECIMAL_FEET);

            XYZ outPoint = new XYZ(XF, YF, ZF);

            return outPoint;
        }
        

    }
}
