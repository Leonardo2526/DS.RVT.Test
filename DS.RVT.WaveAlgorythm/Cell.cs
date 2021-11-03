using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace DS.RVT.WaveAlgorythm
{
    class Cell
    {
        readonly Application App;
        readonly UIApplication Uiapp;
        readonly Document Doc;
        readonly UIDocument Uidoc;

        public Cell(Application app, UIApplication uiapp, Document doc, UIDocument uidoc)
        {
            App = app;
            Uiapp = uiapp;
            Doc = doc;
            Uidoc = uidoc;
        }


        public void GetCells()
        {
            double areaSize = 1000;
            double areaSizeF = UnitUtils.Convert(areaSize / 1000,
                                  DisplayUnitType.DUT_METERS,
                                  DisplayUnitType.DUT_DECIMAL_FEET);

            XYZ corner1 = new XYZ(0, 0, 0);
            XYZ corner2 = new XYZ(areaSizeF, areaSizeF, 0);

            double cellSize = 100;
            double cellSizeF = UnitUtils.Convert(cellSize / 1000,
                                  DisplayUnitType.DUT_METERS,
                                  DisplayUnitType.DUT_DECIMAL_FEET);

            List<XYZ> cellsCoordinates = new List<XYZ>();

            for (double Y = 0; Y <= corner2.Y; Y += cellSizeF)
            {
                for (double X = 0; X <= corner2.X; X += cellSizeF)
                {
                    XYZ centralPoint = new XYZ(X, Y, 0);
                    Family family = new Family();
                    family.CreateCell(Doc, centralPoint);
                    //Cell cell = new Cell(App, Uiapp, Doc, Uidoc);
                    //cell.CreateCellBorders(centralPoint, cellSizeF);
                }

            }
        }


        void CreateCellBorders(XYZ centralPoint, double cellSizeF)
        {
            

            List<CellCorners> cellCorners = new List<CellCorners>();

            cellCorners = GetCellCorners(centralPoint, cellSizeF);

            foreach (CellCorners cC in cellCorners)
            {
                CreateCell(cC);
            }

        }

        void CreateCell(CellCorners cC)
        {
            CreateModelLine(cC.C1, cC.C2);
            CreateModelLine(cC.C2, cC.C3);
            CreateModelLine(cC.C3, cC.C4);
            CreateModelLine(cC.C4, cC.C1);
        }


        List<CellCorners> GetCellCorners(XYZ centerPoint, double cellSizeF)
        {
                List<CellCorners> cellCorners = new List<CellCorners>()
            {
                new CellCorners {},
            };

            cellCorners[0].C1 = new XYZ(centerPoint.X - cellSizeF / 2, centerPoint.Y - cellSizeF / 2, centerPoint.Z + cellSizeF / 2);
            cellCorners[0].C2 = new XYZ(centerPoint.X - cellSizeF / 2, centerPoint.Y + cellSizeF / 2, centerPoint.Z + cellSizeF / 2);
            cellCorners[0].C3 = new XYZ(centerPoint.X + cellSizeF / 2, centerPoint.Y + cellSizeF / 2, centerPoint.Z + cellSizeF / 2);
            cellCorners[0].C4 = new XYZ(centerPoint.X + cellSizeF / 2, centerPoint.Y - cellSizeF / 2, centerPoint.Z + cellSizeF / 2);

            cellCorners[0].C5 = new XYZ(centerPoint.X - cellSizeF / 2, centerPoint.Y - cellSizeF / 2, centerPoint.Z - cellSizeF / 2);
            cellCorners[0].C6 = new XYZ(centerPoint.X - cellSizeF / 2, centerPoint.Y + cellSizeF / 2, centerPoint.Z - cellSizeF / 2);
            cellCorners[0].C7 = new XYZ(centerPoint.X + cellSizeF / 2, centerPoint.Y + cellSizeF / 2, centerPoint.Z - cellSizeF / 2);
            cellCorners[0].C8 = new XYZ(centerPoint.X + cellSizeF / 2, centerPoint.Y - cellSizeF / 2, centerPoint.Z - cellSizeF / 2);

            BoudingBox boudingBox = new BoudingBox(App, Uiapp, Doc, Uidoc);
            boudingBox.FindCollision(cellCorners[0].C5, cellCorners[0].C3); 


            return cellCorners;
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

    }
}
