using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RVT.ModelSpaceFragmentation.Path;
using System.Collections.Generic;


namespace DS.RVT.ModelSpaceFragmentation
{
    class PathFinder
    {
        public List<XYZ> PathCoords { get; set; }

        public static ISpacePointsIterator spacePointsIterator { get; set; }

        public List<XYZ> GetPath(XYZ startPoint, XYZ endPoint, List<XYZ> unpassablePoints)
        {
            InputData data = new InputData(startPoint, endPoint, unpassablePoints);
            data.ConvertToPlane();

            List<XYZ> pathCoords1 = new List<XYZ>();
            List<XYZ> pathCoords2 = new List<XYZ>();
            int len1 = 1000, len2 = 1000;

            WaveAlgorythm waveAlgorythm;
            if (InputData.Ax == InputData.Bx)
            {
                spacePointsIterator = new IteratorByYZPlane();
                waveAlgorythm = new WaveAlgorythm(spacePointsIterator);
                pathCoords1 = waveAlgorythm.Implement();

                if (pathCoords1.Count > 0)
                    len1 = waveAlgorythm.Len;
            }
            else if (InputData.Ay == InputData.By)
            {
                spacePointsIterator = new IteratorByXZPlane();
                waveAlgorythm = new WaveAlgorythm(spacePointsIterator);
                pathCoords1 = waveAlgorythm.Implement();

                if (pathCoords1.Count > 0)
                    len1 = waveAlgorythm.Len;
            }

            if (InputData.Az == InputData.Bz)
            {
                spacePointsIterator = new IteratorByXYPlane();
                waveAlgorythm = new WaveAlgorythm(spacePointsIterator);
                pathCoords2 = waveAlgorythm.Implement();

                if (pathCoords2.Count > 0)
                    len2 = waveAlgorythm.Len;
            }

            if (len1 == 1000 && len2 == 1000)
            {
                spacePointsIterator = new IteratorBy3D();
                waveAlgorythm = new WaveAlgorythm(spacePointsIterator);
                PathCoords = waveAlgorythm.Implement();

                if (PathCoords.Count == 0)
                    TaskDialog.Show("Revit", "Путь не найден!");

                return PathCoords;
            }



            if (len1 > len2)
                PathCoords = pathCoords2;
            else
                PathCoords = pathCoords1;


            return PathCoords;
        }

    }
}
