using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RVT.ModelSpaceFragmentation.Lines;
using DS.RVT.ModelSpaceFragmentation.Path;
using FrancoGustavo;
using System;
using System.Collections.Generic;
using DS.RevitUtils; 

namespace DS.RVT.ModelSpaceFragmentation
{
    class Main  
    {
        readonly Application App;  
        readonly UIDocument Uidoc;
        public static Document Doc { get; set; }
        readonly UIApplication Uiapp; 

        public Main(Application app, UIApplication uiapp, UIDocument uidoc, Document doc)
        {
            App = app; 
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
        }

        public static Element CurrentElement { get; set; }

        public static int PointsStep { get; } = 50;

        public static double PointsStepF { get; set; }

          
        public void Implement() 
        {
            PointsStepF = UnitUtils.Convert(PointsStep,
                                DisplayUnitType.DUT_MILLIMETERS, 
                                DisplayUnitType.DUT_DECIMAL_FEET);

            ElementUtils elementUtils = new ElementUtils(); 
            CurrentElement = elementUtils.GetCurrent(new PickedElement(Uidoc, Doc));

            ElementSize elementSize = new ElementSize();  
            elementSize.GetElementSizes(CurrentElement as MEPCurve);

            SpaceFragmentator spaceFragmentator = new SpaceFragmentator(App, Uiapp, Uidoc, Doc);
            spaceFragmentator.FragmentSpace(CurrentElement);
             
            //Path finding initiation
            PathFinder pathFinder = new PathFinder();
            List<PathFinderNode> path = pathFinder.AStarPath(ElementInfo.StartElemPoint,
                ElementInfo.EndElemPoint, SpaceFragmentator.UnpassablePoints);

            if (path == null)
                TaskDialog.Show("Error", "No available path exist!"); 
            else
            {
                //Convert path to revit coordinates                
                List<XYZ> pathCoords = new List<XYZ>();
                pathCoords.Add(ElementInfo.StartElemPoint);

                foreach (PathFinderNode item in path) 
                {
                    XYZ point = new XYZ(item.ANX, item.ANY, item.ANZ); 
                    XYZ pathpoint = ConvertToModel(point);

                    double xx = Math.Abs(pathCoords[pathCoords.Count - 1].X - pathpoint.X);
                    double xy = Math.Abs(pathCoords[pathCoords.Count - 1].Y - pathpoint.Y);
                    double xz = Math.Abs(pathCoords[pathCoords.Count - 1].Z - pathpoint.Z);

                    if (xx > 0.01 || xy > 0.01 || xz > 0.01)
                        pathCoords.Add(pathpoint);  

                }
              
                pathCoords.Add(ElementInfo.EndElemPoint); 

                //Path visualization 
                LineCreator lineCreator = new LineCreator();
                lineCreator.CreateCurves(new CurvesByPointsCreator(pathCoords));
                
                //MEP system changing
                RevitUtils.MEP.PypeSystem pypeSystem = new RevitUtils.MEP.PypeSystem(Uiapp, Uidoc, Doc , CurrentElement);

                //check min distance
                double minDist = 1.5 * ElementSize.ElemDiameterF;
                if (Math.Abs(pathCoords[pathCoords.Count - 2].X - pathCoords[pathCoords.Count - 1].X) <= minDist ||
                    Math.Abs(pathCoords[pathCoords.Count - 2].Y - pathCoords[pathCoords.Count - 1].Y) <= minDist ||
                    Math.Abs(pathCoords[pathCoords.Count - 2].Z - pathCoords[pathCoords.Count - 1].Z) <= minDist)
                    pathCoords.RemoveAt(pathCoords.Count - 2);

                pypeSystem.CreatePipeSystem(pathCoords);

                RevitUtils.MEP.ElementEraser elementEraser = new RevitUtils.MEP.ElementEraser(Doc);
                elementEraser.DeleteElement(CurrentElement);

                //CLZVisualizator.ShowCLZOfPoint(PointsInfo.StartElemPoint); 
            }
        } 

        private XYZ ConvertToModel(XYZ point)
        {
            XYZ newpoint = point.Multiply(InputData.PointsStepF);
            newpoint += InputData.ZonePoint1;

            return newpoint;
        }

    }
}
