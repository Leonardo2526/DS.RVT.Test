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

        public void MoveElement(Element elementA, Element elementB)
        { 
            double offset = 100;

            XYZ newVector = GetOffset(elementA, elementB, offset);

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

        XYZ GetOffset(Element ElementA, Element ElementB, double offset)
        {
            GetPoints(ElementA, out XYZ startPointA, out XYZ endPointA, out XYZ centerPointElementA);
            GetPoints(ElementB, out XYZ startPointB, out XYZ endPointB, out XYZ centerPointElementB);

            double alfa;
            double beta;

            double radians;
            double result;

            double offsetF;

            double offsetX = 0;
            double offsetY = 0;
            double offsetZ =0 ;

            
            double fullOffsetX = 0;
            double fullOffsetY =0 ;
            double fullOffsetZ =0 ;

            //Get pipes sizes
            Pipe pipeA = ElementA as Pipe;
            double pipeSizeA = pipeA.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsDouble();
            Pipe pipeB = ElementB as Pipe;
            double pipeSizeB = pipeB.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsDouble();

            offsetF = UnitUtils.Convert(offset / 1000,
                                   DisplayUnitType.DUT_METERS,
                                   DisplayUnitType.DUT_DECIMAL_FEET);

           

            if (Math.Round(startPointB.X, 3) == Math.Round(endPointB.X, 3))
            {
               
                fullOffsetX = (pipeSizeA + pipeSizeB) / 2 +
             (centerPointElementA.X - centerPointElementB.X) + offsetF;
            }
            else if (Math.Round(startPointB.Y, 3) == Math.Round(endPointB.Y, 3))
            {
                fullOffsetY = (pipeSizeA + pipeSizeB) / 2 +
             (centerPointElementA.Y - centerPointElementB.Y) + offsetF;
            }
            else
            {
                double A = (endPointB.Y - startPointB.Y) / (endPointB.X - startPointB.X);

                alfa = Math.Atan(A); 
                double angle = alfa * (180 / Math.PI);
                beta = 90 * (Math.PI / 180) - alfa;
                angle = beta * (180 / Math.PI);

                offsetZ = 0;
                double AX = Math.Cos(beta);
                double AY = Math.Sin(beta);

                double H = centerPointElementB.Y + A * (centerPointElementA.X - centerPointElementB.X);

                double deltaCenter = (centerPointElementA.Y - H) * Math.Cos(alfa);

                double fullOffset = ((pipeSizeA + pipeSizeB) / 2 - deltaCenter + offsetF);
                //(centerPointElementA.X - centerPointElementB.X) / AX
                //Get full offset of element B from element A
                fullOffsetX = fullOffset * AX;
                fullOffsetY = - fullOffset * AY;
                fullOffsetZ = 0;
            }

               

            XYZ XYZoffset = new XYZ(fullOffsetX, fullOffsetY, fullOffsetZ);

            return XYZoffset;
        }


        public void GetPoints(Element element, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint)
        {
            //get the current location           
            LocationCurve lc = element.Location as LocationCurve;
            Curve c = lc.Curve;
            c.GetEndPoint(0);
            c.GetEndPoint(1);

            startPoint = c.GetEndPoint(0);
            endPoint = c.GetEndPoint(1);
            centerPoint = new XYZ((startPoint.X + endPoint.X) / 2,
                (startPoint.Y + endPoint.Y) / 2,
                (startPoint.Z + endPoint.Z) / 2);

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
