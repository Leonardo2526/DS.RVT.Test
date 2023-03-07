using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.MEP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.CableTests
{
    public class Tests
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly TransactionBuilder _trb;
        private readonly XYZ _startPoint = new XYZ();
        private readonly XYZ _endPoint = new XYZ(1,0,0);

        public Tests(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = _uiDoc.Document;
            _trb = new TransactionBuilder(_doc);
        }

        private ElementId MEPSystemTypeId
        {
            get
            {
                return new FilteredElementCollector(_doc).
                    OfClass(typeof(MEPSystemType)).Cast<MEPSystemType>().
                    FirstOrDefault().Id;
            }
        }

        private ElementId CableTypeId
        {
            get
            {
                return new FilteredElementCollector(_doc).
                    OfClass(typeof(CableTrayType)).Cast<CableTrayType>().
                    FirstOrDefault().Id;
            }
        }

        private ElementId MEPLevelId
        {
            get
            {
                return new FilteredElementCollector(_doc).
                  OfClass(typeof(Level)).Cast<Level>().
                  FirstOrDefault().Id;
            }
        }

        public void CreateCable()
        {
            _trb.Build(() =>
            {
                var cable = CableTray.Create(_doc, CableTypeId, _startPoint, _endPoint, MEPLevelId);               
            },"Create cable");
        }

    }
}
