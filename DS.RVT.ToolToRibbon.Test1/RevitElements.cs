using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

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
