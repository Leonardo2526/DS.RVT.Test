using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation.Path
{
    class PointsCheker
    {
        readonly InputData data;
        public PointsCheker(InputData inputData)
        {
            data = inputData;
        }


        private bool IsEven(int a)
        {
            return (a % 2) == 0;
        }

        public bool IsCellEmpty(int ix, int iy, int iz)
        {
            if (data.UnpassablePoints.Count == 0)
                return true;

            for (int i = 0; i < InputData.UnpassLocX.Count; i++)
            {
                if (InputData.UnpassLocX[i] == ix &&
                    InputData.UnpassLocY[i] == iy &&
                    InputData.UnpassLocZ[i] == iz)
                    return false;
            }

            return true;
        }


    }
}
