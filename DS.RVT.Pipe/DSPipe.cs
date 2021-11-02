using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RVT.PipeTest
{
    class DSPipe
    {
        readonly UIDocument Uidoc;
        readonly Document Doc;
        readonly UIApplication Uiapp;
         
        public DSPipe(UIApplication uiapp, UIDocument uidoc, Document doc)
        {
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
        }


        MEPSystemType mepSystemType;
        PipeType pipeType;
        Level level;

        public void CreatePipeSystem()
        {
            GetSystems();
            CreateTransaction();
        }


        void GetSystems()
        {
            // Extract all pipe system types
            mepSystemType = new FilteredElementCollector(Doc)
        .OfClass(typeof(MEPSystemType)).Cast<MEPSystemType>()
        .FirstOrDefault(sysType => sysType.SystemClassification == MEPSystemClassification.DomesticColdWater);

            //Pipe Type (Standard, ChilledWater)
            pipeType = new FilteredElementCollector(Doc)
                .OfClass(typeof(PipeType))
                .Cast<PipeType>()
                .FirstOrDefault();


            //Level
            level = new FilteredElementCollector(Doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .FirstOrDefault();

        }

        void CreateTransaction()
        {
            using (Transaction transNew = new Transaction(Doc, "newTransaction"))
            {
                try
                {
                    transNew.Start();

                    Pipe pipe = null;
                    if (null != pipeType)
                    {
                        // create pipe between 2 points
                        XYZ p1 = new XYZ(0, 0, 0);
                        XYZ p2 = new XYZ(10, 0, 0);

                        pipe = Pipe.Create(Doc, mepSystemType.Id, pipeType.Id, level.Id, p1, p2);
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

    }
}
