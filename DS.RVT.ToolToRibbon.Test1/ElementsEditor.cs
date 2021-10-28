using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.ToolToRibbon.Test1
{
    class ElementsEditor
    {
        readonly UIDocument Uidoc;
        readonly Document Doc;

        public ElementsEditor(UIDocument uidoc, Document doc)
        {
            Uidoc = uidoc;
            Doc = doc;
        }

        public void MoveElement(Element element)
        {            
            //get the current location           
            LocationCurve lc = element.Location as LocationCurve;
            Curve c = lc.Curve;
            c.GetEndPoint(0);
            c.GetEndPoint(1);

            XYZ startPoint = c.GetEndPoint(0);
            XYZ endPoint = c.GetEndPoint(1);
            XYZ centerPoint = new XYZ(Math.Abs(startPoint.X - endPoint.X) / 2,
                Math.Abs(startPoint.Y - endPoint.Y)/2, 
                Math.Abs(startPoint.Z - endPoint.Z)/2);

            RevitElements revitElements = new RevitElements(Uidoc, Doc);
            revitElements.CreateModelLine(startPoint, endPoint);


            using (Transaction transNew = new Transaction(Doc, "MoveElement"))
            {
                try
                {
                    transNew.Start();

                    // Move the column to new location.
                    double offsetX = 1000;
                    double offsetXF = UnitUtils.Convert(offsetX / 1000,
                                           DisplayUnitType.DUT_METERS,
                                           DisplayUnitType.DUT_DECIMAL_FEET);

                    XYZ newPlace = new XYZ(centerPoint.X + offsetXF, centerPoint.Y, centerPoint.Z);
                    ElementTransformUtils.MoveElement(Doc, element.Id, newPlace);
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }

                transNew.Commit();
            }


        }

    }
}
