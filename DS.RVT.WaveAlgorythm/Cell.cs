using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.WaveAlgorythm
{
    class Cell
    {
        readonly Application App;
        readonly UIApplication Uiapp;
        readonly Document Doc;
        readonly UIDocument Uidoc;
        readonly Data data;

        public Cell(Application app, UIApplication uiapp, Document doc, UIDocument uidoc, Data data)
        {
            App = app;
            Uiapp = uiapp;
            Doc = doc;
            Uidoc = uidoc;
            this.data = data;
        }

        readonly List<FamilyInstance> Cells = new List<FamilyInstance>();
        readonly ICollection<ElementId> CellsIds = new List<ElementId>();

        public int W { get; set; }
        public int H { get; set; }


        //public List<FamilyInstance> familyInstances = new List<FamilyInstance>();
        //public ICollection<ElementId> cellElementsIds = new List<ElementId>();

        public void GetCells()
        {
            //List for cells XYZ
            List<XYZ> cellsLocations = new List<XYZ>();

            W = 0;
            H = 0;


            //Open transaction cells creation
            using (Transaction transNew = new Transaction(Doc, "newTransaction"))
            {
                try
                {
                    transNew.Start();
                    for (double Z = data.ZonePoint1.Z; Z <= data.ZonePoint2.Z; Z += data.CellSizeF)
                    {
                        for (double Y = data.ZonePoint1.Y; Y <= data.ZonePoint2.Y; Y += data.CellSizeF)
                        {
                            if (Z == data.ZonePoint1.Z)
                                H++;
                            for (double X = data.ZonePoint1.X; X <= data.ZonePoint2.X; X += data.CellSizeF)
                            {
                                if (Y == data.ZonePoint1.Y)
                                    W++;
                                XYZ centralPoint = new XYZ(X, Y, Z);
                                cellsLocations.Add(centralPoint);
                                AddCell(centralPoint);
                            }

                        }
                    }

                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                transNew.Commit();
            }
        }

        public List<XYZ> FindCollisions()
        //Search for collisions between created cells and model elements
        {
            //List for cells XYZ which collide with other model elements
            List<XYZ> ICLocations = new List<XYZ>();

            ExclusionFilter exclusionFilter = new ExclusionFilter(CellsIds);

            Color color = new Color(255, 0, 0);

            //find collisions between each cell and other model elements by filters
            foreach (FamilyInstance familyInstance in Cells)
            {
                XYZ point = GetInstancePoint(familyInstance, exclusionFilter);
                if (point != null)
                {
                    OverwriteGraphic(familyInstance, color);
                    ICLocations.Add(point);
                }

            }

            return ICLocations;

        }

        public void GetCurves()
        {
            // Create a Outline, uses a minimum and maximum XYZ point to initialize the outline. 
            Outline myOutLn = new Outline(data.ZonePoint1, data.ZonePoint2);

            // Create a BoundingBoxIntersects filter with this Outline
            BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(myOutLn);

            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            collector.WherePasses(filter);
            collector.OfClass(typeof(Pipe));

            IList<Element> elements = collector.ToElements();

           

            foreach (Element element in elements)
            {
                Solid solid = GetSolid(element);
                if (solid == null)
                    continue;

                List<XYZ> startPoints = new List<XYZ>();
                List<XYZ> endPoints = new List<XYZ>();

                int i = 0;

                // Get the faces and edges from solid, and transform the formed points
                foreach (Face face in solid.Faces)
                {
                   
                    IList<CurveLoop> faceCurves = face.GetEdgesAsCurveLoops();
                    foreach (CurveLoop cl in faceCurves)
                    {
                        CurveLoopIterator curveLoopIterator = cl.GetCurveLoopIterator();
                        Curve cv = curveLoopIterator.Current;

                      
                        XYZ p1 = cv.GetEndPoint(0);
                        XYZ p2 = cv.GetEndPoint(1);
                      

                        if (i == 0)
                        {
                            startPoints.Add(p1);
                            startPoints.Add(p2);
                        }
                        else
                        {
                            endPoints.Add(p1);
                            endPoints.Add(p2);
                        }
                    }

                    i++;


                }

                tr(startPoints, endPoints);
            }
        }

        void tr(List<XYZ> startPoints, List<XYZ> endPoints)
        {
            XYZ p3 = startPoints[0] + XYZ.BasisZ;
            Line geomLine = Line.CreateBound(startPoints[0], endPoints[0]);

            using (Transaction transNew = new Transaction(Doc, "CreateModelLine"))
            {
                try
                {
                    transNew.Start();
                    //Doc.Create.NewModelCurve(cv, Uidoc.ActiveView.SketchPlane);
                    Plane geomPlane = Plane.CreateByThreePoints(startPoints[0], endPoints[0], p3);
                    SketchPlane sketch = SketchPlane.Create(Doc, geomPlane);
                    Doc.Create.NewModelCurve(geomLine, sketch);

                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }

                transNew.Commit();
            }
        }

        private Solid GetSolid(Element element)
        {
            GeometryElement geomElement = element.get_Geometry(new Options());

            Solid solid = null;
            foreach (GeometryObject geomObj in geomElement)
            {

                solid = geomObj as Solid;
                if (solid != null) break;

            }


            return solid;
        }

        void OverwriteGraphic(FamilyInstance instance, Color color)
        {
            OverrideGraphicSettings pGraphics = new OverrideGraphicSettings();
            pGraphics.SetProjectionLineColor(color);


            var patternCollector = new FilteredElementCollector(Doc);
            patternCollector.OfClass(typeof(FillPatternElement));
            FillPatternElement solidFillPattern = patternCollector.ToElements().Cast<FillPatternElement>().First(a => a.GetFillPattern().IsSolidFill);


            pGraphics.SetSurfaceForegroundPatternId(solidFillPattern.Id);
            pGraphics.SetSurfaceBackgroundPatternColor(color);

            using (Transaction transNew = new Transaction(Doc, "newTransaction"))
            {
                try
                {
                    transNew.Start();
                    Doc.ActiveView.SetElementOverrides(instance.Id, pGraphics);
                }

                catch (Exception e)
                {
                    transNew.RollBack();
                    TaskDialog.Show("Revit", e.ToString());
                }
                transNew.Commit();
            }

        }

        public void OverwriteCell(int x, int y, Color color, XYZ centerPoint = null)
        {
            if (centerPoint == null)
                centerPoint = new XYZ(data.ZonePoint1.X + x * data.CellSizeF, data.ZonePoint1.Y + y * data.CellSizeF, 0);

            BoundingBoxContainsPointFilter boundingBoxContainsPointFilter = new BoundingBoxContainsPointFilter(centerPoint);

            FilteredElementCollector cellsCollector = new FilteredElementCollector(Doc, CellsIds);
            IList<Element> cellsElements = cellsCollector.WherePasses(boundingBoxContainsPointFilter).ToElements();

            foreach (FamilyInstance familyInstance in cellsElements)
                OverwriteGraphic(familyInstance, color);

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


        public void AddCell(XYZ location)
        {

            // get the given view's level for beam creation
            Level level = new FilteredElementCollector(Doc)
                .OfClass(typeof(Level)).Cast<Level>().FirstOrDefault();

            // get a family symbol
            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            collector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_GenericModel);

            FamilySymbol gotSymbol = collector.FirstElement() as FamilySymbol;
            FamilyInstance instance = null;

            instance = Doc.Create.NewFamilyInstance(location, gotSymbol,
                level, StructuralType.NonStructural);
            Cells.Add(instance);
            CellsIds.Add(instance.Id);
        }

        public XYZ GetInstancePoint(FamilyInstance instance, ExclusionFilter exclusionFilter)
        {
            ElementIntersectsElementFilter elementIntersectsElementFilter =
                new ElementIntersectsElementFilter(instance);

            //Get collector with filtered elements
            FilteredElementCollector collector = new FilteredElementCollector(Doc);
            collector.WherePasses(exclusionFilter);
            collector.WherePasses(elementIntersectsElementFilter);


            XYZ point = null;
            if (collector.Count() > 0)
            {
                LocationPoint locationPopint = instance.Location as LocationPoint;
                point = new XYZ(locationPopint.Point.X, locationPopint.Point.Y, locationPopint.Point.Z);
            }

            return point;
        }
    }
}
