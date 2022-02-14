using Autodesk.Revit.DB;
using System.Collections.Generic;
using DS.RevitUtils.MEP;
using System.Linq;

namespace DS.RVT.ModelSpaceFragmentation
{


    class ModelSolid
    {
        readonly Document Doc;

        public ModelSolid(Document doc)
        {
            Doc = doc;
        }

        ElementUtils ElemUtils = new ElementUtils();

        public static Dictionary<Element, List<Solid>> SolidsInModel { get; set; }

        public Dictionary<Element, List<Solid>> GetSolids()
        {
            SolidsInModel = new Dictionary<Element, List<Solid>>();
            FilteredElementCollector collector = new FilteredElementCollector(Doc);

            ICollection<BuiltInCategory> elementCategoryFilters = new List<BuiltInCategory>
                {
                    BuiltInCategory.OST_DuctCurves,
                    BuiltInCategory.OST_DuctFitting,
                    BuiltInCategory.OST_PipeCurves,
                    BuiltInCategory.OST_PipeFitting,
                    BuiltInCategory.OST_MechanicalEquipment,
                    BuiltInCategory.OST_Walls
                };

            ElementMulticategoryFilter elementMulticategoryFilter = new ElementMulticategoryFilter(elementCategoryFilters);

            Outline myOutLn = new Outline(ElementInfo.MinBoundPoint, ElementInfo.MaxBoundPoint);
            BoundingBoxIntersectsFilter boundingBoxFilter = new BoundingBoxIntersectsFilter(myOutLn);

            List<Element> connectedElements = new List<Element>()
            {
                Main.CurrentElement
            };
            ConnectedElement connectedElement = new ConnectedElement();
            connectedElements.AddRange(connectedElement.GetAllConnected(Main.CurrentElement, Doc));

            ICollection<ElementId> elementIds = connectedElements.Select(el => el.Id).ToList();
           
            ExclusionFilter exclusionFilter = new ExclusionFilter(elementIds);

            collector.WhereElementIsNotElementType();
            collector.WherePasses(boundingBoxFilter);
            collector.WherePasses(exclusionFilter);
            IList<Element> intersectedElementsBox = collector.WherePasses(elementMulticategoryFilter).ToElements();

            Dictionary<Element, List<Solid>> solidsDictionary = new Dictionary<Element, List<Solid>>();

            List<Solid> solids = new List<Solid>();
            foreach (Element elem in intersectedElementsBox)
            {
                solids = ElemUtils.GetSolids(elem);
                if (solids.Count !=0)
                {
                    SolidsInModel.Add(elem, solids);
                    solidsDictionary.Add(elem, solids);
                }             
            }

            return solidsDictionary;
        }


    }
}
