using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.AutoPipesCoordinarion
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

        //List for cells XYZ which collide with other model elements
        readonly List<XYZ> ICLocations = new List<XYZ>();
        readonly List<XYZ> elementZonePoints = new List<XYZ>();

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

        public void GetElementZonePoints()
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

                GetElementZoneOddPoints(element);
            }

            /*
            int j;
            for (j = 0; j < elementZonePoints.Count; j++)
            {
                XYZ p = new XYZ(elementZonePoints[j].X + 0.05, elementZonePoints[j].Y, elementZonePoints[j].Z);
                CreateModelLine(elementZonePoints[j], p);
            }
            Uidoc.RefreshActiveView();
            */
            AddElementZonePointsToIC();
        }

        void GetElementZoneOddPoints(Element element)
        {
            List<XYZ> pointsList = new List<XYZ>();



            //Get pipes sizes
            Pipe pipe = element as Pipe;
            GetElementPoints(element, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint);

            XYZ vector = endPoint - startPoint;


            int xs;
            int ys;
            int zs;

            if (Math.Abs(vector.X) < 0.01)
                xs = 0;
            else
                xs = (int)(vector.X / Math.Abs(vector.X));

            if (Math.Abs(vector.Y) < 0.01)
                ys = 0;
            else
                ys = (int)(vector.Y / Math.Abs(vector.Y));

            if (Math.Abs(vector.Z) < 0.01)
                zs = 0;
            else
                zs = (int)(vector.Z / Math.Abs(vector.Z));





            Parameter parameter = pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);

            double distance = startPoint.DistanceTo(endPoint);
            int cellsCount = (int)(distance / data.CellSizeF);

            int i;
            XYZ pR = new XYZ(startPoint.X - ys * (parameter.AsDouble() / 2 + data.ElementOffsetF) - xs * data.CellSizeF, 
                startPoint.Y - xs * (parameter.AsDouble() / 2 + data.ElementOffsetF) - ys * data.CellSizeF, startPoint.Z);

            XYZ pL = new XYZ(startPoint.X + ys * (parameter.AsDouble() / 2 + data.ElementOffsetF) - xs * data.CellSizeF, 
                startPoint.Y + xs * (parameter.AsDouble() / 2 + data.ElementOffsetF) - ys * data.CellSizeF, startPoint.Z);

            pointsList.Add(pR);
            pointsList.Add(pL);

            for (i = 0; i <= cellsCount + 1; i++)
            {
                pR = new XYZ(pR.X + xs * data.CellSizeF, pR.Y + ys * data.CellSizeF, pR.Z);
                pL = new XYZ(pL.X + xs * data.CellSizeF, pL.Y + ys * data.CellSizeF, pL.Z);
                pointsList.Add(pR);
                pointsList.Add(pL);
            }

            elementZonePoints.AddRange(pointsList);
        }

        private List<Solid> GetSolid(Element element)
        {
            List<Solid> solids = new List<Solid>();

            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            GeometryElement geomElem = element.get_Geometry(options);

            if (geomElem == null)
                return null;

            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid)
                {
                    Solid solid = (Solid)geomObj;
                    if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                    {
                        solids.Add(solid);
                    }
                    // Single-level recursive check of instances. If viable solids are more than
                    // one level deep, this example ignores them.
                }
                else if (geomObj is GeometryInstance)
                {
                    GeometryInstance geomInst = (GeometryInstance)geomObj;
                    GeometryElement instGeomElem = geomInst.GetInstanceGeometry();
                    foreach (GeometryObject instGeomObj in instGeomElem)
                    {
                        if (instGeomObj is Solid)
                        {
                            Solid solid = (Solid)instGeomObj;
                            if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                            {
                                solids.Add(solid);
                            }
                        }
                    }
                }
            }


            return solids;
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

        public void OverwriteCell(Color color, int x = 0, int y = 0,  XYZ centerPoint = null)
        {
            if (centerPoint == null)
                centerPoint = new XYZ(data.ZonePoint1.X + x * data.CellSizeF, data.ZonePoint1.Y + y * data.CellSizeF, 0);

            BoundingBoxContainsPointFilter boundingBoxContainsPointFilter = new BoundingBoxContainsPointFilter(centerPoint);

            FilteredElementCollector cellsCollector = new FilteredElementCollector(Doc, CellsIds);
            IList<Element> cellsElements = cellsCollector.WherePasses(boundingBoxContainsPointFilter).ToElements();

            foreach (FamilyInstance familyInstance in cellsElements)
                OverwriteGraphic(familyInstance, color);

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


        void AddElementZonePointsToIC()
        {
            foreach (XYZ point in elementZonePoints)
            {
                Color color = new Color(100, 100, 100);
                OverwriteCell(color, 0, 0, point);
                ICLocations.Add(point);
            }
        }


        void GetElementPoints(Element element, out XYZ startPoint, out XYZ endPoint, out XYZ centerPoint)
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

    }
}
