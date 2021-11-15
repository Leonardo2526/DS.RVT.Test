using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.PipesCollisionsElliminator
{
    class Path
    {
        readonly Application App;
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;
        readonly ElementUtils ElemUtils;

        public Path(Application app, UIApplication uiapp, UIDocument uidoc, Document doc, ElementUtils elemUtils)
        {
            App = app;
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
            ElemUtils = elemUtils;
        }

        public void FindAvailable(Element SelectedElement, IList<Element> collisionElements)
        {
            foreach (Element colElem in collisionElements)
            {
                if (CheckElementForMove(SelectedElement, colElem) == true)
                {
                    ElemUtils.GetPoints(SelectedElement, out XYZ startPoint, out XYZ endPoint, out XYZ centerPointElement);

                    RevitElements revitElements = new RevitElements(App, Uiapp, Uidoc, Doc);
                    revitElements.CreateModelLine(startPoint, endPoint);

                    Line geomLine = Line.CreateBound(startPoint, endPoint);
                }
            }
        }

        bool CheckElementForMove(Element SelectedElement, Element colElem)
        {
            bool elementForMove = false;

            ElemUtils.GetPoints(SelectedElement, out XYZ startPointA, out XYZ endPointA, out XYZ centerPointA);
            ElemUtils.GetPoints(colElem, out XYZ startPointB, out XYZ endPointB, out XYZ centerPointB);

            double tgA = (endPointA.Y - startPointA.Y) / (endPointA.X - startPointA.X);
            double tgB = (endPointB.Y - startPointB.Y) / (endPointB.X - startPointB.X);

            double radA = Math.Atan(tgA);
            double angleA = radA * (180 / Math.PI);

            double radB = Math.Atan(tgB);
            double angleB = radB * (180 / Math.PI);

            double deltaAndle = Math.Abs(angleA - angleB);

            if (deltaAndle < 15 | (180 - deltaAndle) < 15)
                elementForMove = true;

            return elementForMove;
        }


        public XYZ GetOffset(Element SelectedElement, Element colElem, double offset, bool changeDirection)
        {
            ElemUtils.GetPoints(SelectedElement, out XYZ startPointA, out XYZ endPointA, out XYZ centerPointElementA);
            ElemUtils.GetPoints(colElem, out XYZ startPointB, out XYZ endPointB, out XYZ centerPointElementB);

            double alfa;
            double beta;
            double offsetF;

            double fullOffsetX = 0;
            double fullOffsetY = 0;
            double fullOffsetZ = 0;

            //Get pipes sizes
            Pipe pipeA = SelectedElement as Pipe;
            double pipeSizeA = pipeA.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsDouble();
            Pipe pipeB = colElem as Pipe;
            double pipeSizeB = pipeB.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsDouble();

            offsetF = UnitUtils.Convert(offset / 1000,
                                   DisplayUnitType.DUT_METERS,
                                   DisplayUnitType.DUT_DECIMAL_FEET);
            //check correct direction
            int K = 1;
            if (changeDirection == true)
                K = -1;

            if (Math.Round(startPointB.X, 3) == Math.Round(endPointB.X, 3))
            {

                fullOffsetX = (pipeSizeA + pipeSizeB) / 2 +
             K * (centerPointElementA.X - centerPointElementB.X) + offsetF;
            }
            else if (Math.Round(startPointB.Y, 3) == Math.Round(endPointB.Y, 3))
            {
                fullOffsetY = (pipeSizeA + pipeSizeB) / 2 +
             K * (centerPointElementA.Y - centerPointElementB.Y) + offsetF;
            }
            else
            {
                double A = (endPointB.Y - startPointB.Y) / (endPointB.X - startPointB.X);

                alfa = Math.Atan(A);
                double angle = alfa * (180 / Math.PI);
                beta = 90 * (Math.PI / 180) - alfa;
                angle = beta * (180 / Math.PI);

                double AX = Math.Cos(beta);
                double AY = Math.Sin(beta);

                double H = centerPointElementB.Y + A * (centerPointElementA.X - centerPointElementB.X);

                double deltaCenter = (centerPointElementA.Y - H) * Math.Cos(alfa);

                double fullOffset = ((pipeSizeA + pipeSizeB) / 2 - K * deltaCenter + offsetF);

                //Get full offset of element B from element A              
                fullOffsetX = fullOffset * AX;
                fullOffsetY = -fullOffset * AY;
                fullOffsetZ = 0;

            }


            XYZ XYZoffset = new XYZ(K * fullOffsetX, K * fullOffsetY, K * fullOffsetZ);

            return XYZoffset;
        }
    }
}
